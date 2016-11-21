using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using Prism.Mvvm;
using VTVTrafficDataManager.Models;
using ChiDuc.General;
using AutoMapper;
using DataService;

namespace VTVTrafficDataManager
{
    public class AppData : BindableBase
    {
        private static AppData _default = null;
        public static AppData Default
        {
            get
            {
                if (_default == null)
                    _default = new AppData();
                return _default;
            }
        }

        public static bool InDesignMode
        {
            get
            {
                return !(Application.Current is App);
            }
        }

        public static readonly string APPLICATION_NAME = "VTVTrafficDataManager";
        public static readonly string APPLICATION_FRIENDLYNAME = "Phần mềm Xử lý dữ liệu Traffic cho Channel branding";
        public static readonly string APPLICATION_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly string APPLICATION_FOLDERPATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string APPSETTING_FILEPATH = Path.Combine(APPLICATION_FOLDERPATH, "AppSetting.xml");

        public static MapperConfiguration MapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapProperty = p => p.GetMethod.IsPublic;
            cfg.CreateMap<TrafficEventModel, BusinessObjects.TrafficEvent>();
            cfg.CreateMap<BusinessObjects.TrafficEvent, TrafficEventModel>();
            cfg.CreateMap<ChannelModel, BusinessObjects.Channel>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
            cfg.CreateMap<BusinessObjects.Channel, ChannelModel>().ForMember(dest => dest.TrafficEvents, opt => opt.Ignore());
            cfg.CreateMap<ConnectionStringModel, DataObjects.ConnectionString>();
            cfg.CreateMap<DataObjects.ConnectionString, ConnectionStringModel>();
        });
        public static IMapper Mapper = MapperConfiguration.CreateMapper();

        private AppSettingModel _appSetting = new AppSettingModel();
        private ObservableCollection<ChannelModel> _channels = new ObservableCollection<ChannelModel>();
        private TaskbarIcon _notifyIcon;

        public string WindowTitle
        {
            get
            {
                return string.Format("{0} - Phiên bản: {1}", APPLICATION_FRIENDLYNAME, APPLICATION_VERSION);
            }
        }

        public TaskbarIcon NotifyIcon
        {
            get
            {
                return _notifyIcon;
            }

            set
            {
                this._notifyIcon = value;
                OnPropertyChanged(() => this.NotifyIcon);
            }
        }

        public ObservableCollection<ChannelModel> Channels
        {
            get
            {
                return _channels;
            }

            set
            {
                this._channels = value;
                OnPropertyChanged(() => this.Channels);
            }
        }

        public AppSettingModel AppSetting
        {
            get
            {
                return _appSetting;
            }
        }

        public AppData()
        {
            if (InDesignMode == false)
            {
                this.AppSetting.LoadCompleted += AppSetting_LoadCompleted;
            }
        }

        private void AppSetting_LoadCompleted(object sender)
        {
            ClearChannels();
            DataService.Service.Default.ConnectionStrings.Clear();

            ConnectionStringModel connectionStringModel = new ConnectionStringModel
            {
                ServerName = this.AppSetting.ConnectionString.ServerName,
                DatabaseName = this.AppSetting.ConnectionString.DatabaseName,
                UserName = this.AppSetting.ConnectionString.UserName,
                Password = this.AppSetting.ConnectionString.Password,
                ConnectionTimeOut = this.AppSetting.ConnectionString.ConnectionTimeOut
            };
            DataService.Service.Default.ConnectionStrings.Add(AppData.Mapper.Map<ConnectionStringModel, DataObjects.ConnectionString>(connectionStringModel));

            foreach (var appSettingChannel in this.AppSetting.Channels)
            {
                bool canBeAdded = true;
                foreach (var channelModel in this.Channels)
                {
                    if (channelModel.Name.Equals(appSettingChannel.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        canBeAdded = false;
                        break;
                    }
                }

                if (canBeAdded)
                {
                    ChannelModel channelModel = null;
                    BusinessObjects.Channel channel = DataService.Service.Default.GetChannel(appSettingChannel.Name);
                    if (channel != null)
                    {
                        channelModel = AppData.Mapper.Map<BusinessObjects.Channel, ChannelModel>(channel);
                    }
                    else
                    {
                        channelModel = new ChannelModel { Name = appSettingChannel.Name, LastTrafficUpdateFileTime = DateTime.Now.Date.AddYears(-1) };
                    }
                    channelModel.Title = appSettingChannel.Title;

                    channelModel.TrafficUpdateInterval = appSettingChannel.TrafficUpdateInterval;
                    channelModel.TrafficFolderPath = appSettingChannel.TrafficFolderPath;
                    channelModel.TrafficFileFilter = appSettingChannel.TrafficFileFilter;

                    //channelModel.LastTrafficUpdateFileTime = DateTime.Now.Date.AddYears(-1); //đặt cái này để mỗi lần mở lại chương trình sẽ duyệt lại toàn bộ file, tránh xảy ra thiếu sót dữ liệu

                    //if (channel == null)
                    //{
                    //    channel = Mapper.Map<ChannelModel, BusinessObjects.Channel>(channelModel);
                    //    DataService.Service.Default.CreateChannel(channel);
                    //}

                    this.Channels.Add(channelModel);
                }
            }

            List<BusinessObjects.Channel> channelEntities = AppData.Mapper.Map<ObservableCollection<ChannelModel>, List<BusinessObjects.Channel>>(this.Channels);
            DataService.Service.Default.CreateChannels(channelEntities);

            SortChannels();
        }

        public void ClearChannels()
        {
            foreach (var channel in this.Channels)
            {
                channel.StopTrafficProcessor();
            }
            this.Channels.Clear();
            GC.Collect();
        }

        public void SortChannels()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(this.Channels);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }
    }
}
