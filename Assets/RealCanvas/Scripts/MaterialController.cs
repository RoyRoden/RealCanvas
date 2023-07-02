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
        [SerializeField]
        List<Material> materials = new List<Material>();

        private Texture2D processedTexture;

        public void InitializeMaterial(string resolution)
        {
            var width = int.Parse(resolution.Split("x")[0]);
            var height = int.Parse(resolution.Split("x")[1]);

            processedTexture = new Texture2D(width, height);

            OperateOnMaterials(materials, (material) =>
            {
                material.SetTexture(ShaderProperties.VideoTexPropertyName, processedTexture);
            });

            Debug.Log("Material has been initialized with " + width + "x" + height + " resolution.");
        }

        public Texture2D ProcessedTexture {
            get 
            {
                return processedTexture;
            }
            set
            {
                processedTexture = value;
            }
        }

        // Set transformation matrix in the shader assigned to each material
        public void SetVectors(double[] M)
        {
            OperateOnMaterials(materials, (material) =>
            {
                material.SetVector(ShaderProperties.M0PropertyName, new Vector3((float)M[0], (float)M[1], (float)M[2]));
                material.SetVector(ShaderProperties.M1PropertyName, new Vector3((float)M[3], (float)M[4], (float)M[5]));
                material.SetVector(ShaderProperties.M2PropertyName, new Vector3((float)M[6], (float)M[7], 1f));
            });

            Debug.Log("Perspective Warped.");
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
    }
}
