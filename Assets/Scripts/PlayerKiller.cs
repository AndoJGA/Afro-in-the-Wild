using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerKiller : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage to deal to the player")]
    [SerializeField] private int damageAmount = 1;
    
    [Tooltip("Direction to knock the player")]
    [SerializeField] private Vector2 knockbackDirection = Vector2.up;
    
    [Tooltip("Tag of the player object")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Should this object be destroyed after killing the player?")]
    [SerializeField] private bool destroyAfterKill = false;
    
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private string tagToRemove = "Enemy";
    [SerializeField] private LayerMask layerToRemove = 0; // Set in inspector
    
    [Header("Effects")]
    [SerializeField] private AudioClip killSound;
    [SerializeField] private GameObject killEffect;
    [SerializeField] private GameObject respawnEffect;
    
    [Header("Countdown Settings")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private AudioClip countdownSound;
    [SerializeField] private AudioClip respawnSound;
    
    // Internal health tracking to prevent respawn when health is 0 or less
    private int playerCurrentHealth = 0;
    
    private void Start()
    {
        // Make sure countdown panel is initially hidden
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
            
        // Find respawn point if not assigned
        if (respawnPoint == null)
        {
            GameObject respawnObj = GameObject.FindGameObjectWithTag("Respawn");
            if (respawnObj != null)
                respawnPoint = respawnObj.transform;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPlayerContact(collision.gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D collider)
    {
        CheckPlayerContact(collider.gameObject);
    }
    
    private void CheckPlayerContact(GameObject other)
    {
        if (other.CompareTag(playerTag))
        {
            // Get player health component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                // Store current health before damage
                playerCurrentHealth = playerHealth.GetCurrentHealth();
                
                // Calculate knockback direction based on player position
                Vector2 direction = knockbackDirection;
                if (knockbackDirection == Vector2.zero)
                {
                    direction = (other.transform.position - transform.position).normalized;
                }
                
                // Deal damage to player
                playerHealth.TakeDamage(damageAmount, direction);
                
                // Play sound effect
                if (killSound != null)
                {
                    AudioSource.PlayClipAtPoint(killSound, transform.position);
                }
                
                // Spawn effect
                if (killEffect != null)
                {
                    Instantiate(killEffect, other.transform.position, Quaternion.identity);
                }
                
                // Get the updated health after damage
                int updatedHealth = playerHealth.GetCurrentHealth();
                
                // Only start the countdown if player health is greater than 0
                if (updatedHealth > 0)
                {
                    // Start the countdown to respawn
                    StartCoroutine(StartCountdownToRespawn(other));
                }
                else
                {
                    // If health is 0 or less, stop the game
                    Time.timeScale = 0f;
                }
                
                // Destroy this object if needed
                if (destroyAfterKill)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    private IEnumerator StartCountdownToRespawn(GameObject player)
    {
        // Wait a short moment before starting countdown (to let death animation play)
        yield return new WaitForSeconds(0.5f);
        
        // Show countdown panel
        if (countdownPanel != null)
            countdownPanel.SetActive(true);
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Disable player
        DisablePlayer(player);
        
        // Countdown logic - starting from 3 and counting down to 1
        int countdownValue = 3;
        
        while (countdownValue > 0)
        {
            // Update countdown text
            if (countdownText != null)
                countdownText.text = countdownValue.ToString();
            
            // Play countdown sound
            if (countdownSound != null)
                AudioSource.PlayClipAtPoint(countdownSound, Camera.main.transform.position, 1f);
            
            // Wait for a second (using unscaled time)
            yield return new WaitForSecondsRealtime(1f);
            
            // Decrease countdown
            countdownValue--;
        }
        
        // Show "GO!" text
        if (countdownText != null)
            countdownText.text = "GO!";
        
        // Remove specified objects
        RemoveTaggedObjects();
        RemoveLayerObjects();
        
        // Respawn player at respawn point
        if (player != null && respawnPoint != null)
        {
            player.transform.position = respawnPoint.position;
            
            // Spawn respawn effect
            if (respawnEffect != null)
            {
                Instantiate(respawnEffect, respawnPoint.position, Quaternion.identity);
            }
            
            // Play respawn sound
            if (respawnSound != null)
            {
                AudioSource.PlayClipAtPoint(respawnSound, respawnPoint.position);
            }
        }
        
        // Wait a moment with "GO!" displayed
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Hide countdown panel
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
        
        // Re-enable player
        EnablePlayer(player);
        
        // Resume the game
        Time.timeScale = 1f;
    }
    
    private void DisablePlayer(GameObject player)
    {
        if (player == null)
            return;
            
        // Disable all scripts on the player that might handle input
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Keep PlayerHealth active but disable everything else
            if (!(script is PlayerHealth) && !(script is PlayerKiller))
            {
                script.enabled = false;
            }
        }
        
        // Freeze the player's rigidbody if it has one
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
        
        // Make player invisible
        SpriteRenderer renderer = player.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }
    
    private void EnablePlayer(GameObject player)
    {
        if (player == null)
            return;
            
        // Re-enable all scripts on the player
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = true;
        }
        
        // Unfreeze the player's rigidbody
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = true;
        }
        
        // Make player visible again
        SpriteRenderer renderer = player.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }
        
        // Reset player state if needed
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            // Optionally reset health or give invincibility
            // health.AddInvincibility(2f); // If you have such a method
        }
    }
    
    private void RemoveTaggedObjects()
    {
        if (string.IsNullOrEmpty(tagToRemove))
            return;
            
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tagToRemove);
        foreach (GameObject obj in taggedObjects)
        {
            Destroy(obj);
        }
    }
    
    private void RemoveLayerObjects()
    {
        if (layerToRemove.value == 0) // No layer selected
            return;
            
        // Find all objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Check if this object is on the layer to remove
            if (((1 << obj.layer) & layerToRemove.value) != 0)
            {
                Destroy(obj);
            }
        }
    }
}
