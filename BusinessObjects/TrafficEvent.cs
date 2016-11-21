using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public partial class TrafficEvent : BusinessObject
    {
        public string ProgramCode { get; set; }
        public string ProgramTitle1 { get; set; }
        public string ProgramTitle2 { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        public string ChannelName { get; set; }
        //public virtual Channel Channel { get; set; }

        public TrafficEvent()
        {
            this.CreateTime = DateTime.Now;
            this.UpdateTime = DateTime.Now.Date.AddYears(-1);
        }
    }
}
