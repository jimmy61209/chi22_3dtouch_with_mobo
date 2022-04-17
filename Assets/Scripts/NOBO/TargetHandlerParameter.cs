using UnityEngine;
using System.Collections.Generic;
using System;
using OptScourcing;

public class TargetHandlerParameter : MonoBehaviour
{
    public EnvManagerParameter EM;
    // public event UnitTaskFinishDelegate OnFinishUnitTaskEvent;
    public delegate void UnitTaskFinishDelegate(List<float> objectives);
    public float trialStartTime;
    public float longestDisErr;
    public float dwellCount;
    public bool leaveOrigin;
    public string sendStr = "";
    bool trialFailBool;

    void Update()
    {
        //if didn't finish task in 3 second
        if(Time.time - trialStartTime/1000 >= 3 && leaveOrigin){
            trial_fail();
        }
        //update the largest error of a single task
        if(Vector3.Distance(EM.currentController.position, EM.origin.position) > Vector3.Distance(transform.position, EM.origin.position) &&
        Vector3.Distance(EM.currentController.position, EM.origin.position) - Vector3.Distance(transform.position, EM.origin.position) > longestDisErr){
            longestDisErr = Vector3.Distance(EM.currentController.position, EM.origin.position) - Vector3.Distance(transform.position, EM.origin.position);
        }
    }


    void OnTriggerStay(Collider other)
    {
        if(dwellCount == 0 && leaveOrigin){
            gameObject.transform.Find("mesh").GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
        if(leaveOrigin){
            dwellCount += Time.deltaTime;
            if(dwellCount >= 0.5f && gameObject.transform.Find("mesh").GetComponent<MeshRenderer>().material.color == Color.yellow){
                trialFailBool = false;
                gameObject.transform.Find("mesh").GetComponent<MeshRenderer>().material.color = Color.white;
                // sendStr = (Time.time * 1000 - trialStartTime).ToString("0.00") + "," + (longestDisErr * 100).ToString("0.00");
                float o1 = (float)Math.Round((Decimal)(Time.time * 1000 - trialStartTime), 3, MidpointRounding.AwayFromZero);
                float o2 = (float)Math.Round((Decimal)(longestDisErr * 100), 3, MidpointRounding.AwayFromZero);
                List<float> objectives = new List<float>(new float[] {o1,o2});
                OnFinishUnitTask(objectives);

                EM.origin.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.2f);
                leaveOrigin = false;
            }
        }
    }

    void OnTriggerExit(Collider other){
        if(gameObject.transform.Find("mesh").GetComponent<MeshRenderer>().material.color == Color.yellow)
            gameObject.transform.Find("mesh").GetComponent<MeshRenderer>().material.color = Color.white;
        dwellCount = 0;
    }
    void trial_fail(){
        if(trialFailBool == true){
            List<float> objectives = new List<float>(new float[] {5000, 50});
            // sendStr = (10000).ToString() + "," + (100).ToString();
            trialFailBool = false;
            OnFinishUnitTask(objectives);
        }
        else{
            trialFailBool = true;
        }
        leaveOrigin = false;
        EM.origin.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.2f);
    }
    void OnFinishUnitTask(List<float> objectives)
    {
        // OnFinishUnitTaskEvent?.Invoke(objectives);
        // IM.mState = InteractionManager.State.finishUnitTask;
        transform.Find("mesh").GetComponent<MeshRenderer>().enabled = false;
        transform.position = new Vector3(0, 0, 0);
        EM.OnFinishUnitTask(objectives);
    }
}
