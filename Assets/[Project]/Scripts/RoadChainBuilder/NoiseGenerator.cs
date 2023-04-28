using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

    private float Get2DNoise(int positionalIndex, Noise n)
    {
        float noise = (-.5f + Mathf.PerlinNoise(channelIndex * (1f - n.frequency), positionalIndex * (1f - n.frequency))) * n.amplytude;
        return noise;
    }

    private Vector2 Get2DNoise(int positionalIndex, Noise n, Vector2Int coordinate)
    {
        float noise = (-.5f + Mathf.PerlinNoise(channelIndex * (1f - n.frequency) + coordinate.x, positionalIndex * (1f - n.frequency) + coordinate.y)) * n.amplytude;
        return new Vector2(noise, 0f);
    }

    public Vector3 GetNoise(int positionalIndex, NoiseChannel noiseChannelSettings)
    {
        float n = 0;
        Vector3 noiseResult = new Vector3();
        foreach (Noise noise in noiseChannelSettings.noises)
        {
            n = Get2DNoise(positionalIndex, noise);
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

    public Vector2 GetCoordinateNoise(NoiseChannel noiseChannelSettings, Vector2Int coordinate)
    {
        Vector2 noiseResult = new Vector3();
        foreach (Noise noise in noiseChannelSettings.noises)
        {
            if(noise.frequency == 0) return new Vector2(0, 0);
            float xCoordinate = coordinate.x / noise.frequency;
            float yCoordinate = coordinate.y / noise.frequency;

            float n = 0;
            for (int i = 0; i < noise.octaves; i++)
            {
                float x = Mathf.PerlinNoise(xCoordinate, yCoordinate) * 2 - 1;
                n += x * noise.amplytude;
            }


            n += Mathf.PerlinNoise(yCoordinate, xCoordinate) * noise.amplytude;
            switch (noise.noiseDirection)
            {
                case NoiseDirection.Horizontal:
                    noiseResult += new Vector2(n, 0f);
                    break;
                case NoiseDirection.Vertical:
                    noiseResult += new Vector2(0, n);
                    break;
                default:
                    noiseResult += Vector2.zero;
                    break;
            }
        }

        return noiseResult;
    }
}

[System.Serializable]
public class NoiseChannel
{
	public int channel;
	public List<Noise> noises = new List<Noise>();
	public NoiseGenerator generatorInstance
	{
		get
		{
			if (GeneratorInstance == null) { return CreateNoiseGenerator(channel); }
			else { return GeneratorInstance; }
		}
	}

	private NoiseGenerator GeneratorInstance;

	public NoiseGenerator CreateNoiseGenerator(int groupIndex)
	{
		if (GeneratorInstance == null)
			GeneratorInstance = new NoiseGenerator(noises, groupIndex);
		return GeneratorInstance;
	}
}

[System.Serializable]
public class Noise
{
	[SerializeField] private NoiseType noiseType;
	public NoiseDirection noiseDirection;

	[Range(0f, 10f)]
	public float frequency;
    [Range(.001f, 10f)]
    public float scale;
    [Range(.001f, 1f)]
	public float amplytude;
    [Range(1, 5)]
    public int octaves;
	public bool noiseMask;

    private enum NoiseType
	{
		None,
		Short_Noise,
		Medium_Noise,
		Long_Noise,
	}
}

public enum NoiseDirection
{
	None,
	Horizontal,
	Vertical,
}