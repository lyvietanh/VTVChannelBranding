using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects
{
    public interface IChannelDao
    {
        BusinessObjects.Channel GetChannel(string channelName);
        void CreateChannel(BusinessObjects.Channel channel);
        void CreateChannels(List<BusinessObjects.Channel> channels);
        void UpdateChannel(BusinessObjects.Channel channel);
        void DeleteChannel(BusinessObjects.Channel channel);
    }
}
