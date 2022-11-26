using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    #region Variables
    #region Basics
    [Header("Basics")]
    [Tooltip("This is the name of the gun. It's useful in the scripts")]
    public string gunName;
    [Tooltip("This is what model is Instantiated.")]
    public GameObject prefab;
    [Tooltip("This is not recoil. This is for things like shotguns.")]
    public float bulletSpread;
    [Tooltip("This controls how fast the gun fires.")]
    public float fireRate;
    [Tooltip("Make this true if the weapon is melee. If not, ignore this variable.")]
    public bool isMelee;
    #endregion

    #region Ammo
    [Header("Ammo")]
    [Tooltip("This controls how many bullets you can fire before you need to reload.")]
    public int magazineSize; //This doesn't change during play
    [Tooltip("This controls how many bullets you can hold in all.")]
    public int maxAmmoStash; //This doesn't change during play
    [Tooltip("This controls how many bullets you currently hold that are not in the magazine.")]
    public int currentAmmoStash;
    [Tooltip("This controls how many bullets you currently have in the magazine.")]
    public int currentBulletsInMagazine;
    #endregion

    #region Reloading
    [Header("Reloading")]
    [Tooltip("This controls how long the Weapon script waits before calculating the ammo stuff.")]
    public float reloadTime;
    #endregion

    #region Fire Type
    [Header("Fire Type")]
    [Tooltip("Turn this on if the weapon is automatic.")]
    public bool isAutomatic;
    [Tooltip("Turn this on if the weapon is burst. This can mean shotgun or burst depending on the timeBetweenBullets variable.")]
    public bool isBurst;
    [Tooltip("This controls how long the Weapon script waits before shooting the next bullet. Keep this at 0 for shotguns.")]
    public float timeBetweenBullets;
    [Tooltip("Keep this at one if the weapon is not a burst weapon. This controls how many bullets get shot.")]
    public int bulletsPerTap;
    //[Tooltip("This controls how long the Weapon script waits before shooting the next bullet. Keep this at 0 for shotguns.")]
    public float timeBetweenFiring;
    [Tooltip("Use this for pump shotguns, bolt-action rifles, etc.")]
    public bool useRecovery;
    #endregion

    #region Aiming
    [Header("Aiming")]
    [Tooltip("This controls how fast the gun goes to it's ADS and Hipfire positions.")]
    public float aimSpeed;
    [Tooltip("This controls how much the player's camera zooms when you ADS.")]
    public float playerCamZoomMultiplier;
    [Tooltip("This controls how much the weapon camera zooms when you ADS.")]
    public float weaponCamZoomMultiplier;
    [Tooltip("This controls how much the camera is able to rotate while ADSing. The higher the number, the lower the look speed.")]
    public float ADSSensitivityMultiplier;
    [Tooltip("This controls how much the current weapon sways wile aiming down sights.")]
    public float ADSSwayMultiplier;
    #endregion

    #region Bullet
    [Header("Bullet")]
    [Tooltip("This controls how much damage each bullet does.")]
    public int damage;
    [Tooltip("This controls how much a bullet pushes an object.")]
    public float bulletForce;
    [Tooltip("This controls what layers can be shot.")]
    public LayerMask canBeShot;
    [Tooltip("This controls how long the bullet trail is.")]
    public float trailTime;
    #endregion

    #region RecoilPattern
    [Header("Recoil Pattern")]
    public bool randomizeRecoil;
    public Vector2 randomRecoilConstraints;
    [Tooltip("You only need to assign this if randomizeRecoil is false.")]
    public Vector3[] recoilPattern;
    #endregion

    #region RecoilControl
    [Header("Recoil Control")]
    [Tooltip("This controls how fast the camera goes from the normal position to the new position that is set when the RecoilFire function is called in the Recoil script when hipfiring.")]
    public float snappiness;
    [Tooltip("This controls how fast the camera returns to the normal position when hipfiring.")]
    public float returnSpeed;
    [Tooltip("This is the same as the snappiness variable, except for firing when ADS-ing. This effects where the bullet goes.")]
    public float aimSnappiness;
    [Tooltip("This is the same as the returnSpeed variable, except for firing when ADS-ing. This effects where the bullet goes.")]
    public float aimReturnSpeed;
    #endregion

    #region Kickback
    [Header("Kickback")]
    [Tooltip("This controls how much the weapon rotates to kicks up visually.")]
    public float rotationalKickback;
    [Tooltip("This controls how fast the weapon returns from the rotated position to the normal position.")]
    public float rotationalKickbackReturnSpeed;
    [Tooltip("This controls how much the weapon moves to kick back visually.")]
    public float positionalKickback;
    [Tooltip("This controls how fast the weapon returns from the moved position to the normal position.")]
    public float positionalKickbackReturnSpeed;
    #endregion

    #region CameraShake
    [Header("Camera Shake")]
    public float magnitude;
    public float roughness;
    public float fadeInTime;
    public float fadeOutTime;
    #endregion

    //#region Hipfire Recoil
    //[Header("Hipfire Recoil")]
    //[Tooltip("This controls how much the camera rotates on the X axis (maybe) when hipfiring. This effects where the bullet goes.")]
    //public float recoilX;
    //[Tooltip("This controls how much the camera rotates on the Y axis (maybe)when hipfiring. This effects where the bullet goes.")]
    //public float recoilY;
    //[Tooltip("This controls how much the camera rotates on the Z axis (maybe)when hipfiring. This effects where the bullet goes.")]
    //public float recoilZ;
    //#endregion

    //#region ADS Recoil
    //[Header("ADS Recoil")]
    //[Tooltip("This is the same as the recoilX variable, except for firing when ADS-ing. This effects where the bullet goes.")]
    //public float aimRecoilX;
    //[Tooltip("This is the same as the recoilY variable, except for firing when ADS-ing. This effects where the bullet goes.")]
    //public float aimRecoilY;
    //[Tooltip("This is the same as the recoilZ variable, except for firing when ADS-ing. This effects where the bullet goes.")]
    //public float aimRecoilZ;
    //#endregion

    #region Decals
    [Header("Decals")]
    [Tooltip("This is the bullet hole prefab that is Instantiated.")]
    public GameObject bulletHolePrefab;
    [Tooltip("This is the muzzle flash effect that is Instantiated. Not implimented yet.")]
    public GameObject muzzleFlashPrefab;
    #endregion

    #region Audio
    [Header("Audio")]
    [Tooltip("This is the array of sounds that can be played when you shoot.")]
    public AudioClip[] gunshotSounds;
    [Tooltip("This is how much the gun shot sound can vary in pitch.")]
    public float pitchRandomization;
    [Tooltip("This is how loud the gun shot sound is.")]
    public float volume;
    #endregion
    #endregion

    #region Functions
    public void Initialize()
    {
        currentAmmoStash = maxAmmoStash;
        currentBulletsInMagazine = magazineSize;
    }

    public bool UpdateMagazine()
    {
        if (currentBulletsInMagazine > 0)
        {
            currentBulletsInMagazine -= bulletsPerTap;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        currentAmmoStash += currentBulletsInMagazine;
        currentBulletsInMagazine = Mathf.Min(magazineSize, currentAmmoStash);
        currentAmmoStash -= currentBulletsInMagazine;
    }

    public int GetStash() { return currentAmmoStash; }

    public int GetMag() { return currentBulletsInMagazine; }
    #endregion
}
