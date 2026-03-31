using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    public Vector3 recoilRotation = new Vector3(2f, 0.5f, 0f);
    // Amount of rotation to apply when shooting: 
    // x = vertical kick (up), y = horizontal randomness (left/right), z = roll (unused here)

    public float recoilSpeed = 10f;   // How quickly the gun moves toward the target rotation
    public float returnSpeed = 5f;    // How quickly the gun returns to neutral rotation

    private Vector3 currentRotation;  // Current rotation of the gun (used for smooth lerping)
    private Vector3 targetRotation;   // Desired rotation after applying recoil

    // ------------------------------
    void Update()
    {
        // Gradually move targetRotation back to zero over time
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);

        // Smoothly move currentRotation toward targetRotation
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, recoilSpeed * Time.deltaTime);

        // Apply the rotation to the gun
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    // ------------------------------
    public void ShootRecoil()
    {
        // When the gun is fired, add recoil to the target rotation
        targetRotation += new Vector3(
            -recoilRotation.x,                         // Kick upward (negative X because up is negative in Unity's local rotation)
            Random.Range(-recoilRotation.y, recoilRotation.y), // Add slight horizontal randomness
            0                                         // No roll applied
        );
    }
}