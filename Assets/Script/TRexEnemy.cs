using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TRexEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float hitpoints;
    public float maxhitpoints = 10;
    
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float detectionRange = 8f;
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
    public string enemyDescription = "The king of dinosaurs. A fierce predator with powerful jaws that can crush bones.";
    public Sprite enemyIcon;

    public GameObject trexInfoPanel;
    public TMP_Text trexInfoText;
    
    [Header("Attack")]
    public int biteDamage = 3;         // Higher damage than regular enemies
    public float biteRange = 2.0f;     // Longer bite range
    public float biteCooldown = 1.5f;  // Longer cooldown
    private float lastBiteTime = 0f;   
    private bool isPerformingBite = false;
    
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
            Debug.LogError("Rigidbody2D component missing from TRexEnemy!");
            
        if (animator == null)
            Debug.LogWarning("Animator component missing from TRexEnemy!");
            
        if (groundCheck == null)
            Debug.LogWarning("Ground check transform not assigned to TRexEnemy!");
        
        // Verify animator parameters
        if (animator != null)
        {
            bool hasBiteParam = false;
            
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Bite" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasBiteParam = true;
                }
            }
            
            if (!hasBiteParam)
            {
                Debug.LogWarning("Animator is missing 'Bite' trigger parameter!");
            }
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
        if (animator != null)
        {
            animator.SetBool("IsRunning", isRunning);
        }
        
        // Force animation update if stuck
        if (isPerformingBite && animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("Bite") && Time.time < lastBiteTime + 0.2f)
            {
                // Force it to play
                animator.Play("Bite", 0, 0);
                Debug.Log("Forcing bite animation in Update - animator was stuck!");
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
    
    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length <= 0)
            return;
            
        // Move towards current patrol point
        Transform target = patrolPoints[currentPatrolIndex];
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
            
        // Don't move if biting
        if (isPerformingBite)
        {
            rb.linearVelocity = Vector2.zero; // CHANGE FROM linearVelocity
            return;
        }
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // If close enough, try to bite
        if (distanceToPlayer <= biteRange)
        {
            // Only bite if enough time has passed AND not already biting
            if (Time.time > lastBiteTime + biteCooldown && !isPerformingBite)
            {
                PerformBite();
            }
            
            // Stop moving when in bite range
            rb.linearVelocity = Vector2.zero; // CHANGE FROM linearVelocity
        }
        else
        {
            // Move towards player
            MoveTowards(player.position);
        }
        
        // Check if we should go back to patrolling
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            patrolMode = true;
        }
    }
    
    void MoveTowards(Vector3 targetPosition)
    {
        // Check if we're grounded
        bool isGrounded = false;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        }
        
        // Only move horizontally if grounded
        if (isGrounded)
        {
            float xDirection = targetPosition.x - transform.position.x;
            
            // Update facing direction
            if (xDirection > 0 && !faceRight || xDirection < 0 && faceRight)
            {
                Flip();
            }
            
            // Move in appropriate direction
            rb.linearVelocity = new Vector2(Mathf.Sign(xDirection) * moveSpeed, rb.linearVelocity.y);
            
            // Update running animation
            isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        }
        else
        {
            // Stop horizontal movement if not grounded
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            isRunning = false;
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
    
    private void PerformBite()
    {
        // Update last bite time
        lastBiteTime = Time.time;
        
        // Set bite flag
        isPerformingBite = true;
        
        // Play bite animation
        if (animator != null)
        {
            animator.SetTrigger("Bite");
        }
        
        // Flash red for visual feedback
        FlashAttack();
        
        // Apply damage
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= biteRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(biteDamage);
                    Debug.Log($"T-Rex bit player for {biteDamage} damage");
                }
            }
        }
        
        // Reset bite state after delay
        StartCoroutine(EndBiteAfterDelay(0.5f));
    }
    
    // Replace your ResetBiteState with this simpler version
    private IEnumerator EndBiteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPerformingBite = false;
        Debug.Log("T-Rex bite ended");
    }
    
    IEnumerator ApplyBiteDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Check if player is still in range (they might have moved)
        if (player != null)
        {
            float currentDistanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (currentDistanceToPlayer <= biteRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(biteDamage);
                    Debug.Log($"Applied {biteDamage} bite damage to player");
                }
            }
        }
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
    
    public void TakeHit(float damage)
    {
        hitpoints -= damage;
        
        // Visual feedback
        FlashDamage();
        
        if (hitpoints <= 0)
        {
            Debug.Log($"Enemy {enemyName} died, attempting to show popup");
            
            var infoManager = FindFirstObjectByType<InfoPopup>();
            if (infoManager != null)
            {
                infoManager.ShowEnemyInfo(enemyName, enemyDescription, enemyIcon);
            }
            else if (trexInfoPanel != null && trexInfoText != null)
            {
                trexInfoPanel.SetActive(true);
                trexInfoText.text = enemyDescription;
                StartCoroutine(CloseInfoPanel(3.0f));
            }
            else
            {
                Debug.LogError("No way to display enemy info found!");
            }
            
            // Play death animation if available
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
            
            // Disable components
            GetComponent<Collider2D>().enabled = false;
            rb.linearVelocity = Vector2.zero;
            this.enabled = false;
            
            // Destroy after delay
            Destroy(gameObject, 1.0f);
        }
        else
        {
            // When hit but not killed, consider becoming aggressive
            patrolMode = false;
        }
    }
    
    IEnumerator CloseInfoPanel(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (trexInfoPanel != null)
            trexInfoPanel.SetActive(false);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle player collision
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit T-Rex! Triggering Game Over.");
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
                gameManager.GameOver();
        }
        // Check for Projectile component FIRST instead of tag
        else if (collision.gameObject.GetComponent<Projectile>() != null)
        {
            Debug.Log("Projectile hit T-Rex!");
            
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
            Debug.Log("Object with 'bullet' in name hit T-Rex!");
            TakeHit(1f);
            Destroy(collision.gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for Projectile component FIRST instead of tag
        if (other.GetComponent<Projectile>() != null)
        {
            Debug.Log("Projectile trigger hit T-Rex!");
            
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
            Debug.Log("Object with 'bullet' in name trigger hit T-Rex!");
            TakeHit(1f);
            Destroy(other.gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw bite range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, biteRange);
        
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