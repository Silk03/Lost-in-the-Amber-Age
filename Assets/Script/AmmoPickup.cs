using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int ammoAmount = 5;
    public float bobSpeed = 1f;
    public float bobHeight = 0.2f;
    public float rotationSpeed = 90f;
    
    [Header("Effects")]
    public GameObject pickupEffect;
    public AudioClip pickupSound;
    
    private Vector3 startPosition;
    private float bobTime;
    
    void Start()
    {
        // Store the initial position for bobbing
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Make the pickup bob up and down
        bobTime += Time.deltaTime * bobSpeed;
        transform.position = startPosition + new Vector3(0f, Mathf.Sin(bobTime) * bobHeight, 0f);
        
        // Rotate the pickup
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we collided with the player
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player touched ammo pickup");
            
            // Try to get the player's ammo manager
            AmmoManager playerAmmo = collision.GetComponent<AmmoManager>();
            
            if (playerAmmo != null)
            {
                // Give ammo to the player
                playerAmmo.AddAmmo(ammoAmount);
                
                // Play pickup sound if available
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Show effect if available
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                // Destroy the pickup
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Player has no AmmoManager component!");
            }
        }
    }
}