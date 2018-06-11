using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDKManagement;
using System;
using System.Reflection;

namespace SDKManagement
{
    public class TapcoreAdapter : MonoBehaviour, ISDKAdapter
    {

        #region ISDKAdapter
        public void Init(string name)
        {
            if (info.status == SDKStatus.CANCELED)
            {
                info = new SDKInfo(name, SDKStatus.WAITING, "Start Checking");
                InitSDK();
            }
        }

        private SDKInfo _info;
        public SDKInfo info
        {
            get
            {
                return _info;
            }
            private set
            {
                _info = value;
            }
        }
        #endregion

        private void InitSDK()
        {
#if UNITY_ANDROID
            init();
#else
            _info.status = SDKStatus.CANCELED;
            _info.message = "iOS Platform is not supported";
#endif
        }

        #region Android
#if UNITY_ANDROID
        private void init()
        {
            _info.message = "";
            string version = "";
            //Type.GetType("TCPlugin, Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            Type tapcorePluginType = GetTypeByString("TCPlugin");
            if (tapcorePluginType == null)
            {
                _info.status = SDKStatus.FAILED;
                _info.message = "Tapcore plugin not found";
                return;
            }
            version = tapcorePluginType.GetField("Version").GetRawConstantValue().ToString();
            _info.message = "Version: "+ version;
            try
            {
                tapcorePluginType.GetMethod("Init", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
#if UNITY_EDITOR
                _info.status = SDKStatus.TEST;
#else
                _info.status = SDKStatus.INITED;
#endif
            }
            catch (Exception e)
            {
                _info.status = SDKStatus.FAILED;
                _info.message += "\nError: " + e.Message;
            }
            
        }
#endif
        public static Type GetTypeByString(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
            {
                return type;
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
        #endregion
    }
}