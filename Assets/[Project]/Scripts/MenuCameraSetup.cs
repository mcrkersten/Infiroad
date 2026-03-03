using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MenuCameraSetup : MonoBehaviour
{
    void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.aspect = 1f; // 👈 Match the 1:1 square ratio of 1920x1920
    }
}
