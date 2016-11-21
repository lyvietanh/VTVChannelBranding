using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace VTVTrafficDataManager.Models
{
    public class TrafficEventModel : BindableBase
    {
        private string _programCode = "";
        private string _programTitle1 = "";
        private string _programTitle2 = "";
        private DateTime _updateTime = DateTime.Now;

        public string ProgramCode
        {
            get
            {
                return _programCode;
            }

            set
            {
                this._programCode = value;
                OnPropertyChanged(() => this.ProgramCode);
            }
        }

        public string ProgramTitle1
        {
            get
            {
                return _programTitle1;
            }

            set
            {
                this._programTitle1 = value;
                OnPropertyChanged(() => this.ProgramTitle1);
            }
        }

        public string ProgramTitle2
        {
            get
            {
                return _programTitle2;
            }

            set
            {
                this._programTitle2 = value;
                OnPropertyChanged(() => this.ProgramTitle2);
            }
        }

        public DateTime UpdateTime
        {
            get
            {
                return _updateTime;
            }

            set
            {
                this._updateTime = value;
                OnPropertyChanged(() => this.UpdateTime);
            }
        }

    }
}
