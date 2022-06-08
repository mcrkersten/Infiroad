using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleUserInterfaceData
{
    private Engine2 engine;
    private Rigidbody rb;

    public float acceleration;
    public float brake;
    public VehicleUserInterfaceData(Engine2 engine, VehicleController vehicle)
    {
        this.engine = engine;
        rb = vehicle.GetComponent<Rigidbody>();
    }

    public int GetRPM()
    {
        return 0;
    }

    public float GetRPMPercentage()
    {
        return 0;
    }


    public int GetCurrentGear()
    {
        return 0;
    }

    public int GetCurrentSpeed()
    {
        return (int)(Mathf.Abs(rb.transform.InverseTransformDirection(rb.GetPointVelocity(rb.transform.position)).z) * 3.6f);
    }
}
