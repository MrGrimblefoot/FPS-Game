using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

public class MeleeDamageHandler : MonoBehaviour/*PunCallbacks*/
{
    [SerializeField] private Weapon weaponStats;
    private int damage;
    [SerializeField] private int impactForce;
    [SerializeField] private Transform impactPoint;
    [SerializeField] private WeaponSystem weaponSystem;
    public float length;
    public bool hasMadeSound;

    private /*new*/ void OnEnable()
    {
        damage = weaponStats.damage;
        weaponSystem = GetComponentInParent<WeaponSystem>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyBodyPartHealthManager>().DamageEnemyPart(damage);
            if (!hasMadeSound) { weaponSystem.HitmarkerEffect(false); hasMadeSound = true; }
            //if(Physics.Raycast(impactPoint.position, impactPoint.forward, out hit, length))
            //{
            //    if (other.gameObject.GetComponent<Rigidbody>() != null) { other.gameObject.GetComponent<Rigidbody>().AddExplosionForce(impactForce, impactPoint.position, length);/*AddForceAtPosition(impactPoint.up * (impactForce), hit.point);*/ }
            //}
            //other.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * impactForce); }
        }
        //if (photonView.IsMine)
        //{
        //    if (other.gameObject.layer == 7)
        //    {
        //        other.gameObject.GetPhotonView().RPC("DamageEnemyPlayer", RpcTarget.All, damage);

        //        if (!hasMadeSound) { weaponSystem.HitmarkerEffect(false); hasMadeSound = true; }
        //    }
        //}
    }

    public void ResetHitmarker() { hasMadeSound = false; }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(impactPoint.position, length);
    //    //Gizmos.DrawRay(impactPoint.position, impactPoint.up);
    //}
}
