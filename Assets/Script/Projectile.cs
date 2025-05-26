using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 4.5f;
    public float damage = 1f;
    
    [Header("Range Settings")]
    public float maxRange = 10f;     // Maximum distance the bullet can travel
    public bool showRangeGizmo = true;  // For debugging in editor
    
    // Add direction property - set this when instantiating the projectile
    private int direction = 1; // 1 for right, -1 for left
    private Vector3 startPosition;   // Store initial position to track distance
    
    private void Start()
    {
        // Store the starting position for range calculation
        startPosition = transform.position;
    }
    
    // Add method to set direction from outside
    public void SetDirection(bool isFacingRight)
    {
        // Store the facing direction
        direction = isFacingRight ? 1 : -1;
        
        // Use rotation instead of scale to avoid size issues
        if (!isFacingRight)
        {
            // Rotate 180 degrees around Y axis
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void Update()
    {
        // OPTION 1: Use transform.right only (preferred)
        transform.position += transform.right * Time.deltaTime * speed;
        
        // Check if we've exceeded the maximum range
        float distanceTraveled = Vector3.Distance(transform.position, startPosition);
        if (distanceTraveled >= maxRange)
        {
            // Optional: Add a fade or small effect when bullet expires
            Destroy(gameObject);
        }
    }

    // Optional: Visualize the range in editor
    private void OnDrawGizmosSelected()
    {
        if (showRangeGizmo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxRange);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for Enemy component
        var enemy = collision.collider.GetComponent<Enemy>();
        if (enemy)
        {
            enemy.TakeHit(damage);
        }
        
        // Check for TRexEnemy component
        var trex = collision.collider.GetComponent<TRexEnemy>();
        if (trex)
        {
            trex.TakeHit(damage);
        }
        
        // Check for PteridactylEnemy component
        var ptero = collision.collider.GetComponent<PteridactylEnemy>();
        if (ptero)
        {
            ptero.TakeHit(damage);
        }

        Destroy(gameObject);
    }
    
    // Also add trigger version for enemies using triggers
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't destroy pickups
        if (other.CompareTag("Pickups") || other.gameObject.layer == LayerMask.NameToLayer("Pickups"))
        {
            return;
        }
        
        // Check for Enemy component
        var enemy = other.GetComponent<Enemy>();
        if (enemy)
        {
            enemy.TakeHit(damage);
        }
        
        // Check for TRexEnemy component 
        var trex = other.GetComponent<TRexEnemy>();
        if (trex)
        {
            trex.TakeHit(damage);
        }
        
        // Check for PteridactylEnemy component
        var ptero = other.GetComponent<PteridactylEnemy>();
        if (ptero)
        {
            ptero.TakeHit(damage);
        }

        Destroy(gameObject);
    }
}
