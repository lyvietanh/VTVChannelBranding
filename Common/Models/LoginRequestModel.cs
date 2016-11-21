using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Common.Models
{
    public class LoginRequestModel : BindableBase
    {
        private string userName = "";
        private string password = "";
        private object loginCommandParameter = null;
        private LoginResponseModel loginResponse = null;

        public string UserName
        {
            get
            {
                return userName;
            }

            set
            {
                this.userName = value;
                OnPropertyChanged(() => this.UserName);
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                this.password = value;
                OnPropertyChanged(() => this.Password);
            }
        }

        public object LoginCommandParameter
        {
            get
            {
                return loginCommandParameter;
            }

            set
            {
                this.loginCommandParameter = value;
                OnPropertyChanged(() => this.LoginCommandParameter);
            }
        }

        public LoginResponseModel LoginResponse
        {
            get
            {
                return loginResponse;
            }

            set
            {
                this.loginResponse = value;
                OnPropertyChanged(() => this.LoginResponse);
            }
        }

        public LoginRequestModel()
        {

        }
    }
}
