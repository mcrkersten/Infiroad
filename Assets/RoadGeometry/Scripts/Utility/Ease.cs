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

// Simple easing functions. Used for the rotation easing setting on road segments
public enum Ease { Linear, In, Out, InOut }

public static class EaseExtensions {

	public static float GetEased( this Ease ease, float t ) {
		switch( ease ) {
			case Ease.In:    return t*t;
			case Ease.Out:   return (2-t)*t;
			case Ease.InOut: return -t*t*(2*t-3);
			default:	     return t;
		}
	}

}
