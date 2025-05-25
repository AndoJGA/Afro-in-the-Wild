using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxLives = 5;
    [SerializeField] private int currentLives;
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float knockbackForce = 5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private int numberOfFlashes = 3;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI deathMessageText;
    
    [Header("Events")]
    public UnityEvent<int, int> OnLivesChanged;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnGameOver;
    
    // Properties
    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    
    // Private variables
    private bool isInvincible = false;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private string[] deathMessages = new string[] 
    {
        "You died!",
        "Try again!",
        "Ouch!",
        "That looked painful!",
        "Keep trying!"
    };
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    private void Start()
    {
        currentLives = maxLives;
        
        // Update UI
        UpdateLivesText();
        OnLivesChanged?.Invoke(currentLives, maxLives);
    }
    
    // Method to get current health for external scripts
    public int GetCurrentHealth()
    {
        return currentLives;
    }
    
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (isInvincible || currentLives <= 0)
            return;
            
        currentLives -= damage;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        
        // Update UI
        UpdateLivesText();
        OnLivesChanged?.Invoke(currentLives, maxLives);
        
        // Apply knockback
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }
        
        // Play hurt sound
        if (hurtSound != null)
            AudioSource.PlayClipAtPoint(hurtSound, transform.position);
        
        // Visual feedback
        StartCoroutine(FlashRoutine());
        
        // Check for game over
        if (currentLives <= 0)
            Die();
        else
            StartCoroutine(InvincibilityRoutine());
    }
    
    private void UpdateLivesText()
    {
        if (livesText != null)
            livesText.text = "" + currentLives;
    }
    
    private void Die()
    {
        // Play death sound
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        
        // Spawn death effect
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        
        // Trigger death event
        OnPlayerDeath?.Invoke();
        
        // Disable player components
        DisablePlayer();
        
        // Show random death message
        if (deathMessageText != null)
        {
            string randomMessage = deathMessages[Random.Range(0, deathMessages.Length)];
            deathMessageText.text = randomMessage;
        }
        
        if (currentLives <= 0)
            GameOver();
        else
            StartCoroutine(RestartAfterDelay(3f));
    }
    
    private void DisablePlayer()
    {
        // Disable components
        if (playerCollider != null)
            playerCollider.enabled = false;
            
        // Disable scripts that control the player
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this && script.enabled)
                script.enabled = false;
        }
        
        // Stop movement
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }
    }
    
    private void GameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
            
        OnGameOver?.Invoke();
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0); // Assuming main menu is scene 0
    }
    
    private IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null)
            yield break;
            
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }
    }
    
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        
        if (spriteRenderer != null)
        {
            float elapsed = 0;
            while (elapsed < invincibilityDuration)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }
        
        isInvincible = false;
    }
    
    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Reset player position to spawn point
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        if (spawnPoint != null)
            transform.position = spawnPoint.transform.position;
            
        // Re-enable player
        if (playerCollider != null)
            playerCollider.enabled = true;
            
        // Re-enable scripts
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this && !script.enabled)
                script.enabled = true;
        }
        
        // Reset gravity
        if (rb != null)
            rb.gravityScale = 1;
            
        // Start countdown timer
        FindObjectOfType<RestartScript>()?.StartCountdown();
    }
}
