using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Common.Models
{
    public class ConnectionStringModel : BindableBase
    {
        private string _serverName = "";
        private string _databaseName = "";
        private string _userName = "";
        private string _password = "";
        private TimeSpan _connectionTimeOut = TimeSpan.FromSeconds(3);

        public string ServerName
        {
            get
            {
                return _serverName;
            }

            set
            {
                this._serverName = value;
                OnPropertyChanged(() => this.ServerName);
            }
        }

        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }

            set
            {
                this._databaseName = value;
                OnPropertyChanged(() => this.DatabaseName);
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }

            set
            {
                this._userName = value;
                OnPropertyChanged(() => this.UserName);
            }
        }

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

        public override string ToString()
        {
            return string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};Connection Timeout={4};Persist Security Info=True;MultipleActiveResultSets=True;",
                this.ServerName,
                this.DatabaseName,
                this.UserName,
                this.Password,
                (int)this.ConnectionTimeOut.TotalSeconds);
        }
    }
}
