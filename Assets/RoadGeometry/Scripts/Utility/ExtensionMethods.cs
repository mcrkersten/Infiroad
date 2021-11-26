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

public static class ExtensionMethods {

	// So we can use the null-propagation operator with Unity objects
	public static T Ref<T>( this T obj ) where T : Object {
		return obj == null ? null : obj;
	}

	public static float AspectRatio( this Texture texture ) {
		return texture.width / texture.height;
	}

	// Clamp bottom
	public static float AtLeast( this float v, float minVal ) => Mathf.Max( v, minVal );

}
