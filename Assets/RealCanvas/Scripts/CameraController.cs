using UnityEngine;

namespace RealCanvas
{
    public class CameraController : MonoBehaviour
    {
        public WebCamTexture webcamTexture;
        public Color32[] webcamData;
        public Texture2D cameraTexture;

        public void StartCamera(string deviceName, int videoWidth, int videoHeight, int videoFPS)
        {
            // If webcamTexture already exists, destroy it first to free up memory
            if (webcamTexture != null)
            {
                Destroy(webcamTexture);
            }

            // Initialise WebCamTexture
            webcamTexture = new WebCamTexture(deviceName, videoWidth, videoHeight, videoFPS);
            webcamTexture.Play();

            // If cameraTexture already exists, destroy it first to free up memory
            if (cameraTexture != null)
            {
                Destroy(cameraTexture);
            }

            cameraTexture = new Texture2D(webcamTexture.width, webcamTexture.height);

            // If webcamData array already exists and has the correct size, no need to reallocate
            if (webcamData == null || webcamData.Length != webcamTexture.width * webcamTexture.height)
            {
                webcamData = new Color32[webcamTexture.width * webcamTexture.height];
            }

            Debug.Log("Camera started!");
        }

        public void StopCamera()
        {
            //StopCanvasRecognition();

            if (webcamTexture != null)
            {
                webcamTexture.Stop();
                webcamTexture = null;
            }

            Debug.Log("Camera stopped!");
        }
    }
}