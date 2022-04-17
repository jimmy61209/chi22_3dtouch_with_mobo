using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class handPosition : MonoBehaviour
{
    Transform leftHandAnchor;
    Transform rightHandAnchor;
    // Start is called before the first frame update
    void Start()
    {
        leftHandAnchor = transform.Find("LeftHandAnchor");
        rightHandAnchor = transform.Find("RightHandAnchor");


    }

    // Update is called once per frame
    void Update()
    {
        leftHandAnchor.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        rightHandAnchor.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
    }
}
