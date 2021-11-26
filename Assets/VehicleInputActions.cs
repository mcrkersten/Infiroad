// GENERATED AUTOMATICALLY FROM 'Assets/VehicleInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @VehicleInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @VehicleInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""VehicleInputActions"",
    ""maps"": [
        {
            ""name"": ""Vehicle"",
            ""id"": ""3165492c-09f6-4833-a3da-0255ebcee1d5"",
            ""actions"": [
                {
                    ""name"": ""Steering"",
                    ""type"": ""PassThrough"",
                    ""id"": ""9ac870d3-fc4a-4823-af27-033be7b5c295"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Braking"",
                    ""type"": ""Value"",
                    ""id"": ""60732a60-f93c-4182-8da6-b4606b15086e"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Acceleration"",
                    ""type"": ""Value"",
                    ""id"": ""0b6bfa0f-2068-4b47-9c52-13899dd4442d"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Clutch"",
                    ""type"": ""Value"",
                    ""id"": ""2569e045-4d67-4d60-96fd-f06ae46828a4"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""StartEngine"",
                    ""type"": ""Button"",
                    ""id"": ""17f2383c-40db-40af-9e9c-0d5e70cd9243"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ShiftUP"",
                    ""type"": ""Button"",
                    ""id"": ""b6945e8a-e13b-4e90-b12e-9b25b4e5bab8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ShiftDOWN"",
                    ""type"": ""Button"",
                    ""id"": ""8dcb1d5e-7e88-4d08-8b8d-e2ae6403b12d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""18c0ef5e-0ed4-4b36-bd72-319c6d2532ea"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""638824c8-82fd-4909-9ae1-6a975cf14a66"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""56c92af4-5632-4cac-8074-a9a6765d54e8"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""ddfd05d0-8e54-4833-a145-bed3c5af1601"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""c57a5c8b-fdff-41b0-ba83-d72125db3414"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""bc015404-74e5-4384-84cb-876171895a20"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""4d30236c-aa05-4c59-b6f1-43f3bb2922d5"",
                    ""path"": ""<HID::Logitech G29 Driving Force Racing Wheel>/rz"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Braking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c0cf47cc-cc89-464a-a430-248e4c65d582"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Braking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9a98d459-15df-4ae6-b4b3-3a4f8473c22f"",
                    ""path"": ""<HID::Logitech G29 Driving Force Racing Wheel>/z"",
                    ""interactions"": """",
                    ""processors"": ""Invert"",
                    ""groups"": """",
                    ""action"": ""Acceleration"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0f790006-a774-41bd-bf51-7b97296e6029"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Acceleration"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0e5fa782-79a0-4d2f-b263-e58642a65c18"",
                    ""path"": ""<Keyboard>/rightShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""StartEngine"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""15b2144a-627f-4f8e-b645-fde31ec56dcd"",
                    ""path"": ""<HID::Logitech G29 Driving Force Racing Wheel>/stick/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Clutch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9c9c493c-b0ab-4953-a0d6-981ec1e9b726"",
                    ""path"": ""<HID::Logitech G29 Driving Force Racing Wheel>/button5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShiftUP"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0448a37b-5f9a-439d-bca7-a037706ceba4"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShiftUP"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f23ccedf-aabe-4cfa-a90f-e2a0331c2e88"",
                    ""path"": ""<HID::Logitech G29 Driving Force Racing Wheel>/button6"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShiftDOWN"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8f137a5a-602a-46e8-a069-4de598e1ebc2"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShiftDOWN"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""New action map"",
            ""id"": ""d7d295d2-aa13-4859-8fec-62bd2a9dfc12"",
            ""actions"": [
                {
                    ""name"": ""New action"",
                    ""type"": ""Button"",
                    ""id"": ""9e10c162-f755-4e2e-99a0-4317ebba2ae3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""01b2d4c4-74fc-4f92-b1df-57e8aaef9105"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""New action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Touch"",
            ""bindingGroup"": ""Touch"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Joystick"",
            ""bindingGroup"": ""Joystick"",
            ""devices"": [
                {
                    ""devicePath"": ""<Joystick>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""XR"",
            ""bindingGroup"": ""XR"",
            ""devices"": [
                {
                    ""devicePath"": ""<XRController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Vehicle
        m_Vehicle = asset.FindActionMap("Vehicle", throwIfNotFound: true);
        m_Vehicle_Steering = m_Vehicle.FindAction("Steering", throwIfNotFound: true);
        m_Vehicle_Braking = m_Vehicle.FindAction("Braking", throwIfNotFound: true);
        m_Vehicle_Acceleration = m_Vehicle.FindAction("Acceleration", throwIfNotFound: true);
        m_Vehicle_Clutch = m_Vehicle.FindAction("Clutch", throwIfNotFound: true);
        m_Vehicle_StartEngine = m_Vehicle.FindAction("StartEngine", throwIfNotFound: true);
        m_Vehicle_ShiftUP = m_Vehicle.FindAction("ShiftUP", throwIfNotFound: true);
        m_Vehicle_ShiftDOWN = m_Vehicle.FindAction("ShiftDOWN", throwIfNotFound: true);
        // New action map
        m_Newactionmap = asset.FindActionMap("New action map", throwIfNotFound: true);
        m_Newactionmap_Newaction = m_Newactionmap.FindAction("New action", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Vehicle
    private readonly InputActionMap m_Vehicle;
    private IVehicleActions m_VehicleActionsCallbackInterface;
    private readonly InputAction m_Vehicle_Steering;
    private readonly InputAction m_Vehicle_Braking;
    private readonly InputAction m_Vehicle_Acceleration;
    private readonly InputAction m_Vehicle_Clutch;
    private readonly InputAction m_Vehicle_StartEngine;
    private readonly InputAction m_Vehicle_ShiftUP;
    private readonly InputAction m_Vehicle_ShiftDOWN;
    public struct VehicleActions
    {
        private @VehicleInputActions m_Wrapper;
        public VehicleActions(@VehicleInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Steering => m_Wrapper.m_Vehicle_Steering;
        public InputAction @Braking => m_Wrapper.m_Vehicle_Braking;
        public InputAction @Acceleration => m_Wrapper.m_Vehicle_Acceleration;
        public InputAction @Clutch => m_Wrapper.m_Vehicle_Clutch;
        public InputAction @StartEngine => m_Wrapper.m_Vehicle_StartEngine;
        public InputAction @ShiftUP => m_Wrapper.m_Vehicle_ShiftUP;
        public InputAction @ShiftDOWN => m_Wrapper.m_Vehicle_ShiftDOWN;
        public InputActionMap Get() { return m_Wrapper.m_Vehicle; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(VehicleActions set) { return set.Get(); }
        public void SetCallbacks(IVehicleActions instance)
        {
            if (m_Wrapper.m_VehicleActionsCallbackInterface != null)
            {
                @Steering.started -= m_Wrapper.m_VehicleActionsCallbackInterface.OnSteering;
                @Steering.performed -= m_Wrapper.m_VehicleActionsCallbackInterface.OnSteering;
                @Steering.canceled -= m_Wrapper.m_VehicleActionsCallbackInterface.OnSteering;
                @Braking.started -= m_Wrapper.m_VehicleActionsCallbackInterface.OnBraking;
                @Braking.performed -= m_Wrapper.m_VehicleActionsCallbackInterface.OnBraking;
                @Braking.canceled -= m_Wrapper.m_VehicleActionsCallbackInterface.OnBraking;
                @Acceleration.started -= m_Wrapper.m_VehicleActionsCallbackInterface.OnAcceleration;
                @Acceleration.performed -= m_Wrapper.m_VehicleActionsCallbackInterface.OnAcceleration;
                @Acceleration.canceled -= m_Wrapper.m_VehicleActionsCallbackInterface.OnAcceleration;
                @Clutch.started -= m_Wrapper.m_VehicleActionsCallbackInterface.OnClutch;
                @Clutch.performed -= m_Wrapper.m_VehicleActionsCallbackInterface.OnClutch;
                @Clutch.canceled -= m_Wrapper.m_VehicleActionsCallbackInterface.OnClutch;
                @StartEngine.started -= m_Wrapper.m_VehicleActionsCallbackInterface.OnStartEngine;
                @StartEngine.performed -= m_Wrapper.m_VehicleActionsCallbackInterface.OnStartEngine;
                @StartEngine.canceled -= m_Wrapper.m_VehicleActionsCallbackInterface.OnStartEngine;
                @ShiftUP.started -= m_Wrapper.m_VehicleActionsCallbackInterface.OnShiftUP;
                @ShiftUP.performed -= m_Wrapper.m_VehicleActionsCallbackInterface.OnShiftUP;
                @ShiftUP.canceled -= m_Wrapper.m_VehicleActionsCallbackInterface.OnShiftUP;
                @ShiftDOWN.started -= m_Wrapper.m_VehicleActionsCallbackInterface.OnShiftDOWN;
                @ShiftDOWN.performed -= m_Wrapper.m_VehicleActionsCallbackInterface.OnShiftDOWN;
                @ShiftDOWN.canceled -= m_Wrapper.m_VehicleActionsCallbackInterface.OnShiftDOWN;
            }
            m_Wrapper.m_VehicleActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Steering.started += instance.OnSteering;
                @Steering.performed += instance.OnSteering;
                @Steering.canceled += instance.OnSteering;
                @Braking.started += instance.OnBraking;
                @Braking.performed += instance.OnBraking;
                @Braking.canceled += instance.OnBraking;
                @Acceleration.started += instance.OnAcceleration;
                @Acceleration.performed += instance.OnAcceleration;
                @Acceleration.canceled += instance.OnAcceleration;
                @Clutch.started += instance.OnClutch;
                @Clutch.performed += instance.OnClutch;
                @Clutch.canceled += instance.OnClutch;
                @StartEngine.started += instance.OnStartEngine;
                @StartEngine.performed += instance.OnStartEngine;
                @StartEngine.canceled += instance.OnStartEngine;
                @ShiftUP.started += instance.OnShiftUP;
                @ShiftUP.performed += instance.OnShiftUP;
                @ShiftUP.canceled += instance.OnShiftUP;
                @ShiftDOWN.started += instance.OnShiftDOWN;
                @ShiftDOWN.performed += instance.OnShiftDOWN;
                @ShiftDOWN.canceled += instance.OnShiftDOWN;
            }
        }
    }
    public VehicleActions @Vehicle => new VehicleActions(this);

    // New action map
    private readonly InputActionMap m_Newactionmap;
    private INewactionmapActions m_NewactionmapActionsCallbackInterface;
    private readonly InputAction m_Newactionmap_Newaction;
    public struct NewactionmapActions
    {
        private @VehicleInputActions m_Wrapper;
        public NewactionmapActions(@VehicleInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Newaction => m_Wrapper.m_Newactionmap_Newaction;
        public InputActionMap Get() { return m_Wrapper.m_Newactionmap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(NewactionmapActions set) { return set.Get(); }
        public void SetCallbacks(INewactionmapActions instance)
        {
            if (m_Wrapper.m_NewactionmapActionsCallbackInterface != null)
            {
                @Newaction.started -= m_Wrapper.m_NewactionmapActionsCallbackInterface.OnNewaction;
                @Newaction.performed -= m_Wrapper.m_NewactionmapActionsCallbackInterface.OnNewaction;
                @Newaction.canceled -= m_Wrapper.m_NewactionmapActionsCallbackInterface.OnNewaction;
            }
            m_Wrapper.m_NewactionmapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Newaction.started += instance.OnNewaction;
                @Newaction.performed += instance.OnNewaction;
                @Newaction.canceled += instance.OnNewaction;
            }
        }
    }
    public NewactionmapActions @Newactionmap => new NewactionmapActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    private int m_TouchSchemeIndex = -1;
    public InputControlScheme TouchScheme
    {
        get
        {
            if (m_TouchSchemeIndex == -1) m_TouchSchemeIndex = asset.FindControlSchemeIndex("Touch");
            return asset.controlSchemes[m_TouchSchemeIndex];
        }
    }
    private int m_JoystickSchemeIndex = -1;
    public InputControlScheme JoystickScheme
    {
        get
        {
            if (m_JoystickSchemeIndex == -1) m_JoystickSchemeIndex = asset.FindControlSchemeIndex("Joystick");
            return asset.controlSchemes[m_JoystickSchemeIndex];
        }
    }
    private int m_XRSchemeIndex = -1;
    public InputControlScheme XRScheme
    {
        get
        {
            if (m_XRSchemeIndex == -1) m_XRSchemeIndex = asset.FindControlSchemeIndex("XR");
            return asset.controlSchemes[m_XRSchemeIndex];
        }
    }
    public interface IVehicleActions
    {
        void OnSteering(InputAction.CallbackContext context);
        void OnBraking(InputAction.CallbackContext context);
        void OnAcceleration(InputAction.CallbackContext context);
        void OnClutch(InputAction.CallbackContext context);
        void OnStartEngine(InputAction.CallbackContext context);
        void OnShiftUP(InputAction.CallbackContext context);
        void OnShiftDOWN(InputAction.CallbackContext context);
    }
    public interface INewactionmapActions
    {
        void OnNewaction(InputAction.CallbackContext context);
    }
}
