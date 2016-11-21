using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService
{
    public interface IService
    {
        List<DataObjects.ConnectionString> ConnectionStrings { get; }
        bool IsServerConnected { get; }
        bool CheckConnection(DataObjects.ConnectionString connectionString);
        Task<bool> CheckConnectionAsync(DataObjects.ConnectionString connectionString);
        DataObjects.ConnectionString GetCurrentConnectionString();

        BusinessObjects.Channel GetChannel(string name);
        void CreateChannel(BusinessObjects.Channel channel);
        void CreateChannels(List<BusinessObjects.Channel> channels);
        void UpdateChannel(BusinessObjects.Channel channel);
        void DeleteChannel(BusinessObjects.Channel channel);

        bool IsTrafficEventExisted(string channelName, string programCode);
        BusinessObjects.TrafficEvent GetTrafficEvent(string channelName, string programCode);
        List<BusinessObjects.TrafficEvent> GetTrafficEvents(string channelName, string sortExpression = "UpdateTime DESC");
        void CreateTrafficEvent(BusinessObjects.TrafficEvent trafficEvent);
        void CreateTrafficEvents(List<BusinessObjects.TrafficEvent> trafficEvents);
        void UpdateTrafficEvent(BusinessObjects.TrafficEvent trafficEvent);
        void DeleteTrafficEvent(BusinessObjects.TrafficEvent trafficEvent);
        int CountOfTrafficEvents(string channelName);
    }
}
