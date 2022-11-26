using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBodyPartHealthManager : MonoBehaviour
{
    public bool canDie;
    [SerializeField] private EnemyHealthManager EHM;
    [SerializeField] private float damageMultiplier = 1;

    void Awake()
    {
        canDie = true;
    }

    public void DamageEnemyPart(float damage)
    {
        if (canDie) { EHM.DamageEnemy(damage * damageMultiplier); }

        EHM.bodyPartHealth -= damage;
    }
}
