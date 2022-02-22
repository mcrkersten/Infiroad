using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;
using System;
using System.Linq;


// Define the Volume Component for the custom post processing effect 
[System.Serializable, VolumeComponentMenu("Cinematic URP Post Processing/CUPP - PRISM Long Exposure")]
public class PRISMLongExposure : VolumeComponent
{
    [Tooltip("Intensity of the effect")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    public ClampedFloatParameter exposureTime = new ClampedFloatParameter(0f, 0f, 30f);
}

// Define the renderer for the custom post processing effect
[CustomPostProcess("CUPP - PRISMLongExposure", CustomPostProcessInjectionPoint.BeforePostProcess)]
public class PRISMLongExposure_URP : CustomPostProcessRenderer
{
    // A variable to hold a reference to the corresponding volume component
    private PRISMLongExposure m_VolumeComponent;

    // The postprocessing material
    private Material m_Material;

    public RenderTargetHandle _intermediate = default;

    private float t = 0.0f;

    // The ids of the shader variables
    static class ShaderIDs
    {
        internal readonly static int Input = Shader.PropertyToID("_MainTex");
        internal readonly static int Intensity = Shader.PropertyToID("_Intensity");

        internal readonly static int LongExpTex = Shader.PropertyToID("_LongExpTex");
    }

    // By default, the effect is visible in the scene view, but we can change that here.
    public override bool visibleInSceneView => true;

    /// Specifies the input needed by this custom post process. Default is Color only.
    public override ScriptableRenderPassInput input => ScriptableRenderPassInput.Color;

    /// <summary>
    /// Used to store the history of a camera (its previous frame and whether it is valid or not)
    /// </summary>
    private class CameraHistory
    {
        public RenderTexture Frame { get; set; }
        public bool Invalidated { get; set; }

        public CameraHistory(RenderTextureDescriptor descriptor)
        {
            Frame = new RenderTexture(descriptor);
            Invalidated = false;
        }
    }


    // We store the history for each camera separately (key is the camera instance id).
    private Dictionary<int, CameraHistory> _histories = null;

    // Initialized is called only once before the first render call
    // so we use it to create our material
    public override void Initialize()
    {
        _histories = new Dictionary<int, CameraHistory>();
        m_Material = CoreUtils.CreateEngineMaterial("PRISM/PRISMLongExposure");
    }

    // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
    public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // Get the current volume stack
        var stack = VolumeManager.instance.stack;
        // Get the corresponding volume component
        m_VolumeComponent = stack.GetComponent<PRISMLongExposure>();
        // if blend value > 0, then we need to render this effect. 
        return m_VolumeComponent.intensity.value > 0;
    }


    protected void SetShaderValues(Material longExpMaterial, Camera m_Camera)
    {
        longExpMaterial.SetFloat(ShaderIDs.Intensity, m_VolumeComponent.intensity.value);
    }

    // Dispose of the allocated resources
    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Release every history RT
        foreach (var entry in _histories)
        {
            entry.Value.Frame.Release();
        }
        // Clear the histories dictionary
        _histories.Clear();
    }

    // The actual rendering execution is done here
    public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // set material properties
        if (m_Material != null)
        {
            m_Material.SetFloat(ShaderIDs.Intensity, m_VolumeComponent.intensity.value);
        }

        float xpTime = m_VolumeComponent.exposureTime.value;
        float deltaTimeMultiplier = Time.deltaTime;
        t += deltaTimeMultiplier / xpTime;
        if (t >= 1)
        {
            //doUpdate = false;
            t = 0f;
            foreach (var entry in _histories)
            {
                entry.Value.Invalidated = true;
            }
        }
        //Debug.Log("T: " + t + ", expWeight: " + (deltaTimeMultiplier / xpTime));
        m_Material.SetFloat("individualExposureWeight", deltaTimeMultiplier / xpTime);

        cmd.SetGlobalTexture(ShaderIDs.Input, source);

        SetShaderValues(m_Material, renderingData.cameraData.camera);

        // Get camera instance id
        int id = renderingData.cameraData.camera.GetInstanceID();

        // Create a temporary target
        RenderTextureDescriptor descriptor = GetTempRTDescriptor(renderingData);

        var bHalf = RenderTexture.GetTemporary(descriptor);

        CameraHistory history;
        // See if we already have a history for this camera
        if (_histories.TryGetValue(id, out history))
        {
            var frame = history.Frame;
            // If the camera target is resized, we need to resize the history too.
            if (frame.width != descriptor.width || frame.height != descriptor.height)
            {
                RenderTexture newframe = new RenderTexture(descriptor);
                newframe.name = "_CameraHistoryTexture";
                if (history.Invalidated) // if invalidated, blit from source to history
                    cmd.Blit(source, newframe);//, m_Material, 13);
                else // if not invalidated, copy & resize the old history to the new size
                    Graphics.Blit(frame, newframe);//, m_Material, 13);
                frame.Release();
                history.Frame = newframe;
            }
            else if (history.Invalidated)
            {
                //Debug.LogError("History invalidated");
                cmd.Blit(source, frame);//, m_Material, 13); // if invalidated, blit from source to history
            }
            history.Invalidated = false; // No longer invalid :D

            //Debug.Log("GOT HISTORY");
        }
        else
        {
            // If we had no history for this camera, create one for it.
            history = new CameraHistory(descriptor);
            history.Frame.name = "_CameraHistoryTexture";
            _histories.Add(id, history);
            cmd.Blit(source, history.Frame);//, m_Material, 13); // Copy frame from source to history

            //Debug.Log("GOT HISTORY 2");
        }

        int initialPass = 0;
        //int secondPass = 0;
        //int thirdPass = 0;

        //History.Frame = last frame. We need to blend our NEW frame AND history.frame into new RT
        cmd.SetGlobalTexture(ShaderIDs.Input, source);
        m_Material.SetTexture(ShaderIDs.LongExpTex, history.Frame);
        cmd.Blit(source, bHalf, m_Material, initialPass);

        //Now, send the new RT as our destination
        cmd.SetGlobalTexture(ShaderIDs.Input, bHalf);
        cmd.Blit(bHalf, destination);

        //Now, blit the new RT back into History.Frame
        m_Material.SetTexture(ShaderIDs.Input, bHalf);
        cmd.Blit(bHalf, history.Frame);

        RenderTexture.ReleaseTemporary(bHalf);

       // Debug.LogError("Go to end");
    }

}
