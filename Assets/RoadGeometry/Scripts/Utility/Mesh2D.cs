//
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

using UnityEngine;

[CreateAssetMenu]
public class Mesh2D : ScriptableObject {

	// A 2D vertex

	// Just like 3D meshes define connectivity with triangles by triplets of indices,
	// our 2D mesh will define connectivity with lines by pairs of indices
	public Vector2 randomHeightMinMax;
	public VertexPoint[] points;


	public int PointCount => points.Length;

	// Total length covered by the U coordinates in world space
	// Used for making sure the texture has the correct aspect ratio
	public float CalcUspan() {
		float dist = 0; 
		for( int i = 0; i < points.Length -1; i++ ) {
			Vector2 a = points[i].vertex_1.point;
			Vector2 b = points[i+1].vertex_1.point;
			dist += ( a - b ).magnitude;
		}
		return dist;
	}

	public void CalculateLine()
    {
		int count1 = 0;
        foreach (VertexPoint item in points)
        {
			if (item.isHardPoint)
				count1++;
        }

		int count2 = 0;
        for (int i = 0; i < points.Length; i++)
        {
			if (i == 0)
				points[i].line = new Vector2Int(points.Length - 1 + count1, i);
            else
				points[i].line = new Vector2Int(i - 1 + count2, i + count2);

			if (points[i].isHardPoint)
				count2++;

		}
	}

	[ContextMenu("Calculate normals")]
	public void CalculateNormals()
    {
        for (int i = 0; i < points.Length; i++)
        {
			Vector2 nextPoint = Vector2.zero;
			Vector2 currentPoint = points[i].vertex_1.point;
			if (i == points.Length - 1)
				nextPoint = points[0].vertex_1.point;
			else
				nextPoint = points[i + 1].vertex_1.point;

			float dx = nextPoint.x - currentPoint.x;
			float dy = nextPoint.y - currentPoint.y;
			points[i].vertex_1.normal = new Vector2(-dy,dx);
        }
    }
}

[System.Serializable]
public class VertexPoint
{
	public Vertex vertex_1;
	public Vector2Int line;
	public bool randomHeight;
	public bool objectSpawnPoint;
	public bool isHardPoint;
}

[System.Serializable]
public class Vertex
{
	public Vector2 point;
	public Vector2 normal;
	public float u; // UVs, but like, not V :thinking_face:
					// vertex colors
					// tangents
}