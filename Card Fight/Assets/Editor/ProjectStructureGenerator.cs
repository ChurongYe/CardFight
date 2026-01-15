
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ProjectStructureGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Project Folders")]
    public static void GenerateFolders()
    {
        string[] folders = new string[]
        {
            "Scripts/Core",
            "Scripts/Cards",
            "Scripts/Skills",
            "Scripts/UI",
            "Scripts/Utilities",

            "Prefabs/Cards",
            "Prefabs/Enemies",
            "Prefabs/Player",
            "Prefabs/Skills",

            "Animations/Player",
            "Animations/Enemies",

            "Art/Characters",
            "Art/Cards",
            "Art/Effects",
            "Art/UI",

            "Audio/Music",
            "Audio/SFX",

            "Materials",
            "Scenes",
            "Shaders",
            "Resources/CardData",
            "Editor"
        };

        foreach (var folder in folders)
        {
            string fullPath = Path.Combine(Application.dataPath, folder);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log("Created folder: " + folder);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("<color=green><b>项目结构生成完成！</b></color>");
    }
}
