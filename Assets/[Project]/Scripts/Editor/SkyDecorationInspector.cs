using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SkyDecoration))]
public class SkyDecorationInspector : Editor
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
		SkyDecoration t = target as SkyDecoration;
		Undo.RecordObject(t, "SkyDecorationSettings");

		DrawShapes(t);
	}

	private void DrawShapes(SkyDecoration rd)
	{
		foreach (SkyDecor d in rd.skyDecors)
		{
			Handles.color = Color.green;
			Handles.DrawWireCube(Vector3.zero + (Vector3.up * rd.skyHeight), d.spawnAreaSize);
		}
	}
}
