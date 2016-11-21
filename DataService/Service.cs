using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using DataObjects;
using DataService.Exceptions;

namespace DataService
{
    public class Service : IService
    {
        private static Service _default = null;
        public static Service Default
        {
            get
            {
                if (_default == null)
                    _default = new Service();
                return _default;
            }
        }

        private static readonly DataObjects.IDaoFactory _daoFactory = DataObjects.DaoFactories.GetFactory("entityframework");
        private static readonly DataObjects.IChannelDao _channelDao = _daoFactory.ChannelDao;
        private static readonly DataObjects.ITrafficEventDao _trafficEventDao = _daoFactory.TrafficEventDao;

        public bool IsServerConnected
        {
            get
            {
                return _daoFactory.GetSwitchableConnectionString() != null;
            }
        }

        public List<DataObjects.ConnectionString> ConnectionStrings
        {
            get
            {
                return _daoFactory.ConnectionStrings;
            }
        }

        public void CreateChannel(Channel channel)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            _channelDao.CreateChannel(channel);
        }

        public void DeleteChannel(Channel channel)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            _channelDao.DeleteChannel(channel);
        }

        public Channel GetChannel(string name)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            var channelEntity = _channelDao.GetChannel(name);
            if (channelEntity != null)
            {
                channelEntity.TrafficEvents = _trafficEventDao.GetTrafficEvents(channelEntity.Name);
            }
            return channelEntity;
        }

        public void UpdateChannel(Channel channel)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            _channelDao.UpdateChannel(channel);
        }

        public TrafficEvent GetTrafficEvent(string channelName, string programCode)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            var trafficEvent = _trafficEventDao.GetTrafficEvent(channelName, programCode);
            //trafficEvent.ChannelName = _channelDao.GetChannel(channelName);
            return trafficEvent;
        }

        public List<TrafficEvent> GetTrafficEvents(string channelName, string sortExpression = "UpdateTime DESC")
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            return _trafficEventDao.GetTrafficEvents(channelName, sortExpression);
        }

        public void CreateTrafficEvent(TrafficEvent trafficEvent)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            _trafficEventDao.CreateTrafficEvent(trafficEvent);
        }

        public void CreateTrafficEvents(List<TrafficEvent> trafficEvents)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            _trafficEventDao.CreateTrafficEvents(trafficEvents);
        }

        public void UpdateTrafficEvent(TrafficEvent trafficEvent)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            _trafficEventDao.UpdateTrafficEvent(trafficEvent);
        }

        public void DeleteTrafficEvent(TrafficEvent trafficEvent)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            _trafficEventDao.DeleteTrafficEvent(trafficEvent);
        }

        public void CreateChannels(List<Channel> channels)
        {
            if (IsServerConnected == false)
                throw new DatabaseServerNotConnectedException();

            _channelDao.CreateChannels(channels);
        }

        public bool CheckConnection(DataObjects.ConnectionString connectionString)
        {
            return _daoFactory.CheckConnection(connectionString);
        }

        public async Task<bool> CheckConnectionAsync(DataObjects.ConnectionString connectionString)
        {
            return await Task.FromResult(_daoFactory.CheckConnection(connectionString));
        }

        public bool IsTrafficEventExisted(string channelName, string programCode)
        {
            return _daoFactory.TrafficEventDao.IsTrafficEventExisted(channelName, programCode);
        }

        public int CountOfTrafficEvents(string channelName)
        {
            return _daoFactory.TrafficEventDao.CountOfTrafficEvents(channelName);
        }

        public ConnectionString GetCurrentConnectionString()
        {
            return _daoFactory.GetSwitchableConnectionString();
        }
    }
}
