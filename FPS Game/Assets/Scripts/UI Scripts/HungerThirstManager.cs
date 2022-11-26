using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HungerThirstManager : MonoBehaviourPunCallbacks
{
    public float maxHunger;
    public float currentHunger;
    [SerializeField] private float hungerIncreaseRate;
    public float maxThirst;
    public float currentThirst;
    [SerializeField] private float thirstIncreaseRate;
    [SerializeField] private int hungerDamage;
    [SerializeField] private int thirstDamage;
    public bool thirstDamageActive;
    public bool hungerDamageActive;
    private float timer;
    private float hungerDamageTimer;
    private float thirstDamageTimer;

    PlayerPolishManager player;
    HealthBarComplex healthBar;

    void Start()
    {
        if (!photonView.IsMine) { return; }
        player = GetComponent<PlayerPolishManager>();
        healthBar = FindObjectOfType<HealthBarComplex>();
        currentHunger = maxHunger;
        currentThirst = maxThirst;
    }

    void Update()
    {
        if (!photonView.IsMine) { return; }
        if (!player.isDead)
        {
            timer += Time.deltaTime;
            if(timer >= 1)
            {
                currentHunger -= hungerIncreaseRate;
                healthBar.ResetHungerLerpTimer();
                currentThirst -= thirstIncreaseRate;
                healthBar.ResetThirstLerpTimer();
                timer = 0;
            }
        }

        if (currentHunger <= 0)
        {
            hungerDamageTimer += Time.deltaTime;
            if (hungerDamageTimer >= 1)
            {
                //Debug.Log("Starting Hunger Damage!");
                hungerDamageActive = true;
                StartCoroutine(HungerPains(hungerDamage));
                hungerDamageTimer = 0;
            }
        }
        else
        {
            //Debug.Log("Hunger Damage Is No More!");
            hungerDamageActive = false;
            StopCoroutine(HungerPains(thirstDamage));
            hungerDamageTimer = 0;
        }

        if (currentThirst <= 0)
        {
            thirstDamageTimer += Time.deltaTime;
            if (thirstDamageTimer >= 1)
            {
                //Debug.Log("Starting Thirst Damage!");
                thirstDamageActive = true;
                StartCoroutine(ThirstPains(thirstDamage));
                thirstDamageTimer = 0;
            }
        }
        else
        {
            //Debug.Log("Thirst Damage Is No More!");
            thirstDamageActive = false;
            StopCoroutine(ThirstPains(thirstDamage));
            thirstDamageTimer = 0;
        }

    }

    private IEnumerator HungerPains(int damage)
    {
        //Debug.Log("Taking Hunger Damage!");
        PlayerPolishManager.OnTakeDamage(damage);
        yield return new WaitForSeconds(3f);
    }
    private IEnumerator ThirstPains(int damage)
    {
        //Debug.Log("Taking Thirst Damage!");
        PlayerPolishManager.OnTakeDamage(damage);
        yield return new WaitForSeconds(3f);
    }
}
