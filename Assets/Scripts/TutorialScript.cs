using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Canvas tutorialCanvas;
    
    [Header("Tutorial Settings")]
    [SerializeField] private GameObject objectToReveal;
    [SerializeField] private string mainGameSceneName = "MainGame";
    [SerializeField] private float inputDisplayTime = 0.4f;
    [SerializeField] private float freePlayTime = 20f;
    
    private bool waitingForLeftInput = false;
    private bool waitingForRightInput = false;
    private bool waitingForJumpInput = false;
    private bool waitingForShiftInput = false;
    private bool tutorialCompleted = false;

    private void Start()
    {
        // Make sure the object to reveal is hidden at start
        if (objectToReveal != null)
            objectToReveal.SetActive(false);
            
        // Start the tutorial sequence
        StartCoroutine(TutorialSequence());
    }

    private void Update()
    {
        // Check for player inputs during the tutorial
        if (waitingForLeftInput && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)))
        {
            waitingForLeftInput = false;
            Time.timeScale = 1f;
        }
        
        if (waitingForRightInput && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)))
        {
            waitingForRightInput = false;
            Time.timeScale = 1f;
        }
        
        if (waitingForJumpInput && Input.GetKeyDown(KeyCode.Space))
        {
            waitingForJumpInput = false;
            Time.timeScale = 1f;
        }
        
        if (waitingForShiftInput && Input.GetKeyDown(KeyCode.LeftShift))
        {
            waitingForShiftInput = false;
            Time.timeScale = 1f;
        }
    }

    private IEnumerator TutorialSequence()
    {
        // Wait a moment before starting
        yield return new WaitForSeconds(1f);
        
        // Left movement tutorial
        tutorialText.text = "Press Left Arrow or A to move left";
        waitingForLeftInput = true;
        Time.timeScale = 0f;
        
        // Wait until player presses the key
        yield return new WaitUntil(() => !waitingForLeftInput);
        
        // Display for a short time
        yield return new WaitForSeconds(inputDisplayTime);
        
        // Right movement tutorial
        tutorialText.text = "Press Right Arrow or D to move right";
        waitingForRightInput = true;
        Time.timeScale = 0f;
        
        // Wait until player presses the key
        yield return new WaitUntil(() => !waitingForRightInput);
        
        // Display for a short time
        yield return new WaitForSeconds(inputDisplayTime);
        
        // Jump tutorial
        tutorialText.text = "Press Space to jump";
        waitingForJumpInput = true;
        Time.timeScale = 0f;
        
        // Wait until player presses the key
        yield return new WaitUntil(() => !waitingForJumpInput);
        
        // Display for a short time
        yield return new WaitForSeconds(inputDisplayTime);
        
        // Shift tutorial
        tutorialText.text = "Press Left Shift";
        waitingForShiftInput = true;
        Time.timeScale = 0f;
        
        // Wait until player presses the key
        yield return new WaitUntil(() => !waitingForShiftInput);
        yield return new WaitForSecondsRealtime(2f);
        
        // Freeze the scene
        Time.timeScale = 0f;
        
        // Good job message
        tutorialText.text = "Good job! Now try this...";
        yield return new WaitForSecondsRealtime(2f);
        
        // Reveal the game object and let the player play
        if (objectToReveal != null)
            objectToReveal.SetActive(true);
            
        tutorialText.text = "Try to play for a bit...";
        Time.timeScale = 1f;
        
        // Let the player play for the specified time
        yield return new WaitForSeconds(freePlayTime);
        
        // Final message before changing scenes
        Time.timeScale = 0f;
        tutorialText.text = "Let's get this party started!";
        yield return new WaitForSecondsRealtime(2f);
        
        // Change to the main game scene
        tutorialCompleted = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainGameSceneName);
    }
}
