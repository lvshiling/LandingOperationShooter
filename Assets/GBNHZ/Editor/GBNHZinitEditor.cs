using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(GBNHZinit))]
public class GBNHZinitEditor : Editor
{
    SerializedProperty adsFieldsProp;
    SerializedProperty scenesIntervalsProp;
    SerializedProperty eventsWhitelistProp;
    SerializedProperty rewardBlacklistProp;

    Vector2 scrollPos;
    GUIStyle textAllignmentStyle = new GUIStyle();
    Color tableColor = new Color(0.8f, 0.7f, 0.7f, 1f);
    
    private void OnEnable()
    {
        GBNEventManager.AddEventListener(GBNEvent.CLOUD_LOADED, Refresh);
        Refresh();
    }

    private void OnDisable()
    {
        GBNEventManager.RemoveEventListener(GBNEvent.CLOUD_LOADED, Refresh);
    }

    private void Refresh()
    {
        textAllignmentStyle.alignment = TextAnchor.MiddleCenter;
        adsFieldsProp = serializedObject.FindProperty("adFields");
        scenesIntervalsProp = serializedObject.FindProperty("sceneIndexesWithInterval");

        eventsWhitelistProp = serializedObject.FindProperty("eventsWhitelist");
        rewardBlacklistProp = serializedObject.FindProperty("rewardBlacklist");

        if (adsFieldsProp.arraySize < 1)
        {
            adsFieldsProp.InsertArrayElementAtIndex(adsFieldsProp.arraySize);
        }

        if (eventsWhitelistProp.arraySize < 1)
        {
            eventsWhitelistProp.InsertArrayElementAtIndex(eventsWhitelistProp.arraySize);
        }

        if (rewardBlacklistProp.arraySize < 1)
        {
            rewardBlacklistProp.InsertArrayElementAtIndex(rewardBlacklistProp.arraySize);
        }
    }

    public override void OnInspectorGUI()
    {
        Handles.color = Color.black;
        float tableWidth = (Screen.width - 100f) / 4;
        float tableHeight = adsFieldsProp.arraySize * 20;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("globalManagerPrefab"));
        
        serializedObject.FindProperty("useCloudAds").boolValue = EditorGUILayout.BeginToggleGroup("Загружать рекламу из облака (!)", serializedObject.FindProperty("useCloudAds").boolValue);
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudAdsId"));
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("Настройки событий рекламы." + "\n" + "Rate - частота показов" + "\n" +
          "Delay - задержка перед показом." + "\n" + "LastAdDelay - минимальное время с последнего показа рекламы", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("#", GUILayout.Width(20f));
        EditorGUILayout.LabelField("Name", GUILayout.Width(tableWidth));
        EditorGUILayout.LabelField("Rate", GUILayout.Width(tableWidth));
        EditorGUILayout.LabelField("Delay", GUILayout.Width(tableWidth));
        EditorGUILayout.LabelField("LastAdDelay", GUILayout.Width(tableWidth));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < adsFieldsProp.arraySize; i++)
        {
            if (i % 2 == 0)
            {
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = tableColor;
            }
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(i.ToString() + ")", textAllignmentStyle, GUILayout.Width(20f), GUILayout.Height(tableHeight / adsFieldsProp.arraySize));
            EditorGUILayout.PropertyField(adsFieldsProp.GetArrayElementAtIndex(i).FindPropertyRelative("name"), GUIContent.none, GUILayout.Width(tableWidth), GUILayout.Height(tableHeight / adsFieldsProp.arraySize));
            EditorGUILayout.PropertyField(adsFieldsProp.GetArrayElementAtIndex(i).FindPropertyRelative("rate"), GUIContent.none, GUILayout.Width(tableWidth), GUILayout.Height(tableHeight / adsFieldsProp.arraySize));
            EditorGUILayout.PropertyField(adsFieldsProp.GetArrayElementAtIndex(i).FindPropertyRelative("delay"), GUIContent.none, GUILayout.Width(tableWidth), GUILayout.Height(tableHeight / adsFieldsProp.arraySize));
            EditorGUILayout.PropertyField(adsFieldsProp.GetArrayElementAtIndex(i).FindPropertyRelative("delayBetweenLastAd"), GUIContent.none, GUILayout.Width(tableWidth), GUILayout.Height(tableHeight / adsFieldsProp.arraySize));
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(tableHeight / adsFieldsProp.arraySize)))
            {
                adsFieldsProp.DeleteArrayElementAtIndex(i);
            };

            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", GUILayout.Width(30f), GUILayout.Height(20f)))
        {
            adsFieldsProp.InsertArrayElementAtIndex(adsFieldsProp.arraySize);
        };
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Сохранение баланса в локальный файл.\nНужно нажать в режиме Play после загрузки всех данных из облака", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Сохранить баланс");
        if (GUILayout.Button("Сохранить"))
        {
            GBNCloud.WriteAllToFile();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        float wListTableWidth = (Screen.width - 100f);
        float wListTableHeight = eventsWhitelistProp.arraySize * 20;

        EditorGUILayout.HelpBox("Белый список событий рекламы. Реклама по этим событиям НЕ БУДЕТ отключена, при отключении галки \"ads\" в CoolTool.", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("#", GUILayout.Width(20f));
        EditorGUILayout.LabelField("Name", GUILayout.Width(wListTableWidth));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < eventsWhitelistProp.arraySize; i++)
        {
            if (i % 2 == 0)
            {
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = tableColor;
            }
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(i.ToString() + ")", textAllignmentStyle, GUILayout.Width(20f), GUILayout.Height(wListTableHeight / eventsWhitelistProp.arraySize));
            EditorGUILayout.PropertyField(eventsWhitelistProp.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.Width(wListTableWidth), GUILayout.Height(wListTableHeight / eventsWhitelistProp.arraySize));
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(wListTableHeight / eventsWhitelistProp.arraySize)))
            {
                eventsWhitelistProp.DeleteArrayElementAtIndex(i);
            };

            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", GUILayout.Width(30f), GUILayout.Height(20f)))
        {
            eventsWhitelistProp.InsertArrayElementAtIndex(eventsWhitelistProp.arraySize);
        };
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        wListTableHeight = rewardBlacklistProp.arraySize * 20;

        EditorGUILayout.HelpBox("Черный список ревардов. Реварды с этими именами БУДУТ отключены, при отключении галки \"ads\" в CoolTool.", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("#", GUILayout.Width(20f));
        EditorGUILayout.LabelField("Name", GUILayout.Width(wListTableWidth));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < rewardBlacklistProp.arraySize; i++)
        {
            if (i % 2 == 0)
            {
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = tableColor;
            }
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(i.ToString() + ")", textAllignmentStyle, GUILayout.Width(20f), GUILayout.Height(wListTableHeight / rewardBlacklistProp.arraySize));
            EditorGUILayout.PropertyField(rewardBlacklistProp.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.Width(wListTableWidth), GUILayout.Height(wListTableHeight / rewardBlacklistProp.arraySize));
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(wListTableHeight / rewardBlacklistProp.arraySize)))
            {
                rewardBlacklistProp.DeleteArrayElementAtIndex(i);
            };

            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", GUILayout.Width(30f), GUILayout.Height(20f)))
        {
            rewardBlacklistProp.InsertArrayElementAtIndex(rewardBlacklistProp.arraySize);
        };
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        //правим выше

        EditorGUILayout.HelpBox("Настройки показа рекламы на сценах через промежуток времени." + "\n"
            + "Если стоит ТОЛЬКО галочка Interval - в этой сцене будет использоваться общий таймер. " + "\n"
            + "Время его срабатавания указывается в поле Delay события с именем interval." + "\n"
            + "Галочка \"Игровая сцена\" указывает на продолжительность промежутка между показами рекламы:" + "\n"
            + "Если установлена, то используется промежуток adpausegame(Delay), иначе - adpausemenu(Delay).", MessageType.Info, true);

        EditorGUILayout.LabelField("Список сцен:");
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Индекс", GUILayout.Width(tableWidth));
        //EditorGUILayout.LabelField("Интервал", GUILayout.Width(tableWidth));
        EditorGUILayout.LabelField("Игровая сцена", GUILayout.Width(150));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        for (int j = 0; j < scenesIntervalsProp.arraySize; j++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(j.ToString(), GUILayout.Width(tableWidth), GUILayout.Height(tableHeight / adsFieldsProp.arraySize));
            //EditorGUILayout.PropertyField(scenesIntervalsProp.GetArrayElementAtIndex(j).FindPropertyRelative("haveInterval"), GUIContent.none, GUILayout.Width(tableWidth), GUILayout.Height(25f));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(scenesIntervalsProp.GetArrayElementAtIndex(j).FindPropertyRelative("isGameScene"), GUIContent.none, GUILayout.Width(tableWidth), GUILayout.Height(25f));
            if (EditorGUI.EndChangeCheck())
            {
                //CheckIntervalEvent(j);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Обновить список сцен", GUILayout.Height(40f)))
        {
            UpdateSceneList();
        };
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.HelpBox("Если есть какие-то проблемы с Custom Editorом вы можете отключить его." + "\n"
        //    + "Для этого в правом верхнем углу рядом с иконкой замка нужно найти треугольник,нажать на него и" + "\n"
        //    + "Переключиться с Normal на Debug." + "\n"
        //    + "Или вообще найти и удалить скрипт GBNHZinitEditor.cs" + "\n", MessageType.Warning, true);

        serializedObject.ApplyModifiedProperties();
    }

    void CheckIntervalEvent(int sceneIndex)
    {
        //проверяем нужно ли нам добавлять уникальное событие. Если false - значит нужно будет удалить если существует.
        bool needToAdd = scenesIntervalsProp.GetArrayElementAtIndex(sceneIndex).FindPropertyRelative("isGameScene").boolValue;

        bool founded = false;
        int foundedIndex = -1;
        //пробуем найти событие
        for (int i = 0; i < adsFieldsProp.arraySize; i++)
        {
            if ("interval" + sceneIndex == adsFieldsProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue)
            {
                founded = true;
                foundedIndex = i;
            }
        }
        //если нашли
        if (founded)
        {
            if (needToAdd)
            {
                Debug.Log("interval" + sceneIndex + " найден в списке событий рекламы.");
            }
            else
            {
                Debug.Log("interval" + sceneIndex + " найден в списке событий рекламы. Но его там быть не должно - удаляем.");
                adsFieldsProp.DeleteArrayElementAtIndex(foundedIndex);
            }
        }
        else
        {
            if (needToAdd)
            {
                Debug.Log("<b>interval" + sceneIndex + " НЕ найден в списке событий рекламы. Пробую добавить автоматически. Не забудьте настроить ему параметры.</b>");
                adsFieldsProp.InsertArrayElementAtIndex(adsFieldsProp.arraySize);
                adsFieldsProp.GetArrayElementAtIndex(adsFieldsProp.arraySize - 1).FindPropertyRelative("name").stringValue = "interval" + sceneIndex;
            }
            else
            {
                Debug.Log("interval" + sceneIndex + " не найден в списке событий рекламы. Но его там быть и не должно :D ");
            }
        }
    }

    void UpdateSceneList()
    {
        scenesIntervalsProp.arraySize = SceneManager.sceneCountInBuildSettings;
        Debug.Log("Update Scenes List");
        serializedObject.ApplyModifiedProperties();
    }
}
