using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

[ExecuteAlways]
public class RealCanvas : MonoBehaviour
{
    [SerializeField, Delayed]
    private string deviceName = "Select a Webcam Device";
    [SerializeField]
    private int videoWidth = 1280;
    [SerializeField]
    private int videoHeight = 720;
    [SerializeField]
    private int videoFPS = 30;
    private enum steps { Gray, Blur, Threshold, Contour, Poly };
    [SerializeField]
    private steps step = steps.Poly;
    [SerializeField, Range(1, 25)]
    private int blurKSize = 4;
    [SerializeField, Range(0, 255)]
    private int threshValue = 127;
    [SerializeField, Range(0, 255)]
    private int threshMax = 255;
    [SerializeField, Range(0, 1)]
    private double epsilonFactor = 0.08;
    [SerializeField, Range(0, 1)]
    private float areaFactor = 0.2f;

    public List<Material> materials;

    private WebCamTexture webcamTexture;
    private Color32[] webcamData;
    private Texture2D cameraTexture;

    private bool enableCanvasRecognition = false;

    private List<Vector2> srcPts = new List<Vector2>();
    private List<Vector2> dstPts = new List<Vector2>();

    private float[] points;

    void Start()
    {
        ResetTransform();
        SetMaterials();
    }
    private void OnValidate()
    {
        SetMaterials();
    }

    private void Update()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            var rawFrame = webcamTexture.GetPixels32(webcamData);
            if (enableCanvasRecognition)
            {
                CanvasRecognition(ref rawFrame, webcamTexture.width, webcamTexture.height, blurKSize, threshValue, threshMax, epsilonFactor, areaFactor, (int)step, ref points);
            }
            cameraTexture.SetPixels32(rawFrame);
            cameraTexture.Apply();
        }
    }

    public void ResetTransform()
    {
        // Set points to video frame width and height.
        // This array holds the values that will be overridden by the OpenCV plugin
        // with points defining the corners of the recognized canvas.
        points = new float[] { 0f, 0f, videoWidth, 0f, 0f, videoHeight, videoWidth, videoHeight };

        // Reset the transformation matrix in the RealCanvas shader
        SetVectors(new double[] { 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f });

        // Create a list of 4 vectors to hold the normalized coordinates of the screen's corners
        srcPts.Add(new Vector2(0f, 0f));
        srcPts.Add(new Vector2(1f, 0f));
        srcPts.Add(new Vector2(0f, 1f));
        srcPts.Add(new Vector2(1f, 1f));

        Debug.Log("Transformation Matrix has been reset!");
    }

    // Set material properties
    public void SetMaterials()
    {
        int materialsCount = 0;
        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i] != null)
            {
                materials[i].SetTexture("_VideoTex", cameraTexture);
                materialsCount++;
            }
        }
        if (materialsCount == 0) {
            throw new Exception("No materials assigned!");
        } else 
        { 
            materialsCount = 0; 
        }
    }

    public void SetVectors (double[] M)
    {
        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i] != null)
            {
                materials[i].SetVector("_M0", new Vector3((float)M[0], (float)M[1], (float)M[2]));
                materials[i].SetVector("_M1", new Vector3((float)M[3], (float)M[4], (float)M[5]));
                materials[i].SetVector("_M2", new Vector3((float)M[6], (float)M[7], 1f));
            }
        }
    }

    public void StopCamera()
    {
        StopCanvasRecognition();

        if (webcamTexture != null)
        {
            webcamTexture.Stop();
            webcamTexture = null;
        }

        Debug.Log("Camera stopped playing!");
    }

    public void StartCamera()
    {
        //Stop Camera
        StopCamera();

        // Initialise WebCamTexture
        webcamTexture = new WebCamTexture(deviceName, videoWidth, videoHeight, videoFPS);
        webcamTexture.Play();
        cameraTexture = new Texture2D(webcamTexture.width, webcamTexture.height);
        // the below is done to avoid allocating new memory each frame
        webcamData = new Color32[webcamTexture.width * webcamTexture.height];

        // Set material properties
        SetMaterials();

        Debug.Log("Start Playing!");
    }

    public void SaveTexture()
    {
        if (webcamTexture != null)
        {
            byte[] bytes = ImageConversion.EncodeArrayToPNG(cameraTexture.GetRawTextureData(), cameraTexture.graphicsFormat, (uint)webcamTexture.width, (uint)webcamTexture.height);

            String now = System.DateTime.Now.ToString();
            now = now.Replace("/", "");
            now = now.Replace(":", "");
            now = now.Replace(" ", "");

            // Write the returned byte array to a file in the project folder
            File.WriteAllBytes(Application.dataPath + "/RealCanvas/Screenshots/screenshot_" + now + ".png", bytes);
            Debug.Log("Texture Saved!");
        }
        else
        {
            throw new Exception("Camera is not playing!");
        }
    }

    public void StartCanvasRecognition()
    {
        ResetTransform();

        enableCanvasRecognition = true;

        Debug.Log("Canvas Recognition Active!");
    }

    public void StopCanvasRecognition()
    {
        enableCanvasRecognition = false;

        Debug.Log("Canvas Recognition stopped!");
    }

    public void WarpPerspective()
    {
        // Stop Canvas Recognition
        StopCanvasRecognition();

        // Clear the list to only hold current values
        dstPts.Clear();

        // Create a list of destination points
        dstPts.Add(new Vector2(points[0] / videoWidth, points[1] / videoHeight));
        dstPts.Add(new Vector2(points[2] / videoWidth, points[3] / videoHeight));
        dstPts.Add(new Vector2(points[4] / videoWidth, points[5] / videoHeight));
        dstPts.Add(new Vector2(points[6] / videoWidth, points[7] / videoHeight));

        // Run perspective transform passing source and destination points
        // and set the transformation matrix in the shader
        SetVectors(PerspectiveTransform(srcPts, dstPts));

        Debug.Log("Warped Perspective!");
    }

    // Perspective Transform
    private double[] PerspectiveTransform(List<Vector2> src, List<Vector2> dst)
    {
        double[,] A = new double[8, 8];
        double[] B = new double[8];
        double[] X;

        // Algorithm to populate the A and B arrays is based on the OpenCV library getPerspectiveTransform code from (https://github.com/opencv/opencv/blob/4.x/modules/imgproc/src/imgwarp.cpp)
        for (int i = 0; i < 4; i++)
        {
            A[i, 0] = A[i + 4, 3] = src[i].x;
            A[i, 1] = A[i + 4, 4] = src[i].y;
            A[i, 2] = A[i + 4, 5] = 1;
            A[i, 3] = A[i, 4] = A[i, 5] = A[i + 4, 0] = A[i + 4, 1] = A[i + 4, 2] = 0;
            A[i, 6] = -src[i].x * dst[i].x;
            A[i, 7] = -src[i].y * dst[i].x;
            A[i + 4, 6] = -src[i].x * dst[i].y;
            A[i + 4, 7] = -src[i].y * dst[i].y;
            B[i] = dst[i].x;
            B[i + 4] = dst[i].y;
        }

        // Perform Gaussian Elimination
        X = GaussianElimination(A, B);

        return X;
    }

    // Gaussian Elimination
    public double[] GaussianElimination(double[,] A, double[] B)
    {
        int N = B.Length;

        for (int p = 0; p < N; p++)
        {
            // find pivot row and swap
            int max = p;
            for (int i = p + 1; i < N; i++)
            {
                if (Math.Abs(A[i, p]) > Math.Abs(A[max, p]))
                {
                    max = i;
                }
            }
            double[] temp = new double[N];
            for (int i = 0; i < N; i++)
            {
                temp[i] = A[p, i];
                A[p, i] = A[max, i];
                A[max, i] = temp[i];
            }
            double t = B[p];
            B[p] = B[max];
            B[max] = t;

            // singular or nearly singular
            if (Math.Abs(A[p, p]) <= double.Epsilon)
            {
                throw new Exception("Matrix is singular or nearly singular");
            }

            // pivot within A and B
            for (int i = p + 1; i < N; i++)
            {
                double alpha = A[i, p] / A[p, p];
                B[i] -= alpha * B[p];
                for (int j = p; j < N; j++)
                {
                    A[i, j] -= alpha * A[p, j];
                }
            }
        }

        // back substitution
        double[] X = new double[N];
        for (int i = N - 1; i >= 0; i--)
        {
            double sum = 0.0;
            for (int j = i + 1; j < N; j++)
            {
                sum += A[i, j] * X[j];
            }
            X[i] = (B[i] - sum) / A[i, i];
        }
        return X;
    }

// Import C++ CanvasRecognition plugin
[DllImport("CanvasRecognition")]
    private static extern void CanvasRecognition(ref Color32[] raw, int width, int height, int blurKSize, int threshValue, int threshMax, double epsilonFactor, float areaFactor, int step, ref float[] pts);
}
