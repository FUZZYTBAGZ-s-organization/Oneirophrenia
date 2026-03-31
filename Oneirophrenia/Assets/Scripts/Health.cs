using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;  // Maximum health
    public float currentHealth;     // Current health

    void Start()
    {
        currentHealth = maxHealth;  // Set current health to max at the start
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
        // You can implement death animations or other behaviors here
        Debug.Log($"{gameObject.name} died!");
        Destroy(gameObject);  // For now, we just destroy the object when it dies
    }
}