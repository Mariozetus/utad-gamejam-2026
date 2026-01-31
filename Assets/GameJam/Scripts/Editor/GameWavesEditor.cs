using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(GameWaves))]
public class GameWavesEditor : Editor
{
    private SerializedProperty levelWavesProp;

    private void OnEnable()
    {
        levelWavesProp = serializedObject.FindProperty("levelWaves");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configuración de Oleadas por Nivel", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("➕ Añadir Nuevo Nivel", GUILayout.Height(30)))
        {
            levelWavesProp.arraySize++;
        }

        EditorGUILayout.Space();

        for (int i = 0; i < levelWavesProp.arraySize; i++)
        {
            SerializedProperty levelEntry = levelWavesProp.GetArrayElementAtIndex(i);
            DrawLevelEntry(levelEntry, i);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLevelEntry(SerializedProperty levelEntry, int index)
    {
        SerializedProperty gameLevel = levelEntry.FindPropertyRelative("gameLevel");
        SerializedProperty waves = levelEntry.FindPropertyRelative("waves");

        string levelName = gameLevel.objectReferenceValue != null ? gameLevel.objectReferenceValue.name : "Nivel sin asignar";

        GUI.backgroundColor = new Color(0.8f, 0.8f, 1f);
        EditorGUILayout.BeginVertical("helpBox");
        GUI.backgroundColor = Color.white;

        // Cabecera del Nivel
        EditorGUILayout.BeginHorizontal();
        gameLevel.isExpanded = EditorGUILayout.Foldout(gameLevel.isExpanded, $" NIVEL: {levelName}", true, EditorStyles.foldoutHeader);
        
        GUI.color = Color.red;
        if (GUILayout.Button("Eliminar Nivel", GUILayout.Width(100)))
        {
            levelWavesProp.DeleteArrayElementAtIndex(index);
            return;
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        if (gameLevel.isExpanded)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(gameLevel, new GUIContent("ScriptableObject del Nivel"));
            EditorGUILayout.Space(5);

            // Sección de Oleadas
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Oleadas de {levelName}", EditorStyles.miniBoldLabel);
            
            if (GUILayout.Button("+ Añadir Oleada"))
            {
                waves.arraySize++;
            }

            for (int j = 0; j < waves.arraySize; j++)
            {
                DrawWave(waves.GetArrayElementAtIndex(j), j);
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    private void DrawWave(SerializedProperty wave, int index)
    {
        EditorGUILayout.BeginVertical("window");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"🌊 OLEADA {index + 1}", EditorStyles.boldLabel);
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            wave.DeleteCommand(); // Esto maneja mejor la eliminación en arrays de structs
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        // Campos de tiempo
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(wave.FindPropertyRelative("EnemiesPerSecond"), new GUIContent("Enemigos/Seg"));
        EditorGUILayout.PropertyField(wave.FindPropertyRelative("WaveDuration"), new GUIContent("Duración (s)"));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(wave.FindPropertyRelative("GracePeriodBeforeWave"), new GUIContent("Gracia antes (s)"));

        // Mostrar el intervalo calculado (Solo lectura)
        int eps = wave.FindPropertyRelative("EnemiesPerSecond").intValue;
        float interval = eps > 0 ? 1f / eps : 0;
        GUI.enabled = false;
        EditorGUILayout.FloatField("Intervalo calculado (s)", interval);
        GUI.enabled = true;

        EditorGUILayout.Space(5);

        // Lista de Prefabs
        SerializedProperty prefabs = wave.FindPropertyRelative("EnemyPrefabs");
        EditorGUILayout.PropertyField(prefabs, new GUIContent("Prefabs de Enemigos"), true);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}