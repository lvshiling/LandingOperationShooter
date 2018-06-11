using System.Collections.Generic;
using UnityEngine;
using System;


public class SheetTable<T>
{

    public List<string> rowNames = new List<string>();
    public List<string> colNames = new List<string>();

    public Dictionary<string, Dictionary<string, T>> data = new Dictionary<string, Dictionary<string, T>>();

    public T GetCell(string rowName, string colName)
    {
        if (!data.ContainsKey(rowName) || !data[rowName].ContainsKey(colName))
        {
            return GetValue("");
        }
        return data[rowName][colName];
    }

    public Dictionary<string, T> GetRow(string rowName)
    {
        if (!data.ContainsKey(rowName))
        {
            return null;
        }
        return data[rowName];
    }

    public static T GetValue(string value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }

}
public static class GBNCloud
{
    #region Какая-то непонятная штука (лучше не трогать)
    // Helper class: because UnityEngine.JsonUtility does not support deserializing an array...
    // http://forum.unity3d.com/threads/how-to-load-an-array-with-jsonutility.375735/
    private class GSFUJsonHelper
    {
        public static T[] JsonArray<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array = new T[] { };
        }
    }
    #endregion

    class BookData
    {
        public List<string> sheetNames;  // имена листов в книге
        public List<string> rawData;     // данные в листах
    }

    private const string BOOK_SEPOR = "\n_BOOK_SEPOR_\n";
    private const string SHEET_SEPOR = "\n_SHEET_SEPOR_\n";
    private const string DATA_SEPOR = "\n_DATA_SEPOR_\n";

    // словарь   id книги - BookData
    private static readonly Dictionary<string, BookData> AllData = new Dictionary<string, BookData>();
    public static readonly Queue<string> FetchQueue = new Queue<string>(); // Очередь id книг на загрузку
    public static string currentBookId; // id книги, данные по которой ожидаем
    public static bool isCurrentFetched;

    private static readonly Dictionary<string, Action<string, bool>> Callbacks = new Dictionary<string, Action<string, bool>>();
   
    public static bool IsInited { get; private set; }

    public static void Init()
    {
        if (IsInited == false)
        {
            CloudConnector.Instance.spreadsheetId = "";
            CloudConnectorCore.processedResponseCallback.AddListener(OnReadFromCloud);
            CloudConnectorCore.rawResponseCallback.AddListener(OnReadFromCloudFail);
            IsInited = true;
            isCurrentFetched = true;
        }
    }

    public static void Uninit()
    {
        if (IsInited == true)
        {
            CloudConnectorCore.processedResponseCallback.RemoveListener(OnReadFromCloud);
            CloudConnectorCore.rawResponseCallback.RemoveListener(OnReadFromCloudFail);
            IsInited = false;

            // ?? возможно не нужно очищать
            FetchQueue.Clear();
            //allData.Clear();
        }
    }

    private static void OnReadFromCloud(CloudConnectorCore.QueryType query, List<string> sheets, List<string> data)
    {
        if (query == CloudConnectorCore.QueryType.getAllTables)
        {
            BookData book = new BookData();
            book.sheetNames = sheets;
            book.rawData = data;

            AllData[currentBookId] = book;

            if (Callbacks.ContainsKey(currentBookId) && Callbacks[currentBookId] != null)
            {
                Callbacks[currentBookId](currentBookId, true);
                Callbacks[currentBookId] = null;
                Callbacks.Remove(currentBookId);
            }

            isCurrentFetched = true;
            Debug.Log("<color=blue><b>Получено " + sheets.Count + " лист(ов) из книги (" + currentBookId + ")</b></color>");

            FetchNext();
        }
    }

    private static void OnReadFromCloudFail(string raw)
    {
        Debug.Log("<color=red>Ошибка получения данных из облака о книге (" + currentBookId + ")</color>");
        Debug.Log("<color=blue>Чтение данных о книге (" + currentBookId + ") из локального файла</color>");

        GBNCloudHelper.Instance.ReadBookFromFile(currentBookId, OnReadFromFile);
    }

    private static void OnReadFromFile(bool success)
    {
        if (Callbacks.ContainsKey(currentBookId) && Callbacks[currentBookId] != null)
        {
            Callbacks[currentBookId](currentBookId, success);
            Callbacks[currentBookId] = null;
            Callbacks.Remove(currentBookId);
        }

            isCurrentFetched = true;
            FetchNext();
    }
    private static void FetchNext()
    {
        if (isCurrentFetched && IsInited && FetchQueue.Count > 0)
        {
            isCurrentFetched = false;
            currentBookId = FetchQueue.Dequeue();
            CloudConnector.Instance.spreadsheetId = currentBookId;
            Debug.Log("<color=blue>Чтение книги из облака (" + currentBookId + ")</color>");
            CloudConnectorCore.GetAllTables();
        }
    }

    public static void FetchBook(string bookId, Action<string, bool> cbBookRead)
    {
        if (Callbacks.ContainsKey(bookId))
        {
            Callbacks[bookId] += cbBookRead;
        }
        else
        {
            Callbacks[bookId] = cbBookRead;
        }

        FetchQueue.Enqueue(bookId);
        FetchNext();
    }

    public static bool HasSheet(string bookId, string sheetName)
    {
        if (!AllData.ContainsKey(bookId))
        {
            return false;
        }
        if (AllData[bookId].sheetNames != null && AllData[bookId].rawData != null)
        {
            var sn = AllData[bookId].sheetNames;
            for (int i = 0; i < sn.Count; ++i)
            {
                if (string.Compare(sn[i], sheetName) == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static T[] ReadSheet<T>(string bookId, string sheetName)
    {
        if (!AllData.ContainsKey(bookId))
        {
            Debug.LogError("Данные по книге (" + bookId + ") НЕ ПОЛУЧЕНЫ");
            return null;
        }
        if (AllData[bookId].sheetNames != null && AllData[bookId].rawData != null)
        {
            List<string> sn = AllData[bookId].sheetNames;
            for (int i = 0; i < sn.Count; ++i)
            {
                if (string.Compare(sn[i], sheetName) == 0)
                {
                    return GSFUJsonHelper.JsonArray<T>(AllData[bookId].rawData[i]);
                }
            }
            Debug.LogError("Нет листа с именем \"" + sheetName + "\" в книге (" + bookId + ")");
            return null;
        }
        Debug.LogError("Данные, видимо, еще не получены из облака");
        return null;
    }

    static string GetRawString()
    {
        string str = "";
        if (AllData.Count < 1)
        {
            return str;
        }
        /* 
                * book_id1
                * SHEET_SEPOR
                * sheetname1             
                * sheetname2             
                * ...             
                * DATA_SEPOR             * 
                * data1             
                * data2             
                * ...
                * BOOK_SEPOR
                * book_id2
                * ....
        */

        foreach (KeyValuePair<string, BookData> elem in AllData)
        {
            str += elem.Key;
            str += SHEET_SEPOR;
            foreach (string n in elem.Value.sheetNames)
            {
                str += n + " ///\n";
            }
            str += DATA_SEPOR;
            foreach (string n in elem.Value.rawData)
            {
                str += n + " ///\n";
            }
            str += BOOK_SEPOR;
        }

        return str;
    }

    public static void WriteAllToFile()
    {
#if UNITY_EDITOR
        if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
        {
            System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "spreadsheets.txt");
        System.IO.File.WriteAllText(filePath, GetRawString());
        Debug.Log("<b><color=blue>Книги сохранены в файл: \"" + filePath + "\"</color></b>");
#endif
    }

    public static bool ParseFromString(string bookId, string rawData)
    {
        if (string.IsNullOrEmpty(rawData))
        {
            return false;
        }
        string[] books = rawData.Split(new string[] { BOOK_SEPOR }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < books.Length; ++i)
        {
            string[] part1 = books[i].Split(new string[] { SHEET_SEPOR }, StringSplitOptions.RemoveEmptyEntries);
            if (part1[0] == bookId) // нужная книга
            {
                string[] part2 = part1[1].Split(new string[] { DATA_SEPOR }, StringSplitOptions.RemoveEmptyEntries);
                string[] sheetnames = part2[0].Split(new string[] { " ///\n" }, StringSplitOptions.RemoveEmptyEntries);
                string[] sheetdatas = part2[1].Split(new string[] { " ///\n" }, StringSplitOptions.RemoveEmptyEntries);
                BookData book = new BookData();
                book.sheetNames = new List<string>(sheetnames);
                book.rawData = new List<string>(sheetdatas);
                AllData[part1[0]] = book;
                Debug.Log("<color=blue><b>Книга (" + bookId + ") прочитана из локального файла</b></color>");
                return true;
            }
        }
        Debug.Log("<color=red>Книга (" + bookId + ") не найдена в локальном файле</color>");
        return false;
    }
    public static SheetTable<T> ReadSheetTable<T>(string bookId, string sheetName, T emptyValue)
    {
        if (AllData.ContainsKey(bookId) == false)
        {
            Debug.LogError("Данные по книге (" + bookId + ") НЕ ПОЛУЧЕНЫ");
            return null;
        }
        if (AllData[bookId].sheetNames != null && AllData[bookId].rawData != null)
        {
            List<string> sn = AllData[bookId].sheetNames;
            for (int i = 0; i < sn.Count; ++i)
            {
                if (string.Compare(sn[i], sheetName) == 0)
                {
                    return ReadJsonStringList<T>(AllData[bookId].rawData[i], emptyValue);
                }
            }
            Debug.LogError("Нет листа с именем \"" + sheetName + "\" в книге (" + bookId + ")");
            return null;
        }
        Debug.LogError("Данные, видимо, еще не получены из облака");
        return null;
    }

    private static SheetTable<T> ReadJsonStringList<T>(string jsonData, T emptyValue)
    {
        var table = new SheetTable<T>();

        JSONObject json = new JSONObject(jsonData);
        // process each row
        foreach (var obj in json.list)
        {
            string rowName = "";
            Dictionary<string, T> rowData = new Dictionary<string, T>();
            // process cells
            for (int i = 0; i < obj.keys.Count; i++)
            {
                var key = obj.keys[i];

                if (i == 0)
                {
                    // add value of first column as rowName
                    rowName = "";
                    obj.GetField(ref rowName, key);
                    if (table.rowNames.IndexOf(rowName) < 0)
                    {
                        table.rowNames.Add(rowName);
                    }
                }
                else
                {
                    // add key of column as column name
                    if (table.colNames.IndexOf(key) < 0)
                    {
                        table.colNames.Add(key);
                    }
                    // key value pair
                    var t = typeof(T);
                    if (t == typeof(int))
                    {
                        int intValue = 0;
                        obj.GetField(ref intValue, key);
                        rowData.Add(key, (T)Convert.ChangeType(intValue, t));
                    }
                    else if (t == typeof(string))
                    {
                        string stringValue = "";
                        obj.GetField(ref stringValue, key);
                        rowData.Add(key, (T)Convert.ChangeType(stringValue, t));
                    }
                    else if (t == typeof(float))
                    {
                        float floatValue = 0;
                        obj.GetField(ref floatValue, key);
                        rowData.Add(key, (T)Convert.ChangeType(floatValue, t));
                    }
                    else if (t == typeof(bool))
                    {
                        bool boolValue = false;
                        obj.GetField(ref boolValue, key);
                        rowData.Add(key, (T)Convert.ChangeType(boolValue, t));
                    }
                    else
                    {
                        rowData.Add(key, emptyValue);
                    }


                }
            }
            // add row to table
            if (!table.data.ContainsKey(rowName))
                table.data.Add(rowName, rowData);
        }

        return table;
    }
}
