using System.Collections.Generic;
using System.IO;
using Packages.art.hypothetic.unity.Editor.Scripts.ExporterTreeView;
using UnityEditor;
using UnityEngine;
using UnityGLTF;

namespace Packages.art.hypothetic.unity.Editor.Scripts
{
    internal class ExporterWindowModel : ExporterWindow
    {
        [MenuItem("Hypothetic/Export Models")]
        public static void ShowWindow()
        {
            GetWindow<ExporterWindowModel>("Export Models");
        }

        internal override List<string> GetAssetPaths()
        {
            List<string> modelPaths = new List<string>();
            foreach (var modelGuid in AssetDatabase.FindAssets("t:GameObject"))
            {
                string path = AssetDatabase.GUIDToAssetPath(modelGuid);
                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (modelPrefab == null)
                {
                    continue;
                }
                if (modelPrefab.GetComponentInChildren<Renderer>() == null && modelPrefab.GetComponentInChildren<SkinnedMeshRenderer>() == null)
                {
                    continue;
                }
                modelPaths.Add(path);
            }
            return modelPaths;
        }

        internal override void ExportItem(string assetPath, string dstDirPath)
        {

            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (modelPrefab != null)
            {
                GameObject instantiatedModel = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                if (instantiatedModel != null)
                {
                    Transform rootTransform = instantiatedModel.transform.root;
                    Debug.Log("Root transform of " + assetPath + ": " + rootTransform.name);

                    Transform[] rootTransforms = { rootTransform };

                    Export(rootTransforms, true, dstDirPath, Path.GetFileNameWithoutExtension(assetPath));

                    DestroyImmediate(instantiatedModel);
                }
            }
        }

        private static void Export(Transform[] transforms, bool binary, string exportDestination, string sceneName)
        {
            var settings = GLTFSettings.GetOrCreateSettings();
            var exportOptions = new ExportContext(settings) { TexturePathRetriever = RetrieveTexturePath };
            var exporter = new GLTFSceneExporter(transforms, exportOptions);

            var invokedByShortcut = Event.current?.type == EventType.KeyDown;

            var ext = binary ? ".glb" : ".gltf";
            var resultFile = GLTFSceneExporter.GetFileName(exportDestination, sceneName, ext);
            settings.SaveFolderPath = exportDestination;
            if (binary)
                exporter.SaveGLB(exportDestination, sceneName);
            else
                exporter.SaveGLTFandBin(exportDestination, sceneName);

            Debug.Log("Exported to " + resultFile);
        }

        public static string RetrieveTexturePath(Texture texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            // texture is a subasset
            if (AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(Texture2D))
            {
                var ext = Path.GetExtension(path);
                if (string.IsNullOrWhiteSpace(ext)) return texture.name + ".png";
                path = path.Replace(ext, "-" + texture.name + ext);
            }
            return path;
        }
    }
}