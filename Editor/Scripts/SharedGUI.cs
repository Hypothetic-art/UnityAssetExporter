#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Packages.art.hypothetic.unity.Editor.Scripts
{
    public static class SharedGUI
    {
        public static Texture2D bannerTexture = Resources.Load<Texture>("Hypo_Primary") as Texture2D;
        private static GUIStyle _hypotheticLabel;
        private static GUIStyle _hypotheticButton;

        // Update is called once per frame
        public static GUIStyle getHypotheticLabel()
		{
			if(_hypotheticLabel == null)
			{
				_hypotheticLabel = new GUIStyle(EditorStyles.miniLabel);
				_hypotheticLabel.richText = true;
			}

			return _hypotheticLabel;
		}

        public static GUIStyle getHypotheticButton()
		{
			if(_hypotheticButton == null)
			{
				_hypotheticButton = new GUIStyle(GUI.skin.button);
				// _hypotheticButton.font = TitiliumRegular;
				_hypotheticButton.fontSize = 10;
				_hypotheticButton.richText = true;
			}

			return _hypotheticButton;
		}

        public static void displayHeader()
		{
			GUILayout.BeginHorizontal(GUILayout.Height(75));
			GUILayout.BeginVertical();
            if (GUILayout.Button(bannerTexture, getHypotheticLabel(), GUILayout.Width(345), GUILayout.Height(68)))
			{
				Application.OpenURL("https://www.hypothetic.art");
			}
			GUILayout.EndVertical();
			GUILayout.Space(5);
			GUILayout.EndHorizontal();
		}
    }
}

#endif