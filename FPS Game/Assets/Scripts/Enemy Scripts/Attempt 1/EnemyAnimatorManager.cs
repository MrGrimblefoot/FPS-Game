using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimatorManager : AnimatorManager
{
    EnemyAIManager enemyManager;
    [SerializeField] AudioSource audioSource;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyManager = GetComponent<EnemyAIManager>();
    }

    private void OnAnimatorMove()
    {
        float delta = Time.deltaTime;
        enemyManager.rb.drag = 0;
        Vector3 deltaPosition = anim.deltaPosition;
        deltaPosition.y = 0;
        Vector3 velocity = deltaPosition / delta;
        enemyManager.rb.velocity = velocity/* * enemyManager.moveSpeed*/;
    }

    public void PlayScream() { audioSource.PlayOneShot(audioSource.clip); }
}
