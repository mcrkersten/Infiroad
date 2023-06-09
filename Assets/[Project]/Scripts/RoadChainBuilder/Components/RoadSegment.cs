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
	[HideInInspector] public bool isExitSegment;
	[HideInInspector] public OrientedCubicBezier3D bezier;
	[HideInInspector] public List<AssetSpawnEdge> assetSpawnEdges = new List<AssetSpawnEdge>();
	[HideInInspector] public List<SurfaceScriptable> surfaceSettings = new List<SurfaceScriptable>();
	// Serialized stuff, like settings
	public float tangentLength = 3; // Tangent size. Note that it's only the tangent of the first point. The next segment controls the endpoint tangent length

	// Non-serialized stuff
	RoadMeshExtruder meshExtruder = new RoadMeshExtruder();
	// Properties
	public bool HasValidNextPoint => TryGetNextSegment() != null;
	bool IsInValidChain => transform.parent.Ref()?.GetComponent<SegmentChain>() != null;
	public SegmentChain SegmentChain => transform.parent == null ? null : transform.parent.GetComponent<SegmentChain>();

	[HideInInspector]public RoadSettings roadSetting;
    [HideInInspector]public int index;

    public Vector2Int startEndEdgeLoop;

	public int edgeLoopCount = 0;
	// This will regenerate the mesh!
	// uvzStartEnd is used for the (optional) normalized coordinates along the whole track,
	// x = start coordinate, y = end coordinate
	public void CreateMesh( Vector2 nrmCoordStartEnd, RoadSettings settings , int segmentIndex , int chainIndex = 0)
	{
		surfaceSettings.AddRange(settings.GetAllSurfaceSettings(chainIndex));
		roadSetting = settings;
		index = segmentIndex;

		// Only generate a mesh if we've got a next control point
		if ( HasValidNextPoint ) {
			CreateBezier();
			meshExtruder.ExtrudeRoad(
				segment: this,
				mesh: Mesh,
				roadSettings: settings,
				bezier: this.bezier,
				uvMode: SegmentChain.uvMode,
				nrmCoordStartEnd: nrmCoordStartEnd,
				edgeLoopsPerMeter: settings.edgeLoopsPerMeter,
				tilingAspectRatio: GetTextureAspectRatio(),
				chainIndex
			);
		} 
		else if( meshCached != null ) 
			DestroyImmediate( meshCached );
		this.GetComponent<MeshCollider>().sharedMesh = Mesh;
	}

	public void CreateBezier()
	{	
		if ( HasValidNextPoint )
			this.bezier = GetBezierRepresentation(Space.Self);
	}

	float GetTextureAspectRatio() {
		Texture texture = GetComponent<MeshRenderer>().sharedMaterial.Ref()?.mainTexture;
		return texture != null ? texture.AspectRatio() : 1f;
	}

	// Gets one of the 4 bezier control point locations
	// This is a bit convoluted to avoid double-transforming between spaces
	public Vector3 GetControlPoint( int index, Space space ) {
		Vector3 FromSelf( Vector3 selfPos ) => space == Space.Self ? selfPos : transform.TransformPoint( selfPos );
		Vector3 FromWorld( Vector3 worldPos ) => space == Space.World ? worldPos : transform.InverseTransformPoint( worldPos );
		if( index < 2 ) {
			if( index == 0 )
				return FromSelf( Vector3.zero );
			if( index == 1 )
				return FromSelf( Vector3.forward * tangentLength );
		} else {
			RoadSegment next = TryGetNextSegment();
			Transform nextPoint = next.transform;
			if( index == 2 )
				return FromWorld( nextPoint.TransformPoint( Vector3.back * next.tangentLength ) );
			if( index == 3 )
				return FromWorld( nextPoint.position );
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
		if( isLast && SegmentChain.loop )
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
