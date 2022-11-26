using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyAttackDamageHandler : MonoBehaviour
{
    Collider damageCollider;
    public bool hasDamaged;
    [SerializeField] private EnemyAttackAction attack;

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
        //hasDamaged = false;
    }

    [PunRPC]
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log(collision.name);
        if (collision.tag == "Player" || collision.tag == "LocalPlayer")
        {
            Debug.Log("The collision is a player.");
            PlayerPolishManager player = collision.GetComponent<PlayerPolishManager>();
            if (player != null/* && !hasDamaged*/)
            {
                Debug.Log("Player is not null.");
                player.gameObject.GetPhotonView().RPC("DamageEnemyPlayer", RpcTarget.All, attack.damage);
                //hasDamaged = true;
            }
        }
    }
}
