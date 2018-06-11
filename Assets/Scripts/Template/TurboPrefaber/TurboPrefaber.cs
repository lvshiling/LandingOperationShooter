using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// TurboPrefaber v1.0.3
/// </summary>
public class TurboPrefaber : MonoBehaviour
{
    public TurboPrefaberItem storageItem;
    public bool uniqueDontDestroyOnLoad;
    public bool needProgress;
    public bool dontTranslateRoot = true;

    public int LoadingProgressCurrent { get; private set; }
    public int LoadingProgressTotal { get; private set; }

    public UnityEvent OnLoadingProgress;
    public UnityEvent OnLoadingFinished;

    private const float targetFps = 60f;
    private int targetObjectsLoadingRate;
    private int frameObjectsCounter;
    private int routinesCounter;

    // Use this for initialization
    private IEnumerator Start()
    {
        if (uniqueDontDestroyOnLoad)
        {
            if (FindObjectsOfType<TurboPrefaber>()
                .Any(x => x != this && x.uniqueDontDestroyOnLoad && x.storageItem == storageItem))
            {
                if (OnLoadingFinished != null)
                {
                    OnLoadingFinished.Invoke();
                }
                Destroy(gameObject);
                yield break;
            }

            DontDestroyOnLoad(this);
        }

        if (needProgress)
        {
            routinesCounter = 0;
            targetObjectsLoadingRate = 50;
            frameObjectsCounter = 0;

            storageItem.LoadingLimiter += LoadWaitContoller;
            storageItem.LoadingHandler += LoadStart;
            storageItem.LoadingProgress += LoadProgress;

            storageItem.Read(transform, true, dontTranslateRoot);

            yield return new WaitUntil(() => routinesCounter == 0);
        }
        else
        {
            storageItem.Read(transform, false, dontTranslateRoot);
        }

        if (OnLoadingFinished != null)
        {
            OnLoadingFinished.Invoke();
        }
        if (!uniqueDontDestroyOnLoad)
        {
            DestroyAfterLoading();
        }
    }

    private void DestroyAfterLoading()
    {
        if (needProgress)
        {
            storageItem.LoadingLimiter -= LoadWaitContoller;
            storageItem.LoadingHandler -= LoadStart;
            storageItem.LoadingProgress -= LoadProgress;
        }

        Destroy(this);
    }

    private IEnumerator LoadingRoutine(IEnumerator loadNext)
    {
        StartCoroutine(loadNext);
        yield return null;
        routinesCounter--;
    }

    private Coroutine resetFrameCounterRoutine;

    private IEnumerator ResetFrameObjectsCounter()
    {
        yield return null;
        targetObjectsLoadingRate = (int)(frameObjectsCounter / (Time.deltaTime * targetFps));
        frameObjectsCounter = 0;
        resetFrameCounterRoutine = null;
    }

    private bool LoadWaitContoller()
    {
        frameObjectsCounter++;

        bool wasTooMuchObjectsLoaded = (frameObjectsCounter > targetObjectsLoadingRate);

        if (wasTooMuchObjectsLoaded && resetFrameCounterRoutine == null)
        {
            resetFrameCounterRoutine = StartCoroutine(ResetFrameObjectsCounter());
        }

        return wasTooMuchObjectsLoaded;
    }

    private void LoadStart(IEnumerator routine)
    {
        StartCoroutine(LoadingRoutine(routine));
        routinesCounter++;
    }

    private void LoadProgress(int current, int total)
    {
        LoadingProgressCurrent = current;
        LoadingProgressTotal = total;

        if (OnLoadingProgress != null)
        {
            OnLoadingProgress.Invoke();
        }
    }
}