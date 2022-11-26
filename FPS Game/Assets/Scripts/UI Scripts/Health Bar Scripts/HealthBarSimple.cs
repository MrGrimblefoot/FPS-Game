using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarSimple : MonoBehaviour
{
    private Image healthBar;
    [SerializeField] private float currentHealth;
    private float maxHealth = 100f;
    PlayerPolishManager player;

    void Start()
    {
        healthBar = GetComponent<Image>();
        player = FindObjectOfType<PlayerPolishManager>();
        maxHealth = player.maxHealth;
    }

    void Update()
    {
        currentHealth = player.currentHealth;
        healthBar.fillAmount = currentHealth / maxHealth;
    }
}
