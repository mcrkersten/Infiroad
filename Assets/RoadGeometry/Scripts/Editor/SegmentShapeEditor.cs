using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(RoadChain))]
public class SegmentShapeEditor : Editor
{
    // Start is called before the first frame update

    private void OnSceneGUI()
    {
        RoadChain t = target as RoadChain;
        if(t != null && t.allSegments != null && t.allSegments.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < t.mesh2D.points.Length; i++)
            {
                Vector3 position = t.mesh2D.points[i].vertex_1.point;
                position = Handles.PositionHandle(position + t.allSegments[0].transform.position, Quaternion.identity);
                t.mesh2D.points[i].vertex_1.point = position - t.allSegments[0].transform.position;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t.mesh2D, "Changed 2D object");
                t.UpdateMeshes();
            }
        }
    }
}
