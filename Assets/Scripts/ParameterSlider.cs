using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Env3DTouch;
public class ParameterSlider : MonoBehaviour
{
    Text sliderText;
    public Slider slider;
    string parameterName;
    public EnvManagerParameter envManagerParameter;
    Dictionary<string, int> nameIndex = new Dictionary<string, int>() { { "D", 0 }, { "K", 1 }, { "Amplitude", 2 }, { "Gap", 3 } };
    public static bool moved = false;
    public static bool movedForCoverage = false;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        sliderText = transform.parent.Find("Number").GetComponent<Text>();
        parameterName = transform.parent.Find("Name").GetComponent<Text>().text;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateSliderText(float value)
    {
        sliderText.text = value.ToString();
        EnvManagerParameter.x[nameIndex[parameterName]] = slider.value;
        envManagerParameter.UpdateDesignParameters();
        moved = true;
        movedForCoverage = true;
    }
    public void SetSliderValue(float value)
    {
        slider.value = value;
        UpdateSliderText(value);
    }

    public void PlusOne()
    {
        // print("plus");
        print(slider.value);
        if (slider.value < 100)
        {
            UpdateSliderText(++slider.value);
            OnEndDrag();
            moved = true;
            movedForCoverage = true;
        }
    }
    public void MinusOne()
    {
        // print("minus");
        if (slider.value > 0)
        {
            UpdateSliderText(--slider.value);
            OnEndDrag();
            moved = true;
            movedForCoverage = true;
        }
    }
    public void OnEndDrag()
    {
        // print("end drag");
        SaveParameterCSV.AddData(slider.value, parameterName);
        RandomSpawnPoint.UpdateVibrationCollider(ref envManagerParameter.target);

    }
}
