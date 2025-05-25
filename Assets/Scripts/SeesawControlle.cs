using UnityEngine;

public class SeesawController : MonoBehaviour
{
    [Header("Seesaw Settings")]
    [Tooltip("Maximum rotation angle in degrees")]
    public float maxRotationAngle = 30f;
    
    [Tooltip("How quickly the seesaw returns to center")]
    public float returnSpeed = 1f;
    
    [Tooltip("Physical properties of the seesaw")]
    public float mass = 10f;
    public float angularDrag = 0.8f;
    
    private Rigidbody2D rb;
    
    private void Awake()
    {
        // Get or add a Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure the Rigidbody2D for a seesaw
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = mass;
        rb.angularDrag = angularDrag;
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | 
                         RigidbodyConstraints2D.FreezeRotation;
        
        // We'll manually control rotation, so freeze it initially
        rb.freezeRotation = true;
    }
    
    private void FixedUpdate()
    {
        // Get current rotation
        float currentRotation = transform.rotation.eulerAngles.z;
        
        // Convert to -180 to 180 range
        if (currentRotation > 180)
            currentRotation -= 360;
            
        // Clamp rotation to max angle
        float clampedRotation = Mathf.Clamp(currentRotation, -maxRotationAngle, maxRotationAngle);
        
        // If we're at the max angle, prevent further rotation
        if (currentRotation != clampedRotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, clampedRotation);
            rb.angularVelocity = 0;
        }
        
        // Apply return force toward center
        if (Mathf.Abs(currentRotation) > 0.1f)
        {
            float returnForce = -currentRotation * returnSpeed * Time.fixedDeltaTime;
            rb.AddTorque(returnForce, ForceMode2D.Force);
        }
    }
    
    // This method can be called to manually apply torque
    public void ApplyTorque(float force)
    {
        rb.AddTorque(force, ForceMode2D.Force);
    }
}
