using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevLight : MonoBehaviour
{
    [SerializeField] private Material lit;
    [SerializeField] private Material unlit;
    [SerializeField] private MeshRenderer mr;

    public void LitLight()
    {
        mr.material = lit;
    }
    public void UnlitLight()
    {
        mr.material = unlit;
    }
}
