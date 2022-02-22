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

public static class Mathfs {

	// The circle constant
	public const float TAU = 6.28318530718f;

	// Returns a normalized vector given an angle in radians
	public static Vector2 GetUnitVectorByAngle( float angRad ) {
		return new Vector2(
			Mathf.Cos( angRad ),
			Mathf.Sin( angRad )
		);
	}

	// Given a min (a) and a max (b) value,
	// this returns the percentage at which
	// v lies, in that range
	static float InverseLerp( float a, float b, float v ) {
		return (v - a) / (b - a);
	}

	// Remaps a value in one range into another
	public static float Remap( float iMin, float iMax, float oMin, float oMax, float v ) {
		float t = InverseLerp(iMin, iMax, v);
		return Mathf.LerpUnclamped( oMin, oMax, t );
	}

}
