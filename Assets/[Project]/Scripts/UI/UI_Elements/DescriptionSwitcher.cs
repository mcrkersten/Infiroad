using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DescriptionSwitcher : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    public void SetDescriptionText(string text)
    {
        textMesh.text = text;
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Justified;
    }

    public void ResetDescription()
    {
        textMesh.text = "Hover over buttons to view description";
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Top;
    }
}
