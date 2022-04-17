using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ParetoSlider : MonoBehaviour
{
    Slider slider;
    Text objectiveNumber;
    RectTransform o1Line1;
    RectTransform o1Line2;
    RectTransform o1hline;
    Text o1Text;
    Text o1Min;
    Text o1Max;
    RectTransform o2Line1;
    RectTransform o2Line2;
    RectTransform o2hline;
    Text o2Text;
    float unit;
    float width;
    RevisedParetoData currentData;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        objectiveNumber = transform.parent.parent.Find("ObjectiveNumber").GetComponent<Text>();
        o1Line1 = transform.parent.Find("line1").GetComponent<RectTransform>();
        o1Line2 = transform.parent.Find("line2").GetComponent<RectTransform>();
        o1hline = transform.parent.Find("hline").GetComponent<RectTransform>();
        o1Text = transform.parent.Find("Value").GetComponent<Text>();
        o2Line1 = transform.parent.parent.Find("O2Image/line1").GetComponent<RectTransform>();
        o2Line2 = transform.parent.parent.Find("O2Image/line2").GetComponent<RectTransform>();
        o2hline = transform.parent.parent.Find("O2Image/hline").GetComponent<RectTransform>();
        o2Text = transform.parent.parent.Find("O2Image/Value").GetComponent<Text>();
        currentData = AssetDatabase.LoadAssetAtPath("Assets/GraphEditor/RevisedParetoData.asset", typeof(RevisedParetoData)) as RevisedParetoData;
        // List<Dictionary<string, object>> csvData = currentData.csvData;

        transform.parent.Find("Min").GetComponent<Text>().text = currentData.values[0][0].ToString("f2");
        transform.parent.Find("Max").GetComponent<Text>().text = currentData.values[RevisedParetoData.ObjectiveNumber - 1][0].ToString("f2");

        transform.parent.Find("Name").GetComponent<Text>().text = "Time";
        transform.parent.parent.Find("O2Image/Name").GetComponent<Text>().text = "Overshoot";


        transform.parent.parent.Find("O2Image/Min").GetComponent<Text>().text = currentData.values[RevisedParetoData.ObjectiveNumber - 1][1].ToString("f2");
        transform.parent.parent.Find("O2Image/Max").GetComponent<Text>().text = currentData.values[0][1].ToString("f2");
        width = transform.parent.GetComponent<RectTransform>().sizeDelta.x;
        unit = width / RevisedParetoData.ObjectiveNumber;

        o1hline.localScale = new Vector3(unit, 1, 1);

        slider.maxValue = RevisedParetoData.ObjectiveNumber - 1;
        slider.value = currentData.currentIndex;

        Slider slider2 = transform.parent.parent.Find("O2Image/SliderO2").GetComponent<Slider>();
        slider2.maxValue = RevisedParetoData.ObjectiveNumber - 1;
        slider2.value = currentData.currentIndex;

        Optimizer.Instance.updateParameter(currentData.currentIndex);
        // print($"slider {slider.value}");
        UpdateBlackLine();
    }

    // Update is called once per frame
    void Update()
    {
        // UpdateBlackLine();

    }

    public void OnSliderChageO1()
    {
        currentData.currentIndex = (int)slider.value;
        Optimizer.Instance.updateParameter(currentData.currentIndex);
        UpdateBlackLine();
        SaveParetoCSV.AddData(currentData.currentIndex, "IndexChange");
    }
    public void OnSliderChageO2(float value)
    {
        currentData.currentIndex = (int)(RevisedParetoData.ObjectiveNumber - 1 - value);
        Optimizer.Instance.updateParameter(currentData.currentIndex);
        UpdateBlackLine();
        SaveParetoCSV.AddData(currentData.currentIndex, "IndexChange");
    }

    void UpdateBlackLine()
    {
        objectiveNumber.text = currentData.currentIndex.ToString();
        o1Line1.localPosition = new Vector3(currentData.currentIndex * unit - width / 2, 0, 0);
        o1Line2.localPosition = new Vector3((currentData.currentIndex + 1) * unit - width / 2, 0, 0);
        o1hline.localPosition = new Vector3((currentData.currentIndex + 0.5f) * unit - width / 2, 0, 0);
        o1Text.text = currentData.values[currentData.currentIndex][0].ToString("f2");

        int line2Index = RevisedParetoData.ObjectiveNumber - 1 - currentData.currentIndex;
        o2Line1.localPosition = new Vector3(line2Index * unit - width / 2, 0, 0);
        o2Line2.localPosition = new Vector3((line2Index + 1) * unit - width / 2, 0, 0);
        o2hline.localPosition = new Vector3((line2Index + 0.5f) * unit - width / 2, 0, 0);
        o2Text.text = currentData.values[currentData.currentIndex][1].ToString("f2");
    }

    public void OnStartDrag()
    {
        print("click");
        SaveParetoCSV.AddData(currentData.currentIndex, "StartDrag");
    }

    public void OnEndDrag()
    {
        print("end drag");
        SaveParetoCSV.AddData(currentData.currentIndex, "EndDrag");
    }
}
