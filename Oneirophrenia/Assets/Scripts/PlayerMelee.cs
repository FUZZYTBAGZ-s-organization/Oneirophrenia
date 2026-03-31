using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public float attackRange = 2f;      // Player attack range
    public float attackCooldown = 1f;   // Cooldown between attacks
    private float lastAttackTime;       // Last time the player attacked

    void Update()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // Check for player input (e.g., press a key to attack)
            if (Input.GetKeyDown(KeyCode.F))  // Player presses "F" to attack
            {
                MeleeAttack();
            }
        }
    }

    void MeleeAttack()
    {
        // Get all enemies within attack range
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var enemy in hitEnemies)
        {
            // If the enemy has a Health component, damage it
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(20f);  // Adjust damage as needed
                Debug.Log("Player attacked the enemy!");
            }
        }

        // Record the time of the last attack
        lastAttackTime = Time.time;
    }
}