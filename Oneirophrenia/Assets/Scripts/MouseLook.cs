using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    public Transform playerBody; // rotates left/right

    // IMPORTANT: Now public so other scripts can read it
    public float xRotation = 0f;

    void Start()
    {
        // Lock cursor to center
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical rotation (camera up/down)
        xRotation -= mouseY;

        // Clamp so we don’t flip upside down
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply to camera ONLY
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player body horizontally
        playerBody.Rotate(Vector3.up * mouseX);
    }
}