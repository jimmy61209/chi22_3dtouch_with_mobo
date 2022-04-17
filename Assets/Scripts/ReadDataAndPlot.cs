using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts;
using System;
using System.Linq;
using UnityEngine.UI;
using System.Text;
using UnityEditor;
using System.IO;
using Env3DTouch;
public class ReadDataAndPlot : MonoBehaviour
{
    List<Dictionary<string, object>> csvData;
    // List<Dictionary<string, object>> paretoData;
    List<float[]> sortedParetoData = new List<float[]>();
    ScatterChart scatterChart;
    LineChart lineChart;
    Slider slider;
    public EnvManagerParameter envManagerParameter;
    List<int> finaldesign = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        slider = transform.parent.Find("Slider").GetComponent<Slider>();
        lineChart = transform.parent.Find("LineChart").GetComponent<LineChart>();
        csvData = CSVReader.Read("UserObservations");
        // paretoData = CSVReader.Read("ParetoValues");
        scatterChart = transform.GetComponent<ScatterChart>();

        for (int i = 0; i < csvData.Count; i++)
        {
            scatterChart.AddData(0, Convert.ToSingle(csvData[i]["CompletionTime"]), Convert.ToSingle(csvData[i]["SpatialError"]), (scatterChart.series.list[0].data.Count + 1).ToString());
            scatterChart.AddData(1, Convert.ToSingle(csvData[i]["CompletionTime"]), Convert.ToSingle(csvData[i]["SpatialError"]), (scatterChart.series.list[1].data.Count + 1).ToString());
            if (csvData[i]["IsPareto"].ToString() == "TRUE")
            {
                sortedParetoData.Add(new float[] { Convert.ToSingle(csvData[i]["CompletionTime"]), Convert.ToSingle(csvData[i]["SpatialError"]), i });
            }
        }
        // for (int i = 0; i < paretoData.Count; i++)
        // {
        //     sortedParetoData.Add(new float[] { Convert.ToSingle(paretoData[i]["CompletionTime"]), Convert.ToSingle(paretoData[i]["SpatialError"]), Convert.ToSingle(paretoData[i]["Index"]) });
        // }
        sortedParetoData = sortedParetoData.OrderBy(x => x[0]).ToList();

        // sortedParetoData.Sort();
        for (int i = 0; i < sortedParetoData.Count; i++)
        {
            scatterChart.AddData(2, sortedParetoData[i][0], sortedParetoData[i][1]);
        }

        for (int i = 0; i < sortedParetoData.Count; i++)
        {
            int index = (int)sortedParetoData[i][2];
            lineChart.AddSerie(SerieType.Line, lineChart.series.Count.ToString());
            lineChart.AddData(lineChart.series.Count - 1, Convert.ToSingle(csvData[index]["D"]) * 100);
            lineChart.AddData(lineChart.series.Count - 1, Convert.ToSingle(csvData[index]["K"]) * 100);
            lineChart.AddData(lineChart.series.Count - 1, Convert.ToSingle(csvData[index]["Amplitude"]) * 100);
            lineChart.AddData(lineChart.series.Count - 1, Convert.ToSingle(csvData[index]["Gap"]) * 100);
        }

        slider.maxValue = sortedParetoData.Count - 1;
        HighLightParetoPoint(slider.value);
        // scatterChart.RefreshChart();
        // scatterChart.AnimationReset();

    }

    public void SelectTestPoint()
    {
        int index = (int)sortedParetoData[(int)slider.value][2];

        if (scatterChart.series.list[1].data[index].enableItemStyle == true)
        {
            scatterChart.series.list[1].data[index].enableItemStyle = false;
            finaldesign.Remove((int)slider.value);
        }
        else
        {
            scatterChart.series.list[1].data[index].enableItemStyle = true;
            scatterChart.series.list[1].data[index].itemStyle.color = Color.red;
            finaldesign.Add((int)slider.value);
        }

        scatterChart.series.list[0].data[index].symbol.size = 5;
        scatterChart.series.list[0].data[index].symbol.selectedSize = 8;
        scatterChart.RefreshChart();
    }
    public void EvaluateFinalDesign()
    {
        if (finaldesign.Count != 3)
        {
            print("user need to select 3 designs");
        }
        else
        {

            ParameterSliderController.startIteration = true;
            EqualSpawnPoint.ResetSpawnOrder();

            foreach (int i in finaldesign)
            {
                print(i);
            }
        }
    }

    public void HighLightParetoPoint(float value)
    {
        // print(Convert.ToInt32(paretoData[(int)value]["Index"]));
        for (int i = 0; i < sortedParetoData.Count; i++)
        {
            scatterChart.series.list[0].data[(int)sortedParetoData[i][2]].enableSymbol = false;
            print((int)sortedParetoData[i][2]);
        }

        int index = (int)sortedParetoData[(int)value][2];
        // scatterChart.series.list[0].data[index].enableItemStyle = true;
        // scatterChart.series.list[0].data[index].itemStyle.color = Color.red;

        Serie emptyCircleSerie = scatterChart.series.list[0];
        emptyCircleSerie.data[index].enableSymbol = true;
        emptyCircleSerie.data[index].symbol.type = SerieSymbolType.EmptyCircle;
        emptyCircleSerie.data[index].symbol.size = 5;
        emptyCircleSerie.data[index].symbol.selectedSize = 5;
        scatterChart.RefreshChart();

        EnvManagerParameter.x[0] = Convert.ToSingle(csvData[index]["D"]) * 100;
        EnvManagerParameter.x[1] = Convert.ToSingle(csvData[index]["K"]) * 100;
        EnvManagerParameter.x[2] = Convert.ToSingle(csvData[index]["Amplitude"]) * 100;
        EnvManagerParameter.x[3] = Convert.ToSingle(csvData[index]["Gap"]) * 100;
        print(EnvManagerParameter.x[0]);

        for (int i = 0; i < sortedParetoData.Count; i++)
        {
            if (value == i)
            {
                HighLightSerieLine(i, true);
            }
            else
            {
                HighLightSerieLine(i, false);
            }
        }

        envManagerParameter.UpdateDesignParameters();
    }

    public void HighLightSerieLine(int index, bool sethighlight)
    {
        if (index == scatterChart.clickedIndex)
        {
            return;
        }
        // print(serieName);
        int lineIndex = -1;
        for (int i = 0; i < lineChart.series.Count; i++)
        {
            if (lineChart.series.list[i].name == index.ToString())
            {
                lineIndex = i;
                break;
            }
        }

        Serie currentSerie = lineChart.series.list[lineIndex];
        //move serie to the top of the series
        if (lineIndex != lineChart.series.Count - 1 && sethighlight)
        {
            lineChart.series.Remove(currentSerie.name);
            lineChart.AddSerie(SerieType.Line, currentSerie.name);
            lineChart.AddData(lineChart.series.Count - 1, currentSerie.data[0].data[1]);
            lineChart.AddData(lineChart.series.Count - 1, currentSerie.data[1].data[1]);
            lineChart.AddData(lineChart.series.Count - 1, currentSerie.data[2].data[1]);
            lineChart.AddData(lineChart.series.Count - 1, currentSerie.data[3].data[1]);
            currentSerie = lineChart.series.list[lineChart.series.Count - 1];
            lineChart.series.list[lineChart.series.Count - 1].animation.enable = false;
            Color32 color = lineChart.theme.colorPalette[lineIndex];
            lineChart.theme.colorPalette.RemoveAt(lineIndex);
            lineChart.theme.colorPalette.Insert((lineChart.series.Count - 1) % 20, color);
        }


        Serie emptyCircleSerie = scatterChart.series.list[0];
        Serie scatterSerie = scatterChart.series.list[1];
        if (sethighlight == true)
        {
            currentSerie.lineStyle.width = 2;
            currentSerie.lineStyle.color = Color.red;
            currentSerie.itemStyle.color = Color.red;
            // emptyCircleSerie.data[index].data[0] = scatterSerie.data[index].data[0];
            // emptyCircleSerie.data[index].data[1] = scatterSerie.data[index].data[1];
            // emptyCircleSerie.data[index].enableSymbol = true;
            // emptyCircleSerie.data[index].symbol.type = SerieSymbolType.EmptyCircle;
            // emptyCircleSerie.data[index].symbol.size = 5;
            // emptyCircleSerie.data[index].symbol.selectedSize = 5;
            string content = $"D: {currentSerie.data[0].data[1]}\n";
            content += $"K: {currentSerie.data[1].data[1]}\n";
            content += $"Amplitude: {currentSerie.data[2].data[1]}\n";
            content += $"Gap: {currentSerie.data[3].data[1]}";
            TooltipHelper.SetContentAndPosition(lineChart.tooltip, content, lineChart.chartRect);

            lineChart.tooltip.SetActive(true);
        }
        else
        {
            currentSerie.lineStyle.color = new Color32(0, 0, 0, 0);
            currentSerie.itemStyle.color = new Color32(0, 0, 0, 0);
            currentSerie.lineStyle.width = 0;
            // emptyCircleSerie.data[index].enableSymbol = false;
            // emptyCircleSerie.data[0].data[0] = -2;
            // emptyCircleSerie.data[0].data[1] = -2;
            // lineChart.tooltip.SetActive(false);
        }
        // print(currentSerie.lineStyle.width);
    }

    public class CSVdata
    {
        public float CompletionTime;
        public float SpatialError;

        public float D;
        public float K;
        public float Amplitude;
        public float Gap;

        public CSVdata() { }

        public CSVdata(float completionTime, float spatialError, float d, float k, float amplitude, float gap)
        {
            CompletionTime = completionTime;
            SpatialError = spatialError;

            D = (int)d;
            K = (int)k;
            Amplitude = (int)amplitude;
            Gap = (int)gap;
        }
    }

    public void SaveToCSV()
    {
        List<CSVdata> data = new List<CSVdata>();

        // int index = (int)sortedParetoData[(int)slider.value][2];

        for (int i = 0; i < finaldesign.Count; i++)
        {
            int index = (int)sortedParetoData[i][2];
            data.Add(new CSVdata(sortedParetoData[i][0], sortedParetoData[i][1],
            Convert.ToSingle(csvData[index]["D"]) * 100, Convert.ToSingle(csvData[index]["K"]) * 100, Convert.ToSingle(csvData[index]["Amplitude"]) * 100, Convert.ToSingle(csvData[index]["Gap"]) * 100));
        }


        var sb = new StringBuilder("CompletionTime,SpatialError,D,K,Amplitude,Gap");
        foreach (var frame in data)
        {
            sb.Append('\n').Append(frame.CompletionTime.ToString() + ',').Append(frame.SpatialError.ToString() + ',')
            .Append(frame.D.ToString() + ',').Append(frame.K.ToString() + ',').Append(frame.Amplitude.ToString() + ',').Append(frame.Gap.ToString());
        }

        string path = "Assets/Resources/final_design_user_select.csv";

        using (var writer = new StreamWriter(path, false))
        {
            writer.Write(sb.ToString());
        }
        path = $"Assets/Resources/MOBOLed/{System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}ThreeSelectedDesigns.csv";

        using (var writer = new StreamWriter(path, false))
        {
            writer.Write(sb.ToString());
        }
        Debug.Log($"CSV file written to \"{path}\"");
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EvaluateFinalDesign();
        }
    }

    private void OnDestroy()
    {
        SaveToCSV();
    }
}
