using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GBNVariables))]
public class GBNVariablesEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GBNVariables.Instance != null && GBNVariables.Instance.variables != null && GBNVariables.Instance.variables.Count > 0)
        {
            GUILayout.Label("Список переменных:");
            int i = 0;
            var guiStyle = new GUIStyle {fontStyle = FontStyle.Bold};
            foreach (var variable in GBNVariables.Instance.variables)
            {
                EditorGUILayout.BeginHorizontal();
                if (i % 2 == 0)
                {
                    EditorGUILayout.LabelField("<color=black>" + variable.Key + ":</color>", GUIStyle.none);
                    EditorGUILayout.LabelField("<color=black>" + variable.Value + "</color>", guiStyle);
                }
                else
                {
                    EditorGUILayout.LabelField("<color=#000066>" + variable.Key + ":</color>", GUIStyle.none);
                    EditorGUILayout.LabelField("<color=#000066>" + variable.Value + "</color>", guiStyle);
                }
                EditorGUILayout.EndHorizontal();
                i++;
            }
        }
        else
        {
            GUILayout.Label("Переменные не загружены");
        }

    }
    
}
