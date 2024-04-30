using System;
using System.Collections.Generic;
using System.IO;
using Packages.art.hypothetic.hydrogen.Editor.Scripts.ExporterTreeView;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using SearchField = UnityEditor.IMGUI.Controls.SearchField;

namespace Packages.art.hypothetic.hydrogen.Editor.Scripts
{
    internal abstract class ExporterWindow : EditorWindow
    {
        [NonSerialized] bool m_Initialized;
        [SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        SearchField m_SearchField;
        ExporterTreeView.ExporterTreeView m_TreeView;
        IList<ExportTreeElement> m_ExportTreeElements;


        internal abstract List<string> GetAssetPaths();
        internal abstract void ExportItem(string assetPath, string dstDirPath);

        internal void ExportSelected()
        {
            string exportDestination = EditorUtility.OpenFolderPanel("Choose export destination", "", "");
            foreach (ExportTreeElement elem in GetData())
            {
                Debug.Log(elem.path);
                if (elem.exportType != ExportTreeElementType.Directory && elem.enabled)
                    ExportItem(elem.path, exportDestination);
            }
            EditorUtility.RevealInFinder(exportDestination);
        }

        private void OnGUI()
        {
            InitIfNeeded();

            //SharedGUI.displayHeader();
            //EditorGUILayout.BeginHorizontal();
            //if (GUILayout.Button("Export"))
            //{
            //    ExportSelected();
            //}
            //EditorGUILayout.EndHorizontal();

            //// Display checkboxes for each model
            //m_ScrollViewPosition = EditorGUILayout.BeginScrollView(m_ScrollViewPosition);
            SearchBar(toolbarRect);
            DoTreeView(multiColumnTreeViewRect);
            BottomToolBar(bottomToolbarRect);
            //EditorGUILayout.EndScrollView();
        }



        //[MenuItem("TreeView Examples/Multi Columns")]
        public static ExporterWindow GetWindow()
        {
            var window = GetWindow<ExporterWindow>();
            //window.titleContent = new GUIContent("Multi Columns");
            window.Focus();
            window.Repaint();
            return window;
        }

        Rect multiColumnTreeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        }

        Rect toolbarRect
        {
            get { return new Rect(20f, 10f, position.width - 40f, 20f); }
        }

        Rect bottomToolbarRect
        {
            get { return new Rect(20f, position.height - 18f, position.width - 40f, 16f); }
        }

        public ExporterTreeView.ExporterTreeView treeView
        {
            get { return m_TreeView; }
        }

        void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();

                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = ExporterTreeView.ExporterTreeView.CreateDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MyMultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<ExportTreeElement>(GetData());

                m_TreeView = new ExporterTreeView.ExporterTreeView(m_TreeViewState, multiColumnHeader, treeModel);

                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        IList<ExportTreeElement> GetData()
        {
            if (m_ExportTreeElements == null)
            {
                m_ExportTreeElements = PopulateTree();
            }
            return m_ExportTreeElements;
        }

        internal List<ExportTreeElement> PopulateTree()
        {
            var treeElements = new List<ExportTreeElement>();
            int IDCounter = 0;
            var root = new ExportTreeElement("Root", "", ExportTreeElementType.Directory, -1, IDCounter);
            treeElements.Add(root);

            Dictionary<string, ExportTreeElement> nodeMap = new Dictionary<string, ExportTreeElement>
            {
                { "", root }
            };

            ExportTreeElement getDirNode(string dirPath)
            {
                if (dirPath == null || dirPath.Length == 0)
                {
                    return root;
                }
                ExportTreeElement parent;
                List<string> parts = new List<string>();
                string curPath = dirPath;
                while (!nodeMap.TryGetValue(curPath, out parent))
                {
                    parts.Add(Path.GetFileName(curPath));
                    curPath = Path.GetDirectoryName(curPath);
                    if (curPath == null || curPath.Length == 0)
                    {
                        parent = root;
                        break;
                    }
                }
                parts.Reverse();
                foreach (string part in parts)
                {
                    curPath = Path.Combine(curPath, part);
                    var node = new ExportTreeElement(part, curPath, ExportTreeElementType.Directory, parent.depth + 1, ++IDCounter);
                    treeElements.Add(node);
                    nodeMap.Add(curPath, node);
                    parent.children.Add(node);
                    parent = node;
                }
                return parent;
            }

            List<string> assetPaths = GetAssetPaths();
            assetPaths.Sort();

            foreach (string path in assetPaths)
            {
                string baseName = Path.GetFileName(path);
                string dirPath = Path.GetDirectoryName(path);
                ExportTreeElement dirNode = getDirNode(dirPath);
                ExportTreeElement modelNode = new ExportTreeElement(baseName, path, ExportTreeElementType.Model, dirNode.depth + 1, ++IDCounter);
                treeElements.Add(modelNode);
                dirNode.children.Add(modelNode);
            }

            return treeElements;
        }

        void SearchBar(Rect rect)
        {
            treeView.searchString = m_SearchField.OnGUI(rect, treeView.searchString);
        }

        void DoTreeView(Rect rect)
        {
            m_TreeView.OnGUI(rect);
        }

        void BottomToolBar(Rect rect)
        {
            GUILayout.BeginArea(rect);

            using (new EditorGUILayout.HorizontalScope())
            {

                var style = "miniButton";
                if (GUILayout.Button("Export", style))
                {
                    ExportSelected();
                }

                if (GUILayout.Button("Expand All", style))
                {
                    treeView.ExpandAll();
                }

                if (GUILayout.Button("Collapse All", style))
                {
                    treeView.CollapseAll();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Set sorting", style))
                {
                    var myColumnHeader = (MyMultiColumnHeader)treeView.multiColumnHeader;
                    myColumnHeader.SetSortingColumns(new int[] { 4, 3, 2 }, new[] { true, false, true });
                    myColumnHeader.mode = MyMultiColumnHeader.Mode.LargeHeader;
                }


                GUILayout.Label("Header: ", "minilabel");
                if (GUILayout.Button("Large", style))
                {
                    var myColumnHeader = (MyMultiColumnHeader)treeView.multiColumnHeader;
                    myColumnHeader.mode = MyMultiColumnHeader.Mode.LargeHeader;
                }
                if (GUILayout.Button("Default", style))
                {
                    var myColumnHeader = (MyMultiColumnHeader)treeView.multiColumnHeader;
                    myColumnHeader.mode = MyMultiColumnHeader.Mode.DefaultHeader;
                }
                if (GUILayout.Button("No sort", style))
                {
                    var myColumnHeader = (MyMultiColumnHeader)treeView.multiColumnHeader;
                    myColumnHeader.mode = MyMultiColumnHeader.Mode.MinimumHeaderWithoutSorting;
                }

                GUILayout.Space(10);

                if (GUILayout.Button("values <-> controls", style))
                {
                    treeView.showControls = !treeView.showControls;
                }
            }

            GUILayout.EndArea();
        }
    }


    internal class MyMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public MyMultiColumnHeader(MultiColumnHeaderState state)
            : base(state)
        {
            mode = Mode.DefaultHeader;
        }

        public Mode mode
        {
            get
            {
                return m_Mode;
            }
            set
            {
                m_Mode = value;
                switch (m_Mode)
                {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            // Default column header gui
            base.ColumnHeaderGUI(column, headerRect, columnIndex);

            // Add additional info for large header
            if (mode == Mode.LargeHeader)
            {
                // Show example overlay stuff on some of the columns
                if (columnIndex > 2)
                {
                    headerRect.xMax -= 3f;
                    var oldAlignment = EditorStyles.largeLabel.alignment;
                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                    GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.alignment = oldAlignment;
                }
            }
        }
    }


    //[CreateAssetMenu(fileName = "TreeDataAsset", menuName = "Tree Asset", order = 1)]
    public class MyTreeAsset : ScriptableObject
    {
        [SerializeField] List<ExportTreeElement> m_TreeElements = new List<ExportTreeElement>();

        internal List<ExportTreeElement> treeElements
        {
            get { return m_TreeElements; }
            set { m_TreeElements = value; }
        }

        void Awake()
        {
            //if (m_TreeElements.Count == 0)
            //    m_TreeElements = MyTreeElementGenerator.GenerateRandomTree(160);
        }
    }
}
