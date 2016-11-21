using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace VTVPlaylistEditor.Models
{
    public class ComingUpTimeRangeModel : BindableBase
    {
        private string name = "";
        private string programCode = "";
        private TimeSpan endTime = TimeSpan.Zero;
        private int count = 3;
        private bool hasEndTime = false;
        private bool hasCount = false;
        private List<EventModel> events = new List<EventModel>();

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                this.name = Common.Utility.ConvertTextToUpper(value);
                OnPropertyChanged(() => this.Name);
            }
        }

        public string ProgramCode
        {
            get
            {
                return programCode;
            }

            set
            {
                this.programCode = value;
                OnPropertyChanged(() => this.ProgramCode);
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                return endTime;
            }

            set
            {
                this.endTime = value;
                OnPropertyChanged(() => this.EndTime);
            }
        }

        public List<EventModel> Events
        {
            get
            {
                return events;
            }

            set
            {
                this.events = value;
                OnPropertyChanged(() => this.Events);
            }
        }

        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                this.count = value;
                OnPropertyChanged(() => this.Count);
            }
        }

        public bool HasEndTime
        {
            get
            {
                return hasEndTime;
            }

            set
            {
                this.hasEndTime = value;
                OnPropertyChanged(() => this.HasEndTime);
            }
        }

        public bool HasCount
        {
            get
            {
                return hasCount;
            }

            set
            {
                this.hasCount = value;
                OnPropertyChanged(() => this.HasCount);
            }
        }
    }
}
