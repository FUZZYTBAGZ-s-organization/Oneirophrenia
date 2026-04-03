using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class HorrorPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float gravity = -9.81f;
    public Transform cameraTransform;

    public CharacterController controller;
    private Vector3 velocity;
    private bool isRunning;

    [Header("Footstep Settings")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f; // time between footsteps
    private float stepTimer;

    [Header("Sanity / Breathing Settings")]
    public float sanity = 100f;       // 0 = fully panicked
    public float sanityDecreaseRate = 5f;
    public AudioSource breathingSource;
    public AudioClip breathingNormal;
    public AudioClip breathingPanicked;
    public Image sanityOverlay;       // optional UI overlay for visual effect

    public void IncreaseSanity(float amount)
    {
        sanity += amount;
        sanity = Mathf.Clamp(sanity, 0f, 100f); // Ensure sanity stays between 0 and 100
        Debug.Log("Sanity increased by " + amount + ", current sanity: " + sanity);
    }

    // Call this method when an item is collected
    public void DecreaseSanity(float amount)
    {
        sanity -= amount;
        sanity = Mathf.Clamp(sanity, 0f, 100f); // Keep sanity between 0 and 100
        Debug.Log("Sanity decreased by " + amount + ", current sanity: " + sanity);
    }
    private void Start()
    {
        controller = GetComponent<CharacterController>();

        // Default AudioSources
        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (breathingSource != null && breathingNormal != null)
        {
            breathingSource.clip = breathingNormal;
            breathingSource.loop = true;
            breathingSource.Play();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleFootsteps();
        HandleSanity();
    }

    private void HandleMovement()
    {
        // Input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move.Normalize();

        // Move player
        controller.Move(move * (isRunning ? runSpeed : walkSpeed) * Time.deltaTime);

        // Gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f; // small downward force

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleFootsteps()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (controller.isGrounded && isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval / (isRunning ? 1.5f : 1f))
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length == 0 || footstepSource == null) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepSource.pitch = Random.Range(0.9f, 1.1f);
        footstepSource.spatialBlend = 1f; // 3D sound
        footstepSource.PlayOneShot(clip);
        Debug.Log("Playing footstep: " + clip.name);
    }

    private void HandleSanity()
    {
        // Example: sanity decreases over time if running
        if (isRunning)
            sanity -= sanityDecreaseRate * Time.deltaTime;

        sanity = Mathf.Clamp(sanity, 0f, 100f);

        // Breathing sound swap
        if (breathingSource != null)
        {
            if (sanity < 30f && breathingSource.clip != breathingPanicked)
            {
                breathingSource.clip = breathingPanicked;
                breathingSource.Play();
            }
            else if (sanity >= 30f && breathingSource.clip != breathingNormal)
            {
                breathingSource.clip = breathingNormal;
                breathingSource.Play();
            }
        }

        // Optional: sanity overlay effect
        if (sanityOverlay != null)
        {
            sanityOverlay.color = new Color(1f, 0f, 0f, 1f - sanity / 100f);
        }
    }
}