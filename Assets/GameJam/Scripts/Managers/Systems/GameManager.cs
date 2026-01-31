
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] GameLevel[] allGameLevels;
    private Dictionary<int, GameLevel> gameLevelsDictionary = new Dictionary<int, GameLevel>();
    public GameLevel CurrentLevel;
    public int UnlockedLevels;

    private void Awake()
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

        AddLevelsToDictionary(allGameLevels);
        CheckActualLevel();
    }

    void OnEnable()
    {
        SceneController.Instance.OnSceneLoaded += CheckActualLevel;
    }

    void OnDisable()
    {
        SceneController.Instance.OnSceneLoaded -= CheckActualLevel;
    }

    private void OnValidate()
    {
        AddLevelsToDictionary(allGameLevels);
    }

    private void CheckActualLevel()
    {
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if(gameLevelsDictionary.ContainsKey(buildIndex))
        {
            CurrentLevel = gameLevelsDictionary[buildIndex];
        }
    }

    public void ChangeGameLevel(GameLevel gameLevel)
    {
        AudioManager.Instance.StopAllSounds();
        SceneController.Instance.LoadScene(gameLevel.Scene);
        CurrentLevel = gameLevel;
        AudioManager.Instance.PlayMusic(gameLevel.LevelMusic);
    }

    private void AddLevelsToDictionary(GameLevel[] gameLevels)
    {
        foreach(GameLevel level in gameLevels)
        {
            if (!gameLevelsDictionary.ContainsKey(level.Scene.SceneBuildIndex))
            {
                gameLevelsDictionary.Add(level.Scene.SceneBuildIndex, level);
            }
        }
    }

    public void HandlePlayerDeath()
    {
        throw new System.NotImplementedException();
    }
}
