
namespace SDKManagement
{
    public enum SDKStatus
    {
        CANCELED = 0,
        WAITING,
        FAILED,
        TEST,
        INITED
    }

    public struct SDKInfo
    {
        public string name;
        public SDKStatus status;
        public string message;

        public SDKInfo(string name, SDKStatus status, string message)
        {
            this.name = name;
            this.status = status;
            this.message = message;
        }
        public SDKInfo(string name, SDKStatus status)
        {
            this.name = name;
            this.status = status;
            message = "UNDEFINED";
        }
        public SDKInfo(string name, string message)
        {
            this.name = name;
            status = SDKStatus.CANCELED;
            this.message = message;
        }
        public SDKInfo(string name)
        {
            this.name = name;
            status = SDKStatus.CANCELED;
            message = "UNDEFINED";
        }

        public override string ToString()
        {
            return string.Format("SDK: {0}, status: {1}, message: '{2}'", name, status, message);
        }
    }
}
