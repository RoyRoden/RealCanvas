using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace RealCanvas
{
    public class RuntimeUI : MonoBehaviour
    {
        private VisualElement root;

        private DropdownField webcamDropdown;
        private DropdownField resolutionDropdown;
        private DropdownField fpsDropdown;
        private DropdownField renderedStepDropdown;

        private Button startCamera;
        private Button resetTransform;
        private Button canvasDetection;
        private Button warpPerspective;
        private Button saveTexture;
        private Button generateVariation;

        private bool isStartBtnOn = false;
        private bool isDetectionBtnOn = false;

        private MainController mainController;

        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            webcamDropdown = root.Q<DropdownField>("webcam");
            resolutionDropdown = root.Q<DropdownField>("resolution");
            fpsDropdown = root.Q<DropdownField>("fps");
            renderedStepDropdown = root.Q<DropdownField>("step");

            startCamera = root.Q<Button>("start");
            startCamera.clicked += StartCamera_onClick;

            resetTransform = root.Q<Button>("reset");
            resetTransform.clicked += ResetTransform_onClick;

            canvasDetection = root.Q<Button>("detection");
            canvasDetection.SetEnabled(false);
            canvasDetection.clicked += CanvasDetection_onClick;

            warpPerspective = root.Q<Button>("warp");
            warpPerspective.SetEnabled(false);
            warpPerspective.clicked += WarpPerspective_onClick;

            saveTexture = root.Q<Button>("save");
            saveTexture.clicked += SaveTexture_onClick;

            generateVariation = root.Q<Button>("variation");
            generateVariation.clicked += GenerateVariation_onClick;
        }

        public string Webcam => webcamDropdown.value;

        public string RequestedResolution => resolutionDropdown.value;

        public int RequestedFPS => int.Parse(fpsDropdown.value);

        public int Step => renderedStepDropdown.index;

        public int BlurKSize => root.Q<SliderInt>("blurKSize").value;

        public int Threshold => root.Q<SliderInt>("threshold").value;

        public int ThresholdMax => root.Q<SliderInt>("thresholdMax").value;

        public float EpsilonFactor => root.Q<Slider>("epsilonFactor").value;

        public float AreaFactor => root.Q<Slider>("areaFactor").value;

        private void Update()
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                ToggleVisibility();
            }
        }

        public void ConnectMainController(MainController mainController)
        {
            this.mainController = mainController;
        }

        public void UpdateWebcamDropdown(List<string> webcamNames)
        {
            webcamDropdown.choices = webcamNames;
            webcamDropdown.index = 0;
        }

        private void ToggleVisibility()
        {
            root.visible = !root.visible;
        }

        private void StartCamera_onClick()
        {
            isStartBtnOn = !isStartBtnOn;

            canvasDetection.SetEnabled(isStartBtnOn);
            warpPerspective.SetEnabled(isStartBtnOn);

            UpdateButtonState(startCamera, isStartBtnOn, "Stop Camera", "Start Camera", mainController.StartCamera, mainController.StopCamera);
            UpdateButtonState(canvasDetection, false, "Stop Canvas Detection", "Start Canvas Detection", null, mainController.StopCanvasRecognition);          
        }


        private void CanvasDetection_onClick()
        {
            isDetectionBtnOn = !isDetectionBtnOn;
            UpdateButtonState(canvasDetection, isDetectionBtnOn, "Stop Canvas Detection", "Start Canvas Detection", mainController.StartCanvasRecognition, mainController.StopCanvasRecognition);
        }

        private void WarpPerspective_onClick()
        {
            UpdateButtonState(canvasDetection, false, "Stop Canvas Detection", "Start Canvas Detection", null, mainController.StopCanvasRecognition);
            mainController.WarpPerspective();
        }

        private void SaveTexture_onClick()
        {
            mainController.SaveTexture();
        }

        private void ResetTransform_onClick()
        {
            mainController.ResetTransform();
        }

        private void GenerateVariation_onClick()
        {
            mainController.GenerateVariation();
        }

        private void UpdateButtonState(Button button, bool isOn, string onText, string offText, System.Action onAction, System.Action offAction)
        {
            if (isOn)
            {
                button.style.backgroundColor = Color.red;
                button.text = onText;
                onAction?.Invoke();
            }
            else
            {
                button.style.backgroundColor = Color.black;
                button.text = offText;
                offAction?.Invoke();
            }
        }
    }
}
