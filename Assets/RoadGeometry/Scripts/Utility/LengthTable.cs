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

// This class is used for delta compensated UVs
// More info: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A/edit#slide=id.gdefa50559_1_50
public class LengthTable {

	public float[] distances;

	int SmpCount => distances.Length;
	float TotalLength => distances[SmpCount - 1];

	public LengthTable( OrientedCubicBezier3D bezier, int precision = 16 ) {
		// Generate length table
		distances = new float[precision];
		Vector3 prevPoint = bezier.points[0];
		distances[0] = 0f;
		for( int i = 1; i < precision; i++ ) {
			float t = i / (precision - 1f);
			Vector3 currentPoint = bezier.GetPoint( t );
			float delta = (prevPoint-currentPoint).magnitude;
			distances[i] = distances[i - 1] + delta;
			prevPoint = currentPoint;
		}
	}

	// Convert the t-value to percentage of distance along the curve
	public float TToPercentage( float t ) {
		float iFloat = t * (SmpCount-1);
		int idLower = Mathf.FloorToInt(iFloat);
		int idUpper = Mathf.FloorToInt(iFloat + 1);
		if( idUpper >= SmpCount ) idUpper = SmpCount - 1;
		if( idLower < 0 ) idLower = 0;
		return Mathf.Lerp( distances[idLower], distances[idUpper], iFloat - idLower ) / TotalLength;
	}
   
}
