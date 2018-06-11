using SDKManagement;
using System.Collections.Generic;

namespace AnalyticsPack
{
    public class AnalyticsSystem : MonoSingleton<AnalyticsSystem>, ISDKReporter
    {
        public delegate void DelegateInit();

        public delegate void DelegateInvokeEvent(string name, string key, string val);

        public DelegateInit Init = () => { };
        public DelegateInvokeEvent InvokeEvent = (name, key, val) => { };

        private IAnalytics[] services;

        public Queue<Ev> Evs;

        private int countOfEventsInQueue = 20;

        public bool HasActiveService()
        {
            if (services != null)
            {
                for (int i = 0; i < services.Length; i++)
                {
                    if (services[i].IsInited())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void Awake()
        {
            base.Awake();

            countOfEventsInQueue = 20;

            services = new IAnalytics[2];

            services[0] = new AnalyticsDummy();
            services[1] = new AnalyticsFlurry();

            foreach (var i in services)
            {
                Init += i.Init;
            }

            Init += SubscribeToInvokeEvent;

            Evs = new Queue<Ev>();
            //Debug.Log("Init Analytics");
        }

        private void SubscribeToInvokeEvent()
        {
            foreach (var i in services)
            {
                if (i.IsInited())
                {
                    //Debug.Log(i.GetType().ToString() + " subscribed to InvokeEvent");
                    InvokeEvent -= i.InvokeEvent; //для исключения ситуации с двойным подписыванием при повторных вызовах Init()
                    InvokeEvent += i.InvokeEvent;
                }
            }
            Analytics.Instance.events.main.StartGame();
        }

        public void InvokeEventD(Ev e)
        {
            AddToConsole(e);

            foreach (var i in services)
            {
                i.InvokeEvents(e);
            }
        }

        private void AddToConsole(Ev e)
        {
            Evs.Enqueue(e);
            if (Evs.Count > countOfEventsInQueue)
            {
                Evs.Dequeue();
            }
        }

        #region ISDKReporter

        public void GenerateReport()
        {
            SDKReport report = new SDKReport(false);

            foreach (var i in services)
            {
                report.infos.Add(i.GetInfo());
            }
            // пока сделано костылём
            report.infos.Add(new SDKInfo("Test Device", SDKStatus.INITED, CoolTool.isTestDevice.ToString()));
            if (OnReportComplete != null)
            {
                OnReportComplete.Invoke(report);
            }
        }

        public System.Action<SDKReport> OnReportComplete
        {
            get;
            set;
        }

        public bool hasButton
        {
            get
            {
                return false;
            }
        }

        public void ButtonClick()
        {
        }

        public string buttonLabel
        {
            get
            {
                return "";
            }
        }

        #endregion ISDKReporter
    }
}