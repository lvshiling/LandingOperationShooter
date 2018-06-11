using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GBNAPI;

namespace Assets.Editor.ConfigLoader
{


    public class SDKInfoWindow : EditorWindow
    {
        [MenuItem("Config/Посмотреть содержимое companyInfo.txt", false, 0)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<SDKInfoWindow>();
        }

        void OnEnable()
        {
            CompanyInfo.Parse();
            SDKInfo.Parse();
        }

        void OnGUI()
        {
            GUILayout.Label("Resources/companyInfo.txt" , EditorStyles.boldLabel);

            GUILayout.Label("Информация об аккаунте (CompanyInfo.Struct):", EditorStyles.boldLabel);

            Dictionary<string, string> allInfo = CompanyInfo.Struct.ToDictionary();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Field", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            foreach (string param in allInfo.Keys)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("."+param, GUIStyle.none);
                if (string.IsNullOrEmpty(allInfo[param]))
                {
                    EditorGUILayout.LabelField("Empty", EditorStyles.whiteLabel);
                }
                else
                {
                    EditorGUILayout.SelectableLabel(allInfo[param], GUIStyle.none);
                }
                GUILayout.EndHorizontal();
            }
      
            //---
            GUILayout.Label("Ключи SDK (SDKInfo.GetKey(<Field>)):", EditorStyles.boldLabel);

            Dictionary<string, string> sdkKeys = SDKInfo.ToDictionary();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Field", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Key", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            foreach (string field in sdkKeys.Keys)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(field, GUIStyle.none);
                if (string.IsNullOrEmpty(sdkKeys[field]))
                {
                    EditorGUILayout.LabelField("Empty", EditorStyles.whiteLabel);
                }
                else
                {
                    EditorGUILayout.SelectableLabel(sdkKeys[field], GUIStyle.none);
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Обновить"))
            {
                OnEnable();
            }
        }
    }
}
