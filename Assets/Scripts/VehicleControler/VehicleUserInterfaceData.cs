using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleUserInterfaceData
{
    private Engine engine;
    private VehicleController vehicle;
    private Rigidbody rb;

    public float acceleration;
    public float brake;
    public VehicleUserInterfaceData(Engine engine, VehicleController vehicle)
    {
        this.vehicle = vehicle;
        this.engine = engine;
        rb = vehicle.GetComponent<Rigidbody>();
    }

    public int GetRPM()
    {
        return (int)engine.wheelInputRPM;
    }

    public float GetRPMPercentage()
    {
        return engine.wheelInputRPM / vehicle.maxRPM;
    }


    public int GetCurrentGear()
    {
        return engine.CurrentGear;
    }

    public int GetCurrentSpeed()
    {
        return (int)(Mathf.Abs(rb.transform.InverseTransformDirection(rb.GetPointVelocity(rb.transform.position)).z) * 3.6f);
    }
}
