using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 4.5f;
    public float damage = 1f;

    private void Update()
    {
        transform.position += transform.right * Time.deltaTime * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Projectile hit: {collision.gameObject.name}");
        
        // Check for regular Enemy
        var enemy = collision.collider.GetComponent<Enemy>();
        if (enemy)
        {
            Debug.Log($"Damaging Enemy with {damage}");
            enemy.TakeHit(damage);
        }
        
        // Check for TRexEnemy
        var trex = collision.collider.GetComponent<TRexEnemy>();
        if (trex)
        {
            Debug.Log($"Damaging TRexEnemy with {damage}");
            trex.TakeHit(damage);
        }

        Destroy(gameObject);
    }
    
    // Also add trigger version for enemies using triggers
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Projectile trigger with: {collision.gameObject.name}");
        
        // Check for regular Enemy
        var enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            Debug.Log($"Damaging Enemy with {damage}");
            enemy.TakeHit(damage);
        }
        
        // Check for TRexEnemy
        var trex = collision.GetComponent<TRexEnemy>();
        if (trex)
        {
            Debug.Log($"Damaging TRexEnemy with {damage}");
            trex.TakeHit(damage);
        }

        Destroy(gameObject);
    }
}
