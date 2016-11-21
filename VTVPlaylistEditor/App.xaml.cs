using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ChiDuc.General;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Shell;

namespace VTVPlaylistEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApplication
    {
        [STAThread]
        public static void Main()
        {
            if (SingleInstanceApplication<App>.InitializeAsFirstInstance(AppData.APPLICATION_NAME))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();

                //Allow single instance code to perform cleanup operations
                SingleInstanceApplication<App>.Cleanup();
            }
            else
            {
                MessageBox.Show("Chương trình vẫn đang chạy trong hệ thống!");
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            return true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LogUtility.FolderMode = LogUtility.TreeFolderMode.BY_YEAR;
            AppData.Default.Load();

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();

            //Khởi chạy các tác vụ xử lý của từng kênh
            foreach (var channelModel in AppData.Default.Channels)
            {
                channelModel.StartDataProcessor();
                channelModel.StartTrafficProcessor();
            }

            AppData.Default.NotifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (AppData.Default.NotifyIcon != null)
            {
                AppData.Default.NotifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            }

            foreach (var channel in AppData.Default.Channels)
            {
                channel.StopTrafficProcessor();
                channel.StartDataProcessor();
            }
        }
    }
}
