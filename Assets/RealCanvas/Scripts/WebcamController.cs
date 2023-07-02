using System.Collections.Generic;
using UnityEngine;

namespace RealCanvas
{
    public class WebcamController : MonoBehaviour
    {
        private WebCamTexture webcamTexture;

        private Color32[] webcamData;

        public List<string> GetWebcamNames()
        {
            List<string> names = new List<string>();

            foreach (var device in WebCamTexture.devices)
            {
                names.Add(device.name);
            }

            return names;
        }

        public void InitializaWebcam(string deviceName, string resolution, int fps)
        {
            if (webcamTexture != null)
            {
                Debug.LogWarning("Webcam is already initialized. Stop the current webcam before initializing a new one!");
                return;
            }

            var requestedWidth = int.Parse(resolution.Split("x")[0]);
            var requestedHeight = int.Parse(resolution.Split("x")[1]);

            webcamTexture = new WebCamTexture(deviceName, requestedWidth, requestedHeight, fps);
        }

        public void StartWebcam()
        {
            if (webcamTexture == null)
            {
                Debug.LogWarning("Webcam is not initialized. Initialize the webcam before starting it!");
                return;
            }

            webcamTexture.Play();

            // If webcamData array already exists and has the correct size, no need to reallocate
            if (webcamData == null || webcamData.Length != webcamTexture.width * webcamTexture.height)
            {
                webcamData = new Color32[webcamTexture.width * webcamTexture.height];
            }
        }

        public void StopWebcam()
        {
            if (webcamTexture != null)
            {
                webcamTexture.Stop();
                Destroy(webcamTexture);
                webcamTexture = null;
            }
        }

        public Color32[] RawFrame()
        {
            if (webcamTexture != null)
            {
                return webcamTexture.GetPixels32(webcamData);
            }
            else
            {
                Debug.LogWarning("Webcam is not initialized!");
                return null;
            }
        }

        public bool IsPlaying()
        {
            return webcamTexture != null && webcamTexture.isPlaying;
        }

        public string Resolution
        {
            get
            {
                if (webcamTexture != null)
                {
                    var resolution = webcamTexture.width + "x" + webcamTexture.height;
                    return resolution;
                }
                else
                {
                    Debug.LogWarning("Webcam is not initialized!");
                    return null;
                }
            }
        }

        public int Width => webcamTexture.width;

        public int Height => webcamTexture.height;
    }
}
