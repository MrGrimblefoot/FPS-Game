using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarComplex : MonoBehaviour
{
    private float healthLerpTimer;
    private float hungerLerpTimer;
    private float thirstLerpTimer;
    private float health;
    private float maxHealth;
    private float hunger;
    private float maxHunger;
    private float thirst;
    private float maxThirst;
    [SerializeField] private float chipSpeed = 2;
    [SerializeField] private float followUpSpeed = 0.1f;
    PlayerPolishManager player;
    HungerThirstManager hungerThirstManager;
    [SerializeField] private Image frontHealthBar;
    [SerializeField] private Image backHealthBar;
    [SerializeField] private Image frontHungerBar;
    [SerializeField] private Image backHungerBar;
    [SerializeField] private Image frontThirstBar;
    [SerializeField] private Image backThirstBar;

    void Start()
    {
        player = FindObjectOfType<PlayerPolishManager>();
        hungerThirstManager = FindObjectOfType<HungerThirstManager>();
        maxHealth = player.maxHealth;
        health = maxHealth;
        maxHunger = hungerThirstManager.maxHunger;
        hunger = maxHunger;
        maxThirst = hungerThirstManager.maxThirst;
        thirst = maxThirst;
    }

    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
        UpdateHungerUI();
        UpdateThirstUI();
    }

    public void UpdateHealthUI()
    {
        float fillF = frontHealthBar.fillAmount;
        float fillB = backHealthBar.fillAmount;
        float hFraction = health / maxHealth;

        //if taking damage
        if (fillB > hFraction)
        {
            frontHealthBar.fillAmount = hFraction;
            backHealthBar.color = Color.red;
            healthLerpTimer += Time.deltaTime;
            float percentComplete = healthLerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            backHealthBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }

        //if healing
        if(fillF < hFraction)
        {
            backHealthBar.color = Color.green;
            backHealthBar.fillAmount = hFraction;
            healthLerpTimer += Time.deltaTime;
            float percentComplete = healthLerpTimer / chipSpeed;
            percentComplete = /*(*/percentComplete * percentComplete/*) / followUpSpeed*/;
            frontHealthBar.fillAmount = Mathf.Lerp(fillF, backHealthBar.fillAmount, percentComplete);
        }

        health = player.currentHealth;
    }

    public void UpdateHungerUI()
    {
        float fillF = frontHungerBar.fillAmount;
        float fillB = backHungerBar.fillAmount;
        float hFraction = hunger / maxHunger;

        //if losing hunger
        if (fillB > hFraction)
        {
            frontHungerBar.fillAmount = hFraction;
            backHungerBar.color = Color.yellow;
            hungerLerpTimer += Time.deltaTime;
            float percentComplete = hungerLerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            backHungerBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }

        //if gaining hunger
        if (fillF < hFraction)
        {
            backHungerBar.color = Color.red;
            backHungerBar.fillAmount = hFraction;
            hungerLerpTimer += Time.deltaTime;
            float percentComplete = hungerLerpTimer / chipSpeed;
            percentComplete = (percentComplete * percentComplete) / followUpSpeed;
            frontHungerBar.fillAmount = Mathf.Lerp(fillF, backHungerBar.fillAmount, percentComplete);
        }

        hunger = hungerThirstManager.currentHunger;
    }

    public void UpdateThirstUI()
    {
        float fillF = frontThirstBar.fillAmount;
        float fillB = backThirstBar.fillAmount;
        float hFraction = thirst / maxThirst;

        //if losing thirst
        if (fillB > hFraction)
        {
            frontThirstBar.fillAmount = hFraction;
            backThirstBar.color = Color.green;
            thirstLerpTimer += Time.deltaTime;
            float percentComplete = thirstLerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            backThirstBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }

        //if gaining thirst
        if (fillF < hFraction)
        {
            backThirstBar.color = Color.red;
            backThirstBar.fillAmount = hFraction;
            thirstLerpTimer += Time.deltaTime;
            float percentComplete = thirstLerpTimer / chipSpeed;
            percentComplete = (percentComplete * percentComplete) / followUpSpeed;
            frontThirstBar.fillAmount = Mathf.Lerp(fillF, backThirstBar.fillAmount, percentComplete);
        }

        thirst = hungerThirstManager.currentThirst;
    }

    public void ResetHealthLerpTimer()
    {
        healthLerpTimer = 0f;
    }
    public void ResetHungerLerpTimer()
    {
        hungerLerpTimer = 0f;
    }
    public void ResetThirstLerpTimer()
    {
        thirstLerpTimer = 0f;
    }
}
