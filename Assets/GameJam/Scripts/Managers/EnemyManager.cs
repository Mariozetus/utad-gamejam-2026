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
    
    // Projectile pooling system
    private Dictionary<GameObject, Queue<GameObject>> _projectilePools = new Dictionary<GameObject, Queue<GameObject>>();
    private List<GameObject> _activeProjectiles = new List<GameObject>();
    private Transform _projectilePoolParent;

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
        
        GameObject projectilePoolContainer = new GameObject("ProjectilePool");
        projectilePoolContainer.transform.SetParent(transform);
        _projectilePoolParent = projectilePoolContainer.transform;

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
    

    public GameObject GetPooledEnemy(GameObject enemyPrefab)
    {
        Enemy enemyComponent = enemyPrefab.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Logger.Error($"Prefab {enemyPrefab.name} does not have an Enemy component!", LogType.SpawnSystem, this);
            return null;
        }

        return GetPooledEnemy(enemyComponent);
    }

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

    #region Projectile Pooling System
    
    public GameObject GetPooledProjectile(GameObject projectilePrefab)
    {
        if (projectilePrefab == null)
        {
            Logger.Error("Projectile prefab is null", LogType.SpawnSystem, this);
            return null;
        }

        if (!_projectilePools.ContainsKey(projectilePrefab))
        {
            _projectilePools[projectilePrefab] = new Queue<GameObject>();
        }

        Queue<GameObject> pool = _projectilePools[projectilePrefab];
        
        if (pool.Count > 0)
        {
            GameObject pooledProjectile = pool.Dequeue();
            pooledProjectile.SetActive(true);
            _activeProjectiles.Add(pooledProjectile);
            Logger.Log($"Retrieved {projectilePrefab.name} projectile from pool", LogType.SpawnSystem, this);
            return pooledProjectile;
        }
        else
        {
            GameObject newProjectile = Instantiate(projectilePrefab);
            _activeProjectiles.Add(newProjectile);
            Logger.Log($"Created new {projectilePrefab.name} projectile instance", LogType.SpawnSystem, this);
            return newProjectile;
        }
    }

    public void ReturnProjectileToPool(GameObject projectile)
    {
        if (projectile == null) return;

        // Find the prefab this projectile came from
        GameObject prefabKey = FindProjectilePrefab(projectile);
        if (prefabKey != null)
        {
            if (!_projectilePools.ContainsKey(prefabKey))
            {
                _projectilePools[prefabKey] = new Queue<GameObject>();
            }

            projectile.SetActive(false);
            projectile.transform.SetParent(_projectilePoolParent);
            _projectilePools[prefabKey].Enqueue(projectile);
            _activeProjectiles.Remove(projectile);
            
            Logger.Log($"Returned {projectile.name} projectile to pool", LogType.SpawnSystem, this);
        }
        else
        {
            // Fallback: destroy if can't find prefab
            _activeProjectiles.Remove(projectile);
            Destroy(projectile);
        }
    }
    
    private GameObject FindProjectilePrefab(GameObject projectileInstance)
    {
        // Try to find which prefab this projectile came from by comparing names
        foreach (var kvp in _projectilePools)
        {
            if (projectileInstance.name.Contains(kvp.Key.name))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    public void ClearAllProjectiles()
    {
        Logger.Log($"Clearing all active projectiles ({_activeProjectiles.Count})", LogType.SpawnSystem, this);
        
        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (_activeProjectiles[i] != null)
            {
                ReturnProjectileToPool(_activeProjectiles[i]);
            }
        }
        
        _activeProjectiles.Clear();
    }

    public int GetActiveProjectileCount()
    {
        _activeProjectiles.RemoveAll(projectile => projectile == null);
        return _activeProjectiles.Count;
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
        Logger.Log("EnemyManager detected scene load - clearing active enemies and projectiles", LogType.SpawnSystem, this);
        
        // Clear active enemies and projectiles list on scene change
        _activeEnemies.Clear();
        _activeProjectiles.Clear();
        
        GameLevel currentLevel = GameManager.Instance.CurrentLevel;
        if(_levelWavesDictionary.ContainsKey(currentLevel))
        {
            StartCoroutine(HandleGracePeriod(currentLevel.InitialGracePeriod));
        }
    }
}
