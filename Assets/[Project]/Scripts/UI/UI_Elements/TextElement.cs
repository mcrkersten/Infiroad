using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextElement : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> textElements = new List<TextMeshProUGUI>();

    public void UpdateTextElement(int index, string text)
    {
        if (textElements.Count > index)
            textElements[index].text = text;
        else
            Debug.LogWarning("trying to update element outside of list bounds", this);
    }
}
