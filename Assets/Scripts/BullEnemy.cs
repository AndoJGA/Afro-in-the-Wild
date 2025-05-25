using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.5f; // Very slow speed
    [SerializeField] private float minDistanceFromOtherEnemies = 1.5f;
    [SerializeField] private float playerPredictionFactor = 0.4f; // How much to predict player movement
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 10f; // Radius to detect player
    [SerializeField] private LayerMask playerLayer; // Layer mask for the player
    [SerializeField] private bool debugDrawDetectionRadius = true; // For visualization
    [SerializeField] private Color detectionRadiusColor = new Color(0, 0.8f, 1f, 0.2f); // Color for the detection radius
    [SerializeField] private bool playerDetected = false; // Whether player is detected
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // Private variables
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Vector2 targetPosition;
    private bool isGrounded = false;
    private float groundCheckDistance = 0.1f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
            
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
            
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null && transform.childCount > 0)
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            
        rb.gravityScale = 1; // Keep gravity for ground detection
        rb.freezeRotation = true;
            
        // Find player if not assigned
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        // Check if player is within detection radius
        DetectPlayer();
        
        if (playerTransform == null || !playerDetected)
            return;
            
        // Update sprite direction to face the player
        UpdateSpriteDirection();
            
        // Check if grounded
        CheckGrounded();
    }
    
    private void FixedUpdate()
    {
        if (playerTransform == null || !playerDetected)
            return;
            
        // Calculate target position - try to predict where player is going
        CalculateTargetPosition();
            
        // Calculate movement direction
        Vector2 moveDirection = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
        moveDirection.y = 0; // Don't jump - keep y movement at 0
            
        // Check for other enemies and adjust position if needed
        AdjustForOtherEnemies(ref moveDirection);
            
        // Apply horizontal movement only
        if (isGrounded) // Only move if grounded
        {
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
        }
    }
    
    private void DetectPlayer()
    {
        // Use Physics2D.OverlapCircle to detect if player is within radius
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        
        // Update player detection status
        playerDetected = playerCollider != null;
        
        // If we just detected the player and didn't have a reference before, get it
        if (playerDetected && playerTransform == null && playerCollider != null)
        {
            playerTransform = playerCollider.transform;
        }
    }
    
    private void CalculateTargetPosition()
    {
        // Get player velocity to predict movement
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        Vector2 playerVelocity = Vector2.zero;
            
        if (playerRb != null)
        {
            playerVelocity = playerRb.velocity;
        }
            
        // Predict where player is going
        Vector2 predictedPosition = (Vector2)playerTransform.position + (playerVelocity * playerPredictionFactor);
            
        // Try to position to make player fall
        // If player is moving, try to get slightly ahead of them
        if (playerVelocity.magnitude > 0.1f)
        {
            // Check if player is near an edge
            bool playerNearEdge = CheckIfPositionNearEdge(playerTransform.position);
                        
            if (playerNearEdge)
            {
                // If player is near edge, try to push them off
                targetPosition = (Vector2)playerTransform.position;
            }
            else
            {
                // Otherwise, try to intercept their path
                targetPosition = predictedPosition;
            }
        }
        else
        {
            // If player is not moving much, just go directly to them
            targetPosition = (Vector2)playerTransform.position;
        }
    }
    
    private bool CheckIfPositionNearEdge(Vector3 position)
    {
        // Cast a ray downward from slightly in front of the position
        Vector2 raycastOrigin = new Vector2(position.x + 1.0f, position.y);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, 2.0f, groundLayer);
            
        // If no ground is detected, position is near an edge
        return hit.collider == null;
    }
    
    private void UpdateSpriteDirection()
    {
        if (spriteRenderer == null)
            return;
                
        // Flip sprite based on movement direction
        if (targetPosition.x > transform.position.x)
        {
            spriteRenderer.flipX = true; // Face right
        }
        else if (targetPosition.x < transform.position.x)
        {
            spriteRenderer.flipX = false; // Face left
        }
    }
    
    private void CheckGrounded()
    {
        // Cast a ray downward to check if the enemy is grounded
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 
            circleCollider.radius + groundCheckDistance, groundLayer);
            
        isGrounded = hit.collider != null;
    }
    
    private void AdjustForOtherEnemies(ref Vector2 moveDirection)
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, minDistanceFromOtherEnemies * 1.5f, enemyLayer);
            
        Vector2 avoidanceDirection = Vector2.zero;
        bool needsAdjustment = false;
            
        foreach (Collider2D enemyCollider in nearbyEnemies)
        {
            if (enemyCollider.gameObject == gameObject)
                continue;
                        
            float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
                    
            if (distance < minDistanceFromOtherEnemies)
            {
                Vector2 awayDirection = ((Vector2)transform.position - (Vector2)enemyCollider.transform.position).normalized;
                float weight = 1 - (distance / minDistanceFromOtherEnemies);
                avoidanceDirection += awayDirection * weight;
                needsAdjustment = true;
            }
        }
            
        if (needsAdjustment)
        {
            // Combine the original direction with avoidance direction
            // This creates a formation where enemies maintain spacing while still following the player
            moveDirection = (moveDirection + avoidanceDirection.normalized).normalized;
            moveDirection.y = 0; // Ensure no vertical movement
                    
            // Try to position strategically to make player fall
            // If we have multiple enemies, try to surround the player
            if (nearbyEnemies.Length > 1)
            {
                // Calculate average position of all enemies
                Vector2 averageEnemyPos = Vector2.zero;
                foreach (Collider2D enemyCollider in nearbyEnemies)
                {
                    if (enemyCollider.gameObject != gameObject)
                        averageEnemyPos += (Vector2)enemyCollider.transform.position;
                }
                            
                if (nearbyEnemies.Length > 1)
                    averageEnemyPos /= (nearbyEnemies.Length - 1);
                            
                // Try to position on the opposite side of the player from other enemies
                Vector2 playerPos = (Vector2)playerTransform.position;
                Vector2 fromAvgToPlayer = (playerPos - averageEnemyPos).normalized;
                Vector2 strategicPos = playerPos + fromAvgToPlayer;
                            
                // Blend this strategic position into our movement
                Vector2 toStrategicPos = (strategicPos - (Vector2)transform.position).normalized;
                moveDirection = Vector2.Lerp(moveDirection, toStrategicPos, 0.4f);
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If colliding with player, add a small force to push them
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 pushDirection = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
                playerRb.AddForce(pushDirection * 3.0f, ForceMode2D.Impulse);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize minimum distance from other enemies
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromOtherEnemies);
            
        // Visualize target position
        if (Application.isPlaying && playerTransform != null && playerDetected)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPosition, 0.3f);
        }
        
        // Visualize player detection radius
        if (debugDrawDetectionRadius)
        {
            Gizmos.color = detectionRadiusColor;
            Gizmos.DrawSphere(transform.position, detectionRadius);
            
            // Draw wire sphere for better visibility
            Gizmos.color = new Color(detectionRadiusColor.r, detectionRadiusColor.g, detectionRadiusColor.b, 1f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // If player is detected, draw a line to the player
            if (Application.isPlaying && playerDetected && playerTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }
        }
    }
}
