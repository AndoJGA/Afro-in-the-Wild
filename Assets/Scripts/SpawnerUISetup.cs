using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class SpawnerUISetup : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private TextMeshProUGUI progressionText;
    
    [ContextMenu("Setup UI")]
    public void SetupUI()
    {
        if (spawner == null)
        {
            spawner = FindObjectOfType<EnemySpawner>();
            if (spawner == null)
            {
                Debug.LogError("No EnemySpawner found in the scene!");
                return;
            }
        }
        
        // Create Canvas if needed
        if (uiCanvas == null)
        {
            GameObject canvasObj = new GameObject("SpawnerUI Canvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create TextMeshPro if needed
        if (progressionText == null)
        {
            GameObject textObj = new GameObject("Progression Text");
            textObj.transform.SetParent(uiCanvas.transform, false);
            progressionText = textObj.AddComponent<TextMeshProUGUI>();
            
            // Set default properties
            progressionText.fontSize = 24;
            progressionText.color = Color.white;
            progressionText.alignment = TextAlignmentOptions.TopLeft;
            progressionText.rectTransform.anchorMin = new Vector2(0, 1);
            progressionText.rectTransform.anchorMax = new Vector2(0, 1);
            progressionText.rectTransform.pivot = new Vector2(0, 1);
            progressionText.rectTransform.anchoredPosition = new Vector2(20, -20);
            progressionText.rectTransform.sizeDelta = new Vector2(300, 200);
            
            // Add background for better readability
            GameObject bgObj = new GameObject("Text Background");
            bgObj.transform.SetParent(textObj.transform, false);
            bgObj.transform.SetAsFirstSibling();
            UnityEngine.UI.Image bg = bgObj.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0, 0, 0, 0.5f);
            bg.rectTransform.anchorMin = Vector2.zero;
            bg.rectTransform.anchorMax = Vector2.one;
            bg.rectTransform.sizeDelta = new Vector2(20, 20);
        }
        
        // Assign the text to the spawner
        //SerializedObject serializedSpawner = new UnityEditor.SerializedObject(spawner);
        //SerializedProperty textProperty = serializedSpawner.FindProperty("progressionDisplayText");
        //textProperty.objectReferenceValue = progressionText;
        //serializedSpawner.ApplyModifiedProperties();
        
        Debug.Log("UI Setup complete!");
    }
}
