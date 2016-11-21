using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VTVPlaylistEditor.Settings
{
    [XmlType(TypeName = "TrafficDataManager")]
    public class TrafficDataManagerSetting
    {
        private TimeSpan _updateInterval = TimeSpan.FromSeconds(10);

        [XmlIgnore]
        public TimeSpan UpdateInterval
        {
            get
            {
                return _updateInterval;
            }

            set
            {
                this._updateInterval = value;
            }
        }
        [Browsable(false)]
        [XmlAttribute("UpdateInterval")]
        public string UpdateIntervalAsString
        {
            get
            {
                return Common.Utility.GetTimeStringFromTimeSpan(this.UpdateInterval);
            }
            set
            {
                this.UpdateInterval = Common.Utility.GetTimeSpanFromString(value);
            }
        }

    }
}
