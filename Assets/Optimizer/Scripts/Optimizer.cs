using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OptScourcing;
using UnityEngine.UI;
using System;
using UnityEditor;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class Optimizer : MonoBehaviour
{
    public const bool CONTINOUS = false;
    public const bool DISCRETE = true;
    public const bool BIGGER_IS_BETTER = false;
    public const bool SMALLER_IS_BETTER = true;
    public const int LIKERT_7_SCALE = 7;
    public const int LIKERT_5_SCALE = 5;


    public static Optimizer Instance { get; private set; }
    public bool testMode = false;
    public static bool parallel = false;
    public static bool testModeStatic = false;

    public static bool canRead = false;
    public static bool preCanRead = false;
    public static Dictionary<string, ParameterArgs> parameters = new Dictionary<string, ParameterArgs>();
    public static Dictionary<string, ObjectiveArgs> objectives = new Dictionary<string, ObjectiveArgs>();
    // static int iterNumber = 100;
    static int trialNumber = 10;
    int currentIterNumber = 0;
    int currentTrialNumber = 0;
    static int remindNumber = 1;
    int closedInTrialNum = 0;
    public static int TrialNumber
    {
        get
        {
            return trialNumber;
        }
        set
        {
            trialNumber = value;
            remindNumber = (value / 4 > 0) ? value / 4 : 1;
        }
    }
    bool reminded = false;
    public static SocketNetwork socket;
    Text iterNumberText;
    Text trialNumberText;
    GameObject nextIterQuestion;
    GameObject likertQuestionnaire;
    GameObject menu;
    // GameObject idleIcon;
    // GameObject showMsgIcon;
    Queue<KeyValuePair<string, ObjectiveArgs>> AskQuestion = new Queue<KeyValuePair<string, ObjectiveArgs>>();
    // WebViewPrefab _webViewPrefab;
    LineRenderer line;
    // GameObject showPerformance;
    // GameObject hidePerformance;
    // MeshRenderer webViewMesh;
    System.Diagnostics.Process pythonServer = new System.Diagnostics.Process();  // Auto Start Python Server Process
    static List<Dictionary<string, object>> csvData;
    RevisedParetoData paretoData;
    int index = 0;
    int pressStickCount = 0;
    Text finalObjectiveIndex;
    bool pressedStick = false;
    ParetoShowData currentData;


    // Start is called before the first frame update

    private void Awake() {
        Instance = this;
    }
    void Start()
    {
        print("running");
        testModeStatic = testMode;

        // pythonServer.StartInfo.FileName = "CMD.exe";
        // // pythonServer.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        // pythonServer.StartInfo.Arguments = "/C python Python/main.py";
        // pythonServer.Start();

        if (testMode == false)
        {
            menu = transform.Find("Menu").gameObject;

        }
        else
        {
            menu = transform.Find("ObjectiveControl").gameObject;
            paretoData = AssetDatabase.LoadAssetAtPath("Assets/GraphEditor/RevisedParetoData.asset", typeof(RevisedParetoData)) as RevisedParetoData;
            csvData = CSVReader.Read("global_pareto");
        }
        // idleIcon = transform.Find("Icon/IdleIcon").gameObject;
        // showMsgIcon = transform.Find("Icon/ShowMsgIcon").gameObject;
        iterNumberText = transform.Find("Menu/CurrentStatus/Iter:/IterNumber").GetComponent<Text>();
        trialNumberText = transform.Find("Menu/CurrentStatus/Trial:/TrialNumber").GetComponent<Text>();
        nextIterQuestion = transform.Find("Menu/AskIfNextIter").gameObject;
        likertQuestionnaire = transform.Find("Menu/LikertScaleQuestionnaire").gameObject;
        // showPerformance = transform.Find("Menu/ShowPerformance").gameObject;
        // hidePerformance = transform.Find("Menu/HidePerformance").gameObject;
        line = GameObject.Find("UIHelpers/LaserPointer").GetComponent<LineRenderer>();
        // finalObjectiveIndex = transform.Find("FinalObjectiveIndex").GetComponent<Text>();

        currentData = AssetDatabase.LoadAssetAtPath("Assets/GraphEditor/ParetoShowData.asset", typeof(ParetoShowData)) as ParetoShowData;
        // print(gameObject.scene.name);
        // CreateWebView();

    }
    // void CreateWebView()
    // {
    //     Web.ClearAllData();

    //     // Create a 0.6 x 0.4 instance of the prefab.
    //     _webViewPrefab = WebViewPrefab.Instantiate(0.55f, 0.15f);
    //     _webViewPrefab.transform.parent = transform;
    //     _webViewPrefab.transform.localPosition = new Vector3(100f, 0f, 0.6f);
    //     _webViewPrefab.transform.LookAt(transform.parent);
    //     _webViewPrefab.InitialResolution = 2200;
    //     _webViewPrefab.Initialized += (sender, e) =>
    //     {
    //         _webViewPrefab.WebView.LoadUrl("140.113.194.164/performance.html");
    //     };
    //     hidePerformance.transform.parent = _webViewPrefab.transform;
    //     hidePerformance.transform.localPosition = new Vector3(-0.28f, 0, 0);
    //     WaitToInitialized();
    // }
    // async void WaitToInitialized()
    // {
    //     _webViewPrefab.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
    //     await _webViewPrefab.WaitUntilInitialized();
    //     _webViewPrefab.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = true;
    //     _webViewPrefab.gameObject.SetActive(false);
    // }
    // async void WaitToExecuteJS()
    // {
    //     await _webViewPrefab.WaitUntilInitialized();
    //     await _webViewPrefab.WebView.ExecuteJavaScript("window.location.reload(true);");

    // }
    // async void ReloadPage(object sender, ProgressChangedEventArgs eventArgs)
    // {
    //     print("in reload page");
    //     if (eventArgs.Type == ProgressChangeType.Finished)
    //     {
    //         print("finish loading");
    //         // Debug.Log(String.Format(AUTO_LOGIN_JS, username, password));
    //         await _webViewPrefab.WebView.ExecuteJavaScript("window.location.reload(true);");
    //         _webViewPrefab.WebView.LoadProgressChanged -= ReloadPage;
    //     }
    // }

    // Update is called once per frame
    void Update()
    {

        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     print("Q");
        //     WaitToExecuteJS();
        //     //    _webViewPrefab.WebView.Reload();
        // }
        if (Input.GetKeyDown(KeyCode.O) || OVRInput.GetDown(OVRInput.Button.Two))
        {
            TuggleMenu();
        }
        // if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x >= 0.5f && pressedStick == false)
        // {
        //     pressedStick = true;
        //     index++;
        //     finalObjectiveIndex.text = index.ToString() + ": " + csvData[index]["D"] + " " + csvData[index]["K"] + " " + csvData[index]["Amplitude"] + " " + csvData[index]["Gap"];
        //     updateParameter();
        // }
        // else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x <= -0.5f && pressedStick == false)
        // {
        //     pressedStick = true;
        //     index--;
        //     finalObjectiveIndex.text = index.ToString() + ": " + csvData[index]["D"] + " " + csvData[index]["K"] + " " + csvData[index]["Amplitude"] + " " + csvData[index]["Gap"];

        //     updateParameter();
        // }
        // if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x < 0.5f && OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > -0.5f)
        // {
        //     pressedStick = false;
        // }


        if (!testMode)
        {
            //check the least number of objective user update
            int minObjNum = Int32.MaxValue;
            foreach (KeyValuePair<string, ObjectiveArgs> objective in objectives)
            {
                // print(objective.Value.GetTrialNum());
                if (minObjNum > objective.Value.GetTrialNum() && objective.Value.popUp != true)
                {
                    minObjNum = objective.Value.GetTrialNum();
                    currentTrialNumber = minObjNum;
                }
            }
            //remind user when finish an iteration
            if (currentTrialNumber >= trialNumber)
            {
                if (reminded == false)
                {
                    reminded = true;
                    AskIfNextIter();
                }

                if (reminded == true && closedInTrialNum != (currentTrialNumber) && (currentTrialNumber - trialNumber) % remindNumber == 0)
                {
                    reminded = false;
                }
            }

            if (canRead == true && preCanRead == false)
            {
                StartCoroutine(RefreshImage());
            }
            preCanRead = canRead;


            iterNumberText.text = currentIterNumber.ToString();
            trialNumberText.text = currentTrialNumber.ToString();
        }

    }
    IEnumerator RefreshImage()
    {
        yield return null;
        AssetDatabase.Refresh();
    }

    // async void RefreshImage(){
    //     await Task.Run(AssetDatabase.Refresh);
    // }
    public static void startOptimization()
    {
        if (!testModeStatic)
        {
            socket = new SocketNetwork();
            socket.InitSocket();
        }

        if (parallel == true)
        {
            string basePath = "NewScene ";
            for (int i = 0; i < RevisedParetoData.ObjectiveNumber / 5; i++)
            {
                SceneManager.LoadScene(basePath + (i * 5).ToString(), LoadSceneMode.Additive);
            }
        }

    }


    public void updateParameter()
    {
        foreach (var pa in parameters)
        {
            pa.Value.Value = float.Parse(csvData[index][pa.Key].ToString());
        }

        StartCoroutine(ShowIndexChange());

    }

    public void updateParameter(int currentIndex)
    {
        foreach (var pa in parameters)
        {
            pa.Value.Value = float.Parse(csvData[currentIndex][pa.Key].ToString());
        }

        StartCoroutine(ShowIndexChange());

    }
    IEnumerator ShowIndexChange()
    {
        pressStickCount++;
        // finalObjectiveIndex.enabled = true;

        yield return new WaitForSeconds(1);
        pressStickCount--;
        // if (pressStickCount == 0)
        // finalObjectiveIndex.enabled = false;

    }
    public static void setTrial(int trial)
    {
        TrialNumber = trial;
    }

    public static void addParameter(string name, float lowerBound, float upperBound, float step, bool isDiscrete)
    {
        try
        {
            parameters.Add(name, new ParameterArgs(lowerBound, upperBound, (isDiscrete) ? step : 0));
        }
        catch (ArgumentException)
        {
            Debug.LogError(String.Format("An element with Key = {0} already exists.", name), Instance);
        }
    }
    public static void addParameter(string name, float lowerBound, float upperBound, bool isDiscrete)
    {
        int step = 1;
        try
        {
            parameters.Add(name, new ParameterArgs(lowerBound, upperBound, (isDiscrete) ? step : 0));
        }
        catch (ArgumentException)
        {
            Debug.LogError(String.Format("An element with Key = {0} already exists.", name), Instance);
        }
    }
    public static void addParameter(string name, float lowerBound, float upperBound, bool isDiscrete, ref float reference)
    {
        int step = 1;
        try
        {
            parameters.Add(name, new ParameterArgs(lowerBound, upperBound, (isDiscrete) ? step : 0, ref reference));
        }
        catch (ArgumentException)
        {
            Debug.LogError(String.Format("An element with Key = {0} already exists.", name), Instance);
        }
    }
    public static float getParameterValue(string name)
    {
        float value = 0.0f;
        try
        {
            value = parameters[name].Value;

            // if value is upper bound, the algorithm will fail, and no need to do with upperbound
            // bug....need to debug to decomment
            // if(parameters[name].isDiscrete == true && value != parameters[name].upperBound){
            //     float total = (parameters[name].upperBound - parameters[name].lowerBound);
            //     float splitPiece = total / parameters[name].step + 1;
            //     // value form [0, 1]
            //     float partial = (value - parameters[name].lowerBound) / total;
            //     float position = partial / (1 / splitPiece);
            //     value = ((int)position) * parameters[name].step + parameters[name].lowerBound;
            // }
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError(String.Format("Key = {0} is not found.", name), Instance);
        }
        // print($"{name}, {value}");
        return value;
    }
    // setting for parallel show parameters, might not be used for bo toolkit
    public static float getParameterValue(string name, string sceneName)
    {
        float value = 0.0f;
        try
        {
            string[] strArr;
            strArr = sceneName.Split(' ');
            if (strArr[0] == "NewScene")
            {
                value = Convert.ToSingle(csvData[Int32.Parse(strArr[1])][name]);
                print($"in new scene {strArr[1]}");
            }
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError(String.Format("Key = {0} is not found.", name), Instance);
        }
        return value;
    }
    public static ParameterArgs getParameter(string name)
    {
        ParameterArgs value = new ParameterArgs();
        try
        {
            value = parameters[name];
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError(String.Format("Key = {0} is not found.", name), Instance);
        }
        return value;
    }

    public static void addObjective(string name, ObjectiveArgs args)
    {
        try
        {
            objectives.Add(name, args);
        }
        catch (ArgumentException)
        {
            Debug.LogError(String.Format("An element with Key = {0} already exists.", name), Instance);
        }
    }
    public static void addObjective(string name, float lowerBound, float upperBound)
    {
        try
        {
            objectives.Add(name, new ObjectiveArgs(lowerBound, upperBound));
        }
        catch (ArgumentException)
        {
            Debug.LogError(String.Format("An element with Key = {0} already exists.", name), Instance);
        }
    }
    public static void addObjective(string name, float lowerBound, float upperBound, bool smallerIsBetter = false)
    {
        try
        {
            objectives.Add(name, new ObjectiveArgs(lowerBound, upperBound, smallerIsBetter));
        }
        catch (ArgumentException)
        {
            Debug.LogError(String.Format("An element with Key = {0} already exists.", name), Instance);
        }
    }
    public static ObjectiveArgs getObjective(string name)
    {
        ObjectiveArgs value = new ObjectiveArgs();
        try
        {
            value = objectives[name];
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError(String.Format("Key = {0} is not found.", name), Instance);
        }
        return value;
    }
    void AskIfNextIter()
    {
        ShowMenu();
        nextIterQuestion.SetActive(true);
    }
    public void SetNextIter()
    {
        List<float> finalObjectives = new List<float>();
        foreach (KeyValuePair<string, ObjectiveArgs> objective in objectives)
        {
            finalObjectives.Add(objective.Value.GetAvargeObjective());
            objective.Value.clearTrial();
        }
        if (!testMode)
        {
            socket.SendObjectives(finalObjectives);
        }
        reminded = false;
        currentIterNumber++;
        closedInTrialNum = 0;
        canRead = false;
        likertQuestionnaire.SetActive(false);
        HideMenu();
    }
    public void CommitQuestionnaire(Slider slider)
    {
        getObjective(AskQuestion.Peek().Key).addTrial(slider.value);
        AskQuestion.Dequeue();
        SetNextIter();
    }

    public void PressYes()
    {
        nextIterQuestion.SetActive(false);
        foreach (var objective in objectives)
        {
            if (objective.Value.popUp == true)
            {
                AskQuestion.Enqueue(objective);
            }
        }
        if (AskQuestion.Count > 0)
        {
            likertQuestionnaire.SetActive(true);
            reminded = true;
            closedInTrialNum = currentTrialNumber;
            likertQuestionnaire.transform.Find("Question").GetComponent<Text>().text = AskQuestion.Peek().Value.question;
        }
        else
        {
            SetNextIter();
        }

    }
    public void PressNo()
    {
        nextIterQuestion.SetActive(false);
        reminded = true;
        closedInTrialNum = currentTrialNumber;
        HideMenu();
    }
    void TuggleMenu()
    {
        if (menu.activeSelf == true)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }
    public void ShowMenu()
    {
        // idleIcon.SetActive(false);
        // showMsgIcon.SetActive(true);
        menu.SetActive(true);
        line.enabled = true;
        if (testMode)
        {
            SaveParetoCSV.AddData(paretoData.currentIndex, "ShowSlider");
        }
    }
    public void HideMenu()
    {
        // idleIcon.SetActive(true);
        // showMsgIcon.SetActive(false);
        line.enabled = false;
        // HidePerformance();
        menu.SetActive(false);
        if (testMode)
        {
            SaveParetoCSV.AddData(paretoData.currentIndex, "HideSlider");
        }
    }

    // public void ShowPerformance()
    // {
    //     // _webViewPrefab.WebView.Reload();
    //     WaitToExecuteJS();
    //     menu.SetActive(false);
    //     _webViewPrefab.gameObject.SetActive(true);
    //     showPerformance.SetActive(false);
    // }
    // public void HidePerformance()
    // {
    //     //hide menu here temporal setting
    //     menu.SetActive(true);
    //     _webViewPrefab.gameObject.SetActive(false);
    //     showPerformance.SetActive(true);
    // }
    void OnDestroy()
    {
        if (socket != null)
        {
            socket.SocketQuit();
        }
        if (testMode)
        {
            SaveParetoCSV.SaveToFile();
        }
    }
}
