using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ChiDuc.General;
using Prism.Mvvm;

namespace VTVPlaylistEditor.Models
{
    public class PlaylistModel : BindableBase
    {
        private ChannelModel _channel = null;
        private DateTime _date = DateTime.Now.Date;
        private ObservableCollection<EventModel> _events = new ObservableCollection<EventModel>();
        private DateTime _lastPlaylistUpdateFileTime = DateTime.MinValue;
        private string _lastPlaylistUpdateFilePath = "";

        private bool _reentrancyCheckCanShowAllCG = false;
        private bool _reentrancyCheckCanShowAllNowProgram = false;
        private bool _reentrancyCheckCanShowAllNextProgram = false;
        private bool _reentrancyCheckCanShowAllCountDown = false;
        private bool? _canShowAllCG = false;
        private bool? _canShowAllNowProgram = false;
        private bool? _canShowAllNextProgram = false;
        private bool? _canShowAllCountDown = false;

        private bool _isProcessingPlaylist = false;

        private Thread _dataProcessorThread = null;
        private bool _isDataProcessorThreadRunning = false;
        private DateTime _lastPlaylistProcessorUpdateTime = DateTime.MinValue;

        public DateTime Date
        {
            get
            {
                return _date;
            }

            set
            {
                this._date = value;
                OnPropertyChanged(() => this.Date);
            }
        }

        public bool HasNewEvent
        {
            get
            {
                for (int i = 0; this.Events != null && i < this.Events.Count; i++)
                {
                    if (this.Events[i].Status == "1")
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public ChannelModel Channel
        {
            get
            {
                return _channel;
            }
        }

        public ObservableCollection<EventModel> Events
        {
            get
            {
                return _events;
            }

            set
            {
                this._events = value;
                OnPropertyChanged(() => this.Events);

                this.Channel.FilterPlaylistSelected();
            }
        }

        public DateTime LastPlaylistUpdateFileTime
        {
            get
            {
                return _lastPlaylistUpdateFileTime;
            }

            set
            {
                this._lastPlaylistUpdateFileTime = value;
                OnPropertyChanged(() => this.LastPlaylistUpdateFileTime);

                this.Channel.FilterPlaylistSelected();
            }
        }

        public string LastPlaylistUpdateFilePath
        {
            get
            {
                return _lastPlaylistUpdateFilePath;
            }

            set
            {
                this._lastPlaylistUpdateFilePath = value;
                OnPropertyChanged(() => this.LastPlaylistUpdateFilePath);
            }
        }

        public bool? CanShowAllCG
        {
            get
            {
                return _canShowAllCG;
            }

            set
            {
                if (this._canShowAllCG != value)
                {
                    if (this._reentrancyCheckCanShowAllCG)
                        return;
                    this._reentrancyCheckCanShowAllCG = true;
                    this._canShowAllCG = value;
                    OnPropertyChanged(() => this.CanShowAllCG);
                    UpdateCheckState();
                    this._reentrancyCheckCanShowAllCG = false;
                }

                if (this.CanShowAllCG == false)
                {
                    this.CanShowAllNowProgram = false;
                    this.CanShowAllNextProgram = false;
                    this.CanShowAllCountDown = false;
                }
            }
        }

        public bool? CanShowAllNowProgram
        {
            get
            {
                return _canShowAllNowProgram;
            }

            set
            {
                if (this._canShowAllNowProgram != value)
                {
                    if (this._reentrancyCheckCanShowAllNowProgram)
                        return;
                    this._reentrancyCheckCanShowAllNowProgram = true;
                    this._canShowAllNowProgram = value;
                    OnPropertyChanged(() => this.CanShowAllNowProgram);
                    UpdateCheckState();
                    this._reentrancyCheckCanShowAllNowProgram = false;
                }
            }
        }

        public bool? CanShowAllNextProgram
        {
            get
            {
                return _canShowAllNextProgram;
            }

            set
            {
                if (this._canShowAllNextProgram != value)
                {
                    if (_reentrancyCheckCanShowAllNextProgram)
                        return;
                    this._reentrancyCheckCanShowAllNextProgram = true;
                    this._canShowAllNextProgram = value;
                    OnPropertyChanged(() => this.CanShowAllNextProgram);
                    UpdateCheckState();
                    this._reentrancyCheckCanShowAllNextProgram = false;
                }
            }
        }

        public bool? CanShowAllCountDown
        {
            get
            {
                return _canShowAllCountDown;
            }

            set
            {
                if (this._canShowAllCountDown != value)
                {
                    if (this._reentrancyCheckCanShowAllCountDown)
                        return;
                    this._reentrancyCheckCanShowAllCountDown = true;
                    this._canShowAllCountDown = value;
                    OnPropertyChanged(() => this.CanShowAllCountDown);
                    UpdateCheckState();
                    this._reentrancyCheckCanShowAllCountDown = false;
                }
            }
        }

        public bool IsProcessingPlaylist
        {
            get
            {
                return _isProcessingPlaylist;
            }

            set
            {
                this._isProcessingPlaylist = value;
                OnPropertyChanged(() => this.IsProcessingPlaylist);
            }
        }

        public PlaylistModel(ChannelModel channel)
        {
            _channel = channel;
        }

        public void StartDataProcessor()
        {
            StopDataProcessor();
            _isDataProcessorThreadRunning = true;
            _dataProcessorThread = new Thread(new ThreadStart(ExecuteDataProcessorThread));
            _dataProcessorThread.IsBackground = true;
            _dataProcessorThread.Start();
        }

        private void ExecuteDataProcessorThread()
        {
            while (_isDataProcessorThreadRunning)
            {
                OnPropertyChanged(() => this.Date);
                OnPropertyChanged(() => this.HasNewEvent);

                if (DateTime.Now - _lastPlaylistProcessorUpdateTime >= this.Channel.Setting.PlaylistUpdateInterval)
                {
                    UpdateData();
                    _lastPlaylistProcessorUpdateTime = DateTime.Now;
                }

                if (this.Date.Date == DateTime.Now.Date)
                {
                    for (int i = 0; this.Events != null && i < this.Events.Count; i++)
                    {
                        EventModel eventModel = this.Events[i];
                        TimeSpan beginTime = Common.Utility.GetTimeSpanFromString(eventModel.BeginTime);
                        TimeSpan duration = Common.Utility.GetTimeSpanFromString(eventModel.Duration);

                        //Cập nhật trạng thái thời gian đã trôi qua hoặc đang chạy của từng sự kiện
                        eventModel.IsTimeElapsed = DateTime.Now.TimeOfDay > (beginTime + duration);
                        eventModel.IsOnTime = DateTime.Now.TimeOfDay >= beginTime && DateTime.Now.TimeOfDay < (beginTime + duration);

                        ////test
                        //if (eventModel.IsOnTime)
                        //{
                        //    eventModel.IsLocking = false;
                        //    if (eventModel.PreviousPE != null)
                        //    {
                        //        eventModel.PreviousPE.IsLocking = true;
                        //        eventModel.PreviousPE.LockedBy = new Common.Models.UserModel { UserName = "admin" };
                        //    }
                        //}
                        /////
                    }
                }

                Thread.Sleep(250);
            }
            _dataProcessorThread = null;
            GC.Collect();
        }

        public void StopDataProcessor()
        {
            _isDataProcessorThreadRunning = false;
        }

        public void UpdateData()
        {
            try
            {
                //Nếu Events chưa có gì thì load từ file cũ
                string olderFileName = this.Channel.Name + "_" + this.Date.ToString(this.Channel.Setting.DateFormatString, CultureInfo.InvariantCulture) + ".old";
                string olderFilePath = Path.Combine(this.Channel.Setting.OlderPlaylistFolderPath, olderFileName);
                DateTime olderFileLastWriteTime = File.GetLastWriteTime(olderFilePath);

                //////if (File.Exists(olderFilePath) && this.Events != null && this.Events.Count == 0)
                if (File.Exists(olderFilePath) && olderFileLastWriteTime > this.LastPlaylistUpdateFileTime)
                {
                    ObservableCollection<EventModel> olderEvents = LoadEventListFromOlderFile(olderFilePath);
                    if (olderEvents != null && olderEvents.Count > 0)
                    {
                        this.Events = new ObservableCollection<EventModel>();
                        if (SyncNewDataToOlderEventList(olderEvents) && _isDataProcessorThreadRunning)
                        {
                            this.LastPlaylistUpdateFileTime = olderFileLastWriteTime;
                            this.LastPlaylistUpdateFilePath = olderFilePath;
                            //SavePlayListToFile(olderFilePath);

                            foreach (var eventItem in this.Events)
                            {
                                //Xử lý comingup
                                if (eventItem.IsComingUpEvent)
                                {
                                    eventItem.CTodayFilePath = ProcessComingUpData(this.Channel.Setting.CToday, this, eventItem);
                                    eventItem.CTomorrowFilePath = ProcessComingUpData(this.Channel.Setting.CTomorrow, this.Channel.GetPlaylistByDate(this.Date.AddDays(1)), eventItem);
                                }
                                ///
                            }
                            if (this.Channel != null)
                            {
                                this.Channel.SaveMergePlaylistToFile(Path.Combine(this.Channel.Setting.MergePlaylistFolderPath, "Today_" + this.Channel.Name + ".xml"));
                            }

                            Debug.WriteLine("LOAD OLDER EVENTS: " + this.Date + " at " + olderFileLastWriteTime);
                        }
                    }
                }

                //Mở thử mục và sắp xếp các file theo LastWriteTime giảm dần
                var di = new DirectoryInfo(this.Channel.Setting.PlaylistFolderPath);
                if (di.Exists)
                {
                    List<string> sortedFilePaths = new List<string>();
                    string filterFormatString = "";

                    //1. Thêm filter của ngày hôm nay
                    filterFormatString = string.Format("{0}{1}", this.Channel.Name, this.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    var filePaths = di.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly).Where(m => m.Name.Substring(0, filterFormatString.Length).Equals(filterFormatString, StringComparison.OrdinalIgnoreCase) && m.LastWriteTime > this.LastPlaylistUpdateFileTime).OrderByDescending(m => m.LastWriteTime).Select(m => m.FullName).ToArray();
                    sortedFilePaths.AddRange(filePaths);
                    ///

                    //2. Thêm filter của ngày hôm trước
                    filterFormatString = string.Format("{0}{1}", this.Channel.Name, this.Date.AddDays(-1).ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    var previousFilePaths = di.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly).Where(m => m.Name.Substring(0, filterFormatString.Length).Equals(filterFormatString, StringComparison.OrdinalIgnoreCase) && m.LastWriteTime > this.LastPlaylistUpdateFileTime).OrderByDescending(m => m.LastWriteTime).Select(m => m.FullName).ToArray();
                    sortedFilePaths.AddRange(previousFilePaths);
                    ///

                    if (sortedFilePaths != null && sortedFilePaths.Count > 0)
                    {
                        bool hasLoaded = false;
                        for (int i = 0; i < sortedFilePaths.Count && _isDataProcessorThreadRunning; i++)
                        {
                            try
                            {
                                ObservableCollection<EventModel> newEventList = LoadEventListFromMCSFile(sortedFilePaths[i]);
                                if (newEventList != null && newEventList.Count > 0)
                                {
                                    //////if (this.Events == null)
                                    //////{
                                    //////    this.Events = new ObservableCollection<EventModel>();
                                    //////}

                                    if (hasLoaded == false)
                                    {
                                        //Thay thế list hiện tại bằng list mới
                                        if (SyncNewDataToOlderEventList(newEventList) && _isDataProcessorThreadRunning)
                                        {
                                            this.LastPlaylistUpdateFileTime = File.GetLastWriteTime(sortedFilePaths[i]);
                                            this.LastPlaylistUpdateFilePath = sortedFilePaths[i];
                                            SavePlayListToFile(olderFilePath);
                                            hasLoaded = true;

                                            Debug.WriteLine("Playlist update data: " + this.LastPlaylistUpdateFileTime + " - " + this.LastPlaylistUpdateFilePath);
                                            break;
                                        }
                                        ///
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
                ///
                GC.Collect();
            }
            catch (Exception) { }
        }

        private void UpdateCheckState()
        {
            if (this.Channel != null && this.Events != null)
            {
                for (int i = 0; i < this.Events.Count; i++)
                {
                    EventModel eventModel = this.Events[i];
                    if (eventModel.IsPrimaryEvent && eventModel.IsUserPrimaryEvent)
                    {
                        if (this.CanShowAllCG.HasValue)
                            eventModel.CanShowCG = this.CanShowAllCG.Value;
                        if (eventModel.CanShowCG)
                        {
                            if (this.CanShowAllNowProgram.HasValue)
                                eventModel.CanShowNowProgram = this.CanShowAllNowProgram.Value;
                            if (eventModel.NextPENotSameProgramCode != null && eventModel.NextPENotSameProgramCode.CanShowCG && eventModel.NextPENotSameProgramCode.IsValidTenMuLength && eventModel.NextPENotSameProgramCode.IsValidTenCtLength)
                            {
                                if (this.CanShowAllNextProgram.HasValue)
                                    eventModel.CanShowNextProgram = this.CanShowAllNextProgram.Value;
                                if (this.CanShowAllCountDown.HasValue)
                                    eventModel.CanShowCountDown = this.CanShowAllCountDown.Value;
                            }
                        }
                    }
                }
                UpdateValidEvents();
            }
        }

        public void UpdateValidEvents()
        {
            if (this.Events != null)
            {
                InitializeValidEvent(this.Events);

                EventModel lastNextPENotEqualProgramCode = null;
                for (int i = 0; i < this.Events.Count; i++)
                {
                    EventModel currentEvent = this.Events[i];
                    if (currentEvent.IsUserPrimaryEvent)
                    {
                        currentEvent.IsReadOnly = false;
                    }

                    TimeSpan currentEventDuration = Common.Utility.GetTimeSpanFromString(currentEvent.Duration);
                    //EventModel previousEvent = (i > 0) ? this.Events[i - 1] : null;
                    //EventModel nextEvent = (i < this.Events.Count - 1) ? this.Events[i + 1] : null;
                    EventModel previousPE = GetPreviousPE(this.Events, currentEvent, i, true);
                    EventModel nextPE = GetNextPE(this.Events, currentEvent, i, true);
                    EventModel previousPENotEqualProgramCode = GetPreviousPE(this.Events, currentEvent, i, false);
                    EventModel nextPENotEqualProgramCode = lastNextPENotEqualProgramCode;
                    if (currentEvent.IsPrimaryEvent && currentEvent.IsUserPrimaryEvent)
                    {
                        nextPENotEqualProgramCode = GetNextPE(this.Events, currentEvent, i, false);
                        lastNextPENotEqualProgramCode = nextPENotEqualProgramCode;
                    }

                    currentEvent.PreviousPE = previousPE;
                    currentEvent.NextPE = nextPE;
                    currentEvent.PreviousPENotSameProgramCode = previousPENotEqualProgramCode;
                    currentEvent.NextPENotSameProgramCode = nextPENotEqualProgramCode;

                    currentEvent.IsValidTenMuLength = Common.Utility.IsValidStringLength(currentEvent.TenMu, this.Channel.Setting.MaximumStringLength);
                    currentEvent.IsValidTenCtLength = Common.Utility.IsValidStringLength(currentEvent.TenCt, this.Channel.Setting.MaximumStringLength);

                    if (currentEvent.IsPrimaryEvent && currentEvent.IsUserPrimaryEvent)
                    {
                        if (currentEvent.CanShowCG == false || currentEvent.IsValidTenMuLength == false || currentEvent.IsValidTenCtLength == false)
                        {
                            currentEvent.CanShowNowProgram = false;
                            currentEvent.CanShowSpecial = false;

                            if (previousPE != null && previousPE.ProgramCode.Equals(currentEvent.ProgramCode) == false)
                            {
                                previousPE.CanShowNextProgram = false;
                                previousPE.CanShowCountDown = false;
                            }

                            //if (previousPE!=null && previousPE.GroupName.Equals(currentEvent.GroupName, StringComparison.OrdinalIgnoreCase) == false)
                            //{
                            //    previousPE.CanShowNextProgram = false;
                            //    previousPE.CanShowCountDown = false;
                            //}
                        }

                        if (nextPENotEqualProgramCode != null)
                        {
                            nextPENotEqualProgramCode.IsValidTenMuLength = Common.Utility.IsValidStringLength(nextPENotEqualProgramCode.TenMu, this.Channel.Setting.MaximumStringLength);
                            nextPENotEqualProgramCode.IsValidTenCtLength = Common.Utility.IsValidStringLength(nextPENotEqualProgramCode.TenCt, this.Channel.Setting.MaximumStringLength);
                            if (nextPENotEqualProgramCode.CanShowCG == false || nextPENotEqualProgramCode.IsValidTenMuLength == false || nextPENotEqualProgramCode.IsValidTenCtLength == false)
                            {
                                currentEvent.CanShowNextProgram = false;
                                currentEvent.CanShowCountDown = false;
                            }
                        }
                        else
                        {
                            currentEvent.CanShowNextProgram = false;
                            currentEvent.CanShowCountDown = false;
                        }

                        //Mỗi sự kiện chính đều có đệm đồ họa
                        currentEvent.IsTemporaryCGEvent = true;
                    }
                    else
                    {
                        if (currentEvent.IsLiveEvent || currentEvent.IsComingUpEvent || currentEvent.IsAdvertismentEvent)
                        {
                            if (currentEvent.IsComingUpEvent)
                            {
                                currentEvent.IsReadOnly = true;
                            }
                            currentEvent.IsPrimaryEvent = false;
                            currentEvent.IsUserPrimaryEvent = false;
                            if (currentEvent.IsAdvertismentEvent == false)
                            {
                                currentEvent.CanShowCG = false;
                            }
                            currentEvent.CanShowNowProgram = false;
                            currentEvent.CanShowNextProgram = false;
                            currentEvent.CanShowCountDown = false;
                            currentEvent.CanShowSpecial = false;
                        }
                        else
                        {
                            if (previousPENotEqualProgramCode != null)
                            {
                                currentEvent.CanShowCG = previousPENotEqualProgramCode.CanShowCG;
                            }
                        }
                    }

                    //Gán initialize=1 cho sự kiện (n-3) của lịch phát sóng
                    //currentEvent.HasInitializeCommand = (i == this.Events.Count - 3);
                    ///

                    //Gán cleanup=1 cho sự kiện cuối cùng của lịch phát sóng
                    //currentEvent.HasCleanupCommand = (i == this.Events.Count - 1);
                    ///


                    //2016-09-16 14:23
                    //Trước mỗi sự kiện trôi, nếu là sự kiện QC thì gán initialize=1
                    //Sự kiện liền kề sau mỗi sự kiện trôi thì gán cleanup=1
                    //if (currentEvent.IsComingUpEvent)
                    //{
                    //    if (i > 0)
                    //    {
                    //        for (int j = i - 1; j >= 0; j--)
                    //        {
                    //            if (this.Events[j].IsAdvertismentEvent)
                    //            {
                    //                this.Events[j].HasInitializeCommand = true;
                    //                break;
                    //            }
                    //        }
                    //    }

                    //    int afterComingUpCount = 5;
                    //    while (afterComingUpCount > 0)
                    //    {
                    //        if (i < this.Events.Count - afterComingUpCount - 1)
                    //        {
                    //            this.Events[i + afterComingUpCount].HasCleanupCommand = true;
                    //        }
                    //        --afterComingUpCount;
                    //    }
                    //}

                    //if (i == this.Events.Count - 1)
                    //{
                    //    for (int j = i - 1; j >= 0; j--)
                    //    {
                    //        if (this.Events[j].IsAdvertismentEvent)
                    //        {
                    //            this.Events[j].HasInitializeCommand = true;
                    //            break;
                    //        }
                    //    }
                    //}
                    ///

                    //2016-09-22 14:46
                    //Chèn initialize=1 vào mỗi sự kiện (n-3)
                    currentEvent.HasInitializeCommand = (i == this.Events.Count - 3);
                    ///



                    //Tính toán căng thẳng đây!!!
                    //Tính countdown duration cho từng sự kiện
                    //if (currentEvent.IsPrimaryEvent || currentEvent.IsUserPrimaryEvent)
                    if (currentEvent.IsPrimaryEvent && currentEvent.IsUserPrimaryEvent)
                    {
                        currentEvent.CountDownSecondDuration = this.Channel.Setting.DefaultSecondaryEventCountDown != null ? (int)this.Channel.Setting.DefaultSecondaryEventCountDown.Offset.TotalSeconds : 0;
                    }
                    else
                    {
                        currentEvent.CountDownSecondDuration = (int)currentEventDuration.TotalSeconds;
                    }
                    for (int j = i + 1; j < this.Events.Count; j++)
                    {
                        if (this.Events[j].IsPrimaryEvent &&
                            this.Events[j].IsUserPrimaryEvent &&
                            this.Events[j].ProgramCode.Equals(currentEvent.ProgramCode) == false)
                        {
                            break;
                        }
                        else
                        {
                            currentEvent.CountDownSecondDuration += (int)Common.Utility.GetTimeSpanFromString(this.Events[j].Duration).TotalSeconds;
                        }
                    }
                    ///

                    //Tính offset và duration cho các sự kiện secondary
                    currentEvent.SecondaryEvents.Clear();
                    if (currentEvent.CanShowCG &&
                        this.Channel.Setting.DefaultSecondaryEventClear != null &&
                        this.Channel.Setting.DefaultSecondaryEventClock != null &&
                        this.Channel.Setting.DefaultSecondaryEventNowProgram != null &&
                        this.Channel.Setting.DefaultSecondaryEventNextProgram != null &&
                        this.Channel.Setting.DefaultSecondaryEventCountDown != null)
                    {
                        TimeSpan secondaryEventOffset = TimeSpan.Zero;
                        TimeSpan secondaryEventDuration = TimeSpan.Zero;

                        //DANGXEM
                        if (currentEvent.IsPrimaryEvent && currentEvent.IsUserPrimaryEvent && currentEvent.CanShowNowProgram)
                        {
                            if (previousPE == null || (previousPE != null && previousPE.ProgramCode.Equals(currentEvent.ProgramCode) == false))
                            {
                                secondaryEventOffset = this.Channel.Setting.DefaultSecondaryEventNowProgram.Offset;
                            }
                            else
                            {
                                secondaryEventOffset = TimeSpan.Zero;
                            }

                            //Nếu có thiết lập nextprogram
                            if (currentEvent.CanShowNextProgram)
                            {
                                secondaryEventDuration = currentEventDuration - (this.Channel.Setting.DefaultSecondaryEventNowProgram.Offset + this.Channel.Setting.DefaultSecondaryEventNextProgram.Offset) - TimeSpan.FromSeconds(1);
                            }
                            else
                            {
                                secondaryEventDuration = Common.Utility.GetTimeSpanFromString(currentEvent.Duration) - TimeSpan.FromSeconds(1);
                            }

                            if (secondaryEventDuration < TimeSpan.Zero)
                            {
                                secondaryEventDuration = TimeSpan.Zero;
                            }

                            if (secondaryEventOffset >= TimeSpan.Zero && secondaryEventDuration >= TimeSpan.Zero)
                            {
                                currentEvent.SecondaryEvents.Add(new SecondaryEventModel()
                                {
                                    Name = this.Channel.Setting.DefaultSecondaryEventNowProgram.Name,
                                    Offset = secondaryEventOffset,
                                    Duration = secondaryEventDuration
                                });
                            }
                            //else
                            //{
                            //    currentEvent.CanShowNowProgram = false;
                            //}
                        }
                        ///

                        //TIEPTHEO
                        if (currentEvent.IsPrimaryEvent && currentEvent.IsUserPrimaryEvent && currentEvent.CanShowNextProgram)
                        {
                            secondaryEventOffset = currentEventDuration - this.Channel.Setting.DefaultSecondaryEventNextProgram.Offset;
                            secondaryEventDuration = this.Channel.Setting.DefaultSecondaryEventNextProgram.Duration;

                            //Nếu có thiết lập countdown
                            if (currentEvent.CanShowCountDown && currentEvent.CountDownSecondDuration <= (int)this.Channel.Setting.MaximumCountDownDuration.TotalSeconds)
                            {
                                if (this.Channel.Setting.DefaultSecondaryEventNextProgram.Offset - this.Channel.Setting.DefaultSecondaryEventCountDown.Offset <= secondaryEventDuration)
                                {
                                    secondaryEventDuration = this.Channel.Setting.DefaultSecondaryEventNextProgram.Offset - this.Channel.Setting.DefaultSecondaryEventCountDown.Offset - TimeSpan.FromSeconds(1);
                                }
                            }
                            else
                            {
                                if (currentEventDuration - secondaryEventOffset <= secondaryEventDuration)
                                {
                                    secondaryEventDuration = currentEventDuration - secondaryEventOffset - TimeSpan.FromSeconds(1);
                                }
                            }

                            //if (secondaryEventOffset >= TimeSpan.Zero && secondaryEventDuration >= TimeSpan.Zero && (nextPE != null && currentEvent.GroupName.Equals(nextPE.GroupName, StringComparison.OrdinalIgnoreCase) == false))
                            if (secondaryEventOffset >= TimeSpan.Zero && secondaryEventDuration >= TimeSpan.Zero)
                            {
                                currentEvent.SecondaryEvents.Add(new SecondaryEventModel()
                                {
                                    Name = this.Channel.Setting.DefaultSecondaryEventNextProgram.Name,
                                    Offset = secondaryEventOffset,
                                    Duration = secondaryEventDuration
                                });
                            }
                            //else
                            //{
                            //    currentEvent.CanShowNextProgram = false;
                            //}
                        }
                        ///

                        //DEMNGUOC
                        if (currentEvent.CanShowCountDown)
                        {
                            secondaryEventOffset = currentEventDuration - this.Channel.Setting.DefaultSecondaryEventCountDown.Offset;
                            secondaryEventDuration = currentEventDuration;

                            if (currentEvent.IsPrimaryEvent && currentEvent.IsUserPrimaryEvent)
                            {
                                secondaryEventDuration = this.Channel.Setting.DefaultSecondaryEventCountDown.Offset;
                            }
                            else
                            {
                                secondaryEventOffset = TimeSpan.Zero;
                            }

                            //if (secondaryEventOffset >= TimeSpan.Zero && secondaryEventDuration >= TimeSpan.Zero && (nextPE != null && currentEvent.GroupName.Equals(nextPE.GroupName, StringComparison.OrdinalIgnoreCase) == false))
                            if (secondaryEventOffset >= TimeSpan.Zero && secondaryEventDuration >= TimeSpan.Zero)
                            {
                                currentEvent.SecondaryEvents.Add(new SecondaryEventModel()
                                {
                                    Name = this.Channel.Setting.DefaultSecondaryEventCountDown.Name,
                                    Offset = secondaryEventOffset,
                                    Duration = secondaryEventDuration
                                });
                            }
                            //else
                            //{
                            //    currentEvent.CanShowCountDown = false;
                            //}
                        }

                        if (currentEvent.IsPrimaryEvent && currentEvent.IsUserPrimaryEvent)
                        {
                            for (int j = i + 1; j < this.Events.Count; j++)
                            {
                                EventModel tempNextEvent = this.Events[j];
                                if (tempNextEvent.IsPrimaryEvent && tempNextEvent.IsUserPrimaryEvent)
                                {
                                    break;
                                }
                                else
                                {
                                    tempNextEvent.CanShowCG = currentEvent.CanShowCG;
                                    tempNextEvent.CanShowCountDown = currentEvent.CanShowCountDown;
                                    tempNextEvent.IsTemporaryCGEvent = currentEvent.IsTemporaryCGEvent;
                                }
                            }
                        }
                        ///
                    }
                    ///
                }
            }
        }

        private EventModel GetPreviousPE(ObservableCollection<EventModel> eventList, EventModel currentEvent, int currentIndex, bool canBeSameProgrameCode)
        {
            EventModel previousEvent = null;
            if (currentEvent != null)
            {
                bool isFound = false;
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    if (eventList[i].IsLiveEvent)
                    {
                        break;
                    }
                    else
                    {
                        if (eventList[i].IsPrimaryEvent && eventList[i].IsUserPrimaryEvent)
                        {
                            if (canBeSameProgrameCode || (canBeSameProgrameCode == false && eventList[i].ProgramCode.Equals(currentEvent.ProgramCode) == false))
                            {
                                isFound = true;
                            }
                        }

                        if (isFound)
                        {
                            previousEvent = eventList[i];
                            break;
                        }
                    }
                }
            }
            return previousEvent;
        }

        private EventModel GetNextPE(ObservableCollection<EventModel> eventList, EventModel currentEvent, int currentIndex, bool canBeSameProgrameCode)
        {
            EventModel nextEvent = null;
            if (currentEvent != null)
            {
                bool isFound = false;
                for (int i = currentIndex + 1; i < eventList.Count; i++)
                {
                    if (eventList[i].IsLiveEvent)
                    {
                        break;
                    }
                    else
                    {
                        if (eventList[i].IsPrimaryEvent && eventList[i].IsUserPrimaryEvent)
                        {
                            if (canBeSameProgrameCode || (canBeSameProgrameCode == false && eventList[i].ProgramCode.Equals(currentEvent.ProgramCode) == false))
                            {
                                isFound = true;
                            }
                        }

                        if (isFound)
                        {
                            nextEvent = eventList[i];
                            break;
                        }
                    }
                }
            }
            return nextEvent;
        }

        public ObservableCollection<EventModel> LoadEventListFromMCSFile(string filePath)
        {
            ObservableCollection<EventModel> list = null;
            try
            {
                if (File.Exists(filePath))
                {
                    //MCS 2
                    //XmlDocument xmlDocument = new XmlDocument();
                    //xmlDocument.Load(filePath);
                    XmlDocument xmlDocument = FileManager.OpenXML(filePath, 5);
                    if (xmlDocument != null)
                    {
                        list = new ObservableCollection<EventModel>();

                        XmlElement nodePlaylist = (XmlElement)xmlDocument.SelectSingleNode("/playlist");
                        XmlNodeList nodeEventList = nodePlaylist.ChildNodes;
                        int stt = 0;
                        foreach (var itemEvent in nodeEventList)
                        {
                            XmlElement nodeEvent = (XmlElement)itemEvent;
                            if (nodeEvent.Name == "event")
                            {
                                ++stt;
                                XmlNodeList nodeEventChildList = nodeEvent.ChildNodes;
                                string type = "";
                                string channel = "";
                                string programcode = "";
                                string prog_type = "";
                                string date = "";
                                string id = "";
                                string time = "";
                                string description = "";
                                string duration = "";
                                foreach (var itemEventChild in nodeEventChildList)
                                {
                                    XmlElement nodeEventChild = (XmlElement)itemEventChild;
                                    string value = nodeEventChild.InnerText.Trim();
                                    switch (nodeEventChild.Name)
                                    {
                                        case "type":
                                            type = value;
                                            break;
                                        case "channel":
                                            channel = value;
                                            break;
                                        case "programcode":
                                            programcode = value;
                                            break;
                                        case "prog_type":
                                            prog_type = value;
                                            break;
                                        case "date":
                                            date = value;
                                            break;
                                        case "id":
                                            id = value;
                                            break;
                                        case "time":
                                            time = value;
                                            break;
                                        case "description":
                                            description = value;
                                            break;
                                        case "duration":
                                            duration = value;
                                            break;
                                    }
                                }

                                //Kiểm tra đúng kênh và đúng ngày thì mới nạp
                                bool isValidChannel = channel.Equals(this.Channel.Name, StringComparison.OrdinalIgnoreCase);
                                DateTime dt = Common.Utility.GetDateFromString(date, "/").Date;
                                bool isValidDate = (dt == this.Date.Date);
                                if (isValidChannel && isValidDate)
                                {
                                    EventModel eventModel = new EventModel(this.Channel);
                                    eventModel.EventType = type.Equals("MAIN", StringComparison.OrdinalIgnoreCase) ? "PRIMARY" : type;
                                    eventModel.STT = stt.ToString();
                                    eventModel.ProgramCode = programcode;
                                    eventModel.Title = prog_type;
                                    eventModel.EventId = id;
                                    eventModel.BeginTime = time;
                                    eventModel.Duration = duration;
                                    eventModel.Description = description;
                                    eventModel.GetProgramTitleFromDescription(description, ':'); //Lấy TenMu và TenCt từ description

                                    //MakeValidEvent(eventModel);
                                    list.Add(eventModel);
                                }

                                //if (dt.Date == this.Date.Date.AddDays(1) && this.Channel.Setting.CountOfNextDays >= 1)
                                //{
                                //    foreach (PlaylistModel playlistItem in this.Channel.Playlists)
                                //    {
                                //        if (playlistItem.Date == dt)
                                //        {
                                //            playlistItem.LoadEventListFromMCSFile(mcs, filePath);
                                //            break;
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return list;
        }

        public ObservableCollection<EventModel> LoadEventListFromOlderFile(string filePath)
        {
            ObservableCollection<EventModel> list = null;
            try
            {
                if (File.Exists(filePath))
                {
                    //XmlDocument xmlDocument = new XmlDocument();
                    //xmlDocument.Load(filePath);
                    XmlDocument xmlDocument = FileManager.OpenXML(filePath, 5);
                    if (xmlDocument != null)
                    {
                        list = new ObservableCollection<EventModel>();

                        XmlElement nodePlaylist = (XmlElement)xmlDocument.SelectSingleNode("/playlist");
                        Guid sessionId = Guid.NewGuid();
                        if (nodePlaylist.HasAttribute("session_id"))
                        {
                            Guid.TryParse(nodePlaylist.GetAttribute("session_id"), out sessionId);
                            //Nếu sessionId trong file trùng với hiện tại thì bỏ qua
                            if (sessionId == AppData.Default.SessionId)
                            {
                                return null;
                            }
                        }

                        XmlElement nodeGeneralInfo = (XmlElement)xmlDocument.SelectSingleNode("/playlist/general_info");
                        string date = nodeGeneralInfo.GetAttribute("date");
                        string channel = nodeGeneralInfo.GetAttribute("channel");

                        //Kiểm tra đúng kênh và đúng ngày thì mới nạp
                        bool isValidChannel = channel.Equals(this.Channel.Name, StringComparison.OrdinalIgnoreCase);
                        bool isValidDate = Common.Utility.GetDateFromString(date, "/").Date == this.Date.Date;

                        if (isValidChannel && isValidDate)
                        {
                            XmlNodeList nodeEventList = xmlDocument.SelectNodes("/playlist/event");
                            foreach (var itemEvent in nodeEventList)
                            {
                                XmlElement nodeEvent = (XmlElement)itemEvent;

                                if (nodeEvent.Name == "event")
                                {
                                    XmlNodeList nodeEventChildList = nodeEvent.ChildNodes;
                                    string event_type = "";
                                    string stt = "";
                                    //string date = ""; đã khai báo ở trên
                                    string event_id = "";
                                    string mabang = "";
                                    string description = "";
                                    string title = "";
                                    string begin_time = "";
                                    string begin_time2 = "";
                                    string duration = "";
                                    string tc_vao = "";
                                    string tc_ra = "";
                                    string noidung = "";

                                    //tiengviet
                                    string tiengviet_ten_mu = "";
                                    string tiengviet_ten_ct = "";
                                    string tiengviet_status = "";
                                    string tiengviet_modify = "";

                                    //loai_ct
                                    bool loai_ct_skc = true;
                                    bool loai_ct_dem_dohoa = false;
                                    bool loai_ct_qc = false;
                                    bool loai_ct_initialize = false;
                                    bool loai_ct_cleanup = false;

                                    //dem_nguoc
                                    int dem_nguoc_duration = 0;

                                    //hienThi
                                    bool canShowCG = false;
                                    bool canShowNowProgram = false;
                                    bool canShowNextProgram = false;
                                    bool canShowCountDown = false;
                                    bool canShowSpecial = false;

                                    ////nextEvent
                                    //string next_event_ten_mu = "";
                                    //string next_event_ten_ct = "";

                                    string dacSac = "";

                                    foreach (var itemEventChild in nodeEventChildList)
                                    {
                                        XmlElement nodeEventChild = (XmlElement)itemEventChild;
                                        switch (nodeEventChild.Name)
                                        {
                                            case "event_type":
                                                event_type = nodeEventChild.GetAttribute("type");
                                                break;
                                            case "stt":
                                                stt = nodeEventChild.GetAttribute("stt");
                                                break;
                                            case "date":
                                                date = nodeEventChild.GetAttribute("date");
                                                break;
                                            case "event_id":
                                                event_id = nodeEventChild.GetAttribute("id");
                                                break;
                                            case "mabang":
                                                mabang = nodeEventChild.GetAttribute("tapeid");
                                                break;
                                            case "description":
                                                description = nodeEventChild.GetAttribute("des");
                                                break;
                                            case "title":
                                                title = nodeEventChild.GetAttribute("ten_ct");
                                                break;
                                            case "begin_time":
                                                begin_time = nodeEventChild.GetAttribute("time");
                                                break;
                                            case "begin_time2":
                                                begin_time2 = nodeEventChild.GetAttribute("time");
                                                break;
                                            case "duration":
                                                duration = nodeEventChild.GetAttribute("time");
                                                break;
                                            case "tc_vao":
                                                tc_vao = nodeEventChild.GetAttribute("tc_vao");
                                                break;
                                            case "tc_ra":
                                                tc_ra = nodeEventChild.GetAttribute("tc_ra");
                                                break;
                                            case "noidung":
                                                noidung = nodeEventChild.GetAttribute("noi_dung");
                                                break;
                                            case "tiengviet":
                                                tiengviet_ten_mu = nodeEventChild.GetAttribute("ten_mu");
                                                tiengviet_ten_ct = nodeEventChild.GetAttribute("ten_ct");
                                                tiengviet_status = nodeEventChild.GetAttribute("status");
                                                tiengviet_modify = nodeEventChild.GetAttribute("modify");
                                                break;
                                            case "loai_ct":
                                                loai_ct_skc = nodeEventChild.GetAttribute("skc") == "1";
                                                loai_ct_dem_dohoa = nodeEventChild.GetAttribute("dem_dohoa") == "1";
                                                loai_ct_qc = nodeEventChild.GetAttribute("qc") == "1";
                                                //loai_ct_initialize = nodeEventChild.GetAttribute("initialize") == "1";
                                                //loai_ct_cleanup = nodeEventChild.GetAttribute("cleanup") == "1";
                                                break;
                                            case "dem_nguoc":
                                                int.TryParse(nodeEventChild.GetAttribute("duration"), out dem_nguoc_duration);
                                                break;
                                            case "hienthi":
                                                canShowCG = nodeEventChild.GetAttribute("do_hoa") == "1";
                                                canShowNowProgram = nodeEventChild.GetAttribute("dang_xem") == "1";
                                                canShowNextProgram = nodeEventChild.GetAttribute("tiep_theo") == "1";
                                                canShowCountDown = nodeEventChild.GetAttribute("dem_nguoc") == "1";
                                                canShowSpecial = nodeEventChild.GetAttribute("dac_sac") == "1";
                                                break;
                                            //case "next_event":
                                            //    next_event_ten_mu = nodeEventChild.GetAttribute("ten_mu");
                                            //    next_event_ten_ct = nodeEventChild.GetAttribute("ten_ct");
                                            //    break;
                                            case "dac_sac":
                                                //x = nodeEventChild.GetAttribute("");
                                                break;
                                            case "cToday":
                                                //x = nodeEventChild.GetAttribute("");
                                                break;
                                            case "cTomorrow":
                                                //x = nodeEventChild.GetAttribute("");
                                                break;
                                        }
                                    }

                                    EventModel eventModel = new EventModel(this.Channel);
                                    eventModel.EventType = event_type.Equals("MAIN", StringComparison.OrdinalIgnoreCase) ? "PRIMARY" : event_type;
                                    eventModel.STT = stt;
                                    //eventModel.date = stt;
                                    eventModel.EventId = event_id;
                                    eventModel.ProgramCode = mabang;
                                    eventModel.Description = description;
                                    eventModel.Title = title;
                                    eventModel.BeginTime = begin_time;
                                    eventModel.BeginTime2 = begin_time2;
                                    eventModel.Duration = duration;
                                    eventModel.TcVao = tc_vao;
                                    eventModel.TcRa = tc_ra;
                                    eventModel.NoiDung = noidung;

                                    //tiengviet
                                    eventModel.TenMu = tiengviet_ten_mu;
                                    eventModel.TenCt = tiengviet_ten_ct;
                                    eventModel.Status = tiengviet_status;
                                    eventModel.Modify = tiengviet_modify;
                                    ///

                                    //loai_ct
                                    eventModel.IsUserPrimaryEvent = loai_ct_skc;
                                    eventModel.IsTemporaryCGEvent = loai_ct_dem_dohoa;
                                    eventModel.IsAdvertismentEvent = loai_ct_qc;
                                    eventModel.HasInitializeCommand = loai_ct_initialize;
                                    eventModel.HasCleanupCommand = loai_ct_cleanup;
                                    ///

                                    //dem_nguoc
                                    eventModel.CountDownSecondDuration = dem_nguoc_duration;
                                    ///

                                    //hienthi
                                    eventModel.CanShowCG = canShowCG;
                                    eventModel.CanShowNowProgram = canShowNowProgram;
                                    eventModel.CanShowNextProgram = canShowNextProgram;
                                    eventModel.CanShowCountDown = canShowCountDown;
                                    eventModel.CanShowSpecial = canShowSpecial;
                                    ///

                                    //next_event
                                    //eventModel.NextTenMu = next_event_ten_mu;
                                    //eventModel.NextTenCt = next_event_ten_ct;
                                    ///

                                    //MakeValidEvent(eventModel);
                                    list.Add(eventModel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //throw;
            }
            return list;
        }

        private bool CheckStringStartsWithString(string sourceString, string stringChecker)
        {
            bool result = false;
            if (string.IsNullOrEmpty(sourceString) == false && string.IsNullOrEmpty(stringChecker) == false)
            {
                result = sourceString.StartsWith(stringChecker, StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }

        private void InitializeValidEvent(ObservableCollection<EventModel> eventModels)
        {
            if (eventModels != null)
            {
                for (int i = 0; i < eventModels.Count; i++)
                {
                    EventModel eventModel = eventModels[i];
                    //EventModel previousPE = GetPreviousPE(eventModels, eventModel, i, true);

                    //Gán IsLiveEvent=true nếu ProgramCode có chứa "XUNGDEN", "LIVE" ở đầu
                    eventModel.IsLiveEvent = CheckStringStartsWithString(eventModel.ProgramCode, "XUNGDEN") || CheckStringStartsWithString(eventModel.ProgramCode, "LIVE");
                    //Nếu trong setting có định nghĩa trường CustomLiveEventContainProgramCodeFilter thì tách chuỗi thiết lập đó
                    //thành mảng, phân cách nhau bằng dấu chấm phẩy ;
                    if (this.Channel.Setting.CustomLiveEventContainProgramCodeFilter != null)
                    {
                        string[] customLiveProgramCodeList = this.Channel.Setting.CustomLiveEventContainProgramCodeFilter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (customLiveProgramCodeList != null)
                        {
                            for (int j = 0; j < customLiveProgramCodeList.Length; j++)
                            {
                                if (CheckStringStartsWithString(eventModel.ProgramCode, customLiveProgramCodeList[j]))
                                {
                                    eventModel.IsLiveEvent = true;
                                }
                            }
                        }
                    }
                    ///

                    //Nếu sự kiện có programcode trùng với comingup thì gán IsComingUpEvent=true
                    eventModel.IsComingUpEvent = (this.Channel.Setting.CToday.ContainProgramCode(eventModel.ProgramCode) || this.Channel.Setting.CTomorrow.ContainProgramCode(eventModel.ProgramCode));
                    if (eventModel.IsLiveEvent || eventModel.IsComingUpEvent)
                    {
                        if (eventModel.IsComingUpEvent)
                        {
                            eventModel.TenMu = "giới thiệu chương trình";
                            eventModel.TenCt = "";
                            eventModel.IsReadOnly = true;
                        }
                        eventModel.IsPrimaryEvent = false;
                        eventModel.IsUserPrimaryEvent = false;
                    }
                    ///

                    //Gán IsAdvertismentEvent=true nếu ProgramCode có chứa "QC" ở đầu
                    eventModel.IsAdvertismentEvent = CheckStringStartsWithString(eventModel.ProgramCode, "QC");
                    //Nếu trong setting có định nghĩa trường CustomAdvertismentEventContainProgramCodeFilter thì tách chuỗi thiết lập đó
                    //thành mảng, phân cách nhau bằng dấu chấm phẩy ;
                    if (this.Channel.Setting.CustomAdvertismentEventContainProgramCodeFilter != null)
                    {
                        string[] customAdvertismentProgramCodeList = this.Channel.Setting.CustomAdvertismentEventContainProgramCodeFilter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (customAdvertismentProgramCodeList != null)
                        {
                            for (int j = 0; j < customAdvertismentProgramCodeList.Length; j++)
                            {
                                if (CheckStringStartsWithString(eventModel.ProgramCode, customAdvertismentProgramCodeList[j]))
                                {
                                    eventModel.IsAdvertismentEvent = true;
                                }
                            }
                        }
                    }
                    ///

                    //Kiểm tra Duration có thỏa mãn setting MinimumPrimaryEventDuration
                    //và không phải là LIVE, Advertisment
                    eventModel.IsPrimaryEvent = Common.Utility.GetTimeSpanFromString(eventModel.Duration) >= this.Channel.Setting.MinimumPrimaryEventDuration && eventModel.IsLiveEvent == false && eventModel.IsComingUpEvent == false && eventModel.IsAdvertismentEvent == false;
                    //Nếu trong setting có định nghĩa trường CustomPrimaryEventContainProgramCodeFilter thì tách chuỗi thiết lập đó
                    //thành mảng, phân cách nhau bằng dấu chấm phẩy ;
                    if (this.Channel.Setting.CustomPrimaryEventContainProgramCodeFilter != null)
                    {
                        string[] customPrimaryEventContainProgramCodeFilter = this.Channel.Setting.CustomPrimaryEventContainProgramCodeFilter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (customPrimaryEventContainProgramCodeFilter != null)
                        {
                            for (int j = 0; j < customPrimaryEventContainProgramCodeFilter.Length; j++)
                            {
                                if (CheckStringStartsWithString(eventModel.ProgramCode, customPrimaryEventContainProgramCodeFilter[j]))
                                {
                                    eventModel.IsPrimaryEvent = true;
                                }
                            }
                        }
                    }
                    ///


                    //eventModel.IsUserPrimaryEvent = eventModel.IsPrimaryEvent || eventModel.IsLiveEvent;
                    //Nếu sự kiện không phải là SKC và Live thì gán thuộc tính IsReadOnly=True
                    eventModel.IsReadOnly = (eventModel.IsPrimaryEvent == false && eventModel.IsLiveEvent == false);
                    ///

                    //eventModel.GroupName = eventModel.ProgramCode;

                    //Kiểm tra các sự kiện chính bị cắt ra thành nhiều phần mà duration không thỏa mãn
                    if (eventModel.IsPrimaryEvent || eventModel.IsUserPrimaryEvent || eventModel.IsLiveEvent)
                    {
                        int splitIndex = -1;
                        string originalProgramCode = eventModel.ProgramCode;
                        eventModel.GroupName = eventModel.ProgramCode;

                        for (int j = i - 10; j <= i + 10; j++)
                        {
                            if (j < 0 || j >= eventModels.Count || j == i)
                            {
                                continue;
                            }

                            splitIndex = -1;
                            bool isInGroup = false;
                            if (eventModels[j].ProgramCode.Equals(eventModel.ProgramCode, StringComparison.OrdinalIgnoreCase))
                            {
                                isInGroup = true;
                            }

                            if (isInGroup == false)
                            {
                                if (IsProgramCodeInPart(eventModel.ProgramCode, out originalProgramCode, out splitIndex))
                                {
                                    if (eventModels[j].ProgramCode.Equals(originalProgramCode))
                                    {
                                        isInGroup = true;
                                    }
                                }

                                if (isInGroup == false)
                                {
                                    string originalProgramCode2 = eventModels[j].ProgramCode;
                                    if (IsProgramCodeInPart(eventModels[j].ProgramCode, out originalProgramCode2, out splitIndex))
                                    {
                                        if (originalProgramCode.Equals(originalProgramCode2, StringComparison.OrdinalIgnoreCase))
                                        {
                                            isInGroup = true;
                                        }
                                    }
                                }
                            }

                            if (isInGroup)
                            {
                                eventModel.GroupName = originalProgramCode;
                                eventModels[j].GroupName = originalProgramCode;

                                if (eventModel.IsPrimaryEvent && eventModels[j].IsPrimaryEvent == false)
                                {
                                    eventModels[j].IsPrimaryEvent = eventModel.IsPrimaryEvent;
                                    //eventModels[j].IsUserPrimaryEvent = eventModel.IsUserPrimaryEvent;
                                    //eventModels[j].IsTemporaryCGEvent = eventModel.IsTemporaryCGEvent;
                                    //eventModels[j].IsAdvertismentEvent = eventModel.IsAdvertismentEvent;
                                    eventModels[j].IsReadOnly = eventModel.IsReadOnly;
                                }
                                else if (eventModel.IsPrimaryEvent == false && eventModels[j].IsPrimaryEvent)
                                {
                                    eventModel.IsPrimaryEvent = eventModels[j].IsPrimaryEvent;
                                    //eventModel.IsUserPrimaryEvent = eventModels[j].IsUserPrimaryEvent;
                                    //eventModel.IsTemporaryCGEvent = eventModels[j].IsTemporaryCGEvent;
                                    //eventModel.IsAdvertismentEvent = eventModels[j].IsAdvertismentEvent;
                                    eventModel.IsReadOnly = eventModels[j].IsReadOnly;
                                    break;
                                }
                            }
                        }
                    }

                    //EventModel previousPE = GetPreviousPE(eventModels, eventModel, i, true);
                    //EventModel nextPE = GetNextPE(eventModels, eventModel, i, true);
                    //if (eventModel.IsPrimaryEvent == false && previousPE != null && nextPE != null && previousPE.GroupName.Equals(nextPE.GroupName, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    eventModel.GroupName = previousPE.GroupName;
                    //}
                    ///


                }

                //string
                //for (int i = 0; i < eventModels.Count; i++)
                //{
                //    EventModel eventModel = eventModels[i];
                //    EventModel previousPE = GetPreviousPE(eventModels, eventModel, i, true);
                //    EventModel nextPE = GetNextPE(eventModels, eventModel, i, true);
                //    if(eventModel.IsPrimaryEvent==false && previousPE!=null && nextPE!=null && previousPE.GroupName.Equals(nextPE.GroupName, StringComparison.OrdinalIgnoreCase))
                //    {
                //        eventModel.GroupName = previousPE.GroupName;
                //    }
                //}
            }
        }

        private bool IsProgramCodeInPart(string programCode, out string originalProgramCode, out int splitIndex)
        {
            originalProgramCode = programCode;
            splitIndex = -1;

            try
            {
                string[] words = programCode.ToUpper().Split(new char[] { '_' });
                if (words != null && words.Length >= 2)
                {
                    int.TryParse(words[words.Length - 1], out splitIndex);
                    if (splitIndex > 0 && splitIndex < 10 &&
                        ((words[words.Length - 2].Length >= 3 && words[words.Length - 2].Substring(words[words.Length - 2].Length - 3, 3).Equals("TAP") == false) ||
                        (words[words.Length - 2].Length >= 4 && words[words.Length - 2].Substring(words[words.Length - 2].Length - 4, 4).Equals("PHAN") == false)))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < words.Length - 1; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append("_");
                            }
                            sb.Append(words[i]);
                        }
                        originalProgramCode = sb.ToString();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            //string twoCharactersProgramCodeEnding = programCode.Substring(programCode.Length - 2, 2);
            //if (twoCharactersProgramCodeEnding[0] == '_' && int.TryParse(twoCharactersProgramCodeEnding[1].ToString(), out splitIndex))
            //{

            //    originalProgramCode = programCode.Remove(programCode.Length - 2, 2);
            //    return splitIndex > 0;
            //}
            return false;
        }

        public bool SyncNewDataToOlderEventList(ObservableCollection<EventModel> newEventList)
        {
            if (newEventList != null)
            {
                foreach (EventModel eventModel in newEventList)
                {
                    eventModel.PropertyChanged += EventModel_PropertyChanged;
                }

                this.Channel.SyncDataFromTrafficToPlaylist(newEventList, null);
                InitializeValidEvent(newEventList);

                for (int i = 0; i < newEventList.Count; i++)
                {
                    EventModel newEventItem = newEventList[i];
                    EventModel nextNewPE = GetNextPE(newEventList, newEventItem, i, true);

                    //MakeValidEvent(newEventItem);
                    //newEventItem.IsPrimaryEvent = CheckPrimaryEvent(newEventItem);

                    ////Nếu 1 chương trình bị cắt làm nhiều đoạn,
                    ////kiểm tra các sự kiện phía sau đó, nếu trùng mã programcode thì gán lại IsPrimary=True
                    //if (newEventItem.IsPrimaryEvent == false)
                    //{
                    //    for (int j = i + 1; j < newEventList.Count; j++)
                    //    {
                    //        EventModel tempNextEvent = newEventList[j];
                    //        if (tempNextEvent.IsPrimaryEvent && tempNextEvent.ProgramCode.Equals(newEventItem.ProgramCode, StringComparison.OrdinalIgnoreCase))
                    //        {
                    //            newEventItem.IsPrimaryEvent = true;
                    //            //newEventItem.IsUserPrimaryEvent = true;
                    //            newEventItem.IsReadOnly = false;
                    //        }
                    //    }
                    //}
                    /////

                    if (newEventItem.IsPrimaryEvent == false && newEventItem.IsLiveEvent == false)
                    {
                        newEventItem.IsUserPrimaryEvent = false;
                        newEventItem.Status = "0";
                    }

                    if (this.Events != null && this.Events.Count > 0)
                    {
                        for (int j = 0; j < this.Events.Count; j++)
                        {
                            EventModel oldEventItem = this.Events[j];

                            if (newEventItem.EventId.Equals(oldEventItem.EventId))
                            {
                                newEventItem.TenMu = oldEventItem.TenMu;
                                newEventItem.TenCt = oldEventItem.TenCt;
                                newEventItem.Status = oldEventItem.Status;
                                newEventItem.Modify = oldEventItem.Modify;
                                newEventItem.IsUserPrimaryEvent = oldEventItem.IsUserPrimaryEvent;

                                newEventItem.CanShowCG = oldEventItem.CanShowCG;
                                newEventItem.CanShowNowProgram = oldEventItem.CanShowNowProgram;
                                newEventItem.CanShowSpecial = oldEventItem.CanShowSpecial;

                                newEventItem.CanShowNextProgram = oldEventItem.CanShowNextProgram;
                                newEventItem.CanShowCountDown = oldEventItem.CanShowCountDown;

                                ////nếu sự kiện mới có thời lượng lớn hơn hoặc bằng 3/4 thời lượng của sự kiện cũ thì gán 
                                ////nextprogram và countdown như cũ
                                //if (Common.Utility.GetTimeSpanFromString(newEventItem.Duration).TotalSeconds >= (Common.Utility.GetTimeSpanFromString(oldEventItem.Duration).TotalSeconds * (3 / 4)))
                                //{
                                //    newEventItem.CanShowNextProgram = oldEventItem.CanShowNextProgram;
                                //    newEventItem.CanShowCountDown = oldEventItem.CanShowCountDown;
                                //}
                                //có thể còn rất nhiều...
                                break;
                            }
                        }

                        ////nếu sự kiện mới kế tiếp trùng programcode của sự kiện mới này thì gán các thuộc tính đồ họa giống nhau
                        //if (nextNewPE != null && nextNewPE.ProgramCode.Equals(newEventItem.ProgramCode, StringComparison.OrdinalIgnoreCase))
                        //{
                        //    nextNewPE.CanShowCG = newEventItem.CanShowCG;
                        //    nextNewPE.CanShowNowProgram = newEventItem.CanShowNowProgram;
                        //    nextNewPE.CanShowSpecial = newEventItem.CanShowSpecial;
                        //    nextNewPE.CanShowNextProgram = newEventItem.CanShowNextProgram;
                        //    nextNewPE.CanShowCountDown = newEventItem.CanShowCountDown;
                        //}
                    }
                }

                for (int i = 0; i < newEventList.Count; i++)
                {
                    EventModel newEventItem = newEventList[i];
                    EventModel nextNewPE = GetNextPE(newEventList, newEventItem, i, true);

                    if (nextNewPE != null && nextNewPE.Status == "1" && nextNewPE.ProgramCode.Equals(newEventItem.ProgramCode, StringComparison.OrdinalIgnoreCase))
                    {
                        newEventItem.CanShowNextProgram = false;
                        newEventItem.CanShowCountDown = false;
                        nextNewPE.CanShowCG = newEventItem.CanShowCG;
                        nextNewPE.CanShowNowProgram = newEventItem.CanShowNowProgram;
                        nextNewPE.CanShowSpecial = newEventItem.CanShowSpecial;
                        nextNewPE.CanShowNextProgram = newEventItem.CanShowNextProgram;
                        nextNewPE.CanShowCountDown = newEventItem.CanShowCountDown;
                    }
                }

                //safe thread
                System.Windows.Application.Current.Dispatcher.Invoke(delegate
                {
                    this.Events = newEventList;
                });

                UpdateValidEvents();
                OnPropertyChanged(() => this.Events);

                return true;
            }
            return false;
        }

        private void EventModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            List<EventModel> primaryEvents = this.Events.Where(m => m.IsPrimaryEvent && m.IsUserPrimaryEvent).ToList();
            switch (e.PropertyName)
            {
                case "CanShowCG":
                    if (this._reentrancyCheckCanShowAllCG)
                        return;
                    this._reentrancyCheckCanShowAllCG = true;
                    if (primaryEvents.Count(m => m.CanShowCG == true) == primaryEvents.Count)
                    {
                        _canShowAllCG = true;
                    }
                    else if (primaryEvents.Count(m => m.CanShowCG == false) == primaryEvents.Count)
                    {
                        _canShowAllCG = false;
                    }
                    else
                    {
                        _canShowAllCG = null;
                    }
                    OnPropertyChanged(() => this.CanShowAllCG);
                    this._reentrancyCheckCanShowAllCG = false;
                    break;
                case "CanShowNowProgram":
                    if (this._reentrancyCheckCanShowAllNowProgram)
                        return;
                    this._reentrancyCheckCanShowAllNowProgram = true;
                    if (primaryEvents.Count(m => m.CanShowNowProgram == true) == primaryEvents.Count)
                    {
                        _canShowAllNowProgram = true;
                    }
                    else if (primaryEvents.Count(m => m.CanShowNowProgram == false) == primaryEvents.Count)
                    {
                        _canShowAllNowProgram = false;
                    }
                    else
                    {
                        _canShowAllNowProgram = null;
                    }
                    OnPropertyChanged(() => this.CanShowAllNowProgram);
                    this._reentrancyCheckCanShowAllNowProgram = false;
                    break;
                case "CanShowNextProgram":
                    if (_reentrancyCheckCanShowAllNextProgram)
                        return;
                    this._reentrancyCheckCanShowAllNextProgram = true;
                    if (primaryEvents.Count(m => m.CanShowNextProgram == true) == primaryEvents.Count)
                    {
                        _canShowAllNextProgram = true;
                    }
                    else if (primaryEvents.Count(m => m.CanShowNextProgram == false) == primaryEvents.Count)
                    {
                        _canShowAllNextProgram = false;
                    }
                    else
                    {
                        _canShowAllNextProgram = null;
                    }
                    OnPropertyChanged(() => this.CanShowAllNextProgram);
                    this._reentrancyCheckCanShowAllNextProgram = false;
                    break;
                case "CanShowCountDown":
                    if (this._reentrancyCheckCanShowAllCountDown)
                        return;
                    this._reentrancyCheckCanShowAllCountDown = true;
                    if (primaryEvents.Count(m => m.CanShowCountDown == true) == primaryEvents.Count)
                    {
                        _canShowAllCountDown = true;
                    }
                    else if (primaryEvents.Count(m => m.CanShowCountDown == false) == primaryEvents.Count)
                    {
                        _canShowAllCountDown = false;
                    }
                    else
                    {
                        _canShowAllCountDown = null;
                    }
                    OnPropertyChanged(() => this.CanShowAllCountDown);
                    this._reentrancyCheckCanShowAllCountDown = false;
                    break;
            }
        }

        public void SaveComingUpBlockDataToFile(ComingUpBlockModel comingUpBlock, string filePath)
        {
            try
            {
                if (comingUpBlock != null && comingUpBlock.Events != null)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < comingUpBlock.Events.Count; i++)
                    {
                        TimeSpan ts = Common.Utility.GetTimeSpanFromString(comingUpBlock.Events[i].BeginTime);
                        //làm tròn thời gian của từng sự kiện nếu cho phép
                        //vd: 13:32 -> 13:00, 13:36 -> 13:35, 13:38 -> 13:40
                        if (this.Channel.Setting.ComingUpRoundTimeEnabled)
                        {
                            switch (this.Channel.Setting.ComingUpRoundTimeMode.Trim())
                            {
                                case "0358":
                                    ts = Common.Utility.RoundMinutesInTimeSpan(ts, 3);
                                    break;
                                default:
                                    ts = Common.Utility.RoundMinutesInTimeSpan(ts, 5);
                                    break;
                            }
                        }
                        sb.Append(ts.ToString(@"hh\:mm", CultureInfo.InvariantCulture));
                        sb.Append("|");
                        sb.Append(comingUpBlock.Events[i].IsValidTenMuLength ? comingUpBlock.Events[i].TenMu : "");
                        sb.Append(" ");
                        sb.Append("|");
                        sb.Append(comingUpBlock.Events[i].IsValidTenCtLength ? comingUpBlock.Events[i].TenCt : "");
                        sb.Append(" ");
                        if (i < comingUpBlock.Events.Count - 1)
                        {
                            sb.Append(Environment.NewLine);
                        }
                    }

                    //Kiểm tra, nếu file đang được dùng bởi ứng dụng khác thì chờ 1 lúc
                    while (FileManager.IsFileLocked(new FileInfo(filePath)))
                    {
                        Thread.Sleep(100);
                    }

                    using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (Exception)
            {
                SaveComingUpBlockDataToFile(comingUpBlock, filePath);
            }
        }

        public void SaveComingUpTimeRangeDataToFile(ComingUpTimeRangeModel comingUpTimeRange, string filePath)
        {
            try
            {
                if (comingUpTimeRange != null && comingUpTimeRange.Events != null)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < comingUpTimeRange.Events.Count; i++)
                    {
                        TimeSpan ts = Common.Utility.GetTimeSpanFromString(comingUpTimeRange.Events[i].BeginTime);
                        //làm tròn thời gian của từng sự kiện nếu cho phép
                        //vd: 13:32 -> 13:00, 13:33 -> 13:33, 13:37 -> 13:35, 13:38 -> 13:38, 13:39 -> 13:40
                        if (_channel.Setting.ComingUpRoundTimeEnabled)
                        {
                            ts = Common.Utility.RoundMinutesInTimeSpan(ts, 3);
                        }
                        sb.Append(ts.ToString(@"hh\:mm", CultureInfo.InvariantCulture));
                        sb.Append("|");
                        sb.Append(comingUpTimeRange.Events[i].IsValidTenMuLength ? comingUpTimeRange.Events[i].TenMu : "");
                        sb.Append(" ");
                        sb.Append("|");
                        sb.Append(comingUpTimeRange.Events[i].IsValidTenCtLength ? comingUpTimeRange.Events[i].TenCt : "");
                        sb.Append(" ");
                        if (i < comingUpTimeRange.Events.Count - 1)
                        {
                            sb.Append(Environment.NewLine);
                        }
                    }

                    //Kiểm tra, nếu file đang được dùng bởi ứng dụng khác thì chờ 1 lúc
                    while (FileManager.IsFileLocked(new FileInfo(filePath)))
                    {
                        Thread.Sleep(100);
                    }

                    using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (Exception)
            {
                SaveComingUpTimeRangeDataToFile(comingUpTimeRange, filePath);
            }
        }

        private string ProcessComingUpData(ComingUpListModel comingUpList, PlaylistModel playlist, EventModel currentEvent)
        {
            string filePath = "";
            if (comingUpList != null && playlist != null && playlist.Events != null && currentEvent != null)
            {
                //Tạo 1 danh sách tạm chứa toàn bộ sự kiện chính, nếu các sự kiện liền kề có cùng mã băng thì chỉ lấy 1
                List<EventModel> primaryEvents = new List<EventModel>();
                string lastProgramCode = "";
                for (int i = 0; i < playlist.Events.Count; i++)
                {
                    //2016-09-16
                    //bool canBeInitialized = false;
                    ////if (i == playlist.Events.Count - 1 || i == playlist.Events.Count - 2)

                    //if (i == playlist.Events.Count - 2)
                    //{
                    //    playlist.Events[i].HasInitializeCommand = true;
                    //}

                    ////nếu programcode của sự kiện trùng với programcode của block thì gán là true
                    //canBeInitialized = comingUpList.ContainProgramCode(playlist.Events[i].ProgramCode);
                    //if (canBeInitialized)
                    //{
                    //    //if (i > 0) playlist.Events[i - 1].HasInitializeCommand = canBeInitialized;
                    //    if (i > 1) playlist.Events[i - 2].HasInitializeCommand = canBeInitialized;
                    //}
                    ///

                    if (((playlist.Events[i].IsPrimaryEvent && playlist.Events[i].IsUserPrimaryEvent) || playlist.Events[i].IsLiveEvent) && (string.IsNullOrEmpty(lastProgramCode) || playlist.Events[i].ProgramCode.Equals(lastProgramCode) == false))
                    {
                        if (string.IsNullOrEmpty(lastProgramCode) == false && playlist.Events[i].ProgramCode.Contains(lastProgramCode + "_"))
                        {
                            continue;
                        }
                        lastProgramCode = playlist.Events[i].ProgramCode;
                        primaryEvents.Add(playlist.Events[i]);
                    }
                }

                //Block
                if (comingUpList.Blocks != null && comingUpList.Blocks.Count > 0)
                {
                    foreach (var comingUpBlockItem in comingUpList.Blocks)
                    {
                        if (comingUpBlockItem.ProgramCode.Equals(currentEvent.ProgramCode, StringComparison.OrdinalIgnoreCase))
                        {
                            TimeSpan beginTime = Common.Utility.GetTimeSpanFromString(currentEvent.BeginTime);
                            if (comingUpList == this.Channel.Setting.CTomorrow || (comingUpBlockItem.HasBeginTime && comingUpBlockItem.BeginTime > beginTime))
                            {
                                beginTime = comingUpBlockItem.BeginTime;
                            }

                            comingUpBlockItem.Events = primaryEvents.Where(m => Common.Utility.GetTimeSpanFromString(m.BeginTime) >= beginTime && Common.Utility.GetTimeSpanFromString(m.BeginTime) <= comingUpBlockItem.EndTime).ToList();

                            //CToday_VTV2_yyyyMMdd_BL1_EventId.txt
                            string fileName = string.Format("{0}_{1}_{2}_{3}_{4}.txt", comingUpList.Name, this.Channel.Name, this.Date.ToString("yyyyMMdd"), comingUpBlockItem.Name, currentEvent.EventId);
                            filePath = Path.Combine(this.Channel.Setting.OlderPlaylistFolderPath, fileName);
                            SaveComingUpBlockDataToFile(comingUpBlockItem, filePath);
                            //break;
                        }
                    }
                }

                //TimeRange
                if (comingUpList.TimeRanges != null && comingUpList.TimeRanges.Count > 0)
                {
                    foreach (ComingUpTimeRangeModel comingUpTimeRangeItem in comingUpList.TimeRanges)
                    {
                        if (comingUpTimeRangeItem.ProgramCode.Equals(currentEvent.ProgramCode, StringComparison.OrdinalIgnoreCase))
                        {
                            //loại 1: từ begintime của sự kiện -> 12:00 (chỉ có thuộc tính endtime)
                            if (comingUpTimeRangeItem.HasCount == false && comingUpTimeRangeItem.HasEndTime)
                            {
                                comingUpTimeRangeItem.Events = primaryEvents.Where(m => Common.Utility.GetTimeSpanFromString(m.BeginTime) >= Common.Utility.GetTimeSpanFromString(currentEvent.BeginTime) && Common.Utility.GetTimeSpanFromString(m.BeginTime) <= comingUpTimeRangeItem.EndTime).ToList();
                            }

                            //loại 2: 0-6h, 6h-12h, 12-18h, 18-24h (không có thuộc tính)
                            if (comingUpTimeRangeItem.HasCount == false && comingUpTimeRangeItem.HasEndTime == false)
                            {
                                TimeSpan beginTime = Common.Utility.GetTimeSpanFromString(currentEvent.BeginTime);
                                TimeSpan endTime = TimeSpan.FromHours(6); //default
                                if (beginTime >= TimeSpan.FromHours(6) && beginTime <= TimeSpan.FromHours(12))
                                {
                                    endTime = TimeSpan.FromHours(12);
                                }
                                else if (beginTime >= TimeSpan.FromHours(12) && beginTime <= TimeSpan.FromHours(18))
                                {
                                    endTime = TimeSpan.FromHours(18);
                                }
                                else if (beginTime >= TimeSpan.FromHours(18) && beginTime <= TimeSpan.FromHours(30))
                                {
                                    endTime = TimeSpan.FromHours(30);
                                }

                                comingUpTimeRangeItem.Events = primaryEvents.Where(m => Common.Utility.GetTimeSpanFromString(m.BeginTime) >= beginTime && Common.Utility.GetTimeSpanFromString(m.BeginTime) <= endTime).ToList();
                            }

                            //loại 3: từ begintime của sự kiện -> count sự kiện tiếp theo (chỉ có thuộc tính count)
                            if (comingUpTimeRangeItem.HasCount && comingUpTimeRangeItem.HasEndTime == false)
                            {
                                comingUpTimeRangeItem.Events = primaryEvents.Where(m => Common.Utility.GetTimeSpanFromString(m.BeginTime) >= Common.Utility.GetTimeSpanFromString(currentEvent.BeginTime)).Take(comingUpTimeRangeItem.Count).ToList();
                            }

                            //CToday_VTV2_yyyyMMdd_EventId.txt
                            string fileName = string.Format("{0}_{1}_{2}_{3}.txt", comingUpList.Name, this.Channel.Name, this.Date.ToString("yyyyMMdd"), currentEvent.EventId);
                            filePath = Path.Combine(this.Channel.Setting.OlderPlaylistFolderPath, fileName);
                            SaveComingUpTimeRangeDataToFile(comingUpTimeRangeItem, filePath);
                        }
                    }
                }
            }
            return filePath;
        }

        public void AppendEventListToXmlNode(XmlDocument xmlDocument, XmlElement xmlParentNode, ObservableCollection<EventModel> eventList)
        {
            if (xmlDocument != null && xmlParentNode != null && eventList != null)
            {
                foreach (var eventItem in eventList)
                {
                    //Xử lý comingup
                    if (eventItem.IsComingUpEvent)
                    {
                        eventItem.CTodayFilePath = ProcessComingUpData(this.Channel.Setting.CToday, this, eventItem);
                        eventItem.CTomorrowFilePath = ProcessComingUpData(this.Channel.Setting.CTomorrow, this.Channel.GetPlaylistByDate(this.Date.AddDays(1)), eventItem);
                    }
                    ///

                    XmlElement nodeEvent = xmlDocument.CreateElement("event");
                    xmlParentNode.AppendChild(nodeEvent);

                    XmlElement nodeChild = null;
                    //event_type
                    nodeChild = xmlDocument.CreateElement("event_type");
                    nodeChild.SetAttribute("type", eventItem.EventType);
                    nodeEvent.AppendChild(nodeChild);

                    //stt
                    nodeChild = xmlDocument.CreateElement("stt");
                    nodeChild.SetAttribute("stt", eventItem.STT);
                    nodeEvent.AppendChild(nodeChild);

                    //date
                    nodeChild = xmlDocument.CreateElement("date");
                    nodeChild.SetAttribute("date", this.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    nodeEvent.AppendChild(nodeChild);

                    //event_id
                    nodeChild = xmlDocument.CreateElement("event_id");
                    nodeChild.SetAttribute("id", eventItem.EventId);
                    nodeEvent.AppendChild(nodeChild);

                    //mabang
                    nodeChild = xmlDocument.CreateElement("mabang");
                    nodeChild.SetAttribute("tapeid", eventItem.ProgramCode);
                    nodeEvent.AppendChild(nodeChild);

                    //description
                    nodeChild = xmlDocument.CreateElement("description");
                    nodeChild.SetAttribute("des", eventItem.Description);
                    nodeEvent.AppendChild(nodeChild);

                    //title
                    nodeChild = xmlDocument.CreateElement("title");
                    nodeChild.SetAttribute("ten_ct", eventItem.Title);
                    nodeEvent.AppendChild(nodeChild);

                    //begin_time
                    nodeChild = xmlDocument.CreateElement("begin_time");
                    nodeChild.SetAttribute("time", eventItem.BeginTime);
                    nodeEvent.AppendChild(nodeChild);

                    //begin_time2
                    nodeChild = xmlDocument.CreateElement("begin_time2");
                    nodeChild.SetAttribute("time", eventItem.BeginTime2);
                    nodeEvent.AppendChild(nodeChild);

                    //duration
                    nodeChild = xmlDocument.CreateElement("duration");
                    nodeChild.SetAttribute("time", eventItem.Duration);
                    nodeEvent.AppendChild(nodeChild);

                    //tc_vao
                    nodeChild = xmlDocument.CreateElement("tc_vao");
                    nodeChild.SetAttribute("tc_vao", eventItem.TcVao);
                    nodeEvent.AppendChild(nodeChild);

                    //tc_ra
                    nodeChild = xmlDocument.CreateElement("tc_ra");
                    nodeChild.SetAttribute("tc_ra", eventItem.TcRa);
                    nodeEvent.AppendChild(nodeChild);

                    //noidung
                    nodeChild = xmlDocument.CreateElement("noidung");
                    nodeChild.SetAttribute("noi_dung", eventItem.NoiDung);
                    nodeEvent.AppendChild(nodeChild);

                    //////phía dưới này chưa xong
                    //tiengviet
                    nodeChild = xmlDocument.CreateElement("tiengviet");
                    string tenMu = " ";
                    string tenCt = " ";
                    if (string.IsNullOrEmpty(eventItem.TenMu) == false && eventItem.IsValidTenMuLength)
                    {
                        tenMu = eventItem.TenMu;
                    }
                    if (string.IsNullOrEmpty(eventItem.TenCt) == false && eventItem.IsValidTenCtLength)
                    {
                        tenCt = eventItem.TenCt;
                    }

                    nodeChild.SetAttribute("ten_mu", tenMu);
                    nodeChild.SetAttribute("ten_ct", tenCt);
                    nodeChild.SetAttribute("status", eventItem.Status);
                    nodeChild.SetAttribute("modify", eventItem.Modify);
                    nodeEvent.AppendChild(nodeChild);

                    //loai_ct
                    nodeChild = xmlDocument.CreateElement("loai_ct");
                    nodeChild.SetAttribute("skc", eventItem.IsUserPrimaryEvent ? "1" : "0");
                    nodeChild.SetAttribute("dem_dohoa", eventItem.IsTemporaryCGEvent ? "1" : "0");
                    nodeChild.SetAttribute("qc", eventItem.IsAdvertismentEvent ? "1" : "0");
                    nodeChild.SetAttribute("initialize", eventItem.HasInitializeCommand ? "1" : "0");
                    nodeChild.SetAttribute("cleanup", eventItem.HasCleanupCommand ? "1" : "0");
                    nodeEvent.AppendChild(nodeChild);

                    //dem_nguoc
                    nodeChild = xmlDocument.CreateElement("dem_nguoc");
                    nodeChild.SetAttribute("duration", eventItem.CountDownSecondDuration.ToString());
                    nodeChild.SetAttribute("valid", eventItem.CanShowCG && eventItem.CanShowCountDown && eventItem.CountDownSecondDuration <= (int)this.Channel.Setting.MaximumCountDownDuration.TotalSeconds ? "1" : "0");
                    nodeEvent.AppendChild(nodeChild);

                    //hienthi
                    nodeChild = xmlDocument.CreateElement("hienthi");
                    nodeChild.SetAttribute("do_hoa", eventItem.CanShowCG ? "1" : "0");
                    nodeChild.SetAttribute("dang_xem", eventItem.CanShowNowProgram ? "1" : "0");
                    nodeChild.SetAttribute("tiep_theo", eventItem.CanShowNextProgram ? "1" : "0");
                    nodeChild.SetAttribute("dem_nguoc", eventItem.CanShowCountDown ? "1" : "0");
                    nodeChild.SetAttribute("dac_sac", eventItem.CanShowSpecial ? "1" : "0");
                    nodeEvent.AppendChild(nodeChild);

                    //next_event
                    nodeChild = xmlDocument.CreateElement("next_event");
                    string next_event_ten_mu = " ";
                    string next_event_ten_ct = " ";
                    if (eventItem.NextPENotSameProgramCode != null && string.IsNullOrEmpty(eventItem.NextPENotSameProgramCode.TenMu) == false)
                    {
                        next_event_ten_mu = eventItem.NextPENotSameProgramCode.TenMu;
                    }
                    if (eventItem.NextPENotSameProgramCode != null && string.IsNullOrEmpty(eventItem.NextPENotSameProgramCode.TenCt) == false)
                    {
                        next_event_ten_ct = eventItem.NextPENotSameProgramCode.TenCt;
                    }
                    nodeChild.SetAttribute("ten_mu", next_event_ten_mu);
                    nodeChild.SetAttribute("ten_ct", next_event_ten_ct);
                    nodeEvent.AppendChild(nodeChild);

                    //dac_sac
                    nodeChild = xmlDocument.CreateElement("dac_sac");
                    nodeChild.SetAttribute("ten_mu", "");
                    nodeChild.SetAttribute("ten_ct", "");
                    nodeChild.SetAttribute("mota", "");
                    nodeChild.SetAttribute("tg", "");
                    nodeChild.SetAttribute("link", "");
                    nodeEvent.AppendChild(nodeChild);


                    //ctoday
                    nodeChild = xmlDocument.CreateElement("ctoday");
                    nodeChild.SetAttribute("today", this.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    nodeChild.SetAttribute("ctodaylink", eventItem.CTodayFilePath.Replace(@"\", @"\\"));
                    string cTodayContent = FileManager.LoadTextFromFile(eventItem.CTodayFilePath).Trim();
                    nodeChild.SetAttribute("valid", string.IsNullOrEmpty(cTodayContent) == false ? "1" : "0");
                    nodeEvent.AppendChild(nodeChild);

                    //cTomorrow
                    nodeChild = xmlDocument.CreateElement("cTomorrow");
                    nodeChild.SetAttribute("Tomorrow", this.Date.AddDays(1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    nodeChild.SetAttribute("cTomorrowlink", eventItem.CTomorrowFilePath.Replace(@"\", @"\\"));
                    string cTomorrowContent = FileManager.LoadTextFromFile(eventItem.CTomorrowFilePath).Trim();
                    nodeChild.SetAttribute("valid", string.IsNullOrEmpty(cTomorrowContent) == false ? "1" : "0");
                    nodeEvent.AppendChild(nodeChild);
                    ///

                    //secondaryeventlist
                    if (eventItem.SecondaryEvents != null && eventItem.SecondaryEvents.Count > 0)
                    {
                        nodeChild = xmlDocument.CreateElement("secondaryeventlist");
                        nodeEvent.AppendChild(nodeChild);
                        for (int i = 0; i < eventItem.SecondaryEvents.Count; i++)
                        {
                            XmlElement nodeSecondaryEvent = xmlDocument.CreateElement("secondaryevent");
                            nodeSecondaryEvent.SetAttribute("type", "SECONDARY");
                            nodeChild.AppendChild(nodeSecondaryEvent);

                            XmlElement nodeSecondaryEventDetail = xmlDocument.CreateElement("detail");
                            nodeSecondaryEventDetail.SetAttribute("pagename", eventItem.SecondaryEvents[i].Name);
                            nodeSecondaryEventDetail.SetAttribute("offset", Common.Utility.GetTimeAndFrameStringFromTimeSpan(eventItem.SecondaryEvents[i].Offset));
                            nodeSecondaryEventDetail.SetAttribute("duration", Common.Utility.GetTimeAndFrameStringFromTimeSpan(eventItem.SecondaryEvents[i].Duration));
                            nodeSecondaryEvent.AppendChild(nodeSecondaryEventDetail);
                        }
                    }
                    ///
                }
            }
        }

        public void SavePlayListToFile(string filePath)
        {
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
            }
            catch (Exception) { }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement nodePlaylist = xmlDocument.CreateElement("playlist");
                nodePlaylist.SetAttribute("description", "Playlist for " + this.Channel.Name);
                nodePlaylist.SetAttribute("session_id", AppData.Default.SessionId.ToString());
                xmlDocument.AppendChild(nodePlaylist);

                XmlElement nodeGereralInfo = xmlDocument.CreateElement("general_info");
                nodeGereralInfo.SetAttribute("date", this.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                nodeGereralInfo.SetAttribute("channel", this.Channel.Name);
                nodePlaylist.AppendChild(nodeGereralInfo);

                UpdateValidEvents();
                AppendEventListToXmlNode(xmlDocument, nodePlaylist, this.Events);

                //Kiểm tra, nếu file đang được dùng bởi ứng dụng khác thì chờ 1 lúc
                while (FileManager.IsFileLocked(new FileInfo(filePath)))
                {
                    Thread.Sleep(100);
                }

                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    xmlDocument.Save(sw);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                //LogUtility.Save("SavePlayListToFile - " + this.Date + ": " + ex.Message);
            }

            if (this.Channel != null)
            {
                this.Channel.SaveMergePlaylistToFile(Path.Combine(this.Channel.Setting.MergePlaylistFolderPath, "Today_" + this.Channel.Name + ".xml"));
            }
        }

        public void CopyProperties(PlaylistModel anotherPlaylist, bool fromNetwork)
        {
            if (anotherPlaylist != null)
            {
                //private DateTime lastUpdateTime = DateTime.MinValue;
                //private string lastUpdateFilePath = "";

                //private bool reentrancyCheckCanShowAllCG = false;
                //private bool reentrancyCheckCanShowAllNowProgram = false;
                //private bool reentrancyCheckCanShowAllNextProgram = false;
                //private bool reentrancyCheckCanShowAllCountDown = false;
                //private bool? canShowAllCG = null;
                //private bool? canShowAllNowProgram = null;
                //private bool? canShowAllNextProgram = null;
                //private bool? canShowAllCountDown = null;

                this.LastPlaylistUpdateFileTime = anotherPlaylist.LastPlaylistUpdateFileTime;
                this.LastPlaylistUpdateFilePath = anotherPlaylist.LastPlaylistUpdateFilePath;
                this.Events = anotherPlaylist.Events;
                this.CanShowAllCG = anotherPlaylist.CanShowAllCG;
                this.CanShowAllNowProgram = anotherPlaylist.CanShowAllNowProgram;
                this.CanShowAllCountDown = anotherPlaylist.CanShowAllCountDown;
                this.CanShowAllNextProgram = anotherPlaylist.CanShowAllNextProgram;
            }
        }

    }
}
