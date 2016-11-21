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
using VTVPlaylistEditor.Models;
using ChiDuc.General;
using AutoMapper;
using DataService;
using Common.Models;

namespace VTVPlaylistEditor
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

        public static readonly string APPLICATION_NAME = "VTVPlaylistEditor";
        public static readonly string APPLICATION_FRIENDLYNAME = "Phần mềm Biên tập chỉ dẫn cho hệ thống Channel branding";
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

        private Guid _sessionId = Guid.NewGuid();
        private int _playlistFilterSelectedIndex = 1;
        private ObservableCollection<ChannelModel> _channels = new ObservableCollection<ChannelModel>();
        private TaskbarIcon _notifyIcon;
        private bool _enableAsciiCharacter = true;
        private string _userValidCharacter = "áàạãảăắằặẵẳâấầậẫẩéèẹẽẻêếềệễểíìịĩỉúùụũủưứừựữửóòọõỏơớờợỡởôốồộỗổýỳỵỹỷđ";
        private string _convertInvalidCharacterTo = "";
        private bool _enableMucImporterProgram = true;
        private TimeSpan _muCImporterProgramDelayInterval = new TimeSpan(0, 0, 3);
        private string _muCImporterProgramFilePath = @"C:\Program Files (x86)\Vizrt\Viz Multichannel\PlayList Importer\PlayListImporter.exe";
        private bool _enableCleaner = true;
        private TimeSpan _autoCleanAtTime = new TimeSpan(0, 30, 0);
        private TimeSpan _trafficUpdateInterval = TimeSpan.FromSeconds(10);
        private ObservableCollection<ConnectionStringModel> _connectionStrings = new ObservableCollection<ConnectionStringModel>();

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

        public bool EnableAsciiCharacter
        {
            get
            {
                return _enableAsciiCharacter;
            }

            set
            {
                this._enableAsciiCharacter = value;
                OnPropertyChanged(() => this.EnableAsciiCharacter);
            }
        }

        public string UserValidCharacter
        {
            get
            {
                return _userValidCharacter;
            }

            set
            {
                this._userValidCharacter = value;
                OnPropertyChanged(() => this.UserValidCharacter);
            }
        }

        public string ConvertInvalidCharacterTo
        {
            get
            {
                return _convertInvalidCharacterTo;
            }

            set
            {
                this._convertInvalidCharacterTo = value;
                OnPropertyChanged(() => this.ConvertInvalidCharacterTo);
            }
        }

        public bool EnableMucImporterProgram
        {
            get
            {
                return _enableMucImporterProgram;
            }

            set
            {
                this._enableMucImporterProgram = value;
                OnPropertyChanged(() => this.EnableMucImporterProgram);
            }
        }

        public TimeSpan MuCImporterProgramDelayInterval
        {
            get
            {
                return _muCImporterProgramDelayInterval;
            }

            set
            {
                this._muCImporterProgramDelayInterval = value;
                OnPropertyChanged(() => this.MuCImporterProgramDelayInterval);
            }
        }

        public string MuCImporterProgramFilePath
        {
            get
            {
                return _muCImporterProgramFilePath;
            }

            set
            {
                this._muCImporterProgramFilePath = value;
                OnPropertyChanged(() => this.MuCImporterProgramFilePath);
            }
        }

        public bool EnableCleaner
        {
            get
            {
                return _enableCleaner;
            }

            set
            {
                this._enableCleaner = value;
                OnPropertyChanged(() => this.EnableCleaner);
            }
        }

        public TimeSpan AutoCleanAtTime
        {
            get
            {
                return _autoCleanAtTime;
            }

            set
            {
                this._autoCleanAtTime = value;
                OnPropertyChanged(() => this.AutoCleanAtTime);
            }
        }

        public TimeSpan TrafficUpdateInterval
        {
            get
            {
                return _trafficUpdateInterval;
            }

            set
            {
                this._trafficUpdateInterval = value;
                OnPropertyChanged(() => this.TrafficUpdateInterval);
            }
        }

        public int PlaylistFilterSelectedIndex
        {
            get
            {
                return _playlistFilterSelectedIndex;
            }

            set
            {
                this._playlistFilterSelectedIndex = value;
                OnPropertyChanged(() => this.PlaylistFilterSelectedIndex);
            }
        }

        public Guid SessionId
        {
            get
            {
                return _sessionId;
            }

            set
            {
                this._sessionId = value;
                OnPropertyChanged(() => this.SessionId);
            }
        }

        public ObservableCollection<ConnectionStringModel> ConnectionStrings
        {
            get
            {
                return _connectionStrings;
            }

            set
            {
                this._connectionStrings = value;
                OnPropertyChanged(() => this.ConnectionStrings);
            }
        }

        public AppData()
        {
            if (InDesignMode == false)
            {
                LoadDefaultSetting();
            }
        }

        private void LoadDefaultSetting()
        {

        }

        public void Load()
        {
            try
            {
                if (File.Exists(APPSETTING_FILEPATH) == false)
                    throw new FileNotFoundException();

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(APPSETTING_FILEPATH);

                //XmlNodeList nodeUsers = xmlDocument.SelectNodes("/AppSetting/Users/User");
                //this.Users.Clear();
                //foreach (var item in nodeUsers)
                //{
                //    XmlElement node = item as XmlElement;
                //    UserModel userModel = new UserModel
                //    {
                //        Roles = node.GetAttribute("Roles"),
                //        UserName = node.GetAttribute("UserName"),
                //        Password = node.GetAttribute("Password")
                //    };
                //    this.Users.Add(userModel);
                //}

                XmlElement nodeGeneral = (XmlElement)xmlDocument.SelectSingleNode("/AppSetting/General");
                XmlNodeList nodeGeneralList = nodeGeneral.ChildNodes;
                foreach (var item in nodeGeneralList)
                {
                    XmlElement node = item as XmlElement;
                    switch (node.Name)
                    {
                        //case "Application":
                        //    if (node.HasAttribute("Type"))
                        //    {
                        //        this.ApplicationType = node.GetAttribute("Type");
                        //        OnPropertyChanged(() => this.IsMasterMode);
                        //        OnPropertyChanged(() => this.WindowTitle);
                        //    }
                        //    if (node.HasAttribute("HostName"))
                        //    {
                        //        this.ApplicationHostName = node.GetAttribute("HostName");
                        //    }
                        //    if (node.HasAttribute("HostPort"))
                        //    {
                        //        this.ApplicationHostPort = Convert.ToInt32(node.GetAttribute("HostPort"));
                        //    }
                        //    if (this.IsMasterMode)
                        //    {
                        //        this.CurrentUser = new UserModel { UserName = "MASTER" };
                        //        if (this.ScsServer != null)
                        //        {
                        //            this.ScsServer.Stop();
                        //        }
                        //        this.ScsServer = ScsServerFactory.CreateServer(new ScsTcpEndPoint(this.ApplicationHostPort));
                        //    }
                        //    else
                        //    {
                        //        this.CurrentUser = new UserModel { UserName = "CLIENT_TEST" };
                        //        if (this.ScsClient != null)
                        //        {
                        //            this.ScsClient.Disconnect();
                        //        }
                        //        this.ScsClient = ScsClientFactory.CreateClient(new ScsTcpEndPoint(this.ApplicationHostName, this.ApplicationHostPort));
                        //    }
                        //    break;
                        case "CharacterFilter":
                            this.EnableAsciiCharacter = Convert.ToBoolean(node.GetAttribute("EnableAsciiCharacter"));
                            this.UserValidCharacter = node.GetAttribute("UserValidCharacter");
                            this.ConvertInvalidCharacterTo = node.GetAttribute("ConvertInvalidCharacterTo");
                            break;
                        case "MuCImporterProgram":
                            this.EnableMucImporterProgram = Convert.ToBoolean(node.GetAttribute("Enable"));
                            this.MuCImporterProgramDelayInterval = Common.Utility.GetTimeSpanFromString(node.GetAttribute("DelayInterval"));
                            this.MuCImporterProgramFilePath = node.GetAttribute("FilePath");
                            break;
                        case "Cleaner":
                            this.EnableCleaner = Convert.ToBoolean(node.GetAttribute("Enable"));
                            this.AutoCleanAtTime = Common.Utility.GetTimeSpanFromString(node.GetAttribute("AutoCleanAtTime"));
                            break;
                        //case "DatabaseConnection":
                        //    this.DbServer = node.GetAttribute("Server");
                        //    this.DbName = node.GetAttribute("DatabaseName");
                        //    this.DbUserName = node.GetAttribute("UserName");
                        //    this.DbPassword = node.GetAttribute("Password");
                        //    break;
                        case "TrafficDataBroswer":
                            this.TrafficUpdateInterval = Common.Utility.GetTimeSpanFromString(node.GetAttribute("UpdateInterval"));
                            break;
                        case "ConnectionStrings":
                            XmlNodeList nodeConnectionStrings = node.ChildNodes;
                            this.ConnectionStrings.Clear();
                            DataService.Service.Default.ConnectionStrings.Clear();
                            foreach (var nodeConnectionStringItem in nodeConnectionStrings)
                            {
                                XmlElement nodeConnectionString = nodeConnectionStringItem as XmlElement;

                                string serverName = nodeConnectionString.GetAttribute("ServerName");
                                string databaseName = nodeConnectionString.GetAttribute("DatabaseName");
                                string userName = nodeConnectionString.GetAttribute("UserName");
                                string password = nodeConnectionString.GetAttribute("Password");
                                TimeSpan connectionTimeOut = Common.Utility.GetTimeSpanFromString(nodeConnectionString.GetAttribute("ConnectionTimeOut"));

                                ConnectionStringModel connectionStringModel = new ConnectionStringModel { ServerName = serverName, DatabaseName = databaseName, UserName = userName, Password = password, ConnectionTimeOut = connectionTimeOut };
                                this.ConnectionStrings.Add(connectionStringModel);
                                DataService.Service.Default.ConnectionStrings.Add(AppData.Mapper.Map<ConnectionStringModel, DataObjects.ConnectionString>(connectionStringModel));
                            }
                            break;
                    }
                }

                //
                XmlNodeList nodeChannelList = xmlDocument.SelectNodes("/AppSetting/Channels/Channel");
                ClearChannels();

                foreach (var item in nodeChannelList)
                {
                    XmlElement node = item as XmlElement;
                    string channelName = node.GetAttribute("Name");
                    string channelTitle = node.HasAttribute("Title") ? node.GetAttribute("Title") : "";

                    bool canBeAdded = true;
                    foreach (var channelModel in this.Channels)
                    {
                        if (channelModel.Name.Equals(channelName, StringComparison.OrdinalIgnoreCase))
                        {
                            canBeAdded = false;
                            break;
                        }
                    }

                    if (canBeAdded)
                    {
                        ChannelModel channelModel = null;
                        BusinessObjects.Channel channel = DataService.Service.Default.GetChannel(channelName);
                        if (channel != null)
                        {
                            channelModel = AppData.Mapper.Map<BusinessObjects.Channel, ChannelModel>(channel);
                        }
                        else
                        {
                            channelModel = new ChannelModel { Name = channelName, LastTrafficUpdateFileTime = DateTime.Now.Date.AddYears(-1) };
                        }
                        channelModel.Title = channelTitle;

                        if (channel == null)
                        {
                            channel = AppData.Mapper.Map<ChannelModel, BusinessObjects.Channel>(channelModel);
                            DataService.Service.Default.CreateChannel(channel);
                        }

                        XmlNodeList nodeChildList = node.ChildNodes;
                        foreach (var child in nodeChildList)
                        {
                            XmlElement nodeChild = child as XmlElement;
                            string value = nodeChild.HasAttribute("Value") ? nodeChild.GetAttribute("Value") : "";
                            switch (nodeChild.Name)
                            {
                                case "DateFormatString":
                                    channelModel.Setting.DateFormatString = value;
                                    break;
                                case "CountOfPreviousDays":
                                    channelModel.Setting.CountOfPreviousDays = Convert.ToInt32(value);
                                    break;
                                case "CountOfNextDays":
                                    channelModel.Setting.CountOfNextDays = Convert.ToInt32(value);
                                    break;
                                case "PlaylistUpdateInterval":
                                    channelModel.Setting.PlaylistUpdateInterval = Common.Utility.GetTimeSpanFromString(value);
                                    break;
                                case "PlaylistFolderPath":
                                    channelModel.Setting.PlaylistFolderPath = value;
                                    break;
                                case "MergePlaylistFolderPath":
                                    channelModel.Setting.MergePlaylistFolderPath = value;
                                    break;
                                case "OlderPlaylistFolderPath":
                                    channelModel.Setting.OlderPlaylistFolderPath = value;
                                    break;
                                case "MinimumPrimaryEventDuration":
                                    channelModel.Setting.MinimumPrimaryEventDuration = Common.Utility.GetTimeSpanFromString(value);
                                    break;
                                case "MaximumStringLength":
                                    channelModel.Setting.MaximumStringLength = Convert.ToInt32(value);
                                    break;
                                case "MaximumCountDownDuration":
                                    channelModel.Setting.MaximumCountDownDuration = Common.Utility.GetTimeSpanFromString(value);
                                    break;
                                case "CustomPrimaryEventContainProgramCodeFilter":
                                    channelModel.Setting.CustomPrimaryEventContainProgramCodeFilter = value;
                                    break;
                                case "CustomLiveEventContainProgramCodeFilter":
                                    channelModel.Setting.CustomLiveEventContainProgramCodeFilter = value;
                                    break;
                                case "CustomAdvertismentEventContainProgramCodeFilter":
                                    channelModel.Setting.CustomAdvertismentEventContainProgramCodeFilter = value;
                                    break;
                                case "Page":
                                    XmlNodeList nodePageList = nodeChild.ChildNodes;
                                    foreach (XmlNode nodePageListItem in nodePageList)
                                    {
                                        XmlElement nodePageChild = nodePageListItem as XmlElement;
                                        string name = nodePageChild.GetAttribute("Name");
                                        bool hasOffset = nodePageChild.HasAttribute("Offset");
                                        TimeSpan offset = hasOffset ? Common.Utility.GetTimeSpanFromString(nodePageChild.GetAttribute("Offset")) : TimeSpan.Zero;
                                        bool hasDuration = nodePageChild.HasAttribute("Duration");
                                        TimeSpan duration = hasDuration ? Common.Utility.GetTimeSpanFromString(nodePageChild.GetAttribute("Duration")) : TimeSpan.Zero;
                                        switch (nodePageChild.Name)
                                        {
                                            case "Clock":
                                                channelModel.Setting.DefaultSecondaryEventClock = new SecondaryEventModel()
                                                {
                                                    Name = name,
                                                    HasOffset = hasOffset,
                                                    Offset = offset,
                                                    HasDuration = hasDuration,
                                                    Duration = duration
                                                };
                                                break;
                                            case "Clear":
                                                channelModel.Setting.DefaultSecondaryEventClear = new SecondaryEventModel()
                                                {
                                                    Name = name,
                                                    HasOffset = hasOffset,
                                                    Offset = offset,
                                                    HasDuration = hasDuration,
                                                    Duration = duration
                                                };
                                                break;
                                            case "NowProgram":
                                                channelModel.Setting.DefaultSecondaryEventNowProgram = new SecondaryEventModel()
                                                {
                                                    Name = name,
                                                    HasOffset = hasOffset,
                                                    Offset = offset,
                                                    HasDuration = hasDuration,
                                                    Duration = duration
                                                };
                                                break;
                                            case "NextProgram":
                                                channelModel.Setting.DefaultSecondaryEventNextProgram = new SecondaryEventModel()
                                                {
                                                    Name = name,
                                                    HasOffset = hasOffset,
                                                    Offset = offset,
                                                    HasDuration = hasDuration,
                                                    Duration = duration
                                                };
                                                break;
                                            case "CountDown":
                                                channelModel.Setting.DefaultSecondaryEventCountDown = new SecondaryEventModel()
                                                {
                                                    Name = name,
                                                    HasOffset = hasOffset,
                                                    Offset = offset,
                                                    HasDuration = hasDuration,
                                                    Duration = duration
                                                };
                                                break;
                                            case "ImageSpecialProgram":
                                                channelModel.Setting.DefaultSecondaryEventImageSpecialProgram = new SecondaryEventModel()
                                                {
                                                    Name = name,
                                                    HasOffset = hasOffset,
                                                    Offset = offset,
                                                    HasDuration = hasDuration,
                                                    Duration = duration
                                                };
                                                break;
                                            case "VideoSpecialProgram":
                                                channelModel.Setting.DefaultSecondaryEventVideoSpecialProgram = new SecondaryEventModel()
                                                {
                                                    Name = name,
                                                    HasOffset = hasOffset,
                                                    Offset = offset,
                                                    HasDuration = hasDuration,
                                                    Duration = duration
                                                };
                                                break;
                                        }
                                    }
                                    break;
                                case "ComingUp":
                                    if (nodeChild.HasAttribute("EnableRoundTime"))
                                    {
                                        bool comingUpRoundTimeEnabled = false;
                                        bool.TryParse(nodeChild.GetAttribute("EnableRoundTime"), out comingUpRoundTimeEnabled);
                                        channelModel.Setting.ComingUpRoundTimeEnabled = comingUpRoundTimeEnabled;
                                    }

                                    XmlNodeList nodeComingUpList = nodeChild.ChildNodes;
                                    foreach (XmlNode nodeComingUpListItem in nodeComingUpList)
                                    {
                                        XmlNodeList nodeComingUpItems = nodeComingUpListItem.ChildNodes;
                                        foreach (XmlNode comingUpItem in nodeComingUpItems)
                                        {
                                            XmlElement nodeComingUpItem = comingUpItem as XmlElement;
                                            string itemName = nodeComingUpItem.GetAttribute("Name");
                                            string itemProgramCode = nodeComingUpItem.GetAttribute("ProgramCode");
                                            bool itemHasBeginTime = nodeComingUpItem.HasAttribute("BeginTime");
                                            bool itemHasEndTime = nodeComingUpItem.HasAttribute("EndTime");
                                            bool itemHasCount = nodeComingUpItem.HasAttribute("Count");
                                            TimeSpan itemBeginTime = itemHasBeginTime ? Common.Utility.GetTimeSpanFromString(nodeComingUpItem.GetAttribute("BeginTime")) : TimeSpan.Zero;
                                            TimeSpan itemEndTime = itemHasEndTime ? Common.Utility.GetTimeSpanFromString(nodeComingUpItem.GetAttribute("EndTime")) : TimeSpan.Zero;
                                            int itemCount = itemHasCount ? Convert.ToInt32(nodeComingUpItem.GetAttribute("Count")) : 0;
                                            switch (nodeComingUpListItem.Name)
                                            {
                                                case "CToday":
                                                    //channelModel.Setting.CToday.Name = "CToday";
                                                    if (nodeComingUpItem.Name == "Block")
                                                    {
                                                        if (channelModel.Setting.CToday.GetBlockByName(itemName) == null)
                                                        {
                                                            channelModel.Setting.CToday.Blocks.Add(new ComingUpBlockModel()
                                                            {
                                                                Name = itemName,
                                                                ProgramCode = itemProgramCode,
                                                                HasBeginTime = itemHasBeginTime,
                                                                BeginTime = itemBeginTime,
                                                                EndTime = itemEndTime
                                                            });
                                                        }
                                                    }
                                                    else if (nodeComingUpItem.Name == "TimeRange")
                                                    {
                                                        if (channelModel.Setting.CToday.GetTimeRangeByName(itemName) == null)
                                                        {
                                                            channelModel.Setting.CToday.TimeRanges.Add(new ComingUpTimeRangeModel()
                                                            {
                                                                Name = itemName,
                                                                ProgramCode = itemProgramCode,
                                                                EndTime = itemEndTime,
                                                                Count = itemCount,
                                                                HasEndTime = itemHasEndTime,
                                                                HasCount = itemHasCount
                                                            });
                                                        }
                                                    }
                                                    break;
                                                case "CTomorrow":
                                                    //channelModel.Setting.CToday.Name = "CTomorrow";
                                                    if (nodeComingUpItem.Name == "Block")
                                                    {
                                                        if (channelModel.Setting.CTomorrow.GetBlockByName(itemName) == null)
                                                        {
                                                            channelModel.Setting.CTomorrow.Blocks.Add(new ComingUpBlockModel()
                                                            {
                                                                Name = itemName,
                                                                ProgramCode = itemProgramCode,
                                                                HasBeginTime = itemHasBeginTime,
                                                                BeginTime = itemBeginTime,
                                                                EndTime = itemEndTime
                                                            });
                                                        }
                                                    }
                                                    else if (nodeComingUpItem.Name == "TimeRange")
                                                    {
                                                        if (channelModel.Setting.CTomorrow.GetTimeRangeByName(itemName) == null)
                                                        {
                                                            channelModel.Setting.CTomorrow.TimeRanges.Add(new ComingUpTimeRangeModel()
                                                            {
                                                                Name = itemName,
                                                                ProgramCode = itemProgramCode,
                                                                EndTime = itemEndTime,
                                                                Count = itemCount,
                                                                HasEndTime = itemHasEndTime,
                                                                HasCount = itemHasCount
                                                            });
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }

                        this.Channels.Add(channelModel);
                    }
                }

                SortChannels();

                Debug.WriteLine("AppSetting is loaded.");
            }
            catch (FileNotFoundException fnfe)
            {
                LoadDefaultSetting();
                Save();
                Load();
            }
            catch (Exception)
            {
                //throw;
            }
        }

        public void ClearChannels()
        {
            if (this.Channels != null)
            {
                for (int i = this.Channels.Count - 1; i >= 0; i--)
                {
                    this.Channels[i].StopTrafficProcessor();
                    this.Channels[i].StopDataProcessor();
                    this.Channels.RemoveAt(i);
                }
            }
        }

        public void Load(AppData anotherAppSetting, bool isOnlySetting)
        {
            if (anotherAppSetting != null)
            {
                //foreach (UserModel anotherUserModel in anotherAppSetting.Users)
                //{
                //    UserModel userModel = null;
                //    bool isExisted = false;
                //    for (int i = 0; i < this.Users.Count; i++)
                //    {
                //        if (this.Users[i].UserName.Equals(anotherUserModel.UserName, StringComparison.OrdinalIgnoreCase))
                //        {
                //            userModel = this.Users[i];
                //            isExisted = true;
                //            break;
                //        }
                //    }

                //    if (isExisted == false)
                //    {
                //        userModel = new UserModel();
                //    }

                //    userModel.Roles = anotherUserModel.Roles;
                //    userModel.UserName = anotherUserModel.UserName;
                //    userModel.Password = anotherUserModel.Password;

                //    if (isExisted == false)
                //    {
                //        this.Users.Add(userModel);
                //    }
                //}

                foreach (ChannelModel anotherChannelModel in anotherAppSetting.Channels)
                {
                    ChannelModel channelModel = null;
                    bool isExisted = false;
                    for (int i = 0; i < this.Channels.Count; i++)
                    {
                        if (anotherChannelModel.Name.Equals(this.Channels[i].Name, StringComparison.OrdinalIgnoreCase))
                        {
                            channelModel = this.Channels[i];
                            isExisted = true;
                            break;
                        }
                    }

                    if (isExisted == false)
                    {
                        channelModel = new ChannelModel();
                    }

                    channelModel.Name = anotherChannelModel.Name;
                    channelModel.LastTrafficUpdateFilePath = anotherChannelModel.LastTrafficUpdateFilePath;
                    channelModel.LastTrafficUpdateFileTime = anotherChannelModel.LastTrafficUpdateFileTime;

                    if (isExisted == false)
                    {
                        if (isOnlySetting == false)
                        {
                            channelModel.TrafficEvents = anotherChannelModel.TrafficEvents;
                        }
                        this.Channels.Add(channelModel);
                    }

                    if (isOnlySetting == false)
                    {
                        channelModel.StartDataProcessor();
                        channelModel.StartTrafficProcessor();
                    }
                }

                if (isOnlySetting == false)
                {
                    for (int i = 0; i < this.Channels.Count; i++)
                    {
                        bool isExisted = false;
                        for (int j = 0; j < anotherAppSetting.Channels.Count; j++)
                        {
                            if (anotherAppSetting.Channels[j].Name.Equals(this.Channels[i].Name, StringComparison.OrdinalIgnoreCase))
                            {
                                isExisted = true;
                                break;
                            }
                        }

                        if (isExisted == false)
                        {
                            this.Channels[i].StopTrafficProcessor();
                            this.Channels.RemoveAt(i);
                            --i;
                        }
                    }
                }

                SortChannels();
            }
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

        public void Save()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement nodeRoot = xmlDocument.CreateElement("AppSetting");
                xmlDocument.AppendChild(nodeRoot);

                //XmlElement nodeUsers = xmlDocument.CreateElement("Users");
                //nodeRoot.AppendChild(nodeUsers);
                //foreach (UserModel userModel in this.Users)
                //{
                //    XmlElement nodeUser = xmlDocument.CreateElement("User");
                //    nodeUser.SetAttribute("Roles", userModel.Roles);
                //    nodeUser.SetAttribute("UserName", userModel.UserName);
                //    nodeUser.SetAttribute("Password", userModel.Password);
                //    nodeUsers.AppendChild(nodeUser);
                //}

                XmlElement nodeGeneral = xmlDocument.CreateElement("General");
                nodeRoot.AppendChild(nodeGeneral);

                //XmlElement nodeApplication = xmlDocument.CreateElement("Application");
                //nodeApplication.SetAttribute("Type", this.ApplicationType);
                //nodeApplication.SetAttribute("HostName", this.ApplicationHostName);
                //nodeApplication.SetAttribute("HostPort", this.ApplicationHostPort.ToString());
                //nodeGeneral.AppendChild(nodeApplication);

                XmlElement nodeCharacterFilter = xmlDocument.CreateElement("CharacterFilter");
                nodeCharacterFilter.SetAttribute("EnableAsciiCharacter", this.EnableAsciiCharacter.ToString());
                nodeCharacterFilter.SetAttribute("UserValidCharacter", this.UserValidCharacter);
                nodeCharacterFilter.SetAttribute("ConvertInvalidCharacterTo", this.ConvertInvalidCharacterTo);
                nodeGeneral.AppendChild(nodeCharacterFilter);

                XmlElement nodeMuCImporterProgram = xmlDocument.CreateElement("MuCImporterProgram");
                nodeMuCImporterProgram.SetAttribute("Enable", this.EnableMucImporterProgram.ToString());
                nodeMuCImporterProgram.SetAttribute("DelayInterval", Common.Utility.GetTimeStringFromTimeSpan(this.MuCImporterProgramDelayInterval));
                nodeMuCImporterProgram.SetAttribute("FilePath", this.MuCImporterProgramFilePath);
                nodeGeneral.AppendChild(nodeMuCImporterProgram);

                XmlElement nodeCleaner = xmlDocument.CreateElement("Cleaner");
                nodeCleaner.SetAttribute("Enable", this.EnableCleaner.ToString());
                nodeCleaner.SetAttribute("AutoCleanAtTime", Common.Utility.GetTimeStringFromTimeSpan(this.AutoCleanAtTime));
                nodeCleaner.SetAttribute("TrafficDataBroswer", Common.Utility.GetTimeStringFromTimeSpan(this.TrafficUpdateInterval));
                nodeGeneral.AppendChild(nodeCleaner);

                //XmlElement nodeDatabase = xmlDocument.CreateElement("DatabaseConnection");
                //nodeDatabase.SetAttribute("Server", this.DbServer);
                //nodeDatabase.SetAttribute("DatabaseName", this.DbName);
                //nodeDatabase.SetAttribute("UserName", this.DbUserName);
                //nodeDatabase.SetAttribute("Password", this.DbPassword);
                //nodeGeneral.AppendChild(nodeDatabase);

                XmlElement nodeChannels = xmlDocument.CreateElement("Channels");
                nodeChannels.SetAttribute("DefaultChannel", "");
                nodeRoot.AppendChild(nodeChannels);

                foreach (ChannelModel channelModel in this.Channels)
                {
                    XmlElement nodeChannel = xmlDocument.CreateElement("Channel");
                    nodeChannel.SetAttribute("Name", channelModel.Name);
                    nodeChannels.AppendChild(nodeChannel);

                    XmlElement nodeChild = null;

                    nodeChild = xmlDocument.CreateElement("DateFormatString");
                    nodeChild.SetAttribute("Value", channelModel.Setting.DateFormatString);
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("CountOfPreviousDays");
                    nodeChild.SetAttribute("Value", channelModel.Setting.CountOfPreviousDays.ToString());
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("CountOfNextDays");
                    nodeChild.SetAttribute("Value", channelModel.Setting.CountOfNextDays.ToString());
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("PlaylistUpdateInterval");
                    nodeChild.SetAttribute("Value", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.PlaylistUpdateInterval));
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("PlaylistFolderPath");
                    nodeChild.SetAttribute("Value", channelModel.Setting.PlaylistFolderPath);
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("OlderPlaylistFolderPath");
                    nodeChild.SetAttribute("Value", channelModel.Setting.OlderPlaylistFolderPath);
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("MergePlaylistFolderPath");
                    nodeChild.SetAttribute("Value", channelModel.Setting.MergePlaylistFolderPath);
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("MaximumStringLength");
                    nodeChild.SetAttribute("Value", channelModel.Setting.MaximumStringLength.ToString());
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("MinimumPrimaryEventDuration");
                    nodeChild.SetAttribute("Value", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.MinimumPrimaryEventDuration));
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("MaximumCountDownDuration");
                    nodeChild.SetAttribute("Value", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.MaximumCountDownDuration));
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("CustomPrimaryEventContainProgramCodeFilter");
                    nodeChild.SetAttribute("Value", channelModel.Setting.CustomPrimaryEventContainProgramCodeFilter);
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("CustomLiveEventContainProgramCodeFilter");
                    nodeChild.SetAttribute("Value", channelModel.Setting.CustomLiveEventContainProgramCodeFilter);
                    nodeChannel.AppendChild(nodeChild);

                    nodeChild = xmlDocument.CreateElement("CustomAdvertismentEventContainProgramCodeFilter");
                    nodeChild.SetAttribute("Value", channelModel.Setting.CustomAdvertismentEventContainProgramCodeFilter);
                    nodeChannel.AppendChild(nodeChild);

                    XmlElement nodeSubChild = null;

                    //Page
                    nodeChild = xmlDocument.CreateElement("Page");
                    nodeChannel.AppendChild(nodeChild);

                    nodeSubChild = xmlDocument.CreateElement("Clock");
                    nodeSubChild.SetAttribute("Name", channelModel.Setting.DefaultSecondaryEventClock.Name);
                    nodeChild.AppendChild(nodeSubChild);

                    nodeSubChild = xmlDocument.CreateElement("Clear");
                    nodeSubChild.SetAttribute("Name", channelModel.Setting.DefaultSecondaryEventClear.Name);
                    nodeChild.AppendChild(nodeSubChild);

                    nodeSubChild = xmlDocument.CreateElement("NowProgram");
                    nodeSubChild.SetAttribute("Name", channelModel.Setting.DefaultSecondaryEventNowProgram.Name);
                    if (channelModel.Setting.DefaultSecondaryEventNowProgram.HasOffset)
                        nodeSubChild.SetAttribute("Offset", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventNowProgram.Offset));
                    if (channelModel.Setting.DefaultSecondaryEventNowProgram.HasDuration)
                        nodeSubChild.SetAttribute("Duration", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventNowProgram.Duration));
                    nodeChild.AppendChild(nodeSubChild);

                    nodeSubChild = xmlDocument.CreateElement("NextProgram");
                    nodeSubChild.SetAttribute("Name", channelModel.Setting.DefaultSecondaryEventNextProgram.Name);
                    if (channelModel.Setting.DefaultSecondaryEventNextProgram.HasOffset)
                        nodeSubChild.SetAttribute("Offset", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventNextProgram.Offset));
                    if (channelModel.Setting.DefaultSecondaryEventNextProgram.HasDuration)
                        nodeSubChild.SetAttribute("Duration", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventNextProgram.Duration));
                    nodeChild.AppendChild(nodeSubChild);

                    nodeSubChild = xmlDocument.CreateElement("CountDown");
                    nodeSubChild.SetAttribute("Name", channelModel.Setting.DefaultSecondaryEventCountDown.Name);
                    if (channelModel.Setting.DefaultSecondaryEventCountDown.HasOffset)
                        nodeSubChild.SetAttribute("Offset", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventCountDown.Offset));
                    if (channelModel.Setting.DefaultSecondaryEventCountDown.HasDuration)
                        nodeSubChild.SetAttribute("Duration", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventCountDown.Duration));
                    nodeChild.AppendChild(nodeSubChild);

                    nodeSubChild = xmlDocument.CreateElement("ImageSpecialProgram");
                    nodeSubChild.SetAttribute("Name", channelModel.Setting.DefaultSecondaryEventImageSpecialProgram.Name);
                    if (channelModel.Setting.DefaultSecondaryEventImageSpecialProgram.HasOffset)
                        nodeSubChild.SetAttribute("Offset", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventImageSpecialProgram.Offset));
                    if (channelModel.Setting.DefaultSecondaryEventImageSpecialProgram.HasDuration)
                        nodeSubChild.SetAttribute("Duration", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventImageSpecialProgram.Duration));
                    nodeChild.AppendChild(nodeSubChild);

                    nodeSubChild = xmlDocument.CreateElement("VideoSpecialProgram");
                    nodeSubChild.SetAttribute("Name", channelModel.Setting.DefaultSecondaryEventVideoSpecialProgram.Name);
                    if (channelModel.Setting.DefaultSecondaryEventVideoSpecialProgram.HasOffset)
                        nodeSubChild.SetAttribute("Offset", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventVideoSpecialProgram.Offset));
                    if (channelModel.Setting.DefaultSecondaryEventVideoSpecialProgram.HasDuration)
                        nodeSubChild.SetAttribute("Duration", Common.Utility.GetTimeStringFromTimeSpan(channelModel.Setting.DefaultSecondaryEventVideoSpecialProgram.Duration));
                    nodeChild.AppendChild(nodeSubChild);
                    ///

                    //ComingUp
                    nodeChild = xmlDocument.CreateElement("ComingUp");
                    nodeChild.SetAttribute("EnableRoundTime", channelModel.Setting.ComingUpRoundTimeEnabled.ToString());
                    nodeChannel.AppendChild(nodeChild);

                    //CToday
                    nodeSubChild = xmlDocument.CreateElement("CToday");
                    nodeChild.AppendChild(nodeSubChild);

                    foreach (ComingUpBlockModel item in channelModel.Setting.CToday.Blocks)
                    {
                        XmlElement nodeBlock = xmlDocument.CreateElement("Block");
                        nodeBlock.SetAttribute("Name", item.Name);
                        nodeBlock.SetAttribute("ProgramCode", item.ProgramCode);
                        if (item.HasBeginTime)
                            nodeBlock.SetAttribute("BeginTime", Common.Utility.GetTimeStringFromTimeSpan(item.BeginTime));
                        nodeBlock.SetAttribute("EndTime", Common.Utility.GetTimeStringFromTimeSpan(item.EndTime));
                        nodeSubChild.AppendChild(nodeBlock);
                    }

                    foreach (ComingUpTimeRangeModel item in channelModel.Setting.CToday.TimeRanges)
                    {
                        XmlElement nodeTimeRange = xmlDocument.CreateElement("TimeRange");
                        nodeTimeRange.SetAttribute("Name", item.Name);
                        nodeTimeRange.SetAttribute("ProgramCode", item.ProgramCode);
                        if (item.HasEndTime)
                            nodeSubChild.SetAttribute("EndTime", Common.Utility.GetTimeStringFromTimeSpan(item.EndTime));
                        if (item.HasCount)
                            nodeSubChild.SetAttribute("Count", item.Count.ToString());
                        nodeSubChild.AppendChild(nodeTimeRange);
                    }

                    //CTomorrow
                    nodeSubChild = xmlDocument.CreateElement("CTomorrow");
                    nodeChild.AppendChild(nodeSubChild);

                    foreach (ComingUpBlockModel item in channelModel.Setting.CTomorrow.Blocks)
                    {
                        XmlElement nodeBlock = xmlDocument.CreateElement("Block");
                        nodeBlock.SetAttribute("Name", item.Name);
                        nodeBlock.SetAttribute("ProgramCode", item.ProgramCode);
                        if (item.HasBeginTime)
                            nodeBlock.SetAttribute("BeginTime", Common.Utility.GetTimeStringFromTimeSpan(item.BeginTime));
                        nodeBlock.SetAttribute("EndTime", Common.Utility.GetTimeStringFromTimeSpan(item.EndTime));
                        nodeSubChild.AppendChild(nodeBlock);
                    }

                    foreach (ComingUpTimeRangeModel item in channelModel.Setting.CTomorrow.TimeRanges)
                    {
                        XmlElement nodeTimeRange = xmlDocument.CreateElement("TimeRange");
                        nodeTimeRange.SetAttribute("Name", item.Name);
                        nodeTimeRange.SetAttribute("ProgramCode", item.ProgramCode);
                        if (item.HasEndTime)
                            nodeSubChild.SetAttribute("EndTime", Common.Utility.GetTimeStringFromTimeSpan(item.EndTime));
                        if (item.HasCount)
                            nodeSubChild.SetAttribute("Count", item.Count.ToString());
                        nodeSubChild.AppendChild(nodeTimeRange);
                    }

                    ///
                }

                //Kiểm tra, nếu file đang được dùng bởi ứng dụng khác thì chờ 1 lúc
                while (FileManager.IsFileLocked(new FileInfo(APPSETTING_FILEPATH)))
                {
                    Thread.Sleep(100);
                }

                using (StreamWriter sw = new StreamWriter(APPSETTING_FILEPATH, false, Encoding.UTF8))
                {
                    xmlDocument.Save(sw);
                    sw.Close();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
