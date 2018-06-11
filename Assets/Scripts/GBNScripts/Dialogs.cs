using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PaperPlaneTools;

namespace GBNAPI
{
    public class Dialogs
    {
        public static void AgeCheckDialog(System.Action OnPositive, System.Action OnNegative)
        {
            string title = "Request";
            string message = "How old are you?";
            string olderButton = "13 or older";
            string yangerButton = "Under 13";

            new Alert(title, message)
                .SetPositiveButton(olderButton, () =>
                {
                    if (OnPositive != null)
                    {
                        OnPositive.Invoke();
                    }
                })
                .SetNegativeButton(yangerButton, () =>
                {
                    if (OnNegative != null)
                    {
                        OnNegative.Invoke();
                    }
                })
                .Show();
        }

        public static void NoInternetAccessWarning()
        {
            string title = "No Internet Connection";
            string message = "Sorry, your Internet connection is unavailable";
            string continueButton = "OK";

            Alert nia = new Alert(title, message).SetPositiveButton(continueButton);
            nia.Show();
        }

        public static class RateApp
        {
            private static bool isInited = false;

            public static void Init(string rateUrl)
            {
                string title = "Like the game?";
                string message = "Take a moment to rate us!";
                string rateButton = "Rate";
                string postponeButton = "Later";
                string rejectButton = "";

                RateBox.Instance.DebugMode = false;

                RateBox.Instance.Init(
                            rateUrl,
                            new RateBoxConditions()
                            {
                                MinSessionCount = 0,
                                MinCustomEventsCount = 0,
                                DelayAfterInstallInSeconds = 0,
                                DelayAfterLaunchInSeconds = 0,
                                PostponeCooldownInSeconds = 0,
                                RequireInternetConnection = false
                            },
                            new RateBoxTextSettings()
                            {
                                Title = title,
                                Message = message,
                                RateButtonTitle = rateButton,
                                PostponeButtonTitle = postponeButton,
                                RejectButtonTitle = rejectButton.Length > 0 ? rejectButton : null
                            },
                            new RateBoxSettings()
                            {
                                UseIOSReview = true
                            }
                        );

                isInited = true;
            }

            public static void Show(bool iosNativeOnly = true)
            {
                if (iosNativeOnly)
                {
                    IosNativeShow();
                    return;
                }

                if (isInited)
                {
#if UNITY_IOS
                    RateBox.Instance.Show();
#else
                    RateBox.Instance.ForceShow();
#endif
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("E: Can't show rate app popup - call RateApp.Init() at first!");
#endif
                }
            }

            private static void IosNativeShow()
            {
#if UNITY_IOS && XCODE_8
                if (!isInited)
                {
                    Init("");
                }
                Show(false);
#endif
            }
        }

        public static class PauseOnAds
        {
            private static Alert alert = null;

            public static void Show(System.Action OnDismiss = null)
            {
                string title = "Application paused";
                string message = null;
                string continueBtn = "Continue";

                alert = new Alert(title, message)
                    .SetPositiveButton(continueBtn, () =>
                    {
                        if (OnDismiss != null)
                        {
                            OnDismiss.Invoke();
                        }
                    })
                    .SetOnDismiss(() =>
                    {
                        if (OnDismiss != null)
                        {
                            OnDismiss.Invoke();
                        }
                    });

                alert.Show();
            }

            public static void Dismiss()
            {
                if (alert != null)
                {
                    alert.Dismiss();
                    alert = null;
                }
            }
        }
        /*
        public static class FakeAd
        {
            public static void ShowInterstitial(System.Action OnDismiss = null)
            {
                string title = "Окно рекламы";
                string message = "Отладочная заглушка имитирующая показ Interstitial рекламы";
                string continueBtn = "Закрыть";

                Alert alert = new Alert(title, message)
                    .SetPositiveButton(continueBtn, () =>
                    {
                        if (OnDismiss != null)
                        {
                            OnDismiss.Invoke();
                        }
                    });
                alert.Show();
            }
            public static void ShowRewardedVideo(System.Action OnSuccess = null, System.Action OnDismiss = null)
            {
                string title = "Окно рекламы";
                string message = "Отладочная заглушка имитирующая показ RewardedVideo рекламы";
                string continueBtn = "Посмотреть видео";
                string cancelBtn = "Отказаться";

                Alert alert = new Alert(title, message)
                    .SetPositiveButton(continueBtn, () =>
                    {
                        if (OnSuccess != null)
                        {
                            OnSuccess.Invoke();
                        }
                    })
                    .SetNegativeButton(cancelBtn, () =>
                    {
                        if (OnDismiss != null)
                        {
                            OnDismiss.Invoke();
                        }
                    });
                alert.Show();
            }
        }
        */
    }
}
