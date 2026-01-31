using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyStats> enemyTypes;
    [SerializeField] private float minSpawnRadius = 5f;
    [SerializeField] private float maxSpawnRadius = 10f;
    private List<EnemyWave> _enemyWaves = new List<EnemyWave>();
    private int _currentWaveIndex = 0;
    private int _enemiesSpawnedInCurrentWave = 0;
    private float _timeSinceLastSpawn = 0f;
    private float _timeSinceWaveStart = 0f;
    private bool _enabled = true;
    private bool _startNextWave = false;
    private bool _alreadySpawnedBoss = false;
    private PlayerController _player;

    public void SpawnEnemy(Enemy enemy)
    {
        if (_player == null) return;

        if (enemy.EnemyStats.IsBoss)
        {   
            if(_alreadySpawnedBoss) 
                return;
            else
                _alreadySpawnedBoss = true;
        }

        Vector3 spawnPosition = _player.transform.position + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(minSpawnRadius, maxSpawnRadius);
        spawnPosition.y = _player.transform.position.y; 

        GameObject enemyInstance = EnemyManager.Instance.GetPooledEnemy(enemy);
        enemyInstance.transform.position = spawnPosition;
        enemyInstance.transform.rotation = Quaternion.identity;
        
        Logger.Log($"Spawned {enemy.name} at {spawnPosition}", LogType.SpawnSystem, this);
    }

    public void SetEnemyWaves(EnemyWave[] waves)
    {
        if (waves == null || waves.Length == 0) return;

        _enemyWaves = new List<EnemyWave>(waves);
        _currentWaveIndex = 0;
        
        _enemiesSpawnedInCurrentWave = 0;
        _timeSinceWaveStart = Time.time;
        _timeSinceLastSpawn = Time.time - _enemyWaves[0].SpawnInterval;
        
        _enabled = true;
        _startNextWave = true;
    }

    void Awake()
    {
        EnemyManager.Instance.RegisterSpawner(this);
    }

    void Start()
    {
        _player = GameManager.Instance?.Player;
    }

    void Update()
    {
        if(!_enabled || _enemyWaves.Count == 0) return;

        if(_startNextWave)
        {
            if (IsWaveStillActive())
            {
                if (CanSpawnNewEnemy())
                {
                    SpawnEnemy(_enemyWaves[_currentWaveIndex].GetRandomEnemyPrefab());
                    _enemiesSpawnedInCurrentWave++;
                    _timeSinceLastSpawn = Time.time;
                }
            }
            else
            {
                Logger.Log($"Wave {_currentWaveIndex + 1} completed, calling PrepareNextWave", LogType.SpawnSystem, this);
                PrepareNextWave();
            }
        }
        else
        {
            Logger.Log("Waiting for wave transition coroutine to complete", LogType.SpawnSystem, this);
        }
    }

    private bool CanSpawnNewEnemy()
    {
        return Time.time - _timeSinceLastSpawn >= _enemyWaves[_currentWaveIndex].SpawnInterval;
    }

    private bool IsWaveStillActive()
    {
        float timeElapsed = Time.time - _timeSinceWaveStart;
        float waveDuration = _enemyWaves[_currentWaveIndex].WaveDuration;
        bool isActive = timeElapsed < waveDuration;
        
        Logger.Log($"Wave {_currentWaveIndex + 1} status: elapsed={timeElapsed:F1}s, duration={waveDuration:F1}s, active={isActive}", LogType.SpawnSystem, this);
        return isActive;
    }

    private void PrepareNextWave()
    {
        Logger.Log($"PrepareNextWave called - current wave: {_currentWaveIndex + 1}, enemies spawned: {_enemiesSpawnedInCurrentWave}", LogType.SpawnSystem, this);
        _startNextWave = false;
        _currentWaveIndex++;

        if(_currentWaveIndex >= _enemyWaves.Count)
        {
            Logger.Log("All waves completed, disabling spawner", LogType.SpawnSystem, this);
            _enabled = false;
            return;
        }

        Logger.Log($"Starting transition to wave {_currentWaveIndex + 1}", LogType.SpawnSystem, this);
        StartCoroutine(HandleWaveTransition());
    }

    private IEnumerator HandleWaveTransition()
    {
        yield return new WaitForSeconds(_enemyWaves[_currentWaveIndex].GracePeriodBeforeWave);
        
        Logger.Log("Starting next enemy wave", LogType.SpawnSystem, this);
        BuffEnemyStats();
        _alreadySpawnedBoss = false;
        _enemiesSpawnedInCurrentWave = 0;
        _timeSinceWaveStart = Time.time;
        _timeSinceLastSpawn = Time.time - _enemyWaves[_currentWaveIndex].SpawnInterval; 
        
        _startNextWave = true;
    }

    private void BuffEnemyStats()
    {
        foreach(EnemyStats stats in enemyTypes)
        {
            stats.BuffStats();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_player.transform.position, minSpawnRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_player.transform.position, maxSpawnRadius);
        }
    }
}
