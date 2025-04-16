using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    public event EventHandler OnPlayerDeath;

    [Header("Health Settings")]
    public float currentHealth;
    public float maxHealth = 100f;

    [Header("UI References")]
    public Slider slider;
    public TMP_Text healthText;

    [Header("Smoothing Settings")]
    private float sliderSmoothSpeed = 5f;
    private float sliderTargetHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // correct: destroy the duplicate gameObject, not the original Instance
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        sliderTargetHealth = maxHealth;

        slider.maxValue = maxHealth;
        slider.value = maxHealth;

        UpdateHealthText();
    }

    private void Update()
    {
        // Smoothly animate the slider to target value
        slider.value = Mathf.Lerp(slider.value, sliderTargetHealth, Time.deltaTime * sliderSmoothSpeed);
    }

    public void TakeDamage(float dmg)
    {
        // Only take damage in Normal state
        if (PlayerStateManager.Instance.currentState != PlayerState.Normal) return;

        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        sliderTargetHealth = currentHealth;
        UpdateHealthText();

        if (currentHealth <= 0f)
        {
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = $"Health: {currentHealth}/{maxHealth}";
    }
}
