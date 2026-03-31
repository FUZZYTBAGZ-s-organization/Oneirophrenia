using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float damage = 20f;               // Damage dealt by each shot
    public float fireRate = 0.5f;            // Minimum time between shots
    public float range = 100f;               // Maximum distance the bullet can travel
    public Transform gunEnd;                 // The point from where the raycast (bullet) starts, usually gun barrel
    public Transform GunPosition;            // Optional reference for gun's position/animations
    public GunRecoil gunRecoil;              // Reference to the recoil script

    private float lastShootTime;             // Tracks when the gun was last fired

    // ------------------------------
    void Update()
    {
        // Check if enough time has passed to allow shooting again
        if (Time.time >= lastShootTime + fireRate)
        {
            // Check if left mouse button is pressed
            if (Input.GetMouseButton(0))
            {
                Shoot(); // Fire the gun
            }
        }
    }

    // ------------------------------
    void Shoot()
    {
        // Update last shoot time to enforce cooldown
        lastShootTime = Time.time;

        // --- Apply recoil ---
        if (gunRecoil != null)
            gunRecoil.ShootRecoil(); // Add recoil effect to the gun

        // --- Raycast shooting ---
        RaycastHit hit;
        if (Physics.Raycast(gunEnd.position, gunEnd.forward, out hit, range))
        {
            // Print name of object hit (for debugging)
            Debug.Log("Hit: " + hit.collider.name);

            // Check if hit object has a Health component
            Health enemyHealth = hit.collider.GetComponent<Health>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(damage); // Apply damage to enemy
        }

        // Draw a temporary line in the Scene view to visualize the shot
        Debug.DrawRay(gunEnd.position, gunEnd.forward * range, Color.red, 0.1f);
    }
}