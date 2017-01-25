using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
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
                _programCode = Utility.ConvertCompositeToPrecomposed(value);
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
                _programTitle1 = Utility.ConvertCompositeToPrecomposed(value);
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
                _programTitle2 = Utility.ConvertCompositeToPrecomposed(value);
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
