using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class SurfaceScriptable : ScriptableObject
{
    public AnimationCurve slip;
    public Material material;
}
