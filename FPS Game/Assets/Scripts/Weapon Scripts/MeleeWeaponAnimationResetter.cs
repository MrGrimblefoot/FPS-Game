using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponAnimationResetter : MonoBehaviour
{
    [SerializeField] private Animator anim;
    public void ResetAttack() { anim.ResetTrigger("Light Attack"); anim.ResetTrigger("Heavy Attack"); }
}
