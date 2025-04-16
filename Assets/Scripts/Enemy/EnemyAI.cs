using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Alerted, Attacking, Searching }
    public EnemyState CurrentState = EnemyState.Idle;

    protected Transform player;
    protected NavMeshAgent agent;

    [Header("Combat / Vision Settings")]
    [SerializeField] protected float attackRange = 3f;        // how close enemy has to be to attack
    [SerializeField] protected float visionRange = 10f;       // how far the enemy can see
    [SerializeField] protected float visionAngle = 45f;       // how wide the vision cone is
    [SerializeField] protected float loseSightDelay = 3f;     // how long until they give up chasing

    [Header("Wandering Settings")]
    [SerializeField] protected float wanderRadius = 5f;       // how far they wander when idle
    [SerializeField] protected float wanderInterval = 3f;     // how often they pick a new spot
    protected float wanderTimer;

    [Header("Audio Settings")]
    [SerializeField] protected AudioSource audioSource;       // sound source
    [SerializeField] protected AudioClip[] idleSFXs;          // chill sounds
    [SerializeField] protected AudioClip[] walkingSFXs;       // footstep sounds
    [SerializeField] protected AudioClip[] chasingSFXs;       // intense chase noises

    protected bool playerVisible;                             // if we can currently see player
    private float timeSinceLastSeen = Mathf.Infinity;         // how long since we last saw 'em

    protected virtual void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Update()
    {
        playerVisible = CanSeePlayer();

        if (playerVisible)
        {
            timeSinceLastSeen = 0f;
            OnSeePlayer();
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
            wanderTimer += Time.deltaTime;
        }

        HandleState(); // call whatever behavior we’re doing rn
    }

    // checks if we can actually see the player
    protected virtual bool CanSeePlayer()
    {
        if (!player) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        float distance = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (distance <= visionRange && angle <= visionAngle)
        {
            if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer.normalized, out RaycastHit hit, visionRange))
            {
                if (hit.transform.CompareTag("Player"))
                    return true;
            }
        }

        return false;
    }

    // player spotted! if we're chillin, now we're alert
    protected virtual void OnSeePlayer()
    {
        if (CurrentState == EnemyState.Idle)
            CurrentState = EnemyState.Alerted;
    }

    // just checks if we've lost the player for too long
    protected bool LostPlayerToLong() => timeSinceLastSeen >= loseSightDelay;

    // abstract so subclasses can do their thing
    protected abstract void HandleState();

    // picks a random spot on navmesh in range
    protected Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        if (NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask))
            return navHit.position;

        return origin; // fallback if nothing good found
    }

    // plays a random clip from an array if nothing else is playing
    protected void PlayRandomSFX(AudioClip[] sfxArray)
    {
        if (sfxArray == null || sfxArray.Length == 0 || audioSource.isPlaying)
            return;

        int index = Random.Range(0, sfxArray.Length);
        audioSource.clip = sfxArray[index];
        audioSource.Play();
    }
}
