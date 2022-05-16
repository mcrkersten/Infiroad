using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class GuardrailSettings : MeshtaskSettings
{
	[Range(0,10)]
	public int poleSpacing;

	[Header("GameObjects")]
	public GameObject guardrailPolePrefab;
	public GameObject sharpCornerGuardrailPolePrefab;
}