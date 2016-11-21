using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace VTVPlaylistEditor.Models
{
    public class SecondaryEventModel : BindableBase
    {
        private string name = "";
        private bool hasOffset = true;
        private TimeSpan offset = TimeSpan.Zero;
        private bool hasDuration = true;
        private TimeSpan duration = TimeSpan.Zero;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                this.name = value;
                OnPropertyChanged(() => this.Name);
            }
        }

        public TimeSpan Offset
        {
            get
            {
                return offset;
            }

            set
            {
                this.offset = value;
                OnPropertyChanged(() => this.Offset);
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return duration;
            }

            set
            {
                this.duration = value;
                OnPropertyChanged(() => this.Duration);
            }
        }

        public bool HasOffset
        {
            get
            {
                return hasOffset;
            }

            set
            {
                this.hasOffset = value;
                OnPropertyChanged(() => this.HasOffset);
            }
        }

        public bool HasDuration
        {
            get
            {
                return hasDuration;
            }

            set
            {
                this.hasDuration = value;
                OnPropertyChanged(() => this.HasDuration);
            }
        }

        public SecondaryEventModel()
        {

        }
    }
}
