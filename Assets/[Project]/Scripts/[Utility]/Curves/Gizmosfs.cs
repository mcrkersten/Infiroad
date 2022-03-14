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
using UnityEditor;
#if UNITY_EDITOR
public static class Gizmosfs {

	// Drawing a wireframe circle
	public static void DrawWireCircle( Vector3 pos, Quaternion rot, float radius, int detail = 32 ) {

		Vector3[] points3D = new Vector3[detail];
		for( int i = 0; i < detail; i++ ) {
			float t = i / (float)detail;
			float angRad = t * Mathfs.TAU;
			Vector2 point2D = Mathfs.GetUnitVectorByAngle( angRad ) * radius;
			points3D[i] = pos + rot * point2D;
		}

		// Draw all points as tiny lil dots
		for( int i = 0; i < detail - 1; i++ ) {
			Gizmos.DrawLine( points3D[i], points3D[i + 1] );
		}
		Gizmos.DrawLine( points3D[detail - 1], points3D[0] );

	}

	// Draws a gizmo-like set of three colored coordinate axis lines 
	public static void DrawOrientedPoint( OrientedPoint op ) {
		void DrawAxis( Color color, Vector3 axis ) {
			Gizmos.color = color;
			Gizmos.DrawLine( op.pos, op.LocalToWorldPos( axis ) );
		}
		DrawAxis( Handles.xAxisColor, Vector3.right );
		DrawAxis( Handles.yAxisColor, Vector3.up );
		DrawAxis( Handles.zAxisColor, Vector3.forward );
		Gizmos.color = Color.white;
	}

}
#endif

