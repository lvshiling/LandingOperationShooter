using GBNAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKManagement
{
    public class PermissionManager
    {
        Dictionary<string, bool> checkedPermissions;
        Dictionary<string, bool> grantedPermissions;
 
        public PermissionManager()
        {
            checkedPermissions = new Dictionary<string, bool>();
            grantedPermissions = new Dictionary<string, bool>();
        }

        public void Add(string[] permissions)
        {
            var defaultValue = false;
#if UNITY_IOS
            defaultValue = true;
#endif
            if (permissions != null)
            {
                for (int i = 0; i < permissions.Length; i++)
                {
                    if (!string.IsNullOrEmpty(permissions[i]) && !checkedPermissions.ContainsKey(permissions[i]))
                    {

                        checkedPermissions.Add(permissions[i], defaultValue);
                        grantedPermissions.Add(permissions[i], defaultValue);
                    }
                }
            }
        }

        public void RequestStatus()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string[] permissions = new string[checkedPermissions.Keys.Count];
            var i = 0;
            foreach (var key in checkedPermissions.Keys)
            {
                permissions[i] = key;
                i++;
            }
            using (AndroidJavaClass unityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activityContext = unityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaClass pm = new AndroidJavaClass("com.pm.PM"))
                {
                    object[] objs = new object[2];
                    objs[0] = activityContext;
                    objs[1] = permissions;
                    pm.CallStatic("requestPermissions", objs);
                }
            }
#endif
        }

        private void UpdateStatus()
        {
#if UNITY_ANDROID
            // костыль: нельзя итерировать по словарю и изменять его
            List<string> permissions = new List<string>(checkedPermissions.Keys);
#if UNITY_EDITOR
            foreach (string permission in permissions)
            {
                if (!checkedPermissions[permission])
                {
                    checkedPermissions[permission] = true;
                    grantedPermissions[permission] = true;
                }
            }
#endif
#if !UNITY_EDITOR
            foreach (string permission in permissions)
            {
                if (!checkedPermissions[permission])
                {
                    using (AndroidJavaClass unityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        AndroidJavaObject activityContext = unityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
                        using (AndroidJavaClass pm = new AndroidJavaClass("com.pm.PM"))
                        {
                            checkedPermissions[permission] = pm.CallStatic<bool>("isPermissionChecked", activityContext, permission);
                            if (checkedPermissions[permission])
                            {
                                grantedPermissions[permission] = pm.CallStatic<bool>("isPermissionGranted", activityContext, permission);
                            }
                        }
                    }
                }
               // Debug.Log(permission + " checked: " + checkedPermissions[permission] + " granted: " + grantedPermissions[permission]);
            }
#endif
#endif
        }

        public bool isCountryAllowed(string[] countries)
        {
            if (countries == null || countries.Length == 0)
            {
                return true;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass unityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activityContext = unityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaClass pm = new AndroidJavaClass("com.pm.PM"))
                {
                    string country = pm.CallStatic<string>("getCountry", activityContext);
                    for (int i = 0; i < countries.Length; i++) {
                        if (country == countries[i]) return true;
                    }
                    return false;
                }
            }
#else
            return true;
#endif
        }


        public bool isChecked(string[] permissions)
        {
            if (permissions != null)
            {
                UpdateStatus();
                for (int i = 0; i < permissions.Length; i++)
                {
                    if (!string.IsNullOrEmpty(permissions[i]) && !checkedPermissions.ContainsKey(permissions[i]) || !checkedPermissions[permissions[i]])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        public bool isGranted(string[] permissions)
        {
            if (permissions != null)
            {
                for (int i = 0; i < permissions.Length; i++)
                {
                    if (!string.IsNullOrEmpty(permissions[i]) && !grantedPermissions.ContainsKey(permissions[i]) || !grantedPermissions[permissions[i]])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return true;
            }
        }
    }
}