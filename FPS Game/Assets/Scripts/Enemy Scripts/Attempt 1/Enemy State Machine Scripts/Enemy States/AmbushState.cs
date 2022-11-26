using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushState : State
{
    public bool isSleeping;
    public float detectionRadius;
    public string waitingAnimation;
    public string rouseAnimation;
    public LayerMask detectionLayer;

    public ChaseState chaseState;
    public float timer;
    public float startTimer;

    public override State Tick(EnemyManager enemyManager, EnemyHealthManager healthManager, EnemyAnimatorManager enemyAnimator)
    {
        if(enemyManager != null)
        {
            if (isSleeping && enemyManager.isInteracting == false)
            {
                enemyAnimator.PlayTargetAnimation(waitingAnimation, true);
            }

            //The timer stuff is to save on performance. It really helps!
            if (timer > 0) { timer -= Time.deltaTime; }
            if (timer <= 0)
            {
                //This is to check how many times the script is checking for the player every second.
                //Debug.Log(enemyManager.gameObject.name + " is checking for target!");
                #region HandleTargetDetection
                Collider[] colliders = Physics.OverlapSphere(enemyManager.transform.position, detectionRadius, detectionLayer);

                for (int i = 0; i < colliders.Length; i++)
                {
                    PlayerPolishManager player = colliders[i].transform.GetComponent<PlayerPolishManager>();

                    if (player != null)
                    {
                        Vector3 targetDirection = player.transform.position - enemyManager.transform.position;
                        float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

                        if (viewableAngle > enemyManager.minimumDetectionAngle && viewableAngle < enemyManager.maximumDetectionAngle)
                        {
                            enemyManager.currentTarget = player.gameObject;
                            isSleeping = false;
                            enemyAnimator.PlayTargetAnimation(rouseAnimation, true);
                        }
                    }
                }
                #endregion
                timer = startTimer;
            }


            #region HandleStateChange
            if (enemyManager.currentTarget != null) { return chaseState; }
            else { return this; }
            #endregion
        }
        else { return this; }
    }
}
