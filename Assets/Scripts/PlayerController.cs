using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    
    // Property required for power-ups
    public float MoveSpeed 
    { 
        get { return playerMovement != null ? playerMovement.GetMoveSpeed() : 0f; } 
        set { if (playerMovement != null) playerMovement.SetMoveSpeed(value); } 
    }
    
    private void Awake()
    {
        // Get reference to PlayerMovement if not assigned
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }
    
    // Method to apply knockback (used by PlayerHealth)
    public void ApplyKnockback(Vector2 force)
    {
        if (playerMovement != null)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }
}
