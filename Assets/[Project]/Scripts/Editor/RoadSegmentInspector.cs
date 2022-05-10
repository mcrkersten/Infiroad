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
using System;
using System.Collections;
using System.Collections.Generic;

// This custom inspector is here to give us an extra handle to control the tangent length of each curve segment
// It's a bit involved since it is using a lot of the undocumented Unity handle features, but, hopefully it's readable!
// (I had to read up a lot to refresh my knowledge on this because holy heck this is convoluted)
[CustomEditor(typeof(RoadSegment))]
public class RoadSegmentInspector : Editor {
	const string UNDO_STR_ADJUST = "adjust béziér tangent";

	void OnSceneGUI() {
		RoadSegment t = target as RoadSegment;
		EditorUtility.SetDirty(this);

		// Generate IDs for the UI controls
		int arrowIDFrw  = GUIUtility.GetControlID( "Arrow Frw".GetHashCode(),  FocusType.Passive );
		int arrowIDBack = GUIUtility.GetControlID( "Arrow Back".GetHashCode(), FocusType.Passive );

		// Gather data on tangent locations, directions and the origin
		RoadSegment road = target as RoadSegment;
		Vector3 origin = road.GetControlPoint( 0, Space.World );
		Vector3 tangentFrw = road.GetControlPoint( 1, Space.World );
		Vector3 tangentBack = origin * 2 - tangentFrw; // Mirror tangent point around origin
		Vector3 tangentDir = road.transform.forward;

		// Calculate a plane to project against with the mouse ray while dragging
		Vector3 camUp = SceneView.lastActiveSceneView.camera.transform.up;
		Vector3 pNormal = Vector3.Cross( tangentDir, camUp ).normalized;
		Plane draggingPlane = new Plane( pNormal, origin );

		// This function had so many shared parameters, might as well shorten it a bit
		float newDistance = 0;
		bool TangentHandle( int id, Vector3 handlePos, Vector3 direction ) => DrawTangentHandle( id, handlePos, origin, direction, draggingPlane, ref newDistance );

		// Draw handles, and modify tangent if you dragged them
		bool changedFrw  = TangentHandle( arrowIDFrw,  tangentFrw,   tangentDir );
		bool changedBack = TangentHandle( arrowIDBack, tangentBack, -tangentDir );

		// If any of the two were changed, assign the new distance to the tangent length!
		if( changedFrw || changedBack ) {
			Undo.RecordObject( target, UNDO_STR_ADJUST );
			road.tangentLength = newDistance;
			//road.RoadChain?.CreateMeshes();
		}
		
	}


	bool DrawTangentHandle( int id, Vector3 handlePos, Vector3 origin, Vector3 direction, Plane draggingPlane, ref float newDistance ) {

		bool wasChanged = false;
		float size = HandleUtility.GetHandleSize( handlePos ); // For screen-relative size
		float handleRadius = size * 0.25f;
		float cursorDistancePx = HandleUtility.DistanceToCircle( handlePos, handleRadius * 0.5f );

		// Input states
		Event e = Event.current;
		bool leftMouseButtonDown = e.button == 0;
		bool isDraggingThis = GUIUtility.hotControl == id && leftMouseButtonDown;
		bool isHoveringThis = HandleUtility.nearestControl == id;

		// IMGUI branching! DrawTangentHandle is run once per event type
		switch( e.type ) {
			case EventType.Layout:
				// Layout is called very early, and is where we set up interactable controls
				HandleUtility.AddControl( id, cursorDistancePx );
				break;
			case EventType.MouseDown:
				// Focus this control if we clicked it
				if( isHoveringThis && leftMouseButtonDown ) {
					GUIUtility.hotControl = id;
					GUIUtility.keyboardControl = id;
					e.Use();
				}
				break;
			case EventType.MouseDrag:
				if( isDraggingThis ) {
					// Raycast a plane along the axis
					Ray r = HandleUtility.GUIPointToWorldRay( e.mousePosition );
					if( draggingPlane.Raycast( r, out float dist ) ) {
						Vector3 intersectionPt = r.GetPoint( dist );
						// Now project the intersected point on the plane onto the tangent vector line
						// This also ensures it doesn't go under 0.5 meters.
						// Under that, and the mesh will look all fucky
						// Negative values go wild and incorrect ✨
						// So let's not do that
						float projectedDistance = Vector3.Dot( intersectionPt - origin, direction ).AtLeast(0.5f);
						newDistance = projectedDistance;
						wasChanged = true;
					}
					e.Use();
				}
				break;
			case EventType.MouseUp:
				// Defocus control on release
				if( isDraggingThis ) {
					GUIUtility.hotControl = 0;
					e.Use();
				}
				break;
			case EventType.Repaint:
				// Finally, we have to draw everything
				// Set color based on hover/drag state
				Color color = GetHandleColor( isHoveringThis, isDraggingThis );
				using( new TemporaryHandleColor( color ) ) {
					Handles.DrawAAPolyLine( origin, handlePos );
					Quaternion rot = Quaternion.LookRotation( direction );
					Handles.SphereHandleCap( id, handlePos, rot, handleRadius, EventType.Repaint );
				}
				break;
		}

		return wasChanged;
	}

	// We want different colors of the handle based on hover states
	Color GetHandleColor( bool hovering, bool dragging ) {
		if( dragging )
			return Color.yellow;
		else if( hovering )
			return Color.Lerp( Color.yellow, Color.white, 0.6f );
		return Handles.zAxisColor;
	}

	// To more cleanly do temporary handle color
	// This is incredibly overengineered for its one purpose in this code,
	// but that's mostly because I wanted to learn the using/IDisposable syntax!
	class TemporaryHandleColor : IDisposable {
		static Stack<Color> colorStack = new Stack<Color>();
		public TemporaryHandleColor( Color color ) {
			colorStack.Push( Handles.color );
			Handles.color = color;
		}
		public void Dispose() => Handles.color = colorStack.Pop();
	}


}
