using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Add this for UI components

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public bool isAlive = true;
    public int maxHealth = 3;
    public int currentHealth = 3;
    public float invincibilityTime = 1.0f; // Time player is invincible after taking damage
    public float respawnDelay = 2f; // Time before respawn/restart
    
    [Header("UI References")]
    public Image[] healthIcons; // Array of heart icons
    public Sprite fullHeartSprite; // Sprite for full heart
    public Sprite emptyHeartSprite; // Sprite for empty heart
    
    [Header("Player References")]
    public GameObject playerModel; // Visual representation
    public MonoBehaviour[] scriptsToDisableOnDeath; // Movement scripts etc.
    
    [Header("Audio")]
    public AudioClip hitSound;        // Sound when player takes damage
    public AudioClip deathSound;      // Sound when player dies
    public AudioClip healSound;       // Sound when player gets healed
    public AudioClip gameOverSound;   // Sound when game over screen appears
    [Range(0f, 1f)]
    public float hitSoundVolume = 0.75f;

    // Private variables
    private Vector3 startPosition;
    private bool isInvincible = false;
    private AudioSource audioSource;  // Reference to the AudioSource component
    
    void Start()
    {
        startPosition = transform.position;
        currentHealth = maxHealth;
        UpdateHealthUI();
        
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with an enemy
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            TakeDamage(1);
        }
    }
    
    // Alternative for 3D games
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            TakeDamage(1);
        }
    }
    
    // Add this new method
    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || !isAlive) return;
        
        // Apply damage
        currentHealth -= damageAmount;
        
        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, hitSoundVolume);
        }
        
        // Update UI
        UpdateHealthUI();
        
        // Start invincibility frames
        StartCoroutine(InvincibilityFrames());
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Add this new method
    private System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        // Optional: Visual feedback (flashing)
        SpriteRenderer playerRenderer = playerModel?.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            float flashInterval = 0.1f;
            for (float i = 0; i < invincibilityTime; i += flashInterval)
            {
                playerRenderer.enabled = !playerRenderer.enabled;
                yield return new WaitForSeconds(flashInterval);
            }
            playerRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityTime);
        }
        
        isInvincible = false;
    }
    
    // Add this new method
    private void UpdateHealthUI()
    {
        if (healthIcons == null || healthIcons.Length == 0) return;
        
        // Update each heart icon
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                // If we have sprites, change the sprite
                if (fullHeartSprite != null && emptyHeartSprite != null)
                {
                    healthIcons[i].sprite = (i < currentHealth) ? fullHeartSprite : emptyHeartSprite;
                }
                else
                {
                    // Otherwise just change color
                    healthIcons[i].color = (i < currentHealth) ? Color.red : Color.gray;
                }
            }
        }
    }
    
    // Update your Die() method
    public void Die()
    {
        if (!isAlive) return; // Already dead
        
        isAlive = false;
        currentHealth = 0;
        UpdateHealthUI();
        
        Debug.Log("Player died!");
        
        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // IMMEDIATELY play game over sound - don't rely on coroutine
        if (audioSource != null && gameOverSound != null)
        {
            // Play on a different audio source to avoid conflicts
            AudioSource.PlayClipAtPoint(gameOverSound, Camera.main.transform.position, 0.9f);
        }
        
        // Disable player controls/scripts
        foreach (MonoBehaviour script in scriptsToDisableOnDeath)
        {
            if (script != null) script.enabled = false;
        }
        
        // Optional: Hide player model
        if (playerModel != null) playerModel.SetActive(false);
        
        // Restart after delay
        Invoke("Respawn", respawnDelay);
    }

    void Respawn()
    {
        // Option 1: Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // Option 2: Reset player position and state (uncomment if preferred)
        /*
        transform.position = startPosition;
        isAlive = true;
        currentHealth = maxHealth;
        UpdateHealthUI();
        if (playerModel != null) playerModel.SetActive(true);
        foreach (MonoBehaviour script in scriptsToDisableOnDeath)
        {
            if (script != null) script.enabled = true;
        }
        */
    }
    
    // Add this method to your existing PlayerHealth.cs script
    public void RestoreHealth(int amount)
    {
        if (!isAlive) return; // Don't heal if player is dead
        
        // Increase health
        currentHealth += amount;
        
        // Make sure health doesn't exceed maximum
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        // Update UI
        UpdateHealthUI();
        
        // Play heal sound
        if (healSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(healSound);
        }
    }
}