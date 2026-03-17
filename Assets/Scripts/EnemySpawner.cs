using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private BoxCollider2D spawnZone;

    [Header("Wave Settings")]
    [SerializeField] public float waveInterval = 5f;
    [SerializeField] private int minEnemiesPerWave = 3;
    [SerializeField] private int maxEnemiesPerWave = 5;
    [SerializeField] public float minDistanceFromPlayer = 5f;
    
    private float waveTimer;
    private int waveNumber = 0;

    private void Update()
    {
        if (HasAliveEnemies())
        {
            waveTimer = 0f;
            return;
        }

        waveTimer += Time.deltaTime;

        if (waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            SpawnWave();
        }
    }

    private bool HasAliveEnemies()
    {
        return FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length > 0;
    }

    private void SpawnWave()
    {
        if (enemyPrefab == null || player == null || spawnZone == null)
            return;

        waveNumber++;

        int enemiesToSpawn = Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        Bounds bounds = spawnZone.bounds;

        for (int i = 0; i < 20; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            if (Vector2.Distance(randomPos, player.position) < minDistanceFromPlayer)
                continue;

            GameObject enemyObj = Instantiate(enemyPrefab, randomPos, Quaternion.identity);

            Health enemyHealth = enemyObj.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.AddMaxHealth(waveNumber*3);
            }

            return;
        }
    }
}