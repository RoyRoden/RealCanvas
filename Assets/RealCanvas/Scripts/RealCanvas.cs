using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealCanvas
{
    [ExecuteAlways]
    public class RealCanvas : MonoBehaviour
    {

        #region Serialized Fields

        [Tooltip("Name of the Webcam Device")]
        [SerializeField, Delayed]
        private string deviceName = "Select a Webcam Device";

        [Tooltip("Width of the video")]
        [SerializeField]
        private int videoWidth = 1280;

        [Tooltip("Height of the video")]
        [SerializeField]
        private int videoHeight = 720;

        [Tooltip("Frames per second")]
        [SerializeField]
        private int videoFPS = 30;

        private enum steps { Gray, Blur, Threshold, Contour, Poly };
        [SerializeField]
        private steps step = steps.Poly;

        [Tooltip("Blur Kernel Size")]
        [SerializeField, Range(1, 25)]
        private int blurKSize = 4;

        [Tooltip("Threshold Value")]
        [SerializeField, Range(0, 255)]
        private int threshValue = 127;

        [Tooltip("Threshold Maximum")]
        [SerializeField, Range(0, 255)]
        private int threshMax = 255;

        [Tooltip("Epsilon Factor for the contour approximation")]
        [SerializeField, Range(0, 1)]
        private double epsilonFactor = 0.08;

        [Tooltip("Area Factor for the contour approximation")]
        [SerializeField, Range(0, 1)]
        private float areaFactor = 0.2f;

        public List<Material> materials;

        #endregion

        #region Declare Classes

        private CameraController cameraController;
        private MaterialController materialController;
        private PerspectiveProjection perspectiveProjection;
        private CanvasRecognitionDLL canvasRecognitionDLL;

        #endregion

        #region Private Variables

        private bool enableCanvasRecognition = false;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            cameraController = GetComponent<CameraController>();
            if (cameraController == null)
                cameraController = gameObject.AddComponent<CameraController>();

            materialController = GetComponent<MaterialController>();
            if (materialController == null)
                materialController = gameObject.AddComponent<MaterialController>();

            perspectiveProjection = GetComponent<PerspectiveProjection>();
            if (perspectiveProjection == null)
                perspectiveProjection = gameObject.AddComponent<PerspectiveProjection>();

            canvasRecognitionDLL = GetComponent<CanvasRecognitionDLL>();
            if (canvasRecognitionDLL == null)
                canvasRecognitionDLL = gameObject.AddComponent<CanvasRecognitionDLL>();

            cameraController.cameraTexture = new Texture2D(videoWidth, videoHeight);
            cameraController.webcamData = new Color32[videoWidth * videoHeight];
            cameraController.webcamTexture = new WebCamTexture(videoWidth, videoHeight);
        }

        void Start()
        {
            InitializeObjects();

            materialController.SetMaterials(materials, cameraController.cameraTexture);

            ResetTransform();
        }

        private void Update()
        {
            if (cameraController != null)
            {
                if (cameraController.webcamTexture != null && cameraController.webcamTexture.isPlaying)
                {
                    var rawFrame = cameraController.webcamTexture.GetPixels32(cameraController.webcamData);
                    if (enableCanvasRecognition)
                    {
                        canvasRecognitionDLL.PerformRecognition(ref rawFrame, videoWidth, videoHeight, blurKSize, threshValue, threshMax, epsilonFactor, areaFactor, (int)step, ref canvasRecognitionDLL.points);
                    }
                    cameraController.cameraTexture.SetPixels32(rawFrame);
                    cameraController.cameraTexture.Apply();
                }
            }
        }

        private void OnValidate()
        {
            if (materialController != null && cameraController != null)
                materialController.SetMaterials(materials, cameraController.cameraTexture);
        }

        #endregion

        public void StartCanvasRecognition()
        {
            perspectiveProjection.ResetMatrix();
            enableCanvasRecognition = true;

            Debug.Log("Canvas Recognition Active!");
        }

        public void StopCanvasRecognition()
        {
            perspectiveProjection.ResetMatrix();
            enableCanvasRecognition = false;

            Debug.Log("Canvas Recognition stopped!");
        }

        public void StartCamera()
        {
            if(materialController != null)
                cameraController.StartCamera(deviceName, videoWidth, videoHeight, videoFPS);

            if (materialController != null && cameraController != null)
                materialController.SetMaterials(materials, cameraController.cameraTexture);
            
            Debug.Log("Camera Started!");
        }

        public void StopCamera()
        {
            if (materialController != null)
                cameraController.StopCamera();

            Debug.Log("Camera Stopped!");
        }

        public void WarpPerspective()
        {
            StartCanvasRecognition();
            materialController.SetVectors(materials, perspectiveProjection.GetMatrix(canvasRecognitionDLL.points, videoWidth, videoHeight));

            Debug.Log("Perspective Warped!");
        }

        public void SaveTexture()
        {
            Debug.Log("Texture Saved!");
        }

        public void ResetTransform()
        {
            materialController.SetVectors(materials, new double[] { 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f });

            canvasRecognitionDLL.ResetPoints(videoWidth, videoHeight);

            perspectiveProjection.ResetMatrix();

            Debug.Log("Transform Reset!");
        }
    }
}