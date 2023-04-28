using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomEditor(typeof(RoadSettings))]
public class RoadSettingsInspector : Editor
{
	void OnEnable()
	{
		SceneView.duringSceneGui += OnSceneGUI;
	}

	void OnDisable()
	{
		SceneView.duringSceneGui -= OnSceneGUI;
	}

	bool drawMesh3DShape;
	bool drawMeshHandles;
	bool drawAddButtons;
	bool drawMeshtasks;
	bool drawMeshtaskHandles;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		drawMesh3DShape = EditorGUILayout.Toggle("Draw Mesh 3D Shape", drawMesh3DShape);
		drawMeshHandles = EditorGUILayout.Toggle("Draw Mesh handles", drawMeshHandles);
		drawAddButtons = EditorGUILayout.Toggle("Draw Add Buttons", drawAddButtons);

		drawMeshtasks = EditorGUILayout.Toggle("Draw Meshtasks", drawMeshtasks);
		drawMeshtaskHandles = EditorGUILayout.Toggle("Draw Mesh handles", drawMeshtaskHandles);
	}


	void OnSceneGUI(SceneView sceneView)
	{

		EditorUtility.SetDirty(target);
		RoadSettings t = target as RoadSettings;

		Undo.RecordObject(t, "RoadSettings");
		if(drawMesh3DShape)
			Draw3DShape(t);
		if(drawMeshHandles)
			DrawMeshHandles(t);
		if(drawAddButtons)
            DrawAddButtons(t);
		if(drawMeshtasks)
			DrawMeshtask(t);
	}

    private void DrawMeshtask(RoadSettings t)
    {
        foreach (MeshtaskObject g in t.meshtaskObjects)
        {
			Undo.RecordObject(g.meshtaskSettings, "GuardrailSettings" + g.ToString());
			DrawMeshtaskObject(g);

			if(drawMeshtaskHandles)
				DrawMeshtaskHandle(g);
		}
    }

    private void DrawMeshHandles(RoadSettings t)
	{
		for (int i = 0; i < t.PointCount; i++)
		{
			Vector3 position = t.points[i].vertex_1.point;
			Vector3 offset = (Vector3.right * position.x) + (Vector3.up * position.y) + (Vector3.forward * position.z);
			position = Handles.PositionHandle(offset, Quaternion.identity);
			Handles.Label(offset + new Vector3(.2f,.5f,0), i.ToString());

			t.points[i].vertex_1.point = position;
		}
	}

	private void Draw3DShape(RoadSettings t)
    {
		for (int i = 0; i < t.PointCount; i++)
		{
			//Draw 2D Shapes
			for (int row = 0; row < 10; row++)
			{
				float offsetCurve = 0f;
				Vector3 point_0 = Vector3.zero;
				Vector3 point_1 = Vector3.zero;

				if (i == t.PointCount - 1)
                {
					if (t.points[i].scalesWithCorner)
						offsetCurve = CalculateRadiiOffSet(t.points[i].vertex_1.point, t.debugRoadCurveStrenght);
					point_0 = new Vector3(t.points[i].vertex_1.point.x + offsetCurve, t.points[i].vertex_1.point.y, row * 3);
					offsetCurve = 0f;
					if (t.points[0].scalesWithCorner)
						offsetCurve = CalculateRadiiOffSet(t.points[0].vertex_1.point, t.debugRoadCurveStrenght);
					point_1 = new Vector3(t.points[0].vertex_1.point.x + offsetCurve, t.points[0].vertex_1.point.y, row * 3);
				}

				if (i < t.PointCount - 1)
				{
					if (t.points[i].scalesWithCorner)
						offsetCurve = CalculateRadiiOffSet(t.points[i].vertex_1.point, t.debugRoadCurveStrenght);
					point_0 = new Vector3(t.points[i].vertex_1.point.x + offsetCurve, t.points[i].vertex_1.point.y, row * 3);
					offsetCurve = 0f;
					if (t.points[i + 1].scalesWithCorner)
						offsetCurve = CalculateRadiiOffSet(t.points[i + 1].vertex_1.point, t.debugRoadCurveStrenght);
					point_1 = new Vector3(t.points[i + 1].vertex_1.point.x + offsetCurve, t.points[i + 1].vertex_1.point.y, row * 3);
				}

				if (t.points[i].assetSpawnPoint.Count > 0)
				{
					Handles.color = Color.red;

					foreach (AssetSpawnPoint item in t.points[i].assetSpawnPoint)
					{
                        if (item.spawnBetweenPoints)
                        {
							for (float z = .5f; z <= item.spawnPointsBetweenAmount; z++)
							{
								Vector3 point = Vector3.Lerp(point_0, point_1, (float)z / item.spawnPointsBetweenAmount);
								Handles.DrawWireDisc(point, Vector3.up, item.spawnRadius);
							}
                        }
                        else
                        {
							Vector3 point = point_0;
							Handles.DrawWireDisc(point, Vector3.up, item.spawnRadius);
							Handles.color = Color.white;
						}
					}
				}

				Handles.DrawLine(point_0, point_1, 1f);
				Handles.color = Color.white;
			}

			//Draw 3D Shape
			for (int row = 0; row < 9; row++)
			{
				float offsetCurve = 0f;
				Vector3 point_0 = Vector3.zero;
				Vector3 point_1 = Vector3.zero;

				if (t.points[i].ishardEdge)
					Handles.color = Color.blue;

				if (i < t.PointCount)
				{
					if (t.points[i].scalesWithCorner)
						offsetCurve = CalculateRadiiOffSet(t.points[i].vertex_1.point, t.debugRoadCurveStrenght);
					point_0 = new Vector3(t.points[i].vertex_1.point.x + offsetCurve, t.points[i].vertex_1.point.y, row * 3);
					point_1 = new Vector3(t.points[i].vertex_1.point.x + offsetCurve, t.points[i].vertex_1.point.y, ((row + 1) * 3));
				}

				Handles.DrawDottedLine(point_0, point_1, 1f);
				Handles.color = Color.white;
			}

			//t.roadSetting.points[i].vertex_1.point = position - t.transform.position;
		}
	}

	private void DrawMeshtaskObject(MeshtaskObject mto)
    {
		MeshtaskSettings mts = mto.meshtaskSettings;
		Vector2 mto_position = new Vector2(mto.position.x, mto.position.y);

		Handles.color = Color.white;
		for (int i = 0; i < mts.PointCount; i++)
		{
			for (int row = 0; row < 10; row++)
			{
				if (i < mts.PointCount - 1)
				{
					Vector2 p0 = new Vector2(mts.points[i].vertex.point.x, mts.points[i].vertex.point.y);
					Vector3 point_0 = new Vector3(p0.x + mto_position.x, p0.y + mto_position.y, row * 3);

					Vector2 p1 = new Vector2(mts.points[i + 1].vertex.point.x, mts.points[i + 1].vertex.point.y);
					Vector3 point_1 = new Vector3(p1.x + mto_position.x, p1.y + mto_position.y, row * 3);

					Handles.DrawLine(point_0, point_1);
				}
			}

			for (int row = 0; row < 9; row++)
			{
				Vector2 p = new Vector2(mts.points[i].vertex.point.x, mts.points[i].vertex.point.y);
				Vector3 point_0 = new Vector3(p.x + mto_position.x, p.y + mto_position.y, row * 3);
				Vector3 point_1 = new Vector3(p.x + mto_position.x, p.y + mto_position.y, ((row + 1) * 3));

				Handles.DrawLine(point_0, point_1, 1f);
			}
		}
	}

	private void DrawMeshtaskHandle(MeshtaskObject mto)
    {
		Vector3 position = mto.position;
		Vector3 endResult = Handles.PositionHandle(position, Quaternion.identity);
		Handles.Label(position + new Vector3(.2f, .5f, 0), mto.meshtaskSettings.meshTaskType.ToString());
		mto.position = endResult;

	}

	private void DrawAddButtons(RoadSettings r)
    {
        for (int i = 0; i < r.PointCount; i++)
        {
			Vector3 buttonPosition;
			if (i == r.PointCount - 1)
				buttonPosition = Vector2.Lerp(r.points[i].vertex_1.point, r.points[0].vertex_1.point, .5f);
            else
				buttonPosition = Vector2.Lerp(r.points[i].vertex_1.point, r.points[i + 1].vertex_1.point, .5f);

			if(i != 0 && i != r.PointCount - 1)
            {
				Handles.color = Color.green;
				if (Handles.Button(buttonPosition, Quaternion.identity, .1f, .1f, Handles.CircleHandleCap))
				{
					Debug.Log(i);
					Vector3 position = Vector3.Lerp(r.points[i].vertex_1.point, r.points[i + 1].vertex_1.point, .5f);
					List<VertexPoint> l = new List<VertexPoint>();
                    for (int x = 0; x < r.PointCount; x++)
                    {
						l.Add(r.points[x]);
						if (x == i)
                        {
							l.Add(new VertexPoint(position));
						}
					}
					r.points = l.ToArray();
					r.CalculateLine();
					return;
				}
			}
		}
    }

	private float CalculateRadiiOffSet(Vector2 point, float curveStrenght)
    {
		if (point.x > 0f) //Positive
			return Mathf.Max(0f, curveStrenght);
		if (point.x < 0f) //Negative
			return Mathf.Min(0f, curveStrenght);
		return 0f;
	}
}
