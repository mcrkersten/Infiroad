using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RadioSystem
{
    public AK.Wwise.RTPC radioSystemAudio;
    public AK.Wwise.RTPC radioNoiseAudio;
    public void SetRadioQuality(float quality)
    {
        radioSystemAudio.SetGlobalValue(quality);
        radioNoiseAudio.SetGlobalValue(quality);
    }

    public void SkipSong()
    {

    }
}
