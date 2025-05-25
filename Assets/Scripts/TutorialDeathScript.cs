using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialDeathScript : MonoBehaviour
{
    // This method should be called when the player dies
    public void OnPlayerDeath()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // Alternative approach: You can attach this to the player and check for collisions
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the player collided with something that kills them
        if (collision.gameObject.CompareTag("DeathZone"))
        {
            OnPlayerDeath();
        }
    }
    
    // For 2D games, you might want to use this instead
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeathZone"))
        {
            OnPlayerDeath();
        }
    }
}
