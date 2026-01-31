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

        if (GUI.changed) 
        {
            EditorUtility.SetDirty(target);
        }
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
                DrawWave(waves.GetArrayElementAtIndex(j), j, waves);
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    private void DrawWave(SerializedProperty wave, int index, SerializedProperty wavesArray)
    {
        EditorGUILayout.BeginVertical("window");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"🌊 OLEADA {index + 1}", EditorStyles.boldLabel);
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            wavesArray.DeleteArrayElementAtIndex(index);
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

        // Lista de Prefabs con Probabilidades
        SerializedProperty enemySpawnData = wave.FindPropertyRelative("enemySpawnData");
        EditorGUILayout.LabelField("Enemigos y Probabilidades", EditorStyles.miniBoldLabel);
        
        if (GUILayout.Button("+ Añadir Enemigo"))
        {
            enemySpawnData.arraySize++;
        }

        for (int k = 0; k < enemySpawnData.arraySize; k++)
        {
            SerializedProperty spawnData = enemySpawnData.GetArrayElementAtIndex(k);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.PropertyField(spawnData.FindPropertyRelative("enemyPrefab"), new GUIContent($"Enemigo {k + 1}"));
            
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                enemySpawnData.DeleteArrayElementAtIndex(k);
                break;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(spawnData.FindPropertyRelative("spawnChance"), new GUIContent("Probabilidad"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        // Show total probability with validation
        float totalProbability = 0f;
        for (int k = 0; k < enemySpawnData.arraySize; k++)
        {
            SerializedProperty spawnData = enemySpawnData.GetArrayElementAtIndex(k);
            totalProbability += spawnData.FindPropertyRelative("spawnChance").floatValue;
        }
        
        // Visual feedback for probability validation
        Color originalColor = GUI.color;
        if (totalProbability > 1.0f)
        {
            GUI.color = Color.red;
            EditorGUILayout.HelpBox($"⚠️ Las probabilidades suman {totalProbability:F2} (más de 1.0). Considera normalizar.", MessageType.Warning);
        }
        else if (totalProbability < 0.1f)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.HelpBox($"⚠️ Las probabilidades suman {totalProbability:F2} (muy bajo).", MessageType.Info);
        }
        else
        {
            GUI.color = Color.green;
        }
        
        GUI.enabled = false;
        EditorGUILayout.FloatField("Probabilidad Total", totalProbability);
        GUI.enabled = true;
        GUI.color = originalColor;
        
        // Normalization button
        if (totalProbability > 1.0f)
        {
            if (GUILayout.Button("🔧 Normalizar Probabilidades (Ajustar a 1.0)"))
            {
                // Normalize probabilities
                for (int k = 0; k < enemySpawnData.arraySize; k++)
                {
                    SerializedProperty spawnData = enemySpawnData.GetArrayElementAtIndex(k);
                    SerializedProperty chanceProperty = spawnData.FindPropertyRelative("spawnChance");
                    chanceProperty.floatValue = chanceProperty.floatValue / totalProbability;
                }
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}