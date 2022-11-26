using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Enemy Actions/Attack Actions")]
public class EnemyAttackAction : EnemyActions
{
    public int attackScore = 3;//attackChance
    public float recoveryTime = 2;
    public float maximumAttackAngle = 35f;
    public float minimumAttackAngle = -35f;

    public float maximumDistanceNeededToAttack = 3;
    public float minimumDistanceNeededToAttack = 0;

    public LayerMask viableAttackLayers;
    public int damage;
}
