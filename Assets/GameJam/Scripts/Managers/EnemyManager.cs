using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private Dictionary<GameLevel, EnemyWave[]> _levelWavesDictionary = new Dictionary<GameLevel, EnemyWave[]>();

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


    private void OnSceneLoaded()
    {
        Logger.Log("EnemyManager detected scene load", LogType.SpawnSystem, this);
        GameLevel currentLevel = GameManager.Instance.CurrentLevel;
        if(_levelWavesDictionary.ContainsKey(currentLevel))
        {
            StartCoroutine(HandleGracePeriod(currentLevel.InitialGracePeriod));
        }
    }

    public void RegisterSpawner(EnemySpawner spawner)
    {
        _currentSpawner = spawner;
    }
}
