using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpgradeDatabase))]
public class UpgradeDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UpgradeDatabase db = (UpgradeDatabase)target;

        if (GUILayout.Button("重置所有升级等级"))
        {
            foreach (var upgrade in db.upgradeList)
            {
                upgrade.level = 0;
            }

            EditorUtility.SetDirty(db); // 标记资源已修改
            AssetDatabase.SaveAssets(); // 保存修改
            Debug.Log("所有升级等级已重置为 0");
        }
    }
}
