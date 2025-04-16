using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mushling : EnemyAI
{
    [Header("Movement Settings")]
    [SerializeField] protected float normalSpeed = 2f;     // default speed
    [SerializeField] protected float chaseSpeed = 5f;      // speed when chasing

    [Header("Combat Settings")]
    [SerializeField] protected float damageAmount = 10f;   // how much damage it does
    private float attackCooldown;

    [Header("Visual FX")]
    [SerializeField] protected GameObject[] eyes;          // eye meshes for glow effect
    [SerializeField] protected Light eyeLight;             // light to turn on when chasing

    [Header("Audio Settings")]
    private float idleSFXCooldown;

    private Vector3 lastKnownPosition;

    protected override void Awake()
    {
        base.Awake();
        agent.speed = normalSpeed;
    }

    protected override void HandleState()
    {
        print(CurrentState); // debug print
        switch (CurrentState)
        {
            case EnemyState.Idle:
                IdleBehavior();
                break;
            case EnemyState.Alerted:
                StartChasing();
                break;
            case EnemyState.Attacking:
                if (playerVisible)
                {
                    lastKnownPosition = player.position;
                    agent.SetDestination(player.position);
                    AttackPlayer();
                }
                else if (LostPlayerToLong())
                {
                    CurrentState = EnemyState.Searching;
                }
                break;
            case EnemyState.Searching:
                SearchBehavior();
                break;
        }
    }

    protected override void OnSeePlayer()
    {
        agent.speed = chaseSpeed;
        EnableGlowingEyes();

        if (CurrentState == EnemyState.Idle)
            CurrentState = EnemyState.Alerted;

        base.OnSeePlayer();
    }

    private void IdleBehavior()
    {
        // move to random spot every few seconds
        if (wanderTimer >= wanderInterval)
        {
            Vector3 wanderTarget = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(wanderTarget);
            wanderTimer = 0f;
        }

        // play ambient idle sounds occasionally
        idleSFXCooldown -= Time.deltaTime;
        if (idleSFXCooldown < 0f)
        {
            PlayRandomSFX(idleSFXs);
            idleSFXCooldown = Random.Range(3f, 10f);
        }
    }

    private void StartChasing()
    {
        if (!player) return;

        lastKnownPosition = player.position;
        agent.SetDestination(player.position);
        CurrentState = EnemyState.Attacking;
    }

    private void AttackPlayer()
    {
        attackCooldown -= Time.deltaTime;

        if (playerVisible && attackCooldown < 0f)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                PlayerHealth.Instance?.TakeDamage(damageAmount);
                attackCooldown = Random.Range(0.2f, 0.5f); // basic attack cooldown
            }
        }
    }

    private void SearchBehavior()
    {
        agent.SetDestination(lastKnownPosition);

        if (Vector3.Distance(transform.position, lastKnownPosition) < 1.5f)
        {
            agent.speed = normalSpeed;
            DisableGlowingEyes();
            CurrentState = EnemyState.Idle;
        }
    }

    private void EnableGlowingEyes()
    {
        foreach (var eye in eyes)
        {
            eye.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        }
        eyeLight.gameObject.SetActive(true);
    }

    private void DisableGlowingEyes()
    {
        foreach (var eye in eyes)
        {
            eye.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
        }
        eyeLight.gameObject.SetActive(false);
    }
}
