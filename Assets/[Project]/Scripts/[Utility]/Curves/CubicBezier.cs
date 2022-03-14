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
using Unity.Mathematics;

// This class contains the Béziér evaluation math
[System.Serializable]
public class OrientedCubicBezier3D {

	// Control points of the curve
	public Vector3[] points = new Vector3[4];

	// Up vector for the start and end point, respectively.
	// This is required for correct and consistent orientation across the curve
	public Vector3 upStart;
	public Vector3 upEnd;

	// Shortcut to access the control points with bezier[i]
	public Vector3 this[int i] => points[i];

	// Constructor
	public OrientedCubicBezier3D( Vector3 upStart, Vector3 upEnd, params Vector3[] points ) {
		this.points = points;
		this.upStart = upStart;
		this.upEnd = upEnd;
	}

	// Get the OrientedPoint given a t-value, optionally with an orientation easing curve set
	public OrientedPoint GetOrientedPoint( float t, Ease orientationEase = Ease.Linear ) {
		return new OrientedPoint( GetPoint(t), GetOrientation( orientationEase.GetEased( t ) ) );
	}

	public OrientedPoint GetOrientedPoint(float t, float offset, Ease orientationEase = Ease.Linear)
	{
		return new OrientedPoint(GetPoint(t), GetOrientation(orientationEase.GetEased(t + offset)));
	}

	// Returns the orientation at any given t-value along the curve
	Quaternion GetOrientation( float t ) {
		Vector3 forward = GetTangent( t );
		Vector3 up = Vector3.Slerp( upStart, upEnd, t ).normalized;

		return Quaternion.LookRotation( forward, up );
	}

	// There's no closed-form solution for the arc length of
	// the cubic béziér curve. If you're curious as for why:
	// https://en.wikipedia.org/wiki/Abel%E2%80%93Ruffini_theorem
	// So we instead subdivide the curve into linear segments,
	// and sum the length of each segment
	public float GetArcLength( int precision = 16 ) {
		Vector3[] points = new Vector3[precision];
		for( int i = 0; i < precision; i++ ) {
			float t = i / (precision-1);
			points[i] = GetPoint( t );
		}
		float dist = 0;
		for( int i = 0; i < precision - 1; i++ ) {
			Vector3 a = points[i];
			Vector3 b = points[i+1];
			dist += Vector3.Distance( a, b );
		}
		return dist;
	}


	// Quick evaluation of the point at any given t-value using Bernstein Polynomials
	// More info on my blog post on Béziér curves:
	// https://medium.com/@Acegikmo/the-ever-so-lovely-b%C3%A9zier-curve-eb27514da3bf
	public Vector3 GetPoint( float t ) {
		float omt = 1 - t;
		float omt2 = omt * omt;
		float t2 = t * t;
		return
			points[0] * (omt2 * omt) +
			points[1] * (3 * omt2 * t) +
			points[2] * (3 * omt * t2) +
			points[3] * (t2 * t);
	}

    // Quick evaluation of the tangent of a given point.
    // This one is my own little invention I derived,
    // which is a Bernstein-like evaluation but for the tangent instead of
    // position, which is much faster than the lerp method.
    // More info in the slides of a talk I gave:
    // https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A/edit#slide=id.gc41ce114c_1_1
    public Vector3 GetTangent(float t)
    {
        float omt = 1f - t;
        float omt2 = omt * omt;
        float t2 = t * t;
        Vector3 tangent =
            points[0] * (-omt2) +
            points[1] * (3 * omt2 - 2 * omt) +
            points[2] * (-3 * t2 + 2 * t) +
            points[3] * (t2);
        return tangent.normalized;
    }

 //       public Vector3 GetTangent(float t)
	//{
	//	Vector3 a = Vector3.Lerp(points[0], points[1], t);
	//	Vector3 b = Vector3.Lerp(points[1], points[2], t);
	//	Vector3 c = Vector3.Lerp(points[2], points[3], t);
	//	Vector3 d = Vector3.Lerp(a, b, t);
	//	Vector3 e = Vector3.Lerp(b, c, t);
	//	return (e - d).normalized;
	//}

	public Vector2 GetFlatVelocity(float t)
	{
		float t2 = t * t;
		Vector3 velocity =
			points[0] * (-3f * t2 * 6f * t - 3f) +
			points[1] * (9f * t2 - 12f * t + 3f) +
			points[2] * (-9f * t2 + 6f * t) +
			points[3] * (3f * t);

		Vector2 solution = new Vector2(velocity.x, velocity.z);
		return solution;
	}

	public Vector2 GetFlatAcceleration(float t)
    {
		Vector3 acceleration =
			points[0] * (-6f * t + 6f) +
			points[1] * (18f * t - 12f) +
			points[2] * (-18f * t + 6f) +
			points[3] * (6f * t);

		Vector2 solution = new Vector2(acceleration.x, acceleration.z);
		return solution;
    }


	public float GetDeterminant(float t)
    {
		Vector2 a = GetFlatVelocity(t);
		Vector2 b = GetFlatAcceleration(t);
		float determinant = a.x * b.y - a.y * b.x;
		return determinant;
    }

	public float GetFlatCurvature(float t)
	{
		float determinant = GetDeterminant(t);
		float speed = GetFlatVelocity(t).magnitude;
		float K = determinant / math.pow(speed, 3);
		return K;
	}

	public float GetRadius(float t)
    {
		return 1f / GetFlatCurvature(t);
    }


	// These are more intuitive, easy to grasp
	// and slightly more numerically stable methods,
	// but, they are much slower to compute,
	// and the other methods are practically accurate enough
	/*
	public Vector3 GetPoint( float t ) {
			Vector3 a = Vector3.Lerp( points[0], points[1], t );
			Vector3 b = Vector3.Lerp( points[1], points[2], t );
			Vector3 c = Vector3.Lerp( points[2], points[3], t );
			Vector3 d = Vector3.Lerp( a, b, t );
			Vector3 e = Vector3.Lerp( b, c, t );
			return Vector3.Lerp( d, e, t );
	}

	public Vector3 GetTangent( float t ) {
		Vector3 a = Vector3.Lerp( points[0], points[1], t );
		Vector3 b = Vector3.Lerp( points[1], points[2], t );
		Vector3 c = Vector3.Lerp( points[2], points[3], t );
		Vector3 d = Vector3.Lerp( a, b, t );
		Vector3 e = Vector3.Lerp( b, c, t );
		return ( e - d ).normalized;
	}
	*/

}
