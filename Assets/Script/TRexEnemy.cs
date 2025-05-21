using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TRexEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float hitpoints;
    public float maxhitpoints = 15;  // TRex is stronger than standard enemies
    
    [Header("Movement")]
    public float moveSpeed = 3f;      // TRex moves faster
    public float detectionRange = 7f; // TRex has better sight
    public bool faceRight = true;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public bool patrolMode = true;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    
    [Header("Animation")]
    public Animator animator;
    private bool isRunning = false;
    
    [Header("Info Popup Details")]
    public string enemyName = "Tyrannosaurus Rex";
    [TextArea(2, 5)]
    public string enemyDescription = "A massive predator with powerful jaws and tiny arms. The apex predator of its time.";
    public Sprite enemyIcon;
    
    // IMPORTANT: Add these fields to match Enemy.cs
    public GameObject raptorInfoPanel;
    public TMP_Text raptorInfoText;
    
    [Header("Attack")]
    public int biteDamage = 3;        // TRex has stronger bite
    public float biteRange = 2.0f;    // TRex has longer reach
    public float biteCooldown = 1.5f; // Slower attack due to size
    private float lastBiteTime = 0f;
    private bool isPerformingBite = false;
    
    // References
    private Rigidbody2D rb;
    private Transform player;
    
    void Start()
    {
        // Initialize health
        hitpoints = maxhitpoints;
        
        // Get references
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Error checks
        if (rb == null)
            Debug.LogError("Rigidbody2D component missing from TRex!");
            
        if (animator == null)
            Debug.LogWarning("Animator component missing from TRex!");
            
        if (groundCheck == null)
            Debug.LogWarning("Ground Check not assigned to TRex!");
        
        // Set default ground layer if not set
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
            Debug.Log($"Setting default ground layer mask: {groundLayer}");
        }
        
        // Create ground check if missing
        if (groundCheck == null)
        {
            Debug.Log("Creating ground check transform");
            GameObject check = new GameObject("GroundCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0, -0.5f, 0); // Position at feet
            groundCheck = check.transform;
        }
    }
    
    void Update()
    {
        // Handle movement based on mode
        if (patrolMode)
        {
            Patrol();
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
                // TRex spotted the player!
                patrolMode = false;
                PlayRoarSound();  // Optional: Add roar when detecting player
            }
        }
    }
    
    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length <= 0)
            return;
            
        // Move towards current patrol point
        Transform target = patrolPoints[currentPatrolIndex];
        if (target == null) return;
        
        MoveTowards(target.position);
        
        // Check if we reached the patrol point
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget < 0.2f)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
    
    void ChasePlayer()
    {
        if (player == null)
            return;
            
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // If we're already biting, just ensure we stay still
        if (isPerformingBite)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // If close enough to bite, stop and bite
        if (distanceToPlayer <= biteRange)
        {
            // Stop moving when biting
            rb.linearVelocity = Vector2.zero;
            
            // Try to bite
            TryBitePlayer();
        }
        else
        {
            // Move towards player if not in bite range
            MoveTowards(player.position);
        }
        
        // Check if we should go back to patrolling
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            patrolMode = true;
        }
    }
    
    void MoveTowards(Vector2 targetPosition)
    {
        // Determine movement direction
        float xDirection = targetPosition.x - transform.position.x;
        
        // Only move if on ground
        bool isGrounded = groundCheck != null ? Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer) : true;
        
        if (isGrounded)
        {
            // Set velocity
            rb.linearVelocity = new Vector2(Mathf.Sign(xDirection) * moveSpeed, rb.linearVelocity.y);
            
            // Update facing direction
            if (xDirection > 0 && !faceRight || xDirection < 0 && faceRight)
            {
                Flip();
            }
            
            isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        }
        else
        {
            // In air, don't control horizontal movement
            isRunning = false;
        }

        if (!isGrounded)
        {
            Debug.LogWarning("TRex not grounded, can't move!");
        }
    }
    
    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsRunning", isRunning);
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
    
    // MODIFIED: Match Enemy.cs TakeHit exactly
    public void TakeHit(float damage)
    {
        Debug.Log($"TRex taking damage: {damage}");
        
        // Stabilize to prevent spinning
        rb.linearVelocity = Vector2.zero; 
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
        
        hitpoints -= damage;
        Debug.Log($"TRex health: {hitpoints}/{maxhitpoints}");
        
        if (hitpoints <= 0)
        {
            Debug.Log($"Enemy {enemyName} died, showing info popup");
            
            // Show popup - DIRECTLY copied from Enemy.cs
            if (raptorInfoPanel != null && raptorInfoText != null)
            {
                raptorInfoPanel.SetActive(true);
                raptorInfoText.text = enemyDescription;
                StartCoroutine(CloseInfoPanel(3.0f));
            }
            
            Destroy(gameObject, 0.1f);
        }
    }
    
    // Match Enemy.cs exactly
    public void TakeDamage(float damage)
    {
        TakeHit(damage);
    }
    
    // Added to match Enemy.cs
    IEnumerator CloseInfoPanel(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (raptorInfoPanel != null)
        {
            raptorInfoPanel.SetActive(false);
        }
    }
    
    // Simplified to match Enemy.cs pattern
    public void TryBitePlayer()
    {
        // Skip if player is null or on cooldown
        if (player == null || Time.time < lastBiteTime + biteCooldown || isPerformingBite)
            return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if player is in range and we're facing them
        bool isInRange = distanceToPlayer <= biteRange;
        bool isFacingPlayer = (player.position.x > transform.position.x && faceRight) || 
                             (player.position.x < transform.position.x && !faceRight);
        
        if (isInRange && isFacingPlayer)
        {
            isPerformingBite = true;
            PerformBite();
        }
    }

    private void PerformBite()
    {
        // Update last bite time
        lastBiteTime = Time.time;
        
        // Play bite animation
        if (animator != null)
        {
            animator.SetTrigger("Bite");
        }
        
        // Apply damage immediately
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(biteDamage);
                Debug.Log($"TRex bit player for {biteDamage} damage");
            }
        }
        
        // Reset bite state after a delay
        StartCoroutine(ResetBiteState(0.7f));
    }

    private IEnumerator ResetBiteState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPerformingBite = false;
    }
    
    void PlayRoarSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
    
    // MATCH Enemy.cs collision detection EXACTLY
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(biteDamage);
            }
            
            rb.linearVelocity = Vector2.zero;
        }
        else if (collision.gameObject.CompareTag("Bullet") || 
                collision.gameObject.name.ToLower().Contains("bullet"))
        {
            Debug.Log("Bullet hit detected!");
            
            // Apply default damage
            TakeHit(1f);
            
            // Try to get specific damage from projectile
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            if (projectile != null && projectile.damage > 0)
            {
                // Apply the specific projectile damage instead
                TakeHit(projectile.damage);
            }
            
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.rotation = Quaternion.identity;
            
            // Destroy bullet
            Destroy(collision.gameObject);
        }
    }
    
    // Match Enemy.cs trigger detection EXACTLY
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"TRex trigger with: {other.gameObject.name}, tag: {other.gameObject.tag}");
        
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

    // Add this method to the end of your TRexEnemy.cs file
    private void OnDrawGizmosSelected()
    {
        // Draw detection range (yellow circle)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw bite range (red circle)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, biteRange);
        
        // Draw patrol path if points are assigned
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Vector3 pos = patrolPoints[i].position;
                    // Draw patrol point
                    Gizmos.DrawSphere(pos, 0.2f);
                    
                    // Draw line to next patrol point
                    if (patrolPoints.Length > 1 && i < patrolPoints.Length - 1 && patrolPoints[i+1] != null)
                    {
                        Gizmos.DrawLine(pos, patrolPoints[i+1].position);
                    }
                    // Connect last point to first
                    else if (patrolPoints.Length > 1 && i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(pos, patrolPoints[0].position);
                    }
                }
            }
        }
        
        // Draw ground check radius if available
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}