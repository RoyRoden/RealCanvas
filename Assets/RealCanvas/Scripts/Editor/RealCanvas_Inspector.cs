using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

[CustomEditor(typeof(RealCanvas))]
public class RealCanvas_Inspector : Editor
{
    public VisualTreeAsset rcInspectorXML;

    private List<string> deviceNames = new List<string>();

    private RealCanvas script;

    VisualElement rcInspector;

    private bool isPlaying = false;
    private bool isRecognizing = false;

    public override VisualElement CreateInspectorGUI()
    {
        rcInspector = new VisualElement();
        rcInspectorXML.CloneTree(rcInspector);
        script = (RealCanvas)target;

        // add materials
        VisualElement materialsContainer = rcInspector.Q("Materials_Container");
        InspectorElement.FillDefaultInspector(materialsContainer, serializedObject, this);
        for (int i = materialsContainer.childCount-2; i >= 0; i--)
        {
            materialsContainer.Remove(materialsContainer[i]);
        }

        // add list of webcam devices to dropdown field

        DropdownField selectedWebcamDF = rcInspector.Q("Selected_Webcam").GetFirstOfType<DropdownField>();
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            deviceNames.Add(devices[i].name);
        }
        if (deviceNames.Count > 0)
        {
            selectedWebcamDF.choices = deviceNames;
        }

        Button startCamera = rcInspector.Q("Start_Camera").GetFirstOfType<Button>();
        startCamera.clicked += StartCamera_onClick;

        Button resetTransform = rcInspector.Q("Reset_Transform").GetFirstOfType<Button>();
        resetTransform.clicked += ResetTransform_onClick;

        Button canvasRecognition = rcInspector.Q("Canvas_Recognition").GetFirstOfType<Button>();
        canvasRecognition.SetEnabled(false);
        canvasRecognition.clicked += CanvasRecognition_onClick;

        Button warpPerspective = rcInspector.Q("Warp_Perspective").GetFirstOfType<Button>();
        warpPerspective.SetEnabled(false);
        warpPerspective.clicked += WarpPerspective_onClick;

        Button saveTexture = rcInspector.Q("Save_Texture").GetFirstOfType<Button>();
        saveTexture.clicked += SaveTexture_onClick;

        return rcInspector;
    }

    private void StartCamera_onClick()
    {
        Button startCamera = rcInspector.Q("Start_Camera").GetFirstOfType<Button>();
        Button canvasRecognition = rcInspector.Q("Canvas_Recognition").GetFirstOfType<Button>();
        Button warpPerspective = rcInspector.Q("Warp_Perspective").GetFirstOfType<Button>();
        if (!isPlaying)
        {
            script.StartCamera();        
            startCamera.style.backgroundColor = Color.red;
            startCamera.text = "Stop Camera";
            isPlaying = true;
            //isRecognizing = false;
            canvasRecognition.SetEnabled(true);
            warpPerspective.SetEnabled(true);
        } else
        {
            script.StopCamera();
            startCamera.style.backgroundColor = Color.black;
            startCamera.text = "Start Camera";
            isPlaying = false;
            isRecognizing = false;
            canvasRecognition.SetEnabled(false);
            warpPerspective.SetEnabled(false);
            canvasRecognition.style.backgroundColor = Color.black;
            canvasRecognition.text = "Start Canvas Detection";
        }
    }


    private void CanvasRecognition_onClick()
    {
        Button canvasRecognition = rcInspector.Q("Canvas_Recognition").GetFirstOfType<Button>();
        if (!isRecognizing)
        {
            script.StartCanvasRecognition();
            canvasRecognition.style.backgroundColor = Color.red;
            canvasRecognition.text = "Stop Canvas Detection";
            isRecognizing = true;
        }
        else
        {
            script.StopCanvasRecognition();
            canvasRecognition.style.backgroundColor = Color.black;
            canvasRecognition.text = "Start Canvas Detection";
            isRecognizing = false;
        }
    }

    private void WarpPerspective_onClick()
    {
        Button canvasRecognition = rcInspector.Q("Canvas_Recognition").GetFirstOfType<Button>();
        canvasRecognition.style.backgroundColor = Color.black;
        isRecognizing = false;
        canvasRecognition.text = "Start Canvas Detection";
        script.WarpPerspective();
    }

    private void SaveTexture_onClick()
    {
        script.SaveTexture();
    }    
    
    private void ResetTransform_onClick()
    {
        script.ResetTransform();
    }
}