using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BezierCurveDerivatives
{
    public static Vector2 VelocityDerivative(Vector2[] p, float t)
    {
        Vector2 result =
            p[0] * (-3f * Mathf.Pow(t, 2f) + 6f * t - 3f) +
            p[1] * (9f * Mathf.Pow(t, 2f) - 12f * t + 3f) +
            p[2] * (-9f * Mathf.Pow(t, 2f) + 6f * t) +
            p[3] * (3f * t);
        return result;
    }

    public static Vector2 AccelerationDerivative(Vector2[] p, float t)
    {
        Vector2 result =
            p[0] * (-6f * t + 6f) +
            p[1] * (18f * t - 12f) +
            p[2] * (-18f * t + 6) +
            p[3] * (6 * t);
        return result;
    }

    public static float BezierCurveRadius(Vector2 velocity, Vector2 acceleration)
    {
        return Determinant(velocity, acceleration) / Mathf.Pow(velocity.magnitude, 3);
    }

    public static float Determinant(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
}
