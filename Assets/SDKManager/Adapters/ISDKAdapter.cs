namespace SDKManagement
{
    public interface ISDKAdapter
    {
        /// <summary>
        /// Инициализировать SDK
        /// </summary>
        void Init(string name);
        /// <summary>
        /// Информация об SDK: name, status, message
        /// </summary>
        SDKInfo info
        {
            get;
        }
    }
}