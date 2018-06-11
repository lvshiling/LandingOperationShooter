using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ScreenUI : MonoBehaviour
{
    public static List<ScreenUI> All
    {
        get
        {
            return new List<ScreenUI>(Resources.FindObjectsOfTypeAll<ScreenUI>());
            //return new List<GameMenus>(FindObjectOfType<Canvas>().GetComponentsInChildren<GameMenus>(true));
        }
    }

    public enum AnimationType
    {
        move,
        fade
    }

    public AnimationType animType;
    private CanvasGroup canvasGroup;
    public string screenName;
    public List<RectTransform> partOfScreen;
    public List<RectTransform> preSetObj;
    public List<Vector3> positionsIn;
    public List<Vector3> positionsOut;
    public List<Vector2> pivots;
    public float speedIn;
    public float speedOut;
    public bool isMoveIn;
    public bool isMoveOut;
    public bool isNeedOff;

    public delegate void Action(string screenName);

    public event Action onInit;

    public event Action onClose;

    public void Init()
    {
        if (animType == AnimationType.fade && canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (animType == AnimationType.move)
        {
            positionsIn = new List<Vector3>();
            positionsOut = new List<Vector3>();
            pivots = new List<Vector2>();
            for (int i = 0; i < partOfScreen.Count; i++)
            {
                if (preSetObj != null && i < preSetObj.Count)
                {
                    positionsIn.Add(partOfScreen[i].anchoredPosition3D);
                    positionsOut.Add(preSetObj[i].anchoredPosition3D);
                }
                else
                {
                    positionsIn.Add(Vector3.zero);
                    positionsOut.Add(Vector3.zero);
                }
            }
        }
    }

    public virtual void MoveTweenIn()
    {
        switch (animType)
        {
            case AnimationType.move:
                {
                    for (int i = 0; i < partOfScreen.Count; i++)
                    {
                        partOfScreen[i].DOKill();

                        DOTween.Sequence().SetUpdate(true).SetDelay(0.2f).Append(partOfScreen[i].DOAnchorPos(positionsIn[i], speedIn).SetEase(Ease.Linear).SetUpdate(true));
                    }
                    break;
                }
            case AnimationType.fade:
                {
                    canvasGroup.DOFade(1, speedIn).SetUpdate(true);
                    break;
                }
        }
    }

    public void OffObject()
    {
        gameObject.SetActive(false);
    }

    public void DoHEll(int i)
    {
        print(Time.time);
        partOfScreen[i].DOAnchorPos(positionsIn[i], speedIn).SetEase(Ease.Linear).SetUpdate(true);
    }

    public virtual void MoveTweenOut()
    {
        switch (animType)
        {
            case AnimationType.move:
                {
                    if (partOfScreen.Count > 0)
                    {
                        for (int i = 0; i < partOfScreen.Count; i++)
                        {
                            partOfScreen[i].DOKill();
                            partOfScreen[i].DOAnchorPos(positionsOut[i], speedOut).SetUpdate(true).SetEase(Ease.Flash).OnComplete(() => OffObject());
                        }
                    }
                    else
                    {
                        OffObject();
                    }
                    break;
                }
            case AnimationType.fade:
                {
                    canvasGroup.DOFade(0, speedOut).SetUpdate(true).OnComplete(() => OffObject());
                    break;
                }
        }
    }

    public virtual void SetActive(bool on)
    {
        if (positionsOut == null || positionsIn == null || positionsOut.Count < 1 || positionsIn.Count < 1)
        {
            Init();
        }
        if (on)
        {
            try
            {
                onInit(screenName);

            }
            catch
            {
                
            }
            for (int i = 0; i < partOfScreen.Count; i++)
            {
                partOfScreen[i].anchoredPosition3D = positionsOut[i];
            }
            MoveTweenIn();
        }
        else
        {
            try
            {
                onClose(screenName);
            }
            catch
            {
            }
            MoveTweenOut();
        }
    }
}