using UnityEngine;

namespace AnalyticsPack
{
    public enum Menu
    {
        MainMenu = 1,
        PauseWindow,
        WinWindow,
        LoseWindow,
        ShopWindow,
        GameWindow,
        NoMoneyWindow,
        SettingsWindow,
        Levels
    }

    public class Analytics : MonoSingleton<Analytics>
    {
        public const string FLOAT_FORMAT = "0.00";
        public const string LOCATION_FORMAT = "0.000000";

        [HideInInspector]
        public IExtEvent extEvent;

        [HideInInspector]
        public AnalyticsEvents events;
        [HideInInspector]
        public AnalyticsGeneralParams generalParams;
        [HideInInspector]
        public EventsEngine eventsEngine;

        private Menu _currentMenu = Menu.MainMenu;
        public Menu CurrentMenu
        {
            get
            {
                return _currentMenu;
            }

            set
            {
                PrevMenu = _currentMenu;
                _currentMenu = value;
            }
        }

        public Menu PrevMenu
        {
            get;
            private set;
        }

        private bool isInited = false;

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {
                events = gameObject.AddComponent<AnalyticsEvents>();
                generalParams = gameObject.AddComponent<AnalyticsGeneralParams>();
                generalParams.OnInit += Init;
                generalParams.Init();
            }
        }

        void Init() {
            if (!isInited) {
                isInited = true;
                AnalyticsSystem.Instance.Init();
            }
        }

        // TODO need refactoring
        public bool CheckFlurrySession()
        {
#if ADS_VERSION
#if !FINAL_VERSION
            Debug.Log("CheckFlurrySession");
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            return FlurryAndroid.IsSessionActive();
#endif
#if UNITY_IOS && !UNITY_EDITOR
            return FlurryIOS.ActiveSessionExists();
#endif
#endif
            return false;
        }
    }
}