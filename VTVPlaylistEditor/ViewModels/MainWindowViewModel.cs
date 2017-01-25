using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ChiDuc.General;
using ChiDuc.General.WPF.UI;
using Common.Models;
using Common.ViewModels;
using Common.Views;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using VTVPlaylistEditor.Models;
using VTVPlaylistEditor.Views;

namespace VTVPlaylistEditor.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public ICommand ExitApplicationCommand { get; private set; }
        public ICommand OpenSettingWindowCommand { get; private set; }
        public ICommand OpenAboutWindowCommand { get; private set; }
        public ICommand SearchTrafficEventCommand { get; private set; }
        public ICommand ClearSearchTrafficEventCommand { get; private set; }

        public ICommand ProcessSelectedPlaylistCommand { get; private set; }
        public ICommand SyncDataFromTrafficToSelectedPlaylistCommand { get; private set; }
        public ICommand SyncDataFromTrafficToSelectedEventCommand { get; private set; }
        public ICommand SetSelectedEventToDefaultCommand { get; private set; }
        public ICommand SetSelectedEventToUserEventCommand { get; private set; }

        public ICommand BeginningEditCommand { get; private set; }
        public ICommand CellEditEndingCommand { get; private set; }
        public ICommand CellEditingLostFocusCommand { get; private set; }

        public ICommand CheckBoxCellCheckedChangedCommand { get; private set; }
        public ICommand CheckBoxColumnCheckedChangedCommand { get; private set; }

        public ICommand PressKeyOnCellCommand { get; private set; }
        public ICommand OpenSpecialSettingEventCommand { get; private set; }

        private ChannelModel _channelSelected = null;
        private EventModel _eventSelected = null;
        private bool _isSearchTrafficEventOpenning = false;
        private string _searchTrafficEventKeyWord = "";
        private string _lastSearchTrafficEventKeyWord = "";
        private bool _isRootDialogOpenning = false;
        private bool _isPlaylistDialogOpenning = false;
        private object _playlistDialogContent = null;
        private ConnectionStringModel _currentConnectionString = null;
        private bool _canBeSearchedWhenEventSelected = false;

        private Thread _dataProcessorThread = null;
        private bool _isDataProcessorThreadRunning = false;
        private DateTime _lastCommunicationUpdateInterval = DateTime.MinValue;

        private ComingUpViewerWindow _wndComingUpViewer = null;

        public ChannelModel ChannelSelected
        {
            get
            {
                return _channelSelected;
            }

            set
            {
                this._channelSelected = value;
                OnPropertyChanged(() => this.ChannelSelected);

                this.EventSelected = null;
                if (this.ChannelSelected != null)
                {
                    this.ChannelSelected.FilterPlaylistSelected();
                }
                ExecuteClearSearchTrafficEventCommand();
            }
        }

        public EventModel EventSelected
        {
            get
            {
                return _eventSelected;
            }

            set
            {
                this._eventSelected = value;
                OnPropertyChanged(() => this.EventSelected);

                //if (this.EventSelected != null && this.IsSearchTrafficEventOpenning && (string.IsNullOrEmpty(this.SearchTrafficEventKeyWord) || string.IsNullOrWhiteSpace(this.SearchTrafficEventKeyWord)))
                if (this.EventSelected != null && this.IsSearchTrafficEventOpenning && this.CanBeSearchedWhenEventSelected)
                {
                    SearchTrafficEvent(this.EventSelected.ProgramCode);
                }
            }
        }

        public string SearchTrafficEventKeyWord
        {
            get
            {
                return _searchTrafficEventKeyWord;
            }

            set
            {
                this._searchTrafficEventKeyWord = value;
                OnPropertyChanged(() => this.SearchTrafficEventKeyWord);
            }
        }

        public string LastSearchTrafficEventKeyWord
        {
            get
            {
                return _lastSearchTrafficEventKeyWord;
            }

            set
            {
                this._lastSearchTrafficEventKeyWord = value;
                OnPropertyChanged(() => this.LastSearchTrafficEventKeyWord);
            }
        }

        public bool IsRootDialogOpenning
        {
            get
            {
                return _isRootDialogOpenning;
            }

            set
            {
                this._isRootDialogOpenning = value;
                OnPropertyChanged(() => this.IsRootDialogOpenning);
            }
        }

        public bool IsPlaylistDialogOpenning
        {
            get
            {
                return _isPlaylistDialogOpenning;
            }

            set
            {
                this._isPlaylistDialogOpenning = value;
                OnPropertyChanged(() => this.IsPlaylistDialogOpenning);
            }
        }

        public object PlaylistDialogContent
        {
            get
            {
                return _playlistDialogContent;
            }

            set
            {
                this._playlistDialogContent = value;
                OnPropertyChanged(() => this.PlaylistDialogContent);
            }
        }

        public int PlaylistFilterSelectedIndex
        {
            get
            {
                return AppData.Default.PlaylistFilterSelectedIndex;
            }

            set
            {
                AppData.Default.PlaylistFilterSelectedIndex = value;
                OnPropertyChanged(() => this.PlaylistFilterSelectedIndex);

                this.EventSelected = null;
                if (this.ChannelSelected != null)
                {
                    this.ChannelSelected.FilterPlaylistSelected();
                }
            }
        }

        public bool HasEventInSelectedPlaylist
        {
            get
            {
                return this.ChannelSelected != null && this.ChannelSelected.PlaylistSelected != null && this.ChannelSelected.PlaylistSelected.Events != null && this.ChannelSelected.PlaylistSelected.Events.Count > 0;
            }
        }

        public bool IsSearchTrafficEventOpenning
        {
            get
            {
                return _isSearchTrafficEventOpenning;
            }

            set
            {
                this._isSearchTrafficEventOpenning = value;
                OnPropertyChanged(() => this.IsSearchTrafficEventOpenning);
            }
        }

        public ConnectionStringModel CurrentConnectionString
        {
            get
            {
                return _currentConnectionString;
            }

            set
            {
                this._currentConnectionString = value;
                OnPropertyChanged(() => this.CurrentConnectionString);
            }
        }

        public bool CanBeSearchedWhenEventSelected
        {
            get
            {
                return _canBeSearchedWhenEventSelected;
            }

            set
            {
                this._canBeSearchedWhenEventSelected = value;
                OnPropertyChanged(() => this.CanBeSearchedWhenEventSelected);
            }
        }

        public MainWindowViewModel()
        {
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            this.ExitApplicationCommand = new DelegateCommand(ExecuteExitApplicationCommand);
            this.OpenSettingWindowCommand = new DelegateCommand(ExecuteOpenSettingWindowCommand);
            this.OpenAboutWindowCommand = new DelegateCommand(ExecuteOpenAboutWindowCommand);
            this.SearchTrafficEventCommand = new DelegateCommand(ExecuteSearchTrafficEventCommand);
            this.ClearSearchTrafficEventCommand = new DelegateCommand(ExecuteClearSearchTrafficEventCommand);

            this.ProcessSelectedPlaylistCommand = new DelegateCommand(ExecuteProcessSelectedPlaylistCommand);
            this.SyncDataFromTrafficToSelectedPlaylistCommand = new DelegateCommand(ExecuteSyncDataFromTrafficToSelectedPlaylistCommand);
            this.SyncDataFromTrafficToSelectedEventCommand = new DelegateCommand(ExecuteSyncDataFromTrafficToSelectedEventCommand);
            this.SetSelectedEventToDefaultCommand = new DelegateCommand(ExecuteSetSelectedEventToDefaultCommand);
            this.SetSelectedEventToUserEventCommand = new DelegateCommand<object>(ExecuteSetSelectedEventToUserEventCommand);

            this.BeginningEditCommand = new DelegateCommand<DataGrid>(ExecuteBeginningEditCommand);
            this.CellEditEndingCommand = new DelegateCommand<DataGrid>(ExecuteCellEditEndingCommand);
            this.CellEditingLostFocusCommand = new DelegateCommand<DataGrid>(ExecuteCellEditingLostFocusCommand);
            this.CheckBoxCellCheckedChangedCommand = new DelegateCommand<object>(ExecuteCheckBoxCellCheckedChangedCommand);
            this.CheckBoxColumnCheckedChangedCommand = new DelegateCommand(ExecuteCheckBoxColumnCheckedChangedCommand);
            this.PressKeyOnCellCommand = new DelegateCommand<DataGrid>(ExecutePressKeyOnCellCommand);
            this.OpenSpecialSettingEventCommand = new DelegateCommand<object>(ExecuteOpenSpecialSettingEventCommand);

            _isDataProcessorThreadRunning = true;
            _dataProcessorThread = new Thread(new ThreadStart(ExecuteDataProcessorThread));
            _dataProcessorThread.IsBackground = true;
            _dataProcessorThread.Start();

            if (AppData.Default.Channels.Count > 0)
            {
                this.ChannelSelected = AppData.Default.Channels[0];
            }
        }

        private void FilterChannelSelected()
        {

            //////////this.HasEventInSelectedPlaylist = false;
            //////////if (this.ChannelSelected != null && this.ChannelSelected.PlaylistSelected != null)
            //////////{
            //////////    this.HasEventInSelectedPlaylist = this.ChannelSelected.PlaylistSelected.Events != null && this.ChannelSelected.PlaylistSelected.Events.Count > 0;
            //////////}

            //////////////////if (this.ChannelSelected != null && this.ChannelSelected.PlaylistSelected != null && this.ChannelSelected.PlaylistSelected.Events != null)
            //////////////////{
            //////////////////    ICollectionView view = CollectionViewSource.GetDefaultView(this.ChannelSelected.PlaylistSelected.Events);
            //////////////////    if (view != null)
            //////////////////    {
            //////////////////        switch (this.PlaylistFilterSelectedIndex)
            //////////////////        {
            //////////////////            case 0:
            //////////////////                view.Filter = null;
            //////////////////                break;
            //////////////////            default: //1
            //////////////////                view.Filter = new Predicate<object>(x => ((EventModel)x).IsPrimaryEvent || ((EventModel)x).IsLiveEvent || ((EventModel)x).IsUserPrimaryEvent || ((EventModel)x).IsComingUpEvent);
            //////////////////                break;
            //////////////////        }
            //////////////////        this.EventSelected = null;
            //////////////////    }
            //////////////////}

            //////////if (this.HasEventInSelectedPlaylist)
            //////////{
            //////////    this.PlaylistBusyIndicator.SetStatus("", false, "");
            //////////}
            //////////else
            //////////{
            //////////    if (this.ChannelSelected != null && this.ChannelSelected.PlaylistSelected != null)
            //////////    {
            //////////        this.PlaylistBusyIndicator.SetStatus("NOT_FOUND_EVENTS", true, string.Format("Ngày {0} chưa có sự kiện nào trong lịch phát sóng!!!", this.ChannelSelected.PlaylistSelected.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)));
            //////////    }
            //////////    else
            //////////    {
            //////////        this.PlaylistBusyIndicator.SetStatus("NOT_FOUND_EVENTS", true, "Chưa có sự kiện nào trong lịch phát sóng!!!");
            //////////    }
            //////////}
        }

        private void ExecuteOpenSpecialSettingEventCommand(object obj)
        {
            EventModel eventModel = obj as EventModel;
            if (eventModel != null)
            {
                if (eventModel.IsComingUpEvent)
                {
                    string comingUpFilePath = "";
                    if (File.Exists(eventModel.CTodayFilePath))
                    {
                        comingUpFilePath = eventModel.CTodayFilePath;
                    }
                    else if (File.Exists(eventModel.CTomorrowFilePath))
                    {
                        comingUpFilePath = eventModel.CTomorrowFilePath;
                    }
                    if (_wndComingUpViewer == null || PresentationSource.FromVisual(_wndComingUpViewer) == null)
                    {
                        _wndComingUpViewer = new ComingUpViewerWindow();
                        _wndComingUpViewer.Show();
                    }
                    _wndComingUpViewer.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    ComingUpViewerWindowViewModel vmComingUpViewer = _wndComingUpViewer.DataContext as ComingUpViewerWindowViewModel;
                    if (vmComingUpViewer != null && File.Exists(comingUpFilePath))
                    {
                        vmComingUpViewer.LoadDataFromFile(new FileInfo(comingUpFilePath));
                    }
                    _wndComingUpViewer.Title = eventModel.ProgramCode;
                    _wndComingUpViewer.Activate();
                }
            }
        }

        private void ExecutePressKeyOnCellCommand(DataGrid obj)
        {
            if (obj != null)
            {
                DataGridRow selectedRow = obj.GetSelectedRow();
                if (selectedRow != null)
                {
                    DataGridCell columnCellSC = obj.GetCell(selectedRow, obj.CurrentColumn.DisplayIndex);
                    if (columnCellSC != null && columnCellSC.IsReadOnly == false && columnCellSC.IsSelected)
                    {
                        Control control = null;
                        if (columnCellSC.Content is ContentPresenter)
                        {
                            ContentPresenter cp = columnCellSC.Content as ContentPresenter;
                            if (cp != null)
                            {
                                control = FindFirstChild<Control>(cp as FrameworkElement);
                            }
                        }

                        if (control != null && control.IsEnabled)
                        {
                            //control.Focus();
                            if (control is TextBox)
                            {
                                ((TextBox)control).Text = " ";
                            }

                            if (control is CheckBox)
                            {
                                ((CheckBox)control).IsChecked = !((CheckBox)control).IsChecked;
                            }
                        }
                    }
                }
            }
        }

        private T FindFirstChild<T>(FrameworkElement element) where T : FrameworkElement
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            var children = new FrameworkElement[childrenCount];

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                children[i] = child;
                if (child is T)
                    return (T)child;
            }

            for (int i = 0; i < childrenCount; i++)
                if (children[i] != null)
                {
                    var subChild = FindFirstChild<T>(children[i]);
                    if (subChild != null)
                        return subChild;
                }

            return null;
        }

        private void ExecuteCellEditingLostFocusCommand(DataGrid obj)
        {
            try
            {
                if (obj != null)
                {
                    DataGridRow selectedRow = obj.GetSelectedRow();
                    EventModel eventModel = null;
                    if (selectedRow != null)
                    {
                        eventModel = (EventModel)selectedRow.Item;
                        EventModel tagEventModel = (EventModel)obj.Tag;

                        eventModel.Status = "0";
                        if (tagEventModel.TenMu != eventModel.TenMu || tagEventModel.TenCt != eventModel.TenCt)
                        {
                            eventModel.Modify = "1";
                        }
                    }
                    obj.CommitEdit();
                    ApplyContentIfInTheSameGroup(eventModel, this.ChannelSelected.PlaylistSelected.Events);
                }

                //////////this.ChannelSelected.EdittingEvent.IsLocking = false;
                //////////if (AppSetting.Default.IsMasterMode)
                //////////{
                //////////    this.tcpServerNetwork.Broadcast(new TcpResponseObject("OK", JsonConvert.SerializeObject(this.ChannelSelectedItem.EdittingEvent), new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "GET", "EDITTING_EVENT", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem.Date)))), null);
                //////////}
                //////////else
                //////////{
                //////////    AppSetting.Default.TcpClientNetwork.Send(new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "SET", "EDITTING_EVENT", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem.Date) + "|" + JsonConvert.SerializeObject(this.ChannelSelectedItem.EdittingEvent))));
                //////////}

                this.ChannelSelected.EdittingEvent = null;
                this.ChannelSelected.PlaylistSelected.UpdateValidEvents();
            }
            catch (Exception) { }
            Debug.WriteLine("OnCellEditingLostFocus");
        }

        private void ApplyContentIfInTheSameGroup(EventModel currentEvent, ObservableCollection<EventModel> eventModels)
        {
            foreach (var eventModel in eventModels)
            {
                if (currentEvent.EventId.Equals(eventModel.EventId, StringComparison.OrdinalIgnoreCase) == false && (eventModel.IsPrimaryEvent || eventModel.IsUserPrimaryEvent) && currentEvent.GroupName.Equals(eventModel.GroupName, StringComparison.OrdinalIgnoreCase))
                {
                    eventModel.TenMu = currentEvent.TenMu;
                    eventModel.TenCt = currentEvent.TenCt;
                    eventModel.Modify = currentEvent.Modify;
                }
            }
        }

        private void ExecuteBeginningEditCommand(DataGrid obj)
        {
            if (obj != null)
            {
                if (this.ChannelSelected != null)
                {
                    DataGridRow selectedRow = obj.GetSelectedRow();
                    if (selectedRow != null)
                    {
                        EventModel eventModel = (EventModel)selectedRow.Item;
                        obj.Tag = new EventModel(this.ChannelSelected) { TenMu = eventModel.TenMu, TenCt = eventModel.TenCt };
                        this.ChannelSelected.EdittingEvent = eventModel;

                        //////////this.ChannelSelected.EdittingEvent.IsLocking = true;
                        //////////this.ChannelSelected.EdittingEvent.LockedBy = AppSetting.Default.CurrentUser;
                        //////////this.ChannelSelected.EdittingEvent.LockedAt = DateTime.Now;
                        //////////if (AppSetting.Default.IsMasterMode)
                        //////////{
                        //////////    this.tcpServerNetwork.Broadcast(new TcpResponseObject("OK", JsonConvert.SerializeObject(this.ChannelSelectedItem.EdittingEvent), new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "GET", "EDITTING_EVENT", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem.Date)))), null);
                        //////////}
                        //////////else
                        //////////{
                        //////////    AppSetting.Default.TcpClientNetwork.Send(new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "SET", "EDITTING_EVENT", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem.Date) + "|" + JsonConvert.SerializeObject(this.ChannelSelectedItem.EdittingEvent))));
                        //////////}
                        //////////this.ChannelSelected.EdittingEvent.IsLocking = false;
                    }
                }
            }
            Debug.WriteLine("OnBeginningEdit");
        }

        private void ExecuteCheckBoxCellCheckedChangedCommand(object obj)
        {
            ////////////if (obj != null)
            ////////////{
            ////////////    this.ChannelSelected.EdittingEvent = (EventModel)obj;
            ////////////    this.ChannelSelected.EdittingEvent.IsLocking = true;
            ////////////    this.ChannelSelected.EdittingEvent.LockedBy = AppSetting.Default.CurrentUser;
            ////////////    this.ChannelSelected.EdittingEvent.LockedAt = DateTime.Now;
            ////////////    if (AppSetting.Default.IsMasterMode)
            ////////////    {
            ////////////        this.tcpServerNetwork.Broadcast(new TcpResponseObject("OK", JsonConvert.SerializeObject(this.ChannelSelectedItem.EdittingEvent), new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "GET", "EDITTING_EVENT", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem.Date)))), null);
            ////////////    }
            ////////////    else
            ////////////    {
            ////////////        AppSetting.Default.TcpClientNetwork.Send(new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "SET", "EDITTING_EVENT", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem.Date) + "|" + JsonConvert.SerializeObject(this.ChannelSelectedItem.EdittingEvent))));
            ////////////    }
            ////////////    this.ChannelSelectedItem.EdittingEvent.IsLocking = false;
            ////////////    Thread.Sleep(250);
            ////////////    if (AppSetting.Default.IsMasterMode)
            ////////////    {
            ////////////        this.tcpServerNetwork.Broadcast(new TcpResponseObject("OK", JsonConvert.SerializeObject(this.ChannelSelectedItem.EdittingEvent), new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "GET", "EDITTING_EVENT", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem.Date)))), null);
            ////////////    }
            ////////////    else
            ////////////    {
            ////////////        AppSetting.Default.TcpClientNetwork.Send(new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "SET", "EDITTING_EVENT", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem.Date) + "|" + JsonConvert.SerializeObject(this.ChannelSelectedItem.EdittingEvent))));
            ////////////    }
            ////////////    this.ChannelSelected.EdittingEvent = null;
            ////////////}
        }

        private void ExecuteCheckBoxColumnCheckedChangedCommand()
        {
            ////////////if (this.ChannelSelected != null && this.ChannelSelected.PlaylistSelected != null)
            ////////////{
            ////////////    //foreach (var eventModel in this.ChannelSelectedItem.PlaylistSelectedItem.Events)
            ////////////    //{
            ////////////    //    eventModel.IsLocking = true;
            ////////////    //    eventModel.LockedBy = AppSetting.Default.CurrentUser;
            ////////////    //    eventModel.LockedAt = DateTime.Now;
            ////////////    //}
            ////////////    if (AppSetting.Default.IsMasterMode)
            ////////////    {
            ////////////        this.tcpServerNetwork.Broadcast(new TcpResponseObject("OK", "", new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "GET", "EDITTING_PLAYLIST", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem)))), null);
            ////////////    }
            ////////////    else
            ////////////    {
            ////////////        AppSetting.Default.TcpClientNetwork.Send(new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "SET", "EDITTING_PLAYLIST", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem))));
            ////////////    }
            ////////////    //foreach (var eventModel in this.ChannelSelectedItem.PlaylistSelectedItem.Events)
            ////////////    //{
            ////////////    //    eventModel.IsLocking = false;
            ////////////    //}
            ////////////    Thread.Sleep(250);
            ////////////    if (AppSetting.Default.IsMasterMode)
            ////////////    {
            ////////////        this.tcpServerNetwork.Broadcast(new TcpResponseObject("OK", "", new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "GET", "EDITTING_PLAYLIST", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem)))), null);
            ////////////    }
            ////////////    else
            ////////////    {
            ////////////        AppSetting.Default.TcpClientNetwork.Send(new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(this.ChannelSelectedItem.Name, "SET", "EDITTING_PLAYLIST", JsonConvert.SerializeObject(this.ChannelSelectedItem.PlaylistSelectedItem))));
            ////////////    }

            ////////////}
        }

        private void ExecuteCellEditEndingCommand(DataGrid obj)
        {
            Debug.WriteLine("OnCellEditEnding");
        }

        private void ExecuteSetSelectedEventToUserEventCommand(object obj)
        {
            if (this.EventSelected != null)
            {
                bool isUserPrimaryEvent = Convert.ToBoolean(obj);
                this.EventSelected.IsUserPrimaryEvent = isUserPrimaryEvent;
                if (isUserPrimaryEvent == false)
                {
                    this.EventSelected.CanShowCG = false;
                    this.EventSelected.CanShowNowProgram = false;
                    this.EventSelected.CanShowNextProgram = false;
                    this.EventSelected.CanShowCountDown = false;
                    this.EventSelected.CanShowSpecial = false;
                }
                this.ChannelSelected.PlaylistSelected.UpdateValidEvents();
            }
        }

        private void ExecuteSetSelectedEventToDefaultCommand()
        {
            if (this.EventSelected != null)
            {
                ExecuteSetSelectedEventToUserEventCommand(true);
                ExecuteSyncDataFromTrafficToSelectedEventCommand();
                ApplyContentIfInTheSameGroup(this.EventSelected, this.ChannelSelected.PlaylistSelected.Events);
                this.EventSelected.CanShowCG = false;
                this.ChannelSelected.PlaylistSelected.UpdateValidEvents();
            }
        }

        private void ExecuteSyncDataFromTrafficToSelectedPlaylistCommand()
        {
            if (this.ChannelSelected != null && this.ChannelSelected.PlaylistSelected != null)
            {
                this.ChannelSelected.UpdateTrafficData();
                this.ChannelSelected.SyncDataFromTrafficToPlaylist(this.ChannelSelected.PlaylistSelected.Events, this.ChannelSelected.EdittingEvent);
            }
        }

        private void ExecuteSyncDataFromTrafficToSelectedEventCommand()
        {
            if (this.EventSelected != null)
            {
                int syncCount = this.ChannelSelected.SyncDataFromTrafficToEvent(this.EventSelected);
                if (syncCount == 0)
                {
                    this.EventSelected.GetProgramTitleFromDescription(this.EventSelected.Description, ':');
                }
                this.EventSelected.Modify = "0";
                ApplyContentIfInTheSameGroup(this.EventSelected, this.ChannelSelected.PlaylistSelected.Events);
            }
        }

        private void ExecuteProcessSelectedPlaylistCommand()
        {
            if (this.ChannelSelected != null && this.ChannelSelected.PlaylistSelected != null)
            {
                OnProcessPlaylist(this.ChannelSelected, this.ChannelSelected.PlaylistSelected);
            }
        }

        private void OnProcessPlaylist(ChannelModel channelModel, PlaylistModel playlistModel)
        {
            if (channelModel != null && playlistModel != null &&
                playlistModel.Events != null &&
                playlistModel.Events.Count > 0)
            {
                if (this.ChannelSelected == channelModel && this.ChannelSelected.PlaylistSelected == playlistModel)
                {
                    this.IsPlaylistDialogOpenning = true;
                    this.PlaylistDialogContent = string.Format("Đang xử lý lịch phát sóng của kênh {0} ngày {1} ...", channelModel.Name, playlistModel.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                }

                playlistModel.IsProcessingPlaylist = true;
                Debug.WriteLine("playlist is processing...");
                object args = new object[2] { channelModel, playlistModel };
                Thread processSelectedPlaylistThread = new Thread(OnProcessPlaylistThread);
                processSelectedPlaylistThread.IsBackground = true;
                processSelectedPlaylistThread.Start(args);
            }
        }

        private void OnProcessPlaylistThread(object obj)
        {
            Array args = obj as Array;
            ChannelModel channelModel = args.GetValue(0) as ChannelModel;
            PlaylistModel playlistModel = args.GetValue(1) as PlaylistModel;

            Thread.Sleep(500);
            for (int i = 0; i < playlistModel.Events.Count; i++)
            {
                EventModel eventModel = playlistModel.Events[i];
                eventModel.Status = "0";
            }

            //if (AppSetting.Default.IsMasterMode)
            //{
            //Lưu lại vào file older
            string olderFileName = channelModel.Name + "_" + playlistModel.Date.ToString(channelModel.Setting.DateFormatString, CultureInfo.InvariantCulture) + ".old";
            string olderFilePath = Path.Combine(channelModel.Setting.OlderPlaylistFolderPath, olderFileName);
            playlistModel.SavePlayListToFile(olderFilePath);
            //}
            //else
            //{
            //    playlistModel.UpdateValidEvents();
            //    int index = channelModel.Playlists.IndexOf(playlistModel);
            //    AppSetting.Default.TcpClientNetwork.Send(new TcpRequestObject("VTV_PLAYLIST_EDITOR", Guid.NewGuid(), new TcpCommandObject(channelModel.Name, "PROCESS", "PLAYLIST", index.ToString())));
            //}

            Thread.Sleep(500);
            if (this.ChannelSelected == channelModel && this.ChannelSelected.PlaylistSelected == playlistModel)
            {
                this.IsPlaylistDialogOpenning = true;
                this.PlaylistDialogContent = "Đã xử lý xong";
                Thread.Sleep(500);
                this.IsPlaylistDialogOpenning = false;

                //////////if (this.PlaylistBusyIndicator.State == "PROCESSING")
                //////////{
                //////////    this.PlaylistBusyIndicator.SetStatus("PROCESSED", true, "Đã xử lý xong");
                //////////    Thread.Sleep(500);
                //////////}
                //////////if (this.PlaylistBusyIndicator.State == "PROCESSED")
                //////////{
                //////////    this.PlaylistBusyIndicator.SetStatus("", false, "");
                //////////}
            }
            playlistModel.IsProcessingPlaylist = false;
            Debug.WriteLine("playlist is processed");
        }

        private async void ExecuteExitApplicationCommand()
        {
            this.IsRootDialogOpenning = false;
            var view = new ConfirmDialogView
            {
                DataContext = new DialogViewModel
                {
                    Message = "Bạn có chắc muốn thoát khỏi chương trình này không?"
                }
            };
            var result = await DialogHost.Show(view, "RootDialog");
            if (result != null && result is bool && (bool)result == true)
            {
                _isDataProcessorThreadRunning = false;
                Application.Current.Shutdown();
            }
        }

        private void ExecuteOpenAboutWindowCommand()
        {
            this.IsRootDialogOpenning = false;
            var view = new AboutDialogView
            {
                DataContext = new AboutDialogViewModel
                {
                    ApplicationName = AppData.APPLICATION_FRIENDLYNAME,
                    ApplicationVersion = "Phiên bản: " + AppData.APPLICATION_VERSION,
                    ApplicationLogoImageSource = Application.Current.MainWindow.Icon
                }
            };
            var result = DialogHost.Show(view, "RootDialog");
        }

        private void ExecuteOpenSettingWindowCommand()
        {
            this.IsRootDialogOpenning = false;

        }

        private void SearchTrafficEvent(string keyWord)
        {
            if (this.ChannelSelected != null)
            {
                keyWord = keyWord.Trim();
                ICollectionView view = CollectionViewSource.GetDefaultView(this.ChannelSelected.TrafficEvents);
                if (view != null)
                {
                    if (string.IsNullOrEmpty(keyWord))
                    {
                        view.Filter = null;
                    }
                    else
                    {
                        view.Filter = new Predicate<object>(x => ((TrafficEventModel)x).ProgramCode.ToUpper().Contains(keyWord.ToUpper()));
                    }
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
                    view.SortDescriptions.Add(new SortDescription("ProgramCode", ListSortDirection.Ascending));
                }
                this.LastSearchTrafficEventKeyWord = keyWord;
            }
        }

        private void ExecuteSearchTrafficEventCommand()
        {
            this.SearchTrafficEventKeyWord = this.SearchTrafficEventKeyWord.Trim();
            SearchTrafficEvent(this.SearchTrafficEventKeyWord);
            //if (this.ChannelSelected != null)
            //{
            //    this.SearchTrafficEventKeyWord = this.SearchTrafficEventKeyWord.Trim();
            //    ICollectionView view = CollectionViewSource.GetDefaultView(this.ChannelSelected.TrafficEvents);
            //    if (view != null)
            //    {
            //        if (string.IsNullOrEmpty(this.SearchTrafficEventKeyWord))
            //        {
            //            view.Filter = null;
            //        }
            //        else
            //        {
            //            view.Filter = new Predicate<object>(x => ((TrafficEventModel)x).ProgramCode.ToUpper().Contains(this.SearchTrafficEventKeyWord.ToUpper()));
            //        }
            //        view.SortDescriptions.Clear();
            //        view.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
            //        view.SortDescriptions.Add(new SortDescription("ProgramCode", ListSortDirection.Ascending));
            //    }
            //    this.LastSearchTrafficEventKeyWord = this.SearchTrafficEventKeyWord;
            //}
        }

        private void ExecuteClearSearchTrafficEventCommand()
        {
            this.SearchTrafficEventKeyWord = "";
            //ExecuteSearchTrafficEventCommand();
            SearchTrafficEvent(this.SearchTrafficEventKeyWord);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            Thread.Sleep(100);
            Application.Current.MainWindow.Hide();
        }

        private DateTime GetDateFromComingUpFile(FileInfo fi, string filterFileName)
        {
            DateTime dt = DateTime.MinValue;
            try
            {
                string dateString = fi.Name.Substring(filterFileName.Length + 1, 8);
                int year = Convert.ToInt32(dateString.Substring(0, 4));
                int month = Convert.ToInt32(dateString.Substring(4, 2));
                int day = Convert.ToInt32(dateString.Substring(6, 2));
                dt = new DateTime(year, month, day);
            }
            catch (Exception) { }
            return dt;
        }

        private DateTime GetDateFromXmlPlaylistFile(FileInfo fi)
        {
            DateTime dt = DateTime.MinValue;
            try
            {
                XmlDocument xmlDocument = FileManager.OpenXML(fi.FullName, 5);
                if (xmlDocument != null)
                {
                    XmlElement nodeDate = (XmlElement)xmlDocument.SelectSingleNode("/playlist/event/date");
                    if (nodeDate != null)
                    {
                        dt = Common.Utility.GetDateFromString(nodeDate.InnerText.Trim(), "/").Date;
                    }
                }
                else
                {
                    fi.Delete();
                }
            }
            catch (Exception)
            {
            }
            return dt;
        }

        private void ExecuteDataProcessorThread()
        {
            CleanUpData();
            while (_isDataProcessorThreadRunning)
            {
                UpdateData();

                if (DateTime.Now - _lastCommunicationUpdateInterval >= TimeSpan.FromSeconds(10))
                {
                    _lastCommunicationUpdateInterval = DateTime.Now;
                }

                Thread.Sleep(250);
            }
        }

        private void CleanUpData()
        {
            for (int i = 0; AppData.Default.Channels != null && i < AppData.Default.Channels.Count && _isDataProcessorThreadRunning; i++)
            {
                ChannelModel channelModel = AppData.Default.Channels[i];

                //Clean playlist
                if (Directory.Exists(channelModel.Setting.PlaylistFolderPath))
                {
                    string[] filePaths = Directory.GetFiles(channelModel.Setting.PlaylistFolderPath, "*.xml");
                    for (int j = 0; filePaths != null && j < filePaths.Length && _isDataProcessorThreadRunning; j++)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(filePaths[j]);
                            DateTime dt = GetDateFromXmlPlaylistFile(fi);
                            if (dt != DateTime.MinValue && dt.Date < DateTime.Now.Date.AddDays(0 - Math.Abs(channelModel.Setting.CountOfPreviousDays)))
                            {
                                string archiveFolderPath = Path.Combine(channelModel.Setting.PlaylistFolderPath, Path.Combine("Archive", dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
                                if (Directory.Exists(archiveFolderPath) == false)
                                {
                                    Directory.CreateDirectory(archiveFolderPath);
                                }
                                FileManager.RenameFile(fi.FullName, Path.Combine(archiveFolderPath, fi.Name));
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                ///

                //Clean ComingUp
                if (Directory.Exists(channelModel.Setting.OlderPlaylistFolderPath))
                {
                    string[] filePaths = Directory.GetFiles(channelModel.Setting.OlderPlaylistFolderPath, "*.txt");
                    for (int j = 0; filePaths != null && j < filePaths.Length && _isDataProcessorThreadRunning; j++)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(filePaths[j]);
                            string filterFileName = "";
                            if (fi.Name.StartsWith("CToday"))
                            {
                                filterFileName = "CToday_" + channelModel.Name;
                            }
                            else if (fi.Name.StartsWith("CTomorrow"))
                            {
                                filterFileName = "CTomorrow_" + channelModel.Name;
                            }
                            DateTime dt = GetDateFromComingUpFile(fi, filterFileName);
                            if (dt != DateTime.MinValue && dt.Date < DateTime.Now.Date.AddDays(0 - Math.Abs(channelModel.Setting.CountOfPreviousDays)))
                            {
                                string archiveFolderPath = Path.Combine(channelModel.Setting.OlderPlaylistFolderPath, Path.Combine("Archive", dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
                                if (Directory.Exists(archiveFolderPath) == false)
                                {
                                    Directory.CreateDirectory(archiveFolderPath);
                                }
                                FileManager.RenameFile(fi.FullName, Path.Combine(archiveFolderPath, fi.Name));
                            }
                        }
                        catch (Exception) { }
                    }
                }
                ///
            }
        }

        public void UpdateData()
        {
            OnPropertyChanged(() => this.HasEventInSelectedPlaylist);

            //Tự động quét dọn tại thời điểm đặt trước nếu được cho phép
            if (AppData.Default.EnableCleaner && DateTime.Now.TimeOfDay >= AppData.Default.AutoCleanAtTime && DateTime.Now.TimeOfDay <= AppData.Default.AutoCleanAtTime.Add(TimeSpan.FromMinutes(1)))
            {
                CleanUpData();
            }
            ///

            if (_isDataProcessorThreadRunning == false)
                return;

            if (AppData.Default.Channels != null && AppData.Default.Channels.Count > 0 && this.ChannelSelected == null)
            {
                this.ChannelSelected = AppData.Default.Channels[0];
            }

            if (this.ChannelSelected == null || this.ChannelSelected.PlaylistSelected == null)
            {
                this.IsPlaylistDialogOpenning = true;
                this.PlaylistDialogContent = null;
            }
            else
            {
                if (this.ChannelSelected.PlaylistSelected.IsProcessingPlaylist == false)
                {
                    if (this.HasEventInSelectedPlaylist)
                    {
                        this.IsPlaylistDialogOpenning = false;
                    }
                    else
                    {
                        this.IsPlaylistDialogOpenning = true;
                        this.PlaylistDialogContent = string.Format("Ngày {0} chưa có sự kiện nào trong lịch phát sóng!!!", this.ChannelSelected.PlaylistSelected.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    }
                }
            }

            var connectionStringEntity = DataService.Service.Default.GetCurrentConnectionString();
            this.CurrentConnectionString = AppData.Mapper.Map<DataObjects.ConnectionString, ConnectionStringModel>(connectionStringEntity);
        }
    }
}
