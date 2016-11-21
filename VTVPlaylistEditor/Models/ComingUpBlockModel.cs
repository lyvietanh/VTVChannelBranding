using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace VTVPlaylistEditor.Models
{
    public class ComingUpBlockModel : BindableBase
    {
        private string name = "";
        private string programCode = "";
        private TimeSpan beginTime = TimeSpan.Zero;
        private TimeSpan endTime = TimeSpan.Zero;
        private bool hasBeginTime = false;
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

        public TimeSpan BeginTime
        {
            get
            {
                return beginTime;
            }

            set
            {
                this.beginTime = value;
                OnPropertyChanged(() => this.BeginTime);
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

        public bool HasBeginTime
        {
            get
            {
                return hasBeginTime;
            }

            set
            {
                this.hasBeginTime = value;
                OnPropertyChanged(() => this.HasBeginTime);
            }
        }

        public ComingUpBlockModel()
        {

        }
    }
}
