using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable, SelectionBase]
public class SurfaceScriptableSector : ScriptableObject
{
    public List<SurfaceScriptable> layers = new List<SurfaceScriptable>();
    public SurfaceScriptable runoffMaterial;
}
