﻿using UnityEngine;
using System.IO.Ports;
using System.Threading;
using OptScourcing;

public class VibrationHandlerParameter : MonoBehaviour
{
    public EnvManagerParameter EM;
    public float vibStartDegree = 0.1f;
    public float vibrateOffset = 0;
    public string comName = "COM23";
    SerialPort sp;
    // Start is called before the first frame update
    void Start()
    {
        new Thread(Open).Start();
    }

    void Open()
    {
        sp = new SerialPort(comName, 9600);
        sp.Open();
        print("Open serial port");
    }
    void OnApplicationQuit()
    {
        sp.Close();
        print("Close serial port");
    }

    // Update is called once per frame
    void Update()
    {
        //vibrate!
        if (Input.GetKeyDown(KeyCode.V))
        {
            sp.WriteLine("v");
            print("pressed v");
        }
        //update vibration paramerer to arduino
        if (Input.GetKeyDown(KeyCode.I))
        {
            upateArduinoVibPar();
            print("pressed i");
        }
        //reconnect serial port
        if (Input.GetKeyDown(KeyCode.C))
        {
            sp.Close();
            new Thread(Open).Start();
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (EM != null)
        {
            if (EM.startTrial)
            {
                sp.WriteLine("v");
            }
        }
        else
        {
            sp.WriteLine("v");
        }


    }

    //4 design parameter
    public void setVibratePar(float start_degree, float offset)
    {
        vibStartDegree = (int)start_degree;
        vibrateOffset = offset;
        upateArduinoVibPar();
    }
    void upateArduinoVibPar()
    {
        if (sp != null)
        {
            sp.WriteLine("a" + vibStartDegree.ToString() + "l");
        }
    }
    public void updateCollider(float nowSize)
    {
        float tmpSize = nowSize + vibrateOffset;
        if (tmpSize < 0) tmpSize = 0;
        transform.GetComponent<BoxCollider>().size = new Vector3(tmpSize, tmpSize, tmpSize);
    }

    private void OnDestroy()
    {
        OnApplicationQuit();
    }
}
