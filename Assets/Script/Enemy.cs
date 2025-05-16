using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            
        // Move towards player
        MoveTowards(player.position);
        
        // Check if we should go back to patrolling
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
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
    }
}
