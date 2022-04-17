using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Pugrad;
using System.IO;
using System;

public class OptimizerWizard : EditorWindow
{
    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;
    ObjectField currentScene;

    static int objNum = 0;
    public static int revisedObjNum = 11;
    static List<Dictionary<string, object>> csvData;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/OptimizerWizard %g")]
    public static void ShowWindow()
    {
        // Opens the window, otherwise focuses it if it’s already open.
        var window = GetWindow<OptimizerWizard>();

        // Adds a title to the window.
        window.titleContent = new GUIContent("Pareto Slider");

        // Sets a minimum size to the window.
        window.minSize = new Vector2(720, 200);

        csvData = CSVReader.Read("global_pareto");
        objNum = csvData.Count;

        // change origin data to revisedData
        Vector2[] originPoints = new Vector2[csvData.Count];
        for (int i = 0; i < csvData.Count; i++)
        {
            originPoints[i] = new Vector2(Convert.ToSingle(csvData[i]["o1"]), Convert.ToSingle(csvData[i]["o2"]));
        }
        // Vector2[] points = BezierApproximation.GetBezierApproximation(originPoints, 1024);
        // Debug.Log(points.Length);  


        RevisedParetoData currentData = AssetDatabase.LoadAssetAtPath("Assets/GraphEditor/RevisedParetoData.asset", typeof(RevisedParetoData)) as RevisedParetoData;


        /////////////////////////////////////////////////// start resample ///////////////////////////////////////////////////
        float totalLength = 0.0f;
        for (int i = 1; i < originPoints.Length; i++)
        {
            totalLength += (originPoints[i - 1] - originPoints[i]).magnitude;
        }
        // cut for 11 pieces, set objective values
        float unitLength = totalLength / revisedObjNum;
        float currentLength = 0.0f;
        int currentPiece = 1;
        currentData.values[0] = originPoints[0];
        for (int i = 1; i < originPoints.Length; i++)
        {
            currentLength += (originPoints[i - 1] - originPoints[i]).magnitude;
            while (currentLength >= unitLength * currentPiece)
            {
                float t = (currentLength - unitLength * currentPiece) / (originPoints[i - 1] - originPoints[i]).magnitude;
                // Debug.Log($"i:{i} currentLength:{currentLength} unitLength:{unitLength * currentPiece} currentPiece:{currentPiece} t:{t}");
                currentData.values[currentPiece] = Vector2.Lerp(originPoints[i - 1], originPoints[i], 1 - t);
                currentPiece++;
            }
        }
        Debug.Log(currentPiece);
        currentData.values[revisedObjNum - 1] = originPoints[csvData.Count - 1];

        for (int i = 1; i < revisedObjNum; i++)
        {
            Debug.Log((currentData.values[i - 1] - currentData.values[i]).magnitude);
        }
        ////////////////////////////////////////////////////// end resample /////////////////////////////////////////////////////

        ////////////////////////////////////////////////////// start origin /////////////////////////////////////////////////////
        // currentData.values = originPoints;
        // revisedObjNum = csvData.Count;
        //////////////////////////////////////////////////////// end origin //////////////////////////////////////////////////////
        // currentData.csvData = csvData;
        

        Texture2D pointsTexture = new Texture2D(700, 700);
        for (int i = 0; i < originPoints.Length; i++)
        {
            pointsTexture.SetPixel((int)(originPoints[i].x * 1000), (int)(originPoints[i].y * 1000), new Color(0, 0, 1, 1));

        }
        for (int i = 0; i < revisedObjNum; i++)
        {
            pointsTexture.SetPixel((int)(currentData.values[i].x * 1000), (int)(currentData.values[i].y * 1000), new Color(1, 0, 0, 1));

        }
        var pngColorMap2 = pointsTexture.EncodeToPNG();

        File.WriteAllBytes("Assets/Resources/points.png", pngColorMap2);

        pointsTexture = new Texture2D(700, 700);
        for (int i = 0; i < originPoints.Length; i++)
        {
            pointsTexture.SetPixel((int)(originPoints[i].x * 1000), (int)(originPoints[i].y * 1000), new Color(0, 0, 1, 1));

        }
        pngColorMap2 = pointsTexture.EncodeToPNG();

        File.WriteAllBytes("Assets/Resources/points-origin.png", pngColorMap2);

        pointsTexture = new Texture2D(700, 700);
        for (int i = 0; i < revisedObjNum; i++)
        {
            pointsTexture.SetPixel((int)(currentData.values[i].x * 1000), (int)(currentData.values[i].y * 1000), new Color(1, 0, 0, 1));

        }
        pngColorMap2 = pointsTexture.EncodeToPNG();

        File.WriteAllBytes("Assets/Resources/points-new.png", pngColorMap2);


        // set parameter values
        // currentData.parameters[0] = originPoints[0];
        // for (int i = 1; i < originPoints.Length; i++)
        // {
        //     currentLength += (originPoints[i - 1] - originPoints[i]).magnitude;
        //     while (currentLength >= unitLength * currentPiece)
        //     {
        //         float t = (currentLength - unitLength * currentPiece) / (originPoints[i - 1] - originPoints[i]).magnitude;
        //         currentData.values[currentPiece] = Vector2.Lerp(originPoints[i - 1], originPoints[i], t);
        //         currentPiece++;
        //     }
        // }
        // currentData.values[10] = originPoints[csvData.Count - 1];


        // get full colormap
        Color[] preColormap = TurboColormap.Generate(1050, 10);
        Color[] colormap = new Color[1024 * 10];
        for (int i = 0; i < 10; i++)
        {
            Array.Copy(preColormap, i * 1024 + 26 * (i + 1), colormap, i * 1024, 1024);
        }
        // Color [][] newColormap = new Color [10][];
        // for(int i = 0; i < 10; i++){
        //     newColormap[i] = colormap;
        // }

        var texture = new Texture2D(1024, 10);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colormap);
        texture.Apply();

        float o1ValueMin = Convert.ToSingle(csvData[0]["o1"]);
        float o1ValueMax = Convert.ToSingle(csvData[csvData.Count - 1]["o1"]);

        bool[] flip = { false, false };

        if (o1ValueMin > o1ValueMax)
        {
            (o1ValueMin, o1ValueMax) = (o1ValueMax, o1ValueMin);
            flip[0] = true;
        }

        float o2ValueMin = Convert.ToSingle(csvData[0]["o2"]);
        float o2ValueMax = Convert.ToSingle(csvData[csvData.Count - 1]["o2"]);

        if (o2ValueMin > o2ValueMax)
        {
            (o2ValueMin, o2ValueMax) = (o2ValueMax, o2ValueMin);
            flip[1] = true;
        }

        //reset the full colormap maping (-1, 1) to the range we obtains
        float min = o1ValueMin > o2ValueMin ? o2ValueMin : o1ValueMin;
        float max = o1ValueMax < o2ValueMax ? o2ValueMax : o1ValueMax;
        // Debug.Log($"max {NormalToTextureCor(max)} min {NormalToTextureCor(min)} minus {NormalToTextureCor(max - min)}");
        // Texture2D FinalTexture = new Texture2D(NormalToTextureCor(max) - NormalToTextureCor(min), 10);

        // Color[] finalColor = texture.GetPixels(NormalToTextureCor(min), 0, NormalToTextureCor(max) - NormalToTextureCor(min), 10);
        // Texture2D finalTexture = new Texture2D(NormalToTextureCor(max) - NormalToTextureCor(min), 10);
        // finalTexture.SetPixels(finalColor);
        // finalTexture.Apply();

        int o1Offset = (int)Remap(o1ValueMin, min, max, 0, 1024);
        int o1Length = (int)Remap(o1ValueMax, min, max, 0, 1024) - o1Offset;

        // 2D array get from full colormap
        Color[] o1Color = texture.GetPixels(o1Offset, 0, o1Length, 10);
        Texture2D o1Texture = new Texture2D(1024, 10);
        // flip if the order is decending
        if (flip[0] == true)
        {
            Array.Reverse(o1Color);
        }
        // o1Texture.SetPixels(o1Color);
        // o1Texture.Apply();

        // get the 11 colors of every blocks
        Color[] colorBlocks = new Color[revisedObjNum];
        for (int i = 0; i < revisedObjNum; i++)
        {
            colorBlocks[i] = o1Color[(int)((currentData.values[i].x - o1ValueMin) / (o1ValueMax - o1ValueMin) * (o1Length - 1))];
        }

        Color[] newO1Color = new Color[(1024) * 10];
        for (int i = 0; i < 1024; i++)
        {
            int colorNum = (int)(((float)i / (1024)) * revisedObjNum);
            for (int j = 0; j < 10; j++)
            {
                newO1Color[j * (1024) + i] = colorBlocks[colorNum];
            }
        }
        o1Texture.SetPixels(newO1Color);
        o1Texture.Apply();

        var pngColorMap = o1Texture.EncodeToPNG();

        File.WriteAllBytes("Assets/Resources/colormap_o1.png", pngColorMap);


        int o2Offset = (int)Remap(o2ValueMin, min, max, 0, 1024);
        int o2Length = (int)Remap(o2ValueMax, min, max, 0, 1024) - o2Offset;


        Color[] o2Color = texture.GetPixels(o2Offset, 0, o2Length, 10);
        Texture2D o2Texture = new Texture2D(1024, 10);
        // flip so that the colormap will be accending
        if (flip[1] == true)
        {
            Array.Reverse(o2Color);
        }
        // o2Texture.SetPixels(o2Color);
        // o2Texture.Apply();

        for (int i = 0; i < revisedObjNum; i++)
        {
            colorBlocks[i] = o2Color[(int)((currentData.values[i].y - o2ValueMin) / (o2ValueMax - o2ValueMin) * (o2Length - 1))];
            // Debug.Log((int)((currentData.values[i].y - o2ValueMin) / (o2ValueMax - o2ValueMin) * (o2Length - 1)));
        }

        Color[] newO2Color = new Color[(1024) * 10];
        for (int i = 0; i < 1024; i++)
        {
            int colorNum = (int)(((float)i / (1024)) * revisedObjNum);
            for (int j = 0; j < 10; j++)
            {
                newO2Color[j * (1024) + i] = colorBlocks[colorNum];
            }
        }
        o2Texture.SetPixels(newO2Color);
        o2Texture.Apply();

        pngColorMap = o2Texture.EncodeToPNG();

        File.WriteAllBytes("Assets/Resources/colormap_o2.png", pngColorMap);

        Texture2D temp = new Texture2D(o2Length, 10);
        temp.SetPixels(o2Color);
        temp.Apply();

        pngColorMap = temp.EncodeToPNG();

        File.WriteAllBytes("Assets/Resources/colormap_o2_old.png", pngColorMap);

        Texture2D temp2 = new Texture2D(1024, 10);
        temp2.SetPixels(colormap);
        temp2.Apply();

        pngColorMap = temp2.EncodeToPNG();

        File.WriteAllBytes("Assets/Resources/colormap.png", pngColorMap);



        //plot debug img
        Color[] debugImg = new Color[256 * 256];

        for (int i = 0; i < objNum; i++)
        {
            debugImg[(int)(currentData.values[i].x * 100 + currentData.values[i].y * 100 * 256)] = new Color(0, 0, i / objNum, 1);
            debugImg[(int)(originPoints[i].x * 100 + originPoints[i].y * 100 * 256)] = new Color(i / objNum, 0, 0, 1);
        }

        Texture2D debugTexture = new Texture2D(256, 256);
        debugTexture.SetPixels(debugImg);
        debugTexture.Apply();

        pngColorMap = debugTexture.EncodeToPNG();

        File.WriteAllBytes("Assets/Resources/debug.png", pngColorMap);


        AssetDatabase.Refresh();



    }

    [MenuItem("Assets/Create/ParetoShowData")]
    public static void CreateMyAsset()
    {
        ParetoShowData asset = ScriptableObject.CreateInstance<ParetoShowData>();

        AssetDatabase.CreateAsset(asset, "Assets/GraphEditor/ParetoShowData.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    static int NormalToTextureCor(float normal)
    {
        return (int)((normal + 1) * (1024 / 2));
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    private void OnGUI()
    {
        var root = rootVisualElement;
        Image o1Img = root.Q<Image>("o1-img");
        SliderInt o1Slider = root.Q<SliderInt>("o1-slider");
        // FloatField o1Value = root.Q<FloatField>("o1-value");

        SliderInt o2Slider = root.Q<SliderInt>("o2-slider");
        // FloatField o2Value = root.Q<FloatField>("o2-value");

        // TextElement o1Text = root.Q<TextElement>("o1-value");
        // TextElement o2Text = root.Q<TextElement>("o2-value");
        // RevisedParetoData currentData;

        // currentData = AssetDatabase.LoadAssetAtPath("Assets/GraphEditor/RevisedParetoData.asset", typeof(RevisedParetoData)) as RevisedParetoData;

        float unitWidth = o1Img.resolvedStyle.width / revisedObjNum;

        VisualElement o1Line1, o1Line2, o2Line1, o2Line2;
        VisualElement o1Line3, o1Line4, o2Line3, o2Line4;
        o1Line1 = root.Q<VisualElement>("o1-line1");
        o1Line2 = root.Q<VisualElement>("o1-line2");
        o2Line1 = root.Q<VisualElement>("o2-line1");
        o2Line2 = root.Q<VisualElement>("o2-line2");

        o1Line3 = root.Q<VisualElement>("o1-line3");
        o1Line4 = root.Q<VisualElement>("o1-line4");
        o2Line3 = root.Q<VisualElement>("o2-line3");
        o2Line4 = root.Q<VisualElement>("o2-line4");

        o1Line1.style.left = root.Q<VisualElement>("o1-label").resolvedStyle.width + o1Slider.value * unitWidth;
        o1Line2.style.left = root.Q<VisualElement>("o1-label").resolvedStyle.width + (o1Slider.value + 1) * unitWidth;
        o2Line1.style.left = root.Q<VisualElement>("o2-label").resolvedStyle.width + o2Slider.value * unitWidth;
        o2Line2.style.left = root.Q<VisualElement>("o2-label").resolvedStyle.width + (o2Slider.value + 1) * unitWidth;

        o1Line3.style.left = o1Line1.style.left;
        o1Line4.style.left = o1Line1.style.left;
        o2Line3.style.left = o2Line1.style.left;
        o2Line4.style.left = o2Line1.style.left;

        o1Line3.style.width = unitWidth + o1Line3.resolvedStyle.height;
        o1Line4.style.width = unitWidth + o1Line4.resolvedStyle.height;
        o2Line3.style.width = unitWidth + o2Line3.resolvedStyle.height;
        o2Line4.style.width = unitWidth + o2Line4.resolvedStyle.height;


        // root.Q<VisualElement>("o2-line3").style.left = (-o1Img.resolvedStyle.width) + o2Slider.value * unitWidth;
        // root.Q<VisualElement>("o2-line4").style.left = (-3 - o1Img.resolvedStyle.width) + (o2Slider.value + 1) * unitWidth;

        root.Q<TextElement>("o1-min").style.left = (5 - o1Img.resolvedStyle.width);
        root.Q<TextElement>("o1-max").style.left = -8 - root.Q<TextElement>("o1-min").resolvedStyle.width - root.Q<TextElement>("o1-max").resolvedStyle.width;
        root.Q<TextElement>("o2-min").style.left = (5 - o1Img.resolvedStyle.width);
        root.Q<TextElement>("o2-max").style.left = -8 - root.Q<TextElement>("o1-min").resolvedStyle.width - root.Q<TextElement>("o2-max").resolvedStyle.width;

        root.Q<TextElement>("o1-value").style.left = -root.Q<TextElement>("o1-min").resolvedStyle.width - root.Q<TextElement>("o1-max").resolvedStyle.width;
        root.Q<TextElement>("o2-value").style.left = -root.Q<TextElement>("o2-min").resolvedStyle.width - root.Q<TextElement>("o2-max").resolvedStyle.width;

    }


    private void OnEnable()
    {
        // Reference to the root of the window.
        var root = rootVisualElement;

        // Associates a stylesheet to our root. Thanks to inheritance, all root’s
        // children will have access to it.
        // root.styleSheets.Add(Resources.Load<StyleSheet>("QuickTool_Style"));

        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIElements/OptWizardWelcome.uxml");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UIElements/OptWizardWelcome.uss");
        // quickToolVisualTree.CloneTree(root);
        VisualElement labelFromUXML = visualTree.CloneTree();
        //5.为UXML添加USS样式
        labelFromUXML.styleSheets.Add(styleSheet);
        //6.将子节点添加到根节点
        root.Add(labelFromUXML);

        currentScene = root.Q<ObjectField>("scene");
        currentScene.objectType = typeof(SceneAsset);

        var button = root.Q<Button>("evaluation");
        if (button != null) button.clicked += GenerateScenes;

        Image o1Img = root.Q<Image>("o1-img");
        SliderInt o1Slider = root.Q<SliderInt>("o1-slider");
        FloatField o1Value = root.Q<FloatField>("o1-value-field");

        SliderInt o2Slider = root.Q<SliderInt>("o2-slider");
        FloatField o2Value = root.Q<FloatField>("o2-value-field");

        TextElement o1Text = root.Q<TextElement>("o1-value");
        TextElement o2Text = root.Q<TextElement>("o2-value");


        RevisedParetoData currentData;

        currentData = AssetDatabase.LoadAssetAtPath("Assets/GraphEditor/RevisedParetoData.asset", typeof(RevisedParetoData)) as RevisedParetoData;

        if (objNum == 0 || csvData == null)
        {
            csvData = CSVReader.Read("global_pareto");
            objNum = csvData.Count;
        }
        root.Q<TextElement>("o1-min").text = currentData.values[0].x.ToString("f2");
        root.Q<TextElement>("o1-max").text = currentData.values[revisedObjNum - 1].x.ToString("f2");
        root.Q<TextElement>("o2-min").text = currentData.values[revisedObjNum - 1].y.ToString("f2");
        root.Q<TextElement>("o2-max").text = currentData.values[0].y.ToString("f2");

        o1Text.text = currentData.values[o1Slider.value].x.ToString("f2");
        o2Text.text = currentData.values[o1Slider.value].y.ToString("f2");

        o1Slider.lowValue = 0;
        o1Slider.highValue = revisedObjNum - 1;

        o1Slider.RegisterValueChangedCallback(evt =>
        {
            o1Value.value = (float)evt.newValue / (revisedObjNum - 1);
            o2Slider.value = (revisedObjNum - 1) - evt.newValue;
            o1Text.text = currentData.values[o1Slider.value].x.ToString("f2");
            o2Text.text = currentData.values[o1Slider.value].y.ToString("f2");
            currentData.currentIndex = o1Slider.value;
            float unitWidth = o1Img.resolvedStyle.width / revisedObjNum;
            // root.Q<VisualElement>("o1-line1").style.left = (-o1Img.resolvedStyle.width) + o1Slider.value * unitWidth;
            // root.Q<VisualElement>("o1-line2").style.left = (-2 - o1Img.resolvedStyle.width) + (o1Slider.value + 1) * unitWidth;
        });
        // o1Value.RegisterValueChangedCallback(evt =>
        // {
        //     int newValue = (int)Math.Round(evt.newValue * 10);
        //     if (newValue > revisedObjNum - 1)
        //     {
        //         newValue = revisedObjNum - 1;
        //     }
        //     else if (newValue < 0)
        //     {
        //         newValue = 0;
        //     }
        //     o1Slider.value = newValue;
        // });

        o2Slider.lowValue = 0;
        o2Slider.highValue = revisedObjNum - 1;
        o2Value.value = 1;
        o2Slider.value = revisedObjNum - 1;

        o2Slider.RegisterValueChangedCallback(evt =>
        {
            o2Value.value = (float)evt.newValue / (revisedObjNum - 1);
            o1Slider.value = (revisedObjNum - 1) - evt.newValue;
            // o2Text.text = currentData.values[o1Slider.value].y.ToString("f4");
            float unitWidth = o1Img.resolvedStyle.width / revisedObjNum;
            // root.Q<VisualElement>("o2-line1").style.left = (-o1Img.resolvedStyle.width) + o2Slider.value * unitWidth;
            // root.Q<VisualElement>("o2-line2").style.left = (-2 - o1Img.resolvedStyle.width) + (o2Slider.value + 1) * unitWidth;
        });
        // o2Value.RegisterValueChangedCallback(evt =>
        // {
        //     int newValue = (int)Math.Round(evt.newValue * 10);
        //     if (newValue > revisedObjNum - 1)
        //     {
        //         newValue = revisedObjNum - 1;
        //     }
        //     else if (newValue < 0)
        //     {
        //         newValue = 0;
        //     }
        //     o2Slider.value = newValue;
        // });



        // Queries all the buttons (via type) in our root and passes them
        // in the SetupButton method.
        // var toolButtons = root.Query<Button>();
        // toolButtons.ForEach(SetupButton);
    }



    void GenerateScenes()
    {
        List<Dictionary<string, object>> csvData = CSVReader.Read("global_pareto");
        for (int i = 0; i < csvData.Count; i++)
        {
            bool sussess = AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(currentScene.value), $"Assets/Optimizer/OptScenes/NewScene {i}.unity");
        }
        AssetDatabase.SaveAssets();
    }
}
