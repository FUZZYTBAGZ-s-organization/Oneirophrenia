using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DamageOnHit : MonoBehaviour
{
    public AttributesManager attacker;
    public AttributesManager playerAtm; // Who deals the damage

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object hit has an AttributesManager (the enemy)
        AttributesManager enemyAtm = other.GetComponent<AttributesManager>();
        AttributesManager targetAtm = other.GetComponent<AttributesManager>();

        // Only damage the player
        if (other.CompareTag("Player"))
        {
            AttributesManager playerAtm = other.GetComponent<AttributesManager>();

            if (playerAtm != null)
            {
                playerAtm.TakeDamage(attacker.attack);
                Debug.Log("Player took damage!");
            }
        }

        if (targetAtm != null && targetAtm != attacker)
        {
            attacker.DealDamage(other.gameObject);
            Debug.Log("Hit " + other.name);
        }
        
    }
}
