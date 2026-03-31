using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float damage = 20f;               // Damage dealt by each shot
    public float fireRate = 0.5f;            // Time between shots (fire rate)
    public float range = 100f;               // Range of the gun's bullets
    public Transform gunEnd;                 // Point where the gun's raycast starts (usually at the barrel)

    private float lastShootTime;             // Time of last shot (used for cooldown)

    void Update()
    {
        if (Time.time >= lastShootTime + fireRate)
        {
            if (Input.GetMouseButton(0)) // Left-click to shoot
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
        lastShootTime = Time.time; // Update the last shoot time

        RaycastHit hit;
        // Raycast to detect if we hit something in range
        if (Physics.Raycast(gunEnd.position, gunEnd.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // If the hit object has a Health component, apply damage
            Health enemyHealth = hit.collider.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // Optional: Add a visual effect or sound for the gunshot
        Debug.DrawRay(gunEnd.position, gunEnd.forward * range, Color.red, 0.1f); // Show a red line where the bullet hit
    }
}