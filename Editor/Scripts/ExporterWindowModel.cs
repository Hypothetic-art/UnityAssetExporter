using System.IO;
using System.Linq;
using Hypothetic;
using UnityEditor;
using UnityEngine;
using UnityGLTF;

namespace Assets.Hypothetic_for_Unity.Scripts
{
    internal class ExporterWindowModel : EditorWindow
    {
        private bool[] modelSelections;
        private string[] modelGuids;
        private string[] modelPaths;
        private Vector2 scrollViewPosition;

        [MenuItem("Hypothetic/Export Models")]
        public static void ShowWindow()
        {
            GetWindow<ExporterWindowModel>("Export Models");
        }

        private void OnEnable()
        {
            RefreshModelList();
        }

        private void RefreshModelList()
        {
            // Get all model assets in the project
            modelGuids = AssetDatabase.FindAssets("t:GameObject");
            modelPaths = new string[modelGuids.Length];

            int i = 0;
            foreach (var modelGuid in modelGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (modelPrefab == null)
                {
                    continue;
                }
                if (modelPrefab.GetComponentInChildren<Renderer>() == null && modelPrefab.GetComponentInChildren<SkinnedMeshRenderer>() == null)
                {
                    continue;
                }
                // todo: implement filter that checks if any model shows up in the path
                //instantiatedModel.GetComponent<Renderer>();
                modelPaths[i++] = path;
            }
            modelPaths = modelPaths.Take(i).ToArray();
            modelSelections = new bool[modelPaths.Length];
        }

        private void OnGUI()
        {
            SharedGUI.displayHeader();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export"))
            {
                ExportSelectedModels();
            }
            EditorGUILayout.EndHorizontal();


            // Select All and Deselect All buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                SelectAllModels(true);
            }
            if (GUILayout.Button("Deselect All"))
            {
                SelectAllModels(false);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("List of Models", EditorStyles.boldLabel);
            // Display checkboxes for each model
            scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition);
            for (int i = 0; i < modelPaths.Length; i++)
            {
                modelSelections[i] = EditorGUILayout.ToggleLeft(modelPaths[i], modelSelections[i]);
            }
            EditorGUILayout.EndScrollView();
        }

        private void SelectAllModels(bool select)
        {
            for (int i = 0; i < modelSelections.Length; i++)
            {
                modelSelections[i] = select;
            }
        }

        private void ExportSelectedModels()
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
                    GameObject instantiatedModel = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                    if (instantiatedModel != null)
                    {
                        Debug.Log(instantiatedModel.name);
                        Transform rootTransform = instantiatedModel.transform.root;
                        Debug.Log("Root transform of " + modelPaths[i] + ": " + rootTransform.name);

                        Transform[] rootTransforms = { rootTransform };

                        Export(rootTransforms, true, exportDestination, Path.GetFileNameWithoutExtension(modelPaths[i]));

                        DestroyImmediate(instantiatedModel);
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