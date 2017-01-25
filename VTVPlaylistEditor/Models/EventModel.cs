using Common;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Prism.Mvvm;

namespace VTVPlaylistEditor.Models
{
    public class EventModel : BindableBase
    {
        private ChannelModel channel = null;
        private EventModel previousPE = null;
        private EventModel nextPE = null;
        private EventModel previousPENotSameProgramCode = null;
        private EventModel nextPENotSameProgramCode = null;
        private bool isLocking = false;
        private DateTime lockedAt = DateTime.MinValue;
        private UserModel lockedBy = null;
        private EventModel parent = null;

        private ObservableCollection<SecondaryEventModel> secondaryEvents = new ObservableCollection<SecondaryEventModel>();

        private string eventType = "";
        private string stt = "";
        private string eventId = "";
        private string groupName = "";
        private string maBang = "";
        private string description = "";
        private string title = "";
        private string beginTime = "00:00:00:00";
        private string beginTime2 = "00:00:00:00";
        private string duration = "00:00:00:00";
        private string tcVao = "";
        private string tcRa = "";
        private string noiDung = "";
        private string tenMu = "";
        private string tenCt = "";
        private string status = "1";
        private string modify = "";
        //private string 

        private bool isUserPrimaryEvent = true;
        private bool isTemporaryCGEvent = false;
        private bool hasInitializeCommand = false;
        private bool hasCleanupCommand = false;

        private int countDownSecondDuration = 0;

        private bool canShowCG = false;
        private bool canShowNowProgram = false;
        private bool canShowNextProgram = false;
        private bool canShowCountDown = false;
        private bool canShowSpecial = false;

        private bool isPrimaryEvent = false;
        private bool isAdvertismentEvent = false;
        private bool isLiveEvent = false;
        private bool isComingUpEvent = false;
        private bool isReadOnly = false;

        private bool isTimeElapsed = false;
        private bool isOnTime = false;

        private bool isValidTenMuLength = true;
        private bool isValidTenCtLength = true;

        private string cTodayFilePath = "";
        private string cTomorrowFilePath= "";

        public string STT
        {
            get
            {
                return stt;
            }

            set
            {
                this.stt = value;
                OnPropertyChanged(() => this.STT);
            }
        }

        public string EventId
        {
            get
            {
                return eventId;
            }

            set
            {
                this.eventId = value;
                OnPropertyChanged(() => this.EventId);
            }
        }

        public string ProgramCode
        {
            get
            {
                return maBang;
            }

            set
            {
                this.maBang = Utility.ConvertCompositeToPrecomposed(value);
                this.maBang = Utility.ConvertTextToUpper(maBang);
                OnPropertyChanged(() => this.ProgramCode);
            }
        }

        public string BeginTime
        {
            get
            {
                return beginTime;
            }

            set
            {
                this.beginTime = value;
                OnPropertyChanged(() => this.BeginTime);
            }
        }

        public string BeginTime2
        {
            get
            {
                return beginTime2;
            }

            set
            {
                this.beginTime2 = value;
                OnPropertyChanged(() => this.BeginTime2);
            }
        }

        public string Duration
        {
            get
            {
                return duration;
            }

            set
            {
                this.duration = value;
                OnPropertyChanged(() => this.Duration);
            }
        }

        public string TenMu
        {
            get
            {
                return tenMu;
            }

            set
            {
                this.tenMu = Utility.ConvertCompositeToPrecomposed(value);
                this.tenMu = Utility.ConvertToValidString(this.tenMu, AppData.Default.EnableAsciiCharacter, AppData.Default.UserValidCharacter, AppData.Default.ConvertInvalidCharacterTo);
                this.tenMu = Utility.ConvertTextToUpper(this.tenMu);
                OnPropertyChanged(() => this.TenMu);

                if (this.channel != null && this.channel.Setting != null)
                {
                    this.IsValidTenMuLength = Utility.IsValidStringLength(this.TenMu, this.channel.Setting.MaximumStringLength);
                }
            }
        }

        public string TenCt
        {
            get
            {
                return tenCt;
            }

            set
            {
                this.tenCt = Utility.ConvertCompositeToPrecomposed(value);
                this.tenCt = Utility.ConvertToValidString(this.tenCt, AppData.Default.EnableAsciiCharacter, AppData.Default.UserValidCharacter, AppData.Default.ConvertInvalidCharacterTo);
                this.tenCt = Utility.ConvertTextToUpper(this.tenCt);
                OnPropertyChanged(() => this.TenCt);

                if (this.channel != null && this.channel.Setting != null)
                {
                    this.IsValidTenCtLength = Utility.IsValidStringLength(this.TenCt, this.channel.Setting.MaximumStringLength);
                }
            }
        }

        public string EventType
        {
            get
            {
                return eventType;
            }

            set
            {
                this.eventType = value;
                OnPropertyChanged(() => this.EventType);
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                this.description = Utility.ConvertCompositeToPrecomposed(value);
                OnPropertyChanged(() => this.Description);
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                this.title = Utility.ConvertCompositeToPrecomposed(value);
                OnPropertyChanged(() => this.Title);
            }
        }

        public string TcVao
        {
            get
            {
                return tcVao;
            }

            set
            {
                this.tcVao = value;
                OnPropertyChanged(() => this.TcVao);
            }
        }

        public string TcRa
        {
            get
            {
                return tcRa;
            }

            set
            {
                this.tcRa = value;
                OnPropertyChanged(() => this.TcRa);
            }
        }

        public string NoiDung
        {
            get
            {
                return noiDung;
            }

            set
            {
                this.noiDung = Utility.ConvertCompositeToPrecomposed(value);
                OnPropertyChanged(() => this.NoiDung);
            }
        }

        public string Modify
        {
            get
            {
                return modify;
            }

            set
            {
                this.modify = value;
                OnPropertyChanged(() => this.Modify);
            }
        }

        public bool IsPrimaryEvent
        {
            get
            {
                return isPrimaryEvent;
            }

            set
            {
                this.isPrimaryEvent = value;
                OnPropertyChanged(() => this.IsPrimaryEvent);
            }
        }

        public bool IsAdvertismentEvent
        {
            get
            {
                return isAdvertismentEvent;
            }

            set
            {
                this.isAdvertismentEvent = value;
                OnPropertyChanged(() => this.IsAdvertismentEvent);
            }
        }

        public bool IsLiveEvent
        {
            get
            {
                return isLiveEvent;
            }

            set
            {
                this.isLiveEvent = value;
                OnPropertyChanged(() => this.IsLiveEvent);
            }
        }

        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                this.status = value;
                OnPropertyChanged(() => this.Status);
            }
        }

        public bool IsTimeElapsed
        {
            get
            {
                return isTimeElapsed;
            }

            set
            {
                this.isTimeElapsed = value;
                OnPropertyChanged(() => this.IsTimeElapsed);
            }
        }

        public bool IsOnTime
        {
            get
            {
                return isOnTime;
            }

            set
            {
                this.isOnTime = value;
                OnPropertyChanged(() => this.IsOnTime);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }

            set
            {
                this.isReadOnly = value;
                OnPropertyChanged(() => this.IsReadOnly);
            }
        }

        public bool CanShowCG
        {
            get
            {
                return canShowCG;
            }

            set
            {
                this.canShowCG = value;
                OnPropertyChanged(() => this.CanShowCG);

                if (this.CanShowCG == false)
                {
                    this.CanShowNowProgram = false;
                    this.CanShowSpecial = false;
                    this.CanShowNextProgram = false;
                    this.CanShowCountDown = false;

                    if (this.PreviousPENotSameProgramCode != null && this.PreviousPE != null && this.PreviousPE.ProgramCode.Equals(this.PreviousPENotSameProgramCode.ProgramCode))
                    {
                        this.PreviousPENotSameProgramCode.CanShowNextProgram = false;
                        this.PreviousPENotSameProgramCode.CanShowCountDown = false;
                    }
                }
            }
        }

        public bool CanShowNowProgram
        {
            get
            {
                return canShowNowProgram;
            }

            set
            {
                this.canShowNowProgram = value;
                OnPropertyChanged(() => this.CanShowNowProgram);
            }
        }

        public bool CanShowNextProgram
        {
            get
            {
                return canShowNextProgram;
            }

            set
            {
                this.canShowNextProgram = value;
                OnPropertyChanged(() => this.CanShowNextProgram);

                if (this.CanShowNextProgram == false)
                {
                    if (this.NextPENotSameProgramCode != null && this.NextPENotSameProgramCode.CanShowCG == false &&
                        this.PreviousPE != null && this.PreviousPE.ProgramCode.Equals(this.ProgramCode))
                    {
                        this.PreviousPE.CanShowNextProgram = false;
                    }
                }
            }
        }

        public bool CanShowCountDown
        {
            get
            {
                return canShowCountDown;
            }

            set
            {
                this.canShowCountDown = value;
                OnPropertyChanged(() => this.CanShowCountDown);

                if (this.CanShowCountDown == false)
                {
                    if (this.NextPENotSameProgramCode != null && this.NextPENotSameProgramCode.CanShowCG == false &&
                        this.PreviousPE != null && this.PreviousPE.ProgramCode.Equals(this.ProgramCode))
                    {
                        this.PreviousPE.CanShowCountDown = false;
                    }
                }
            }
        }

        public bool CanShowSpecial
        {
            get
            {
                return canShowSpecial;
            }

            set
            {
                this.canShowSpecial = value;
                OnPropertyChanged(() => this.CanShowSpecial);
            }
        }

        public int CountDownSecondDuration
        {
            get
            {
                return countDownSecondDuration;
            }

            set
            {
                this.countDownSecondDuration = value;
                OnPropertyChanged(() => this.CountDownSecondDuration);
            }
        }

        public bool IsUserPrimaryEvent
        {
            get
            {
                return isUserPrimaryEvent;
            }

            set
            {
                this.isUserPrimaryEvent = value;
                OnPropertyChanged(() => this.IsUserPrimaryEvent);
            }
        }

        public bool IsTemporaryCGEvent
        {
            get
            {
                return isTemporaryCGEvent;
            }

            set
            {
                this.isTemporaryCGEvent = value;
                OnPropertyChanged(() => this.IsTemporaryCGEvent);
            }
        }

        public bool HasInitializeCommand
        {
            get
            {
                return hasInitializeCommand;
            }

            set
            {
                this.hasInitializeCommand = value;
                OnPropertyChanged(() => this.HasInitializeCommand);
            }
        }

        public bool HasCleanupCommand
        {
            get
            {
                return hasCleanupCommand;
            }

            set
            {
                this.hasCleanupCommand = value;
                OnPropertyChanged(() => this.HasCleanupCommand);
            }
        }

        public bool IsValidTenMuLength
        {
            get
            {
                return isValidTenMuLength;
            }

            set
            {
                this.isValidTenMuLength = value;
                OnPropertyChanged(() => this.IsValidTenMuLength);
            }
        }

        public bool IsValidTenCtLength
        {
            get
            {
                return isValidTenCtLength;
            }

            set
            {
                this.isValidTenCtLength = value;
                OnPropertyChanged(() => this.IsValidTenCtLength);
            }
        }

        public EventModel PreviousPENotSameProgramCode
        {
            get
            {
                return previousPENotSameProgramCode;
            }

            set
            {
                this.previousPENotSameProgramCode = value;
                OnPropertyChanged(() => this.PreviousPENotSameProgramCode);
            }
        }

        public EventModel NextPENotSameProgramCode
        {
            get
            {
                return nextPENotSameProgramCode;
            }

            set
            {
                this.nextPENotSameProgramCode = value;
                OnPropertyChanged(() => this.NextPENotSameProgramCode);
            }
        }

        public EventModel PreviousPE
        {
            get
            {
                return previousPE;
            }

            set
            {
                this.previousPE = value;
                OnPropertyChanged(() => this.PreviousPE);
            }
        }

        public EventModel NextPE
        {
            get
            {
                return nextPE;
            }

            set
            {
                this.nextPE = value;
                OnPropertyChanged(() => this.NextPE);
            }
        }

        public ObservableCollection<SecondaryEventModel> SecondaryEvents
        {
            get
            {
                return secondaryEvents;
            }
        }

        public bool IsComingUpEvent
        {
            get
            {
                return isComingUpEvent;
            }

            set
            {
                this.isComingUpEvent = value;
                OnPropertyChanged(() => this.IsComingUpEvent);
            }
        }

        public string CTodayFilePath
        {
            get
            {
                return cTodayFilePath;
            }

            set
            {
                this.cTodayFilePath = value;
                OnPropertyChanged(() => this.CTodayFilePath);
            }
        }

        public string CTomorrowFilePath
        {
            get
            {
                return cTomorrowFilePath;
            }

            set
            {
                this.cTomorrowFilePath = value;
                OnPropertyChanged(() => this.CTomorrowFilePath);
            }
        }

        public bool IsLocking
        {
            get
            {
                return isLocking;
            }

            set
            {
                this.isLocking = value;
                OnPropertyChanged(() => this.IsLocking);
            }
        }

        public UserModel LockedBy
        {
            get
            {
                return lockedBy;
            }

            set
            {
                this.lockedBy = value;
                OnPropertyChanged(() => this.LockedBy);
            }
        }

        public DateTime LockedAt
        {
            get
            {
                return lockedAt;
            }

            set
            {
                this.lockedAt = value;
                OnPropertyChanged(() => this.LockedAt);
            }
        }

        public string GroupName
        {
            get
            {
                return groupName;
            }

            set
            {
                groupName = value;
                OnPropertyChanged(() => this.GroupName);
            }
        }

        public EventModel Parent
        {
            get
            {
                return parent;
            }

            set
            {
                parent = value;
                OnPropertyChanged(() => this.Parent);
            }
        }

        public EventModel(ChannelModel channel)
        {
            this.channel = channel;
        }

        public void GetProgramTitleFromDescription(string description, char splitChar)
        {
            if (string.IsNullOrEmpty(description) == false)
            {
                int splitIndex = description.IndexOf(splitChar, 0);
                if (splitIndex > -1)
                {
                    this.TenMu = description.Substring(0, splitIndex);
                    this.TenCt = description.Substring(splitIndex + 1, description.Length - splitIndex - 1);
                }
                else
                {
                    this.TenMu = description;
                }
            }
        }
    }
}
