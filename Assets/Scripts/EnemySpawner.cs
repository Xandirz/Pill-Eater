using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameObject> enemyPrefabs = new();
    [SerializeField] private Transform player;
    [SerializeField] private BoxCollider2D spawnZone;

    [Header("Wave Settings")]
    [SerializeField] public float waveInterval = 5f;
    [SerializeField] public float minDistanceFromPlayer = 5f;

    private float waveTimer;
    private int waveNumber = 0;
    private bool specialPillSpawnedForThisWaveEnd = false;

    private void Update()
    {
        if (HasAliveEnemies())
        {
            specialPillSpawnedForThisWaveEnd = false;
            waveTimer = 0f;
            return;
        }

        if (!specialPillSpawnedForThisWaveEnd && waveNumber > 0)
        {
            SpawnSpecialPillInCenter();
            specialPillSpawnedForThisWaveEnd = true;
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
        if (enemyPrefabs == null || enemyPrefabs.Count == 0 || player == null || spawnZone == null)
            return;

        waveNumber++;

        int stage = GetWaveStage(waveNumber);
        int unlockedEnemyTypes = Mathf.Min(stage, enemyPrefabs.Count);

        for (int enemyTypeIndex = 0; enemyTypeIndex < unlockedEnemyTypes; enemyTypeIndex++)
        {
            GameObject enemyPrefab = enemyPrefabs[enemyTypeIndex];
            if (enemyPrefab == null)
                continue;

            Vector2Int spawnRange = GetSpawnCountRange(stage, enemyTypeIndex);
            int enemiesToSpawn = Random.Range(spawnRange.x, spawnRange.y + 1);

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                TrySpawnEnemy(enemyPrefab);
            }
        }
    }

    private int GetWaveStage(int currentWave)
    {
        // 1-5 = stage 1
        // 6-10 = stage 2
        // 11-15 = stage 3
        // 16-20 = stage 4
        return ((currentWave - 1) / 5) + 1;
    }

    private Vector2Int GetSpawnCountRange(int stage, int enemyTypeIndex)
    {
        // enemyTypeIndex:
        // 0 = первый враг
        // 1 = второй враг
        // 2 = третий враг
        // 3 = четвертый враг
        //
        // Прогрессия:
        // stage 1: enemy 1 -> 3-5
        // stage 2: enemy 1 -> 4-6, enemy 2 -> 1-2
        // stage 3: enemy 1 -> 5-7, enemy 2 -> 2-3, enemy 3 -> 1-2
        // stage 4: enemy 1 -> 6-8, enemy 2 -> 3-4, enemy 3 -> 2-3, enemy 4 -> 1-2

        if (enemyTypeIndex == 0)
        {
            int min = 3 + (stage - 1);
            int max = 5 + (stage - 1);
            return new Vector2Int(min, max);
        }
        else
        {
            int min = stage - enemyTypeIndex;
            int max = min + 1;

            min = Mathf.Max(1, min);
            max = Mathf.Max(min, max);

            return new Vector2Int(min, max);
        }
    }

    private void SpawnSpecialPillInCenter()
    {
        if (PillManager.Instance == null)
            return;

        Vector3 centerPosition = spawnZone != null ? spawnZone.bounds.center : Vector3.zero;
        centerPosition.z = 0f;

        PillManager.Instance.SpawnSpecialPill(centerPosition);
    }

    private void TrySpawnEnemy(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null)
            return;

        Bounds bounds = spawnZone.bounds;

        for (int i = 0; i < 20; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            if (Vector2.Distance(randomPos, player.position) < minDistanceFromPlayer)
                continue;

            GameObject enemyObj = Instantiate(prefabToSpawn, randomPos, Quaternion.identity);

            Health enemyHealth = enemyObj.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.AddMaxHealth(waveNumber * 3);
                enemyHealth.FullHeal();
            }

            return;
        }
    }
}