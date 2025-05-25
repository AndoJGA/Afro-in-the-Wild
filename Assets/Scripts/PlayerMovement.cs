using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 60f;
    [SerializeField] private float velPower = 0.9f;
    [SerializeField] private float frictionAmount = 0.2f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private int maxJumps = 2;
    
    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Roll")]
    [SerializeField] private float rollSpeed = 12f;
    [SerializeField] private float rollDuration = 0.5f;
    [SerializeField] private float rollCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem runningParticles;
    [SerializeField] private ParticleSystem landingParticles;
    [SerializeField] private float runningParticleThreshold = 2f; // Minimum speed to show running particles

    // Private variables
    private Rigidbody2D rb;
    private float moveInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool wasGrounded; // Track previous grounded state
    private bool isJumping;
    private bool isDashing;
    private bool isRolling;
    private bool canDash = true;
    private bool canRoll = true;
    private int jumpCount;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // Animation and visual components
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Initialize particle systems if not assigned
        if (runningParticles == null)
        {
            Debug.LogWarning("Running particles not assigned to PlayerMovement script!");
        }
        
        if (landingParticles == null)
        {
            Debug.LogWarning("Landing particles not assigned to PlayerMovement script!");
        }
    }

    private void Update()
    {
        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Store previous grounded state
        wasGrounded = isGrounded;
        
        // Ground check
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        // Check if just landed
        if (isGrounded && !wasGrounded)
        {
            OnLanding();
        }

        // Coyote time logic
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpCount = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump buffer logic
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Jump
        if (jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || (jumpCount < maxJumps && jumpCount > 0)))
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        // Jump height control
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing && !isRolling)
        {
            StartCoroutine(Dash());
        }

        // Roll
        //if (Input.GetKeyDown(KeyCode.LeftControl) && canRoll && !isRolling && !isDashing)
        //{
          //  StartCoroutine(Roll());
        //}

        // Flip character based on movement direction
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        // Handle running particles
        HandleRunningParticles();

        // Update animations (if you have an animator)
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!isDashing && !isRolling)
        {
            Move();
        }

        // Apply friction when not moving horizontally
        if (isGrounded && Mathf.Abs(moveInput) < 0.01f)
        {
            ApplyFriction();
        }
    }

    private void Move()
    {
        // Calculate target velocity
        float targetSpeed = moveInput * moveSpeed;
        
        // Calculate difference between current velocity and target velocity
        float speedDiff = targetSpeed - rb.velocity.x;
        
        // Change acceleration rate depending on situation
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        
        // Apply acceleration to speed difference, then raise to power for more control
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velPower) * Mathf.Sign(speedDiff);
        
        // Apply force to rigidbody
        rb.AddForce(movement * Vector2.right);
    }

    private void ApplyFriction()
    {
        // Calculate friction amount
        float frictionForce = Mathf.Min(Mathf.Abs(rb.velocity.x), frictionAmount);
        
        // Apply friction in opposite direction of movement
        frictionForce *= Mathf.Sign(rb.velocity.x);
        rb.AddForce(-frictionForce * Vector2.right, ForceMode2D.Impulse);
    }

    private void Jump()
    {
        // Reset Y velocity and apply jump force
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        jumpCount++;
        isJumping = true;
        
        // Stop running particles when jumping
        if (runningParticles != null && runningParticles.isPlaying)
        {
            runningParticles.Stop();
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        
        // Store original gravity
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        
        // Set velocity in facing direction
        rb.velocity = new Vector2(dashSpeed * (isFacingRight ? 1 : -1), 0);
        
        // Stop running particles during dash
        if (runningParticles != null && runningParticles.isPlaying)
        {
            runningParticles.Stop();
        }
        
        yield return new WaitForSeconds(dashDuration);
        
        // Restore gravity and reset state
        rb.gravityScale = originalGravity;
        isDashing = false;
        
        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator Roll()
    {
        canRoll = false;
        isRolling = true;
        
        // Set velocity in facing direction
        rb.velocity = new Vector2(rollSpeed * (isFacingRight ? 1 : -1), rb.velocity.y);
        
        // Stop running particles during roll
        if (runningParticles != null && runningParticles.isPlaying)
        {
            runningParticles.Stop();
        }
        
        yield return new WaitForSeconds(rollDuration);
        
        // Reset state
        isRolling = false;
        
        // Cooldown
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        
        // Option 1: Flip using transform scale
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
        
        // Adjust particle system rotation if needed
        if (runningParticles != null)
        {
            var main = runningParticles.main;
            var shape = runningParticles.shape;
            shape.rotation = new Vector3(shape.rotation.x, isFacingRight ? 0 : 180, shape.rotation.z);
        }
    }

    private void HandleRunningParticles()
    {
        if (runningParticles == null) return;

        // Play running particles when moving on ground above threshold speed
        if (isGrounded && Mathf.Abs(rb.velocity.x) > runningParticleThreshold && !isDashing && !isRolling)
        {
            if (!runningParticles.isPlaying)
            {
                runningParticles.Play();
            }
        }
        else
        {
            if (runningParticles.isPlaying)
            {
                runningParticles.Stop();
            }
        }
    }

    private void OnLanding()
    {
        // Play landing particles when landing from a jump or fall
        if (landingParticles != null && rb.velocity.y < -2f) // Only play when falling at a certain speed
        {
            landingParticles.Play();
        }
        
        isJumping = false;
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsJumping", isJumping);
            animator.SetBool("IsDashing", isDashing);
            animator.SetBool("IsRolling", isRolling);
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check area
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}
