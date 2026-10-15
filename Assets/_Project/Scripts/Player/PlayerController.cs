using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement
    private float Horizontal;
    public float speed = 8f;
    public float jump = 10f;
    private bool isJumping = false;
    private bool isGrounded;
    private Rigidbody2D rb;
    private bool isWallSliding;
    public float wallSlidingSpeed = 2f;
    public Transform wallCheck;
    public LayerMask wallLayer;
    private bool isFacingRight = true;
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);


    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }



    private void Update()
    {
        if (isGrounded)
        {
            Debug.Log("Grounded");
        }
        if (isJumping)
        {
            Debug.Log("Jumping");
        }
        Horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(Horizontal * speed, rb.linearVelocity.y);
        // Jump from ground
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow)) && isJumping == false)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump); //ts the jumping action
            Debug.Log("Jump");
            isJumping = true;
        }

        WallSlide();
        WallJump();
        if (!isWallJumping)
        {
            Flip();
        }
    }
    private void FixedUpdate()
    {
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(Horizontal * speed, rb.linearVelocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
    private void WallSlide()
    {
        if (IsWalled() && isJumping && Horizontal != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow)) && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }



    }
    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        if (isFacingRight && Horizontal < 0f || !isFacingRight && Horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
    private enum MovementState
    {
        running,
        jumping,
        idle,
        grounded,
        onWall,
        onGround
    }
}