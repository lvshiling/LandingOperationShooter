using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_Instance;

    public static T Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindObjectOfType(typeof(T)) as T;

                if (s_Instance == null)
                {
                    GameObject gameObject = new GameObject(typeof(T).Name);
                    DontDestroyOnLoad(gameObject);

                    s_Instance = gameObject.AddComponent(typeof(T)) as T;
                }
                else
                {
                    DontDestroyOnLoad(s_Instance.gameObject);
                }
            }

            return s_Instance;
        }
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (s_Instance == null)
        {
            s_Instance = this as T;
            DontDestroyOnLoad(transform.gameObject);
        }
        else
        {
            if (this != s_Instance)
            {
                Destroy(gameObject);
            }
        }
    }
}