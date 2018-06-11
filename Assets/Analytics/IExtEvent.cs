using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public interface IExtEvent
    {
        Dictionary<string, string> GetAllParams();
    }
}