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

// This is a simple example of how to generate a single quad mesh
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class QuadGenerator : UniqueMesh {

	void Awake() {

		// These are vertex locations in the mesh local space
		List<Vector3> points = new List<Vector3>(){
			new Vector3(-.5f, .5f),
			new Vector3( .5f, .5f),
			new Vector3(-.5f,-.5f),
			new Vector3(.5f,-.5f)
		};

		// These define how to connect triangles across your supplied vertices
		// Each number is the index of any given vertex,
		// and each triplet of integers is a single triangle.
		// In this case we have two triangles.
		// The face normal will point according to the left-hand-rule.
		// Hold up your left hand and do a thumbs-up!
		// The face normal / front face, will point in the direction of your thumb,
		// if the order you supply the vertex indices
		// is in the curl direction the rest of your fingers
		int[] triIndices = new int[]{
			1,0,2,
			3,1,2
		};

		// 2D coordinates, one per vertex, usually used for textures
		List<Vector2> uvs = new List<Vector2>(){
			new Vector2(1,1),
			new Vector2(0,1),
			new Vector2(1,0),
			new Vector2(0,0),
		};

		// Vertex normals, commonly used for shading! One per vertex
		List<Vector3> normals = new List<Vector3>(){
			new Vector3(0,0,1),
			new Vector3(0,0,1),
			new Vector3(0,0,1),
			new Vector3(0,0,1)
		};

		// Apply all of this to the mesh object
		Mesh.SetVertices( points );
		Mesh.SetNormals( normals );
		Mesh.SetUVs( channel:0, uvs ); // You can have multiple UV channels, which is why it's useful to specify which one
		Mesh.triangles = triIndices;

	}




}

