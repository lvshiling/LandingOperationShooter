using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GBNAPI
{
    public class Network : MonoBehaviour
    {
        public static Network Instance = null;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
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



    }
}