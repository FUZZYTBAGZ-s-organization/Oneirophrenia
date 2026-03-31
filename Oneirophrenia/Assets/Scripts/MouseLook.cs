using UnityEngine;
using UnityEngine.Rendering; // Not actually needed here unless you plan to use rendering features

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f; // How sensitive the mouse movement is
    public Transform playerBody;           // Reference to the player's body (for horizontal rotation)

    float xRotation = 0f; // Current vertical rotation of the camera (pitch)

    // ------------------------------
    void Start()
    {
        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
    }

    // ------------------------------
    void Update()
    {
        // Get mouse input for this frame
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; // Horizontal movement
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // Vertical movement

        // Adjust the vertical rotation based on mouse Y (up/down)
        xRotation -= mouseY;

        // Clamp the vertical rotation so the camera cannot flip over
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply vertical rotation to the camera (local rotation only)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player horizontally (yaw) around the Y-axis
        playerBody.Rotate(Vector3.up * mouseX);
    }
}