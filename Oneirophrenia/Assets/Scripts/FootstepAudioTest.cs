using UnityEngine;

public class FootstepAudioTest : MonoBehaviour
{
    public AudioSource audioSource;  // Assign your player's AudioSource here
    public AudioClip testClip;       // Assign one footstep clip here

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))  // Press T to test sound
        {
            audioSource.PlayOneShot(testClip);
            Debug.Log("Played footstep test sound.");
        }
    }
}