using UnityEngine;
using TMPro; // IMPORTANT

public class AttributesManager : MonoBehaviour
{
    public int health;
    public int attack;

    public TextMeshProUGUI healthText; // Drag UI text here in Inspector

    void Start()
    {
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log(gameObject.name + " health " + health);

        UpdateHealthUI(); // Update UI after damage

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

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + health;
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " died");
        Destroy(gameObject);
    }
}

