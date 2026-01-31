using UnityEngine;

/// <summary>
/// Carga los sistemas necesarios antes de que se cargue la escena.
/// </summary>
public static class Bootstraper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Execute(){
        Logger.Log("Bootstraper: Cargando sistemas...", LogType.System, null, false);
        Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Systems")));
    }
}