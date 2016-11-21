using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoMapper;
using Prism.Commands;
using Prism.Mvvm;
using VTVTrafficDataManager.Models;

namespace VTVTrafficDataManager.ViewModels
{
    public class SettingWindowViewModel : BindableBase
    {
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand TestConnectionCommand { get; private set; }
        public ICommand AddNewChannelSettingCommand { get; private set; }
        public ICommand RemoveChannelSettingCommand { get; private set; }
        public ICommand BrowseFolderPathCommand { get; private set; }

        private AppSettingModel _model = new AppSettingModel();
        private ChannelModel _channelSelected = null;
        private System.Windows.Forms.FolderBrowserDialog dlgFolderBrowser = null;
        private int unnamedCount = 0;

        public AppSettingModel Model
        {
            get
            {
                return _model;
            }

            set
            {
                this._model = value;
                OnPropertyChanged(() => this.Model);
            }
        }

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
            }
        }

        public SettingWindowViewModel()
        {
            this.SaveCommand = new DelegateCommand<object>(ExecuteSaveCommand, CanExecuteSaveCommand);
            this.CancelCommand = new DelegateCommand<object>(ExecuteCancelCommand);
            this.TestConnectionCommand = new DelegateCommand(ExecuteTestConnectionCommand);
            this.AddNewChannelSettingCommand = new DelegateCommand(ExecuteAddNewChannelSettingCommand);
            this.RemoveChannelSettingCommand = new DelegateCommand(ExecuteRemoveChannelSettingCommand);
            this.BrowseFolderPathCommand = new DelegateCommand<object>(ExecuteBrowseFolderPathCommand);

            if (this.Model.Channels != null && this.Model.Channels.Count > 0)
            {
                this.ChannelSelected = this.Model.Channels[0];
            }
        }

        private void ExecuteBrowseFolderPathCommand(object obj)
        {
            TextBox txt = obj as TextBox;
            if (txt != null)
            {
                txt.Text = Common.Utility.TrimText(txt.Text);
                if (dlgFolderBrowser == null)
                {
                    this.dlgFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                    this.dlgFolderBrowser.Description = "Chọn thư mục chứa dữ liệu Traffic trích xuất từ Automation";
                }
                if (Directory.Exists(txt.Text))
                {
                    this.dlgFolderBrowser.SelectedPath = txt.Text;
                }
                if (this.dlgFolderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txt.Text = this.dlgFolderBrowser.SelectedPath;
                }
            }
        }

        private void ExecuteRemoveChannelSettingCommand()
        {
            if (this.ChannelSelected != null)
            {
                this.Model.Channels.Remove(this.ChannelSelected);
                if (this.Model.Channels != null && this.Model.Channels.Count > 0)
                {
                    this.ChannelSelected = this.Model.Channels[0];
                }
                else
                {
                    unnamedCount = 0;
                }
            }
        }

        private void ExecuteAddNewChannelSettingCommand()
        {
            ChannelModel channel = new ChannelModel()
            {
                Name = "UNNAMED_" + (++unnamedCount),
                TrafficFolderPath = "",
                TrafficFileFilter = ""
            };
            this.Model.Channels.Add(channel);
            this.ChannelSelected = channel;
        }

        private void ExecuteTestConnectionCommand()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            string serverName = "?";
            string databaseName = "?";
            if (string.IsNullOrEmpty(this.Model.ConnectionString.ServerName) == false)
            {
                serverName = this.Model.ConnectionString.ServerName;
            }
            if (string.IsNullOrEmpty(this.Model.ConnectionString.DatabaseName) == false)
            {
                databaseName = this.Model.ConnectionString.DatabaseName;
            }
            ConnectionStringModel connectionStringModel = new ConnectionStringModel
            {
                ServerName = serverName,
                DatabaseName =databaseName,
                UserName = this.Model.ConnectionString.UserName,
                Password = this.Model.ConnectionString.Password,
                ConnectionTimeOut = this.Model.ConnectionString.ConnectionTimeOut
            };

            Task<bool> outerTask = Task.Run(() => DataService.Service.Default.CheckConnectionAsync(AppData.Mapper.Map<ConnectionStringModel, DataObjects.ConnectionString>(connectionStringModel)));
            if (outerTask.Result == true)
            {
                MessageBox.Show("Kết nối thành công :)");
            }
            else
            {
                MessageBox.Show("Kết nối thất bại. Vui lòng kiểm tra lại thông tin kết nối!!!");
            }
            //if (DataService.Service.Default.CheckConnection(connectionString))
            //{
            //    MessageBox.Show("Kết nối thành công :)");
            //}
            //else
            //{
            //    MessageBox.Show("Kết nối thất bại. Vui lòng kiểm tra lại thông tin kết nối!!!");
            //}
            Mouse.OverrideCursor = null;
        }

        public bool CanExecuteSaveCommand(object obj)
        {
            bool hasErrors = false;
            this.Model.ConnectionString.Validate();
            hasErrors |= this.Model.ConnectionString.HasErrors;
            foreach (var channel in this.Model.Channels)
            {
                channel.Validate();
                hasErrors |= channel.HasErrors;
                if (hasErrors)
                    break;
            }
            if (hasErrors)
            {
                MessageBox.Show(" Yêu cầu điền đầy đủ thông tin thiết lập.");
            }
            return hasErrors == false;
        }
        private void ExecuteSaveCommand(object obj)
        {
            if (obj != null)
            {
                Window wnd = (Window)obj;
                wnd.DialogResult = true;
            }
        }

        private void ExecuteCancelCommand(object obj)
        {
            if (obj != null)
            {
                Window wnd = (Window)obj;
                wnd.DialogResult = false;
            }
        }
    }
}
