using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHurtable
{
    void TakeDamage(int damage, bool isCrit);
}
