using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class PlayerPolishManager : MonoBehaviourPunCallbacks
{
    #region Variables
    [Header("Health Parameters")]
    public int maxHealth = 100;
    public int currentHealth;
    [SerializeField] private float timeBeforeRegenStarts = 3f;
    [SerializeField] private int healthValueIncrement = 1;
    [SerializeField] private float healthTimeIncrement = 0.1f;
    public bool isDead;
    public bool canTakeDamage;
    //private Coroutine regeneratingHealth;
    public static Action<int> OnTakeDamage;
    public static Action<int> OnDamage;
    public static Action<int> OnHeal;
    public static Action OnHealToMax;
    HealthBarComplex healthBar;

    [Header("Headbob Parameters")]
    [SerializeField] private Transform weaponParent;
    [SerializeField] private float idleBobMultiplier;
    [SerializeField] private float idleXMultiplier;
    [SerializeField] private float idleYMultiplier;
    [SerializeField] private float idleTransitionSmooth;
    [SerializeField] private float movementBobMultiplier;
    [SerializeField] private float movementXMultiplier;
    [SerializeField] private float movementYMultiplier;
    [SerializeField] private float movementTransitionSmooth;
    [SerializeField] private float sprintBobMultiplier;
    [SerializeField] private float sprintXMultiplier;
    [SerializeField] private float sprintYMultiplier;
    [SerializeField] private float sprintTransitionSmooth;
    [SerializeField] private float crouchBobMultiplier;
    [SerializeField] private float crouchXMultiplier;
    [SerializeField] private float crouchYMultiplier;
    [SerializeField] private float crouchTransitionSmooth;
    private Vector3 weaponParentOrigin;
    private Vector3 targetWeaponBob;
    //private float idleCounter = 0f;
    //private float movementCounter = 0f;
    //private float sprintCounter = 0f;
    //private float crouchCounter = 0f;

    #region Outdated Headbob Parameters
    //[Header("Headbob Parameters")]
    //[SerializeField] private float walkBobSpeed = 14f;
    //[SerializeField] private float walkBobAmount = 0.5f;
    //[SerializeField] private float sprintBobSpeed = 18f;
    //[SerializeField] private float sprintBobAmount = 1f;
    //[SerializeField] private float crouchBobSpeed = 8f;
    //[SerializeField] private float crouchBobAmount = 0.25f;
    //private float defaultYPos = 0;
    //private float timer;
    #endregion

    [Header("Footstep Parameters")]
    [SerializeField] private bool useFootsteps;
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] metalClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    [SerializeField] private AudioClip[] concreteClips = default;
    private float footstepTimer = 0f;
    private float GetCurrentOffset => movement.isCrouching ? baseStepSpeed * crouchStepMultiplier : movement.isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    ////Sliding Parameters
    //private Vector3 hitPointNormal;
    //#region isSlidingBool
    //private bool isSliding
    //{
    //    get
    //    {
    //        if (controller.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
    //        {
    //            hitPointNormal = slopeHit.normal;
    //            return Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //}
    //#endregion

    [Header("UI Parameters")]
    private Transform uiHealthBar;
    private Image damageRecievedIndicator;
    private float damageRecievedIndicatorWaitTime;
    private Color transparentRed = new Color(1, 0, 0, 0);

    SpawnManager spawnManager;
    PlayerMovement movement;

    #endregion

    new private void OnEnable()
    {
        if (!photonView.IsMine) { return; }
        OnTakeDamage += ApplyDamage;
        OnHealToMax += HealToMax;
    }

    new private void OnDisable()
    {
        if (!photonView.IsMine) { return; }
        OnTakeDamage -= ApplyDamage;
        OnHealToMax -= HealToMax;
    }

    void Awake()
    {

        if (!photonView.IsMine) { ChangeLayersRecursively(gameObject, 7); }
        if (!photonView.IsMine) { return; }
        weaponParentOrigin = weaponParent.localPosition;
        currentHealth = maxHealth;
        healthBar = FindObjectOfType<HealthBarComplex>();
        uiHealthBar = GameObject.Find("HUD/Healthbar/Health").transform;
        damageRecievedIndicator = GameObject.Find("HUD/Recieve Hit Effect").GetComponent<Image>();
        damageRecievedIndicator.color = Color.clear;
        spawnManager = GameObject.Find("Multiplayer Objects/Manager").GetComponent<SpawnManager>();
        movement = GetComponent<PlayerMovement>();
        UpdateHealthBar();
    }

    void Update()
    {
        if (!photonView.IsMine) { return; }
        if (useFootsteps) { HandleFootsteps(); }

        UpdateHealthBar();

        if (damageRecievedIndicatorWaitTime > 0) { damageRecievedIndicatorWaitTime -= Time.deltaTime; }
        else if (damageRecievedIndicator.color.a > 0) { damageRecievedIndicator.color = Color.Lerp(damageRecievedIndicator.color, transparentRed, Time.deltaTime * 8); }
        if (transform.position.y <= -10) { PlayerDie(); }
    }

    //private void HandleHeadbob()
    //{
    //    if (!controller.isGrounded) { return; }

    //    if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
    //    {
    //        timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
    //        playerCamera.transform.localPosition = new Vector3(
    //            playerCamera.transform.localPosition.x,
    //            defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount),
    //            playerCamera.transform.localPosition.z);
    //    }
    //}

    void Headbob(float parameterZ, float xIntensity, float yIntensity)
    {
        float t_aim_adjust = 1f;
        if (movement.isAiming) { t_aim_adjust = 0.1f; }
        targetWeaponBob = weaponParentOrigin + new Vector3(Mathf.Cos(parameterZ) * xIntensity * t_aim_adjust, Mathf.Sin(parameterZ * 2) * yIntensity * t_aim_adjust, 0);
    }

    private void UpdateHealthBar()
    {
        float tempHealthRatio = (float)currentHealth / (float)maxHealth;
        uiHealthBar.localScale = Vector3.Lerp(uiHealthBar.localScale, new Vector3(tempHealthRatio, 1, 1), 3f);
    }

    public void ApplyDamage(int damage)
    {
        if (photonView.IsMine)
        {
            if (canTakeDamage)
            {
                currentHealth -= damage;
                UpdateHealthBar();

                //if (no one is listening for OnDamage) { return; } else { Invoke(currentHealth); }
                OnDamage?.Invoke(currentHealth);

                if (currentHealth <= 0) { PlayerDie(); }
                //else if (regeneratingHealth != null) { StopCoroutine(regeneratingHealth); }

                //regeneratingHealth = StartCoroutine(RegenerateHealth());

                //healthBar.UpdateHealthUI();
                //healthBar.ResetHealthLerpTimer();
                damageRecievedIndicator.color = Color.red;
                damageRecievedIndicatorWaitTime += 0.2f;

                Debug.Log("I took " + damage + " damage.");
            }
        }
    }

    private void PlayerDie()
    {
        currentHealth = 0;
        movement.isAiming = false;
        damageRecievedIndicator.color = transparentRed;
        UpdateHealthBar();
        isDead = true;

        //if(regeneratingHealth != null) { StopCoroutine(regeneratingHealth); }

        //weaponManager.PlayerIsDead();
        spawnManager.HandleSpawn();
        PhotonNetwork.Destroy(gameObject);
    }

    private void HealToMax() { currentHealth = maxHealth; isDead = false; }

    [PunRPC]
    private void HandleFootsteps()
    {
        if (!movement.grounded || movement.currentInput == Vector2.zero) { return; }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    case "Footsteps/Wood":
                        footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length - 1)]);
                        break;
                    case "Footsteps/Metal":
                        footstepAudioSource.PlayOneShot(metalClips[UnityEngine.Random.Range(0, metalClips.Length - 1)]);
                        break;
                    case "Footsteps/Grass":
                        footstepAudioSource.PlayOneShot(grassClips[UnityEngine.Random.Range(0, grassClips.Length - 1)]);
                        break;
                    case "Footsteps/Concrete":
                        footstepAudioSource.PlayOneShot(concreteClips[UnityEngine.Random.Range(0, concreteClips.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(concreteClips[UnityEngine.Random.Range(0, concreteClips.Length - 1)]);
                        break;
                }
            }

            footstepTimer = GetCurrentOffset;
        }
    }

    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(timeBeforeRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while (currentHealth < maxHealth)
        {
            currentHealth += healthValueIncrement;
            healthBar.ResetHealthLerpTimer();

            if (currentHealth > maxHealth) { currentHealth = maxHealth; }

            //if (no one is listening for OnHeal) { return; } else { Invoke(currentHealth); }
            OnHeal?.Invoke(currentHealth);
            yield return timeToWait;
        }

        //regeneratingHealth = null;
    }

    private void ChangeLayersRecursively(GameObject p_target, int p_layer)
    {
        p_target.layer = p_layer;
        foreach (Transform a in p_target.transform) { ChangeLayersRecursively(a.gameObject, p_layer); }
    }
}
