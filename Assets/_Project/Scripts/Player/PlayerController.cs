using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;

    // Wall slide
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    // Wall jump
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private LayerMask wallLayer;

    private void Awake()
    {
        // Auto-get Rigidbody2D
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Auto-find GroundCheck if not assigned
        if (groundCheck == null)
            groundCheck = transform.Find("GroundCheck");

        // Auto-find wall checks if not assigned
        if (wallCheckRight == null)
            wallCheckRight = transform.Find("WallCheckRight");
        if (wallCheckLeft == null)
            wallCheckLeft = transform.Find("WallCheckLeft");

        // Error checks
        if (groundCheck == null)
            Debug.LogError("GroundCheck not assigned or found!");
        if (wallCheckRight == null)
            Debug.LogError("WallCheckRight not assigned or found!");
        if (wallCheckLeft == null)
            Debug.LogError("WallCheckLeft not assigned or found!");
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // Jump from ground
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow)) && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }

        // Variable jump height (release jump early = shorter jump)
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
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
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        // Check both sides
        bool wallRight = false;
        bool wallLeft = false;

        if (wallCheckRight != null)
            wallRight = Physics2D.OverlapCircle(wallCheckRight.position, 0.2f, wallLayer);
        if (wallCheckLeft != null)
            wallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, 0.2f, wallLayer);

        return wallRight || wallLeft;
    }

    // Returns 1 if wall is on left, -1 if wall is on right
    private int GetWallDirection()
    {
        if (wallCheckRight != null && Physics2D.OverlapCircle(wallCheckRight.position, 0.2f, wallLayer))
            return -1;  // Wall on right → jump left
        if (wallCheckLeft != null && Physics2D.OverlapCircle(wallCheckLeft.position, 0.2f, wallLayer))
            return 1;   // Wall on left → jump right
        return 0;
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue)
            );
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
            wallJumpingDirection = GetWallDirection();  // Use the side with the wall
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;

            // Jump AWAY from the wall
            rb.linearVelocity = new Vector2(
                wallJumpingDirection * wallJumpingPower.x,
                wallJumpingPower.y
            );
            wallJumpingCounter = 0f;

            // Flip sprite if needed
            if ((wallJumpingDirection == 1 && !isFacingRight) ||
                (wallJumpingDirection == -1 && isFacingRight))
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
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Ground check (green)
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }

        // Wall checks (blue for right, red for left)
        if (wallCheckRight != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheckRight.position, 0.2f);
        }
        if (wallCheckLeft != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheckLeft.position, 0.2f);
        }
    }
}