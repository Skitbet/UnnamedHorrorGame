using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Player Settings
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float acceleration = 10f;
    public float jumpForce = 8f;
    public float groundCheckDistance = 1.1f;
    public LayerMask groundMask;

    [Header("Camera Settings")]
    public float mouseSensitivity = 500f;
    private float verticalLookLimit = 70f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource walkingAudioSource;
    [SerializeField] private AudioClip[] walkClips;
    private float walkSoundTimer = 0f;
    private float walkSoundInterval = 0.4f;
    private int currentWalkClipIndex = 0;
    #endregion

    #region Player References
    [SerializeField] private Transform headTransform;
    private Camera playerCamera;
    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 currentVelocity;
    private bool isGrounded;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    private float xRotation = 0f;
    #endregion

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (PlayerStateManager.Instance.currentState != PlayerState.Normal)
            return;

        // Example health damage for testing purposes
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayerHealth.Instance.TakeDamage(5);
        }

        HandleCamera();
        HandleInputAndJump();
    }

    private void FixedUpdate()
    {
        if (PlayerStateManager.Instance.currentState != PlayerState.Normal)
            return;

        HandleMovement();
    }

    private void HandleCamera()
    {
        // Get raw mouse input
        float rawMouseX = Input.GetAxisRaw("Mouse X");
        float rawMouseY = Input.GetAxisRaw("Mouse Y");

        // Smooth mouse input for a better experience
        Vector2 targetMouseDelta = new Vector2(rawMouseX, rawMouseY);
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, 0.05f);

        // Apply mouse movement to the camera's rotation
        float mouseX = currentMouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = currentMouseDelta.y * mouseSensitivity * Time.deltaTime;

        // Clamp the vertical rotation to prevent flipping
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);

        // Apply the camera's pitch and the player's yaw (body rotation)
        headTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleInputAndJump()
    {
        // Handle player movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveInput = new Vector3(horizontal, 0f, vertical).normalized;

        // Check if the player is grounded using a sphere cast
        isGrounded = Physics.SphereCast(transform.position, 0.3f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask);

        // Handle jump if player is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void HandleMovement()
    {
        // Calculate the direction of movement
        Vector3 moveDir = transform.TransformDirection(moveInput) * moveSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, moveDir, acceleration * Time.fixedDeltaTime);

        // Apply velocity to the Rigidbody, only modifying x and z to avoid overriding gravity
        Vector3 targetVelocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
        rb.velocity = targetVelocity;

        // Check if the player is moving
        bool isMoving = moveInput.magnitude > 0.1f;

        // Play walking sound if moving
        if (isMoving)
        {
            walkSoundTimer -= Time.deltaTime;

            if (walkSoundTimer <= 0f)
            {
                PlayNextWalkClip();
                walkSoundTimer = walkSoundInterval;
            }
        }
        else
        {
            walkSoundTimer = 0f;
        }
    }

    private void PlayNextWalkClip()
    {
        // Ensure there are clips to play
        if (walkClips.Length == 0) return;

        walkingAudioSource.clip = walkClips[currentWalkClipIndex];
        walkingAudioSource.Play();

        // Loop through the walk clips in a circular manner
        currentWalkClipIndex = (currentWalkClipIndex + 1) % walkClips.Length;
    }
}
