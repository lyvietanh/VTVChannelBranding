using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Models;

namespace VTVTrafficDataManager.Models
{
    [XmlType(TypeName = "Paging")]
    public class PagingModel : ValidationModelBase
    {
        private int _itemsPerPage = 100;

        [XmlAttribute]
        public int ItemsPerPage
        {
            get
            {
                return _itemsPerPage;
            }

            set
            {
                _itemsPerPage = value;

            }
        }
    }
}
