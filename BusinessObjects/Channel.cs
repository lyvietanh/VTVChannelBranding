using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public partial class Channel : BusinessObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string LastTrafficUpdateFilePath { get; set; }
        public DateTime LastTrafficUpdateFileTime { get; set; }

        public virtual List<TrafficEvent> TrafficEvents { get; set; }

        public Channel()
        {
            this.LastTrafficUpdateFileTime = DateTime.Now.Date.AddYears(-1);
            this.TrafficEvents = new List<TrafficEvent>();
        }
    }
}
