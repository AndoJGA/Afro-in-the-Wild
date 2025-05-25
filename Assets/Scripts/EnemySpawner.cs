using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject enemyPrefab;
        public float spawnWeight = 1f;
    }

    [Header("Spawn Settings")]
    [SerializeField] private EnemyType[] enemyTypes;
    [SerializeField] private Transform spawnAreaCenter;
    [SerializeField] private string enemyTag = "Enemy";
    
    [Header("Spawn Area")]
    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private float spawnAreaWidth = 20f;
    [SerializeField] private float spawnAreaDepth = 20f;
    
    [Header("Progression Settings")]
    [SerializeField] private int startMaxEnemies = 25;
    [SerializeField] private int endMaxEnemies = 100;
    [SerializeField] private int startMinEnemiesThreshold = 15;
    [SerializeField] private int endMinEnemiesThreshold = 80;
    [SerializeField] private float startSpawnDelay = 1f;
    [SerializeField] private float endSpawnDelay = 0.2f;
    [SerializeField] private float timeIncrementInterval = 5f; // Increment every 5 seconds
    [SerializeField] private TextMeshProUGUI progressionDisplayText;
    
    // Current dynamic values
    private int maxEnemies;
    private int minEnemiesThreshold;
    private float spawnDelay;
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float totalSpawnWeight;
    private bool isSpawning = false;
    private int gameTimeInSeconds = 1; // Start at 1 second
    private float timeSinceLastIncrement = 0f;

    private void Start()
    {
        if (spawnAreaCenter == null)
            spawnAreaCenter = transform;
            
        // Initialize with starting values
        maxEnemies = startMaxEnemies;
        minEnemiesThreshold = startMinEnemiesThreshold;
        spawnDelay = startSpawnDelay;
        
        // Calculate total spawn weights
        CalculateTotalSpawnWeight();
        
        // Initial spawn to max
        StartCoroutine(SpawnEnemiesRoutine());
        
        // Initialize display
        UpdateProgressionDisplay();
    }

    private void Update()
    {
        // Update time increment
        timeSinceLastIncrement += Time.deltaTime;
        
        // Check if it's time to increment the game time
        if (timeSinceLastIncrement >= timeIncrementInterval)
        {
            gameTimeInSeconds++;
            timeSinceLastIncrement = 0f;
            UpdateProgressionDisplay();
        }
        
        // Calculate max progression value for normalization
        int maxGameTime = 300; // 5 minutes (adjust as needed)
        
        // Calculate progression (0 to 1)
        float progressionRatio = Mathf.Clamp01((float)gameTimeInSeconds / maxGameTime);
        
        // Update dynamic values based on progression
        maxEnemies = Mathf.RoundToInt(Mathf.Lerp(startMaxEnemies, endMaxEnemies, progressionRatio));
        minEnemiesThreshold = Mathf.RoundToInt(Mathf.Lerp(startMinEnemiesThreshold, endMinEnemiesThreshold, progressionRatio));
        spawnDelay = Mathf.Lerp(startSpawnDelay, endSpawnDelay, progressionRatio);
        
        // Clean up list of any destroyed enemies
        CleanupDestroyedEnemies();
        
        // Check if we need to spawn more enemies
        if (activeEnemies.Count <= minEnemiesThreshold && !isSpawning)
        {
            StartCoroutine(SpawnEnemiesRoutine());
        }
    }

    private void UpdateProgressionDisplay()
    {
        if (progressionDisplayText != null)
        {
            int minutes = gameTimeInSeconds / 60;
            int seconds = gameTimeInSeconds % 60;
            
            // Calculate max progression value for normalization
            int maxGameTime = 300; // 5 minutes (adjust as needed)
            float progressionRatio = Mathf.Clamp01((float)gameTimeInSeconds / maxGameTime);
            
            string progressText = string.Format(
                "Score: " + seconds
            );
            
            progressionDisplayText.text = progressText;
        }
    }

    private void CalculateTotalSpawnWeight()
    {
        totalSpawnWeight = 0f;
        foreach (EnemyType enemyType in enemyTypes)
        {
            totalSpawnWeight += enemyType.spawnWeight;
        }
    }

    private void CleanupDestroyedEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        isSpawning = true;
        while (activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        if (enemyTypes.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned to spawner!");
            return;
        }
        
        // Select random position within spawn area
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Select random enemy type based on weights
        GameObject enemyPrefab = SelectRandomEnemyType();
        
        // Spawn the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Add to active enemies list
        activeEnemies.Add(enemy);
        
        // Ensure it has the correct tag for tracking
        enemy.tag = enemyTag;
        
        // Update display after spawning
        UpdateProgressionDisplay();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        float randomZ = Random.Range(-spawnAreaDepth / 2, spawnAreaDepth / 2);
        
        return spawnAreaCenter.position + new Vector3(randomX, spawnHeight, randomZ);
    }

    private GameObject SelectRandomEnemyType()
    {
        float randomValue = Random.Range(0, totalSpawnWeight);
        float weightSum = 0;
        
        foreach (EnemyType enemyType in enemyTypes)
        {
            weightSum += enemyType.spawnWeight;
            if (randomValue <= weightSum)
            {
                return enemyType.enemyPrefab;
            }
        }
        
        // Fallback to first enemy type (should never reach here if weights are positive)
        return enemyTypes[0].enemyPrefab;
    }

    // For debugging - visualize spawn area
    private void OnDrawGizmosSelected()
    {
        Transform center = spawnAreaCenter != null ? spawnAreaCenter : transform;
        
        // Draw spawn area
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(center.position + new Vector3(0, spawnHeight/2, 0), 
                         new Vector3(spawnAreaWidth, spawnHeight, spawnAreaDepth));
        
        // Draw spawn plane
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawCube(center.position + new Vector3(0, spawnHeight, 0), 
                         new Vector3(spawnAreaWidth, 0.1f, spawnAreaDepth));
    }

    public void NotifyEnemyDestroyed(GameObject enemy)
    {
        // Remove from active enemies list if it exists
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            
            // Update display when an enemy is destroyed
            UpdateProgressionDisplay();
        }
    }
}
