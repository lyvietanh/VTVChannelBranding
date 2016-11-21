using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects
{
    public interface ITrafficEventDao
    {
        BusinessObjects.TrafficEvent GetTrafficEvent(string channelName, string programCode);
        List<BusinessObjects.TrafficEvent> GetTrafficEvents(string channelName, string sortExpression = "UpdateTime DESC");

        bool IsTrafficEventExisted(string channelName, string programCode);
        void CreateTrafficEvent(BusinessObjects.TrafficEvent trafficEvent);
        void CreateTrafficEvents(List<BusinessObjects.TrafficEvent> trafficEvents);
        void UpdateTrafficEvent(BusinessObjects.TrafficEvent trafficEvent);
        void DeleteTrafficEvent(BusinessObjects.TrafficEvent trafficEvent);
        int CountOfTrafficEvents(string channelName);

    }
}
