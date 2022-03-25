using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationTriggerAsset : MonoBehaviour
{
    [HideInInspector] public List<VegetationAssetTypeTag> scannedBy = new List<VegetationAssetTypeTag>();
    public List<VegetationAssetTypeTag> tags = new List<VegetationAssetTypeTag>();
}

