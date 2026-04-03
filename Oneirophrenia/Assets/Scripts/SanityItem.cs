using UnityEngine;

public class SanityItem : MonoBehaviour
{
    public float sanityIncreaseAmount = 10f; // Amount of sanity to increase

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player touched the item
        HorrorPlayerController player = other.GetComponent<HorrorPlayerController>();
        if (player != null)
        {
            // Increase sanity
            player.IncreaseSanity(sanityIncreaseAmount);

            // Destroy the item after collection
            Destroy(gameObject);
        }
    }
}