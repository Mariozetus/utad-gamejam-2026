using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class EnemyWave{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        [Range(0f, 1f)]
        public float spawnChance = 0.5f;
    }

    public List<EnemySpawnData> enemySpawnData = new List<EnemySpawnData>();
    public int EnemiesPerSecond = 1;
    public float WaveDuration = 10f;
    public float GracePeriodBeforeWave = 5f;

    public bool IsBossWave => enemySpawnData.Exists(data => data.enemyPrefab != null && data.enemyPrefab.GetComponent<EnemyStats>() != null && data.enemyPrefab.GetComponent<EnemyStats>().IsBoss);

    public EnemyWave()
    {
        enemySpawnData = new List<EnemySpawnData>();
    }

    public float SpawnInterval
    {
        get
        {
            return EnemiesPerSecond > 0 ? 1f / EnemiesPerSecond : WaveDuration;
        }
    }

    public float GetTotalProbability()
    {
        float total = 0f;
        foreach (var data in enemySpawnData)
        {
            if (data.enemyPrefab != null)
                total += data.spawnChance;
        }
        return total;
    }

    public void NormalizeProbabilities()
    {
        float totalProb = GetTotalProbability();
        if (totalProb > 1f)
        {
            foreach (var data in enemySpawnData)
            {
                if (data.enemyPrefab != null)
                    data.spawnChance = data.spawnChance / totalProb;
            }
        }
    }

    public GameObject GetRandomEnemyPrefab()
    {
        if (enemySpawnData == null || enemySpawnData.Count == 0)
            return null;

        float totalWeight = 0f;
        foreach (var data in enemySpawnData)
        {
            if (data.enemyPrefab != null)
                totalWeight += data.spawnChance;
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var data in enemySpawnData)
        {
            if (data.enemyPrefab != null)
            {
                currentWeight += data.spawnChance;
                if (randomValue <= currentWeight)
                    return data.enemyPrefab;
            }
        }

        foreach (var data in enemySpawnData)
        {
            if (data.enemyPrefab != null)
                return data.enemyPrefab;
        }

        return null;
    }
}
