using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int healthRestored = 1;
    public float bobSpeed = 1f;
    public float bobHeight = 0.5f;
    public float rotationSpeed = 90f;
    
    [Header("Effects")]
    public GameObject pickupEffect;
    public AudioClip pickupSound;
    
    private Vector3 startPosition;
    private float bobTime;
    
    void Start()
    {
        // Store the initial position for the bobbing effect
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Bob up and down
        bobTime += Time.deltaTime * bobSpeed;
        transform.position = startPosition + new Vector3(0f, Mathf.Sin(bobTime) * bobHeight, 0f);
        
        // Rotate around y-axis
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision is with the player
        if (collision.CompareTag("Player"))
        {
            // Try to get the player health component
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            
            if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
            {
                // Heal the player
                playerHealth.RestoreHealth(healthRestored);
                
                // Play sound if available
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Spawn effect if available
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                // Destroy the apple
                Destroy(gameObject);
            }
        }
    }
}