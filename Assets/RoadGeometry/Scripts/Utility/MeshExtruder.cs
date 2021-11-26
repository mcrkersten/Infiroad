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

using System.Collections.Generic;
using UnityEngine;

// This class contains the heart of the spline extrusion code!
// You provide data and a mesh, and this will write to that mesh for you!
public class MeshExtruder {

	// Used when generating the mesh
	List<Vector3> verts = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();
	List<Vector2> uvs0 = new List<Vector2>();
	List<Vector2> uvs1 = new List<Vector2>();
	List<int> triIndices = new List<int>();
	RoadChain chain;

	
	public void Extrude( RoadChain roadChain, RoadSegment segment, Mesh mesh, Mesh2D mesh2D, OrientedCubicBezier3D bezier, Ease rotationEasing, UVMode uvMode, Vector2 nrmCoordStartEnd, float edgeLoopsPerMeter, float tilingAspectRatio ) {
		mesh2D.CalculateLine();
		// Clear all data. This could be optimized by using arrays and only reconstruct when lengths change
		mesh.Clear();
		verts.Clear();
		normals.Clear();
		uvs0.Clear();
		uvs1.Clear();
		triIndices.Clear();
		this.chain = roadChain;

		// UVs/Texture fitting
		LengthTable table = null;
		if(uvMode == UVMode.TiledDeltaCompensated)
			table = new LengthTable( bezier, 12 );
		float curveArcLength = bezier.GetArcLength();
		
		// Tiling
		float tiling = tilingAspectRatio;
		if( uvMode == UVMode.Tiled || uvMode == UVMode.TiledDeltaCompensated ) {
			float uSpan = mesh2D.CalcUspan(); // World space units covered by the UVs on the U axis
			tiling *= curveArcLength / uSpan;
			tiling = Mathf.Max( 1, Mathf.Round( tiling ) ); // Snap to nearest integer to tile correctly
		}

		float min = float.PositiveInfinity;
		float max = float.NegativeInfinity;
		int hardEdgeCount = 0;
		foreach (var item in mesh2D.points)
		{
			if (item.vertex_1.point.x < min)
				min = item.vertex_1.point.x;

			if (item.vertex_1.point.x > max)
				max = item.vertex_1.point.x;

			if (item.isHardPoint)
				hardEdgeCount++;
		}

		// Generate vertices
		// Foreach edge loop
		// Calc edge loop count
		int targetCount = Mathf.RoundToInt( curveArcLength * edgeLoopsPerMeter );
		int edgeLoopCount = Mathf.Max( 2, targetCount ); // Make sure it's at least 2
		for( int ring = 0; ring < edgeLoopCount; ring++ ) {
			float t = ring / (edgeLoopCount-1f);
			OrientedPoint op = bezier.GetOrientedPoint( t, rotationEasing );

			// Prepare UV coordinates. This branches lots based on type
			float tUv = t;
			if( uvMode == UVMode.TiledDeltaCompensated )
				tUv = table.TToPercentage( tUv );
			float uv0V = tUv * tiling;
			float uv1U = Mathf.Lerp( nrmCoordStartEnd.x, nrmCoordStartEnd.y, tUv ); // Normalized coordinate for entire chain

			// Foreach vertex in the 2D shape
			for( int i = 0; i < mesh2D.PointCount; i++ ) {
				Vector2 pointPosition = mesh2D.points[i].vertex_1.point;
                if (mesh2D.points[i].randomHeight && ring != 0 && ring != edgeLoopCount -1)
                {
					Vector2 p = mesh2D.points[i].vertex_1.point;
					p[1] += p[1] + Random.Range(mesh2D.randomHeightMinMax.x, mesh2D.randomHeightMinMax.y);
					pointPosition = p;
                }

                if (mesh2D.points[i].objectSpawnPoint)
                {
					roadChain.meshSpawnPoints.AddPoint(segment.transform.TransformPoint(op.LocalToWorldPos(pointPosition)), i);
				}
				float u = Mathf.InverseLerp(min, max, mesh2D.points[i].vertex_1.point.x);
				verts.Add( op.LocalToWorldPos(pointPosition) ); //World position of point
				uvs0.Add( new Vector2(u, uv0V ) );
				uvs1.Add( new Vector2( uv1U, 0 ) );
				if (mesh2D.points[i].isHardPoint)
				{
					verts.Add(op.LocalToWorldPos(pointPosition)); //World position of point
					uvs0.Add(new Vector2(u, uv0V));
					uvs1.Add(new Vector2(uv1U, 0));
				}
			}
		}


		// Generate Trianges
		// Foreach edge loop (except the last, since this looks ahead one step)
		for ( int edgeLoop = 0; edgeLoop < edgeLoopCount - 1; edgeLoop++ ) {
			//Debug.Log("New Edgeloop");
			int rootIndex = ( mesh2D.PointCount + hardEdgeCount ) * edgeLoop;
			int rootIndexNext = ( mesh2D.PointCount + hardEdgeCount ) * (edgeLoop + 1);
			Debug.Log("new edge " + edgeLoop);
			// Foreach pair of line indices in the 2D shape
			for ( int line = 0; line < mesh2D.PointCount; line++ ) {

				int vertex1 = mesh2D.points[line].line.x;
				int vertex2 = mesh2D.points[line].line.y;
				//Bottom
				Vector2Int current = new Vector2Int();
				current[0] = vertex1 + rootIndex;
				current[1] = vertex2 + rootIndex;
				//Debug.Log(current.x +" "+current.y);

				//TOP
				Vector2Int next = new Vector2Int();
				next[0] = vertex1 + rootIndexNext;
				next[1] = vertex2 + rootIndexNext;
				
				triIndices.Add( current.x );
				triIndices.Add( next.x );
				triIndices.Add( next.y );

				triIndices.Add( current.x );
				triIndices.Add( next.y );
				triIndices.Add( current.y );
			}
		}

		// Assign it all to the mesh
		mesh.SetVertices( verts );
		mesh.SetUVs( 0, uvs0 );
		mesh.SetUVs( 1, uvs1 );
		mesh.SetTriangles( triIndices, 0 );
		mesh.RecalculateNormals();
	}


}
