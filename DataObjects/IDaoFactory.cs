using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects
{
    public interface IDaoFactory
    {
        List<ConnectionString> ConnectionStrings { get; }
        IChannelDao ChannelDao { get; }
        ITrafficEventDao TrafficEventDao { get; }
        ConnectionString GetSwitchableConnectionString();
        bool CheckConnection(ConnectionString connectionString);
    }
}
