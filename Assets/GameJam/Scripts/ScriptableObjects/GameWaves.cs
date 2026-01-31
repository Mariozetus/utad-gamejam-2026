using UnityEngine;
using System;

[CreateAssetMenu(fileName = "GameWaves", menuName = "Custom/GameWaves")]
public class GameWaves : ScriptableObject
{
    public LevelWavesDictionary[] levelWaves;
}

[Serializable]
public class LevelWavesDictionary
{
    public GameLevel gameLevel;
    public EnemyWave[] waves;

    public GameLevel key { get => gameLevel; private set{} }
    public EnemyWave[] value { get => waves; private set{} }
}