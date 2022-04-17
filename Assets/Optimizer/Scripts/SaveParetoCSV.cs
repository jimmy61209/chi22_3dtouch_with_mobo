using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEditor;
using System.IO;

public class SaveParetoCSV
{
    public class KeyFrame
    {
        public int SliderIndex;
        public string Action;
        public float Time;

        public KeyFrame() { }

        public KeyFrame(int sliderIndex, string action, float time)
        {
            SliderIndex = sliderIndex;
            Action = action;
            Time = time;
        }
    }

    private static List<KeyFrame> keyFrames = new List<KeyFrame>();

    // private int _counter;
    // public int Counter
    // {
    //     get => _counter;
    //     set
    //     {
    //         _counter = value;
    //         keyFrames.Add(new KeyFrame(value, Time.time));
    //     }
    // }

    public static void AddData(int sliderIndex, string action){
        keyFrames.Add(new KeyFrame(sliderIndex, action, Time.time));
    }

    public static string ToCSV()
    {
        var sb = new StringBuilder("Time,Action,SliderIndex");
        foreach (var frame in keyFrames)
        {
            sb.Append('\n').Append(frame.Time.ToString()).Append(',' + frame.Action + ',').Append(frame.SliderIndex.ToString());
        }

        return sb.ToString();
    }

    public static void SaveToFile()
    {
        // Use the CSV generation from before
        var content = ToCSV();

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
    var folder = Application.persistentDataPath;
#endif

        var filePath = Path.Combine(folder, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "user_slider.csv");

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
