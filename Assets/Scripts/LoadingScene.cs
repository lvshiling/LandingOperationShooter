using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void ProgressDelegate(float progress);

public class LoadingScene : MonoBehaviour {

    public Image bar;

    static public Image barSt;
    void Start()
    {
        StartCoroutine(LoadSceneAsyncByName("Game", OnLoadLevelProgressUpdate));
    }

    public static IEnumerator LoadSceneAsyncByName(string nextLevel, ProgressDelegate progressDelegate)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(nextLevel);
        
        while (!async.isDone)
        {

            print("test1" + async.progress);
            progressDelegate(async.progress);
            async.allowSceneActivation = async.progress > 0.8;
            yield return null;
        }
    }


    private void OnLoadLevelProgressUpdate(float progress)
    {
        bar.fillAmount = progress;
        print("test2"+progress);
    }

}
