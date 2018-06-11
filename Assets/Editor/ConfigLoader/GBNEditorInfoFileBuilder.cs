using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Editor.GBNEditor
{

    public class GBNEditorInfoFileBuilder
    {
        private static string fileName = "companyInfo";

        private static string filePath
        {
            get
            {
                if (!Directory.Exists("Assets/Resources/"))
                {
                    Directory.CreateDirectory("Assets/Resources/");
                }
                return "Assets/Resources/" + fileName + ".txt";
            }
        }

        public static void SetFileName(string newFileName)
        {
            Remove();
            fileName = newFileName;
            Clear();
        }

        public static void Clear()
        {
            if (File.Exists(filePath))
            {
                JSONObject jsonToFile = new JSONObject("{}");
                File.WriteAllText(filePath, jsonToFile.ToString());
            }
        }

        public static void Remove()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static JSONObject GetCurrentObjectFromFile()
        {
            JSONObject jsonInFile = new JSONObject("{}");

            if (File.Exists(filePath))
            {
                string current = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(current))
                {
                    try
                    {
                        jsonInFile = new JSONObject(current);
                        if (!jsonInFile.IsObject)
                        {
                            jsonInFile = new JSONObject("{}");
                            Debug.Log("File " + filePath + " contains information in a wrong format. File cleared.");
                        }
                    }
                    catch
                    {
                        jsonInFile = new JSONObject("{}");
                        Debug.Log("File " + filePath + " contains information in a wrong format. Exception catched. File cleared.");
                    }
                }
            }

            return jsonInFile;
        }

        private static void SaveToFile(JSONObject jsonToFile)
        {
            File.WriteAllText(filePath, jsonToFile.ToString());
        }

        public static void SetValue(string field, string value)
        {
            if (string.IsNullOrEmpty(field))
            {
                Debug.LogError("Can't set value with the empty field name!");
                return;
            }

            JSONObject jsonToFile = GetCurrentObjectFromFile();
            
            string[] fields = field.Split('/');
            int fieldsLength = fields.Length;

            JSONObject tmpObj = jsonToFile;

            for (int i = 0; i < fieldsLength; i++)
            {
                if (string.IsNullOrEmpty(fields[i]))
                {
                    Debug.LogError("Can't set value with the empty field name!");
                    return;
                }
                if (i == fieldsLength - 1)
                {
                    if (tmpObj.HasField(fields[i]))
                    {
                         tmpObj.SetField(fields[i], value);
                    }
                    else
                    {
                        tmpObj.AddField(fields[i], value);
                    }
                    break;
                }

                if (tmpObj.HasField(fields[i]))
                {
                    tmpObj = tmpObj.GetField(fields[i]);
                }
                else
                {
                    tmpObj.AddField(fields[i], new JSONObject("{}"));
                    tmpObj = tmpObj.GetField(fields[i]);
                }
            }

            SaveToFile(jsonToFile);
        }

        public static void SetValue(string field, bool value)
        {
            if (string.IsNullOrEmpty(field))
            {
                Debug.LogError("Can't set value with the empty field name!");
                return;
            }

            JSONObject jsonToFile = GetCurrentObjectFromFile();

            string[] fields = field.Split('/');
            int fieldsLength = fields.Length;

            JSONObject tmpObj = jsonToFile;

            for (int i = 0; i < fieldsLength; i++)
            {
                if (string.IsNullOrEmpty(fields[i]))
                {
                    Debug.LogError("Can't set value with the empty field name!");
                    return;
                }
                if (i == fieldsLength - 1)
                {
                    if (tmpObj.HasField(fields[i]))
                    {
                        tmpObj.SetField(fields[i], value);
                    }
                    else
                    {
                        tmpObj.AddField(fields[i], value);
                    }
                    break;
                }

                if (tmpObj.HasField(fields[i]))
                {
                    tmpObj = tmpObj.GetField(fields[i]);
                }
                else
                {
                    tmpObj.AddField(fields[i], new JSONObject("{}"));
                    tmpObj = tmpObj.GetField(fields[i]);
                }
            }

            SaveToFile(jsonToFile);
        }

        public static void SetValue(string field, int value)
        {
            if (string.IsNullOrEmpty(field))
            {
                Debug.LogError("Can't set value with the empty field name!");
                return;
            }

            JSONObject jsonToFile = GetCurrentObjectFromFile();

            string[] fields = field.Split('/');
            int fieldsLength = fields.Length;

            JSONObject tmpObj = jsonToFile;

            for (int i = 0; i < fieldsLength; i++)
            {
                if (string.IsNullOrEmpty(fields[i]))
                {
                    Debug.LogError("Can't set value with the empty field name!");
                    return;
                }
                if (i == fieldsLength - 1)
                {
                    if (tmpObj.HasField(fields[i]))
                    {
                        tmpObj.SetField(fields[i], value);
                    }
                    else
                    {
                        tmpObj.AddField(fields[i], value);
                    }
                    break;
                }

                if (tmpObj.HasField(fields[i]))
                {
                    tmpObj = tmpObj.GetField(fields[i]);
                }
                else
                {
                    tmpObj.AddField(fields[i], new JSONObject("{}"));
                    tmpObj = tmpObj.GetField(fields[i]);
                }
            }

            SaveToFile(jsonToFile);
        }

        public static void SetValue(string field, float value)
        {
            if (string.IsNullOrEmpty(field))
            {
                Debug.LogError("Can't set value with the empty field name!");
                return;
            }

            JSONObject jsonToFile = GetCurrentObjectFromFile();

            string[] fields = field.Split('/');
            int fieldsLength = fields.Length;

            JSONObject tmpObj = jsonToFile;

            for (int i = 0; i < fieldsLength; i++)
            {
                if (string.IsNullOrEmpty(fields[i]))
                {
                    Debug.LogError("Can't set value with the empty field name!");
                    return;
                }
                if (i == fieldsLength - 1)
                {
                    if (tmpObj.HasField(fields[i]))
                    {
                        tmpObj.SetField(fields[i], value);
                    }
                    else
                    {
                        tmpObj.AddField(fields[i], value);
                    }
                    break;
                }

                if (tmpObj.HasField(fields[i]))
                {
                    tmpObj = tmpObj.GetField(fields[i]);
                }
                else
                {
                    tmpObj.AddField(fields[i], new JSONObject("{}"));
                    tmpObj = tmpObj.GetField(fields[i]);
                }
            }

            SaveToFile(jsonToFile);
        }

        public static void MergeJSON(string field, JSONObject value)
        {
            JSONObject jsonToFile = GetCurrentObjectFromFile();

            if (string.IsNullOrEmpty(field))
            {
                jsonToFile.Merge(value);
            }
            else
            {
                string[] fields = field.Split('/');
                int fieldsLength = fields.Length;

                JSONObject tmpObj = jsonToFile;

                for (int i = 0; i < fieldsLength; i++)
                {
                    if (i == fieldsLength - 1)
                    {
                        if (tmpObj.HasField(fields[i]))
                        {
                            tmpObj = tmpObj.GetField(fields[i]);
                            tmpObj.Merge(value);
                        }
                        else
                        {
                            tmpObj.AddField(fields[i], value);
                        }
                        break;
                    }

                    if (tmpObj.HasField(fields[i]))
                    {
                        tmpObj = tmpObj.GetField(fields[i]);
                    }
                    else
                    {
                        tmpObj.AddField(fields[i], new JSONObject("{}"));
                        tmpObj = tmpObj.GetField(fields[i]);
                    }
                }
            }

            SaveToFile(jsonToFile);
        }

        public static List<string> ExtractAllStringValues()
        {
            return ExtractStringValuesRecursively(GetCurrentObjectFromFile());
        }

        private static List<string> ExtractStringValuesRecursively(JSONObject input)
        {
            List<string> output = new List<string>();

            List<string> keys = input.keys;

            foreach (string key in keys)
            {
                JSONObject tmpObj = input.GetField(key);
                if (tmpObj.IsObject)
                {
                    output.AddRange(ExtractStringValuesRecursively(tmpObj));
                }
                else if (tmpObj.IsArray)
                {
                    for (int i = 0; i < tmpObj.Count; ++i)
                    {
                        if (tmpObj[i].type == JSONObject.Type.STRING)
                        {
                            output.Add(tmpObj[i].str);
                        }
                    }
                }
                else
                {
                    if (tmpObj.type == JSONObject.Type.STRING)
                    {
                        output.Add(tmpObj.str);
                    }
                }
            }

            return output;
        }
    }
}
