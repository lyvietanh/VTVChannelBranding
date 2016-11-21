using Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Common.Converters
{
    public class LoginRequestMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            LoginRequestModel loginRequestModel = new LoginRequestModel();
            if (values != null && values.Length > 0)
            {
                if (values.Length >= 1)
                {
                    loginRequestModel.LoginResponse = values[0] as LoginResponseModel;
                }
                if (values.Length >= 2)
                {
                    //TextBox txtUserName = values[1] as TextBox;
                    //loginRequestModel.UserName = txtUserName.Text;
                    loginRequestModel.UserName = values[1].ToString();
                }
                if (values.Length >= 3)
                {
                    //PasswordBox pwbPassword = values[2] as PasswordBox;
                    //loginRequestModel.Password = pwbPassword.Password;
                    loginRequestModel.Password = values[2].ToString();
                }
                if (values.Length >= 4)
                {
                    loginRequestModel.LoginCommandParameter = values[3];
                }
            }
            return loginRequestModel;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
