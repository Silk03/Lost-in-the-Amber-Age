using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player
    public float smoothSpeed = 0.125f; // Adjust for smooth movement
    public float xOffset = 5f; // Adjust how much the camera is ahead

    private float fixedY; // Store the initial Y position

    void Start()
    {
        // Save the initial Y position of the camera
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Move the camera slightly ahead of the player
            Vector3 desiredPosition = new Vector3(target.position.x + xOffset, fixedY, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        }
    }
}


