using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TireCollision : MonoBehaviour
{
    [Tooltip("Collision force needed to break the suspension")]
    [SerializeField] float breakForce;
    [SerializeField] Rigidbody rb;
    [SerializeField] private List<Wheel_Raycast> wheelRaycasts = new List<Wheel_Raycast>();

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Collider col = collision.GetContact(0).thisCollider;
        Wheel_Raycast raycast = wheelRaycasts.FirstOrDefault(c => c.tireCollider == col);
        if (raycast == null)
            return;

        Vector3 contactNormal = collision.GetContact(0).normal;
        Debug.Log(collision.relativeVelocity.magnitude, collision.gameObject);
        if (collision.relativeVelocity.magnitude > breakForce)
            raycast.DamageSuspension(rb.GetPointVelocity(collision.GetContact(0).point), collision.relativeVelocity, contactNormal);
    }
}
