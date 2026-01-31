using UnityEngine;


[CreateAssetMenu(fileName = "GameLevel", menuName = "Custom/Scenes/GameLevel", order = 1)]
public class GameLevel : ScriptableObject
{
    [SerializeField] private string levelName;
    [SerializeField] private SceneReference sceneReference;
    [SerializeField] private float startGracePeriod = 30f;

    public SceneReference Scene { get => sceneReference; private set{} }
    public string Name { get => levelName; private set{} }
    public float InitialGracePeriod { get => startGracePeriod; private set{} }
}
