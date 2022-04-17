using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeLikertSlderValue : MonoBehaviour
{
    // Start is called before the first frame update
    Text mText;
    void Start()
    {
        mText = transform.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeLikertTextValue(float value){
        mText.text = value.ToString();
    }
}
