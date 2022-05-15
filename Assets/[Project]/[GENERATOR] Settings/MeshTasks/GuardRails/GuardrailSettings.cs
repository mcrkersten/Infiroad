using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class GuardrailSettings : MeshtaskSettings
{
	[Range(0,10)]
	public int poleSpacing;
	public float sharpCornerRadius;

	[Header("GameObjects")]
	public GameObject guardrailPolePrefab;
	public GameObject sharpCornerGuardrailPolePrefab;


	public void CreateGuardrailPole(Vector2 direction, GuardrailSettings guardrailSettings, MeshTask.Point p, Vector3 noise, float localX_offset, GameObject parent)
	{
		Vector3 offset = new Vector3(direction.x * (guardrailSettings.meshtaskWidth + localX_offset),0f, 0f) + noise;
		offset = Quaternion.Euler(0, 0, (p.extrusionVariables.averageExtrusion) * guardrailSettings.maxChamfer) * offset;

		Vector3 relativePosition = p.rotation * offset;
		Vector3 position = relativePosition + (p.position);

		GameObject pole = null;
		if (Mathf.Abs(guardrailSettings.sharpCornerRadius) < localX_offset)
			pole = GameObject.Instantiate(guardrailSettings.sharpCornerGuardrailPolePrefab, position, p.rotation, parent.transform);
		else
			pole = GameObject.Instantiate(guardrailSettings.guardrailPolePrefab, position, p.rotation, parent.transform);

		if (offset.x < 0)
			pole.transform.localScale = new Vector3(1, 1, 1);
		else
			pole.transform.localScale = new Vector3(-1, 1, 1);

	}
}


[System.Serializable]
public class VertexPosition
{
	public Vertex vertex;
	public Vector2Int line;
	public Vector2Int inversedLine;
	public bool isHardEdge;
}