using System.Collections;
using UnityEngine;

public class FrogEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float idleTime = 1f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float enemyDetectionRadius = 2f;
    [SerializeField] private float spacingOffset = 1.5f;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    private bool isJumping = false;
    private Vector2 targetPosition;
    private string currentState = "Idle";

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            
        StartCoroutine(FrogBehavior());
    }

    private IEnumerator FrogBehavior()
    {
        while (true)
        {
            // Idle state
            ChangeAnimationState("Idle");
            yield return new WaitForSeconds(idleTime);
            
            // Calculate jump target (player position)
            targetPosition = CalculateJumpTarget();
            
            // Jump state
            ChangeAnimationState("Jump");
            isJumping = true;
            
            // Calculate jump direction
            Vector2 jumpDirection = (targetPosition - (Vector2)transform.position).normalized;
            
            // Apply jump force with arc
            rb.velocity = new Vector2(jumpDirection.x * jumpForce, jumpHeight);
            
            // Flip sprite based on jump direction
            if (jumpDirection.x > 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (jumpDirection.x < 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            
            // Wait until landing
            yield return new WaitUntil(() => IsGrounded());
            
            isJumping = false;
            ChangeAnimationState("Land");
            yield return new WaitForSeconds(0.2f); // Short landing animation
        }
    }

    private Vector2 CalculateJumpTarget()
    {
        // Start with player position as target
        Vector2 target = playerTransform.position;
        
        // Check for nearby enemies
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, enemyDetectionRadius, enemyLayer);
        
        foreach (Collider2D enemyCollider in nearbyEnemies)
        {
            // Skip self
            if (enemyCollider.gameObject == gameObject)
                continue;
                
            // Calculate direction from other enemy to player
            Vector2 enemyToPlayer = ((Vector2)playerTransform.position - (Vector2)enemyCollider.transform.position).normalized;
            
            // Adjust target position to create spacing between enemies
            // This will position the frog slightly away from other enemies but still near the player
            target += enemyToPlayer * spacingOffset;
        }
        
        return target;
    }

    private bool IsGrounded()
    {
        // Simple ground check - can be improved with raycasts
        return rb.velocity.y <= 0.1f && Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
    }

    private void ChangeAnimationState(string newState)
    {
        // Prevent same animation from interrupting itself
        if (currentState == newState) return;
        
        // Play the animation
        animator.Play(newState);
        
        // Reassign the current state
        currentState = newState;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize enemy detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);
    }
}
