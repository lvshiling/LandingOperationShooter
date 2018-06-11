using System.Collections;
using UnityEngine;


public class GBNCloudHelper : MonoBehaviour
{
    #region SINGLETON
    private static GBNCloudHelper instance;
    public static GBNCloudHelper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GBNCloudHelper>();
                if (instance == null)
                {
                    instance = new GameObject("GBNCloudHelper").AddComponent<GBNCloudHelper>();
                }
            }
            return instance;
        }
    }
    #endregion
        
    public void ReadBookFromFile(string bookId, System.Action<bool> cbOnLoadFromFile)
    {
        StartCoroutine(ReadStreamingAssets("spreadsheets.txt", bookId, cbOnLoadFromFile));
    }

    IEnumerator ReadStreamingAssets(string filename, string bookid, System.Action<bool> cbOnLoadFromFile)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, filename);

        string rawData = "";

        if (filePath.Contains("://"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            rawData = www.text;
        }
        else
        {
            if (System.IO.File.Exists(filePath))
            {
                rawData = System.IO.File.ReadAllText(filePath);
            }
        }

        var success = GBNCloud.ParseFromString(bookid, rawData);

        if (cbOnLoadFromFile != null)
        {
            cbOnLoadFromFile(success);
        }

       
    }
}

