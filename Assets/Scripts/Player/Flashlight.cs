using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flashlight : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;

    [Header("Light Settings")]
    [SerializeField] private Light spotLight;
    public float maxPower = 100f;
    public float drainRate = 5f;

    [Header("State")]
    public bool flashlightOn = false;
    private float currentPower;

    private void Start()
    {
        currentPower = maxPower;
        ToggleFlashlight(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight(!flashlightOn);
        }

        if (flashlightOn)
        {
            DrainPower();
        }

        UpdateBar();
    }

    // Decrease flashlight power over time
    void DrainPower()
    {
        currentPower -= drainRate * Time.deltaTime;
        currentPower = Mathf.Clamp(currentPower, 0f, maxPower);

        if (currentPower <= 0f)
        {
            ToggleFlashlight(false);
        }
    }

    // Update UI bar based on current power
    void UpdateBar()
    {
        slider.value = currentPower / maxPower;
    }

    // Turn flashlight on/off
    public void ToggleFlashlight(bool state)
    {
        flashlightOn = state;
        spotLight.enabled = state;
    }
}
