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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");


        if (Input.GetButtonDown("Jump") && IsGrounded())
        {

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);


        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);


        }

        isRunning = horizontal != 0;
        animator.SetBool("Run", isRunning);


        



        Flip();

        if (Input.GetButtonDown("Fire1"))
        {
            Instantiate(projectilePrefab, launchOffset.position, transform.rotation);
        }
    }


    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }


    private bool IsGrounded()
    {

        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);


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

    /*public bool canAttack()
    {

        return horizontal == 0 && IsGrounded();


    }*/
}
