using UnityEngine;
using UnityEngine.UI;

namespace AnalyticsPack
{
    public class AnalyticsConsole : MonoBehaviour
    {
        public GameObject console;

        public int currentEvent;
        public Text textCurrentEvent;

        public Text textName;
        public Text[] textKeys;
        public Text[] textVals;

        private Ev[] evst;

        public void DemoEvent()
        {
            Analytics.Instance.events.main.StartGame();
        }

        public void OpenCloseConsole()
        {
            console.SetActive(!console.activeSelf);
            currentEvent = AnalyticsSystem.Instance.Evs.Count - 1;
            UpdateConsole();
        }

        public void NextEvent()
        {
            if (AnalyticsSystem.Instance.Evs.Count > 1)
            {
                currentEvent++;
                if (currentEvent >= AnalyticsSystem.Instance.Evs.Count)
                {
                    currentEvent = 0;
                }
                UpdateConsole();
            }
        }

        public void PrevEvent()
        {
            if (AnalyticsSystem.Instance.Evs.Count > 1)
            {
                currentEvent--;
                if (currentEvent < 0)
                {
                    currentEvent = AnalyticsSystem.Instance.Evs.Count - 1;
                }
                UpdateConsole();
            }
        }

        private void UpdateConsole()
        {
            evst = AnalyticsSystem.Instance.Evs.ToArray();
            if (evst.Length > 0)
            {
                textCurrentEvent.text = (currentEvent + 1).ToString() + "/" + AnalyticsSystem.Instance.Evs.Count.ToString();
                textName.text = "<" + evst[currentEvent].name + ">";

                for (int i = 0; i < textKeys.Length; i++)
                {
                    textKeys[i].text = "-";
                    textVals[i].text = "-";
                }

                int n = 0;
                foreach (var kv in evst[currentEvent].paramsCommon)
                {
                    if (n < textKeys.Length && n < textVals.Length)
                    {
                        textKeys[n].text = kv.Key;
                        textVals[n].text = kv.Value;
                        n++;
                    }
                }
                foreach (var kv in evst[currentEvent].paramsPackGeneral)
                {
                    if (n < textKeys.Length && n < textVals.Length)
                    {
                        textKeys[n].text = kv.Key;
                        textVals[n].text = kv.Value;
                        n++;
                    }
                }
                foreach (var kv in evst[currentEvent].paramsPackEngine)
                {
                    if (n < textKeys.Length && n < textVals.Length)
                    {
                        textKeys[n].text = kv.Key;
                        textVals[n].text = kv.Value;
                        n++;
                    }
                }
            }
            else
            {
                textCurrentEvent.text = "0/0";
                textName.text = "<NULL>";

                for (int i = 0; i < textKeys.Length; i++)
                {
                    textKeys[i].text = "-";
                    textVals[i].text = "-";
                }
            }
        }
    }
}