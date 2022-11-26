using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatStanceState : State
{
    public AttackState attackState;
    public ChaseState chaseState;

    public override State Tick(EnemyManager enemyManager, EnemyHealthManager healthManager, EnemyAnimatorManager enemyAnimator)
    {
        if(enemyManager != null)
        {
            float distanceFromTarget = Vector3.Distance(enemyManager.currentTarget.transform.position, enemyManager.transform.position);
            //potentially circle player ot walk around them

            HandleRotateTowardsTarget(enemyManager);

            if (enemyManager.isPreformingAction) { enemyAnimator.anim.SetFloat("Vertical", 0, 0.1f, Time.deltaTime); enemyManager.navMeshAgent.enabled = false; }
            else { enemyManager.navMeshAgent.enabled = true; }

            if (enemyManager.currentRecoveryTime <= 0 && distanceFromTarget <= enemyManager.maximumAttackRange) { return attackState; }
            else if (distanceFromTarget > enemyManager.maximumAttackRange) { return chaseState; }
            else { return this; }
        }
        else { return this; }
    }

    private void HandleRotateTowardsTarget(EnemyManager enemyManager)
    {
        //Rotate manually
        if (enemyManager.isPreformingAction)
        {
            Vector3 direction = enemyManager.currentTarget.transform.position - enemyManager.transform.position;
            direction.y = 0;
            direction.Normalize();

            if (direction == Vector3.zero) { direction = enemyManager.transform.forward; }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            enemyManager.navMeshAgent.enabled = false;
            enemyManager.transform.rotation = Quaternion.Slerp(enemyManager.transform.rotation, targetRotation, enemyManager.rotationSpeed / Time.deltaTime);
        }
        //Rotate with pathfinding (navmesh)
        else
        {
            Vector3 relativeDirection = enemyManager.transform.InverseTransformDirection(enemyManager.navMeshAgent.desiredVelocity);
            Vector3 targetVelocity = enemyManager.rb.velocity;

            enemyManager.navMeshAgent.enabled = true;
            enemyManager.navMeshAgent.SetDestination(enemyManager.currentTarget.transform.position);
            enemyManager.rb.velocity = targetVelocity;
            enemyManager.transform.rotation = Quaternion.Slerp(enemyManager.transform.rotation, enemyManager.navMeshAgent.transform.rotation, enemyManager.rotationSpeed / Time.deltaTime);
        }
    }
}
