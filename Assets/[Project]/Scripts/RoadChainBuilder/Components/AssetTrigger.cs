using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetTrigger : MonoBehaviour
{
    [HideInInspector] public List<VegetationScannerTypeTag> scannedBy = new List<VegetationScannerTypeTag>();
    public List<VegetationScannerTypeTag> scanableByScannerType = new List<VegetationScannerTypeTag>();
}

