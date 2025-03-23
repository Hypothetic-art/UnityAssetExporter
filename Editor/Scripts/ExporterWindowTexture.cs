using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Packages.art.hypothetic.unity.Editor.Scripts.ExporterTreeView;
using UnityEditor;
using UnityEngine;
using UnityGLTF;

namespace Packages.art.hypothetic.unity.Editor.Scripts
{
    internal class ExporterWindowTexture : ExporterWindow
    {
        [MenuItem("Hypothetic/Export Textures")]
        public static void ShowWindow()
        {
            GetWindow<ExporterWindowTexture>("Export Textures");
        }


        internal override List<string> GetAssetPaths()
        {

            Texture[] allTextures = Resources.FindObjectsOfTypeAll<Texture>();

            // Find material textures
            Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();
            HashSet<Texture> materialTextures = new HashSet<Texture>();

            foreach (Material material in allMaterials)
            {
                // Get all texture property names from the shader
                string[] texturePropertyNames = material.GetTexturePropertyNames();

                // Print the names of all textures used by the material
                foreach (string propertyName in texturePropertyNames)
                {
                    Texture texture = material.GetTexture(propertyName);
                    if (texture != null)
                    {
                        materialTextures.Add(texture);
                    }
                }
            }

            // exclude material textures from exportable textures
            List<string> images = new List<string>();
            foreach (Texture texture in allTextures)
            {
                if (!materialTextures.Contains(texture))
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    if (path != null && path.Length > 0)
                    {
                        images.Add(path);
                    }
                }
            }

            return images;
        }

        internal override void ExportItem(string assetPath, string dstDirPath)
        {
            string basename = Path.GetFileName(assetPath);
            string ext = Path.GetExtension(assetPath).ToLower();
            Debug.Log(ext);
            string dst = Path.Join(dstDirPath, basename);
            File.Copy(assetPath, dst, false);
            //string dst = Path.Join(exportDestination, Path.ChangeExtension(basename, "png"));
            //Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            //File.WriteAllBytes(dst, texture.EncodeToPNG());
        }
    }
}