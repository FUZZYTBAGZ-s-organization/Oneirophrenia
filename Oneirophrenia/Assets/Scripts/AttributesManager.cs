using UnityEngine;

public class AttributesManager : MonoBehaviour

{
    public int health;
    public int attack;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log(gameObject.name + "health" + health);

        if (health <= 0)
        {
            Die();
        }
    }

    public void DealDamage(GameObject target)
    {
        var atm = target.GetComponent<AttributesManager>();
        if (atm != null)
        {
            atm.TakeDamage(attack);
        }
    }
        private void Die()
        {
            Debug.Log(gameObject.name + "died");

            Destroy(gameObject);
        }
    
}
    
