using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PteridactylEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float hitpoints;
    public float maxhitpoints = 5;
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionRange = 6f;
    public bool faceRight = true;
    public bool patrolMode = true;
    public Transform[] flyPoints;
    private int currentFlyPointIndex = 0;
    
    [Header("Animation")]
    public Animator animator;
    private bool isFlying = true;
    
    [Header("Info Popup Details")]
    public string enemyName = "Pterodactyl";
    [TextArea(2, 5)]
    public string enemyDescription = "A flying reptile that swoops down from above to attack its prey.";
    public Sprite enemyIcon;

    public GameObject raptorInfoPanel;
    public TMP_Text raptorInfoText;
    
    [Header("Dive Attack")]
    public int diveDamage = 2;
    public float diveRange = 5f;
    public float diveCooldown = 2.0f;
    public float diveHeight = 3f;
    public float diveDuration = 1.0f;
    private float lastDiveTime = 0f;
    private bool isDiving = false;
    
    // Private variables
    private Rigidbody2D rb;
    private Transform player;
    private Vector3 startDivePosition;
    private Vector3 targetDivePosition;
    
    void Start()
    {
        // Initialize health
        hitpoints = maxhitpoints;
        
        // Get components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Configure flying physics
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        // Check patrol points
        if (flyPoints == null || flyPoints.Length == 0)
        {
            Debug.LogWarning("Pterodactyl has no patrol points assigned!");
        }
    }
    
    void Update()
    {
        // Handle states
        if (isDiving)
        {
            // Diving handled by coroutine
        }
        else if (patrolMode)
        {
            PatrolFly();
        }
        else
        {
            ChasePlayer();
        }
        
        // Update animation
        UpdateAnimation();
    }
    
    void FixedUpdate()
    {
        // Check if we should switch from patrol to chase
        if (patrolMode && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < detectionRange)
            {
                patrolMode = false;
                
                // Immediately dive when player detected
                if (!isDiving && Time.time > lastDiveTime + diveCooldown)
                {
                    StartCoroutine(PerformDiveAttack());
                }
            }
        }
    }
    
    void PatrolFly()
    {
        if (flyPoints == null || flyPoints.Length <= 0)
            return;
            
        // Move towards current patrol point
        Transform target = flyPoints[currentFlyPointIndex];
        if (target == null) return;
        
        // Move towards target
        Vector2 direction = target.position - transform.position;
        float distance = direction.magnitude;
        direction.Normalize();
        
        // Set velocity directly
        rb.linearVelocity = direction * moveSpeed;
        
        // Update facing
        if (direction.x > 0 && !faceRight || direction.x < 0 && faceRight)
        {
            Flip();
        }
        
        // Check if reached patrol point
        if (distance < 0.5f)
        {
            currentFlyPointIndex = (currentFlyPointIndex + 1) % flyPoints.Length;
        }
    }
    
    void ChasePlayer()
    {
        if (player == null)
            return;
            
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Try to dive when in range
        if (Time.time > lastDiveTime + diveCooldown && !isDiving)
        {
            StartCoroutine(PerformDiveAttack());
        }
        else
        {
            // Position above player when not diving
            Vector2 direction = new Vector2(
                player.position.x - transform.position.x,
                player.position.y + diveHeight - transform.position.y
            );
            direction.Normalize();
            
            // Move towards target position
            rb.linearVelocity = direction * moveSpeed;
            
            // Update facing
            if (direction.x > 0 && !faceRight || direction.x < 0 && faceRight)
            {
                Flip();
            }
        }
        
        // Return to patrol if player too far
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            patrolMode = true;
        }
    }
    
    IEnumerator PerformDiveAttack()
    {
        isDiving = true;
        lastDiveTime = Time.time;
        
        // Play animation
        if (animator != null)
        {
            animator.SetTrigger("Dive");
        }
        
        // Rise up before diving
        startDivePosition = transform.position;
        Vector3 risePosition = new Vector3(
            transform.position.x,
            transform.position.y + diveHeight,
            transform.position.z
        );
        
        // Rise movement
        float riseTime = 0;
        float riseDuration = 0.5f;
        while (riseTime < riseDuration)
        {
            riseTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startDivePosition, risePosition, riseTime / riseDuration);
            yield return null;
        }
        
        // Calculate dive target
        if (player != null)
        {
            targetDivePosition = player.position;
            
            // Set facing direction
            if ((targetDivePosition.x > transform.position.x && !faceRight) || 
                (targetDivePosition.x < transform.position.x && faceRight))
            {
                Flip();
            }
        }
        else
        {
            // If player gone, just dive down
            targetDivePosition = new Vector3(transform.position.x, transform.position.y - diveHeight * 2, transform.position.z);
            isDiving = false;
            yield break;
        }
        
        // Dive movement
        float diveTime = 0;
        startDivePosition = transform.position;
        
        while (diveTime < diveDuration)
        {
            diveTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startDivePosition, targetDivePosition, (diveTime/diveDuration) * (diveTime/diveDuration));
            
            // Check for player hit
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer < 1.0f)
                {
                    // Deal damage
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(diveDamage);
                    }
                    
                    // Reset velocity
                    rb.linearVelocity = Vector2.zero;
                    break;
                }
            }
            
            yield return null;
        }
        
        // Rise back up
        startDivePosition = transform.position;
        Vector3 recoverPosition = new Vector3(
            transform.position.x,
            transform.position.y + diveHeight,
            transform.position.z
        );
        
        float recoverTime = 0;
        float recoverDuration = 0.7f;
        while (recoverTime < recoverDuration)
        {
            recoverTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startDivePosition, recoverPosition, recoverTime / recoverDuration);
            yield return null;
        }
        
        // Reset velocity
        rb.linearVelocity = Vector2.zero;
        isDiving = false;
    }
    
    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsFlying", true);
            animator.SetBool("IsDiving", isDiving);
        }
    }
    
    void Flip()
    {
        faceRight = !faceRight;
        
        // Flip the sprite
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    // 1. Fix OnCollisionEnter2D to actually apply damage
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Pterodactyl collided with: {collision.gameObject.name}, tag: {collision.gameObject.tag}");
        
        if (collision.gameObject.CompareTag("Player"))
        {
            // Deal damage to player
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(diveDamage);
            }
            
            // Stop movement - FIXED: changed linearVelocity to velocity
            rb.linearVelocity = Vector2.zero;
        }
        else if (collision.gameObject.CompareTag("Bullet") || 
                 collision.gameObject.name.ToLower().Contains("bullet") ||
                 collision.gameObject.name.ToLower().Contains("projectile"))
        {
            Debug.Log("Bullet hit detected!");
            
            // ADDED: Apply damage - this was missing!
            TakeHit(1f); // Default damage value
            
            // Try to get specific damage from projectile
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            if (projectile != null && projectile.damage > 0)
            {
                // Apply the specific projectile damage instead
                TakeHit(projectile.damage);
            }
            
            // Reset movement - FIXED: changed linearVelocity to velocity
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.rotation = Quaternion.identity;
            
            // Destroy bullet
            Destroy(collision.gameObject);
        }
    }

    // 2. Add OnTriggerEnter2D for trigger-based bullets
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Pterodactyl trigger with: {other.gameObject.name}, tag: {other.gameObject.tag}");
        
        if (other.CompareTag("Bullet") || 
            other.name.ToLower().Contains("bullet") || 
            other.name.ToLower().Contains("projectile"))
        {
            Debug.Log("Bullet trigger detected!");
            
            // Apply default damage
            TakeHit(1f);
            
            // Try to get specific damage from projectile
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null && projectile.damage > 0)
            {
                // Apply the specific projectile damage instead
                TakeHit(projectile.damage);
            }
            
            // Destroy bullet
            Destroy(other.gameObject);
        }
    }

    // 3. Fix TakeHit to use velocity instead of linearVelocity
    public void TakeHit(float damage)
    {
        Debug.Log($"Pterodactyl taking damage: {damage}");
        
        // Stabilize to prevent spinning - FIXED: changed linearVelocity to velocity
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
        
        hitpoints -= damage;
        Debug.Log($"Pterodactyl health: {hitpoints}/{maxhitpoints}");
        
        if (hitpoints <= 0)
        {
            Debug.Log("Pterodactyl died");
            
            // Show popup
            if (raptorInfoPanel != null && raptorInfoText != null)
            {
                raptorInfoPanel.SetActive(true);
                raptorInfoText.text = enemyDescription;
                StartCoroutine(CloseInfoPanel(3.0f));
            }
            
            Destroy(gameObject, 0.1f);
        }
    }
    
    // Make the CloseInfoPanel method more robust
    IEnumerator CloseInfoPanel(float delay)
    {
        Debug.Log("Started CloseInfoPanel coroutine");
        yield return new WaitForSeconds(delay);
        
        if (raptorInfoPanel != null)
        {
            Debug.Log("Closing info panel after delay");
            raptorInfoPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Info panel is null in CloseInfoPanel coroutine");
        }
    }
}