using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LikertScale : ObjectiveArgs
{
    public LikertScale(int points, string question): base(1, points, Optimizer.BIGGER_IS_BETTER, true, question){
        
    }
}
