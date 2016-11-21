using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;

namespace Common.ViewModels
{
    public class AboutDialogViewModel : BindableBase
    {
        public ICommand HyperlinkRequestNavigateCommand { get; private set; }

        private ImageSource _applicationLogoImageSource = null;
        private string _applicationName = "";
        private string _applicationVersion = "";
        private string _companyName = "";
        private string _companyAddress = "";
        private string _companyTelephoneNumber = "";
        private string _companyWebsiteUrl = "";

        public ImageSource ApplicationLogoImageSource
        {
            get
            {
                return _applicationLogoImageSource;
            }

            set
            {
                this._applicationLogoImageSource = value;
                OnPropertyChanged(() => this.ApplicationLogoImageSource);
            }
        }

        public string ApplicationName
        {
            get
            {
                return _applicationName;
            }

            set
            {
                this._applicationName = value;
                OnPropertyChanged(() => this.ApplicationName);
                OnPropertyChanged(() => this.ApplicationFullName);
            }
        }

        public string ApplicationVersion
        {
            get
            {
                return _applicationVersion;
            }

            set
            {
                this._applicationVersion = value;
                OnPropertyChanged(() => this.ApplicationVersion);
                OnPropertyChanged(() => this.ApplicationFullName);
            }
        }

        public string ApplicationFullName
        {
            get
            {
                return this.ApplicationName + " - " + this.ApplicationVersion;
            }
        }

        public string CompanyName
        {
            get
            {
                return _companyName;
            }

            set
            {
                this._companyName = value;
                OnPropertyChanged(() => this.CompanyName);
            }
        }

        public string CompanyAddress
        {
            get
            {
                return _companyAddress;
            }

            set
            {
                this._companyAddress = value;
                OnPropertyChanged(() => this.CompanyAddress);
            }
        }

        public string CompanyTelephoneNumber
        {
            get
            {
                return _companyTelephoneNumber;
            }

            set
            {
                this._companyTelephoneNumber = value;
                OnPropertyChanged(() => this.CompanyTelephoneNumber);
            }
        }

        public string CompanyWebsiteUrl
        {
            get
            {
                return _companyWebsiteUrl;
            }

            set
            {
                this._companyWebsiteUrl = value;
                OnPropertyChanged(() => this.CompanyWebsiteUrl);
            }
        }

        public AboutDialogViewModel()
        {
            this.HyperlinkRequestNavigateCommand = new DelegateCommand(ExecuteHyperlinkRequestNavigateCommand);

            Version v = Assembly.GetEntryAssembly().GetName().Version;
            this.ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
            this.ApplicationVersion = string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);

            this.CompanyName = "Chi Duc Co.,Ltd";
            this.CompanyAddress = "88 Pham Huy Thong str., Ba Dinh dist., Hanoi, Vietnam.";
            this.CompanyTelephoneNumber = "(84 - 4) 3771 6010";
            this.CompanyWebsiteUrl = "http://cdc.com.vn";
        }

        private void ExecuteHyperlinkRequestNavigateCommand()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
            Process.Start(new ProcessStartInfo(this.CompanyWebsiteUrl));
        }
    }
}
