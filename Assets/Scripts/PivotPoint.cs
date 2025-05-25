using UnityEngine;

public class PivotPoint : MonoBehaviour
{
    public Transform board;
    public Transform pivot;
    
    void Start()
    {
        // Get or add the required components
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        HingeJoint2D hinge = GetComponent<HingeJoint2D>();
        
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
            
        if (hinge == null)
            hinge = gameObject.AddComponent<HingeJoint2D>();
        
        // Configure the rigidbody
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        
        // Configure the hinge joint
        hinge.anchor = Vector2.zero; // Set the anchor at the center of the seesaw
        
        // Set rotation limits
        hinge.useLimits = true;
        JointAngleLimits2D limits = new JointAngleLimits2D();
        limits.min = -30;
        limits.max = 30;
        hinge.limits = limits;
        
        // Optional: Add some motor settings for smoother movement
        hinge.useMotor = false;
        JointMotor2D motor = new JointMotor2D();
        motor.maxMotorTorque = 10000;
        motor.motorSpeed = 0;
        hinge.motor = motor;
    }
}
