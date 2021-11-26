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

// This generates a ring of quads. Kinda like two connected rings, or a flattened ring!
// You can specify detail level and such, and it'll dynamically update
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class QuadRing : UniqueMesh {

	// Specify what type of projection we want!
	// Either an Angular/Radiual projection, wrapping around the ring, or
	// a planar projection on the Z axis, like a top down flat projection
	public enum UvProjection {
		AngularRadial,
		ProjectZ
	}
	
	// Configuration
	[Range(3,128)]
	[SerializeField] int angularSegmentCount = 3; // You can also think of it as the detail level of the circle
	[SerializeField] float radiusInner = 0.5f;
	[SerializeField] float thickness = 0.5f;
	[SerializeField] UvProjection uvProjection = UvProjection.AngularRadial;
	[SerializeField] int textureTiling = 1; // Used to repeat the texture

	// Properties for some simple calculations
	float RadiusOuter => radiusInner + thickness;
	int VertexCount => angularSegmentCount * 2;

	// Data holders used when generating the mesh
	List<Vector3> vertices = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();
	List<Vector2> uvs = new List<Vector2>();
	List<int> triangleIndices = new List<int>();

	void Awake() => GenerateMesh();

#if UNITY_EDITOR
	// Obviously not ideal to generate every frame in the editor, but,
	// you know how to optimize this with caching and whatnot so 'sfine!
	void Update() => GenerateMesh();
#endif

	void GenerateMesh() {

		// Clear existing data first
		Mesh.Clear();
		vertices.Clear();
		normals.Clear();
		uvs.Clear();
		triangleIndices.Clear();

		// We now iterate angularly around the whole ring
		for( int i = 0; i < angularSegmentCount+1; i++ ) {

			// t is the percentage of progress around the ring.
			// t = 0.0 = first vertex
			// t = 0.5 = halfway
			// t = 1.0 = final vertex (same location as first)
			float t = i / (float)angularSegmentCount;

			// Current angle in radians
			float angRad = t * Mathfs.TAU; 

			// Get a normalized vector pointing in that angle
			Vector2 dir = Mathfs.GetUnitVectorByAngle( angRad );

			// Now we add the vertex positions. Outer vertex first, then inner
			vertices.Add( dir * RadiusOuter);
			vertices.Add( dir * radiusInner);

			// Both vertices have the same normal, just add both
			normals.Add( Vector3.forward );
			normals.Add( Vector3.forward );

			// Calculate UVs based on projection method
			switch( uvProjection ) {
				case UvProjection.AngularRadial:
					// Angular/Radial projection, wrapping around the circle
					uvs.Add( new Vector2( t * textureTiling, 1 ) );
					uvs.Add( new Vector2( t * textureTiling, 0 ) );
					break;
				case UvProjection.ProjectZ:
					// Top-down/planar projection
					uvs.Add( ( dir * 0.5f + Vector2.one * 0.5f ) * textureTiling );
					uvs.Add( ( dir * (radiusInner / RadiusOuter) * 0.5f + Vector2.one * 0.5f) * textureTiling );
					break;
			}
		}

		// Now we need to iterate over all segments again, to connect vertices with triangles
		// Each of these indices refer to a specific vertex, and we then find neighboring ones
		// to define a quad for each segment
		for( int i = 0; i < angularSegmentCount; i++ ) {

			int indexRoot = i * 2;				// Outer circle
			int indexInnerRoot = indexRoot + 1; // Inner circle
			int indexOuterNext = indexRoot + 2; // Outer circle, but on the next segment
			int indexInnerNext = indexRoot + 3; // Inner circle, but on the next segment

			// First triangle
			triangleIndices.Add( indexRoot );
			triangleIndices.Add( indexOuterNext );
			triangleIndices.Add( indexInnerNext );

			// Second triangle
			triangleIndices.Add( indexRoot );
			triangleIndices.Add( indexInnerNext );
			triangleIndices.Add( indexInnerRoot );

		}

		// Apply it all to the mesh
		Mesh.SetVertices( vertices );
		Mesh.SetTriangles( triangleIndices, 0 );
		Mesh.SetNormals( normals );
		Mesh.SetUVs( 0, uvs );


	}










}

