using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity;
    public float sanityDecayRate = 5f;
    public float recoveryRate = 2f;
    public float enemyProximityRange = 10f;

    [Header("Screen Effects")]
    public Volume postProcessVolume;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    [Header("Audio Effects")]
    public AudioSource heartbeatSource;
    public AudioSource breathingSource;
    public float maxAudioVolume = 1f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;

    [Header("References")]
    public Transform player;
    public HorrorPlayerController playerController;

    void Start()
    {
        currentSanity = maxSanity;

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet<Vignette>(out vignette);
            postProcessVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
        }
    }

    void Update()
    {
        HandleSanityDecay();
        UpdateScreenEffects();
        UpdateAudio();
    }

    // -------------------------
    void HandleSanityDecay()
    {
        float decay = 0f;
        HostileAI[] enemies = FindObjectsOfType<HostileAI>();
        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(player.position, enemy.transform.position);
            if (dist <= enemyProximityRange)
                decay += sanityDecayRate * (1f - dist / enemyProximityRange);
        }

        if (decay > 0) currentSanity -= decay * Time.deltaTime;
        else currentSanity += recoveryRate * Time.deltaTime;

        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
    }

    // -------------------------
    void UpdateScreenEffects()
    {
        if (vignette != null) vignette.intensity.value = Mathf.Lerp(0.2f, 0.6f, 1f - currentSanity / maxSanity);
        if (chromaticAberration != null) chromaticAberration.intensity.value = Mathf.Lerp(0f, 0.5f, 1f - currentSanity / maxSanity);
    }

    // -------------------------
    void UpdateAudio()
    {
        if (heartbeatSource == null || breathingSource == null || playerController == null) return;

        // Base volume from sanity
        float sanityFactor = 1f - currentSanity / maxSanity;

        // Movement factor from player speed
        float moveFactor = playerController.controller.velocity.magnitude / playerController.runSpeed;

        float intensity = Mathf.Clamp01(sanityFactor + moveFactor); // combine both
        float pitch = Mathf.Lerp(minPitch, maxPitch, intensity);

        heartbeatSource.volume = intensity * maxAudioVolume;
        heartbeatSource.pitch = pitch;

        breathingSource.volume = intensity * maxAudioVolume;
        breathingSource.pitch = pitch * 0.9f; // slightly slower than heartbeat
    }

    // -------------------------
    public void DecreaseSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, maxSanity);
    }

    public void IncreaseSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0f, maxSanity);
    }
}