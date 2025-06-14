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

        if (GUILayout.Button("�������������ȼ�"))
        {
            foreach (var upgrade in db.upgradeList)
            {
                upgrade.level = 0;
            }

            EditorUtility.SetDirty(db); // �����Դ���޸�
            AssetDatabase.SaveAssets(); // �����޸�
            Debug.Log("���������ȼ�������Ϊ 0");
        }
    }
}
