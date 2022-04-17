using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class RevisedParetoData : ScriptableObject
{

    public Vector2 Middle
    {
        get
        {
            return values[ObjectiveNumber / 2];
        }
    }
    public Vector2[] values = new Vector2[ObjectiveNumber];

    public List<Dictionary<string, object>> csvData = new List<Dictionary<string, object>>();

    public static int ObjectiveNumber
    {
        get
        {
            return OptimizerWizard.revisedObjNum;
        }
    }
    public int currentIndex;

    [MenuItem("Assets/Create/RevisedParetoData")]
    public static void CreateMyAsset()
    {
        RevisedParetoData asset = ScriptableObject.CreateInstance<RevisedParetoData>();

        AssetDatabase.CreateAsset(asset, "Assets/GraphEditor/RevisedParetoData.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [System.Serializable]
    public class InputOutputData
    {
        public Guid id;
        public int index;
    }
}
