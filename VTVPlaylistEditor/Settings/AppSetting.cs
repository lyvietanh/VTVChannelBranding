using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoMapper;

namespace VTVPlaylistEditor.Settings
{
    [Serializable]
    [XmlRoot(ElementName = "AppSetting")]
    public class AppSetting
    {
        public delegate void LoadedHandler(object sender);
        public event LoadedHandler Loaded = null;
        protected void OnLoaded(object sender)
        {
            if (this.Loaded != null)
            {
                this.Loaded(sender);
            }
        }

        public delegate void SavedHandler(object sender);
        public event SavedHandler Saved = null;
        protected void OnSaved(object sender)
        {
            if (this.Saved != null)
            {
                this.Saved(sender);
            }
        }

        private CharacterFilterSetting _characterFilterSetting = new CharacterFilterSetting();
        //private List<ChannelSetting> _channelSettings = new List<ChannelSetting>();

        [XmlElement(ElementName = "Database", Order = 1)]
        public CharacterFilterSetting CharacterFilterSetting
        {
            get
            {
                return _characterFilterSetting;
            }

            set
            {
                this._characterFilterSetting = value;
            }
        }

        //[XmlArray(ElementName = "Channels", Order = 2)]
        //public List<ChannelSetting> ChannelSettings
        //{
        //    get
        //    {
        //        return _channelSettings;
        //    }

        //    set
        //    {
        //        this._channelSettings = value;
        //    }
        //}

        public bool Load(string filePath)
        {
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(AppSetting));
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    AppSetting appSetting = (AppSetting)mySerializer.Deserialize(reader);
                    Mapper.Initialize(cfg => cfg.CreateMap<AppSetting, AppSetting>());
                    Mapper.Map(appSetting, this, typeof(AppSetting), typeof(AppSetting));
                }
                OnLoaded(this);
                return true;
            }
            catch (Exception)
            {

            }
            Save(filePath);
            return false;
        }

        public bool Save(string filePath)
        {
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(AppSetting));
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    mySerializer.Serialize(writer, this);
                    writer.Flush();
                }
                OnSaved(this);
                return true;
            }
            catch (Exception)
            {

            }
            return false;
        }
    }
}
