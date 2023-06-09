using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Wheel_Raycast : MonoBehaviour
{
    public bool debug;
    public Transform TireWheelAssembly => tireWheelAssembly;
    [SerializeField] private Transform tireWheelAssembly;
    [SerializeField] private Transform tire;
    [SerializeField] private Transform suspensionTransform;
    public Collider tireCollider;

    [Header("General Slip")]
    public SurfaceScriptable currentSurface;


    [SerializeField] private AnimationCurve brakeLock;
    [SerializeField] private AnimationCurve wheelSpin;


    private Vector3 lastHitPosition = Vector3.zero;

    [HideInInspector] public Vector3 wheelVelocityLocalSpace;
    [HideInInspector] public float steeringWheelForce;
    public float RPM;

    [Header("Wheel")]
    public float wheelRadius;
    private float tireCircumference;


    public GameObject smokeParticleSystemPrefab;
    [SerializeField] private ParticleSystem slipSmokeParticleSystem;

    [HideInInspector] public Vector3 forceDirectionDebug = Vector3.zero;
    [HideInInspector] public float grip_UI = 0.01f;
    public float gripTime_UI = 0.01f;
    private Rigidbody rb;

    //Start vectors
    private Vector3 startLocalEulerAngle;
    private Vector3 TWAstartLocalEulerAngle;
    private Vector3 suspensionLocalStartPosition;

    private void Awake()
    {
        rb = this.transform.root.GetComponent<Rigidbody>();

        startLocalEulerAngle = this.transform.localEulerAngles;
        TWAstartLocalEulerAngle = tireWheelAssembly.localEulerAngles;
    }

    private void Start()
    {
        tireCircumference = (Mathf.PI * 2f) * wheelRadius;
        suspensionTransform.localPosition = suspensionTransform.parent.InverseTransformPoint(this.transform.position);
        suspensionLocalStartPosition = suspensionTransform.localPosition;
    }

    public void SetSteerAngle(float angle)
    {
        this.transform.localEulerAngles = new Vector3(startLocalEulerAngle.x, angle + startLocalEulerAngle.y, startLocalEulerAngle.z);
        tireWheelAssembly.localEulerAngles = new Vector3(TWAstartLocalEulerAngle.x, angle + TWAstartLocalEulerAngle.y, TWAstartLocalEulerAngle.z);
    }

    public bool Raycast(float maxLenght, LayerMask layerMask, out RaycastHit hit)
    {
        if (Physics.SphereCast(this.transform.position, wheelRadius, -this.transform.up, out RaycastHit hitPoint, maxLenght, layerMask))
        {
            suspensionTransform.localPosition = suspensionLocalStartPosition + (-Vector3.up * (hitPoint.distance));
            lastHitPosition = hitPoint.point;
            hit = hitPoint;

            //Material m = GetMaterialFromRaycastHit(hit, hit.transform.GetComponent<Mesh>());
            //currentSurface = hit.transform.GetComponent<RoadSegment>()?.surfaceSettings.First(s => s.material == m);

            if(slipSmokeParticleSystem != null)
                slipSmokeParticleSystem.transform.position = hit.point;
            return true;
        }
        slipSmokeParticleSystem.transform.position = this.transform.position;
        //suspensionTransform.transform.localPosition = suspensionLocalStartPosition + (-this.transform.up * maxLenght);
        hit = new RaycastHit();
        return false;
    }

    private Material GetMaterialFromRaycastHit(RaycastHit hit, Mesh mesh)
    {
        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return null;

        mesh = meshCollider.sharedMesh;
        Renderer renderer = hit.collider.GetComponent<MeshRenderer>();

        int[] hitTriangle = new int[]
        {
                    mesh.triangles[hit.triangleIndex * 3],
                    mesh.triangles[hit.triangleIndex * 3 + 1],
                    mesh.triangles[hit.triangleIndex * 3 + 2]
        };

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] subMeshTris = mesh.GetTriangles(i);
            for (int j = 0; j < subMeshTris.Length; j += 3)
            {
                if (subMeshTris[j] == hitTriangle[0] &&
                    subMeshTris[j + 1] == hitTriangle[1] &&
                    subMeshTris[j + 2] == hitTriangle[2])
                {
                    return renderer.sharedMaterials[i];
                }
            }
        }
        return null;
    }

    float lastRotation;
    public Vector2 RotateWheelModel(Vector2 slipForce)
    {
        float wheelLockPercentage = brakeLock.Evaluate(Mathf.Abs(slipForce.x));
        float wheelSpinPercentage = wheelSpin.Evaluate(Mathf.Abs(slipForce.y));
        float combinedPercentage = wheelLockPercentage + wheelSpinPercentage;
        float rotationSpeed = (wheelVelocityLocalSpace.z * 3.6f) / tireCircumference;
        float rotation = rotationSpeed * Mathf.Clamp(combinedPercentage, 0f , 1f);

        if(debug)
            Debug.Log(combinedPercentage);

        if (float.IsNaN(rotation))
        {
             rotation = lastRotation;
             lastRotation = lastRotation * .9f;
        }
        else
            lastRotation = rotation;

        if(tire != null)
            tire.Rotate(Vector3.right, rotation, Space.Self);
        else
            tireWheelAssembly.Rotate(Vector3.right, rotation, Space.Self);

        float meterPerSecond = wheelVelocityLocalSpace.z;
        RPM = meterPerSecond / wheelRadius;

        TyreLockSmoke(wheelLockPercentage, lastHitPosition);

        return new Vector2(wheelLockPercentage, wheelSpinPercentage);
    }

    private float smokeDuration = 0f;
    private float lastFrameLockPercentage = 0f;
    public void TyreLockSmoke(float wheelLockPercentage, Vector3 position)
    {
        SlipSmoke(wheelLockPercentage);
        if(wheelLockPercentage < .2f)
        {
            smokeDuration += Time.deltaTime;
        }
        else
        {
            if(lastFrameLockPercentage < .2f)
            {
                ReleaseSmoke(position);
            }
        }
        lastFrameLockPercentage = wheelLockPercentage;

    }

    private void ReleaseSmoke(Vector3 position)
    {
        GameObject s = Instantiate(smokeParticleSystemPrefab, position, Quaternion.identity, tireWheelAssembly);
        s.GetComponent<SmokeParticleSystem>().rb = rb;
        ParticleSystem ps = s.GetComponent<ParticleSystem>();

        var emission = ps.emission;
        AnimationCurve ourCurve = new AnimationCurve();
        ourCurve.AddKey(0.0f, smokeDuration * this.transform.root.InverseTransformDirection(rb.GetPointVelocity(transform.position)).z * 2f);
        ourCurve.AddKey(1.0f, 0f);
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(1, ourCurve);

        var main = ps.main;
        main.duration = smokeDuration / 3f;

        var LvelOverLt = ps.limitVelocityOverLifetime;
        LvelOverLt.limit = rb.velocity.magnitude;
        LvelOverLt.dampen = .2f;
        ps.Play();
        smokeDuration = 0f;
    }

    public void SlipSmoke(float slipPercentage)
    {
        var main = slipSmokeParticleSystem.main;
        main.stopAction = ParticleSystemStopAction.None;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

        var emission = slipSmokeParticleSystem.emission;
        emission.rateOverDistance = (1f - slipPercentage) * 30f;

        var LvelOverLt = slipSmokeParticleSystem.limitVelocityOverLifetime;
        LvelOverLt.limit = rb.velocity.magnitude;
        LvelOverLt.dampen = .2f;
        slipSmokeParticleSystem.Play();
    }

    public void DamageSuspension(Vector3 pointVelocity, Vector3 impactDirection, Vector3 contactNormal)
    {
        rb.AddForceAtPosition(impactDirection * (pointVelocity.magnitude/10f), this.transform.position);
    }

    public void OnReset()
    {

    }
}


