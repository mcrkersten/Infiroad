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

// This is a helper data type to manage an orientation and a position as a single object.
// You can think of it as a lightweight transform without a GameObject, locked to a scale of (1,1,1)
public struct OrientedPoint {

	public Vector3 pos;
	public Quaternion rot;

	public OrientedPoint( Vector3 pos, Quaternion rot ) {
		this.pos = pos;
		this.rot = rot;
	}

	public OrientedPoint( Vector3 pos, Vector3 forward ) {
		this.pos = pos;
		this.rot = Quaternion.LookRotation( forward );
	}

	public Vector3 LocalToWorldPos( Vector3 localSpacePos ) {
		return pos + rot * localSpacePos;
	}

	public Vector3 LocalToWorldVec( Vector3 localSpacePos ) {
		return rot * localSpacePos;
	}


}
