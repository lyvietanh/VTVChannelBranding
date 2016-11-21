using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace VTVPlaylistEditor.Models
{
    public class ComingUpListModel : BindableBase
    {
        private string name = "";
        private ObservableCollection<ComingUpBlockModel> blocks = new ObservableCollection<ComingUpBlockModel>();
        private ObservableCollection<ComingUpTimeRangeModel> timeRanges = new ObservableCollection<ComingUpTimeRangeModel>();

        public ObservableCollection<ComingUpBlockModel> Blocks
        {
            get
            {
                return blocks;
            }

            set
            {
                this.blocks = value;
                OnPropertyChanged(() => this.Blocks);
            }
        }

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

        public ObservableCollection<ComingUpTimeRangeModel> TimeRanges
        {
            get
            {
                return timeRanges;
            }

            set
            {
                this.timeRanges = value;
                OnPropertyChanged(() => this.TimeRanges);
            }
        }

        public ComingUpListModel()
        {

        }

        public ComingUpBlockModel GetBlockByName(string name)
        {
            if (this.Blocks != null)
            {
                foreach (var item in this.Blocks)
                {
                    if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public ComingUpTimeRangeModel GetTimeRangeByName(string name)
        {
            if (this.TimeRanges != null)
            {
                foreach (var item in this.TimeRanges)
                {
                    if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public bool ContainProgramCode(string programCode)
        {
            if (this.Blocks != null)
            {
                foreach (var item in this.Blocks)
                {
                    if (item.ProgramCode.Equals(programCode, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            if (this.TimeRanges != null)
            {
                foreach (var item in this.TimeRanges)
                {
                    if (item.ProgramCode.Equals(programCode, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
