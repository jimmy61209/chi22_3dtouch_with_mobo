using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts;
using System.Text;
using System.IO;
using UnityEditor;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Linq;

public class ScatterController : MonoBehaviour
{
    // Start is called before the first frame update
    private ScatterChart scatterChart;
    private LineChart lineChart;

    public Slider sliderD;
    public Slider sliderK;
    public Slider sliderAmplitude;
    public Slider sliderGap;
    public Button loadParameterBtn;
    Vector2 tooltipPosition;
    Color32 designerColor = new Color32(14, 27, 100, 255);
    Color32 moboColor = new Color32(39, 159, 55, 255);

    // for hybrid, design from mobo or designer
    // -1: default
    // 0: designer
    // 1: mobo
    List<int> linetype = new List<int>();

    public bool isHybrid = false;
    public bool isMOBO = false;

    void Start()
    {
        scatterChart = gameObject.GetComponent<ScatterChart>();
        lineChart = transform.parent.Find("LineChart").GetComponent<LineChart>();
        if (scatterChart == null) return;
        // for(int i = 0; i < 1; i++){
        //     AddScatterData();
        // }
        scatterChart.action = HighLightSerieLine;
        scatterChart.onPointerClick = OnPointerClick;

    }
    void OnPointerClick(PointerEventData eventData, BaseGraph chart)
    {
        bool changed = false;
        var serie = scatterChart.series.list[1];
        var dataCount = serie.data.Count;
        for (int j = 0; j < serie.data.Count; j++)
        {
            var serieData = serie.data[j];
            if (serieData.selected == true)
            {
                scatterChart.clickedIndex = j;
                serieData.symbol.size = 5;
                serieData.symbol.selectedSize = 5;
                serieData.symbol.type = SerieSymbolType.Circle;
                serieData.enableSymbol = true;
                changed = true;
            }
            else
            {
                if (serieData.name != "")
                {
                    serieData.enableSymbol = false;
                }
            }
        }
        if (!changed)
        {
            scatterChart.tooltip.alwayShow = false;
            lineChart.tooltip.alwayShow = false;
            scatterChart.clickedIndex = -1;
            if (loadParameterBtn != null)
                loadParameterBtn.interactable = false;
        }
        else
        {
            tooltipPosition = scatterChart.tooltip.GetContentPos();
            scatterChart.tooltip.alwayShow = true;
            lineChart.tooltip.alwayShow = true;
            LoadParameterToSlider();
            if (loadParameterBtn != null)
                loadParameterBtn.interactable = true;

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            int randomValue = Random.Range(0, 2);
            RefreshColorForHybrid(randomValue);
            AddObservationData(Random.Range(10, 100), Random.Range(10, 100), Random.Range(10, 100), Random.Range(10, 100), Random.Range(900, 1600f), Random.Range(0, 10f), randomValue);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddPreviousData();
        }
        if (scatterChart.clickedIndex != -1)
        {

            int lineIndex = -1;
            for (int i = 0; i < lineChart.series.Count; i++)
            {
                if (lineChart.series.list[i].name == scatterChart.clickedIndex.ToString())
                {
                    lineIndex = i;
                    break;
                }
            }
            Serie currentSerie = lineChart.series.list[lineIndex];
            //move serie to the top of the series
            if (lineIndex != lineChart.series.Count - 1)
            {
                lineChart.series.Remove(currentSerie.name);
                lineChart.AddSerie(SerieType.Line, currentSerie.name);
                lineChart.AddData(lineChart.series.Count - 1, currentSerie.data[0].data[1]);
                lineChart.AddData(lineChart.series.Count - 1, currentSerie.data[1].data[1]);
                lineChart.AddData(lineChart.series.Count - 1, currentSerie.data[2].data[1]);
                lineChart.AddData(lineChart.series.Count - 1, currentSerie.data[3].data[1]);
                currentSerie = lineChart.series.list[lineChart.series.Count - 1];
            }

            // Serie currentSerie = lineChart.series.list[scatterChart.clickedIndex];
            string content = $"D: {currentSerie.data[0].data[1]}\n";
            content += $"K: {currentSerie.data[1].data[1]}\n";
            content += $"Amplitude: {currentSerie.data[2].data[1]}\n";
            content += $"Gap: {currentSerie.data[3].data[1]}";
            TooltipHelper.SetContentAndPosition(lineChart.tooltip, content, lineChart.chartRect);
            lineChart.tooltip.UpdateContentPos(new Vector2(0, 0));

            var serieData = scatterChart.series.list[1].data[scatterChart.clickedIndex];
            content = $"Iter.: {serieData.name}\n";
            content += $"Time: {serieData.data[0].ToString("f2")}\n";
            content += $"Error: {serieData.data[1].ToString("f2")}";
            scatterChart.tooltip.UpdateContentText(content);
            scatterChart.tooltip.UpdateContentPos(tooltipPosition);

        }

    }

    public void AddRandomData()
    {
        scatterChart.AddData(0, Random.Range(1000, 2000), Random.Range(0, 50), (scatterChart.series.list[1].data.Count + 1).ToString());
        lineChart.AddSerie(SerieType.Line, lineChart.series.Count.ToString());
        lineChart.AddData(lineChart.series.Count - 1, Random.Range(10, 100));
        lineChart.AddData(lineChart.series.Count - 1, Random.Range(10, 100));
        lineChart.AddData(lineChart.series.Count - 1, Random.Range(10, 100));
        lineChart.AddData(lineChart.series.Count - 1, Random.Range(10, 100));
        linetype.Add(-1);
        scatterChart.yAxis0.minMaxType = Axis.AxisMinMaxType.MinMax;

        Serie currentSerie = scatterChart.series.list[1];
        float mmin = 5000;
        float mmax = 0;
        for (int i = 0; i < currentSerie.data.Count; i++)
        {
            if (currentSerie.data[i].data[0] > mmax)
            {
                mmax = currentSerie.data[i].data[0];
            }
            if (currentSerie.data[i].data[0] < mmin)
            {
                mmin = currentSerie.data[i].data[0];
            }
            currentSerie.data[i].enableItemStyle = false;
        }

        //mark new data as red
        currentSerie.data[scatterChart.series.list[1].data.Count - 1].enableItemStyle = true;
        currentSerie.data[scatterChart.series.list[1].data.Count - 1].itemStyle.color = Color.red;

        scatterChart.xAxis0.min = ((int)Math.Floor(mmin / 100.0)) * 100;
        scatterChart.xAxis0.max = ((int)Math.Ceiling(mmax / 100.0)) * 100;
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public void AddObservationData(int D, int K, int Amplitude, int Gap, float CompletionTime, float SpatialError, int isMOBO = -1)
    {
        // if trial fail, don't draw, 5 for not append y axis
        if (CompletionTime == 3000 && SpatialError == 30)
        {
            SerieData tmp = scatterChart.AddData(0, -1, -1, "");
            scatterChart.AddData(1, -1, -1, "");
            // tmp.enableLabel = false;
            // tmp.show = false;

            tmp.symbol.type = SerieSymbolType.None;
            tmp.enableSymbol = true;

        }
        else
        {
            float NormalizedCompletionTime = Remap(CompletionTime, 900, 1600, -1, 1) * -1;
            float NormalizedSpatialError = Remap(SpatialError, 0, 10, -1, 1) * -1;

            if (NormalizedCompletionTime < -1)
            {
                NormalizedCompletionTime = -1;
            }
            if (NormalizedSpatialError < -1)
            {
                NormalizedSpatialError = -1;
            }

            // print($"Time:{NormalizedCompletionTime}, Error:{NormalizedSpatialError}");

            scatterChart.AddData(1, NormalizedCompletionTime, NormalizedSpatialError, (scatterChart.series.list[1].data.Count + 1).ToString());
            scatterChart.AddData(0, NormalizedCompletionTime, NormalizedSpatialError, (scatterChart.series.list[0].data.Count + 1).ToString());
        }

        if (isMOBO != -1)
        {
            linetype.Add(isMOBO);
            if (isMOBO == 0)
            {
                if (lineChart.series.Count == 0)
                {
                    lineChart.theme.colorPalette[0] = new Color32(14, 27, 100, 255);
                }
                else
                {
                    lineChart.theme.colorPalette.Add(new Color32(14, 27, 100, 255));

                }
            }
            else if (isMOBO == 1)
            {
                if (lineChart.series.Count == 0)
                {
                    lineChart.theme.colorPalette[0] = new Color32(39, 159, 55, 255);
                }
                else
                {
                    lineChart.theme.colorPalette.Add(new Color32(39, 159, 55, 255));

                }
            }
        }

        lineChart.AddSerie(SerieType.Line, lineChart.series.Count.ToString());
        lineChart.AddData(lineChart.series.Count - 1, D);
        lineChart.AddData(lineChart.series.Count - 1, K);
        lineChart.AddData(lineChart.series.Count - 1, Amplitude);
        lineChart.AddData(lineChart.series.Count - 1, Gap);



        // scatterChart.yAxis0.minMaxType = Axis.AxisMinMaxType.MinMax;
        Serie currentSerie = scatterChart.series.list[1];
        Serie emptyCurrentSerie = scatterChart.series.list[0];
        // float mmin = 5000;
        // float mmax = 500;
        // for (int i = 0; i < currentSerie.data.Count; i++)
        // {
        //     if (currentSerie.data[i].data[0] > mmax)
        //     {
        //         mmax = currentSerie.data[i].data[0];
        //     }
        //     if (currentSerie.data[i].data[0] < mmin)
        //     {
        //         mmin = currentSerie.data[i].data[0];
        //     }
        //     currentSerie.data[i].enableItemStyle = false;
        // }
        for (int i = 0; i < currentSerie.data.Count; i++)
        {
            if (isMOBO == -1)
                currentSerie.data[i].enableItemStyle = false;
            emptyCurrentSerie.data[i].enableItemStyle = false;
            emptyCurrentSerie.data[i].enableSymbol = false;
            if (lineChart.series.list[i].lineStyle.color != Color.red)
            {
                lineChart.series.list[i].lineStyle.color = new Color32(0, 0, 0, 0);
                lineChart.series.list[i].itemStyle.color = new Color32(0, 0, 0, 0);
            }
        }

        if (isMOBO == 0)
        {
            currentSerie.data[scatterChart.series.list[1].data.Count - 1].enableItemStyle = true;
            currentSerie.data[scatterChart.series.list[1].data.Count - 1].itemStyle.color = new Color32(14, 27, 100, 255);

            Serie emptyCircleSerie = scatterChart.series.list[0];
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].enableItemStyle = true;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].enableSymbol = true;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].symbol.size = 5;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].symbol.selectedSize = 5;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].itemStyle.borderWidth = -3;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].itemStyle.color = Color.gray;

        }
        else if (isMOBO == 1)
        {
            currentSerie.data[scatterChart.series.list[1].data.Count - 1].enableItemStyle = true;
            currentSerie.data[scatterChart.series.list[1].data.Count - 1].itemStyle.color = new Color32(39, 159, 55, 255);

            Serie emptyCircleSerie = scatterChart.series.list[0];
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].enableItemStyle = true;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].enableSymbol = true;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].symbol.size = 5;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].symbol.selectedSize = 5;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].itemStyle.borderWidth = -3;
            emptyCircleSerie.data[scatterChart.series.list[1].data.Count - 1].itemStyle.color = Color.gray;

        }

        //comment after hybrid
        //mark new data as dark blue
        else
        {
            currentSerie.data[scatterChart.series.list[1].data.Count - 1].enableItemStyle = true;
            currentSerie.data[scatterChart.series.list[1].data.Count - 1].itemStyle.color = new Color32(21, 27, 85, 255);

            currentSerie = lineChart.series.list[lineChart.series.Count - 1];
            currentSerie.lineStyle.color = new Color32(21, 27, 85, 255);
            currentSerie.itemStyle.color = new Color32(21, 27, 85, 255);
        }








        // scatterChart.xAxis0.min = ((int)Math.Floor(mmin / 100.0)) * 100;
        // scatterChart.xAxis0.max = ((int)Math.Ceiling(mmax / 100.0)) * 100;
    }

    public void AddPreviousData()
    {
        List<Dictionary<string, object>> csvData;
        csvData = CSVReader.Read("LoadPreviousData");

        for (int i = 0; i < csvData.Count; i++)
        {
            AddObservationData(Convert.ToInt32(csvData[i]["D"]), Convert.ToInt32(csvData[i]["K"]), Convert.ToInt32(csvData[i]["Amplitude"]), Convert.ToInt32(csvData[i]["Gap"]),
            Convert.ToSingle(csvData[i]["CompletionTime"]), Convert.ToSingle(csvData[i]["SpatialError"]));
        }
    }

    public void LoadParameterToSlider()
    {
        Serie currentSerie = lineChart.series.list[lineChart.series.Count - 1];
        sliderD.value = currentSerie.data[0].data[1];
        sliderK.value = currentSerie.data[1].data[1];
        sliderAmplitude.value = currentSerie.data[2].data[1];
        sliderGap.value = currentSerie.data[3].data[1];
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

            D = d;
            K = k;
            Amplitude = amplitude;
            Gap = gap;
        }
    }

    public void SaveToCSVBtn()
    {
        List<CSVdata> data = new List<CSVdata>();

        Serie scatterSerie = scatterChart.series.list[1];
        for (int i = 0; i < scatterSerie.data.Count; i++)
        {
            Serie lineSerie = lineChart.series.list[i];
            data.Add(new CSVdata(scatterSerie.data[i].data[0], scatterSerie.data[i].data[1],
             lineSerie.data[0].data[1], lineSerie.data[1].data[1], lineSerie.data[2].data[1], lineSerie.data[3].data[1]));
        }
        var sb = new StringBuilder("CompletionTime,SpatialError,D,K,Amplitude,Gap");
        foreach (var frame in data)
        {
            sb.Append('\n').Append(frame.CompletionTime.ToString() + ',').Append(frame.SpatialError.ToString() + ',')
            .Append(frame.D.ToString() + ',').Append(frame.K.ToString() + ',').Append(frame.Amplitude.ToString() + ',').Append(frame.Gap.ToString());
        }

        string filePath;
        if (isHybrid == false)
        {
            string path = "Assets/Resources/DesignerLed/";
            filePath = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "ObservationsPerEvalution.csv");
        }
        else
        {
            string path = "Assets/Resources/Hybrid/";
            filePath = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "ObservationsPerEvalution.csv");

        }

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(sb.ToString());
        }
        Debug.Log($"CSV file written to \"{filePath}\"");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

    }



    public void HighLightSerieLine(int index, bool sethighlight, string serieName)
    {
        if (index == scatterChart.clickedIndex || serieName == "circle")
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
            lineChart.series.list[lineChart.series.Count - 1].animation.enable = false;
            currentSerie = lineChart.series.list[lineChart.series.Count - 1];
            if (lineChart.theme.colorPalette.Count <= lineIndex)
            {
                lineChart.theme.colorPalette.AddRange(new List<Color32>(lineChart.theme.colorPalette));
            }
            Color32 color = lineChart.theme.colorPalette[lineIndex];
            lineChart.theme.colorPalette.RemoveAt(lineIndex);
            lineChart.theme.colorPalette.Insert((lineChart.series.Count - 1), color);
        }


        Serie emptyCircleSerie = scatterChart.series.list[0];
        Serie scatterSerie = scatterChart.series.list[1];
        if (sethighlight == true)
        {
            currentSerie.lineStyle.width = 2;
            currentSerie.lineStyle.color = Color.red;
            currentSerie.itemStyle.color = Color.red;
            emptyCircleSerie.data[index].data[0] = scatterSerie.data[index].data[0];
            emptyCircleSerie.data[index].data[1] = scatterSerie.data[index].data[1];
            emptyCircleSerie.data[index].enableSymbol = true;
            emptyCircleSerie.data[index].enableItemStyle = false;
            emptyCircleSerie.data[index].symbol.type = SerieSymbolType.EmptyCircle;
            emptyCircleSerie.data[index].symbol.size = 5;
            emptyCircleSerie.data[index].symbol.selectedSize = 5;
            string content = $"D: {currentSerie.data[0].data[1]}\n";
            content += $"K: {currentSerie.data[1].data[1]}\n";
            content += $"Amplitude: {currentSerie.data[2].data[1]}\n";
            content += $"Gap: {currentSerie.data[3].data[1]}";
            TooltipHelper.SetContentAndPosition(lineChart.tooltip, content, lineChart.chartRect);
            lineChart.tooltip.UpdateContentPos(new Vector2(0, 0));

            lineChart.tooltip.SetActive(true);
        }
        else
        {
            currentSerie.lineStyle.color = new Color32(0, 0, 0, 0);
            currentSerie.itemStyle.color = new Color32(0, 0, 0, 0);
            // comment after hybrid
            // if (currentSerie.name == (lineChart.series.Count - 1).ToString())
            // {
            //     currentSerie.lineStyle.color = new Color32(21, 27, 85, 255);
            //     currentSerie.itemStyle.color = new Color32(21, 27, 85, 255);
            // }
            // else
            // {
            //     currentSerie.lineStyle.color = new Color32(0, 0, 0, 0);
            //     currentSerie.itemStyle.color = new Color32(0, 0, 0, 0);
            // }

            if (currentSerie.name == (lineChart.series.Count - 1).ToString())
            {
                emptyCircleSerie.data[index].enableItemStyle = true;
            }
            else
            {
                emptyCircleSerie.data[index].enableSymbol = false;
            }

            currentSerie.lineStyle.width = 0;
            // emptyCircleSerie.data[0].data[0] = -2;
            // emptyCircleSerie.data[0].data[1] = -2;
            // lineChart.tooltip.SetActive(false);
        }
        // print(currentSerie.lineStyle.width);
    }

    public void RefreshColorForHybrid(int getFromMOBOData = -1)
    {
        int designerTotal = 1;
        int moboTotal = 1;
        for (int i = 0; i < linetype.Count; i++)
        {
            if (linetype[i] == 0)
            {
                designerTotal++;
            }
            else if (linetype[i] == 1)
            {
                moboTotal++;
            }
        }

        int designerCount = 0;
        int moboCount = 0;
        for (int i = 0; i < lineChart.series.Count; i++)
        {
            if (getFromMOBOData == linetype[i])
            {
                byte newR = 0;
                byte newG = 0;
                byte newB = 0;
                if (getFromMOBOData == 0)
                {
                    designerCount++;
                    newR = Convert.ToByte(designerColor.r + (float)(200 - designerColor.r) * (float)(designerTotal - designerCount) / designerTotal);
                    newG = Convert.ToByte(designerColor.g + (float)(200 - designerColor.g) * (float)(designerTotal - designerCount) / designerTotal);
                    newB = Convert.ToByte(designerColor.b + (float)(200 - designerColor.b) * (float)(designerTotal - designerCount) / designerTotal);
                }
                if (getFromMOBOData == 1)
                {
                    moboCount++;
                    newR = Convert.ToByte(moboColor.r + (float)(200 - moboColor.r) * (float)(moboTotal - moboCount) / moboTotal);
                    newG = Convert.ToByte(moboColor.g + (float)(200 - moboColor.g) * (float)(moboTotal - moboCount) / moboTotal);
                    newB = Convert.ToByte(moboColor.b + (float)(200 - moboColor.b) * (float)(moboTotal - moboCount) / moboTotal);
                }

                float tint_factor = (float)1 / 8;
                Serie currentSerie = scatterChart.series.list[1];

                // byte newR = Convert.ToByte(currentSerie.data[i].itemStyle.color.r + (float)(200 - currentSerie.data[i].itemStyle.color.r) * tint_factor);
                // byte newG = Convert.ToByte(currentSerie.data[i].itemStyle.color.g + (float)(200 - currentSerie.data[i].itemStyle.color.g) * tint_factor);
                // byte newB = Convert.ToByte(currentSerie.data[i].itemStyle.color.b + (float)(200 - currentSerie.data[i].itemStyle.color.b) * tint_factor);

                currentSerie.data[i].itemStyle.color = new Color32(newR, newG, newB, 255);

                int lineIndex = -1;
                for (int j = 0; j < lineChart.series.Count; j++)
                {
                    if (lineChart.series.list[j].name == i.ToString())
                    {
                        lineIndex = j;
                        break;
                    }
                }

                // newR = Convert.ToByte(lineChart.theme.colorPalette[lineIndex].r + (float)(200 - lineChart.theme.colorPalette[lineIndex].r) * tint_factor);
                // newG = Convert.ToByte(lineChart.theme.colorPalette[lineIndex].g + (float)(200 - lineChart.theme.colorPalette[lineIndex].g) * tint_factor);
                // newB = Convert.ToByte(lineChart.theme.colorPalette[lineIndex].b + (float)(200 - lineChart.theme.colorPalette[lineIndex].b) * tint_factor);
                lineChart.theme.colorPalette[lineIndex] = new Color32(newR, newG, newB, 255);
            }

        }
        // int moboTotal = linetype.Where(x => x.Equals(1)).Count();
        // int designerTotal = linetype.Where(x => x.Equals(0)).Count();
        // int moboCount = 0;
        // int designerCount = 0;
        // int lineIndex = -1;
        // for (int i = 0; i < lineChart.series.Count; i++)
        // {
        //     int index = Convert.ToInt32(lineChart.series.list[i].name);

        // }
    }

    void OnDestroy()
    {
        if (!isMOBO)
        {
            SaveToCSVBtn();
        }
    }

}
