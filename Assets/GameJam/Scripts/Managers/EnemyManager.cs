using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    [SerializeField] private List<LevelWavesDictionary> levelWaves;
    private Dictionary<GameLevel, EnemyWave[]> _levelWavesDictionary = new Dictionary<GameLevel, EnemyWave[]>();

    private Coroutine _gracePeriodCoroutine;
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

        foreach(LevelWavesDictionary entry in levelWaves)
        {
            if(!_levelWavesDictionary.ContainsKey(entry.key))
            {
                _levelWavesDictionary.Add(entry.key, entry.value);
            }
        }

        SceneController.Instance.OnSceneLoaded += OnSceneLoaded;
    }

    private IEnumerator HandleGracePeriod(float gracePeriodDuration)
    {
        yield return new WaitForSeconds(gracePeriodDuration);
        _gracePeriodCoroutine = null;
        _currentSpawner.SetEnemyWaves(_levelWavesDictionary[GameManager.Instance.CurrentLevel]);
    }


    private void OnSceneLoaded()
    {
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

[Serializable]
public class LevelWavesDictionary
{
    public GameLevel gameLevel;
    public EnemyWave[] waves;

    public GameLevel key { get => gameLevel; private set{} }
    public EnemyWave[] value { get => waves; private set{} }
}
