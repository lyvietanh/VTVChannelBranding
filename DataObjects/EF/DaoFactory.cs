using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace DataObjects.EF
{
    public class DaoFactory : IDaoFactory
    {
        //private static readonly string entityConnectionString = ConfigurationManager.ConnectionStrings["VTVChannelBrandingEntities"].ConnectionString;
        //private static readonly string providerConnectionString = new EntityConnectionStringBuilder(entityConnectionString).ProviderConnectionString;
        private static ConnectionString _currentConnectionString = null;

        public static MapperConfiguration MapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapProperty = p => p.GetMethod.IsPublic;
            //cfg.CreateMap<TrafficEventEntity, BusinessObjects.TrafficEvent>().ForMember(dest => dest.Channel, opt => opt.Ignore());
            //cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventEntity>().ForMember(dest=>dest.Channel, opt=>opt.Ignore());
            //cfg.CreateMap<ChannelEntity, BusinessObjects.Channel>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
            //cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
            cfg.CreateMap<TrafficEventEntity, BusinessObjects.TrafficEvent>();
            cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventEntity>();
            cfg.CreateMap<ChannelEntity, BusinessObjects.Channel>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
            cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
        });
        public static IMapper Mapper = MapperConfiguration.CreateMapper();

        private List<ConnectionString> _connectionStrings = new List<ConnectionString>();

        public List<ConnectionString> ConnectionStrings
        {
            get
            {
                return _connectionStrings;
            }
        }

        //public bool IsServerConnected
        //{
        //    get
        //    {
        //        using (SqlConnection connection = new SqlConnection(providerConnectionString))
        //        {
        //            try
        //            {
        //                connection.Open();
        //                return true;
        //            }
        //            catch (SqlException)
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //}

        public IChannelDao ChannelDao
        {
            get
            {
                return new ChannelDao(this);
            }
        }

        public ITrafficEventDao TrafficEventDao
        {
            get
            {
                return new TrafficEventDao(this);
            }
        }

        public ConnectionString GetSwitchableConnectionString()
        {
            if (_currentConnectionString != null)
            {
                if (this.ConnectionStrings.Any(m => m.ServerName.Equals(_currentConnectionString.ServerName, StringComparison.OrdinalIgnoreCase)) == false || CheckConnection(_currentConnectionString) == false)
                {
                    _currentConnectionString = null;
                }
            }

            if (_currentConnectionString == null)
            {
                for (int i = 0; i < this.ConnectionStrings.Count; i++)
                {
                    if (CheckConnection(this.ConnectionStrings[i]))
                    {
                        _currentConnectionString = this.ConnectionStrings[i];
                        break;
                    }
                }
            }

            return _currentConnectionString;
        }

        public bool CheckConnection(ConnectionString connectionString)
        {
            if (connectionString != null)
            {
                using (SqlConnection connection = new SqlConnection(connectionString.ToString()))
                {
                    try
                    {
                        connection.Open();
                        return true;
                    }
                    catch (SqlException) { }
                }
            }
            return false;
        }
    }
}
