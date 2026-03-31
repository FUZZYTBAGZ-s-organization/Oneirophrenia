using UnityEngine;

public class WeaponAnimation : MonoBehaviour
{
    public Animator weaponAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weaponAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        weaponAnimator.SetBool("Aim", Input.GetMouseButton(1));


    }
}
