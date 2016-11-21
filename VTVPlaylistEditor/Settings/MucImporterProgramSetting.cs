using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VTVPlaylistEditor.Settings
{
    [XmlType(TypeName = "MucImporterProgram")]
    public class MucImporterProgramSetting
    {
        private bool _isEnable = true;
        private TimeSpan _delayInterval = TimeSpan.FromSeconds(3);
        private string _filePath = @"C:\Program Files (x86)\Vizrt\Viz Multichannel\PlayList Importer\PlayListImporter.exe";

        [XmlAttribute]
        public bool IsEnable
        {
            get
            {
                return _isEnable;
            }

            set
            {
                this._isEnable = value;
            }
        }

        [XmlIgnore]
        public TimeSpan DelayInterval
        {
            get
            {
                return _delayInterval;
            }

            set
            {
                this._delayInterval = value;
            }
        }
        [Browsable(false)]
        [XmlAttribute("DelayInterval")]
        public string DelayIntervalAsString
        {
            get
            {
                return Common.Utility.GetTimeStringFromTimeSpan(this.DelayInterval);
            }
            set
            {
                this.DelayInterval = Common.Utility.GetTimeSpanFromString(value);
            }
        }

        [XmlAttribute]
        public string FilePath
        {
            get
            {
                return _filePath;
            }

            set
            {
                this._filePath = value;
            }
        }

    }
}
