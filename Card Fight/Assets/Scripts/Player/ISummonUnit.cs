using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISummonUnit
{
    void MultiplyStats(float multiplier);// 用于统一修改召唤物攻击力、血量等
    void SetManager(CardYe manager);
}
