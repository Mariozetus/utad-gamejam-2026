using UnityEngine;


[CreateAssetMenu(fileName = "GameLevel", menuName = "Custom/Scenes/GameLevel", order = 1)]
public class GameLevel : ScriptableObject
{
    [SerializeField] private string levelName;
    [SerializeField] private string descrpition;
    [SerializeField] private SceneReference sceneReference;
    [SerializeField] private bool levelIsUnlocked;

    public SceneReference Scene { get => sceneReference; private set{} }
    public string Name { get => levelName; private set{} }
    public string Description { get => descrpition; private set{} }
    public bool IsUnlocked { get => levelIsUnlocked; set{ levelIsUnlocked = value; } }
}
