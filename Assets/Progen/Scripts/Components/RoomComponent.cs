using UnityEngine;

public class RoomComponent : MonoBehaviour 
{
    [SerializeField]
    public float rayDistance = 0;

    [SerializeField]
    public Vector3 rayPosition;

    [SerializeField]
    public Vector3 offset;

    [SerializeField]
    private bool hitGenerated = false;

    [SerializeField]
    private bool showRay = true;

    void Update()
    {
        if(showRay)
        {
            Debug.DrawRay(transform.localPosition - offset + rayPosition, transform.TransformDirection(Vector3.up) * rayDistance, Color.yellow);
        }

        if(rayDistance > 0 && !hitGenerated)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.localPosition - offset + rayPosition, transform.TransformDirection(Vector3.up), out hit, rayDistance))
            {
                hitGenerated = true;
            }
        }
    }
}