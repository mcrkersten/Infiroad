using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Wheel_Raycast : MonoBehaviour
{
    public bool broken;
    [SerializeField] private GameObject wheelModel;
    [SerializeField] private Transform suspension;
    public Collider wheelCollider;
    [SerializeField] private List<SuspensionPointer> suspensionPointers = new List<SuspensionPointer>();

    [Header("General Slip")]
    public SurfaceScriptable currentSurface;


    [SerializeField] private AnimationCurve brakeLock;
    [SerializeField] private AnimationCurve wheelSpin;

    private Vector3 wheelModelLocalStartPosition;
    private Vector3 suspensionLocalStartPosition;
    private Vector3 lastHitPosition = Vector3.zero;

    [HideInInspector] public Vector3 wheelVelocityLocalSpace;
    [HideInInspector] public float steeringWheelForce;
    public float RPM;

    [Header("Wheel")]
    public float wheelRadius;
    private float tireCircumference;
    public float steerAngle;

    public GameObject smokeParticleSystemPrefab;
    [SerializeField] private ParticleSystem slipSmokeParticleSystem;

    [HideInInspector] public Vector3 forceDirectionDebug = Vector3.zero;
    public float gripDebug = 0.01f;
    private Rigidbody rb;

    private void Start()
    {
        rb = this.transform.root.GetComponent<Rigidbody>();
        tireCircumference = (Mathf.PI * 2f) * wheelRadius;
        wheelModelLocalStartPosition = new Vector3(wheelModel.transform.localPosition.x, 0, wheelModel.transform.localPosition.z);
        suspensionLocalStartPosition = new Vector3(suspension.transform.localPosition.x, 0, suspension.transform.localPosition.z);
    }

    private void Update()
    {
        transform.localEulerAngles = new Vector3(0, steerAngle, 0);

        foreach (SuspensionPointer s in suspensionPointers)
        {
            if(!broken)
                s.UpdatePointer();
        }
    }

    public bool Raycast(float maxLenght, LayerMask layerMask, out RaycastHit hit)
    {
        if (broken) {
            hit = new RaycastHit();
            return false;
        }
        if (Physics.SphereCast(transform.position, wheelRadius, -transform.root.up, out RaycastHit hitPoint, maxLenght, layerMask))
        {
            wheelModel.transform.localPosition = wheelModelLocalStartPosition + (-Vector3.up * (hitPoint.distance));
            suspension.transform.localPosition = suspensionLocalStartPosition + (-Vector3.up * (hitPoint.distance));
            lastHitPosition = hitPoint.point;
            hit = hitPoint;
            //Material m = GetMaterialFromRaycastHit(hit, hit.transform.GetComponent<Mesh>());
            //currentSurface = hit.transform.GetComponent<RoadSegment>()?.surfaceSettings.First(s => s.material == m);
            slipSmokeParticleSystem.transform.position = hit.point;
            return true;
        }
        slipSmokeParticleSystem.transform.position = this.transform.position;
        wheelModel.transform.localPosition = wheelModelLocalStartPosition + (-Vector3.up * maxLenght);
        suspension.transform.localPosition = suspensionLocalStartPosition + (-Vector3.up * maxLenght);
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

    public Vector2 RotateWheelModel(Vector2 force, SuspensionPosition suspensionPosition)
    {
        float wheelLockPercentage = brakeLock.Evaluate(Mathf.Abs(force.x));
        float wheelSpinPercentage = wheelSpin.Evaluate(Mathf.Abs(force.y));
        float combinedPercentage = wheelLockPercentage;// + wheelSpinPercentage;

        float rotationSpeed = (wheelVelocityLocalSpace.z * 3.6f) / tireCircumference;
        if (suspensionPosition == SuspensionPosition.FrontRight || suspensionPosition == SuspensionPosition.RearRight)
            wheelModel.transform.Rotate(Vector3.right, rotationSpeed * combinedPercentage);
        else
            wheelModel.transform.Rotate(Vector3.left, rotationSpeed * combinedPercentage);

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
        GameObject s = Instantiate(smokeParticleSystemPrefab, position, Quaternion.identity, wheelModel.transform);
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


        //if (!broken)
        //{
        //    rb.AddForceAtPosition(-impactDirection, this.transform.position);
        //    broken = true;
        //    this.gameObject.AddComponent<Rigidbody>();
        //    this.wheelCollider.gameObject.layer = LayerMask.NameToLayer("Default");
        //    this.transform.parent = null;
        //}
    }
}


