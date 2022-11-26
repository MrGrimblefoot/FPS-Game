using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public ChaseState chaseState;
    public LayerMask detectionLayer;

    public override State Tick(EnemyManager enemyManager, EnemyHealthManager healthManager, EnemyAnimatorManager enemyAnimator)
    {
        if(enemyManager != null)
        {
            #region HandleEnemyTargetDetection
            Collider[] colliders = Physics.OverlapSphere(enemyManager.transform.position, enemyManager.detectionRadius, detectionLayer);

            for (int i = 0; i < colliders.Length; i++)
            {
                PlayerPolishManager player = colliders[i].transform.GetComponent<PlayerPolishManager>();

                //if (characterStats != null)
                //{
                //    //Check for team ID

                //    Vector3 targetDirection = characterStats.transform.position - transform.position;
                //    float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

                //    if (viewableAngle > enemyManager.minimumDetectionAngle && viewableAngle < enemyManager.maximumDetectionAngle)
                //    {
                //        enemyManager.currentTarget = characterStats.gameObject;
                //    }
                //}

                if (player != null)
                {
                    //Check for team ID

                    Vector3 targetDirection = player.transform.position - enemyManager.transform.position;
                    float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

                    if (viewableAngle > enemyManager.minimumDetectionAngle && viewableAngle < enemyManager.maximumDetectionAngle)
                    {
                        enemyManager.currentTarget = player.gameObject;
                    }
                }
            }
            #endregion

            #region HandleSwitchState
            if (enemyManager.currentTarget != null) { return chaseState; }
            else { return this; }
            #endregion
        }
        else { return this; }
    }
}
