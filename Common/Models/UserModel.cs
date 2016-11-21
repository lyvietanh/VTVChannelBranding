using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Common.Models
{
    public class UserModel : BindableBase
    {
        private string roles = "USER";
        private string userName = "";
        private string password = "";

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

        public string Roles
        {
            get
            {
                return roles;
            }

            set
            {
                this.roles = value;
                OnPropertyChanged(() => this.Roles);
            }
        }

        public UserModel()
        {

        }

        public bool HasRole(string roleName)
        {
            bool result = false;
            if (roleName != null)
            {
                roleName = roleName.ToLower();
                string[] roles = this.Roles.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (roles != null && roles.Length > 0)
                {
                    result = roles.Contains(roleName);
                }
            }
            return result;
        }
    }
}
