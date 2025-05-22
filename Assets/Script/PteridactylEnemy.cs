using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PteridactylEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float hitpoints;
    public float maxhitpoints = 4;
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionRange = 7f;
    public bool faceRight = true;
    public float flyHeight = 3f; // Height to maintain while flying
    public float diveCooldown = 3f; // Time between dive attacks
    private float lastDiveTime = 0f;
    public bool patrolMode = true;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    
    [Header("Animation")]
    public Animator animator;
    private bool isFlying = true;
    private bool isDiving = false;
    
    [Header("Info Popup Details")]
    public string enemyName = "Pterodactyl";
    [TextArea(2, 5)]
    public string enemyDescription = "A flying reptile that dives at its prey from above.";
    public Sprite enemyIcon;

    public GameObject infoPanel;
    public TMP_Text infoText;
    
    [Header("Attack")]
    public int diveDamage = 1;        // Damage per dive attack
    public float diveRange = 5f;      // How close player must be to dive
    public float diveSpeed = 5f;      // Speed of the dive attack
    private bool isPerformingDive = false;
    
    // References
    private Rigidbody2D rb;
    private Transform player;
    
    void Start()
    {
        hitpoints = maxhitpoints;
        
        // Get references
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Error checks
        if (rb == null)
            Debug.LogError("Rigidbody2D component missing from PteridactylEnemy!");
            
        if (animator == null)
            Debug.LogWarning("Animator component missing from PteridactylEnemy!");
        
        // Verify animator parameters
        if (animator != null)
        {
            // Check if "Dive" parameter exists
            bool hasDiveParam = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                Debug.Log("Animator has parameter: " + param.name + " (Type: " + param.type + ")");
                if (param.name == "Dive" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasDiveParam = true;
                }
            }
            
            if (!hasDiveParam)
            {
                Debug.LogWarning("Animator is missing 'Dive' trigger parameter!");
            }
        }
    }
    
    void Update()
    {
        // Handle movement based on mode
        if (patrolMode)
        {
            PatrolFly();
        }
        else
        {
            ChasePlayer();
        }
        
        // Update animation
        UpdateAnimation();
        
        // Force animation update if stuck
        if (isPerformingDive && animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // If we're still not in the dive animation after trying to dive
            if (!stateInfo.IsName("Dive") && Time.time < lastDiveTime + 0.2f)
            {
                // Force it to play
                animator.Play("Dive", 0, 0);
                Debug.Log("Forcing dive animation in Update - animator was stuck!");
            }
        }
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
            }
        }
    }
    
    void PatrolFly()
    {
        if (patrolPoints == null || patrolPoints.Length <= 0)
            return;
            
        // Move towards current patrol point
        Transform target = patrolPoints[currentPatrolIndex];
        
        // Calculate direction to target
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        
        // Apply movement
        rb.linearVelocity = direction * moveSpeed;
        
        // Update facing direction
        if (direction.x > 0 && !faceRight || direction.x < 0 && faceRight)
        {
            Flip();
        }
        
        // Check if we reached the patrol point
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget < 0.5f)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        
        isFlying = true;
    }
    
    void ChasePlayer()
    {
        if (player == null)
            return;
            
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // If we're already diving, don't change course
        if (isPerformingDive)
            return;
        
        // If close enough to dive and cooldown has passed, perform dive attack
        if (distanceToPlayer <= diveRange && Time.time > lastDiveTime + diveCooldown)
        {
            StartCoroutine(PerformDiveAttack());
        }
        else
        {
            // Calculate target position above player
            Vector2 targetPosition = new Vector2(player.position.x, player.position.y + flyHeight);
            
            // Calculate direction to target
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            
            // Apply movement
            rb.linearVelocity = direction * moveSpeed;
            
            // Update facing direction
            if (direction.x > 0 && !faceRight || direction.x < 0 && faceRight)
            {
                Flip();
            }
        }
        
        // Check if we should go back to patrolling
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            patrolMode = true;
        }
        
        isFlying = true;
    }
    
    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsFlying", isFlying);
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
    
    public void TakeHit(float damage)
    {
        hitpoints -= damage;
        
        // Visual feedback
        FlashDamage();
        
        if (hitpoints <= 0)
        {
            Debug.Log($"Enemy {enemyName} died, attempting to show popup");
            
            if (InfoPopup.Instance != null)
            {
                InfoPopup.Instance.ShowEnemyInfo(enemyName, enemyDescription, enemyIcon);
            }
            else if (infoPanel != null && infoText != null)
            {
                infoPanel.SetActive(true);
                infoText.text = enemyDescription;
                StartCoroutine(CloseInfoPanel(3.0f));
            }
            else
            {
                Debug.LogError("No way to display enemy info found!");
            }
            
            // Stop all movement
            rb.linearVelocity = Vector2.zero;
            
            // Disable collider
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;
                
            // Play death animation if available
            if (animator != null)
                animator.SetTrigger("Die");
            
            Destroy(gameObject, 0.5f);
        }
    }
    
    IEnumerator CloseInfoPanel(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
    
    IEnumerator PerformDiveAttack()
    {
        isPerformingDive = true;
        isDiving = true;
        lastDiveTime = Time.time;
        
        // Stop current movement
        rb.linearVelocity = Vector2.zero;
        
        // Trigger dive animation
        if (animator != null)
        {
            animator.ResetTrigger("Dive");
            animator.SetTrigger("Dive");
        }
        
        // Visual feedback
        FlashAttack();
        
        // Wait a moment before diving
        yield return new WaitForSeconds(0.2f);
        
        // Calculate dive direction
        Vector2 diveDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        
        // Apply dive movement
        rb.linearVelocity = diveDirection * diveSpeed;
        
        // Update facing direction
        if (diveDirection.x > 0 && !faceRight || diveDirection.x < 0 && faceRight)
        {
            Flip();
        }
        
        // Wait during dive
        yield return new WaitForSeconds(0.7f);
        
        // End dive attack
        isDiving = false;
        rb.linearVelocity = Vector2.zero;
        
        // Short pause before resuming normal flight
        yield return new WaitForSeconds(0.3f);
        
        isPerformingDive = false;
    }
    
    void FlashAttack()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Store original color
            Color originalColor = sr.color;
            
            // Flash to attack color
            sr.color = Color.red;
            
            // Return to original color after delay
            StartCoroutine(ResetColorAfterDelay(sr, originalColor, 0.15f));
        }
    }
    
    void FlashDamage()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Store original color
            Color originalColor = sr.color;
            
            // Flash white when damaged
            sr.color = Color.white;
            
            // Return to original color after delay
            StartCoroutine(ResetColorAfterDelay(sr, originalColor, 0.15f));
        }
    }
    
    IEnumerator ResetColorAfterDelay(SpriteRenderer sr, Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sr != null) // Check if object still exists
        {
            sr.color = originalColor;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle player collision
        if (collision.gameObject.CompareTag("Player"))
        {
            // Only damage player if diving
            if (isDiving)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(diveDamage);
                }
                
                // End dive early
                isDiving = false;
                isPerformingDive = false;
                rb.linearVelocity = Vector2.zero; // FIXED FROM linearVelocity
            }
            else
            {
                // Regular collision
                Debug.Log("Player hit pterodactyl! Triggering Game Over.");
                var gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                    gameManager.GameOver();
            }
        }
        // Check for Projectile component FIRST instead of tag
        else if (collision.gameObject.GetComponent<Projectile>() != null)
        {
            Debug.Log("Projectile hit pterodactyl!");
            
            // Get projectile component
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            float damageAmount = (projectile != null) ? projectile.damage : 1f;
            
            // Apply damage
            TakeHit(damageAmount);
            
            // Destroy bullet
            Destroy(collision.gameObject);
        }
        // Fallback for objects with "bullet" in name
        else if (collision.gameObject.name.ToLower().Contains("bullet"))
        {
            Debug.Log("Object with 'bullet' in name hit pterodactyl!");
            TakeHit(1f);
            Destroy(collision.gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for Projectile component FIRST instead of tag
        if (other.GetComponent<Projectile>() != null)
        {
            Debug.Log("Projectile trigger hit pterodactyl!");
            
            // Get projectile component
            Projectile projectile = other.GetComponent<Projectile>();
            float damageAmount = (projectile != null) ? projectile.damage : 1f;
            
            // Apply damage
            TakeHit(damageAmount);
            
            // Destroy bullet
            Destroy(other.gameObject);
        }
        // Fallback for objects with "bullet" or "projectile" in name
        else if (other.name.ToLower().Contains("bullet") ||
                 other.name.ToLower().Contains("projectile"))
        {
            Debug.Log("Object with 'bullet' in name trigger hit pterodactyl!");
            TakeHit(1f);
            Destroy(other.gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw dive range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, diveRange);
        
        // Draw patrol path
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Vector3 pos = patrolPoints[i].position;
                    Vector3 nextPos = patrolPoints[(i + 1) % patrolPoints.Length] != null ? 
                        patrolPoints[(i + 1) % patrolPoints.Length].position : pos;
                    
                    Gizmos.DrawLine(pos, nextPos);
                    Gizmos.DrawSphere(pos, 0.2f);
                }
            }
        }
    }
}