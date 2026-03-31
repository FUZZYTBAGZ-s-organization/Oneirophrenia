using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public float attackRange = 2f;      // How far the player's melee attack reaches
    public float attackCooldown = 1f;   // Minimum time between attacks
    private float lastAttackTime;       // Tracks the time of the last attack for cooldown

    // ------------------------------
    void Update()
    {
        // Check if cooldown has passed
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // Check for player input: "F" key triggers melee attack
            if (Input.GetKeyDown(KeyCode.F))
            {
                MeleeAttack();
            }
        }
    }

    // ------------------------------
    void MeleeAttack()
    {
        // Detect all colliders within a sphere of radius 'attackRange' centered on the player
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange);

        // Loop through each collider found
        foreach (var enemy in hitEnemies)
        {
            // Check if the object has a Health component
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Apply damage to the enemy
                enemyHealth.TakeDamage(20f);  // You can adjust damage value here
                Debug.Log("Player attacked the enemy!");
            }
        }

        // Update the last attack time to enforce cooldown
        lastAttackTime = Time.time;
    }
}