using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace VTVPlaylistEditor.Models
{
    public class ChannelSettingModel : BindableBase
    {
        private ChannelModel channel = null;
        private string dateFormatString = "yyyyMMdd";
        private int countOfPreviousDays = 0;
        private int countOfNextDays = 1;
        private TimeSpan playlistUpdateInterval = TimeSpan.FromSeconds(5);
        private string playlistFolderPath = "";
        private string mergePlaylistFolderPath = "";
        private string olderPlaylistFolderPath = "";
        private TimeSpan minimumPrimaryEventDuration = TimeSpan.FromMinutes(6);
        private TimeSpan maximumCountDownDuration = TimeSpan.FromMinutes(10);
        private int maximumStringLength = 60;
        private string customLiveEventContainProgramCodeFilter = "";
        private string customAdvertismentEventContainProgramCodeFilter = "";
        private string customPrimaryEventContainProgramCodeFilter = "";

        private bool trafficEnableDatabaseConnection = false;
        private TimeSpan trafficUpdateInterval = TimeSpan.FromSeconds(5);
        private string trafficFolderPath = "";

        private SecondaryEventModel defaultSecondaryEventClear = null;
        private SecondaryEventModel defaultSecondaryEventClock = null;
        private SecondaryEventModel defaultSecondaryEventNowProgram = null;
        private SecondaryEventModel defaultSecondaryEventNextProgram = null;
        private SecondaryEventModel defaultSecondaryEventCountDown = null;
        private SecondaryEventModel defaultSecondaryEventImageSpecialProgram = null;
        private SecondaryEventModel defaultSecondaryEventVideoSpecialProgram = null;

        private bool comingUpRoundTimeEnabled = false;
        private ComingUpListModel cToday = new ComingUpListModel() { Name = "CToday" };
        private ComingUpListModel cTomorrow = new ComingUpListModel() { Name = "CTomorrow" };

        public string DateFormatString
        {
            get
            {
                return dateFormatString;
            }

            set
            {
                this.dateFormatString = value;
                OnPropertyChanged(() => this.DateFormatString);
            }
        }

        public int CountOfPreviousDays
        {
            get
            {
                return countOfPreviousDays;
            }

            set
            {
                this.countOfPreviousDays = Math.Abs(value);
                OnPropertyChanged(() => this.CountOfPreviousDays);
            }
        }

        public int CountOfNextDays
        {
            get
            {
                return countOfNextDays;
            }

            set
            {
                this.countOfNextDays = Math.Abs(value);
                OnPropertyChanged(() => this.CountOfNextDays);
            }
        }

        public TimeSpan PlaylistUpdateInterval
        {
            get
            {
                return playlistUpdateInterval;
            }

            set
            {
                this.playlistUpdateInterval = value;
                OnPropertyChanged(() => this.PlaylistUpdateInterval);
            }
        }

        public TimeSpan TrafficUpdateInterval
        {
            get
            {
                return trafficUpdateInterval;
            }

            set
            {
                this.trafficUpdateInterval = value;
                OnPropertyChanged(() => this.TrafficUpdateInterval);
            }
        }

        public string PlaylistFolderPath
        {
            get
            {
                return playlistFolderPath;
            }

            set
            {
                this.playlistFolderPath = value;
                OnPropertyChanged(() => this.PlaylistFolderPath);
            }
        }

        public string MergePlaylistFolderPath
        {
            get
            {
                return mergePlaylistFolderPath;
            }

            set
            {
                this.mergePlaylistFolderPath = value;
                OnPropertyChanged(() => this.MergePlaylistFolderPath);
            }
        }

        public string OlderPlaylistFolderPath
        {
            get
            {
                return olderPlaylistFolderPath;
            }

            set
            {
                this.olderPlaylistFolderPath = value;
                OnPropertyChanged(() => this.OlderPlaylistFolderPath);
            }
        }

        public string TrafficFolderPath
        {
            get
            {
                return trafficFolderPath;
            }

            set
            {
                this.trafficFolderPath = value;
                OnPropertyChanged(() => this.TrafficFolderPath);
            }
        }

        public TimeSpan MinimumPrimaryEventDuration
        {
            get
            {
                return minimumPrimaryEventDuration;
            }

            set
            {
                this.minimumPrimaryEventDuration = value;
                OnPropertyChanged(() => this.MinimumPrimaryEventDuration);
            }
        }

        public TimeSpan MaximumCountDownDuration
        {
            get
            {
                return maximumCountDownDuration;
            }

            set
            {
                this.maximumCountDownDuration = value;
                OnPropertyChanged(() => this.MaximumCountDownDuration);
            }
        }

        public int MaximumStringLength
        {
            get
            {
                return maximumStringLength;
            }

            set
            {
                this.maximumStringLength = value;
                OnPropertyChanged(() => this.MaximumStringLength);
            }
        }

        public ChannelModel Channel
        {
            get
            {
                return channel;
            }
        }

        public string CustomLiveEventContainProgramCodeFilter
        {
            get
            {
                return customLiveEventContainProgramCodeFilter;
            }

            set
            {
                this.customLiveEventContainProgramCodeFilter = value;
                OnPropertyChanged(() => this.CustomLiveEventContainProgramCodeFilter);
            }
        }

        public SecondaryEventModel DefaultSecondaryEventClear
        {
            get
            {
                return defaultSecondaryEventClear;
            }

            set
            {
                this.defaultSecondaryEventClear = value;
                OnPropertyChanged(() => this.DefaultSecondaryEventClear);
            }
        }

        public SecondaryEventModel DefaultSecondaryEventClock
        {
            get
            {
                return defaultSecondaryEventClock;
            }

            set
            {
                this.defaultSecondaryEventClock = value;
                OnPropertyChanged(() => this.DefaultSecondaryEventClock);
            }
        }

        public SecondaryEventModel DefaultSecondaryEventNowProgram
        {
            get
            {
                return defaultSecondaryEventNowProgram;
            }

            set
            {
                this.defaultSecondaryEventNowProgram = value;
                OnPropertyChanged(() => this.DefaultSecondaryEventNowProgram);
            }
        }

        public SecondaryEventModel DefaultSecondaryEventNextProgram
        {
            get
            {
                return defaultSecondaryEventNextProgram;
            }

            set
            {
                this.defaultSecondaryEventNextProgram = value;
                OnPropertyChanged(() => this.DefaultSecondaryEventNextProgram);
            }
        }

        public SecondaryEventModel DefaultSecondaryEventCountDown
        {
            get
            {
                return defaultSecondaryEventCountDown;
            }

            set
            {
                this.defaultSecondaryEventCountDown = value;
                OnPropertyChanged(() => this.DefaultSecondaryEventCountDown);
            }
        }

        public SecondaryEventModel DefaultSecondaryEventImageSpecialProgram
        {
            get
            {
                return defaultSecondaryEventImageSpecialProgram;
            }

            set
            {
                this.defaultSecondaryEventImageSpecialProgram = value;
                OnPropertyChanged(() => this.DefaultSecondaryEventImageSpecialProgram);
            }
        }

        public SecondaryEventModel DefaultSecondaryEventVideoSpecialProgram
        {
            get
            {
                return defaultSecondaryEventVideoSpecialProgram;
            }

            set
            {
                this.defaultSecondaryEventVideoSpecialProgram = value;
                OnPropertyChanged(() => this.DefaultSecondaryEventVideoSpecialProgram);
            }
        }

        public ComingUpListModel CToday
        {
            get
            {
                return cToday;
            }

            set
            {
                this.cToday = value;
                OnPropertyChanged(() => this.CToday);
            }
        }

        public ComingUpListModel CTomorrow
        {
            get
            {
                return cTomorrow;
            }

            set
            {
                this.cTomorrow = value;
                OnPropertyChanged(() => this.CTomorrow);
            }
        }

        public bool TrafficEnableDatabaseConnection
        {
            get
            {
                return trafficEnableDatabaseConnection;
            }

            set
            {
                this.trafficEnableDatabaseConnection = value;
                OnPropertyChanged(() => this.TrafficEnableDatabaseConnection);
            }
        }

        public bool ComingUpRoundTimeEnabled
        {
            get
            {
                return comingUpRoundTimeEnabled;
            }

            set
            {
                this.comingUpRoundTimeEnabled = value;
                OnPropertyChanged(() => this.ComingUpRoundTimeEnabled);
            }
        }

        public string CustomAdvertismentEventContainProgramCodeFilter
        {
            get
            {
                return customAdvertismentEventContainProgramCodeFilter;
            }

            set
            {
                this.customAdvertismentEventContainProgramCodeFilter = value;
                OnPropertyChanged(() => this.CustomAdvertismentEventContainProgramCodeFilter);
            }
        }

        public string CustomPrimaryEventContainProgramCodeFilter
        {
            get
            {
                return customPrimaryEventContainProgramCodeFilter;
            }

            set
            {
                this.customPrimaryEventContainProgramCodeFilter = value;
                OnPropertyChanged(() => this.CustomPrimaryEventContainProgramCodeFilter);
            }
        }

        public ChannelSettingModel(ChannelModel channel)
        {
            this.channel = channel;
            //OnPropertyChanged(() => this.Channel);

            //this.DefaultSecondaryEventClear = new SecondaryEventModel() { HasOffset = false, HasDuration = false };
            //this.DefaultSecondaryEventClock = new SecondaryEventModel() { HasOffset = false, HasDuration = false };
            //this.DefaultSecondaryEventNowProgram = new SecondaryEventModel() { HasOffset = true, HasDuration = false };
            //this.DefaultSecondaryEventNextProgram = new SecondaryEventModel() { HasOffset = true, HasDuration = true };
            //this.DefaultSecondaryEventCountDown = new SecondaryEventModel() { HasOffset = true, HasDuration = false };
            //this.DefaultSecondaryEventSpecialProgram = new SecondaryEventModel() { HasOffset = false, HasDuration = true };
        }
    }
}
