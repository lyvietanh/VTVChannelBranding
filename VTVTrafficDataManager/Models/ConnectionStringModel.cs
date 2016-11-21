using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Models;
using Prism.Mvvm;

namespace VTVTrafficDataManager.Models
{
    [XmlType(TypeName = "ConnectionString")]
    public class ConnectionStringModel : ValidationModelBase
    {
        private string _serverName = "localhost";
        private string _databaseName = "db_VTVChannelBranding";
        private string _userName = "sa";
        private string _password = "123456";
        private TimeSpan _connectionTimeOut = TimeSpan.FromSeconds(3);

        [XmlAttribute]
        [Required(ErrorMessage = "Bắt buộc.")]
        public string ServerName
        {
            get
            {
                return _serverName;
            }

            set
            {
                this._serverName = Common.Utility.TrimText(value);
                OnPropertyChanged(() => this.ServerName);
                Validate();
            }
        }

        [XmlAttribute]
        [Required(ErrorMessage = "Bắt buộc.")]
        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }

            set
            {
                this._databaseName = Common.Utility.TrimText(value);
                OnPropertyChanged(() => this.DatabaseName);
                Validate();
            }
        }

        [XmlAttribute]
        [Required(ErrorMessage = "Bắt buộc.")]
        public string UserName
        {
            get
            {
                return _userName;
            }

            set
            {
                this._userName = Common.Utility.TrimText(value);
                OnPropertyChanged(() => this.UserName);
                Validate();
            }
        }

        [XmlAttribute]
        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                this._password = value;
                OnPropertyChanged(() => this.Password);
            }
        }

        [XmlIgnore]
        public TimeSpan ConnectionTimeOut
        {
            get
            {
                return _connectionTimeOut;
            }

            set
            {
                this._connectionTimeOut = value;
                OnPropertyChanged(() => this.ConnectionTimeOut);
            }
        }
        [Browsable(false)]
        [XmlAttribute("ConnectionTimeOut")]
        public string ConnectionTimeOutAsString
        {
            get
            {
                return Common.Utility.GetTimeStringFromTimeSpan(this.ConnectionTimeOut);
            }
            set
            {
                this.ConnectionTimeOut = Common.Utility.GetTimeSpanFromString(value);
                OnPropertyChanged(() => this.ConnectionTimeOutAsString);
                OnPropertyChanged(() => this.ConnectionTimeOut);
            }
        }

    }
}
