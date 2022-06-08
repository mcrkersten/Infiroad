using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownForceWing : MonoBehaviour
{
    public float wingAngle;
    public float width;
    public float lenght;
    private Rigidbody rb;

    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
    }

    public float GetArea()
    {
        return width * lenght;
    }

    public float CalculateLiftforce(float airDensity)
    {
        float cof = 2 * Mathf.PI * wingAngle * Mathf.Deg2Rad;
        float forwardPointVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(transform.position)).z;
        float force = (cof * airDensity * Mathf.Sqrt(forwardPointVelocity) * GetArea()) / 2;

        if (float.IsNaN(force))
            return 0f;

        return force;
    }
}
