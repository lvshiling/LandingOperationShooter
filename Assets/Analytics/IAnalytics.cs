using SDKManagement;

namespace AnalyticsPack
{
    public interface IAnalytics
    {
        bool IsInited();

        void Init();

        void InvokeEvent(string name, string key, string val);

        void InvokeEvents(Ev e);

        SDKInfo GetInfo();
    }
}