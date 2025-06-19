using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillEffectConfig", menuName = "Custom/Skill Effect Config", order = 1)]
public class SkillEffectConfig : ScriptableObject
{
    [System.Serializable]
    public class EffectData
    {
        public string key;                   // 比如 "Slash"
        public GameObject effectPrefab;      // 对应特效
    }

    public EffectData[] effects;

    // 快速查找映射表（运行时构建）
    private Dictionary<string, GameObject> _effectDict;

    public GameObject GetEffect(string key)
    {
        if (_effectDict == null)
        {
            _effectDict = new Dictionary<string, GameObject>();
            foreach (var e in effects)
            {
                _effectDict[e.key] = e.effectPrefab;
            }
        }
        _effectDict.TryGetValue(key, out GameObject prefab);
        return prefab;
    }
}