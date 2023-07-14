using UnityEngine;

namespace RealCanvas
{
    public class MainController : MonoBehaviour
    {
        #region Declare Classes

        private RuntimeUI runtimeUI;
        private WebcamController webcamController;
        private MaterialController materialController;
        private PerspectiveProjection perspectiveProjection;
        private CanvasRecognitionDLL canvasRecognitionDLL;
        private CaptureController captureController;
        private OpenAIController openAIController;

        #endregion

        private bool isCanvasDectionOn = false;

        #region Unity Methods

        private void Awake()
        {
            runtimeUI = GetComponent<RuntimeUI>();
            if(runtimeUI == null )
                runtimeUI = gameObject.AddComponent<RuntimeUI>();

            runtimeUI.ConnectMainController(this);

            webcamController = GetComponent<WebcamController>();
            if (webcamController == null)
                webcamController = gameObject.AddComponent<WebcamController>();

            materialController = GetComponent<MaterialController>();
            if (materialController == null)
                materialController = gameObject.AddComponent<MaterialController>();

            perspectiveProjection = GetComponent<PerspectiveProjection>();
            if (perspectiveProjection == null)
                perspectiveProjection = gameObject.AddComponent<PerspectiveProjection>();

            canvasRecognitionDLL = GetComponent<CanvasRecognitionDLL>();
            if (canvasRecognitionDLL == null)
                canvasRecognitionDLL = gameObject.AddComponent<CanvasRecognitionDLL>();

            captureController = GetComponent<CaptureController>();
            if(captureController == null)
                captureController = gameObject.AddComponent<CaptureController>();

            openAIController = GetComponent<OpenAIController>();
            if (openAIController == null)
                openAIController = gameObject.AddComponent<OpenAIController>();
        }

        void Start()
        {
            UpdateWebcamDropdownOptions();
        }

        private void Update()
        {
            if (webcamController.IsPlaying())
            {
                var rawFrame = webcamController.RawFrame();
                if (isCanvasDectionOn)
                {
                    canvasRecognitionDLL.PerformRecognition(ref rawFrame, webcamController.Width, webcamController.Height, runtimeUI.BlurKSize, runtimeUI.Threshold, runtimeUI.ThresholdMax, runtimeUI.EpsilonFactor, runtimeUI.AreaFactor, runtimeUI.Step, ref canvasRecognitionDLL.points);
                }
                materialController.ProcessedTexture.SetPixels32(rawFrame);
                materialController.ProcessedTexture.Apply();
            }

        }

        #endregion

 
        public void UpdateWebcamDropdownOptions()
        {
            var webcamNames = webcamController.GetWebcamNames();
            runtimeUI.UpdateWebcamDropdown(webcamNames);
        }

        public void StartCanvasRecognition()
        {
            isCanvasDectionOn = true;
            Debug.Log("Canvas Detection started.");
        }

        public void StopCanvasRecognition()
        {
            isCanvasDectionOn = false;
            Debug.Log("Canvas Detection stopped.");
        }

        public void StartCamera()
        {
            var deviceName = runtimeUI.Webcam;
            var requestedResolution = runtimeUI.RequestedResolution;
            var fps = runtimeUI.RequestedFPS;

            webcamController.InitializaWebcam(deviceName, requestedResolution, fps);

            webcamController.StartWebcam();

            materialController.InitializeMaterial(webcamController.Resolution);

            perspectiveProjection.InitializeMatrix();

            canvasRecognitionDLL.InitializePoints(webcamController.Width, webcamController.Height);
        }

        public void StopCamera()
        {
            webcamController.StopWebcam();
        }

        public void WarpPerspective()
        {
            foreach(var point in canvasRecognitionDLL.points)
            {
                Debug.Log(point);
            }
            materialController.SetVectors(perspectiveProjection.GetMatrix(canvasRecognitionDLL.points, webcamController.Width, webcamController.Height));
        }

        public void SaveTexture()
        {
            captureController.CaptureAndSaveImage();
        }

        public void GenerateVariation()
        {
            openAIController.SendPostRequest();
        }

        public void ResetTransform()
        {
            materialController.SetVectors(new double[] { 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f });

            canvasRecognitionDLL.InitializePoints(webcamController.Height, webcamController.Width);

            perspectiveProjection.InitializeMatrix();

            Debug.Log("Transform Reset.");
        }
    }
}