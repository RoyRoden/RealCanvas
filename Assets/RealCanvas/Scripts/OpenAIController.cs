using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace RealCanvas
{
    [Serializable]
    public class Data
    {
        public string b64_json;
    }

    [Serializable]
    public class JsonResponse
    {
        public List<Data> data;
    }

    public class OpenAIController : MonoBehaviour
    {
        private string uri;
        private string imagePath;
        private string apiKey;

        public void SendPostRequest()
        {
            StartCoroutine(PostRequestCoroutine());
        }

        private IEnumerator PostRequestCoroutine()
        {
            uri = "https://api.openai.com/v1/images/variations";
            imagePath = Path.Combine(Application.persistentDataPath, "Screenshots", "screenshot_230704.png");

            byte[] bytes = File.ReadAllBytes(imagePath);

            WWWForm form = new WWWForm();

            form.AddBinaryData("image", bytes);

            form.AddField("response_format", "b64_json");

            using (UnityWebRequest www = UnityWebRequest.Post(uri, form))
            {
                www.SetRequestHeader("Authorization", "Bearer " + apiKey);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("OpenAI request complete!");

                    // Save the raw response for debugging
                    string debugPath = Path.Combine(Application.persistentDataPath, "debugResponse.json");
                    File.WriteAllText(debugPath, www.downloadHandler.text);
                    Debug.Log("Raw response saved to: " + debugPath);

                    // Parse the JSON response
                    JsonResponse jsonResponse = JsonUtility.FromJson<JsonResponse>(www.downloadHandler.text);

                    // Get the base64 image string from the first item in the "data" array of the JSON response
                    string base64Image = jsonResponse.data[0].b64_json;

                    // Convert the base64 string to a byte array
                    byte[] imageBytes = Convert.FromBase64String(base64Image);

                    // Create a texture2D object from the byte array
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageBytes);  // Automatically resizes the texture dimensions

                    // Save the image
                    string pathToSave = Path.Combine(Application.persistentDataPath, "decodedImage.png");
                    File.WriteAllBytes(pathToSave, texture.EncodeToPNG());

                    Debug.Log("Image decoded and saved to: " + pathToSave);
                }
            }
        }
    }
}
