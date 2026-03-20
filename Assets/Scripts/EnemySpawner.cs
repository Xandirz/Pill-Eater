using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameObject> enemyPrefabs = new();
    [SerializeField] private Transform player;
    [SerializeField] private BoxCollider2D spawnZone;
    [SerializeField] private TMP_Text waveText;

    [Header("Wave Settings")]
    [SerializeField] public float waveInterval = 5f;
    [SerializeField] public float minDistanceFromPlayer = 5f;
    [SerializeField] private GameObject bossPrefab;
    private float waveTimer;
    private int waveNumber = 0;
    private bool specialPillSpawnedForThisWaveEnd = false;
    [SerializeField] private float healthGrowthPower = 1.35f;
    [SerializeField] private float healthGrowthMultiplier = 2f;
    private void Start()
    {
        UpdateWaveText();
    }

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
        UpdateWaveText();

        if (IsBossWave(waveNumber))
        {
            TrySpawnBoss();
            return;
        }

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
    private bool IsBossWave(int currentWave)
    {
        return currentWave > 20 && currentWave % 10 == 0;
    }
    
    private void UpdateWaveText()
    {
        if (waveText == null)
            return;

        waveText.text = $"Wave: {waveNumber}";
    }

    private int GetWaveStage(int currentWave)
    {
        // 1-5 = stage 1
        // 6-10 = stage 2
        // 11-15 = stage 3
        // 16-20 = stage 4
        // 21-25 = stage 5
        return ((currentWave - 1) / 5) + 1;
    }
    private int GetBossIndex(int currentWave)
    {
        if (currentWave <= 20 || currentWave % 10 != 0)
            return 0;

        return (currentWave - 20) / 10;
    }
    private void TrySpawnBoss()
    {
        if (bossPrefab == null)
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

            GameObject bossObj = Instantiate(bossPrefab, randomPos, Quaternion.identity);

            Health bossHealth = bossObj.GetComponent<Health>();
            if (bossHealth != null)
            {
                int bossTargetHealth = 1000000 * GetBossIndex(waveNumber);
                int healthToAdd = bossTargetHealth - bossHealth.MaxHealth;

                if (healthToAdd > 0)
                    bossHealth.AddMaxHealth(healthToAdd);

                bossHealth.FullHeal();
            }

            return;
        }
    }
    private Vector2Int GetSpawnCountRange(int stage, int enemyTypeIndex)
    {
        // enemyTypeIndex:
        // 0 = первый враг
        // 1 = второй враг
        // 2 = третий враг
        // 3 = четвертый враг
        // 4 = пятый враг
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
                int healthBonus = Mathf.RoundToInt(Mathf.Pow(waveNumber, healthGrowthPower) * healthGrowthMultiplier);
                enemyHealth.AddMaxHealth(healthBonus);
                enemyHealth.FullHeal();
            }

            return;
        }
    }
}