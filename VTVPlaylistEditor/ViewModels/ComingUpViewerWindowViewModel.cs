using ChiDuc.General;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTVPlaylistEditor.Models;
using Prism.Mvvm;

namespace VTVPlaylistEditor.ViewModels
{
    public class ComingUpViewerWindowViewModel : BindableBase
    {
        private ObservableCollection<EventModel> comingUpEvents = new ObservableCollection<EventModel>();
        private string filePath = "";
        private DateTime lastUpdateTime = DateTime.MinValue;

        public ObservableCollection<EventModel> ComingUpEvents
        {
            get
            {
                return comingUpEvents;
            }

            set
            {
                this.comingUpEvents = value;
                OnPropertyChanged(() => this.ComingUpEvents);
            }
        }

        public string FilePath
        {
            get
            {
                return filePath;
            }

            set
            {
                this.filePath = value;
                OnPropertyChanged(() => this.FilePath);
            }
        }

        public DateTime LastUpdateTime
        {
            get
            {
                return lastUpdateTime;
            }

            set
            {
                this.lastUpdateTime = value;
                OnPropertyChanged(() => this.LastUpdateTime);
            }
        }

        public ComingUpViewerWindowViewModel()
        {

        }

        public void LoadDataFromFile(FileInfo fi)
        {
            if (File.Exists(fi.FullName))
            {
                this.FilePath = fi.FullName;
                this.LastUpdateTime = fi.LastWriteTime;

                string[] comingUpLines = FileManager.LoadTextArrayFromFile(this.FilePath);
                if (comingUpLines != null)
                {
                    this.ComingUpEvents.Clear();
                    for (int i = 0; i < comingUpLines.Length; i++)
                    {
                        EventModel eventModel = new EventModel(null);
                        string[] cols = comingUpLines[i].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        eventModel.BeginTime = cols.Length > 0 ? cols[0] : "";
                        eventModel.TenMu = cols.Length > 1 ? cols[1] : "";
                        eventModel.TenCt = cols.Length > 2 ? cols[2] : "";
                        this.ComingUpEvents.Add(eventModel);
                    }
                }
            }
        }
    }
}
