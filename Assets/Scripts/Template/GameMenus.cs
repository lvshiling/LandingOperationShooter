using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// GameMenu switcher script v1.2.2
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class GameMenus : MonoBehaviour {

    public static List<GameMenus> All {
        get {
            return new List<GameMenus>(Resources.FindObjectsOfTypeAll<GameMenus>());
            //return new List<GameMenus>(FindObjectOfType<Canvas>().GetComponentsInChildren<GameMenus>(true));
        }
    }

    public static Tween HideAll(HideAnim? overrideHide = null) {
        var allfnd = All.Where(m =>m.gameObject.activeInHierarchy).ToList();
        var fnd = allfnd.FirstOrDefault();
        allfnd.Where(m => m != fnd).ToList().ForEach(m => m.Hide(overrideHide ?? m.hideAnim));

        if(fnd)
        {
            var ret = fnd.Hide(overrideHide??fnd.hideAnim).SetUpdate(true);
            if (fnd.waitForAnimEnds) return ret;
        }

        return null;
    }

    /// <summary>
    /// Показать меню
    /// </summary>
    /// <param name="mid">ID меню</param>
    /// <param name="overrideShow">заменить анимацию показа</param>
    /// <param name="overrideHide">заменить анимацию скрытия</param>
    /// <param name="onShowCallback">вызывается когда все анимации закончились</param>
    /// <returns></returns>
    public static GameMenus ShowMenu(string mid, ShowAnim? overrideShow = null, HideAnim? overrideHide = null, Action onShowCallback = null) {
        var sq = DOTween.Sequence();
        var hd = HideAll(overrideHide);
        if (hd != null)
            sq.Append(hd);

        GameMenus m = All.Find(x => x.MenuID == mid);
        if (m) {            
            sq.Append(m.Show(overrideShow ?? m.showAnim));
        }

        if (!m) return null;

        if(onShowCallback!=null) sq.AppendCallback(()=>onShowCallback());

        sq.SetUpdate(true);

        return m;
    }

    public static T ShowMenu<T>(string mid, ShowAnim? overrideShow = null, HideAnim? overrideHide = null, Action onShowCallback = null)
        where T:GameMenus
    {
        return (T)ShowMenu(mid, overrideShow, overrideHide, onShowCallback);
    }

    public static GameMenus ShowMenu(Enum midEnum, ShowAnim? overrideShow = null, HideAnim? overrideHide = null,Action onShowCallback = null)
    {
        return ShowMenu(midEnum.ToString(), overrideShow, overrideHide, onShowCallback);
    }
    
    public static T ShowMenu<T>(Enum midEnum, ShowAnim? overrideShow = null, HideAnim? overrideHide = null, Action onShowCallback = null)
        where T:GameMenus
    {
        return (T)ShowMenu(midEnum, overrideShow, overrideHide, onShowCallback);
    }

    public enum ShowAnim
    {
        None, FadeIn, FadeInPop, FadeInSlideDown
    }
    public enum HideAnim
    {
        None, FadeOut, FadeOutPop, FadeOutSlideUp
    }


    [Delayed]
    public string MenuID;
    public RectTransform body;

    [Header("Animations")]
    public ShowAnim showAnim;
    public HideAnim hideAnim;
    public float duration = .4f;
    public bool waitForAnimEnds = true;

    CanvasGroup _cGroup;
    Vector2 _originalPos;

    bool _initialized = false;

    void Initialize()
    {
        if (_initialized) return;

        _cGroup = GetComponent<CanvasGroup>();
        _originalPos = body.anchoredPosition;        

        _initialized = true;
    }

    private void Awake()
    {
        Initialize();
    }

    Tween Show(ShowAnim showAnim)
    {
        Initialize();

        _cGroup.alpha = 0;
        
        var sq = DOTween.Sequence().AppendCallback(() => {
            gameObject.SetActive(true);
        });
        
        if (showAnim == ShowAnim.None) return sq.AppendCallback(()=>_cGroup.alpha=1);

        sq.AppendCallback(() =>
        {
            body.localScale = Vector3.one;
            body.anchoredPosition = _originalPos;

            _cGroup.blocksRaycasts = false;

            switch (showAnim)
            {
                case ShowAnim.FadeInPop:
                    body.localScale = new Vector3(.7f, .7f, .7f);
                    body.DOScale(Vector3.one, duration).SetEase(Ease.OutBack).SetUpdate(true);
                    break;
                case ShowAnim.FadeInSlideDown:
                    body.anchoredPosition = new Vector2(_originalPos.x, _originalPos.y + 150);
                    body.DOAnchorPosY(_originalPos.y, duration).SetUpdate(true);
                    break;
            }
        }).Append(_cGroup.DOFade(1, duration));

        return sq.AppendCallback(()=> _cGroup.blocksRaycasts = true);
    }

    Tween Hide(HideAnim hideAnim)
    {
        Initialize();

        var sq = DOTween.Sequence();
        if (hideAnim == HideAnim.None) {
            sq.AppendCallback(() => gameObject.SetActive(false));
            return sq;
        }

        sq.AppendCallback(() =>
        {
            _cGroup.blocksRaycasts = false;

            switch (hideAnim)
            {
                case HideAnim.FadeOutPop:
                    body.DOScale(new Vector3(.7f, .7f, .7f), duration).SetEase(Ease.InBack).SetUpdate(true);
                    break;
                case HideAnim.FadeOutSlideUp:
                    body.DOAnchorPosY(_originalPos.y + 150, duration).SetUpdate(true);
                    break;
            }
        }).Append(_cGroup.DOFade(0, duration));

        return sq.AppendCallback(() => gameObject.SetActive(false));
    }

    public void ShowMe()
    {
        ShowMenu(MenuID);
    }
}