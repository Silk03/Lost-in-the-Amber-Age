using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float hitpoints;
    public float maxhitpoints = 5;
    
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
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
    public string enemyName = "Goblin";
    [TextArea(2, 5)]
    public string enemyDescription = "A weak but annoying creature that attacks in groups.";
    public Sprite enemyIcon;

    public GameObject raptorInfoPanel;
    public TMP_Text raptorInfoText;
    
    [Header("Attack")]
    public int biteDamage = 1;         // Damage per bite (changed to int to match PlayerHealth)
    public float biteRange = 1.5f;     // How close player must be to bite
    public float biteCooldown = 1.0f;  // Time between bites
    private float lastBiteTime = 0f;   // When we last bit the player
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
            Debug.LogError("Rigidbody2D component missing from Enemy!");
            
        if (animator == null)
            Debug.LogWarning("Animator component missing from Enemy!");
            
        if (groundCheck == null)
            Debug.LogWarning("Ground Check not assigned to Enemy!");
        
        // Verify animator parameters
        if (animator != null)
        {
            // Check if "Bite" parameter exists
            bool hasBiteParam = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                Debug.Log("Animator has parameter: " + param.name + " (Type: " + param.type + ")");
                if (param.name == "Bite" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasBiteParam = true;
                }
            }
            
            if (!hasBiteParam)
            {
                Debug.LogError("Animator is missing 'Bite' trigger parameter!");
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
        UpdateAnimation();
        
        // Force animation update if stuck
        if (isPerformingBite && animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // If we're still not in the bite animation after trying to bite
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
            
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // If we're already biting, just ensure we stay still
        if (isPerformingBite)
        {
            rb.linearVelocity = Vector2.zero; // CHANGE FROM linearVelocity
            return;
        }
        
        // If close enough to bite, stop and bite
        if (distanceToPlayer <= biteRange)
        {
            // Stop moving when biting
            rb.linearVelocity = Vector2.zero; // CHANGE FROM linearVelocity
            
            // Try to bite - this should always trigger if in range and facing player
            TryBitePlayer(biteRange);
            
            Debug.Log($"Player in bite range: {distanceToPlayer} <= {biteRange}");
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
            rb.linearVelocity = new Vector2(Mathf.Sign(xDirection) * moveSpeed, rb.linearVelocity.y); // CHANGE FROM linearVelocity
            
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
    
    public void TakeHit(float damage)
    {
        hitpoints -= damage;
        if (hitpoints <= 0)
        {
            Debug.Log($"Enemy {enemyName} died, attempting to show popup");
            
            if (InfoPopup.Instance != null)
            {
                InfoPopup.Instance.ShowEnemyInfo(enemyName, enemyDescription, enemyIcon);
            }
            else
            {
                Debug.LogError("InfoPopup.Instance is null! Make sure InfoPopup is in the scene");
            }
            
            Destroy(gameObject, 0.1f);
        }
    }
    
    void ShowRaptorInfo()
    {
        if (raptorInfoPanel != null && raptorInfoText != null)
        {
            raptorInfoPanel.SetActive(true);
            raptorInfoText.text = "Raptor Info:\n- Species: Velociraptor\n- Speed: Fast\n- Habitat: Prehistoric Forests";
        }
    }
    
    public void TryBitePlayer(float range)
    {
        // Skip if player is null, on cooldown, or already biting
        if (player == null || Time.time < lastBiteTime + biteCooldown || isPerformingBite)
            return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if player is in range and we're facing them
        bool isInRange = distanceToPlayer <= range;
        bool isFacingPlayer = (player.position.x > transform.position.x && faceRight) || 
                              (player.position.x < transform.position.x && !faceRight);
        
        // Debug output to check what's happening
        Debug.Log($"Bite check: InRange={isInRange}, Facing={isFacingPlayer}, Distance={distanceToPlayer}, Range={range}");
        
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
        
        // Force immediate animation
        if (animator != null)
        {
            // Clear ALL animation triggers first (important fix)
            animator.ResetTrigger("Bite");
            
            // Force immediate transition to bite animation
            animator.Play("Bite", 0, 0);
            
            // Also set the trigger for normal animation flow
            animator.SetTrigger("Bite");
            
            Debug.Log("FORCED bite animation to play immediately!");
        }
        
        // Visual feedback
        FlashAttack();
        
        // Apply damage immediately instead of waiting
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Apply damage right away
                playerHealth.TakeDamage(biteDamage);
                Debug.Log($"Immediate damage: {biteDamage} to player");
            }
        }
        
        // Reset bite state after a delay
        StartCoroutine(ResetBiteState(0.5f));
    }

    private IEnumerator ResetBiteState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPerformingBite = false;
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
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit! Triggering Game Over.");
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
                gameManager.GameOver();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw patrol path
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Vector3 pos = patrolPoints[i] != null ? patrolPoints[i].position : transform.position;
                Vector3 nextPos = patrolPoints[(i + 1) % patrolPoints.Length] != null ? 
                    patrolPoints[(i + 1) % patrolPoints.Length].position : transform.position;
                
                Gizmos.DrawLine(pos, nextPos);
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }
        
        // Draw bite range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }

    // Add this method to force animation updates
    void OnAnimatorIK(int layerIndex)
    {
        // Force animation update if we're trying to bite
        if (isPerformingBite && animator != null)
        {
            animator.Update(Time.deltaTime);
        }
    }
}
