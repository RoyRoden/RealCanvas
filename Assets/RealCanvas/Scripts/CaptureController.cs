using System;
using UnityEngine;


namespace RealCanvas
{
    public class CaptureController : MonoBehaviour
    {
        public Camera captureCamera;
        private int size = 1024;

        private void Awake()
        {
            if (captureCamera == null)
                Debug.LogError("Missing camera object");
        }

        public void CaptureAndSaveImage()
        {
            string fileName = "screenshot_" + DateTime.Now.ToString("yyMMddhhmmss") + ".png";
            string folderPath = Application.persistentDataPath + "/Screenshots";
            System.IO.Directory.CreateDirectory(folderPath);

            RenderTexture rt = new RenderTexture(size, size, 24);
            captureCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(size, size, TextureFormat.RGB24, false);
            captureCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            captureCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string path = System.IO.Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log($"Saved camera capture to {path}");
        }
    }
}
