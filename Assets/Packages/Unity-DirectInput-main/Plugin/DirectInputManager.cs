﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
#if UNITY_STANDALONE_WIN
  using UnityEngine;
#endif

namespace DirectInputManager {
  class Native {
#if UNITY_STANDALONE_WIN
    const string DLLFile = @"DirectInputForceFeedback.dll";
#else
    const string DLLFile = @"..\..\..\..\..\Plugin\DLL\DirectInputForceFeedback.dll";
#endif

    [DllImport(DLLFile)] public static extern int StartDirectInput();
    [DllImport(DLLFile)] public static extern int StopDirectInput();
    [DllImport(DLLFile)] public static extern IntPtr EnumerateDevices(out int deviceCount);
    [DllImport(DLLFile)] public static extern int CreateDevice(string guidInstance);
    [DllImport(DLLFile)] public static extern int DestroyDevice(string guidInstance);
    [DllImport(DLLFile)] public static extern int GetDeviceState(string guidInstance, out FlatJoyState2 DeviceState);
    [DllImport(DLLFile)] public static extern int GetDeviceStateRaw(string guidInstance, out DIJOYSTATE2 DeviceState);
    [DllImport(DLLFile)] public static extern int GetDeviceCapabilities(string guidInstance, out DIDEVCAPS DeviceCapabilitiesOut);
    [DllImport(DLLFile)] public static extern int GetActiveDevices([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] ActiveGUIDs);
    [DllImport(DLLFile)] public static extern int EnumerateFFBEffects(string guidInstance, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] SupportedFFBEffects);
    [DllImport(DLLFile)] public static extern int EnumerateFFBAxis(string guidInstance, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] SupportedFFBAxis);
    [DllImport(DLLFile)] public static extern int CreateFFBEffect(string guidInstance, FFBEffects effectType);
    [DllImport(DLLFile)] public static extern int DestroyFFBEffect(string guidInstance, FFBEffects effectType);
    [DllImport(DLLFile)] public static extern int UpdateFFBEffect(string guidInstance, FFBEffects effectType, DICondition[] conditions);
    [DllImport(DLLFile)] public static extern int StopAllFFBEffects(string guidInstance);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void DeviceChangeCallback(DBTEvents DBTEvent);
    [DllImport(DLLFile)] public static extern void SetDeviceChangeCallback([MarshalAs(UnmanagedType.FunctionPtr)] DeviceChangeCallback onDeviceChange);

    [DllImport(DLLFile)] public static extern int DEBUG1([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] DEBUGDATA);
  }
  public class DIManager {
    //////////////////////////////////////////////////////////////
    // Cross Platform "Macros" - Allows lib to work in Visual Studio & Unity
    //////////////////////////////////////////////////////////////

#if UNITY_STANDALONE_WIN
    const string DLLFile = @"DirectInputForceFeedback.dll";
    private static uint ClampAgnostic(uint value, uint min, uint max) => (uint)Mathf.Clamp(value, min, max);
    private static int ClampAgnostic(int value, int min, int max) => Mathf.Clamp(value, min, max);
    private static void DebugLog(string message) => Debug.Log(message);
#else
    const string DLLFile = @"..\..\..\..\..\Plugin\DLL\DirectInputForceFeedback.dll";
    private static uint ClampAgnostic(uint value, uint min, uint max) => Math.Clamp(value, min, max);
    private static int ClampAgnostic(int value, int min, int max) => Math.Clamp(value, min, max);
    private static void DebugLog(string message) => System.Diagnostics.Debug.WriteLine(message);
#endif

    //////////////////////////////////////////////////////////////
    // Private Variables - For Internal use
    //////////////////////////////////////////////////////////////

    private static bool _isInitialized = false;               // is DIManager ready
    private static DeviceInfo[] _devices = new DeviceInfo[0]; // Hold data for devices plugged in
    private static Dictionary<string, ActiveDeviceInfo> _activeDevices = new Dictionary<string, ActiveDeviceInfo>(); // Hold data for devices actively attached

    //////////////////////////////////////////////////////////////
    // Public Variables
    //////////////////////////////////////////////////////////////
    public static bool isInitialized { get => _isInitialized; }
    public static DeviceInfo[] devices { get => _devices; }
    public static Dictionary<string, ActiveDeviceInfo> activeDevices { get => _activeDevices; }

    public static string getDLL(){ return DLLFile; }

    //////////////////////////////////////////////////////////////
    // Methods
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Initializes DirectInput<br/><br/>
    /// </summary>
    /// <returns>
    /// True if sucessful or DI already initialized<br/>
    /// False if failed
    /// </returns>
    public static bool Initialize() {
      if (_isInitialized) { return _isInitialized; }
      if (Native.StartDirectInput() != 0) { return _isInitialized = false; }
      Native.SetDeviceChangeCallback(OnDeviceChange);
      return _isInitialized = true;
    }

    /// <summary>
    /// Fetch currently available devices and populate DIManager.devices<br/>
    /// </summary>
    public static void EnumerateDevices() {
      int deviceCount = 0;
      IntPtr ptrDevices = Native.EnumerateDevices(out deviceCount); // Returns pointer to list of devices and how many are available

      if (deviceCount > 0) {
        _devices = new DeviceInfo[deviceCount];

        int deviceSize = Marshal.SizeOf(typeof(DeviceInfo)); // Size of each Device entry
        for (int i = 0; i < deviceCount; i++) {
          IntPtr pCurrent = ptrDevices + i * deviceSize; // Ptr to the current device
          _devices[i] = Marshal.PtrToStructure<DeviceInfo>(pCurrent); // Transform the Ptr into a C# instance of DeviceInfo
        }
      }
      return;
    }

    public static async Task EnumerateDevicesAsync(){
      await Task.Run(EnumerateDevices);
    }

    /// <summary>
    /// Attach to Device, ready to get state/ForceFeedback<br/><br/>
    /// </summary>
    /// <returns>
    /// A boolean representing the if the Device was attached
    /// </returns>
    public static bool Attach(string guidInstance) {
      if (_activeDevices.ContainsKey(guidInstance)) { return true; } // We're already attached to that device
      int hresult = Native.CreateDevice(guidInstance);
      if (hresult != 0) { DebugLog($"[DirectInputManager] CreateDevice Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)} {guidInstance}"); return false; }
      DeviceInfo device = _devices.Where(device => device.guidInstance == guidInstance).First();
      _activeDevices.Add(guidInstance, new ActiveDeviceInfo(){ deviceInfo = device }); // Add device to our C# active device tracker (Dictionary allows us to easily check if GUID already exists)
      return true;
    }


    /// <summary>
    /// Remove a specified Device
    /// </summary>
    /// <returns>
    /// True upon sucessful destruction or device already didn't exist
    /// </returns>
    public static bool Destroy(string guidInstance) {
      if (!_activeDevices.ContainsKey(guidInstance)) { return true; } // We don't think we're attached to that device, consider it removed
      int hresult = Native.DestroyDevice(guidInstance);
      _activeDevices.Remove(guidInstance); // remove from our C# active device tracker
      if (hresult != 0) { DebugLog($"[DirectInputManager] DestroyDevice Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// Retrieve state of the Device, Flattened for easier comparison.<br/>
    /// </summary>
    /// <returns>
    /// FlatJoyState2
    /// </returns>
    public static FlatJoyState2 GetDeviceState(string guidInstance) {
      FlatJoyState2 DeviceState = new FlatJoyState2();
      int hresult = Native.GetDeviceState(guidInstance, out DeviceState);
      if (hresult != 0) { DebugLog($"[DirectInputManager] GetDeviceState Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); /*return false;*/ }
      return DeviceState;
    }

    // public static async Task<FlatJoyState2> GetDeviceStateAsync(string guidInstance){
    //   return await Task.Run(()=>{return GetDeviceState(guidInstance);});
    // }
    
    /// <summary>
    /// Retrieve state of the Device<br/>
    /// *Warning* DIJOYSTATE2 contains arrays making it difficult to compare, concider using GetDeviceState
    /// </summary>
    /// <returns>
    /// DIJOYSTATE2
    /// </returns>
    public static DIJOYSTATE2 GetDeviceStateRaw(string guidInstance) {
      DIJOYSTATE2 DeviceState = new DIJOYSTATE2();
      int hresult = Native.GetDeviceStateRaw(guidInstance, out DeviceState);
      if (hresult != 0) { DebugLog($"[DirectInputManager] GetDeviceStateRaw Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); /*return false;*/ }
      return DeviceState;
    }

    /// <summary>
    /// Lists all attached device GUIDs
    /// </summary>
    /// <returns>
    /// string[] of attached GUIDs
    /// </returns>
    public static string[] GetActiveDevices() {
      string[] ActiveGUIDs = null;
      int hresult = Native.GetActiveDevices(out ActiveGUIDs);
      //if (hresult != 0) { Debug.LogError($"[DirectInputManager] GetActiveDevices Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); /*return false;*/ }
      if (hresult != 0) { DebugLog($"[DirectInputManager] GetActiveDevices Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); /*return false;*/ }

      if (ActiveGUIDs.Length != _activeDevices.Count) { DebugLog($"[DirectInputManager] Active Device mismatch! DLL:{ActiveGUIDs.Length}, DIManager:{_activeDevices.Count}"); }

      return ActiveGUIDs;
    }

    /// <summary>
    /// Retrieve the capabilities of the device. E.g. # Buttons, # Axes, Driver Version<br/>
    /// Device must be attached first<br/>
    /// </summary>
    /// <returns>
    /// DIDEVCAPS https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee416607(v=vs.85)
    /// </returns>
    public static DIDEVCAPS GetDeviceCapabilities(string guidInstance) {
      DIDEVCAPS DeviceCapabilities = new DIDEVCAPS();
      int hresult = Native.GetDeviceCapabilities(guidInstance, out DeviceCapabilities);
      if (hresult != 0) { DebugLog($"[DirectInputManager] GetDeviceCapabilities Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); /*return false;*/ }
      return DeviceCapabilities;
    }

    /// <summary>
    /// Returns if the device has the ForceFeedback Flag<br/>
    /// </summary>
    /// <returns>
    /// True if device can provide ForceFeedback<br/>
    /// </returns>
    public static bool FFBCapable(string guidInstance) {
      return GetDeviceCapabilities(guidInstance).dwFlags.HasFlag(dwFlags.DIDC_FORCEFEEDBACK);
    }

    /// <summary>
    /// Returns the attached status of the device<br/>
    /// </summary>
    /// <returns>
    /// True if device is attached
    /// </returns>
    public static bool isDeviceActive(string guidInstance) {
      return _activeDevices.ContainsKey(guidInstance);
    }

    /// <summary>
    /// Enables an FFB Effect on the device.<br/>
    /// E.g. FFBEffects.ConstantForce<br/>
    /// Refer to FFBEffects enum for all effect types
    /// </summary>
    /// <returns>
    /// True if effect was added sucessfully
    /// </returns>
    public static bool EnableFFBEffect(string guidInstance, FFBEffects effectType) {
      int hresult = Native.CreateFFBEffect(guidInstance, effectType);
      if (hresult != 0) { DebugLog($"[DirectInputManager] CreateFFBEffect Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// Removes an FFB Effect from the device<br/>
    /// Refer to FFBEffects enum for all effect types
    /// </summary>
    /// <returns>
    /// True if effect was removed sucessfully
    /// </returns>
    public static bool DestroyFFBEffect(string guidInstance, FFBEffects effectType) {
      int hresult = Native.DestroyFFBEffect(guidInstance, effectType);
      if (hresult != 0) { DebugLog($"[DirectInputManager] DestroyFFBEffect Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// Fetches supported FFB Effects by specified Device<br/>
    /// </summary>
    /// <returns>
    /// string[] of effect names supported
    /// </returns>
    public static string[] GetDeviceFFBCapabilities(string guidInstance) {
      string[] SupportedFFBEffects = null;
      int hresult = Native.EnumerateFFBEffects(guidInstance, out SupportedFFBEffects);
      if (hresult != 0) { DebugLog($"[DirectInputManager] GetDeviceFFBCapabilities Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); /*return false;*/ }
      return SupportedFFBEffects;
    }

    /// <summary>
    /// Stops and removes all active effects on a device<br/>
    /// *Warning* Effects will have to be enabled again before use<br/>
    /// </summary>
    /// <returns>
    /// True if effects stopped successfully
    /// </returns>
    public static bool StopAllFFBEffects(string guidInstance) {
      int hresult = Native.StopAllFFBEffects(guidInstance);
      if (hresult != 0) { DebugLog($"[DirectInputManager] StopAllFFBEffects Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// Stops DirectInput<br/>
    /// </summary>
    /// <returns>
    /// True if DirectInput was stopped successfully
    /// </returns>
    public static bool StopDirectInput() {
      int hresult = Native.StopDirectInput();
      if (hresult != 0) { DebugLog($"[DirectInputManager] StopDirectInput Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// Returns byte[] of state<br/>
    /// </summary>
    /// <returns>
    /// byte[]
    /// </returns>
    public static byte[] FlatStateToBytes(FlatJoyState2 state) {
      int size = Marshal.SizeOf(state);
      byte[] StateRawBytes = new byte[size];
      IntPtr ptr = Marshal.AllocHGlobal(size);
      Marshal.StructureToPtr(state, ptr, true);
      Marshal.Copy(ptr, StateRawBytes, 0, size);
      Marshal.FreeHGlobal(ptr);
      return StateRawBytes;
    }

    /// <summary>
    /// Computes MD5 for FlatState<br/>
    /// </summary>
    /// <returns>
    /// byte[] MD5 Hash
    /// </returns>
    public static byte[] FlatStateMD5(FlatJoyState2 state) {
      MD5 md5 = new MD5CryptoServiceProvider();
      var StateRawBytes = FlatStateToBytes(state);
      return md5.ComputeHash(StateRawBytes);
    }

    /// <summary>
    /// Fetches device state and triggers events if state changed<br/>
    /// </summary>
    public static void Poll(string guidInstance) {
      ActiveDeviceInfo ADI;
      if (_activeDevices.TryGetValue(guidInstance, out ADI)) { // Check if device active
        Int32 oldHash = ADI.stateHash;
        var state = GetDeviceState(guidInstance);
        ADI.stateHash = state.GetHashCode();

        if (oldHash != ADI.stateHash) {
          ADI.DeviceStateChange(ADI.deviceInfo, state); // Invoke all event listeners for this device
          //DebugLog($"{ADI.deviceInfo.productName} State Changed!");
        }
      } else {
        // Device isn't attached
      }

    }

    /// <summary>
    /// Fetches device state for all devices and queues events<br/>
    /// </summary>
    public static void PollAll() {
      foreach(ActiveDeviceInfo ADI in _activeDevices.Values) {
        Poll(ADI.deviceInfo);
      }
    }

    /// <summary>
    /// Obtains ActiveDeviceInfo for specified GUID<br/>
    /// </summary>
    /// <returns>
    /// Bool if GUID was found <br/>
    /// OUT ADI of device if found
    /// </returns>
    public static bool GetADI(string guidInstance, out ActiveDeviceInfo ADI) {
      return _activeDevices.TryGetValue(guidInstance, out ADI);
    }

    /// <summary>
    /// *Internal use only*
    /// Used to test C++ code in the DLL during devlopment
    /// </summary>
    public static string[] DEBUG1() {
      string[] DEBUGDATA = null;
      DEBUGDATA = new string[1] { "Test" };
      int hresult = Native.DEBUG1(out DEBUGDATA);
      if (hresult != 0) { DebugLog($"[DirectInputManager] DEBUG1 Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); /*return false;*/ }

      return DEBUGDATA;
    }


    //////////////////////////////////////////////////////////////
    // Device Events
    //////////////////////////////////////////////////////////////

    // Events to add listners too                   E.g. DIManager.OnDeviceAdded += MyFunctionWhenDeviceAdded;
    public static event deviceInfoEvent OnDeviceAdded;
    public static event deviceInfoEvent OnDeviceRemoved;

    // Functions to invoke event listeners
    public static void DeviceAdded  (DeviceInfo device){ OnDeviceAdded  ?.Invoke(device); } 
    public static void DeviceRemoved(DeviceInfo device){ OnDeviceRemoved?.Invoke(device); }

    // static Action InvokeDebounce;

    private static Debouncer ODCDebouncer = new Debouncer(150);        // 150ms (OnDeviceChangeDebouncer)

    /// <summary>
    /// *Internal use only*
    /// Called from the DLL when a windows WM_DEVICECHANGE event is captured
    /// This function invokes the necessary events
    /// </summary>
    private static void OnDeviceChange(DBTEvents DBTEvent) {
      //DebugLog($"[DirectInputManager] DeviceChange {DBTEvent.ToString()}");

      ODCDebouncer.Debounce(() => { ScanDevicesForChanges(); });
    }

    private static async void ScanDevicesForChanges(){
      DeviceInfo[] oldDevices = _devices;                              // Store currently known devices
      await EnumerateDevicesAsync();                                   // Fetch what devices are available now

      var removedDevices = oldDevices.Except(_devices);
      var addedDevices = _devices.Except(oldDevices);

      foreach (DeviceInfo device in removedDevices) {                  // Process removed devices
        ActiveDeviceInfo ADI;
        if(_activeDevices.TryGetValue(device.guidInstance, out ADI)){
          ADI.DeviceRemoved(device);                                   // Invoke event listeners for this device
        }
        DeviceRemoved(device);                                         // Invoke all event listeners all devices
        Destroy(device);                                               // If device was connceted remove it gracefully
        // DebugLog($"{device.productName} Removed!");
      }

      foreach (DeviceInfo device in addedDevices) {                    // Process newly added devices
        DeviceAdded(device);                                           // Invoke event to broadcast a new device is available
      }
    }

    //////////////////////////////////////////////////////////////
    // Effect Specific Methods
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Update existing effect with new DICONDITION array<br/><br/>
    /// 
    /// DICondition[DeviceFFBEffectAxesCount]:<br/><br/>
    /// deadband: Inacive Zone [-10,000 - 10,000]<br/>
    /// offset: Move Effect Center[-10,000 - 10,000]<br/>
    /// negativeCoefficient: Negative of center coefficient [-10,000 - 10,000]<br/>
    /// positiveCoefficient: Positive of center Coefficient [-10,000 - 10,000]<br/>
    /// negativeSaturation: Negative of center saturation [0 - 10,000]<br/>
    /// positiveSaturation: Positive of center saturation [0 - 10,000]<br/>
    /// </summary>
    /// <returns>
    /// A boolean representing the if the Effect updated successfully
    /// </returns>
    public static bool UpdateEffect(DeviceInfo device, DICondition[] conditions) {
      for (int i = 0; i < conditions.Length; i++) {
        conditions[i] = new DICondition();
        conditions[i].deadband =            ClampAgnostic(conditions[i].deadband,                 0, 10000);
        conditions[i].offset =              ClampAgnostic(conditions[i].offset,              -10000, 10000);
        conditions[i].negativeCoefficient = ClampAgnostic(conditions[i].negativeCoefficient, -10000, 10000);
        conditions[i].positiveCoefficient = ClampAgnostic(conditions[i].positiveCoefficient, -10000, 10000);
        conditions[i].negativeSaturation =  ClampAgnostic(conditions[i].negativeSaturation,       0, 10000);
        conditions[i].positiveSaturation =  ClampAgnostic(conditions[i].positiveSaturation,       0, 10000);
      }

      int hresult = Native.UpdateFFBEffect(device.guidInstance, FFBEffects.Spring, conditions);
      if (hresult != 0) { DebugLog($"[DirectInputManager] UpdateFFBEffect Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// Magnitude: Strength of Force [-10,000 - 10,0000]
    /// </summary>
    /// <returns>
    /// A boolean representing the if the Effect updated successfully
    /// </returns>
    public static bool UpdateConstantForceSimple(DeviceInfo device, int Magnitude) {
      DICondition[] conditions = new DICondition[1];
      for (int i = 0; i < conditions.Length; i++) {
        conditions[i] = new DICondition();
        conditions[i].deadband = 0;
        conditions[i].offset = 0;
        conditions[i].negativeCoefficient = ClampAgnostic(Magnitude, -10000, 10000);
        conditions[i].positiveCoefficient = ClampAgnostic(Magnitude, -10000, 10000);
        conditions[i].negativeSaturation = 0;
        conditions[i].positiveSaturation = 0;
      }

      int hresult = Native.UpdateFFBEffect(device.guidInstance, FFBEffects.ConstantForce, conditions);
      if (hresult != 0) { DebugLog($"[DirectInputManager] UpdateFFBEffect Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// deadband: Inacive Zone [-10,000 - 10,000]<br/>
    /// offset: Move Effect Center[-10,000 - 10,000]<br/>
    /// negativeCoefficient: Negative of center coefficient [-10,000 - 10,000]<br/>
    /// positiveCoefficient: Positive of center Coefficient [-10,000 - 10,000]<br/>
    /// negativeSaturation: Negative of center saturation [0 - 10,000]<br/>
    /// positiveSaturation: Positive of center saturation [0 - 10,000]<br/>
    /// </summary>
    /// <returns>
    /// A boolean representing the if the Effect updated successfully
    /// </returns>
    public static bool UpdateSpringSimple(DeviceInfo device, uint deadband, int offset, int negativeCoefficient, int positiveCoefficient, uint negativeSaturation, uint positiveSaturation) {
      DICondition[] conditions = new DICondition[1];
      for (int i = 0; i < conditions.Length; i++) {
        conditions[i] = new DICondition();
        conditions[i].deadband =            ClampAgnostic(deadband,                 0, 10000);
        conditions[i].offset =              ClampAgnostic(offset,              -10000, 10000);
        conditions[i].negativeCoefficient = ClampAgnostic(negativeCoefficient, -10000, 10000);
        conditions[i].positiveCoefficient = ClampAgnostic(positiveCoefficient, -10000, 10000);
        conditions[i].negativeSaturation =  ClampAgnostic(negativeSaturation,       0, 10000);
        conditions[i].positiveSaturation =  ClampAgnostic(positiveSaturation,       0, 10000);
      }

      int hresult = Native.UpdateFFBEffect(device.guidInstance, FFBEffects.Spring, conditions);
      if (hresult != 0) { DebugLog($"[DirectInputManager] UpdateFFBEffect Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// Magnitude: Strength of Force [-10,000 - 10,0000]
    /// </summary>
    /// <returns>
    /// A boolean representing the if the Effect updated successfully
    /// </returns>
    public static bool UpdateDamperSimple(DeviceInfo device, int Magnitude) {
      DICondition[] conditions = new DICondition[1];
      for (int i = 0; i < conditions.Length; i++) {
        conditions[i] = new DICondition();
        conditions[i].deadband = 0;
        conditions[i].offset = 0;
        conditions[i].negativeCoefficient = ClampAgnostic(Magnitude, -10000, 10000);
        conditions[i].positiveCoefficient = ClampAgnostic(Magnitude, -10000, 10000);
        conditions[i].negativeSaturation = 0;
        conditions[i].positiveSaturation = 0;
      }

      int hresult = Native.UpdateFFBEffect(device.guidInstance, FFBEffects.Damper, conditions);
      if (hresult != 0) { DebugLog($"[DirectInputManager] UpdateFFBEffect Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }


    /// <summary>
    /// Magnitude: Strength of Force [-10,000 - 10,0000]
    /// </summary>
    /// <returns>
    /// A boolean representing the if the Effect updated successfully
    /// </returns>
    public static bool UpdateFrictionSimple(DeviceInfo device, int Magnitude) {
      DICondition[] conditions = new DICondition[1];
      for (int i = 0; i < conditions.Length; i++) {
        conditions[i] = new DICondition();
        conditions[i].deadband = 0;
        conditions[i].offset = 0;
        conditions[i].negativeCoefficient = ClampAgnostic(Magnitude, -10000, 10000);
        conditions[i].positiveCoefficient = ClampAgnostic(Magnitude, -10000, 10000);
        conditions[i].negativeSaturation = 0;
        conditions[i].positiveSaturation = 0;
      }

      int hresult = Native.UpdateFFBEffect(device.guidInstance, FFBEffects.Friction, conditions);
      if (hresult != 0) { DebugLog($"[DirectInputManager] UpdateFFBEffect Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    /// <summary>
    /// Magnitude: Strength of Force [-10,000 - 10,0000]
    /// </summary>
    /// <returns>
    /// A boolean representing the if the Effect updated successfully
    /// </returns>
    public static bool UpdateInertiaSimple(DeviceInfo device, int Magnitude) {
      DICondition[] conditions = new DICondition[1];
      for (int i = 0; i < conditions.Length; i++) {
        conditions[i] = new DICondition();
        conditions[i].deadband = 0;
        conditions[i].offset = 0;
        conditions[i].negativeCoefficient = ClampAgnostic(Magnitude, -10000, 10000);
        conditions[i].positiveCoefficient = ClampAgnostic(Magnitude, -10000, 10000);
        conditions[i].negativeSaturation = 0;
        conditions[i].positiveSaturation = 0;
      }

      int hresult = Native.UpdateFFBEffect(device.guidInstance, FFBEffects.Inertia, conditions);
      if (hresult != 0) { DebugLog($"[DirectInputManager] UpdateFFBEffect Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}"); return false; }
      return true;
    }

    //////////////////////////////////////////////////////////////
    // Overloads - Unfortunately summaries don't propagate to overloads
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Attach to Device, ready to get state/ForceFeedback<br/><br/>
    /// </summary>
    /// <returns>
    /// A boolean representing the if the Device was attached
    /// </returns>
    public static bool Attach(DeviceInfo device) => Attach(device.guidInstance);

    /// <summary>
    /// Remove a specified Device
    /// </summary>
    /// <returns>
    /// True upon sucessful destruction
    /// </returns>
    public static bool Destroy(DeviceInfo device) => Destroy(device.guidInstance);

    /// <summary>
    /// Retrieve state of the Device, Flattened for easier comparison.<br/>
    /// </summary>
    /// <returns>
    /// FlatJoyState2
    /// </returns>
    public static FlatJoyState2 GetDeviceState(DeviceInfo device) => GetDeviceState(device.guidInstance);

    /// <summary>
    /// Retrieve state of the Device<br/>
    /// *Warning* DIJOYSTATE2 contains arrays making it difficult to compare, concider using GetDeviceState
    /// </summary>
    /// <returns>
    /// DIJOYSTATE2
    /// </returns>
    public static DIJOYSTATE2 GetDeviceStateRaw(DeviceInfo device) => GetDeviceStateRaw(device.guidInstance);

    /// <summary>
    /// Retrieve the capabilities of the device. E.g. # Buttons, # Axes, Driver Version<br/>
    /// Device must be attached first<br/>
    /// </summary>
    /// <returns>
    /// DIDEVCAPS https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee416607(v=vs.85)
    /// </returns>
    public static DIDEVCAPS GetDeviceCapabilities(DeviceInfo device) => GetDeviceCapabilities(device.guidInstance);

    /// <summary>
    /// Returns if the device has the ForceFeedback Flag<br/>
    /// </summary>
    /// <returns>
    /// True if device can provide ForceFeedback<br/>
    /// </returns>
    public static bool FFBCapable(DeviceInfo device) => FFBCapable(device.guidInstance);

    /// <summary>
    /// Returns the attached status of the device<br/>
    /// </summary>
    /// <returns>
    /// True if device is attached
    /// </returns>
    public static bool isDeviceActive(DeviceInfo device) => isDeviceActive(device.guidInstance);

    /// <summary>
    /// Enables an FFB Effect on the device.<br/>
    /// E.g. FFBEffects.ConstantForce<br/>
    /// Refer to FFBEffects enum for all effect types
    /// </summary>
    /// <returns>
    /// True if effect was added sucessfully
    /// </returns>
    public static bool EnableFFBEffect(DeviceInfo device, FFBEffects effectType) => EnableFFBEffect(device.guidInstance, effectType);

    /// <summary>
    /// Removes an FFB Effect from the device<br/>
    /// Refer to FFBEffects enum for all effect types
    /// </summary>
    /// <returns>
    /// True if effect was removed sucessfully
    /// </returns>
    public static bool DestroyFFBEffect(DeviceInfo device, FFBEffects effectType) => DestroyFFBEffect(device.guidInstance, effectType);

    /// <summary>
    /// Fetches supported FFB Effects by specified Device<br/>
    /// </summary>
    /// <returns>
    /// string[] of effect names supported
    /// </returns>
    public static string[] GetDeviceFFBCapabilities(DeviceInfo device) => GetDeviceFFBCapabilities(device.guidInstance);

    /// <summary>
    /// Stops and removes all active effects on a device<br/>
    /// *Warning* Effects will have to be enabled again before use<br/>
    /// </summary>
    /// <returns>
    /// True if effects stopped successfully
    /// </returns>
    public static bool StopAllFFBEffects(DeviceInfo device) => StopAllFFBEffects(device.guidInstance);

    /// <summary>
    /// Fetches device state and triggers events if state changed<br/>
    /// </summary>
    public static void Poll(DeviceInfo device) => Poll(device.guidInstance);

    /// <summary>
    /// Obtains ActiveDeviceInfo for specified GUID<br/>
    /// </summary>
    /// <returns>
    /// Bool if GUID was found <br/>
    /// OUT ADI of device if found
    /// </returns>    
    public static bool GetADI(DeviceInfo device, out ActiveDeviceInfo ADI) => GetADI(device.guidInstance, out ADI);

  } // End of DIManager



  //////////////////////////////////////////////////////////////
  // Utilities
  //////////////////////////////////////////////////////////////

  /// <summary>
  /// Helper class to print out user friendly system error codes.
  /// Taken from: https://stackoverflow.com/a/21174331/9053848
  /// </summary>
  public static class WinErrors {
#region definitions
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr LocalFree(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern int FormatMessage(FormatMessageFlags dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr Arguments);

    [Flags]
    private enum FormatMessageFlags : uint {
      FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
      FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
      FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
      FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000,
      FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
      FORMAT_MESSAGE_FROM_STRING = 0x00000400,
    }
#endregion

    /// <summary>
    /// Gets a user friendly string message for a system error code
    /// </summary>
    /// <param name="errorCode">System error code</param>
    /// <returns>Error string</returns>
    public static string GetSystemMessage(int errorCode) {
      try {
        IntPtr lpMsgBuf = IntPtr.Zero;

        int dwChars = FormatMessage(
            FormatMessageFlags.FORMAT_MESSAGE_ALLOCATE_BUFFER | FormatMessageFlags.FORMAT_MESSAGE_FROM_SYSTEM | FormatMessageFlags.FORMAT_MESSAGE_IGNORE_INSERTS,
            IntPtr.Zero,
            (uint)errorCode,
            0, // Default language
            ref lpMsgBuf,
            0,
            IntPtr.Zero);
        if (dwChars == 0) {
          // Handle the error.
          int le = Marshal.GetLastWin32Error();
          return "Unable to get error code string from System - Error " + le.ToString();
        }

        string sRet = Marshal.PtrToStringAnsi(lpMsgBuf);

        // Free the buffer.
        lpMsgBuf = LocalFree(lpMsgBuf);
        return sRet;
      } catch (Exception e) {
        return "Unable to get error code string from System -> " + e.ToString();
      }
    }
  }

}

/// <summary>
/// Only execute an Action after it hasn't been called for a timeout period <br/>
/// Setup: private static Debouncer DebouncerName = new Debouncer(300); // 300ms<br/>
/// Invocation: DebouncerName.Debounce(() => { Console.WriteLine("Executed"); });<br/>
/// Source: https://stackoverflow.com/a/47933557/3055031 (Modifed)
/// </summary>
public class Debouncer {
  private List<CancellationTokenSource> CancelTokens = new List<CancellationTokenSource>();
  private int TimeoutMs;
  private readonly object _lockThis = new object();                    // Use a locking object to prevent the debouncer to trigger again while the func is still running

  public Debouncer(int timeoutMs = 300) {
    this.TimeoutMs = timeoutMs;
  }

  public void Debounce(Action TargetAction) {
    CancelAllTokens();                                                 // Cancel existing Tokens Each invocation
    var tokenSource = new CancellationTokenSource();                   // Token for this invocation
    lock (_lockThis) { CancelTokens.Add(tokenSource); }                // Safely add this Token to the list
    Task.Delay(TimeoutMs, tokenSource.Token).ContinueWith(task => {    // (Note: All Tasks continue)
      if (!tokenSource.IsCancellationRequested) {                      // if this is the task that hasn't been canceled
        CancelAllTokens();                                             // Clear
        CancelTokens = new List<CancellationTokenSource>();            // Empty List
        lock (_lockThis) { TargetAction(); }                           // Excute Action
      }
    }, TaskScheduler.FromCurrentSynchronizationContext());             // Perform on current thread
  }

  private void CancelAllTokens() {
    foreach (var token in CancelTokens) {
      if (!token.IsCancellationRequested) { token.Cancel(); }
    }
  }

}