using UnityEngine;
namespace Env3DTouch
{
public class GogoController
{
    public float D = 0.4f;
    public float K = 0.2f;
    public void UpdateHandPosition(Transform origin, Transform target, Transform realHand, Transform currentController){
        float dist = Vector3.Distance(origin.position, realHand.position);
        if(dist > D){
            Vector3 direction = realHand.position - origin.position;
            Vector3.Normalize(direction);
            Vector3 worlddir = currentController.InverseTransformDirection(direction);
            float cmBase = Mathf.Pow((dist - D) * 100, 2) * K;
            //to advoid too far from origin
            if(cmBase > 500000){
                cmBase = 500000;
            }
            currentController.localPosition = worlddir * cmBase / 100;
        }
        else{
            currentController.localPosition = new Vector3(0, 0, 0);
        }
    }

    public void SetGogoParmeter(float mD, float mK){
        D = mD;
        K = mK;
    }
}
}