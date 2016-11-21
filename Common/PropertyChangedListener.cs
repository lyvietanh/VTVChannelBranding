using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class PropertyChangedListener
    {
        private static PropertyChangedListener defaultInstance = null;
        public static PropertyChangedListener Default
        {
            get
            {
                if (defaultInstance == null) defaultInstance = new PropertyChangedListener();
                return defaultInstance;
            }
        }

        public delegate void ListeningHandler(object sender, string propertyName);
        public event ListeningHandler Listening = null;
        protected void OnListening(object sender, string propertyName)
        {
            if (this.Listening != null)
            {
                this.Listening(sender, propertyName);
            }
        }

        public PropertyChangedListener()
        {

        }

        public void Listen(string propertyName)
        {
            OnListening(this, propertyName);
        }
    }
}
