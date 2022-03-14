using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NoiseGenerator
{
    private float shortNoise;
    private float sPower;

    private float mediumNoise;
    private float mPower;

    private float longNoise;
    private float lPower;

    private int seed;


    public NoiseGenerator(float s,float sp, float m, float mp, float l, float lp, int seed)
    {
        shortNoise = s;
        sPower = sp;
        mediumNoise = m;
        mPower = mp;
        longNoise = l;
        lPower = lp;
        this.seed = seed;
    }

    public float getShortNoise(int groupIndex, int positionalIndex)
    {
        float noise = (-.5f + Mathf.PerlinNoise(groupIndex * shortNoise, positionalIndex * shortNoise)) * sPower;
        return noise;
    }

    public float getMediumNoise(int groupIndex, int positionalIndex)
    {
        float noise = (-.5f + Mathf.PerlinNoise(groupIndex * mediumNoise, positionalIndex * mediumNoise)) * mPower;
        return noise;
    }

    public float getLongNoise(int groupIndex, int positionalIndex)
    {
        float noise = (-.5f + Mathf.PerlinNoise(groupIndex * longNoise, positionalIndex * longNoise)) * lPower;
        return noise;
    }

    public Vector3 getNoise(int positionalIndex, NoiseChannelSettings noiseChannelSettings)
    {
        int groupIndex = noiseChannelSettings.group;
        float n = 0;
        switch (noiseChannelSettings.noiseType)
        {
            case NoiseType.short_Noise:
                n = getShortNoise(groupIndex, positionalIndex);
                break;
            case NoiseType.medium_Noise:
                n = getMediumNoise(groupIndex, positionalIndex);
                break;
            case NoiseType.long_Noise:
                n = getLongNoise(groupIndex, positionalIndex);
                break;
            case NoiseType.shortMedium_Noise:
                n = getShortNoise(groupIndex, positionalIndex) + getMediumNoise(groupIndex, positionalIndex);
                break;
            case NoiseType.mediumLong_Noise:
                n = getMediumNoise(groupIndex, positionalIndex) + getLongNoise(groupIndex, positionalIndex);
                break;
            case NoiseType.shortLong_Noise:
                n = getShortNoise(groupIndex, positionalIndex) + getLongNoise(groupIndex, positionalIndex);
                break;
            case NoiseType.allTypes:
                n = getShortNoise(groupIndex, positionalIndex) + getMediumNoise(groupIndex, positionalIndex) + getLongNoise(groupIndex, positionalIndex);
                break;
            default:
                n = 0f;
                break;
        }

        switch (noiseChannelSettings.noiseDirection)
        {
            case NoiseDirection.Horizontal:
                return new Vector3(n,0f,0f);
            case NoiseDirection.Vertical:
                return new Vector3(0, n, 0f);
            case NoiseDirection.Biderectional:
                return new Vector3(n, n, 0f);
            default:
                return Vector3.zero;
        }
    }
}