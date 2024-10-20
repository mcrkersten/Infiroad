using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(RoadDecoration))]
public class RoadDecorationInspector : Editor
{
	void OnEnable()
	{
		SceneView.duringSceneGui += OnSceneGUI;
	}

	void OnDisable()
	{
		SceneView.duringSceneGui -= OnSceneGUI;
	}

	void OnSceneGUI(SceneView sceneView)
	{

		EditorUtility.SetDirty(target);
		RoadDecoration t = target as RoadDecoration;
		Undo.RecordObject(t, "DecorationSettings");

		DrawShapes(t);
	}

    private void DrawShapes(RoadDecoration rd)
    {
        foreach (Decoration d in rd.decor)
        {
			float t = d.curveTime * 130f;
			Vector3 p1 = new Vector3(d.position.x, d.position.y, t);
			Vector3 position = Handles.PositionHandle(p1, Quaternion.identity);
			d.position = position;
		}
    }
}
