using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private TMP_Dropdown sceneDropdown;
    
    [Header("Scene Settings")]
    [SerializeField] private string tutorialSceneName = "Tutorial";
    [SerializeField] private string defaultGameSceneName = "Level1";
    
    private const string LastSceneKey = "LastSelectedScene";
    private List<string> sceneNames = new List<string>();
    
    private void Start()
    {
        InitializeButtons();
        PopulateSceneDropdown();
    }
    
    private void InitializeButtons()
    {
        // Set up Play button
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }
        
        // Set up Tutorial button
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(StartTutorial);
        }
    }
    
    private void PopulateSceneDropdown()
    {
        if (sceneDropdown == null) return;
        
        // Clear existing options
        sceneDropdown.ClearOptions();
        sceneNames.Clear();
        
        // Get all scenes in build settings
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            sceneNames.Add(sceneName);
        }
        
        // Add scene names to dropdown
        sceneDropdown.AddOptions(sceneNames);
        
        // Try to set the dropdown to the last selected scene
        string lastScene = PlayerPrefs.GetString(LastSceneKey, defaultGameSceneName);
        int sceneIndex = sceneNames.IndexOf(lastScene);
        if (sceneIndex >= 0)
        {
            sceneDropdown.value = sceneIndex;
        }
        
        // Add listener for dropdown changes
        sceneDropdown.onValueChanged.AddListener(OnSceneDropdownChanged);
    }
    
    private void OnSceneDropdownChanged(int index)
    {
        if (index >= 0 && index < sceneNames.Count)
        {
            PlayerPrefs.SetString(LastSceneKey, sceneNames[index]);
            PlayerPrefs.Save();
        }
    }
    
    public void PlayGame()
    {
        // Get the selected scene from the dropdown
        string selectedScene = defaultGameSceneName;
        
        if (sceneDropdown != null && sceneDropdown.value < sceneNames.Count)
        {
            selectedScene = sceneNames[sceneDropdown.value];
        }
        
        // Load the selected scene
        SceneManager.LoadScene(selectedScene);
    }
    
    public void StartTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
