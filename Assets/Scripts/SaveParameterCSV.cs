using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEditor;
using System.IO;
using Env3DTouch;

public class SaveParameterCSV
{
    public class KeyFrame
    {
        public float SliderValue;
        public string Action;
        public float Time;

        public KeyFrame() { }

        public KeyFrame(float sliderValue, string action, float time)
        {
            SliderValue = sliderValue;
            Action = action;
            Time = time;
        }
    }
    public class Evaluation
    {
        public float CompletionTime;
        public float SpatialError;
        public string Action;
        public float Time;

        public int AZIndex;
        public int IIndex;
        public int distance;
        public int boxSize;

        public Evaluation() { }

        public Evaluation(float completionTime, float spatialError, string action, float time, int AZ, int I, int currentDistance, int currentBoxSize)
        {
            CompletionTime = completionTime;
            Action = action;
            SpatialError = spatialError;
            Time = time;

            AZIndex = AZ;
            IIndex = I;
            distance = currentDistance;
            boxSize = currentBoxSize;
        }
    }
    public class ForbiddenExtraData{
        public string action;
        public float time;
        public int index;
        public ForbiddenExtraData(string action, float time, int index){
            this.time = time;
            this.action = action;
            this.index = index;
        }
    }



    private static List<KeyFrame> keyFrames = new List<KeyFrame>();
    private static List<Evaluation> evaluation = new List<Evaluation>();
    private static List<LineChartData> ForbiddenFrames = new List<LineChartData>();
    private static List<ForbiddenExtraData> ForbiddenAction = new List<ForbiddenExtraData>();

    public static void AddData(float sliderValue, string action)
    {
        keyFrames.Add(new KeyFrame(sliderValue, action, Time.time));
    }
    public static void AddForbiddenData(LineChartData lineChartData, string action, int index)
    {
        ForbiddenFrames.Add(new LineChartData(lineChartData));
        ForbiddenAction.Add(new ForbiddenExtraData(action, Time.time, index));
    }
    public static void AddEvaluationData(float completionTime, float spatialError, string action)
    {
        evaluation.Add(new Evaluation(completionTime, spatialError, action, Time.time, EqualSpawnPoint.AZIndex, EqualSpawnPoint.IIndex, EqualSpawnPoint.currentDistance, EqualSpawnPoint.currentBoxSize));
    }
    public static void FinalAddEvaluationData(float completionTime, float spatialError, string action)
    {
        evaluation.Add(new Evaluation(completionTime, spatialError, action, Time.time, FinalEqualSpawnPoint.AZIndex, FinalEqualSpawnPoint.IIndex, FinalEqualSpawnPoint.currentDistance, FinalEqualSpawnPoint.currentBoxSize));
    }

    public static string ToCSV(string stringOfName)
    {
        var sb = new StringBuilder(stringOfName);
        foreach (var frame in keyFrames)
        {
            sb.Append('\n').Append(frame.Time.ToString()).Append(',' + frame.Action + ',').Append(frame.SliderValue.ToString());
        }

        return sb.ToString();
    }
    public static string ToForbiddenCSV(string stringOfName)
    {
        var sb = new StringBuilder(stringOfName);
        for (int i = 0; i < ForbiddenFrames.Count; i++)
        {
            if (!ForbiddenFrames[i].isDeleted)
            {
                sb.Append('\n' + ForbiddenAction[i].time.ToString() + ',' + ForbiddenAction[i].action + ',' + ForbiddenAction[i].index.ToString() + ',')
                                        .Append(ForbiddenFrames[i].rootValue[0].ToString() + ',' + ForbiddenFrames[i].upperBound[0].ToString() + ',' + ForbiddenFrames[i].lowerBound[0].ToString() + ',')
                                        .Append(ForbiddenFrames[i].rootValue[1].ToString() + ',' + ForbiddenFrames[i].upperBound[1].ToString() + ',' + ForbiddenFrames[i].lowerBound[1].ToString() + ',')
                                        .Append(ForbiddenFrames[i].rootValue[2].ToString() + ',' + ForbiddenFrames[i].upperBound[2].ToString() + ',' + ForbiddenFrames[i].lowerBound[2].ToString() + ',')
                                        .Append(ForbiddenFrames[i].rootValue[3].ToString() + ',' + ForbiddenFrames[i].upperBound[3].ToString() + ',' + ForbiddenFrames[i].lowerBound[3].ToString() + ',')
                                        .Append(ForbiddenFrames[i].beta.ToString());
            }

        }

        return sb.ToString();
    }
    public static string ToEvaluetaionCSV(string stringOfName)
    {
        var sb = new StringBuilder(stringOfName);
        foreach (var frame in evaluation)
        {
            sb.Append('\n').Append(frame.Time.ToString() + ',').Append(frame.CompletionTime.ToString()).Append(',' + frame.SpatialError.ToString() + ',')
            .Append(frame.AZIndex.ToString() + ',').Append(frame.IIndex.ToString() + ',').Append(frame.distance.ToString() + ',').Append(frame.boxSize.ToString() + ',').Append(frame.Action);
        }

        return sb.ToString();
    }

    //index 0: mobo, index 1: trial-and-error, index2: final, index 3: hybrid
    public static void SaveToFile2(int fileNameIndex)
    {
        // Use the CSV generation from before
        var content = ToEvaluetaionCSV("Time,CompletionTime,SpatialError,AZIndex,IIndex,distance,boxSize,Action");

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = "Assets/Resources";

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
    var folder = Application.persistentDataPath;
#endif
        string identify;
        string subfolder;
        if (fileNameIndex == 0)
        {
            identify = "DataPerTargetPerEvalutions";
            subfolder = "/MOBOLed";
        }
        else if (fileNameIndex == 1)
        {
            identify = "DataPerTargetPerEvalutions";
            subfolder = "/DesignerLed";
        }
        else if (fileNameIndex == 2)
        {
            identify = "DataPerTargetPerThreeSelectedEvalution";
            subfolder = "/ResultsOfThreeSelectedDesigns";
        }
        else
        {
            identify = "DataPerTargetPerEvalution";
            subfolder = "/Hybrid";
        }
        var filePath = Path.Combine(folder + subfolder, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + identify + ".csv");

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }

        // Or just
        //File.WriteAllText(content);

        Debug.Log($"CSV file written to \"{filePath}\"");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    public static void SaveToFile(bool isHybrid = false)
    {
        // Use the CSV generation from before
        var content = ToCSV("Time,Action,SliderValue");

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = "Assets/Resources";

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
    var folder = Application.persistentDataPath;
#endif

        var filePath = "";
        if (isHybrid == false)
        {
            filePath = Path.Combine(folder + "/DesignerLed", System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "DataPerParameterSliderChange.csv");
        }
        else
        {
            filePath = Path.Combine(folder + "/Hybrid", System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "DataPerParameterSliderChange.csv");
        }
        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }

        // Or just
        //File.WriteAllText(content);

        Debug.Log($"CSV file written to \"{filePath}\"");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    public static void SaveForbiddenEvent()
    {
        // Use the CSV generation from before
        var content = ToForbiddenCSV("Time,Action,RegionIndex,D_root,D_upper,D_lower,K_root,K_upper,K_lower,Amplitude_root,Amplitude_upper,Amplitude_lower,Gap_root,Gap_upper,Gap_lower,beta");

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = "Assets/Resources";

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
    var folder = Application.persistentDataPath;
#endif


        var filePath = Path.Combine(folder + "/Hybrid", System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "DataPerForbiddedRegionChange.csv");
        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }

        // Or just
        //File.WriteAllText(content);

        Debug.Log($"CSV file written to \"{filePath}\"");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
