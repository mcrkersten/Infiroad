using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NoiseGenerator
{
    private List<Noise> noises = new List<Noise>();
    private int channelIndex;


    public NoiseGenerator(List<Noise> n, int groupIndex)
    {
        noises = n;
        this.channelIndex = groupIndex;
    }

    public float getNoise(int positionalIndex, Noise n)
    {
        float noise = (-.5f + Mathf.PerlinNoise(channelIndex * (1f - n.noiseLenght), positionalIndex * (1f - n.noiseLenght)) * n.noisePower;
        return noise;
    }

    public Vector3 getNoise(int positionalIndex, NoiseChannel noiseChannelSettings)
    {
        float n = 0;
        Vector3 noiseResult = new Vector3();
        foreach (Noise noise in noiseChannelSettings.noises)
        {
            n = getNoise(positionalIndex, noise);
            switch (noise.noiseDirection)
            {
                case NoiseDirection.Horizontal:
                    noiseResult += new Vector3(n, 0f, 0f);
                    break;
                case NoiseDirection.Vertical:
                    noiseResult += new Vector3(0, n, 0f);
                    break;
                default:
                    noiseResult += Vector3.zero;
                    break;
            }
        }

        return noiseResult;
    }
}