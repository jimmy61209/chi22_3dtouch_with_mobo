using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Env3DTouch;
using OptScourcing;
using System;
using System.Linq;
using BNG;
public class EnvManagerParameter : MonoBehaviour
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
    protected float armLen = 0.4f;
    // [System.NonSerialized]
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
    protected GogoController gogoController = new GogoController();

    protected static float D, K, Amplitude, Gap;
    // scale form 0 to 100
    public static float[] x = new float[4];
    public static List<float> CompletionTime = new List<float>();
    public static List<float> SpatialError = new List<float>();

    protected ParameterSliderController parameterSliderController;
    protected int currentIteration = 0;
    public PlayerTeleport playerTeleport;
    public Transform playerOrigin;
    public ScatterController scatterController;
    public Canvas info;
    public Canvas skipEvaluation;
    public bool IsMOBO = false;
    public bool IsHybrid = false;
    public VRUISystem vRUISystem;
    public GameObject UIInRightSides;
    public Camera monitorCamera;

    protected virtual void Start()
    {
        InitEnvironment();
        StartUnitTask();
        if (!IsMOBO)
        {
            parameterSliderController = transform.GetComponent<ParameterSliderController>();
        }
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    public static void RemapValues()
    {
        D = Remap(x[0], 0, 100, 0, 1);
        K = Remap(x[1], 0, 100, 0, 0.5f);
        Amplitude = Remap(x[2], 0, 100, 30, 100);
        Gap = Remap(x[3], 0, 100, -5, 15);
    }
    public void UpdateDesignParameters()
    {
        RemapValues();
        target.GetComponentInChildren<VibrationHandler>().setVibratePar(Amplitude, Gap);
        gogoController.SetGogoParmeter(D * armLen, K);
    }
    public virtual void OnFinishUnitTask(List<float> objectives)
    {
        startTrial = false;
        if (ParameterSliderController.startIteration == false)
        {
            SaveParameterCSV.AddData(0, $"Finished Non-Evaluation Unit Task,{x[0]},{x[1]},{x[2]},{x[3]}");
        }
        else
        {
            SaveParameterCSV.AddEvaluationData(objectives[0], objectives[1], "");
            CompletionTime.Add(objectives[0]);
            SpatialError.Add(objectives[1]);
        }
        // print($"{objectives[0]}, {objectives[1]}");

        if (EqualSpawnPoint.count == EqualSpawnPoint.trial_num)
        {
            FinishIteration();
            EqualSpawnPoint.count = 0;
        }
        StartUnitTask();


    }

    IEnumerator WaitForVibration()
    {
        OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);

        yield return new WaitForSeconds(0.5f);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    protected virtual void Update()
    {
        // print(InputBridge.Instance.RightThumbstickAxis);

        // print($"{x[0]}, {x[1]}, {x[2]}, {x[3]}");
        // recenter user's position and direction
        if (Input.GetKeyDown(KeyCode.R))
        {
            // OVRManager.display.RecenterPose();
            playerTeleport.TeleportPlayerToTransform(playerOrigin);

        }
        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     StartCoroutine(WaitForVibration());
        //     print("a");
        // }
        // set the origin point
        if (Input.GetKeyDown(KeyCode.T))
        {
            origin.position = realHand.position;
            // print("set origin");
        }
        // 
        if (InputBridge.Instance.BButtonDown || InputBridge.Instance.AButtonDown || InputBridge.Instance.XButtonDown || InputBridge.Instance.YButtonDown)
        {
            if (ParameterSliderController.startIteration == true && !IsMOBO)
            {
                if (skipEvaluation.enabled == true)
                {
                    skipEvaluation.enabled = false;
                }
                else
                {
                    skipEvaluation.enabled = true;
                }
            }
            else
            {
                if (info.enabled == true)
                {
                    info.enabled = false;
                    // UIInRightSides.SetActive(true);
                }
                else
                {
                    info.enabled = true;
                    // UIInRightSides.SetActive(false);
                }
            }


        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            armLen = Vector3.Distance(origin.position, realHand.position);
            EqualSpawnPoint.armLen = armLen;
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
            // vRUISystem.SelectedHand = ControllerHand.Right;
        }
        else
        {
            currentController = Lcontroller;
            Rcontroller.gameObject.SetActive(false);
            Rcontroller.parent.Find("RightHandPointer").gameObject.SetActive(false);
            // vRUISystem.SelectedHand = ControllerHand.Left;
        }
        realHand = currentController.parent.parent;
    }
    public void StartUnitTask()
    {
        // print("start unit task");
        startTrial = true;
        UpdateDesignParameters();
        SetText("");
        target.Find("mesh").GetComponent<MeshRenderer>().enabled = true;
        if (ParameterSliderController.startIteration == false)
        {
            RandomSpawnPoint.AppearInRandomPos(ref target, origin);

        }
        else
        {
            EqualSpawnPoint.AppearInPredefinePos(ref target, origin);
        }
    }

    protected virtual void FinishIteration()
    {
        if (!IsMOBO)
        {
            // get rid of the first 10 elements
            if (CompletionTime.Count > 10)
            {
                CompletionTime.RemoveRange(0, 10);
                SpatialError.RemoveRange(0, 10);
            }

            currentIteration++;
            // parameterSliderController.UpdateInformation(++currentIteration, x[0], x[1], x[2], x[3], GetListAverage(CompletionTime), GetListAverage(SpatialError));
            scatterController.AddObservationData((int)x[0], (int)x[1], (int)x[2], (int)x[3], GetListAverage(CompletionTime), GetListAverage(SpatialError));
        }
        CompletionTime.Clear();
        SpatialError.Clear();
        parameterSliderController.ShowSlider();
        // UIInRightSides.SetActive(true);

    }

    protected float GetListAverage(List<float> values)
    {
        return (float)(values.Count > 0 ? values.Average() : 0.0);
    }

    private void OnDestroy()
    {
        // save per slider changes, temperary removed
        //SaveParameterCSV.SaveToFile();
        SaveParameterCSV.SaveToFile2(1);
    }


    public void SetText(string s)
    {
        mText.text = s;
    }
}
