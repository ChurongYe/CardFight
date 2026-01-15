using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SkillButton
{
    public string displayName;     // 显示用（如 "红1"）
    public Suit suit;              // 用枚举，Inspector 下拉选择
    [Range(1, 10)] public int buttonIndex = 1; // 1..4 表示 1号/2号/3号/4号
    [HideInInspector] public int level = 0;    // 当前等级（会在 Inspector 显示若去掉 HideInInspector）
    public UnityEvent onClick;     // 在 Inspector 绑定要触发的方法（AttackFire 等）
}
