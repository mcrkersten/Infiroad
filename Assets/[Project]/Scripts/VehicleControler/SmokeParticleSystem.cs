using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeParticleSystem : MonoBehaviour
{
    private ParticleSystem pSystem;
    private Rigidbody rb;

    private void Awake()
    {
        pSystem = this.GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            rb = this.transform.root.GetComponent<Rigidbody>();

        //Sets velocity of particles as vehicle velocity;
        var main = pSystem.main;
        main.emitterVelocity = rb.velocity;
    }
}
