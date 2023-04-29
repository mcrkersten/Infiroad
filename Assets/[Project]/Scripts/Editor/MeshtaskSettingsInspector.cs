using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.UIElements;

[CustomEditor(typeof(MeshtaskSettings))]
public class MeshtaskSettingsInspector : Editor
{
	public override void OnInspectorGUI()
    {
		DrawDefaultInspector();
		if(GUILayout.Button("Build From PointFile"))
		{
            MeshtaskSettings t = target as MeshtaskSettings;
			List<Vector2> points = JsonVector2Reader.GetVector2ListFromJson(t.PointsFile.text);
            t.SetListOfPoints(points);
        }
	}

	void OnEnable()
	{
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}

	void OnSceneGUI(SceneView sceneView)
	{
		EditorUtility.SetDirty(target);
		MeshtaskSettings t = target as MeshtaskSettings;

		Undo.RecordObject(t, "MeshSettings");
		Draw3DShape(t);
		DrawHandles(t);
		t.EDITOR_offSetPosition = DrawOffsetHandle(t);
	}

	public void Draw3DShape(MeshtaskSettings t)
    {
		for (int i = 0; i < t.PointCount; i++)
		{
			//Draw 2D Shapes
			for (int row = 0; row < 10; row++)
			{
				Vector3 point_0 = t.EDITOR_offSetPosition;
				Vector3 point_1 = t.EDITOR_offSetPosition;

				if (i == t.PointCount - 1 && t.meshIsClosed)
                {
					point_0 += new Vector3(t.points[i].vertex.point.x, t.points[i].vertex.point.y, row * 3);
					point_1 += new Vector3(t.points[0].vertex.point.x, t.points[0].vertex.point.y, row * 3);
				}

				if (i < t.PointCount - 1)
				{
					point_0 += new Vector3(t.points[i].vertex.point.x, t.points[i].vertex.point.y, row * 3);
					point_1 += new Vector3(t.points[i + 1].vertex.point.x, t.points[i + 1].vertex.point.y, row * 3);
				}

				Handles.DrawLine(point_0, point_1, 1f);
				Handles.color = Color.white;
			}

			//Draw 3D Shape
			for (int row = 0; row < 9; row++)
			{
				Vector3 point_0 = t.EDITOR_offSetPosition;
				Vector3 point_1 = t.EDITOR_offSetPosition;

				if (t.points[i].isHardEdge)
					Handles.color = Color.blue;

				if (i < t.PointCount)
				{
					point_0 += new Vector3(t.points[i].vertex.point.x, t.points[i].vertex.point.y, row * 3);
					point_1 += new Vector3(t.points[i].vertex.point.x, t.points[i].vertex.point.y, ((row + 1) * 3));
				}

				Handles.DrawDottedLine(point_0, point_1, 1f);
				Handles.color = Color.white;
			}
		}
	}

	public void DrawHandles(MeshtaskSettings t)
	{
		for (int i = 0; i < t.PointCount; i++)
		{
			Vector3 position = t.points[i].vertex.point;
			position += t.EDITOR_offSetPosition;
			Vector3 offset = (Vector3.right * position.x) + (Vector3.up * position.y) + (Vector3.forward * position.z);
			position = Handles.PositionHandle(offset, Quaternion.identity);
			Handles.Label(offset + new Vector3(.2f,.5f,0), i.ToString());

			position -= t.EDITOR_offSetPosition;
			t.points[i].vertex.point = position;
		}
	}

	public Vector3 DrawOffsetHandle(MeshtaskSettings t)
    {
        Vector3 position = Handles.PositionHandle(t.EDITOR_offSetPosition, Quaternion.identity);
		return position;
	}
}
