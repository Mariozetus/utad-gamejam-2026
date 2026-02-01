using UnityEngine;


[CreateAssetMenu(fileName = "GameLevel", menuName = "Custom/Scenes/GameLevel", order = 1)]
public class GameLevel : ScriptableObject
{
    [SerializeField] private string levelName;
    [SerializeField] private SceneReference sceneReference;
    [SerializeField] private float startGracePeriod = 30f;
    [SerializeField] private MusicTrack levelMusic;
    [Tooltip("Fallback music name for backwards compatibility")]
    [SerializeField] private string legacyMusicName;

    public SceneReference Scene { get => sceneReference; private set{} }
    public string Name { get => levelName; private set{} }
    public float InitialGracePeriod { get => startGracePeriod; private set{} }
    public MusicTrack LevelMusic { get => levelMusic; private set{} }
    public string LegacyMusicName { get => legacyMusicName; private set{} }
}
