using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Env3DTouch;
using UnityEngine.UI;
public class ParameterSliderController : MonoBehaviour
{
    public GameObject parameterSlider;
    public Canvas info;
    public Canvas skipEvaluation;
    public static bool startIteration = false;
    public Text infoText;
    public EnvManagerParameter envManagerParameter;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CloseSlider()
    {
        parameterSlider.SetActive(false);
        info.enabled = false;
        skipEvaluation.enabled = false;
        startIteration = true;
        SaveParameterCSV.AddEvaluationData(0, 0, $"{EnvManagerParameter.x[0]},{EnvManagerParameter.x[1]},{EnvManagerParameter.x[2]},{EnvManagerParameter.x[3]}");
        SaveParameterCSV.AddData(0, $"Start Evaluation,{EnvManagerParameter.x[0]},{EnvManagerParameter.x[1]},{EnvManagerParameter.x[2]},{EnvManagerParameter.x[3]}");
        EqualSpawnPoint.ResetSpawnOrder();
        EqualSpawnPoint.AppearInPredefinePos(ref envManagerParameter.target, envManagerParameter.origin);
    }

    public void ControlledCaseStartEvaluation(){
        skipEvaluation.enabled = false;
        startIteration = true;
        SaveParameterCSV.AddEvaluationData(0, 0, $"{EnvManagerParameter.x[0]},{EnvManagerParameter.x[1]},{EnvManagerParameter.x[2]},{EnvManagerParameter.x[3]}");
        SaveParameterCSV.AddData(0, $"Start Evaluation,{EnvManagerParameter.x[0]},{EnvManagerParameter.x[1]},{EnvManagerParameter.x[2]},{EnvManagerParameter.x[3]}");
        EqualSpawnPoint.ResetSpawnOrder();
        EqualSpawnPoint.AppearInPredefinePos(ref envManagerParameter.target, envManagerParameter.origin);

        Camera.main.backgroundColor = new Color32(106, 115, 106, 5);
        envManagerParameter.monitorCamera.backgroundColor = new Color32(106, 115, 106, 5);
    }

    public void SkipEvaluation()
    {
        ShowSlider();
        EqualSpawnPoint.count = 0;
        EnvManagerParameter.CompletionTime.Clear();
        EnvManagerParameter.SpatialError.Clear();
        SaveParameterCSV.AddEvaluationData(-1, -1, $"{EnvManagerParameter.x[0]},{EnvManagerParameter.x[1]},{EnvManagerParameter.x[2]},{EnvManagerParameter.x[3]}");
        SaveParameterCSV.AddData(0, $"Skip Evaluation,{EnvManagerParameter.x[0]},{EnvManagerParameter.x[1]},{EnvManagerParameter.x[2]},{EnvManagerParameter.x[3]}");
    }

    public void HideSkipEvaluation(){
        skipEvaluation.enabled = false;
    }

    public void ShowSlider()
    {
        parameterSlider.SetActive(true);
        info.enabled = true;
        skipEvaluation.enabled = false;
        startIteration = false;
    }




    public void UpdateInformation(int iteration, float D, float K, float Amplitude, float Gap, float CompletionTime, float SpatialError)
    {
        string newIteration = "";
        newIteration += $"Iteration {iteration}:\n";
        newIteration += $"D: {D}\n";
        newIteration += $"K: {K}\n";
        newIteration += $"Amplitude: {Amplitude}\n";
        newIteration += $"Gap: {Gap}\n";

        newIteration += $"Completion Time: {CompletionTime}\n";
        newIteration += $"Spatial Error: {SpatialError}\n\n";

        infoText.text += newIteration;
    }


}
