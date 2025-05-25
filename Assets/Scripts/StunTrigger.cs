using UnityEngine;
using System.Collections;

public class StunTrigger : MonoBehaviour
{
    [Tooltip("Tag of the object to stun when touched")]
    public string targetTag = "Player";
    
    [Tooltip("Duration of the stun effect in seconds")]
    public float stunDuration = 0.3f;

    private void OnCollisionEnter(Collision collision)
    {
        CheckForStun(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckForStun(other.gameObject);
    }

    private void CheckForStun(GameObject obj)
    {
        if (obj.CompareTag(targetTag))
        {
            // Find the player's movement script
            // This assumes the movement script is called "PlayerMovement"
            // You may need to change this to match your actual movement script name
            PlayerMovement playerMovement = obj.GetComponent<PlayerMovement>();
            
            if (playerMovement != null)
            {
                StartCoroutine(StunPlayer(playerMovement));
            }
        }
    }

    private IEnumerator StunPlayer(PlayerMovement playerMovement)
    {
        // Disable player movement
        playerMovement.enabled = false;
        
        // Wait for the stun duration
        yield return new WaitForSeconds(stunDuration);
        
        // Re-enable player movement
        playerMovement.enabled = true;
    }
}
