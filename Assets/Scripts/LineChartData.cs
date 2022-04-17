using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;


public class LineChartData
{
    public LineChartData(List<int> parameterPoint, int padding, bool isCustomRegion = false)
    {
        this.isCustomRegion = isCustomRegion;
        for (int i = 0; i < parameterPoint.Count; i++)
        {
            if (isCustomRegion)
            {
                rootValue[i] = -1;
            }
            else
            {
                rootValue[i] = parameterPoint[i];
            }
            upperBound[i] = (parameterPoint[i] + padding > 100) ? 100 : parameterPoint[i] + padding; // upperBound
            lowerBound[i] = (parameterPoint[i] - padding < 0) ? 0 : parameterPoint[i] - padding; // lowerBound
        }
    }
    public LineChartData(LineChartData lineChartData){
        upperBound = new List<int>(lineChartData.upperBound).ToArray();
        lowerBound = new List<int>(lineChartData.lowerBound).ToArray();
        rootValue = new List<int>(lineChartData.rootValue).ToArray();
        beta = lineChartData.beta;
        sprite = lineChartData.sprite;
        isCustomRegion = lineChartData.isCustomRegion;
        isDeleted = lineChartData.isDeleted;
    }
    public int[] upperBound = new int[4];
    public int[] lowerBound = new int[4];
    public int[] rootValue = new int[4];
    public float beta = 1;
    public Sprite sprite;
    public bool isCustomRegion = false;
    public bool isDeleted = false;
}
