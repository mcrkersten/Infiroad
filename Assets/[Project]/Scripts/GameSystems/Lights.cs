using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystems
{
    public class Lights : MonoBehaviour
    {
        public Material greenLightMaterial;
        public List<GameObject> lights = new List<GameObject>();

        private void Awake()
        {
            GameManager.onStartGame += GreenLights;
        }

        private void GreenLights()
        {
            foreach (GameObject g in lights)
                g.GetComponent<MeshRenderer>().material = greenLightMaterial;
        }

        private void OnDestroy()
        {
            GameManager.onStartGame -= GreenLights;
        }
    }
}