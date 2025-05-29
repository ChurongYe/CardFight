using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDatabase", menuName = "Meta/UpgradeDatabase")]
public class UpgradeDatabase : ScriptableObject
{
    public List<UpgradeData> upgradeList;
}
