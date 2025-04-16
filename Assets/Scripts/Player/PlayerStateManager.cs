using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    #region Singleton
    public static PlayerStateManager Instance { get; private set; }
    #endregion

    #region Player State
    public PlayerState currentState { get; private set; } = PlayerState.Normal;
    private PlayerState lastState;
    #endregion

    #region UI & Cursor
    [SerializeField] private GameObject pauseUI;
    #endregion

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        SetState(currentState);  // Set initial player state
    }

    private void Start()
    {
        // Subscribe to player death event
        PlayerHealth.Instance.OnPlayerDeath += PlayerHealth_OnPlayerDeath;
    }

    private void Update()
    {
        // Handle pausing/unpausing when escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == PlayerState.Normal)
            {
                SetState(PlayerState.Paused);
            }
            else
            {
                UnpauseGame();
            }
        }
    }

    // Event handler for when player dies
    private void PlayerHealth_OnPlayerDeath(object sender, EventArgs e)
    {
        SetState(PlayerState.Dead);  // Set player state to Dead
    }

    // Set a new state for the player
    public void SetState(PlayerState newState)
    {
        lastState = currentState;  // Save the last state
        currentState = newState;    // Update current state

        print($"State changed: {lastState} -> {currentState}");

        switch (newState)
        {
            case PlayerState.Normal:
                pauseUI.SetActive(false);  // Hide pause UI
                LockCursor(true);          // Lock the cursor for gameplay
                break;
            case PlayerState.Paused:
                pauseUI.SetActive(true);   // Show pause UI
                LockCursor(false);         // Unlock cursor during pause
                break;
            case PlayerState.Dead:
                pauseUI.SetActive(true);   // Show pause UI even when dead
                LockCursor(false);         // Unlock cursor
                break;
        }
    }

    // Unpause the game, restoring the previous state
    public void UnpauseGame()
    {
        print("Unpausing the game...");
        SetState(lastState);
    }

    // Lock or unlock the cursor based on game state
    public void LockCursor(bool lockCur)
    {
        Cursor.visible = !lockCur;
        Cursor.lockState = lockCur ? CursorLockMode.Locked : CursorLockMode.None;
    }

    // Close the game (useful for quitting the game)
    public void CloseGame()
    {
        Application.Quit();
    }
}
