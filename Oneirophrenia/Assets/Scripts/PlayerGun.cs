using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    public float damage = 20f;
    public float fireRate = 0.5f;
    public float range = 100f;
    public Transform gunEnd;
    public GunRecoil gunRecoil;

    public float alertRange = 50f;
    public SanityManager sanityManager;

    private float lastShootTime;

    void Update()
    {
        if (Time.time >= lastShootTime + fireRate && Input.GetMouseButtonDown(0))
            Shoot();
    }

    void Shoot()
    {
        lastShootTime = Time.time;

        if (gunRecoil != null)
            gunRecoil.ShootRecoil();

        if (gunEnd == null) return;

        RaycastHit hit;
        Vector3 shotPosition = gunEnd.position;

        if (Physics.Raycast(gunEnd.position, gunEnd.forward, out hit, range))
        {
            shotPosition = hit.point;
            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
                targetHealth.TakeDamage(damage);
        }

        // Increase alert range based on sanity
        float sanityFactor = 1f - sanityManager.currentSanity / sanityManager.maxSanity;
        float effectiveRange = alertRange * (1f + sanityFactor);

        HostileAI[] enemies = FindObjectsOfType<HostileAI>();
        foreach (HostileAI enemy in enemies)
        {
            if (Vector3.Distance(shotPosition, enemy.transform.position) <= effectiveRange)
                enemy.HearSound(shotPosition, 1f + sanityFactor);
        }

        Debug.DrawRay(gunEnd.position, gunEnd.forward * range, Color.red, 0.1f);
    }
}