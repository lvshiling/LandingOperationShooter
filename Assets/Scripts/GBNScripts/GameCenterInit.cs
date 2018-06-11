using UnityEngine;
#if UNITY_IPHONE
using UnityEngine.SocialPlatforms.GameCenter;
#endif

public class GameCenterInit : MonoBehaviour {

    private void Awake()
    {
        // init game center for app store auto build 
        #if UNITY_IPHONE
        // по умолчании при получении достижения под iOS ничего не происходит, чтобы игрок видел стандартное сообщение о получении достижения нужно вызвать эту функцию
        GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
        #endif
    }

   
}
