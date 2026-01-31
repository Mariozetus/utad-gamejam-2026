using System.Collections;
using UnityEngine;
using System;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;
    public Action OnSceneLoaded;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            this.gameObject.transform.parent = null;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void LoadScene(SceneReference scene)
    {
        StartCoroutine(HandleSceneLoading(scene));
    }

    private IEnumerator HandleSceneLoading(SceneReference scene)
    {
        // UIManager.FadeOut(2f);
        yield return new WaitForSeconds(2f);

        AsyncOperation _loadingSceneOperation = scene.LoadSceneAsync();
        
        float currentLoadingPercentage = 0;
        
        while (!_loadingSceneOperation.isDone)
        {
            currentLoadingPercentage = _loadingSceneOperation.progress;
            // UIManager.UpdateLoadingProgressBar(currentLoadingPercentage);
            yield return null;
        }
        // UIManager.FadeIn(2f);
        OnSceneLoaded?.Invoke();
    }


}
