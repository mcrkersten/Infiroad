﻿//
// Dedicated to all my Patrons on Patreon,
// as a thanks for your continued support 💖
//
// Source code © Freya Holmér, 2019
// This code is provided exclusively to supporters,
// under the Attribution Assurance License
// "https://tldrlegal.com/license/attribution-assurance-license-(aal)"
// 
// You can basically do whatever you want with this code,
// as long as you include this license and credit me for it,
// in both the source code and any released binaries using this code
//
// Thank you so much again <3
//
// Freya
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class Road : ScriptableObject {

	public List<RoadSettings> roadSettings = new List<RoadSettings>();

	[Header("Road Decoration")]
	public List<RoadDecoration> standardDecoration = new List<RoadDecoration>();
	public List<RoadDecoration> randomizedDecoration = new List<RoadDecoration>();
	public List<RoadDecoration> sceneryObjects = new List<RoadDecoration>();

	[Header("Sky Decoration")]
	public SkyDecoration skyDecoration;

	public GameObject assetSpawnPoint;
	public int assetSpawnPointPoolSize;
}

[System.Serializable]
public class VariationSettings
{
	public List<Variation> roadSettings;

	[System.Serializable]
	public class Variation
    {
		[Range(0f, 1f)]
		public float weight;
		public RoadSettings roadSettings;
	}
}



[System.Serializable]
public class VertexPoint
{
	public VertexPoint(Vector3 point)
    {
		vertex_1 = new Vertex();
		vertex_1.point = point;
    }

	public VertexPoint()
    {

    }

	[Header("Vertex Information")]
	public Vertex vertex_1;
	public Vector2Int line;

	[Header("Vertex Settings")]
	public bool ishardEdge;
	public int noiseChannel;
	public int materialIndex;
	public bool scalesWithCorner;
	public bool extrudePoint;

	[Header("Asset Settings")]
	public List<AssetSpawnPoint> assetSpawnPoint = new List<AssetSpawnPoint>();

}

[System.Serializable]
public class AssetSpawnPoint
{
	public List<VegetationScannerTypeTag> assetPointType;
	public float spawnRadius;
	public bool spawnBetweenPoints;
	public int spawnPointsBetweenAmount;
}

[System.Serializable]
public class Vertex
{
	public Vector2 point;
	[HideInInspector]public float uvX;
}