using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Ui_AnimationGameObject : MonoBehaviour
{
    public Ui_AnimationObject ui_Animation;
    public List<TextMeshProUGUI> textElements = new List<TextMeshProUGUI>();


    public void Start()
    {
        ui_Animation.Init();
    }
}
