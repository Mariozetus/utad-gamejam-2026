using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private Dictionary<GameLevel, EnemyWave[]> _levelWavesDictionary = new Dictionary<GameLevel, EnemyWave[]>();
    
    // Enemy pooling and management
    private Dictionary<Enemy, Queue<GameObject>> _enemyPools = new Dictionary<Enemy, Queue<GameObject>>();
    private List<GameObject> _activeEnemies = new List<GameObject>();
    private Transform _poolParent;

    private EnemySpawner _currentSpawner;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            this.gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
            return;
        }

        // Initialize pooling system
        GameObject poolContainer = new GameObject("EnemyPool");
        poolContainer.transform.SetParent(transform);
        _poolParent = poolContainer.transform;

        GameWaves gameWaves = Resources.Load<GameWaves>("GameWaves");

        foreach(LevelWavesDictionary entry in gameWaves.levelWaves)
        {
            if(!_levelWavesDictionary.ContainsKey(entry.key))
            {
                _levelWavesDictionary.Add(entry.key, entry.value);
            }
        }

        OnSceneLoaded();
        SceneController.Instance.OnSceneLoaded += OnSceneLoaded;
    }

    private IEnumerator HandleGracePeriod(float gracePeriodDuration)
    {
        yield return new WaitForSeconds(gracePeriodDuration);
        Logger.Log("Grace period ended, starting enemy waves", LogType.SpawnSystem, this);
        _currentSpawner.SetEnemyWaves(_levelWavesDictionary[GameManager.Instance.CurrentLevel]);
    }

    public void RegisterSpawner(EnemySpawner spawner)
    {
        _currentSpawner = spawner;
    }

    #region Enemy Pooling System
    
    public GameObject GetPooledEnemy(Enemy enemyPrefab)
    {
        if (!_enemyPools.ContainsKey(enemyPrefab))
        {
            _enemyPools[enemyPrefab] = new Queue<GameObject>();
        }

        Queue<GameObject> pool = _enemyPools[enemyPrefab];
        
        if (pool.Count > 0)
        {
            GameObject pooledEnemy = pool.Dequeue();
            pooledEnemy.SetActive(true);
            _activeEnemies.Add(pooledEnemy);
            Logger.Log($"Retrieved {enemyPrefab.name} from pool", LogType.SpawnSystem, this);
            return pooledEnemy;
        }
        else
        {
            GameObject newEnemy = Instantiate(enemyPrefab.gameObject);
            _activeEnemies.Add(newEnemy);
            Logger.Log($"Created new {enemyPrefab.name} instance", LogType.SpawnSystem, this);
            return newEnemy;
        }
    }

    public void ReturnEnemyToPool(GameObject enemy)
    {
        if (enemy == null) return;

        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            if (!_enemyPools.ContainsKey(enemyComponent))
            {
                _enemyPools[enemyComponent] = new Queue<GameObject>();
            }

            enemy.SetActive(false);
            enemy.transform.SetParent(_poolParent);
            _enemyPools[enemyComponent].Enqueue(enemy);
            _activeEnemies.Remove(enemy);
            
            Logger.Log($"Returned {enemyComponent.name} to pool", LogType.SpawnSystem, this);
        }
    }

    #endregion

    #region Enemy Lifecycle Management

    public void KillAllEnemies()
    {
        Logger.Log($"Killing all active enemies ({_activeEnemies.Count})", LogType.SpawnSystem, this);
        
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            if (_activeEnemies[i] != null)
            {
                ReturnEnemyToPool(_activeEnemies[i]);
            }
        }
        
        _activeEnemies.Clear();
    }

    public void KillEnemiesOfType(Enemy enemyType)
    {
        Logger.Log($"Killing all enemies of type {enemyType.name}", LogType.SpawnSystem, this);
        
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            if (_activeEnemies[i] != null)
            {
                Enemy enemyComponent = _activeEnemies[i].GetComponent<Enemy>();
                if (enemyComponent != null && enemyComponent.EnemyStats == enemyType.EnemyStats)
                {
                    ReturnEnemyToPool(_activeEnemies[i]);
                }
            }
        }
    }

    public int GetActiveEnemyCount()
    {
        // Clean up null references
        _activeEnemies.RemoveAll(enemy => enemy == null);
        return _activeEnemies.Count;
    }

    public List<GameObject> GetActiveEnemies()
    {
        // Clean up null references
        _activeEnemies.RemoveAll(enemy => enemy == null);
        return new List<GameObject>(_activeEnemies);
    }

    #endregion

    private void OnSceneLoaded()
    {
        Logger.Log("EnemyManager detected scene load - clearing active enemies", LogType.SpawnSystem, this);
        
        // Clear active enemies list on scene change
        _activeEnemies.Clear();
        
        GameLevel currentLevel = GameManager.Instance.CurrentLevel;
        if(_levelWavesDictionary.ContainsKey(currentLevel))
        {
            StartCoroutine(HandleGracePeriod(currentLevel.InitialGracePeriod));
        }
    }
}
