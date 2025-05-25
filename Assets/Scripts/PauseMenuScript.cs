using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuScript : MonoBehaviour
{
    [Header("Pause Menu Settings")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private string defaultMenuSceneName = "First Scene";
    [SerializeField] private Button pauseButton;
    
    [Header("Audio Settings")]
    [SerializeField] private Sprite audioOnSprite;
    [SerializeField] private Sprite audioOffSprite;
    [SerializeField] private Button audioToggleButton;
    [SerializeField] private AudioSource[] audioSources;
    
    // Static property with proper getter/setter
    private static bool _gameIsPaused = false;
    public static bool GameIsPaused 
    { 
        get { return _gameIsPaused; }
        private set { _gameIsPaused = value; }
    }
    
    private bool isAudioEnabled = true;
    
    private void Start()
    {
        // Ensure the pause menu is initially hidden
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // Set up the pause button click listener
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }
        
        // Set up the audio toggle button click listener
        if (audioToggleButton != null)
        {
            audioToggleButton.onClick.AddListener(ToggleAudio);
            UpdateAudioButtonSprite();
        }
        
        // Find all audio sources in the scene if not assigned
        if (audioSources == null || audioSources.Length == 0)
        {
            audioSources = FindObjectsOfType<AudioSource>();
        }
    }
    
    private void Update()
    {
        // Allow pausing/unpausing with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    /// <summary>
    /// Toggles between pause and resume states
    /// </summary>
    public void TogglePause()
    {
        if (GameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
    
    /// <summary>
    /// Resumes the game
    /// </summary>
    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    
    /// <summary>
    /// Pauses the game
    /// </summary>
    public void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
    
    /// <summary>
    /// Toggles audio on/off
    /// </summary>
    public void ToggleAudio()
    {
        isAudioEnabled = !isAudioEnabled;
        
        // Update all audio sources
        foreach (AudioSource source in audioSources)
        {
            if (source != null)
            {
                source.mute = !isAudioEnabled;
            }
        }
        
        UpdateAudioButtonSprite();
    }
    
    /// <summary>
    /// Updates the audio button sprite based on current audio state
    /// </summary>
    private void UpdateAudioButtonSprite()
    {
        if (audioToggleButton != null && audioToggleButton.image != null)
        {
            audioToggleButton.image.sprite = isAudioEnabled ? audioOnSprite : audioOffSprite;
        }
    }
    
    /// <summary>
    /// Returns to the main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(defaultMenuSceneName);
    }
    
    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
