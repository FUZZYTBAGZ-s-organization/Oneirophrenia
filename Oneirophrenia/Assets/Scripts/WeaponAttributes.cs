using UnityEngine;

public class WeaponAttributes : MonoBehaviour
{
    public AttributesManager atm;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("enemy"))
        {
            other.GetComponent<AttributesManager>().TakeDamage(atm.attack);
        }
    }
}
