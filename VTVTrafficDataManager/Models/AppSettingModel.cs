using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoMapper;
using Common.Models;
using Prism.Mvvm;

namespace VTVTrafficDataManager.Models
{
    [Serializable]
    [XmlRoot(ElementName = "AppSetting")]
    public class AppSettingModel : AppSettingModelBase<AppSettingModel>
    {
        private PagingModel _paging = new PagingModel();
        private ConnectionStringModel _connectionString = new ConnectionStringModel();
        private ObservableCollection<ChannelModel> _channels = new ObservableCollection<ChannelModel>();

        [XmlElement(ElementName = "Paging", Order = 1)]
        public PagingModel Paging
        {
            get
            {
                return _paging;
            }

            set
            {
                _paging = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(ElementName = "ConnectionString", Order = 2)]
        public ConnectionStringModel ConnectionString
        {
            get
            {
                return _connectionString;
            }

            set
            {
                this._connectionString = value;
                OnPropertyChanged(() => this.ConnectionString);
            }
        }

        [XmlArray(ElementName = "Channels", Order = 3)]
        public ObservableCollection<ChannelModel> Channels
        {
            get
            {
                return _channels;
            }

            set
            {
                this._channels = value;
                OnPropertyChanged(() => this.Channels);
            }
        }

        protected override void OnLoading(AppSettingModel newAppSetting)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<AppSettingModel, AppSettingModel>());
            Mapper.Map(newAppSetting, this, typeof(AppSettingModel), typeof(AppSettingModel));
        }

    }
}
