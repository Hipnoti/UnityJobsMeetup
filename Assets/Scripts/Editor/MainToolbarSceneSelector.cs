using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor.Toolbars;

namespace LordGame.Editor
{
    public static class MainToolbarSceneSelector
    {
        private const string ELEMENT_ID = "LordGame/SceneSelector";

        [MainToolbarElement(ELEMENT_ID, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static IEnumerable<MainToolbarElement> CreateSceneSelector()
        {
            var dropdown = new MainToolbarDropdown(
                new MainToolbarContent("Select Scene", (Texture2D)EditorGUIUtility.IconContent("d_SceneAsset Icon").image, "Select a scene from the Scenes folder"),
                OnDropdownClick
            );
            yield return dropdown;
        }

        private static void OnDropdownClick(Rect rect)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var scene in GetAllScenesInScenesFolder())
            {
                string menuPath = scene.Name;
                if (menuPath.ToLower().Contains("test") || scene.Path.Contains("/Tests/"))
                {
                    menuPath = "Tests/" + menuPath;
                }

                menu.AddItem(new GUIContent(menuPath), false, () =>
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scene.Path);
                    }
                });
            }
            menu.DropDown(rect);
        }

        private static List<SceneInfo> GetAllScenesInScenesFolder()
        {
            var scenes = new List<SceneInfo>();

            // Find all scene files in the project under Assets/Scenes
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Filter out scenes that aren't actually in Assets/Scenes (FindAssets is recursive)
                if (!path.StartsWith("Assets/Scenes/")) continue;

                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                scenes.Add(new SceneInfo(name, path));
            }

            return scenes;
        }

        private class SceneInfo
        {
            public string Name;
            public string Path;

            public SceneInfo(string name, string path)
            {
                Name = name;
                Path = path;
            }
        }
    }
}
