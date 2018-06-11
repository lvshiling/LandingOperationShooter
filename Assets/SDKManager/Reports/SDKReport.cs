using System.Collections.Generic;

namespace SDKManagement
{
    public class SDKReport 
    {
        public bool OrderByStatus { get; private set; }

        public List<SDKInfo> infos {
            get; private set;
        }

        public SDKReport() {
            OrderByStatus = true;
            infos = new List<SDKInfo>();
        }

        public SDKReport(bool orderByStatus)
        {
            OrderByStatus = orderByStatus;
            infos = new List<SDKInfo>();
        }

        public SDKReport(List<SDKInfo> infos)
        {
            OrderByStatus = true;
            if (infos != null)
            {
                this.infos = infos;
                if (OrderByStatus)
                {
                    infos.Sort(SDKInfoComparer);
                }
            }
            else
            {
                this.infos = new List<SDKInfo>();
            }
        }

        public SDKReport(List<SDKInfo> infos, bool orderByStatus)
        {
            OrderByStatus = orderByStatus;
            if (infos != null)
            {
                this.infos = infos;
                if (OrderByStatus)
                {
                    infos.Sort(SDKInfoComparer);
                }
            }
            else
            {
                this.infos = new List<SDKInfo>();
            }
        }

        public void Add(SDKInfo info) {
            infos.Add(info);
            if (OrderByStatus)
            {
                infos.Sort(SDKInfoComparer);
            }
        }

        private int SDKInfoComparer(SDKInfo x, SDKInfo y)
        {
            if (x.status == y.status) return x.name.CompareTo(y.name);
            if (x.status > y.status) return -1;
            return 1;
        }
    }
}