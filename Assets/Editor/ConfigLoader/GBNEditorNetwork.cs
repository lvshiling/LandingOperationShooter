using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Assets.Editor.GBNEditor;

namespace Assets.Editor.GBNEditor
{
    public class GBNEditorNetwork : MonoBehaviour
    {
        public static GBNEditorNetwork Instance = null;

#if EDITOR_DEBUG_MODE_ON
		public static bool printDebug = true;
#else
        public static bool printDebug = false;
#endif

        public static bool stopDownload;
        public static string postRequestResult = string.Empty;
        public static string putRequestResult = string.Empty;
        public static string requestResult = string.Empty;

        static protected List<string> recievers;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                stopDownload = false;
            }
            else
                Destroy(gameObject);
        }

        public static bool IsConnected()
        {
#if !UNITY_WINRT
            bool internetPossiblyAvailable = false;

            switch (Application.internetReachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    internetPossiblyAvailable = true;
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    internetPossiblyAvailable = true;
                    break;
                default:
                    internetPossiblyAvailable = false;
                    break;
            }
            return internetPossiblyAvailable;
#else
			return true;
#endif
        }

        public static string GetRequest(string url, string urlParameters)
        {
            if (printDebug)
                Debug.LogWarning("Request to send : GET " + url + urlParameters);
            try
            {
                if (url.Contains("gitlab"))
                    ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url + urlParameters);
                request.UserAgent = "User-Agent=Mozilla/5.0 Firefox/1.0.7";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 10000;
                string result = string.Empty;
                HttpWebResponse response = null;
                response = (HttpWebResponse)request.GetResponse();

                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
                result = streamReader.ReadToEnd();
                streamReader.Close();

                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string FileGetRequest(string url, string urlParameters, string filePathToDownload)
        {
            if (printDebug)
                Debug.LogWarning("Request to get files : GET " + url + urlParameters);
            try
            {
                if (url.Contains("gitlab"))
                    ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + urlParameters);
                request.Timeout = 300000;
                request.UserAgent = "User-Agent=Mozilla/5.0 Firefox/1.0.7";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream stream = response.GetResponseStream();
                const int size = 262144;
                byte[] bytes = new byte[262144];
                int numBytes;
                using (FileStream fileStream = new FileStream(filePathToDownload, FileMode.Create, FileAccess.Write))
                    while ((numBytes = stream.Read(bytes, 0, size)) > 0)
                    {
                        if (stopDownload)
                        {
                            fileStream.Close();
                            fileStream.Dispose();
                            if (File.Exists(filePathToDownload))
                                File.Delete(filePathToDownload);
                        }
                        else
                            fileStream.Write(bytes, 0, numBytes);
                    }
                string result = "";

                if (!stopDownload)
                    result = "Downloaded file : " + filePathToDownload;
                else
                    result = "";

                stopDownload = false;

                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public struct UrlParameter
        {
            public string name;
            public string value;
        }

        public static string PutRequest(string url, List<UrlParameter> urlParameters)
        {
            UrlParameter contentParameter = urlParameters.Find((p) => { return p.name == "content"; });
            string paramsString = "?";
            string contentParameterString = "&" + contentParameter.name + "=" + contentParameter.value;

            foreach (var item in urlParameters)
            {
                if (item.name != contentParameter.name)
                {
                    paramsString += item.name + "=" + item.value + "&";
                }
            }
            paramsString = paramsString.Remove(paramsString.Length - 1);

            if (printDebug)
                Debug.LogWarning("Request to push file : PUT " + url + paramsString);

            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + paramsString);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Method = "PUT";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(contentParameterString);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    putRequestResult = streamReader.ReadToEnd();

                    if (printDebug)
                        Debug.Log(putRequestResult);

                    return putRequestResult;
                }
            }
            catch (Exception ex)
            {
                if (printDebug)
                    Debug.Log(ex.ToString());

                return ex.ToString();
            }
        }

        public static string PostRequest(string url, List<UrlParameter> urlParameters)
        {
            WWWForm form = new WWWForm();
            foreach (var p in urlParameters)
            {
                form.AddField(p.name, p.value);
            }

            WWW w = new WWW(url, form);

            while (!w.isDone) { }

            if (w.isDone)
            {
                if (printDebug)
                    Debug.Log("Auth result : " + w.text);
                postRequestResult = w.text;
            }
            else if (w.error != null || w.error != String.Empty)
            {
                Debug.LogError(w.text);
                postRequestResult = w.error;
            }
            else
                postRequestResult = w.text;

            return postRequestResult;
        }

        public static IEnumerator RequestCoroutine(string url, string urlParameters)
        {
            if (printDebug)
                Debug.LogWarning("Request to send : GET " + url + urlParameters);

            WWW w = new WWW(url + urlParameters);
            yield return w;

            if (w.isDone)
            {
                if (printDebug)
                    Debug.Log("Request result : " + w.text);
            }
            else if (w.error == null)
                Debug.LogError(w.text);

            requestResult = w.text;
        }

        public static void StopDownload()
        {
            stopDownload = true;
        }

        void OnDestroy()
        {
            recievers = null;
            postRequestResult = string.Empty;
            putRequestResult = string.Empty;
            requestResult = string.Empty;
        }
    }
}