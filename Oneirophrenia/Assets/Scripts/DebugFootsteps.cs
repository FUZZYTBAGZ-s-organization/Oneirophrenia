using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DebugFootsteps : MonoBehaviour
{
    public CharacterController controller;
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;

    public float stepInterval = 0.5f; // Time between footsteps
    private float stepTimer;

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Only count steps if moving and grounded
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval; // Reset timer
            }
        }
        else
        {
            stepTimer = 0f; // Reset timer when stopped
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepSource.PlayOneShot(clip);

        Debug.Log("Footstep played!"); // Confirm footsteps are triggering
    }
}