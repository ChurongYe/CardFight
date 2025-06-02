using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHurtable
{
    void TakeDamage(int damage, bool isCrit);
}

public interface IStunnable
{
    void Stun(float duration);
}
