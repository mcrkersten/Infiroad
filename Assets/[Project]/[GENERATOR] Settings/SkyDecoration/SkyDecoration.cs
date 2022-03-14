using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu, System.Serializable]
public class SkyDecoration : ScriptableObject
{
    public float skyHeight;
    public List<SkyDecor> skyDecors = new List<SkyDecor>();
    public int decorAmount;
}

[System.Serializable]
public class SkyDecor
{
    public Vector3 spawnAreaSize;
    public GameObject prefab;
    [Range(0f,1f)]
    public float probability;
}
