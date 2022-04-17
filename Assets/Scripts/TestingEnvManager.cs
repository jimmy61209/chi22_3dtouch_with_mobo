using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingEnvManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Optimizer.addParameter("D", 0.3f, 1f, Optimizer.DISCRETE);
        Optimizer.addParameter("K", 0f, 0.5f, Optimizer.CONTINOUS);
        Optimizer.addParameter("Amplitude", 30f, 100f, Optimizer.DISCRETE);
        Optimizer.addParameter("Gap", -5f, 15f, Optimizer.CONTINOUS);
    
        

        Optimizer.addObjective("Time", 900f, 1600f, Optimizer.BIGGER_IS_BETTER);
        Optimizer.addObjective("Error", 0f, 10f, Optimizer.SMALLER_IS_BETTER);
    }

    void GetParameterValues(){
        // float D = Optimizer.getParameterValue("D");
    }

    void SetObjectiveValues(List<float> objectives){
        // Optimizer.getObjective("Time").addTrial(objectives[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


