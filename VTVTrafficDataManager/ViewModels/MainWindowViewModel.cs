using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AutoMapper;
using Common.ViewModels;
using Common.Views;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using VTVTrafficDataManager.Models;
using VTVTrafficDataManager.Views;

namespace VTVTrafficDataManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public ICommand ExitApplicationCommand { get; private set; }
        public ICommand OpenSettingWindowCommand { get; private set; }
        public ICommand OpenAboutWindowCommand { get; private set; }
        public ICommand SearchTrafficEventCommand { get; private set; }
        public ICommand ClearSearchTrafficEventCommand { get; private set; }
        public ICommand GoToFirstPageCommand { get; private set; }
        public ICommand GoToPreviousPageCommand { get; private set; }
        public ICommand GoToNextPageCommand { get; private set; }
        public ICommand GoToLastPageCommand { get; private set; }

        private ChannelModel _channelSelected = null;
        private string _searchTrafficEventKeyWord = "";
        private string _lastSearchTrafficEventKeyWord = "";
        private bool _isRootDialogOpenning = false;

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

                ExecuteClearSearchTrafficEventCommand();
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

        private Thread _mainProcessorThread = null;
        private bool _isMainProcessorThreadRunning = false;
        private DateTime _lastCommunicationUpdateInterval = DateTime.MinValue;

        public MainWindowViewModel()
        {
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            this.ExitApplicationCommand = new DelegateCommand(ExecuteExitApplicationCommand);
            this.OpenSettingWindowCommand = new DelegateCommand(ExecuteOpenSettingWindowCommand);
            this.OpenAboutWindowCommand = new DelegateCommand(ExecuteOpenAboutWindowCommand);
            this.SearchTrafficEventCommand = new DelegateCommand(ExecuteSearchTrafficEventCommand);
            this.ClearSearchTrafficEventCommand = new DelegateCommand(ExecuteClearSearchTrafficEventCommand);
            this.GoToFirstPageCommand = new DelegateCommand(ExecuteGoToFirstPageCommand);
            this.GoToPreviousPageCommand = new DelegateCommand(ExecuteGoToPreviousPageCommand);
            this.GoToNextPageCommand = new DelegateCommand(ExecuteGoToNextPageCommand);
            this.GoToLastPageCommand = new DelegateCommand(ExecuteGoToLastPageCommand);

            _isMainProcessorThreadRunning = true;
            _mainProcessorThread = new Thread(new ThreadStart(ExecuteMainProcessorThread));
            _mainProcessorThread.IsBackground = true;
            _mainProcessorThread.Start();

            if (AppData.Default.Channels.Count > 0)
            {
                this.ChannelSelected = AppData.Default.Channels[0];
            }
        }

        private void ExecuteGoToLastPageCommand()
        {
            if (this.ChannelSelected != null) this.ChannelSelected.GoToLastPage();
        }

        private void ExecuteGoToNextPageCommand()
        {
            if (this.ChannelSelected != null) this.ChannelSelected.GoToNextPage();
        }

        private void ExecuteGoToPreviousPageCommand()
        {
            if (this.ChannelSelected != null) this.ChannelSelected.GoToPreviousPage();
        }

        private void ExecuteGoToFirstPageCommand()
        {
            if (this.ChannelSelected != null) this.ChannelSelected.GoToFirstPage();
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
                _isMainProcessorThreadRunning = false;
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
            this.IsRootDialogOpenning = true;
            SettingWindow wndSetting = new SettingWindow();
            SettingWindowViewModel vmSettingWindow = wndSetting.DataContext as SettingWindowViewModel;
            Mapper.Initialize(cfg =>
            {
                cfg.ShouldMapProperty = p => p.GetMethod.IsPublic;
                cfg.CreateMap<AppSettingModel, AppSettingModel>();
                cfg.CreateMap<ConnectionStringModel, ConnectionStringModel>();
                cfg.CreateMap<ChannelModel, ChannelModel>();
            });
            Mapper.Map(AppData.Default.AppSetting, vmSettingWindow.Model, typeof(AppSettingModel), typeof(AppSettingModel));
            if (vmSettingWindow.Model.Channels != null && vmSettingWindow.Model.Channels.Count > 0)
            {
                vmSettingWindow.ChannelSelected = vmSettingWindow.Model.Channels[0];
            }
            wndSetting.Owner = Application.Current.MainWindow;
            var result = wndSetting.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                vmSettingWindow.Model.Save(AppData.APPSETTING_FILEPATH);
                //AppData.Default.AppSetting.ConnectionString = Mapper.Map<ConnectionStringModel, ConnectionStringModel>(vmSettingWindow.Model.ConnectionString);
                //AppData.Default.AppSetting.Channels = Mapper.Map<ObservableCollection<ChannelModel>, ObservableCollection<ChannelModel>>(vmSettingWindow.Model.Channels);

                //DataService.Service.Default.ConnectionStrings.Clear();
                //DataService.Service.Default.ConnectionStrings.Add(string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};Connection Timeout={4};persist security info=True;MultipleActiveResultSets=True;",
                //    AppData.Default.AppSetting.DatabaseSetting.ServerName,
                //    AppData.Default.AppSetting.DatabaseSetting.DatabaseName,
                //    AppData.Default.AppSetting.DatabaseSetting.UserName,
                //    AppData.Default.AppSetting.DatabaseSetting.Password,
                //    (int)AppData.Default.AppSetting.DatabaseSetting.ConnectionTimeOut.TotalSeconds));

                //AppData.Default.AppSetting.Save(AppData.APPSETTING_FILEPATH);
                AppData.Default.AppSetting.Channels.Clear();
                if (AppData.Default.AppSetting.Load(AppData.APPSETTING_FILEPATH))
                {
                    //Khởi chạy các tác vụ xử lý của từng kênh
                    foreach (ChannelModel channelModel in AppData.Default.Channels)
                    {
                        channelModel.StartTrafficProcessor();
                    }

                    if (AppData.Default.Channels != null && AppData.Default.Channels.Count > 0)
                    {
                        this.ChannelSelected = AppData.Default.Channels[0];
                    }
                }
            }
            this.IsRootDialogOpenning = false;
        }

        private void ExecuteSearchTrafficEventCommand()
        {
            if (this.ChannelSelected != null)
            {
                this.SearchTrafficEventKeyWord = this.SearchTrafficEventKeyWord.Trim();
                ICollectionView view = CollectionViewSource.GetDefaultView(this.ChannelSelected.TrafficEvents);
                if (view != null)
                {
                    if (string.IsNullOrEmpty(this.SearchTrafficEventKeyWord))
                    {
                        view.Filter = null;
                    }
                    else
                    {
                        view.Filter = new Predicate<object>(x => ((TrafficEventModel)x).ProgramCode.ToUpper().Contains(this.SearchTrafficEventKeyWord.ToUpper()));
                    }
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
                    view.SortDescriptions.Add(new SortDescription("ProgramCode", ListSortDirection.Ascending));
                }
                this.LastSearchTrafficEventKeyWord = this.SearchTrafficEventKeyWord;
            }
        }

        private void ExecuteClearSearchTrafficEventCommand()
        {
            this.SearchTrafficEventKeyWord = "";
            ExecuteSearchTrafficEventCommand();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (DataService.Service.Default.IsServerConnected)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
                Thread.Sleep(100);
                Application.Current.MainWindow.Hide();
            }
            else
            {
                ExecuteExitApplicationCommand();
            }
        }

        private void ExecuteMainProcessorThread()
        {
            while (_isMainProcessorThreadRunning)
            {
                if (DataService.Service.Default.IsServerConnected == false)
                {
                    if (this.IsRootDialogOpenning == false)
                    {
                        _isMainProcessorThreadRunning = false;
                        Application.Current.Dispatcher.Invoke(async delegate
                        {
                            this.IsRootDialogOpenning = false;
                            var view = new DialogView
                            {
                                DataContext = new DialogViewModel
                                {
                                    Message = "Không thể kết nối tới cơ sở dữ liệu!\nVui lòng thoát khỏi chương trình và kiểm tra lại thiết lập..."
                                }
                            };
                            var result = await DialogHost.Show(view, "RootDialog");
                            if (result != null && result is bool && (bool)result == true)
                            {
                                Application.Current.Shutdown();
                            }
                        });
                    }
                }
                else
                {
                    if (DateTime.Now - _lastCommunicationUpdateInterval >= TimeSpan.FromSeconds(10))
                    {
                        _lastCommunicationUpdateInterval = DateTime.Now;
                    }
                }
                Thread.Sleep(250);
            }
        }

    }
}
