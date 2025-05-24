using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float horizontal;
    [SerializeField] float vertical;
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpingPower = 7f;
    [SerializeField] bool isFacingRight = true;

    public Projectile projectilePrefab;
    public Transform launchOffset;

    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Animator animator;
    [SerializeField] string RUN_ANIMATION = "Run"; 

    [SerializeField] bool isRunning;
    
    // Add reference to ammo manager
    private AmmoManager ammoManager;

    [Header("Enemy Jump")]
    [SerializeField] float enemyBounceHeight = 10f; // Higher than normal jump
    [SerializeField] bool canJumpOnEnemies = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the ammo manager component
        ammoManager = GetComponent<AmmoManager>();
        
        // If ammo manager doesn't exist, add a warning
        if (ammoManager == null)
        {
            Debug.LogWarning("AmmoManager component not found on player. Unlimited ammo will be used.");
        }
        
        // Verify components
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not assigned to Movement script!");
        }
        
        if (groundCheck == null)
        {
            Debug.LogError("Ground Check transform not assigned to Movement script!");
        }
        
        if (animator == null)
        {
            Debug.LogWarning("Animator not assigned to Movement script!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            bool grounded = IsGrounded();
            Debug.Log("Jump pressed, grounded: " + grounded);
            
            if (grounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower); // CHANGED FROM linearVelocity
            }
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f) // CHANGED FROM linearVelocity
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); // CHANGED FROM linearVelocity
        }

        Flip();

        // Modified shooting code to check for ammo
        if (Input.GetButtonDown("Fire1"))
        {
            // Check if we have ammo manager and if we can use ammo
            if (ammoManager == null || ammoManager.UseAmmo())
            {
                // Instantiate the projectile
                Projectile projectile = Instantiate(projectilePrefab, launchOffset.position, transform.rotation);
                
                // Set the direction based on player facing
                projectile.SetDirection(isFacingRight);
            }
        }
    }

    private void FixedUpdate()
    {
        // Apply movement
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y); // CHANGED FROM linearVelocity
        
        // Update animation based on actual velocity
        isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f; // CHANGED FROM linearVelocity
        if (animator != null)
        {
            animator.SetBool("Run", isRunning);
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        
        // Draw a debug ray to visualize the ground check
        Debug.DrawRay(groundCheck.position, Vector2.down * 0.2f, Color.red, 0.1f);
        
        // Check for ground
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        return grounded;
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    // This detects when the player collides with an enemy from above
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with an enemy
        bool isEnemy = collision.gameObject.CompareTag("Enemy") || 
                       collision.gameObject.GetComponent<Enemy>() != null ||
                       collision.gameObject.GetComponent<TRexEnemy>() != null ||
                       collision.gameObject.GetComponent<PteridactylEnemy>() != null;
        
        if (isEnemy && canJumpOnEnemies)
        {
            // Check if we're hitting the enemy from above
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.y > 0.5f) // We're hitting from above if normal points up
            {
                // Apply an upward force/velocity for the bounce
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, enemyBounceHeight);
                
                // Optional: Play sound
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Play();
                }
                
                // Optional: Deal damage to the enemy
                DamageEnemyFromAbove(collision.gameObject);
                
                Debug.Log("Jumped on enemy: " + collision.gameObject.name);
            }
        }
    }
    
    // Apply damage to the enemy when jumped on
    private void DamageEnemyFromAbove(GameObject enemy)
    {
        // Try different enemy types
        Enemy standardEnemy = enemy.GetComponent<Enemy>();
        if (standardEnemy != null)
        {
            standardEnemy.TakeHit(1f); // Deal 1 damage from jump
            return;
        }
        
        TRexEnemy trex = enemy.GetComponent<TRexEnemy>();
        if (trex != null)
        {
            trex.TakeHit(1f);
            return;
        }
        
        PteridactylEnemy ptero = enemy.GetComponent<PteridactylEnemy>();
        if (ptero != null)
        {
            ptero.TakeHit(1f);
            return;
        }
    }
}