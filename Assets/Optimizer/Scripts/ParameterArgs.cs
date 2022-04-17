using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterArgs
{
    public int optSeqOrder;
    public bool isDiscrete = false;
    public float lowerBound = 0.0f;
    public float upperBound = 0.0f;
    public float step = 1.0f;
    public float Value
    {
        get;
        set;
    } = 0.0f;
    float reference;

    public ParameterArgs()
    {

    }
    public ParameterArgs(float lowerBound, float upperBound)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;

    }
    public ParameterArgs(float lowerBound, float upperBound, float step)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
        this.step = step;
        isDiscrete = true;

    }
    public ParameterArgs(float lowerBound, float upperBound, float step, ref float reference)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
        this.step = step;
        isDiscrete = true;
        this.reference = reference;

    }
    public string GetInitInfoStr()
    {
        return string.Format("{0},{1},{2}/", lowerBound, upperBound, step);
    }
}
