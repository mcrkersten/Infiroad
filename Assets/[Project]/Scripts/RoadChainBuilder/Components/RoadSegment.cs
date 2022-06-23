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
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer) )]
public class RoadSegment : UniqueMesh {

	public bool usesSectorSurfaceScriptable;
	[HideInInspector] public OrientedCubicBezier3D bezier;
	[HideInInspector] public List<AssetSpawnEdge> assetSpawnEdges = new List<AssetSpawnEdge>();
	[HideInInspector] public List<SurfaceScriptable> surfaceSettings = new List<SurfaceScriptable>();
	// Serialized stuff, like settings
	public float tangentLength = 3; // Tangent size. Note that it's only the tangent of the first point. The next segment controls the endpoint tangent length

	// Non-serialized stuff
	RoadMeshExtruder meshExtruder = new RoadMeshExtruder();
	// Properties
	public bool HasValidNextPoint => TryGetNextSegment() != null;
	bool IsInValidChain => transform.parent.Ref()?.GetComponent<RoadChain>() != null;
	public RoadChain RoadChain => transform.parent == null ? null : transform.parent.GetComponent<RoadChain>();

	[HideInInspector]public RoadSettings roadSetting;

	public Vector2Int startEndEdgeLoop;

	public int edgeLoopCount;
	// This will regenerate the mesh!
	// uvzStartEnd is used for the (optional) normalized coordinates along the whole track,
	// x = start coordinate, y = end coordinate
	public void CreateMesh( Vector2 nrmCoordStartEnd, RoadSettings settings , int index = 0) {
		surfaceSettings.AddRange(settings.GetAllSurfaceSettings(index));
		roadSetting = settings;
		// Only generate a mesh if we've got a next control point
		if ( HasValidNextPoint ) {
			this.bezier = GetBezierRepresentation(Space.Self);
			meshExtruder.ExtrudeRoad(
				segment: this,
				mesh: Mesh,
				roadSettings: settings,
				bezier: this.bezier,
				uvMode: RoadChain.uvMode,
				nrmCoordStartEnd: nrmCoordStartEnd,
				edgeLoopsPerMeter: settings.edgeLoopsPerMeter,
				tilingAspectRatio: GetTextureAspectRatio(),
				index
			);
		} else if( meshCached != null ) {
			DestroyImmediate( meshCached );
		}
		this.GetComponent<MeshCollider>().sharedMesh = Mesh;
	}

	float GetTextureAspectRatio() {
		Texture texture = GetComponent<MeshRenderer>().sharedMaterial.Ref()?.mainTexture;
		return texture != null ? texture.AspectRatio() : 1f;
	}

	// Gets one of the 4 bezier control point locations
	// This is a bit convoluted to avoid double-transforming between spaces
	public Vector3 GetControlPoint( int i, Space space ) {
		Vector3 FromLocal( Vector3 localPos ) => space == Space.Self ? localPos : transform.TransformPoint( localPos );
		Vector3 FromWorld( Vector3 worldPos ) => space == Space.World ? worldPos : transform.InverseTransformPoint( worldPos );
		if( i < 2 ) {
			if( i == 0 )
				return FromLocal( Vector3.zero );
			if( i == 1 )
				return FromLocal( Vector3.forward * tangentLength );
		} else {
			RoadSegment next = TryGetNextSegment();
			Transform nextTf = next.transform;
			if( i == 2 )
				return FromWorld( nextTf.TransformPoint( Vector3.back * next.tangentLength ) );
			if( i == 3 )
				return FromWorld( nextTf.position );
		}
        return default;
	}
	 
	// Gives you the next road segment, if it exists
	// It also branches based on whether or not this whole road forms a loop
	RoadSegment TryGetNextSegment() {
		if( IsInValidChain == false )
			return null;
		int thisIndex = transform.GetSiblingIndex();
		bool isLast = thisIndex == transform.parent.childCount-1;
		RoadSegment GetSiblingSegment( int i ) => transform.parent.GetChild( i ).GetComponent<RoadSegment>();
		if( isLast && RoadChain.loop )
			return GetSiblingSegment( 0 ); // First segment
		else if( !isLast )
			return GetSiblingSegment( thisIndex + 1 ); // Next segment
		return null;
	}

	// Returns the oriented béziér representation of this segment
	// We need this in both world and local space! Meshes are in local space,
	// while gizmos/handles are in world space. This is used by the RoadSegmentInspector
	public OrientedCubicBezier3D GetBezierRepresentation( Space space ) {
		return new OrientedCubicBezier3D(
			GetUpVector( 0, space ),
			GetUpVector( 3, space ),
			GetControlPoint( 0, space ),
			GetControlPoint( 1, space ),
			GetControlPoint( 2, space ),
			GetControlPoint( 3, space )
		);
	}

	// Returns the up vector of either the first or last control point, in a given space
	Vector3 GetUpVector( int i, Space space ) {
		if( i == 0 ) {
			return space == Space.Self ? Vector3.up : transform.up;
		} else if( i == 3 ) {
			Vector3 wUp = TryGetNextSegment().transform.up;
			return space == Space.World ? wUp : transform.InverseTransformVector( wUp );
		}
		return default;
	}

}
