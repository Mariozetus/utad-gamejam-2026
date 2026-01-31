using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnPoints;
    private List<EnemyWave> _enemyWaves = new List<EnemyWave>();
    private int _currentWaveIndex = 0;
    private int _enemiesSpawnedInCurrentWave = 0;
    private float _timeSinceLastSpawn = 0f;
    private float _timeSinceWaveStart = 0f;
    private bool _enabled = true;
    private bool _startNextWave = false;

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
        GameObject spawnPoint = spawnPoints[randomIndex];

        Instantiate(enemyPrefab, spawnPoint.transform.position, Quaternion.identity);
    }

    public void SetEnemyWaves(EnemyWave[] waves)
    {
        _enemyWaves = new List<EnemyWave>(waves);
        _currentWaveIndex = 0;
        _enemiesSpawnedInCurrentWave = 0;
        _timeSinceLastSpawn = Time.time;
        _timeSinceWaveStart = Time.time;
        _enabled = true;
        _startNextWave = true;
    }

    void Awake()
    {
        EnemyManager.Instance.RegisterSpawner(this);
    }

    void Update()
    {
        if(!_enabled) return;

        if(_startNextWave && Time.time - _timeSinceLastSpawn >= _enemyWaves[_currentWaveIndex].SpawnInterval)
        {
            if(_timeSinceWaveStart < _enemyWaves[_currentWaveIndex].WaveDuration)
            {
                SpawnEnemy(_enemyWaves[_currentWaveIndex].GetRandomEnemyPrefab());
                _enemiesSpawnedInCurrentWave++;
                _timeSinceLastSpawn = Time.time;
            }
            else
            {
                _currentWaveIndex++;
                _enemiesSpawnedInCurrentWave = 0;
                _timeSinceWaveStart = Time.time;
                _startNextWave = false;
                
                if(_currentWaveIndex >= _enemyWaves.Count)
                {
                    _enabled = false;
                    return;
                }

                StartCoroutine(HandleWaveTransition());
            }
        }
    }

    private IEnumerator HandleWaveTransition()
    {
        yield return new WaitForSeconds(_enemyWaves[_currentWaveIndex].GracePeriodBeforeWave);
        _startNextWave = true;
    }
}
