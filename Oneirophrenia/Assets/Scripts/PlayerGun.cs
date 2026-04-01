using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float damage = 20f;          // The damage dealt by each shot
    public float fireRate = 0.5f;       // The minimum time between two shots
    public float range = 100f;          // The maximum range the bullet can travel
    public Transform gunEnd;            // The point from where the bullet starts (gun barrel tip)
    public GunRecoil gunRecoil;         // Optional recoil script (used to add recoil effects)

    [Header("Alert Range")]
    public float alertRange = 50f;      // The maximum distance for AI to hear the shot

    private float lastShootTime;        // Keeps track of the time when the gun was last fired

    void Update()
    {
        // Check if enough time has passed since the last shot and if the left mouse button is pressed
        if (Time.time >= lastShootTime + fireRate && Input.GetMouseButton(0))
        {
            Shoot();  // Call the Shoot method when the player clicks the mouse
        }
    }

    void Shoot()
    {
        lastShootTime = Time.time;  // Update the last shoot time to enforce the cooldown

        // Apply recoil if we have a recoil script
        if (gunRecoil != null)
            gunRecoil.ShootRecoil();  // This will apply the recoil effect to the gun

        // Safety check: Ensure the gunEnd (the point where the bullet originates) is assigned
        if (gunEnd == null)
        {
            Debug.LogWarning("gunEnd is not assigned! Alert skipped.");
            return;  // If gunEnd is not assigned, don't proceed with shooting or alerting
        }

        RaycastHit hit;
        Vector3 shotPosition = gunEnd.position; // Default shot position is gunEnd's position (the barrel tip)

        // Perform a raycast to simulate the bullet's path
        if (Physics.Raycast(gunEnd.position, gunEnd.forward, out hit, range))
        {
            shotPosition = hit.point; // If the ray hits something, update the shot position to the hit point

            // If we hit an object with a Health script (e.g., an enemy), apply damage
            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);  // Deal damage to the object (enemy)
            }
        }

        // Only alert AI if the shot position is valid (not (0,0,0)) and if within the alert range
        if (shotPosition != Vector3.zero)
        {
            // Find all AI enemies in the scene
            HostileAI[] enemies = FindObjectsOfType<HostileAI>();
            foreach (HostileAI enemy in enemies)
            {
                // Check if the AI is within the alert range of the shot
                if (Vector3.Distance(shotPosition, enemy.transform.position) <= alertRange)
                {
                    enemy.AlertToPosition(shotPosition);  // Alert the AI to the shot position
                }
            }
        }

        // Debugging: Draw a line in the Scene view to visualize the shot path
        Debug.DrawRay(gunEnd.position, gunEnd.forward * range, Color.red, 0.1f);
    }
}