using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Mvvm;

namespace Common.ViewModels
{
    public class DialogViewModel : BindableBase
    {
        private string _message = "";

        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                this._message = value;
                OnPropertyChanged(() => this.Message);
            }
        }
    }
}
