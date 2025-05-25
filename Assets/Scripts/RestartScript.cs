using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestartScript : MonoBehaviour
{
    [Header("Countdown Settings")]
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject countdownPanel;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button muteButton;
    
    [Header("Audio")]
    [SerializeField] private AudioClip countdownSound;
    [SerializeField] private AudioClip gameOverSound;
    
    private PlayerHealth playerHealth;
    private bool isMuted = false;
    
    private void Awake()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Set up button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
        if (muteButton != null)
            muteButton.onClick.AddListener(ToggleMute);
    }
    
    private void Start()
    {
        // Subscribe to player death event
        if (playerHealth != null)
        {
            playerHealth.OnGameOver.AddListener(ShowGameOverPanel);
        }
    }
    
    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }
    
    private IEnumerator CountdownRoutine()
    {
        if (countdownPanel != null)
            countdownPanel.SetActive(true);
            
        // Freeze player movement during countdown
        FreezePlayer(true);
        
        float timeLeft = countdownDuration;
        
        while (timeLeft > 0)
        {
            // Update countdown text
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
                
            // Play countdown sound
            if (countdownSound != null && timeLeft <= 3 && Mathf.Approximately(timeLeft, Mathf.Floor(timeLeft)))
                AudioSource.PlayClipAtPoint(countdownSound, Camera.main.transform.position);
                
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        
        // Show "GO!" text
        if (countdownText != null)
            countdownText.text = "GO!";
            
        yield return new WaitForSeconds(0.5f);
        
        // Hide countdown panel
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
            
        // Unfreeze player movement
        FreezePlayer(false);
    }
    
    private void FreezePlayer(bool freeze)
    {
        // Find player movement scripts and enable/disable them
        if (playerHealth != null)
        {
            MonoBehaviour[] playerScripts = playerHealth.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in playerScripts)
            {
                // Skip the PlayerHealth script
                if (script is PlayerHealth)
                    continue;
                    
                // Skip this script
                if (script == this)
                    continue;
                    
                // Enable/disable other scripts
                script.enabled = !freeze;
            }
            
            // Freeze rigidbody if present
            Rigidbody2D rb = playerHealth.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.simulated = !freeze;
        }
    }
    
    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
            
        if (gameOverSound != null)
            AudioSource.PlayClipAtPoint(gameOverSound, Camera.main.transform.position);
    }
    
    public void RestartGame()
    {
        playerHealth.RestartGame();
    }
    
    public void ReturnToMainMenu()
    {
        playerHealth.ReturnToMainMenu();
    }
    
    public void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0 : 1;
        
        // Update mute button text or image if needed
        Text buttonText = muteButton.GetComponentInChildren<Text>();
        if (buttonText != null)
            buttonText.text = isMuted ? "Unmute" : "Mute";
    }
}
