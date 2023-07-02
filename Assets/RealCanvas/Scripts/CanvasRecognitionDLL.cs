using System.Runtime.InteropServices;
using UnityEngine;

namespace RealCanvas
{
    public class CanvasRecognitionDLL : MonoBehaviour
    {
        public float[] points;

        public void InitializePoints(int videoWidth, int videoHeight)
        {
            // Set points to video frame width and height.
            // This array holds the values that will be overridden by the OpenCV plugin
            // with points defining the corners of the recognized canvas.
            points = new float[] { 0f, 0f, videoWidth, 0f, 0f, videoHeight, videoWidth, videoHeight };
        }

        public void PerformRecognition(ref Color32[] rawFrame, int width, int height, int blurKSize, int threshValue, int threshMax, double epsilonFactor, float areaFactor, int step, ref float[] points)
        {
            CanvasRecognition(ref rawFrame, width, height, blurKSize, threshValue, threshMax, epsilonFactor, areaFactor, (int)step, ref points);
        }

        [DllImport("CanvasRecognition")]
        private static extern void CanvasRecognition(ref Color32[] raw, int width, int height, int blurKSize, int threshValue, int threshMax, double epsilonFactor, float areaFactor, int step, ref float[] pts);
    }
}
