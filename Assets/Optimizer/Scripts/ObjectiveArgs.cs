using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ObjectiveArgs
{
    public int optSeqOrder;
    public List<float> values = new List<float>();
    float lowerBound = 0.0f;
    float upperBound = 0.0f;
    bool smallerIsBetter = false;
    public bool popUp = false;
    public string question;

    public ObjectiveArgs() { }
    public ObjectiveArgs(float lowerBound, float upperBound)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
    }
    public ObjectiveArgs(float lowerBound, float upperBound, bool smallerIsBetter)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
        this.smallerIsBetter = smallerIsBetter;
    }
    public ObjectiveArgs(float lowerBound, float upperBound, bool smallerIsBetter, bool popUp, string question)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
        this.smallerIsBetter = smallerIsBetter;
        this.popUp = popUp;
        this.question = question;
    }
    public void addTrial(float value)
    {
        values.Add(value);
    }
    public void clearTrial()
    {
        values.Clear();
    }
    public int GetTrialNum()
    {
        return values.Count;
    }
    public float GetAvargeObjective()
    {
        return (float)(values.Count > 0 ? values.Average() : 0.0);
    }

    public float RemoveAndGetPracticedAvargeObjective(int ridOfNumber)
    {
        if(values.Count > ridOfNumber){
            values.RemoveRange(0, ridOfNumber);
        }
        return (float)(values.Count > 0 ? values.Average() : 0.0);
    }
    public string GetInitInfoStr()
    {
        return string.Format("{0},{1},{2}/", lowerBound, upperBound, smallerIsBetter ? 1 : 0);
    }

}
