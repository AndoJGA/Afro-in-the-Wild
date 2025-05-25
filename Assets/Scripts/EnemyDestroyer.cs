using UnityEngine;

public class EnemyDestroyer : MonoBehaviour
{
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private bool notifySpawner = true;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private float destroyDelay = 0.1f;
    [SerializeField] private GameObject destroyEffectPrefab;
    [SerializeField] private bool useOnTrigger = true;
    [SerializeField] private bool useOnCollision = false;

    private void Start()
    {
        // Auto-find spawner if not assigned and notification is enabled
        if (notifySpawner && spawner == null)
        {
            spawner = FindObjectOfType<EnemySpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("EnemyDestroyer: No EnemySpawner found in scene but notifySpawner is enabled!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (useOnTrigger && other.CompareTag(enemyTag))
        {
            DestroyEnemy(other.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useOnTrigger && other.CompareTag(enemyTag))
        {
            DestroyEnemy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useOnCollision && collision.gameObject.CompareTag(enemyTag))
        {
            DestroyEnemy(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useOnCollision && collision.gameObject.CompareTag(enemyTag))
        {
            DestroyEnemy(collision.gameObject);
        }
    }

    private void DestroyEnemy(GameObject enemy)
    {
        // Notify spawner before destroying
        if (notifySpawner && spawner != null)
        {
            spawner.NotifyEnemyDestroyed(enemy);
        }

        // Spawn destroy effect if assigned
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, enemy.transform.position, Quaternion.identity);
        }

        // Destroy the enemy
        if (destroyDelay > 0)
        {
            // Disable components to prevent further interaction
            DisableEnemyComponents(enemy);
            Destroy(enemy, destroyDelay);
        }
        else
        {
            Destroy(enemy);
        }
    }

    private void DisableEnemyComponents(GameObject enemy)
    {
        // Disable colliders
        Collider[] colliders = enemy.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
        
        Collider2D[] colliders2D = enemy.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders2D)
        {
            collider.enabled = false;
        }

        // Disable rigidbody
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        Rigidbody2D rb2D = enemy.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            rb2D.simulated = false;
        }
    }
}
