using UnityEngine;
using System.Collections;

public class HorrorEffectsSystem : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public AudioSource whisperSource;
    public AudioSource extraStepSource;

    [Header("Sanity")]
    [Range(0, 100)] public float sanity = 100f;
    public float drainRate = 1f;

    [Header("Whispers")]
    public AudioClip[] whisperClips;
    public float whisperCooldown = 10f;

    [Header("Hallucinations")]
    public GameObject hallucinationPrefab;
    public float hallucinationDistance = 5f;

    [Header("Fake Footsteps")]
    public AudioClip[] stepClips;

    float whisperTimer;

    void Update()
    {
        HandleSanity();
        HandleWhispers();
        HandleHallucinations();
        HandleFakeFootsteps();
    }

    // -------------------------
    // 🧠 SANITY SYSTEM
    void HandleSanity()
    {
        sanity -= drainRate * Time.deltaTime;
        sanity = Mathf.Clamp(sanity, 0, 100);
    }

    // -------------------------
    // 👻 WHISPERS (BEHIND PLAYER)
    void HandleWhispers()
    {
        whisperTimer -= Time.deltaTime;

        float insanity = 1f - (sanity / 100f);

        if (whisperTimer <= 0f && insanity > 0.3f)
        {
            whisperTimer = Random.Range(5f, whisperCooldown);

            // Pick random whisper
            AudioClip clip = whisperClips[Random.Range(0, whisperClips.Length)];

            // Position it BEHIND player
            Vector3 behind = playerCamera.position - playerCamera.forward * 2f;

            whisperSource.transform.position = behind;
            whisperSource.PlayOneShot(clip);

            // NOTE:
            // This creates paranoia — player hears something but sees nothing
        }
    }

    // -------------------------
    // 👁️ HALLUCINATIONS
    void HandleHallucinations()
    {
        float insanity = 1f - (sanity / 100f);

        if (insanity > 0.5f && Random.value < 0.002f)
        {
            Vector3 spawnPos = playerCamera.position + playerCamera.forward * hallucinationDistance;

            Instantiate(hallucinationPrefab, spawnPos, Quaternion.identity);

            // NOTE:
            // Keep prefab VERY simple (shadow, silhouette, etc.)
            // Destroy itself after a few seconds
        }
    }

    // -------------------------
    // 🦶 FAKE FOOTSTEPS
    void HandleFakeFootsteps()
    {
        float insanity = 1f - (sanity / 100f);

        if (insanity > 0.4f && Random.value < 0.003f)
        {
            Vector3 side = playerCamera.right * Random.Range(-3f, 3f);

            extraStepSource.transform.position = playerCamera.position + side;

            AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
            extraStepSource.PlayOneShot(clip);

            // NOTE:
            // This makes player think something is nearby
        }
    }
}