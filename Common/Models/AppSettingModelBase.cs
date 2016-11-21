using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Prism.Mvvm;

namespace Common.Models
{
    [Serializable]
    [XmlRoot(ElementName = "AppSetting")]
    public class AppSettingModelBase<T> : BindableBase where T : class, new()
    {
        public delegate void LoadedHandler(object sender);
        public event LoadedHandler LoadCompleted = null;
        protected void OnLoadCompleted(object sender)
        {
            if (this.LoadCompleted != null)
            {
                this.LoadCompleted(sender);
            }
        }

        public delegate void SavedHandler(object sender);
        public event SavedHandler SaveCompleted = null;
        protected void OnSaveCompleted(object sender)
        {
            if (this.SaveCompleted != null)
            {
                this.SaveCompleted(sender);
            }
        }


        public bool Load(string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    T newAppSetting = (T)serializer.Deserialize(reader);
                    OnLoading(newAppSetting);
                }
                OnLoadCompleted(this);
                return true;
            }
            catch (Exception ex)
            {

            }
            Save(filePath);
            return false;
        }

        protected virtual void OnLoading(T newAppSetting) { }

        public bool Save(string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    serializer.Serialize(writer, this);
                    writer.Flush();
                }
                OnSaveCompleted(this);
                return true;
            }
            catch (Exception)
            {

            }
            return false;
        }
    }
}
