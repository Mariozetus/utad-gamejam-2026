using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public struct EnemyWave
{
    public List<GameObject> EnemyPrefabs;
    public int EnemiesPerSecond;
    public float WaveDuration;
    public float GracePeriodBeforeWave;

    public float SpawnInterval
    {
        get
        {
            return EnemiesPerSecond > 0 ? 1f / EnemiesPerSecond : WaveDuration;
        }
    }

    public GameObject GetRandomEnemyPrefab()
    {
        if (EnemyPrefabs == null || EnemyPrefabs.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, EnemyPrefabs.Count);
        return EnemyPrefabs[randomIndex];
    }
}
