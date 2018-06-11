using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ScreenUI))]
public class GameMenusEditor : Editor {

    public void GenerateEnum()
    {
        var tgt = ((ScreenUI)target);
        tgt.screenName = tgt.name;

        //ztgt.screenName = Regex.Replace(tgt.screenName, @"[^a-zA-Z]", "");

        var templateTextFile = AssetDatabase.LoadAssetAtPath("Assets/Scripts/Template/Editor/MenuTemplate.txt",
            typeof(TextAsset)) as TextAsset;

        string contents = "";

        if (templateTextFile == null)
        {
            Debug.LogError("Не найден файл MenuTemplate.txt! Он должен быть в папке ВАШ_ПРОЕКТ/Assets/Scripts/Template/Editor/");
            return;
        }
        foreach(ScreenUI x in ScreenUI.All)
        {
            x.screenName = x.name;
        }
        var lst = ScreenUI.All.Distinct().Aggregate("", (str, x) => str + x.screenName + ",", str => str.TrimEnd(','));
        var scNm = SceneManager.GetActiveScene().name;

        contents = templateTextFile.text;
        contents = contents.Replace("%SCENE%", scNm);
        contents = contents.Replace("%DATA%", lst);

        var dir = Application.dataPath + "/Scripts/Template/GameMenusEnums";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        using (StreamWriter sw = new StreamWriter(string.Format("{0}/MenuNames_{1}.cs", dir, scNm)))
        {
            sw.Write(contents);
        }
    }
	public override void OnInspectorGUI()
	{
        base.OnInspectorGUI();
        //GenerateEnum();
        if (GUILayout.Button("Добавить в енум"))
        {

            GenerateEnum();
            AssetDatabase.Refresh();
        }
		
	}
}
