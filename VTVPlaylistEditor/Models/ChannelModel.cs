using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using AutoMapper;
using ChiDuc.General;
using MoreLinq;
using Prism.Mvvm;

namespace VTVPlaylistEditor.Models
{
    public class ChannelModel : BindableBase
    {
        private string _name = "";
        private string _title = "";
        private string _description = "";
        private string _lastTrafficUpdateFilePath = "";
        private DateTime _lastTrafficUpdateFileTime = DateTime.MinValue;
        private ObservableCollection<TrafficEventModel> _trafficEvents = new ObservableCollection<TrafficEventModel>();
        private ObservableCollection<PlaylistModel> _playlists = new ObservableCollection<PlaylistModel>();
        private PlaylistModel _playlistSelected = null;
        private ChannelSettingModel _setting = null;

        private Thread _trafficProcessorThread = null;
        private bool _isTrafficProcessorThreadRunning = false;
        private DateTime _lastTrafficProcessorUpdateTime = DateTime.MinValue;

        private Thread _dataProcessorThread = null;
        private bool _isDataProcessorThreadRunning = false;

        private EventModel _edittingEvent = null;
        private DateTime _lastMuCImporterProgramRunning = DateTime.Now;
        private int _canRunMuCImporterProgram = 0;

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                this._name = value;
                OnPropertyChanged(() => this.Name);

                if (string.IsNullOrEmpty(this.Title))
                {
                    this.Title = _name;
                }
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                _title = Common.Utility.TrimText(value);

                if (string.IsNullOrEmpty(_title))
                {
                    _title = _name;
                }
                OnPropertyChanged(() => this.Title);
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                this._description = value;
                OnPropertyChanged(() => this.Description);
            }
        }

        public string LastTrafficUpdateFilePath
        {
            get
            {
                return _lastTrafficUpdateFilePath;
            }

            set
            {
                this._lastTrafficUpdateFilePath = value;
                OnPropertyChanged(() => this.LastTrafficUpdateFilePath);
            }
        }

        public DateTime LastTrafficUpdateFileTime
        {
            get
            {
                return _lastTrafficUpdateFileTime;
            }

            set
            {
                this._lastTrafficUpdateFileTime = value;
                OnPropertyChanged(() => this.LastTrafficUpdateFileTime);
            }
        }

        public ObservableCollection<TrafficEventModel> TrafficEvents
        {
            get
            {
                return _trafficEvents;
            }

            set
            {
                this._trafficEvents = value;
                OnPropertyChanged(() => this.TrafficEvents);
            }
        }

        public ObservableCollection<PlaylistModel> Playlists
        {
            get
            {
                return _playlists;
            }

            set
            {
                this._playlists = value;
                OnPropertyChanged(() => this.Playlists);

                FilterPlaylistSelected();
            }
        }

        public PlaylistModel PlaylistSelected
        {
            get
            {
                return _playlistSelected;
            }

            set
            {
                this._playlistSelected = value;
                OnPropertyChanged(() => this.PlaylistSelected);

                FilterPlaylistSelected();
            }
        }

        public bool HasNewEvent
        {
            get
            {
                if (this.Playlists != null)
                {
                    foreach (var playlist in this.Playlists)
                    {
                        if (playlist.HasNewEvent)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public EventModel EdittingEvent
        {
            get
            {
                return _edittingEvent;
            }

            set
            {
                this._edittingEvent = value;
                OnPropertyChanged(() => this.EdittingEvent);
            }
        }

        public ChannelSettingModel Setting
        {
            get
            {
                return _setting;
            }
        }

        public ChannelModel()
        {
            _setting = new ChannelSettingModel(this);
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
                UpdateData();
                Thread.Sleep(250);
            }
            _dataProcessorThread = null;
            GC.Collect();
        }

        public void StopDataProcessor()
        {
            _isDataProcessorThreadRunning = false;
            foreach (var playlist in this.Playlists)
            {
                playlist.StopDataProcessor();
            }
        }

        public void UpdateData()
        {
            try
            {
                //Cập nhật trạng thái HasNewEvent
                OnPropertyChanged(() => this.HasNewEvent);

                //Tự động xóa playlist nếu không đúng setting
                for (int i = 0; i < this.Playlists.Count && _isDataProcessorThreadRunning; i++)
                {
                    PlaylistModel playlist = this.Playlists[i];
                    if (playlist.Date.Date < DateTime.Now.Date.AddDays(0 - this.Setting.CountOfPreviousDays) ||
                        playlist.Date.Date > DateTime.Now.Date.AddDays(this.Setting.CountOfNextDays))
                    {
                        playlist.StopDataProcessor();
                        if (this.PlaylistSelected == playlist)
                        {
                            this.PlaylistSelected = null;
                        }

                        //safe thread
                        System.Windows.Application.Current.Dispatcher.Invoke(delegate
                        {
                            this.Playlists.Remove(playlist);
                        });

                        if (this.PlaylistSelected == null && this.Playlists.Count > 0)
                        {
                            this.PlaylistSelected = this.Playlists[0];
                        }

                        GC.Collect();
                        --i;
                    }
                }
                ///

                //Tự động thêm playlist khi qua ngày theo setting
                CreatePlaylistBySetting();
                ///

                //Tự động chạy trình MuC Importer nếu được cho phép
                if (DateTime.Now - AppData.Default.MuCImporterProgramDelayInterval >= _lastMuCImporterProgramRunning)
                {
                    while (_canRunMuCImporterProgram > 0 && _isDataProcessorThreadRunning)
                    {
                        RunMuCImporterProgram();
                        _lastMuCImporterProgramRunning = DateTime.Now;
                        --_canRunMuCImporterProgram;
                    }
                }
                ///
            }
            catch (Exception) { }
        }

        public void CreatePlaylistBySetting()
        {
            //Tạo ra danh sách playlist ứng với thiết lập CountOfPreviousDays và CountOfNextDays
            for (int i = 0 - this.Setting.CountOfPreviousDays; i <= this.Setting.CountOfNextDays; i++)
            {
                DateTime date = DateTime.Now.Date.AddDays(i);

                bool isExisted = false;
                for (int j = 0; j < this.Playlists.Count; j++)
                {
                    if (this.Playlists[j].Date.Date == date)
                    {
                        isExisted = true;
                        break;
                    }
                }

                if (isExisted == false)
                {
                    //safe thread
                    System.Windows.Application.Current.Dispatcher.Invoke(delegate
                    {
                        PlaylistModel playlistModel = new PlaylistModel(this) { Date = date };
                        playlistModel.StartDataProcessor();

                        int insertAtIndex = -1;
                        if (this.Playlists.Count > 0)
                        {
                            for (int ii = 0; ii < this.Playlists.Count; ii++)
                            {
                                if (this.Playlists[ii].Date.Date >= date.AddDays(1))
                                {
                                    insertAtIndex = ii - 1;
                                    if (insertAtIndex < 0)
                                    {
                                        insertAtIndex = 0;
                                    }
                                    break;
                                }
                            }
                        }

                        if (insertAtIndex == -1)
                        {
                            this.Playlists.Add(playlistModel);
                        }
                        else
                        {
                            this.Playlists.Insert(insertAtIndex, playlistModel);
                        }
                    });
                }
            }

            if (this.PlaylistSelected == null && this.Playlists.Count > 0)
            {
                this.PlaylistSelected = this.Playlists[0];
            }
        }

        public void FilterPlaylistSelected()
        {
            if (this.PlaylistSelected != null && this.PlaylistSelected.Events != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(this.PlaylistSelected.Events);
                if (view != null)
                {
                    switch (AppData.Default.PlaylistFilterSelectedIndex)
                    {
                        case 0:
                            view.Filter = null;
                            break;
                        default: //1
                            view.Filter = new Predicate<object>(x => ((EventModel)x).IsPrimaryEvent || ((EventModel)x).IsLiveEvent || ((EventModel)x).IsUserPrimaryEvent || ((EventModel)x).IsComingUpEvent);
                            break;
                    }
                }
            }
        }

        private void RunMuCImporterProgram()
        {
            if (AppData.Default.EnableMucImporterProgram && File.Exists(AppData.Default.MuCImporterProgramFilePath))
            {
                //Thread.Sleep(500);
                // Prepare the process to run
                ProcessStartInfo start = new ProcessStartInfo();
                // Enter in the command line arguments, everything you would enter after the executable name itself
                //start.Arguments = arguments;
                // Enter the executable to run, including the complete path
                start.FileName = AppData.Default.MuCImporterProgramFilePath;
                // Do you want to show a console window?
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.CreateNoWindow = true;

                // Run the external process & wait for it to finish
                try
                {
                    Process proc = Process.Start(start);
                }
                catch (Exception ex) { }
            }
        }

        public void StartTrafficProcessor()
        {
            StopTrafficProcessor();
            _isTrafficProcessorThreadRunning = true;
            _trafficProcessorThread = new Thread(new ThreadStart(ExecuteTrafficProcessorThread));
            _trafficProcessorThread.IsBackground = true;
            _trafficProcessorThread.Start();
        }

        private void ExecuteTrafficProcessorThread()
        {
            while (_isTrafficProcessorThreadRunning)
            {
                if (DateTime.Now - _lastTrafficProcessorUpdateTime >= AppData.Default.TrafficUpdateInterval)
                {
                    UpdateTrafficData();
                    _lastTrafficProcessorUpdateTime = DateTime.Now;
                }
                Thread.Sleep(250);
            }
            _trafficProcessorThread = null;
            GC.Collect();
        }

        public void StopTrafficProcessor()
        {
            _isTrafficProcessorThreadRunning = false;
        }

        public void UpdateTrafficData()
        {
            try
            {
                var channelEntity = DataService.Service.Default.GetChannel(this.Name);
                if (channelEntity != null)
                {
                    if (this.TrafficEvents.Count != DataService.Service.Default.CountOfTrafficEvents(this.Name) || this.LastTrafficUpdateFileTime != channelEntity.LastTrafficUpdateFileTime)
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            var trafficEventEntities = DataService.Service.Default.GetTrafficEvents(this.Name);
                            this.TrafficEvents = AppData.Mapper.Map<List<BusinessObjects.TrafficEvent>, ObservableCollection<TrafficEventModel>>(trafficEventEntities);
                        });
                        this.SyncDataFromTrafficToPlaylist(this.EdittingEvent);
                    }
                }

                //var channelEntity = DataService.Service.Default.GetChannel(this.Name);
                //if (channelEntity != null)
                //{
                //    if (this.LastTrafficUpdateFileTime != channelEntity.LastTrafficUpdateFileTime)
                //    {
                //        var trafficEventEntities = DataService.Service.Default.GetTrafficEvents(this.Name);
                //        var newTrafficEventModels = AppData.Mapper.Map<List<BusinessObjects.TrafficEvent>, ObservableCollection<TrafficEventModel>>(trafficEventEntities);

                //        //1. Tìm và xóa các item không có trong csdl
                //        var exceptList = this.TrafficEvents.Where(l1 => !newTrafficEventModels.Any(l2 => l2.ProgramCode.Equals(l1.ProgramCode, StringComparison.OrdinalIgnoreCase))).ToList();
                //        foreach (var exceptItem in exceptList)
                //        {
                //            if (_isTrafficProcessorThreadRunning == false)
                //                return;

                //            Application.Current.Dispatcher.Invoke(delegate
                //            {
                //                this.TrafficEvents.Remove(exceptItem);
                //            });
                //        }

                //        //2. Cập nhật giá trị các item trong danh sách
                //        var tempList = new List<TrafficEventModel>(newTrafficEventModels);
                //        foreach (var trafficEventModel in this.TrafficEvents)
                //        {
                //            if (_isTrafficProcessorThreadRunning == false)
                //                return;

                //            for (int i = 0; i < tempList.Count && _isTrafficProcessorThreadRunning; i++)
                //            {
                //                if (trafficEventModel.ProgramCode.Equals(tempList[i].ProgramCode, StringComparison.OrdinalIgnoreCase) && trafficEventModel.UpdateTime != tempList[i].UpdateTime)
                //                {
                //                    trafficEventModel.ProgramTitle1 = tempList[i].ProgramTitle1;
                //                    trafficEventModel.ProgramTitle2 = tempList[i].ProgramTitle2;
                //                    trafficEventModel.UpdateTime = tempList[i].UpdateTime;
                //                    tempList.RemoveAt(i);
                //                    break;
                //                }
                //            }
                //        }

                //        //3. Tìm và thêm các item có trong csdl mà chưa có trong danh sách
                //        exceptList = newTrafficEventModels.Where(l1 => !this.TrafficEvents.Any(l2 => l2.ProgramCode.Equals(l1.ProgramCode, StringComparison.OrdinalIgnoreCase))).ToList();
                //        Application.Current.Dispatcher.Invoke(delegate
                //        {
                //            this.TrafficEvents.AddRange(exceptList);
                //        });

                //        this.LastTrafficUpdateFileTime = channelEntity.LastTrafficUpdateFileTime;
                //        this.LastTrafficUpdateFilePath = channelEntity.LastTrafficUpdateFilePath;
                //        Debug.WriteLine("UpdateTrafficData");
                //    }
                //}
            }
            catch (Exception) { }
        }

        public int SyncDataFromTrafficToPlaylist(EventModel excludedEvent)
        {
            int result = 0;
            try
            {
                if (this.TrafficEvents != null)
                {
                    foreach (PlaylistModel playlistModel in this.Playlists)
                    {
                        if (playlistModel.Events != null)
                        {
                            foreach (EventModel eventModel in playlistModel.Events)
                            {
                                if (eventModel != excludedEvent && eventModel.Modify != "1")
                                {
                                    result += SyncDataFromTrafficToEvent(eventModel);
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
            return result;
        }

        public int SyncDataFromTrafficToPlaylist(ObservableCollection<EventModel> events, EventModel excludedEvent)
        {
            int result = 0;
            try
            {
                if (this.TrafficEvents != null && events != null)
                {
                    foreach (EventModel eventModel in events)
                    {
                        if (eventModel != excludedEvent && eventModel.Modify != "1")
                        {
                            result += SyncDataFromTrafficToEvent(eventModel);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //throw;
            }
            return result;
        }

        public int SyncDataFromTrafficToEvent(EventModel eventModel)
        {
            int result = 0;
            try
            {
                if (this.TrafficEvents != null && eventModel != null)
                {
                    foreach (TrafficEventModel trafficEventModel in this.TrafficEvents)
                    {
                        if (eventModel.ProgramCode.Equals(trafficEventModel.ProgramCode, StringComparison.OrdinalIgnoreCase) || eventModel.GroupName.Equals(trafficEventModel.ProgramCode, StringComparison.OrdinalIgnoreCase))
                        {
                            eventModel.TenMu = trafficEventModel.ProgramTitle1;
                            eventModel.TenCt = trafficEventModel.ProgramTitle2;
                            ++result;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //throw;
            }
            return result;
        }

        public void SaveMergePlaylistToFile(string filePath)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement nodePlaylist = xmlDocument.CreateElement("playlist");
                nodePlaylist.SetAttribute("description", "Playlist for " + this.Name);
                xmlDocument.AppendChild(nodePlaylist);

                XmlElement nodeGereralInfo = xmlDocument.CreateElement("general_info");
                nodeGereralInfo.SetAttribute("date", DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                nodeGereralInfo.SetAttribute("channel", this.Name);
                nodePlaylist.AppendChild(nodeGereralInfo);

                if (this.Playlists != null)
                {
                    for (int i = 0; i < this.Playlists.Count; i++)
                    {
                        bool isValid = this.Playlists[i].Date.Date == DateTime.Now.Date || (this.Playlists[i].Date.Date == DateTime.Now.AddDays(1).Date && DateTime.Now.Hour >= 0);
                        if (isValid)
                        {
                            //Chèn sự kiện ngăn cách giữa 2 lịch phát sóng
                            if (i > 0 && this.Playlists.Count >= 2 && i <= this.Playlists.Count - 1)
                            {
                                this.Playlists[i - 1].AppendEventListToXmlNode(xmlDocument, nodePlaylist, new ObservableCollection<EventModel>() {
                                    new EventModel(this)
                                    {
                                        EventType="PRIMARY",
                                        STT="999",
                                        EventId="||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||",
                                        ProgramCode="||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||",
                                        Description="||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||:||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||",
                                        Title="||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||",
                                        NoiDung="||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||",
                                        BeginTime="23:59:59",
                                        BeginTime2="23:59:59",
                                        Duration="",
                                        Status="0"
                                    }
                                });
                            }
                            ///

                            this.Playlists[i].UpdateValidEvents();
                            this.Playlists[i].AppendEventListToXmlNode(xmlDocument, nodePlaylist, this.Playlists[i].Events);
                        }
                    }
                }

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

                ++_canRunMuCImporterProgram;
            }
            catch (Exception ex)
            {
                //throw;
                //SaveMergePlaylistToFile(filePath);
            }
        }

        public PlaylistModel GetPlaylistByDate(DateTime date)
        {
            if (this.Playlists != null)
            {
                foreach (PlaylistModel playlistItem in this.Playlists)
                {
                    if (playlistItem.Date.Date == date.Date)
                    {
                        return playlistItem;
                    }
                }
            }
            return null;
        }

    }
}
