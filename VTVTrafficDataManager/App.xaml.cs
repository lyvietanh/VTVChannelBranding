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
using VTVTrafficDataManager.Models;

namespace VTVTrafficDataManager
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
                //DataService.Service.Default.ConnectionStrings.Add("Data Source=LYVIETANH-PC;Initial Catalog=db_VTVChannelBranding;User Id=sa;Password=123456;Connection Timeout=3;");
                //if (DataService.Service.Default.IsServerConnected)
                //{
                var application = new App();
                application.InitializeComponent();
                application.Run();

                //Allow single instance code to perform cleanup operations
                SingleInstanceApplication<App>.Cleanup();
                //}
                //else
                //{
                //    MessageBox.Show("Không thể kết nối tới cơ sở dữ liệu!\nChương trình không thể chạy được..");
                //}
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
            AppData.Default.AppSetting.Load(AppData.APPSETTING_FILEPATH);

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();

            //Khởi chạy các tác vụ xử lý của từng kênh
            foreach (ChannelModel channelModel in AppData.Default.Channels)
            {
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

            AppData.Default.AppSetting.Save(AppData.APPSETTING_FILEPATH);

            foreach (var channel in AppData.Default.Channels)
            {
                channel.StopTrafficProcessor();
            }
        }
    }
}
