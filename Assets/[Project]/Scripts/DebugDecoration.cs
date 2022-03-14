using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugDecoration : MonoBehaviour
{
    public RoadDecoration roadDecoration;
    private List<GameObject> instantiations = new List<GameObject>();

    [ContextMenu("Debug Decor")]
    public void ShowDecoration()
    {
        ResetDebug();
        InstantiateDecoration();
    }

    private void ResetDebug()
    {
        for (int i = 0; i < instantiations.Count; i++)
            DestroyImmediate(instantiations[i]);
        instantiations = new List<GameObject>();
    }

    private void InstantiateDecoration()
    {
        foreach (Decoration decor in roadDecoration.decor)
        {
            float t = decor.curveTime * 130f;
            Vector3 p1 = new Vector3(decor.position.x, decor.position.y, t);
            GameObject g = Instantiate(decor.prefab, p1, Quaternion.identity, this.transform);
            instantiations.Add(g);
        }
    }
}
