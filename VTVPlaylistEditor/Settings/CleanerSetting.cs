using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VTVPlaylistEditor.Settings
{
    [XmlType(TypeName = "Cleaner")]
    public class CleanerSetting
    {
        private bool _isEnable = true;
        private TimeSpan _autoCleanAtTime = new TimeSpan(0, 30, 0);

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
        public TimeSpan AutoCleanAtTime
        {
            get
            {
                return _autoCleanAtTime;
            }

            set
            {
                this._autoCleanAtTime = value;
            }
        }
        [Browsable(false)]
        [XmlAttribute("AutoCleanAtTime")]
        public string AutoCleanAtTimeAsString
        {
            get
            {
                return Common.Utility.GetTimeStringFromTimeSpan(this.AutoCleanAtTime);
            }
            set
            {
                this.AutoCleanAtTime = Common.Utility.GetTimeSpanFromString(value);
            }
        }

    }
}
