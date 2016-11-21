using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects;
using EntityFramework.BulkInsert.Extensions;

namespace DataObjects.EF
{
    public class ChannelDao : IChannelDao
    {
        //private MapperConfiguration _mapperConfig = new MapperConfiguration(cfg =>
        //{
        //    cfg.ShouldMapProperty = p => p.GetMethod.IsPublic;
        //    //cfg.CreateMap<ChannelEntity, BusinessObjects.Channel>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
        //    //cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
        //    cfg.CreateMap<ChannelEntity, BusinessObjects.Channel>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
        //    cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
        //    cfg.CreateMap<TrafficEventEntity, BusinessObjects.TrafficEvent>();
        //    cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventEntity>();
        //});
        private IDaoFactory _daoFactory = null;

        public ChannelDao(IDaoFactory daoFactory)
        {
            _daoFactory = daoFactory;
        }

        public void CreateChannel(BusinessObjects.Channel channel)
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entity = db.ChannelEntities.SingleOrDefault(m => m.Name.Equals(channel.Name, StringComparison.OrdinalIgnoreCase));
                if (entity == null)
                {
                    //Mapper.Initialize(cfg => cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>());
                    entity = DaoFactory.Mapper.Map<BusinessObjects.Channel, ChannelEntity>(channel);
                    db.ChannelEntities.Add(entity);
                    db.SaveChanges();
                }
            }
        }

        public void CreateChannels(List<BusinessObjects.Channel> channels)
        {
            try
            {
                List<ChannelEntity> entities = new List<ChannelEntity>();
                foreach (var channel in channels)
                {
                    //Mapper.Initialize(cfg => cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>());
                    ChannelEntity entity = DaoFactory.Mapper.Map<BusinessObjects.Channel, ChannelEntity>(channel);
                    entities.Add(entity);
                }

                EntityFramework.BulkInsert.ProviderFactory.Register<EntityFramework.BulkInsert.Providers.EfSqlBulkInsertProviderWithMappedDataReader>("System.Data.SqlClient.SqlConnection");
                using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
                {
                    db.BulkInsert(entities);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR CreateTrafficEvents - " + ex.Message);
                //throw ex;
            }
        }

        public void DeleteChannel(BusinessObjects.Channel channel)
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entity = db.ChannelEntities.SingleOrDefault(m => m.Name.Equals(channel.Name, StringComparison.OrdinalIgnoreCase));
                if (entity != null)
                {
                    db.ChannelEntities.Remove(entity);
                    db.SaveChanges();
                }
            }
        }

        public Channel GetChannel(string channelName)
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entity = db.ChannelEntities.SingleOrDefault(m => m.Name == channelName);
                //Mapper.Initialize(cfg => cfg.CreateMap<ChannelEntity, BusinessObjects.Channel>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore()));
                return DaoFactory.Mapper.Map<ChannelEntity, BusinessObjects.Channel>(entity);
            }
        }

        public void UpdateChannel(BusinessObjects.Channel channel)
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entity = db.ChannelEntities.SingleOrDefault(m => m.Name.Equals(channel.Name, StringComparison.OrdinalIgnoreCase));
                if (entity != null)
                {
                    //Mapper.Initialize(cfg => cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>());
                    DaoFactory.Mapper.Map(channel, entity, typeof(BusinessObjects.Channel), typeof(ChannelEntity));
                    //entity.Name = channel.Name;
                    //entity.Description = channel.Description;
                    //entity.LastTrafficFilePath = channel.LastTrafficFilePath;
                    //entity.LastTrafficUpdateTime = channel.LastTrafficUpdateTime;
                    db.SaveChanges();
                }
            }
        }
    }
}
