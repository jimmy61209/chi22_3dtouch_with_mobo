using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Env3DTouch;
using OptScourcing;
using System;
using BNG;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine.UI;
public class FinalEnvManager : MonoBehaviour
{
    public Transform target;
    public Transform origin;
    [SerializeField]
    Transform Rcontroller;
    [SerializeField]
    Transform Lcontroller;
    [SerializeField]
    TextMesh mText;
    [SerializeField]
    float armLen = 0.4f;
    [System.NonSerialized]
    public Transform currentController;
    [System.NonSerialized]
    public Transform realHand;
    enum HandType
    {
        right,
        left
    }
    [SerializeField]
    HandType hand = new HandType();
    [NonSerialized]
    public bool pause = false;

    public bool startTrial = false;
    Action<List<float>> sendObjectives;
    GogoController gogoController = new GogoController();

    float D, K;

    public GameObject startIterationCanvas;
    public GameObject WaitingText;
    public bool waiting = true;
    public bool startIteration = false;
    public PlayerTeleport playerTeleport;
    public Transform playerOrigin;
    int iterationNumber = 0;
    public GameObject takeABreak;
    public VRUISystem vRUISystem;
    List<Dictionary<string, object>> csvData;
    List<int> order = new List<int>();
    public static List<float> CompletionTime = new List<float>();
    public static List<float> SpatialError = new List<float>();
    List<CSVdata> outputData = new List<CSVdata>();


    void Start()
    {
        csvData = CSVReader.Read("final_design_user_select");
        for(int i = 0; i < csvData.Count; i++){
            order.Add(i);
        }
        order = order.OrderBy(a => Guid.NewGuid()).ToList();

        InitEnvironment();
        // Optimizer.startOptimization();
        SetToWaitingMode();
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    float GetNormalizedParameter(string name)
    {

        float val = Optimizer.getParameterValue(name);
        return Remap(val, Optimizer.getParameter(name).lowerBound, Optimizer.getParameter(name).upperBound, 0, 1);
    }

    // start an iteration, user press button
    public void StartIteration()
    {
        FinalEqualSpawnPoint.ResetSpawnOrder();
        startIterationCanvas.SetActive(false);
        takeABreak.SetActive(false);
        startIteration = true;
        iterationNumber++;
        StartUnitTask();
        SaveParameterCSV.FinalAddEvaluationData(0, 0, $"{csvData[order[iterationNumber - 1]]["D"]},{csvData[order[iterationNumber - 1]]["K"]},{csvData[order[iterationNumber - 1]]["Amplitude"]},{csvData[order[iterationNumber - 1]]["Gap"]}");
    }

    public void UpdateDesignParameters()
    {
        float gogo_d = Convert.ToSingle(csvData[order[iterationNumber - 1]]["D"]) * armLen;
        float gogo_k = Convert.ToSingle(csvData[order[iterationNumber - 1]]["K"]);

        float vibAmplitude = Convert.ToSingle(csvData[order[iterationNumber - 1]]["Amplitude"]);
        float vibThreashold = Convert.ToSingle(csvData[order[iterationNumber - 1]]["Gap"]);

        // rescale
        gogo_d = Remap(gogo_d, 0, 100, 0, 1);
        gogo_k = Remap(gogo_k, 0, 100, 0, 0.5f);
        vibAmplitude = Remap(vibAmplitude, 0, 100, 30, 100);
        vibThreashold = Remap(vibThreashold, 0, 100, -5, 15);
        target.GetComponentInChildren<FinalVibrationHandler>().setVibratePar(vibAmplitude, vibThreashold);
        gogoController.SetGogoParmeter(gogo_d, gogo_k);
    }
    public void OnFinishUnitTask(List<float> objectives)
    {
        // Optimizer.getObjective("Time").addTrial(objectives[0]);
        // Optimizer.getObjective("Overshoot").addTrial(objectives[1]);
        SaveParameterCSV.FinalAddEvaluationData(objectives[0], objectives[1], "");
        CompletionTime.Add(objectives[0]);
        SpatialError.Add(objectives[1]);

        startTrial = false;

        if (FinalEqualSpawnPoint.count == FinalEqualSpawnPoint.trial_num)
        {
            FinishIteration();
            FinalEqualSpawnPoint.count = 0;
        }
        else
        {
            StartUnitTask();
        }
    }
    void SetToWaitingMode()
    {
        startIterationCanvas.SetActive(true);
        if(iterationNumber == csvData.Count){
            startIterationCanvas.transform.Find("Text").GetComponent<Text>().text = "Finished";
            startIterationCanvas.transform.Find("Button").gameObject.SetActive(false);

        }
    }

    // clear the values in objectives and send values through socket
    void FinishIteration()
    {
        // get rid of the first 10 elements
        if (CompletionTime.Count > 10)
        {
            CompletionTime.RemoveRange(0, 10);
            SpatialError.RemoveRange(0, 10);
        }
        outputData.Add(new CSVdata(GetListAverage(CompletionTime), GetListAverage(SpatialError), Convert.ToSingle(csvData[order[iterationNumber - 1]]["D"]), Convert.ToSingle(csvData[order[iterationNumber - 1]]["K"]),
         Convert.ToSingle(csvData[order[iterationNumber - 1]]["Amplitude"]), Convert.ToSingle(csvData[order[iterationNumber - 1]]["Gap"])));
        // Optimizer.socket.SendObjectives(finalObjectives);
        CompletionTime.Clear();
        SpatialError.Clear();
        startIteration = false;
        SetToWaitingMode();
    }
    float GetListAverage(List<float> values)
    {
        return (float)(values.Count > 0 ? values.Average() : 0.0);
    }


    void Update()
    {


        // recenter user's position and direction
        if (Input.GetKeyDown(KeyCode.R))
        {
            // OVRManager.display.RecenterPose();
            playerTeleport.TeleportPlayerToTransform(playerOrigin);

        }

        // set the origin point
        if (Input.GetKeyDown(KeyCode.T))
        {
            origin.position = realHand.position;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            armLen = Vector3.Distance(origin.position, realHand.position);
            FinalEqualSpawnPoint.armLen = armLen;
            RandomSpawnPoint.armLen = armLen;
            print($"update arm length: {armLen}");
        }
        gogoController.UpdateHandPosition(origin, target, realHand, currentController);
    }
    public void InitEnvironment()
    {
        print("start at begin");
        target.Find("mesh").GetComponent<MeshRenderer>().enabled = false;
        target.position = new Vector3(0, 0, 0);
        if (hand == HandType.right)
        {
            currentController = Rcontroller;
            Lcontroller.gameObject.SetActive(false);
            Lcontroller.parent.Find("LeftHandPointer").gameObject.SetActive(false);
            vRUISystem.SelectedHand = ControllerHand.Right;
        }
        else
        {
            currentController = Lcontroller;
            Rcontroller.gameObject.SetActive(false);
            Rcontroller.parent.Find("RightHandPointer").gameObject.SetActive(false);
            vRUISystem.SelectedHand = ControllerHand.Left;
        }
        realHand = currentController.parent.parent;
    }
    // start a trial
    public void StartUnitTask()
    {
        UpdateDesignParameters();
        SetText("");
        target.Find("mesh").GetComponent<MeshRenderer>().enabled = true;
        FinalEqualSpawnPoint.AppearInPredefinePos(ref target, origin);
        startTrial = true;
    }


    void OnDestroy()
    {
        if (Optimizer.socket != null)
        {
            Optimizer.socket.SocketQuit();
        }
        SaveParameterCSV.SaveToFile2(2);
        SaveToCSV();
    }

    public void SetText(string s)
    {
        mText.text = s;
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

    public void SaveToCSV()
    {

        var sb = new StringBuilder("CompletionTime,SpatialError,D,K,Amplitude,Gap");
        foreach (var frame in outputData)
        {
            sb.Append('\n').Append(frame.CompletionTime.ToString() + ',').Append(frame.SpatialError.ToString() + ',')
            .Append(frame.D.ToString() + ',').Append(frame.K.ToString() + ',').Append(frame.Amplitude.ToString() + ',').Append(frame.Gap.ToString());
        }

        string path = "Assets/Resources/ResultsOfThreeSelectedDesigns/";
        var filePath = Path.Combine(path, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "ResultsOfThreeSelectedDesigns.csv");

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(sb.ToString());
        }
        Debug.Log($"CSV file written to \"{filePath}\"");

    }
}
