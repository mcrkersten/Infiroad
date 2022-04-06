using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace GameSystems
{
    public class DistanceClock : MonoBehaviour
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private TextMeshProUGUI numbersText;

        [SerializeField] private bool useColours;
        [SerializeField] private Color positiveColor;
        [SerializeField] private Color negativeColor;

        public void Start()
        {
            Deactivate();
        }

        public void Activate()
        {
            numbersText.gameObject.SetActive(true);
        }

        public void UpdateDistance(int distance)
        {
            numbersText.text = System.String.Format("{0:n0}", distance);
            if (useColours)
                if (distance < 0)
                    numbersText.color = negativeColor;
                else
                    numbersText.color = positiveColor;
        }

        public void Deactivate()
        {
            numbersText.gameObject.SetActive(false);
        }

    }
}