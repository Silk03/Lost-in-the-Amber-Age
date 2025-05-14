using UnityEngine;

public class Projectile : MonoBehaviour
{

    public float speed = 4.5f;


    private void Update()
    {
        transform.position += transform.right * Time.deltaTime * speed;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.collider.GetComponent<Enemy>();
        
        if (enemy)
        {

            enemy.TakeHit(1);

        }

        Destroy(gameObject);


    }


}
