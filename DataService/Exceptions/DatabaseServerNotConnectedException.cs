using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Exceptions
{
    public class DatabaseServerNotConnectedException : Exception
    {
        public DatabaseServerNotConnectedException() : base()
        {

        }

        public DatabaseServerNotConnectedException(string message) : base(message)
        {

        }

        public DatabaseServerNotConnectedException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
