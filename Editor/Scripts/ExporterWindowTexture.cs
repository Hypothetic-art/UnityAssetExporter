using System.IO;
using Hypothetic;
using UnityEditor;
using UnityEngine;
using UnityGLTF;

namespace Assets.Hypothetic_for_Unity.Scripts
{
    internal class ExporterWindowTexture : EditorWindow
    {
        private bool[] modelSelections;
        private string[] modelGuids;
        private string[] modelPaths;
        private Vector2 scrollViewPosition;

        [MenuItem("Hypothetic/Export Textures")]
        public static void ShowWindow()
        {
            GetWindow<ExporterWindowTexture>("Export Textures");
        }

        private void OnEnable()
        {
            RefreshTexturelList();
        }

        private void RefreshTexturelList()
        {
            // Get all model assets in the project
            modelGuids = AssetDatabase.FindAssets("t:Texture");
            modelPaths = new string[modelGuids.Length];
            modelSelections = new bool[modelGuids.Length];

            for (int i = 0; i < modelGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
                modelPaths[i] = path;
            }
        }

        private void OnGUI()
        {
            SharedGUI.displayHeader();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export"))
            {
                ExportSelectedTextures();
            }
            EditorGUILayout.EndHorizontal();


            // Select All and Deselect All buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                SelectAllTextures(true);
            }
            if (GUILayout.Button("Deselect All"))
            {
                SelectAllTextures(false);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("List of Textures", EditorStyles.boldLabel);
            // Display checkboxes for each model
            scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition);
            for (int i = 0; i < modelPaths.Length; i++)
            {
                modelSelections[i] = EditorGUILayout.ToggleLeft(modelPaths[i], modelSelections[i]);
            }
            EditorGUILayout.EndScrollView();
        }

        private void SelectAllTextures(bool select)
        {
            for (int i = 0; i < modelSelections.Length; i++)
            {
                modelSelections[i] = select;
            }
        }

        private void ExportSelectedTextures()
        {
            string exportDestination = EditorUtility.OpenFolderPanel("Choose export destination", "", "");
            Debug.Log(exportDestination);
            if (string.IsNullOrEmpty(exportDestination))
            {
                Debug.LogError(exportDestination);
                return;
            }

            for (int i = 0;i < modelSelections.Length;i++)
            {
                Debug.Log(modelSelections[i]);
                if (!modelSelections[i])
                {
                    continue;
                }
                Debug.Log(modelPaths[i]);
                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPaths[i]);
                if (modelPrefab != null)
                {
                    Debug.Log(modelPrefab.name);
                    GameObject instantiatedTexture = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                    if (instantiatedTexture != null)
                    {
                        Debug.Log(instantiatedTexture.name);
                        Transform rootTransform = instantiatedTexture.transform.root;
                        Debug.Log("Root transform of " + modelPaths[i] + ": " + rootTransform.name);

                        Transform[] rootTransforms = { rootTransform };

                        Export(rootTransforms, true, exportDestination, Path.GetFileNameWithoutExtension(modelPaths[i]));

                        DestroyImmediate(instantiatedTexture);
                    }
                }
            }
            EditorUtility.RevealInFinder(exportDestination);
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