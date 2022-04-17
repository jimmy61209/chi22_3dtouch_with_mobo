using UnityEngine;
using OptScourcing;

public class OriginHandler : MonoBehaviour
{
    // public EnvManagerParameter EM;
    public TargetHandler TH;

    void OnTriggerStay(Collider other)
    {
        if(TH.leaveOrigin == false && TH.EM.startIteration == true && other.tag != "Player"){
            transform.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0.2f);
        }
    }
    void OnTriggerExit(Collider other){
        if(TH.leaveOrigin == false && TH.EM.startIteration == true && other.tag != "Player"){
            transform.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.0f);
            ResetTargetColorAndStartTime();
        }
    }
    public void ResetTargetColorAndStartTime()
    {
        TH.leaveOrigin = true;
        TH.trialStartTime = Time.time * 1000;
        TH.longestDisErr = 0;
        TH.transform.Find("mesh").GetComponent<MeshRenderer>().material.color = Color.white;
    }
}
