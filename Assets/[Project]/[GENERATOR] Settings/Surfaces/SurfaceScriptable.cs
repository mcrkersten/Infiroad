using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class SurfaceScriptable : ScriptableObject
{
    public AnimationCurve gripped;
    public AnimationCurve unGripped;
    public float SlipValue;
    public float UnSlipValue;
    public Material material;
    public bool UV_mirrored;
}
