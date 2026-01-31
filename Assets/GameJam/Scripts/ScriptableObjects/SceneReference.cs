using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;

[Serializable]
[CreateAssetMenu(fileName = "SceneReference", menuName = "Custom/Scenes/SceneReference", order = 0)]
public class SceneReference : ScriptableObject
{
        [SerializeField] private SceneAsset sceneAsset;
        [SerializeField, ReadOnly] private string sceneName;
        [SerializeField, ReadOnly] private string scenePath;
        [SerializeField, ReadOnly] private int sceneBuildIndex;
        [SerializeField, ReadOnly] private bool isInBuildSettings;
        

        private void OnValidate()
        {
            if (sceneAsset)
            {
                scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                sceneName = sceneAsset.name;
                sceneBuildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

                isInBuildSettings = sceneBuildIndex >= 0;
            }
            else
            {
                sceneName = "";
                scenePath = "";
                sceneBuildIndex = -1;
                isInBuildSettings = false;
            }
        }

        public void LoadScene(LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!ValidateScene()) return;
            
            // Usar build index si está disponible 
            if (sceneBuildIndex >= 0)
            {
                SceneManager.LoadScene(sceneBuildIndex, mode);
            }
            else
            {
                SceneManager.LoadScene(sceneName, mode);
            }
        }
        
        public AsyncOperation LoadSceneAsync(LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!ValidateScene()) return null;
            
            if (sceneBuildIndex >= 0)
            {
                return SceneManager.LoadSceneAsync(sceneBuildIndex, mode);
            }
            else
            {
                return SceneManager.LoadSceneAsync(sceneName, mode);
            }
        }
        
        private bool ValidateScene()
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"[{name}] Scene reference is empty! Please assign a SceneAsset in the inspector.");
                return false;
            }
            
            if (!isInBuildSettings)
            {
                Debug.LogWarning($"[{name}] Scene '{sceneName}' is not in Build Settings! Add it in File > Build Settings.");
            }
            
            return true;
        }


        public string SceneName => sceneName;
        public string ScenePath => scenePath;
        public int SceneBuildIndex => sceneBuildIndex;
        public bool IsInBuildSettings => isInBuildSettings;
    
}
