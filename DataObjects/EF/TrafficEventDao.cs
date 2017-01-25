using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects;
using EntityFramework.BulkInsert.Extensions;

namespace DataObjects.EF
{
    public class TrafficEventDao : ITrafficEventDao
    {
        //private MapperConfiguration _mapperConfig = new MapperConfiguration(cfg =>
        //{
        //    cfg.ShouldMapProperty = p => p.GetMethod.IsPublic;
        //    //cfg.CreateMap<TrafficEventEntity, BusinessObjects.TrafficEvent>().ForMember(dest => dest.Channel, opt => opt.Ignore());
        //    //cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventEntity>().ForMember(dest=>dest.Channel, opt=>opt.Ignore());
        //    //cfg.CreateMap<ChannelEntity, BusinessObjects.Channel>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
        //    //cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
        //    cfg.CreateMap<ChannelEntity, BusinessObjects.Channel>();
        //    cfg.CreateMap<BusinessObjects.Channel, ChannelEntity>();
        //    cfg.CreateMap<TrafficEventEntity, BusinessObjects.TrafficEvent>();
        //    cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventEntity>();
        //});
        private IDaoFactory _daoFactory = null;

        public TrafficEventDao(IDaoFactory daoFactory)
        {
            _daoFactory = daoFactory;
        }

        public void CreateTrafficEvent(BusinessObjects.TrafficEvent trafficEvent)
        {
            //Debug.WriteLine("BEGIN CreateTrafficEvent");
            try
            {
                using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
                {
                    TrafficEventEntity entity = entity = db.TrafficEventEntities.SingleOrDefault(m => m.ChannelName == trafficEvent.ChannelName && m.ProgramCode.Equals(trafficEvent.ProgramCode, StringComparison.OrdinalIgnoreCase));
                    if (entity == null)
                    {
                        trafficEvent.CreateTime = DateTime.Now;

                        //Mapper.Initialize(cfg => cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventEntity>());
                        entity = DaoFactory.Mapper.Map<BusinessObjects.TrafficEvent, TrafficEventEntity>(trafficEvent);
                        entity.ID = Guid.NewGuid();
                        entity.ChannelName = trafficEvent.ChannelName;
                        db.TrafficEventEntities.Add(entity);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR CreateTrafficEvent - " + ex.Message);
                throw ex;
            }
            //Debug.WriteLine("END CreateTrafficEvent");
        }

        public void DeleteTrafficEvent(BusinessObjects.TrafficEvent trafficEvent)
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entity = db.TrafficEventEntities.SingleOrDefault(m => m.ChannelName == trafficEvent.ChannelName && m.ProgramCode.Equals(trafficEvent.ProgramCode, StringComparison.OrdinalIgnoreCase));
                if (entity != null)
                {
                    db.TrafficEventEntities.Remove(entity);
                    db.SaveChanges();
                }
            }
        }

        public BusinessObjects.TrafficEvent GetTrafficEvent(string channelName, string programCode)
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entity = db.TrafficEventEntities.SingleOrDefault(m => m.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase) && m.ProgramCode.Equals(programCode, StringComparison.OrdinalIgnoreCase));
                //Mapper.Initialize(cfg => cfg.CreateMap<TrafficEventEntity, BusinessObjects.TrafficEvent>());
                return DaoFactory.Mapper.Map<TrafficEventEntity, BusinessObjects.TrafficEvent>(entity);
            }
        }

        public List<BusinessObjects.TrafficEvent> GetTrafficEvents(string channelName, string sortExpression = "UpdateTime DESC")
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entities = db.TrafficEventEntities.AsQueryable().Where(m => m.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase)).OrderBy(sortExpression).ToList();
                //Mapper.Initialize(cfg => cfg.CreateMap<TrafficEventEntity, BusinessObjects.TrafficEvent>());
                return DaoFactory.Mapper.Map<ICollection<TrafficEventEntity>, List<BusinessObjects.TrafficEvent>>(entities);
            }
        }

        public List<TrafficEvent> GetTrafficEvents(string channelName, int currentPage, int itemsPerPage, string sortExpression = "UpdateTime DESC")
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entities = db.TrafficEventEntities.AsQueryable().Where(m => m.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase)).OrderBy(sortExpression).Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                return DaoFactory.Mapper.Map<ICollection<TrafficEventEntity>, List<BusinessObjects.TrafficEvent>>(entities);
            }
        }

        public List<TrafficEvent> GetTrafficEvents(string channelName, string searchForProgramCode = "", string sortExpression = "UpdateTime DESC")
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entities = db.TrafficEventEntities.AsQueryable().Where(m => m.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase) && m.ProgramCode.ToLower().Contains(searchForProgramCode.ToLower())).OrderBy(sortExpression).ToList();
                return DaoFactory.Mapper.Map<ICollection<TrafficEventEntity>, List<BusinessObjects.TrafficEvent>>(entities);
            }
        }

        public void UpdateTrafficEvent(BusinessObjects.TrafficEvent trafficEvent)
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                var entity = db.TrafficEventEntities.SingleOrDefault(m => m.ChannelName == trafficEvent.ChannelName && m.ProgramCode.Equals(trafficEvent.ProgramCode, StringComparison.OrdinalIgnoreCase));
                if (entity != null)
                {
                    trafficEvent.CreateTime = entity.CreateTime;

                    //Mapper.Initialize(cfg => cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventEntity>());
                    DaoFactory.Mapper.Map(trafficEvent, entity, typeof(BusinessObjects.TrafficEvent), typeof(TrafficEventEntity));
                    db.SaveChanges();
                }
            }
        }

        public void CreateTrafficEvents(List<TrafficEvent> trafficEvents)
        {
            try
            {
                List<TrafficEventEntity> entities = new List<TrafficEventEntity>();
                foreach (var trafficEvent in trafficEvents)
                {
                    //Mapper.Initialize(cfg => cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventEntity>());
                    TrafficEventEntity entity = DaoFactory.Mapper.Map<BusinessObjects.TrafficEvent, TrafficEventEntity>(trafficEvent);
                    entity.ID = Guid.NewGuid();
                    entity.ChannelName = trafficEvent.ChannelName;
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

        public bool IsTrafficEventExisted(string channelName, string programCode)
        {
            bool isExisted = false;
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                isExisted = db.TrafficEventEntities.Any(m => m.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase) && m.ProgramCode.Equals(programCode, StringComparison.OrdinalIgnoreCase));
            }
            return isExisted;
        }

        public int CountOfTrafficEvents(string channelName)
        {
            using (VTVChannelBrandingEntities db = new VTVChannelBrandingEntities(_daoFactory.GetSwitchableConnectionString()))
            {
                return db.TrafficEventEntities.Count(m => m.ChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
