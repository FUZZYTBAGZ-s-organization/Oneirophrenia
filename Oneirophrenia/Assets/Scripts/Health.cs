using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;  // Maximum health
    public float currentHealth;     // Current health

    private HostileAI enemyAI;      // Reference to AI script (to disable it on death)

    void Start()
    {
        currentHealth = maxHealth;  // Set current health to max at the start
        enemyAI = GetComponent<HostileAI>(); // Get reference to HostileAI script
    }

    // Method to take damage
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;    // Reduce health by the damage amount
        if (currentHealth <= 0)
        {
            Die();  // If health is zero or less, die
        }
    }

    // Method for death logic
    void Die()
    {
        // Disable AI behavior to prevent actions after death
        if (enemyAI != null)
        {
            enemyAI.enabled = false; // Disable the AI script
        }

        // You can implement death animations or other behaviors here
        Debug.Log($"{gameObject.name} died!");

        // Optionally, destroy the AI after a short delay
        Destroy(gameObject, 1f); // Destroy after 1 second to give time for any animations
    }
}