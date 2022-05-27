using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeParticleSystem : MonoBehaviour
{
    private ParticleSystem pSystem;
    public Rigidbody rb;

    private void Awake()
    {
        pSystem = this.GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        //Sets velocity of particles as vehicle velocity;
        var main = pSystem.main;
        main.emitterVelocity = rb.velocity;
    }
}
