#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.art.hypothetic.hydrogen.Editor.Scripts
{
    static class PathUtils
    {
        public static bool isInProject(string path)
        {
            return path.Contains(Application.dataPath);
        }

        public static string getRelativeFromAbsolutePath(string path)
        {
            return ReplaceFirst(path, Application.dataPath, "Assets");
        }

        public static string getAbsoluteFromRelativePath(string path)
        {
            return ReplaceFirst(path, "Assets", Application.dataPath);
        }

        private static string ReplaceFirst(string text, string search, string replace)
        {
            // hands down.....that C# doesn't have this built in is just....
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}

#endif