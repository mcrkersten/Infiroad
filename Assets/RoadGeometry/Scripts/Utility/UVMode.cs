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

// Specifies how to generate the V coordinate of UVs in a béziér mesh.
//
// Normalized:
//		The texture is stretched to the full length
// Tiled:
//		The texture is tiling with basic compensation,
//		to prevent the texture from stretching on long curves.
//		It does also snap to the nearest match, to make it seamless with minimal stretching
// TiledDeltaCompensated:
//		Same as Tiled, but it also compensates the inherent non-uniformity of Béziér curves,
//		where the t value does not increase uniformly with distance.
//		This mode prevents that stretching/squishing effect, using a length table!
//		More info in my slides: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A/edit#slide=id.gdefa50559_1_50
public enum UVMode {
	Normalized,
	Tiled,
	TiledDeltaCompensated
}
