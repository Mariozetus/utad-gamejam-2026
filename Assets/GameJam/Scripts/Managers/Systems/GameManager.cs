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
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SceneController.Instance.LoadScene(allGameLevels[1].Scene);
        }
    }

    private void OnValidate()
    {
        AddLevelsToDictionary(allGameLevels);
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

    public void ChangeGameLevel(GameLevel gameLevel)
    {
        gameLevelsDictionary[CurrentLevel.Scene.SceneBuildIndex].IsUnlocked = true;
        SceneController.Instance.LoadScene(gameLevel.Scene);
        CurrentLevel = gameLevel;
    }
}
