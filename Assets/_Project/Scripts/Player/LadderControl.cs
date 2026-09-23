using UnityEngine;

public class LadderMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float climbSpeed = 8f;
    [SerializeField] private float normalGravity = 4f;
    [SerializeField] private float ladderJumpForce = 12f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    private float verticalInput;
    private bool isTouchingLadder;
    private bool isClimbing;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");

        // Start climbing
        if (isTouchingLadder && Mathf.Abs(verticalInput) > 0.01f)
        {
            isClimbing = true;
        }

        // Stop climbing when no input
        if (Mathf.Abs(verticalInput) < 0.01f)
        {
            isClimbing = false;
        }

        // Jump off ladder
        if (isClimbing && Input.GetKeyDown(KeyCode.Space))
        {
            isClimbing = false;
            rb.gravityScale = normalGravity;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, ladderJumpForce);
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
        }
        else
        {
            rb.gravityScale = normalGravity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isTouchingLadder = true;
            Debug.Log("Touching ladder");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isTouchingLadder = false;
            isClimbing = false;
            rb.gravityScale = normalGravity;
            Debug.Log("Left ladder");
        }
    }

    // Public method so PlayerController knows if we're climbing
    public bool IsClimbing()
    {
        return isClimbing;
    }
}