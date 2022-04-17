using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Env3DTouch;
using OptScourcing;
using System;
using BNG;
using UnityEngine.UI;
public class EnvManager : MonoBehaviour
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
    public Canvas info;
    public ScatterController scatterController;
    public VRUISystem vRUISystem;
    public bool practice = true;


    void Start()
    {
        Optimizer.addParameter("D", 0f, 1f, Optimizer.CONTINOUS);
        Optimizer.addParameter("K", 0f, 0.5f, Optimizer.CONTINOUS);
        Optimizer.addParameter("Amplitude", 30f, 100f, Optimizer.CONTINOUS);
        Optimizer.addParameter("Gap", -5f, 15f, Optimizer.CONTINOUS);


        Optimizer.addObjective("Time", 900f, 1600f, Optimizer.SMALLER_IS_BETTER);
        Optimizer.addObjective("Overshoot", 0f, 10f, Optimizer.SMALLER_IS_BETTER);
        // Optimizer.addObjective("HapticFeedback", new LikertScale(Optimizer.LIKERT_7_SCALE, "Haptic feedback"));

        InitEnvironment();
        StartIteration();
        // Optimizer.setTrial(36);
        // SetToWaitingMode();
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
        EqualSpawnPoint.ResetSpawnOrder();
        StartUnitTask();
        startIterationCanvas.SetActive(false);
        takeABreak.SetActive(false);
        startIteration = true;
        if (!practice)
        {
            iterationNumber++;
            SaveParameterCSV.AddEvaluationData(0, 0, $"{GetNormalizedParameter("D")},{GetNormalizedParameter("K")},{GetNormalizedParameter("Amplitude")},{GetNormalizedParameter("Gap")}");
        }
        CloseInfo();
    }

    public void UpdateDesignParameters()
    {
        if (practice)
        {
            target.GetComponentInChildren<VibrationHandler>().setVibratePar(60, 0);
            gogoController.SetGogoParmeter(0.3f, 0.3f);
            return;
        }
        float gogo_d = Optimizer.getParameterValue("D") * armLen;
        float gogo_k = Optimizer.getParameterValue("K");
        

        float vibAmplitude = Optimizer.getParameterValue("Amplitude");
        float vibThreashold = Optimizer.getParameterValue("Gap");


        target.GetComponentInChildren<VibrationHandler>().setVibratePar(vibAmplitude, vibThreashold);
        gogoController.SetGogoParmeter(gogo_d, gogo_k);
    }
    public void OnFinishUnitTask(List<float> objectives, bool trialFail)
    {
        if (practice)
        {
            startTrial = false;
            StartUnitTask();
            return;
        }

        Optimizer.getObjective("Time").addTrial(objectives[0]);
        Optimizer.getObjective("Overshoot").addTrial(objectives[1]);
        SaveParameterCSV.AddEvaluationData(objectives[0], objectives[1], "");


        startTrial = false;

        if (EqualSpawnPoint.count == EqualSpawnPoint.trial_num || trialFail)
        {
            FinishIteration(trialFail);
            EqualSpawnPoint.count = 0;
        }
        else
        {
            StartUnitTask();
        }
    }
    void SetToWaitingMode()
    {
        if ((iterationNumber) % 10 == 0 && iterationNumber != 0)
        {
            takeABreak.SetActive(true);
            if (iterationNumber == 40)
            {
                WaitingText.GetComponent<Text>().text = "Finished!";
            }
        }

        ShowInfo();
        WaitingText.SetActive(true);
        waiting = true;

    }

    // clear the values in objectives and send values through socket
    void FinishIteration(bool trialFail)
    {
        List<float> finalObjectives = new List<float>();

        if (trialFail)
        {
            // 3000 and 30 is temporal setting
            finalObjectives.Add(3000);
            finalObjectives.Add(30);
        }
        foreach (KeyValuePair<string, ObjectiveArgs> objective in Optimizer.objectives)
        {

            if (!trialFail)
            {
                finalObjectives.Add(objective.Value.RemoveAndGetPracticedAvargeObjective(10));
            }
            objective.Value.clearTrial();
        }
        // print($"{finalObjectives[0]}, {finalObjectives[1]}");
        scatterController.AddObservationData((int)(GetNormalizedParameter("D") * 100), (int)(GetNormalizedParameter("K") * 100), (int)(GetNormalizedParameter("Amplitude") * 100), (int)(GetNormalizedParameter("Gap") * 100),
            finalObjectives[0], finalObjectives[1]);
        Optimizer.socket.SendObjectives(finalObjectives);

        startIteration = false;
        SetToWaitingMode();
    }

    IEnumerator WaitForVibration()
    {
        OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);

        yield return new WaitForSeconds(0.5f);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }


    void Update()
    {
        // print(waiting);
        // if is waiting for data, check if there is data come
        if (waiting == true)
        {
            if (Optimizer.canRead == true)
            {
                waiting = false;
                WaitingText.SetActive(false);
                startIterationCanvas.SetActive(true);
                Optimizer.canRead = false;
            }
        }

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
            EqualSpawnPoint.armLen = armLen;
            RandomSpawnPoint.armLen = armLen;
            print($"update arm length: {armLen}");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            startIteration = false;
            startTrial = false;
            SetToWaitingMode();
            practice = false;
            Optimizer.startOptimization();
            EqualSpawnPoint.count = 0;
            target.Find("mesh").GetComponent<MeshRenderer>().enabled = false;
        }

        if (InputBridge.Instance.BButtonDown || InputBridge.Instance.AButtonDown || InputBridge.Instance.XButtonDown || InputBridge.Instance.YButtonDown)
        {
            if (info.enabled == true)
            {
                info.enabled = false;
            }
            else
            {
                info.enabled = true;
            }
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
        if (practice)
        {
            RandomSpawnPoint.AppearInRandomPos(ref target, origin);
            startTrial = true;
            return;
        }
        EqualSpawnPoint.AppearInPredefinePos(ref target, origin);
        startTrial = true;
    }
    public void CloseInfo()
    {
        info.enabled = false;
    }
    public void ShowInfo()
    {
        info.enabled = true;
    }

    void OnDestroy()
    {
        if (Optimizer.socket != null)
        {
            Optimizer.socket.SocketQuit();
        }
        SaveParameterCSV.SaveToFile2(0);
    }

    public void SetText(string s)
    {
        mText.text = s;
    }
}







