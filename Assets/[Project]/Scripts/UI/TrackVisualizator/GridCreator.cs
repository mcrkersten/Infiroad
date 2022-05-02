using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCreator : MonoBehaviour
{
    [SerializeField] private int size;
    [SerializeField] private int pointPerSide;
    [SerializeField] private GameObject pointPrefab;

    private void Start()
    {
        CreateCorners();
    }

    private void CreateCorners()
    {
        GameObject p1 = Instantiate(pointPrefab);
        p1.transform.parent = this.transform;
        p1.transform.localPosition = new Vector3(0, 0, 0);

        GameObject p2 = Instantiate(pointPrefab);
        p2.transform.parent = this.transform;
        p2.transform.localPosition = new Vector3(0, 0, size);

        GameObject p3 = Instantiate(pointPrefab);
        p3.transform.parent = this.transform;
        p3.transform.localPosition = new Vector3(size, 0, size);

        GameObject p4 = Instantiate(pointPrefab);
        p4.transform.parent = this.transform;
        p4.transform.localPosition = new Vector3(size, 0, 0);
    }
}
