using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GBNLocalization))]
public class GBNLocalizationEditor : Editor
{
    private int currentLanguage = -1;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var index = serializedObject.FindProperty("language").enumValueIndex;
        if (index != currentLanguage)
        {
            currentLanguage = index;
            PlayerPrefs.SetInt("language", currentLanguage);
            GBNEventManager.TriggerEvent(GBNEvent.UPDATE_GUI);
        }
    }
}