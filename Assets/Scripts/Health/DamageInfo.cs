using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInfo
{
    public float Damage { get; set; }
    public float StunChance { get; set; }
    public float BiteStunDuration { get; set; }

    public DamageInfo(float damage, float stunChance = 0, float biteStunDuration = 0)
    {
        Damage = damage;
        StunChance = stunChance;
        BiteStunDuration = biteStunDuration;
    }
}
