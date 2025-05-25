using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float idleSpeed = 2.5f;
    [SerializeField] private float dashSpeed = 5.0f;
    [SerializeField] private float dashCooldown = 5.0f;
    [SerializeField] private float dashDuration = 1.5f;
    [SerializeField] private float pauseDuration = 1.0f;
    [SerializeField] private float detectionRadius = 10.0f;
    [SerializeField] private float minDistanceFromOtherEnemies = 2.0f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Edge Detection")]
    [SerializeField] private float edgeRaycastLength = 1.0f;
    [SerializeField] private Vector2 edgeRaycastOffset = new Vector2(0.5f, -0.1f);

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Private variables
    private Vector3 lastPlayerPosition;
    private bool isDashing = false;
    private bool isPaused = false;
    private float dashTimer = 0f;
    private float pauseTimer = 0f;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private bool isPlayerDetected = false;
    private int movementDirection = 1; // 1 for right, -1 for left

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

    private void Start()
    {
        if (playerTransform != null)
            lastPlayerPosition = playerTransform.position;
            
        dashTimer = dashCooldown; // Start ready to dash
    }

    private void Update()
    {
        if (playerTransform == null)
            return;

        // Check if player is detected
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer < detectionRadius;

        // Update sprite direction to face the player
        UpdateSpriteDirection();

        // Update timers
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                isPaused = true;
                pauseTimer = pauseDuration;
                lastPlayerPosition = playerTransform.position;
            }
        }
        else if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                isPaused = false;
                dashTimer = dashCooldown;
            }
        }
        else
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0 && isPlayerDetected)
            {
                isDashing = true;
                dashTimer = dashDuration;
                lastPlayerPosition = playerTransform.position;
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
            return;

        Vector2 moveDirection;
        float currentSpeed;

        if (isPlayerDetected)
        {
            // Player is detected - follow or dash behavior
            if (isDashing)
            {
                // During dash, move toward the last known player position
                moveDirection = (lastPlayerPosition - transform.position).normalized;
                moveDirection.y = 0; // Don't jump - keep y movement at 0
                currentSpeed = dashSpeed;
            }
            else if (isPaused)
            {
                // When paused, stay at the last player position
                rb.velocity = new Vector2(0, rb.velocity.y); // Maintain vertical velocity for gravity
                return;
            }
            else
            {
                // Normal following behavior
                moveDirection = (playerTransform.position - transform.position).normalized;
                moveDirection.y = 0; // Don't jump - keep y movement at 0
                currentSpeed = idleSpeed;
                
                // Check for other enemies and adjust position if needed
                AdjustForOtherEnemies(ref moveDirection);
            }
        }
        else
        {
            // No player detected - patrol behavior with edge detection
            if (CheckForEdge())
            {
                // Edge detected, reverse direction
                movementDirection *= -1;
            }
            
            moveDirection = new Vector2(movementDirection, 0);
            currentSpeed = idleSpeed * 0.7f; // Slower patrol speed
        }

        // Apply horizontal movement only
        rb.velocity = new Vector2(moveDirection.x * currentSpeed, rb.velocity.y);
    }

    private void UpdateSpriteDirection()
    {
        if (spriteRenderer == null)
            return;
            
        Vector3 targetPosition = isPlayerDetected ? playerTransform.position : transform.position + new Vector3(movementDirection, 0, 0);
        
        // Flip sprite based on movement direction
        if (targetPosition.x > transform.position.x)
        {
            spriteRenderer.flipX = false; // Face right
        }
        else if (targetPosition.x < transform.position.x)
        {
            spriteRenderer.flipX = true; // Face left
        }
    }

    private bool CheckForEdge()
    {
        if (isPlayerDetected)
            return false; // Ignore edge detection when player is detected
            
        // Calculate raycast origin based on movement direction
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(edgeRaycastOffset.x * movementDirection, edgeRaycastOffset.y);
        
        // Cast ray downward to check for ground
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, edgeRaycastLength, groundLayer);
        
        // If no ground is detected, we're at an edge
        return hit.collider == null;
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
            
            // Strategically position to make player fall (if on platforms)
            // This slightly adjusts the enemy position to try to get in the player's escape path
            Vector2 playerVelocity = playerTransform.GetComponent<Rigidbody2D>()?.velocity ?? Vector2.zero;
            if (playerVelocity.magnitude > 0.1f)
            {
                Vector2 predictedPlayerPos = (Vector2)playerTransform.position + playerVelocity.normalized;
                Vector2 interceptDirection = (predictedPlayerPos - (Vector2)transform.position).normalized;
                interceptDirection.y = 0; // Ensure no vertical movement
                moveDirection = Vector2.Lerp(moveDirection, interceptDirection, 0.3f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Visualize minimum distance from other enemies
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromOtherEnemies);
        
        // Visualize edge detection raycast
        Gizmos.color = Color.blue;
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(edgeRaycastOffset.x * movementDirection, edgeRaycastOffset.y);
        Gizmos.DrawLine(raycastOrigin, raycastOrigin + Vector2.down * edgeRaycastLength);
    }
}
