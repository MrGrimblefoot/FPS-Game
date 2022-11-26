using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EZCameraShake;

public class WeaponSystem : MonoBehaviourPunCallbacks
{
    #region Variables

    #region Loadout
    [Header("Guns")]
    /*[SerializeField] private*/ public Weapon[] loadout;
    [HideInInspector] public Weapon currentGunData;
    public GameObject currentWeapon;
    #endregion

    #region Shooting
    private int bulletsToShoot;
    private Camera cam;
    private Camera weaponCam;
    private GameObject sniperCam;
    private RaycastHit hit;
    private bool shooting;
    private bool readyToShoot;
    public bool isReloading;
    private float currentCooldown;
    #endregion

    #region Inputs
    private PlayerInput playerInput;
    private BasicInputActions basicInputActions;
    [SerializeField] private bool canReciveInput;
    #endregion

    #region Aiming
    [HideInInspector]
    public bool aiming;
    private float targetFOV;
    private float weaponTargetFOV;
    [SerializeField] private float normalFOV = 60;
    private CameraLook camLook;
    #endregion

    #region Recoil
    private Recoil recoilScript;
    #endregion

    #region Equiping
    [Header("Equiping")]
    [SerializeField] private Transform weaponParent;
    private int currentIndex;
    #endregion

    #region UI
    private GameObject cursor;
    private TextMeshProUGUI ammoCounterText;
    private Image hitMarkerImage;
    private float hitMarkerWaitTime;
    private Color transparentWhite = new Color(1, 1, 1, 0);
    #endregion

    #region SFX
    [Header("SFX")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip hitMarkerSound;
    #endregion

    #region VFX
    [Header("VFX")]
    private Transform firePoint;
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color headshotColor;
    #endregion
    #endregion

    #region MonoBehaviour Callbacks
    private void OnEnable()
    {
        basicInputActions.Player.Reload.Enable();
        basicInputActions.Player.Fire.Enable();
        basicInputActions.Player.Weapon1.Enable();
        basicInputActions.Player.Weapon2.Enable();
    }

    private void OnDisable()
    {
        basicInputActions.Player.Reload.Disable();
        basicInputActions.Player.Fire.Disable();
        basicInputActions.Player.Weapon1.Disable();
        basicInputActions.Player.Weapon2.Disable();

    }

    private void Awake()
    {
        if (!photonView.IsMine) { readyToShoot = false; return; }
        cursor = GameObject.Find("HUD/Crosshair");
        recoilScript = transform.Find("Cameras/CameraRecoil").GetComponent<Recoil>();
        camLook = GetComponent<CameraLook>();
        readyToShoot = false;
        cam = GetComponentInChildren<Camera>();
        cam = transform.Find("Cameras/CameraRecoil/CameraShaker/Player Camera").GetComponent<Camera>();
        weaponCam = transform.Find("Cameras/CameraRecoil/CameraShaker/Weapon Camera").GetComponent<Camera>();
        hitMarkerImage = GameObject.Find("HUD/Hit Marker").GetComponent<Image>();
        hitMarkerImage.color = transparentWhite;
        ammoCounterText = GameObject.Find("HUD/Ammo Display").GetComponent<TextMeshProUGUI>();
        canReciveInput = true;
        playerInput = GetComponent<PlayerInput>();

        #region InputActions
        basicInputActions = new BasicInputActions();
        basicInputActions.Player.Reload.performed += Reload;
        basicInputActions.Player.Weapon1.performed += PhotonEquip1;
        basicInputActions.Player.Weapon2.performed += PhotonEquip2;

        #endregion

        foreach (Weapon g in loadout) { g.Initialize(); }
        photonView.RPC("Equip", RpcTarget.All, 0);

        firePoint = currentWeapon.transform.Find("Anchor/Fire Point");
    }

    private void Update()
    {
        if (!photonView.IsMine) { return; }
        normalFOV = camLook.fieldOfView;
        if (currentWeapon != null && !currentGunData.isMelee) { ammoCounterText.SetText(currentGunData.GetMag() / currentGunData.bulletsPerTap + " / " + currentGunData.GetStash() / currentGunData.bulletsPerTap); }
        else { ammoCounterText.SetText(" "); }

        if (currentWeapon != null)
        {
            //if (currentGunData.isAutomatic) { shooting = basicInputActions.Player.Movement.ReadValue<bool>(); }
            //else { shooting = Input.GetKeyDown(shootButton); }

            if (currentGunData.name != "Sniper")
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * currentGunData.aimSpeed);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, weaponTargetFOV, Time.deltaTime * currentGunData.aimSpeed);
            }
        }

        if(currentCooldown > 0) { currentCooldown -= Time.deltaTime; }

        if (hitMarkerWaitTime > 0) { hitMarkerWaitTime -= Time.deltaTime; }
        else if (hitMarkerImage.color.a > 0) { hitMarkerImage.color = Color.Lerp(hitMarkerImage.color, transparentWhite, Time.deltaTime * 8); }

        if (canReciveInput) { HandleInput(); }
    }

    #endregion

    #region Private Methods
    private void HandleInput()
    {
        if (currentWeapon != null)
        {
            if (photonView.IsMine)
            {
                if (currentGunData.isMelee) { photonView.RPC("Attack", RpcTarget.All); }

                if (readyToShoot && shooting && !isReloading && currentCooldown <= 0 && currentGunData.currentBulletsInMagazine > 0)
                {
                    if (currentGunData.UpdateMagazine()) { bulletsToShoot = currentGunData.bulletsPerTap; photonView.RPC("Shoot", RpcTarget.All); }
                }

                if (readyToShoot && shooting && !isReloading && currentCooldown <= 0 && currentGunData.currentBulletsInMagazine <= 0 && currentGunData.currentAmmoStash > 0) { StartCoroutine(HandleReload()); }

                //Aim(Input.GetMouseButton(1));

            }

            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * currentGunData.positionalKickbackReturnSpeed);
            //currentWeapon.transform.localRotation = Quaternion.Lerp(currentWeapon.transform.localRotation, Quaternion.identity, Time.deltaTime * currentGunData.rotationalKickbackReturnSpeed);
        }
    }

    private void PhotonEquip1(InputAction.CallbackContext context) { photonView.RPC("Equip", RpcTarget.All, 0); }
    private void PhotonEquip2(InputAction.CallbackContext context) { photonView.RPC("Equip", RpcTarget.All, 1); }

    [PunRPC]
    void Equip(int p_ind)
    {
        if (isReloading) { return; }
        if(currentWeapon != null) { Destroy(currentWeapon); }

        currentIndex = p_ind;

        GameObject t_newWeapon = Instantiate(loadout[currentIndex].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;
        t_newWeapon.GetComponentInChildren<Sway>().isMine = photonView.IsMine;

        if (photonView.IsMine) { ChangeLayersRecursively(t_newWeapon, 8); }
        else { ChangeLayersRecursively(t_newWeapon, 0); }

        currentWeapon = t_newWeapon;
        currentGunData = loadout[currentIndex];
        recoilScript.gun = currentGunData;

        if (!loadout[currentIndex].isMelee)
        {
            //if (loadout[i].name == "Sniper")
            //{
            //    sniperCam = currentWeapon.transform.Find("Anchor/Sniper Prefab/Scope Effect/Camera").gameObject;
            //    sniperCam.GetComponent<Camera>().fieldOfView = currentGunData.playerCamZoomMultiplier;
            //    sniperCam.gameObject.SetActive(false);
            //}
            /*else { */
            sniperCam = null;/* }*/
        }

        targetFOV = 60;
        weaponTargetFOV = 60;

        firePoint = loadout[currentIndex].prefab.transform.Find("Anchor/Fire Point");

        if (!photonView.IsMine) { return; }
        if (currentGunData.isAutomatic || currentGunData.isMelee)
        {
            basicInputActions.Player.Fire.started += _ => shooting = true;
            basicInputActions.Player.Fire.canceled += _ => shooting = false;
        }
        else { basicInputActions.Player.Fire.performed += _ => shooting = true; }

        targetFOV = 60;
        weaponTargetFOV = 60;
        readyToShoot = true;
    }

    private void ChangeLayersRecursively(GameObject p_target, int p_layer)
    {
        p_target.layer = p_layer;
        foreach(Transform a in p_target.transform) { ChangeLayersRecursively(a.gameObject, p_layer); }
    }

    private void Reload(InputAction.CallbackContext context)
    {
        if (photonView.IsMine)
        {
            if (!isReloading && currentGunData.GetMag() <= currentGunData.magazineSize - 1 && currentGunData.GetStash() > 0) { StartCoroutine(HandleReload()); }
        }
    }

    private IEnumerator HandleReload()
    {
        isReloading = true;
        currentWeapon.SetActive(false);
        //Debug.Log("Wating " + currentGunData.reloadTime + " second(s) to reload!");

        yield return new WaitForSeconds(currentGunData.reloadTime);

        //Debug.Log("Reloading!");
        currentGunData.Reload();
        currentWeapon.SetActive(true);
        isReloading = false;
    }

    public void Aim(bool isAiming)
    {
        if(!currentWeapon) { return; }

        aiming = isAiming;

        //find the Anchor
        Transform tempAnchor = currentWeapon.transform.Find("Anchor");
        Transform tempStateADS = currentWeapon.transform.Find("States/ADS");
        Transform tempStateHip = currentWeapon.transform.Find("States/Hip");

        //determine gun position
        if (!currentGunData.isMelee)
        {
            if (isAiming)
            {
                tempAnchor.position = Vector3.Lerp(tempAnchor.position, tempStateADS.position, Time.deltaTime * currentGunData.aimSpeed);
                cursor.SetActive(false);
                if (currentGunData.name != "Sniper")
                {
                    targetFOV = normalFOV / currentGunData.playerCamZoomMultiplier;
                    weaponTargetFOV = normalFOV / currentGunData.weaponCamZoomMultiplier;
                }
                else { sniperCam.gameObject.SetActive(true); }
            }
            else
            {
                tempAnchor.position = Vector3.Lerp(tempAnchor.position, tempStateHip.position, Time.deltaTime * currentGunData.aimSpeed);
                cursor.SetActive(true);
                if (currentGunData.name != "Sniper") { targetFOV = normalFOV; weaponTargetFOV = normalFOV; }
                else { sniperCam.gameObject.SetActive(false); }
            }
        }
        else { targetFOV = normalFOV; weaponTargetFOV = normalFOV; }
    }

    [PunRPC]
    private void Attack()
    {
        if(shooting == true && aiming == true) { shooting = false; aiming = false; }
        currentWeapon.GetComponentInChildren<Animator>().SetBool("Light Attack", shooting);
        currentWeapon.GetComponentInChildren<Animator>().SetBool("Heavy Attack", aiming);
    }

    [PunRPC]
    private void Shoot()
    {
        if (currentGunData.isMelee) { return; }
        cam = transform.Find("Cameras/CameraRecoil/CameraShaker/Player Camera").GetComponent<Camera>();

        readyToShoot = false;

        //cooldown
        currentCooldown = currentGunData.fireRate;

        //bullet spread
        Vector3 tempSpread = cam.transform.position + cam.transform.forward * 1000f;
        tempSpread += Random.Range(-currentGunData.bulletSpread, currentGunData.bulletSpread) * cam.transform.up;
        tempSpread += Random.Range(-currentGunData.bulletSpread, currentGunData.bulletSpread) * cam.transform.right;
        tempSpread -= cam.transform.position;
        tempSpread.Normalize();

        //raycast
        if (Physics.Raycast(cam.transform.position, tempSpread, out hit, 1000f, currentGunData.canBeShot))
        {
            Debug.Log(hit.collider.name);

            //TrailRenderer trail = Instantiate(bulletTrail, firePoint.position, firePoint.rotation);
            //StartCoroutine(SpawnTrail(trail, hit));

            if (hit.collider.transform.gameObject.layer == 12)
            {
                hit.collider.GetComponent<EnemyBodyPartHealthManager>().DamageEnemyPart(currentGunData.damage);
                if (hit.collider.CompareTag("EnemyHead")) { HitmarkerEffect(true); }
                else { HitmarkerEffect(false); }
            }
            if (hit.rigidbody != null) { hit.rigidbody.AddForceAtPosition(cam.transform.forward * (currentGunData.bulletForce * 1000), hit.point); }
            ApplyBulletHole();

            //if we are shooting another player (not ourselves), deal damage to them equal to the damage set in the Gun ScriptableObject, and trigger the hit marker.
            if (photonView.IsMine)
            {
                if (hit.collider.gameObject.layer == 7)
                {
                    hit.collider.gameObject.GetPhotonView().RPC("DamageEnemyPlayer", RpcTarget.All, currentGunData.damage);

                    HitmarkerEffect(false);
                }
            }
        }

        bulletsToShoot--;

        if (bulletsToShoot > 0) { Invoke("Shoot", currentGunData.timeBetweenBullets/* / 60*/); }

        if (!IsInvoking("ResetShot") && !readyToShoot)
        {
            Invoke("ResetShot", currentGunData.timeBetweenFiring);
        }
        if (photonView.IsMine && bulletsToShoot == 0)
        {
            //gunshot sound
            photonView.RPC("PlayGunshotSound", RpcTarget.All);
            //recoil
            recoilScript.RecoilFire();
            CameraShaker.Instance.ShakeOnce(currentGunData.magnitude, currentGunData.roughness, currentGunData.fadeInTime, currentGunData.fadeOutTime);
        }

        if (currentGunData.useRecovery) { /*currentWeapon.GetComponent<Animator>().Play("Recovery", 0, 0);*/ }
    }

    public void HitmarkerEffect(bool heashot)
    {
        if (heashot)
        {
            hitMarkerImage.color = headshotColor;
            sfx.PlayOneShot(hitMarkerSound, 1);
            hitMarkerWaitTime = 0.25f;
        }
        else
        {
            hitMarkerImage.color = normalColor;
            sfx.PlayOneShot(hitMarkerSound, 1);
            hitMarkerWaitTime = 0.25f;
        }
    }

    [PunRPC]
    private void PlayGunshotSound()
    {
        sfx.clip = currentGunData.gunshotSounds[Random.Range(0, currentGunData.gunshotSounds.Length - 1)];
        sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
        sfx.PlayOneShot(sfx.clip, currentGunData.volume);
    }

    private void ResetShot() { readyToShoot = true; if (!currentGunData.isAutomatic) { shooting = false; } }

    private void ApplyBulletHole()
    {
        GameObject bulletHole = Instantiate(currentGunData.bulletHolePrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
        bulletHole.transform.parent = hit.collider.gameObject.transform;
        Destroy(bulletHole, 4f);
    }

    [PunRPC]
    private void DamageEnemyPlayer(int damage)
    {
        GetComponent<PlayerPolishManager>().ApplyDamage(damage);
        //Debug.Log(GetComponent<PlayerController>().currentHealth);
    }

    //private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit)
    //{
    //    float time = 0;
    //    Vector3 startPosition = Trail.transform.position;
        
    //    while (time < 1)
    //    {
    //        Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
    //        time += Time.deltaTime / currentGunData.trailTime;

    //        yield return null;
    //    }

    //    Trail.transform.position = Hit.point;
    //    //Instantiate(impactParticalSystem, Hit.point, Quaternion.LookRotation(Hit.normal));

    //    Destroy(Trail.gameObject, Trail.time);
    //}
    #endregion
}
