using Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Commands;

namespace Common.Controls
{
    /// <summary>
    /// Interaction logic for UCLogin.xaml
    /// </summary>
    public partial class UCLogin : UserControl
    {
        public static DependencyProperty LoginCommandProperty = DependencyProperty.Register("LoginCommand", typeof(ICommand), typeof(UCLogin), new PropertyMetadata(null));
        public static DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(UCLogin), new PropertyMetadata(null));
        public static DependencyProperty LoginCommandParameterProperty = DependencyProperty.Register("LoginCommandParameter", typeof(object), typeof(UCLogin), new PropertyMetadata(null));
        public static DependencyProperty CancelCommandParameterProperty = DependencyProperty.Register("CancelCommandParameter", typeof(object), typeof(UCLogin), new PropertyMetadata(null));
        public static DependencyProperty LoginResponseProperty = DependencyProperty.Register("LoginResponse", typeof(LoginResponseModel), typeof(UCLogin), new PropertyMetadata(null));

        public LoginResponseModel LoginResponse
        {
            get
            {
                return (LoginResponseModel)GetValue(LoginResponseProperty);
            }

            set
            {
                SetValue(LoginResponseProperty, value);
            }
        }

        //public LoginResponseModel LoginResponse { get; set; }

        public ICommand ClearPasswordCommand { get; private set; }

        public ICommand LoginCommand
        {
            get
            {
                return (ICommand)GetValue(LoginCommandProperty);
            }

            set
            {
                SetValue(LoginCommandProperty, value);
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return (ICommand)GetValue(CancelCommandProperty);
            }

            set
            {
                SetValue(CancelCommandProperty, value);
            }
        }

        public object LoginCommandParameter
        {
            get
            {
                return (object)GetValue(LoginCommandParameterProperty);
            }

            set
            {
                SetValue(LoginCommandParameterProperty, value);
            }
        }

        public object CancelCommandParameter
        {
            get
            {
                return (object)GetValue(CancelCommandParameterProperty);
            }

            set
            {
                SetValue(CancelCommandParameterProperty, value);
            }
        }

        public UCLogin()
        {
            InitializeComponent();

            this.LoginResponse = new LoginResponseModel();
            this.ClearPasswordCommand = new DelegateCommand<object>(OnClearPassword);
        }

        private void OnClearPassword(object obj)
        {
            PasswordBox pwb = obj as PasswordBox;
            pwb.Clear();
            this.LoginResponse.IsLogon = null;
            this.LoginResponse.MessageString = "";
            //Debug.WriteLine("Clear password");
        }
    }
}
