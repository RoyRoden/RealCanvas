using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealCanvas
{
    public static class ShaderProperties
    {
        public const string VideoTexPropertyName = "_VideoTex";
        public const string M0PropertyName = "_M0";
        public const string M1PropertyName = "_M1";
        public const string M2PropertyName = "_M2";
    }

    public class MaterialController : MonoBehaviour
    {
        // Set material properties
        public void SetMaterials(List<Material> materials, Texture2D cameraTexture)
        {
            OperateOnMaterials(materials, (material) =>
            {
                material.SetTexture(ShaderProperties.VideoTexPropertyName, cameraTexture);
            });
        }

        // Set transformation matrix in the shader assigned to each material
        public void SetVectors(List<Material> materials, double[] M)
        {
            OperateOnMaterials(materials, (material) =>
            {
                material.SetVector(ShaderProperties.M0PropertyName, new Vector3((float)M[0], (float)M[1], (float)M[2]));
                material.SetVector(ShaderProperties.M1PropertyName, new Vector3((float)M[3], (float)M[4], (float)M[5]));
                material.SetVector(ShaderProperties.M2PropertyName, new Vector3((float)M[6], (float)M[7], 1f));
            });
        }

        private void OperateOnMaterials(List<Material> materials, Action<Material> operation)
        {
            if (materials != null && materials.Count > 0)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    if (materials[i] != null)
                    {
                        operation(materials[i]);
                    }
                }
            }
            else
            {
                Debug.LogError("No materials assigned!");
            }
        }

        public void SaveTexture()
        {
            /*if (webcamTexture != null)
            {
                byte[] bytes = ImageConversion.EncodeArrayToPNG(cameraTexture.GetRawTextureData(), cameraTexture.graphicsFormat, (uint)webcamTexture.width, (uint)webcamTexture.height);

                String now = System.DateTime.Now.ToString();
                now = now.Replace("/", "");
                now = now.Replace(":", "");
                now = now.Replace(" ", "");

                // Write the returned byte array to a file in the project folder
                string filePath = Path.Combine(Application.dataPath, "RealCanvas/Screenshots", $"screenshot_{now}.png");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                }

                Debug.Log("Texture saved!");
            }
            else
            {
                throw new Exception("Camera is not playing!");
            }*/
        }
    }
}
