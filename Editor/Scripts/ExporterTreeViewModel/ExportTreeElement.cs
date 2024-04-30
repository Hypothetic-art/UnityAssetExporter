using System;
using System.Collections.Generic;
using UnityEngine;


namespace Packages.art.hypothetic.hydrogen.Editor.Scripts.ExporterTreeView
{
    public enum ExportTreeElementType
    {
		Directory,
        Model,
        Texture
    }


    [Serializable]
	public class ExportTreeElement
	{
		[SerializeField] int m_ID;
        [SerializeField] int m_Depth;
        [SerializeField] string m_Name;
		[SerializeField] string m_Path;
		[SerializeField] bool m_Enabled;
		[SerializeField] ExportTreeElementType m_ExportType;
		[NonSerialized] ExportTreeElement m_Parent;
		[NonSerialized] List<ExportTreeElement> m_Children;

        public int depth
		{
			get { return m_Depth; }
			set { m_Depth = value; }
		}

		public ExportTreeElement parent
		{
			get { return m_Parent; }
			set { m_Parent = value; }
		}

		public List<ExportTreeElement> children
		{
			get { return m_Children; }
			set { m_Children = value; }
		}

		public bool hasChildren
		{
			get { return children != null && children.Count > 0; }
		}

		public string name
		{
			get { return m_Name; } set { m_Name = value; }
		}

        public string path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }

        public ExportTreeElementType exportType
        {
            get { return m_ExportType; }
            set { m_ExportType = value; }
        }

        public bool enabled
		{
			get { return m_Enabled; }
			set {
				// safe guard against OnGUI value refreshes
				if (value != m_Enabled)
				{
					// set the values of the children
                    if (hasChildren)
                    {
						// set self to false to terminate early in parent check of childs
                        m_Enabled = false;

                        foreach (ExportTreeElement child in m_Children)
                        {
                            child.enabled = value;
                        }
                    }

					// disable parents, as we changed the value of a child
                    ExportTreeElement parent = m_Parent;
                    while (parent != null && parent.m_Enabled != false)
                    {
						// directly set m_Enabled instead of enabled
						// to prevent the parent from directly disabling its childs
                        parent.m_Enabled = false;
						parent = parent.parent;
                    }
                }
				m_Enabled = value; 
				
			}
		}

        public int id
		{
			get { return m_ID; } set { m_ID = value; }
		}

		public ExportTreeElement ()
		{
		}

		public ExportTreeElement (string name, string path, ExportTreeElementType type, int depth, int id)
		{
			m_Name = name;
			m_Path = path;
			m_ExportType = type;
			m_ID = id;
			m_Depth = depth;
            m_Children = new List<ExportTreeElement>();
        }
	}

}


