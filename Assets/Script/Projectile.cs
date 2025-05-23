using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 4.5f;
    public float damage = 1f;
    
    // Add direction property - set this when instantiating the projectile
    private int direction = 1; // 1 for right, -1 for left
    
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
        
        // OR OPTION 2: If that doesn't work, keep direction variable but don't rotate
        // transform.position += Vector3.right * direction * Time.deltaTime * speed;
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
