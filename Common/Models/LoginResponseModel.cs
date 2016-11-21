using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Common.Models
{
    public class LoginResponseModel : BindableBase
    {
        private bool? isLogon = null;
        private string messageString = "";

        public bool? IsLogon
        {
            get
            {
                return isLogon;
            }

            set
            {
                this.isLogon = value;
                OnPropertyChanged(() => this.IsLogon);
            }
        }

        public string MessageString
        {
            get
            {
                return messageString;
            }

            set
            {
                this.messageString = value;
                OnPropertyChanged(() => this.MessageString);
            }
        }

        public LoginResponseModel()
        {

        }
    }
}
