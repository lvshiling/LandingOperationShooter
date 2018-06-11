using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TurboPrefaber))]
public class TurboPrefaberEditor : Editor
{
    private MonoScript script;

    private TurboPrefaber tgt;

    private void OnEnable()
    {
        tgt = ((TurboPrefaber)target);
        script = MonoScript.FromMonoBehaviour(tgt);
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;
        EditorGUI.EndDisabledGroup();

        tgt.storageItem = EditorGUILayout.ObjectField("Storage Item", tgt.storageItem, typeof(TurboPrefaberItem), false) as TurboPrefaberItem;
        tgt.uniqueDontDestroyOnLoad = EditorGUILayout.Toggle("Unique Dont Destroy On Load", tgt.uniqueDontDestroyOnLoad);
        tgt.needProgress = EditorGUILayout.Toggle("Need Progress", tgt.needProgress);
//      tgt.dontTranslateRoot = EditorGUILayout.Toggle("Dont Translate Root", tgt.dontTranslateRoot);

        if (tgt.needProgress)
        {
            SerializedProperty propProgress = serializedObject.FindProperty("OnLoadingProgress");
            EditorGUILayout.PropertyField(propProgress);
        }

        SerializedProperty propFinished = serializedObject.FindProperty("OnLoadingFinished");
        EditorGUILayout.PropertyField(propFinished);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginHorizontal();

        if (tgt.transform.childCount > 0)
        {
            if (GUILayout.Button("Save only", GUILayout.MaxWidth(100)))
            {
                Apply(tgt);
            }

            if (GUILayout.Button("Pack", GUILayout.MaxWidth(100)) && EditorUtility.DisplayDialog("Упаковать?",
                    "Упаковать? Упаковка уничтожает все объекты в иерархии, вернуть их можно распаковкой. Можно использовать \"Save only\" чтобы только сохранить иерархию",
                    "Да, упаковать", "НЕТ"))
            {
                if (Apply(tgt))
                {
                    for (int i = tgt.transform.childCount - 1; i >= 0; i--)
                    {
                        DestroyImmediate(tgt.transform.GetChild(i).gameObject);
                    }
                }
            }
        }

        if (!tgt.storageItem)
        {
            EditorGUILayout.EndHorizontal();
            return;
        }

        if (GUILayout.Button("Unpack", GUILayout.MaxWidth(100)) && (tgt.transform.childCount < 1 || EditorUtility.DisplayDialog("Иерархия не пуста",
                    "Иерархия не пуста. Точно распаковать?",
                    "ДА", "НЕТ")))
        {
            tgt.storageItem.Read(tgt.transform, false, tgt.dontTranslateRoot);
        }

        EditorGUILayout.EndHorizontal();
    }

    private bool Apply(TurboPrefaber tgt)
    {
        var applyPrefabs = EditorUtility.DisplayDialog("Применить все префабы?",
            "Применить все префабы? Этот процесс необратим и может зянять некоторое время",
            "ДА", "НЕТ");
        
        if (!tgt.storageItem)
        {
            tgt.storageItem = ScriptableObjectUtility.CreateAsset<TurboPrefaberItem>("Assets/OtherAssets/TurboPrefaber Configs/", tgt.gameObject.name.Replace(" ", ""));
            EditorUtility.SetDirty(tgt);
        }

        return tgt.storageItem.Write(tgt.transform,applyPrefabs);
    }
}