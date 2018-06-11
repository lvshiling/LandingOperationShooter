using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace AnalyticsPack
{
    public class AnalyticsEvents : MonoBehaviour
    {
        public EventMain main;

        public EventAirplane airplane;
        public EventAngryAnimal angryAnimal;
        public EventAnimal animal;
        public EventAnimalFighting animalFighting;
        public EventArchery archery;
        public EventCarCrashTest carCrashTest;
        public EventCooking cooking;
        public EventDriver driver;
        public EventEpicBattle epicBattle;
        public EventEscape escape;
        public EventFighting fighting;
        public EventHelloNeighbor helloNeighbor;
        public EventJason jason;
        public EventKnightFighting knightFighting;
        public EventNights nights;
        public EventOffroad offroad;
        public EventPeacefulSurvival peacefulSurvival;
        public EventRacing racing;
        public EventRopeHero ropeHero;
        public EventShooter shooter;
        public EventSniper sniper;
        public EventSuperhot superHot;
        public EventSurgery surgery;
        public EventSurvival survival;
        public EventTrain train;
        public EventTransformer transformer;

        public void SendEvent(string name, Dictionary<string, string> paramsDictionary)
        {
#if !ADS_VERSION
            return;
#endif
            // Создаем ивент с именем name
            Ev e = new Ev(name);

            // Добавляем непустые значения
            foreach (var kv in paramsDictionary)
            {
                if (!string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value))
                {
                    e.paramsCommon.Add(kv.Key, kv.Value);
                }
            }

            // Добавляем системные постоянно передающиеся параметры группы GENERAL
            e.paramsPackGeneral = Analytics.Instance.generalParams.GetGeneralParams();

            // Добавляем системные постоянно передающиеся параметры группы ПО ENGINE
            if (Analytics.Instance.extEvent != null)
            {
                e.paramsPackEngine = Analytics.Instance.extEvent.GetAllParams();
            }
            else
            {
                e.paramsPackEngine = new Dictionary<string, string>();
            }

            // Output
            if (Analytics.Instance.generalParams.isTestDevice)
            {
                // дебаг в эдиторе делает AnalyticsDummy
#if !UNITY_EDITOR
                Debug.Log(e);
#endif
            }

            AnalyticsSystem.Instance.InvokeEventD(e);
        }

        public void SendEventByName(string s)
        {
            SendEvent(s, new Dictionary<string, string>() { { "", "" } });
        }

        public void SendEventByName(string name, string key, string val)
        {
            SendEvent(name, new Dictionary<string, string>() { { key, val } });
        }

        private void Init()
        {
            main = new EventMain();
            airplane = new EventAirplane();
            angryAnimal = new EventAngryAnimal();
            animal = new EventAnimal();
            animalFighting = new EventAnimalFighting();
            archery = new EventArchery();
            carCrashTest = new EventCarCrashTest();
            cooking = new EventCooking();
            driver = new EventDriver();
            epicBattle = new EventEpicBattle();
            escape = new EventEscape();
            fighting = new EventFighting();
            helloNeighbor = new EventHelloNeighbor();
            jason = new EventJason();
            knightFighting = new EventKnightFighting();
            nights = new EventNights();
            offroad = new EventOffroad();
            peacefulSurvival = new EventPeacefulSurvival();
            racing = new EventRacing();
            ropeHero = new EventRopeHero();
            shooter = new EventShooter();
            sniper = new EventSniper();
            superHot = new EventSuperhot();
            surgery = new EventSurgery();
            survival = new EventSurvival();
            train = new EventTrain();
            transformer = new EventTransformer();
        }

        private void Awake()
        {
            Init();
        }
    }
}