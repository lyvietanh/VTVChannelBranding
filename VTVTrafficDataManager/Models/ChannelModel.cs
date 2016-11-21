using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using ChiDuc.General;
using Common.Models;
using Common.Validations;
using DataService.Exceptions;
using MoreLinq;
using Prism.Mvvm;

namespace VTVTrafficDataManager.Models
{
    [XmlType(TypeName = "Channel")]
    public class ChannelModel : ValidationModelBase
    {
        private string _name = "";
        private string _title = "";
        private string _description = "";
        private string _lastTrafficUpdateFilePath = "";
        private DateTime _lastTrafficUpdateFileTime = DateTime.MinValue;
        private TimeSpan _trafficUpdateInterval = TimeSpan.FromSeconds(10);
        private string _trafficFolderPath = "";
        private string _trafficFileFilter = "";
        private ObservableCollection<TrafficEventModel> _trafficEvents = new ObservableCollection<TrafficEventModel>();

        private Thread _trafficProcessorThread = null;
        private bool _isTrafficProcessorThreadRunning = false;
        private DateTime _lastTrafficProcessorUpdateTime = DateTime.MinValue;
        private List<string> _skippedTrafficFiles = new List<string>();

        [XmlAttribute]
        [Required(ErrorMessage = "Bắt buộc.")]
        [NonSpacingAttribute(ErrorMessage = "Không được có khoảng trắng.")]
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                this._name = Common.Utility.TrimText(value);
                OnPropertyChanged(() => this.Name);
                Validate();
                //OnPropertyChanged(() => this.MultipleNames);
                if (string.IsNullOrEmpty(this.Title))
                {
                    this.Title = _name;
                }
            }
        }

        [XmlAttribute]
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                _title = Common.Utility.TrimText(value);

                if (string.IsNullOrEmpty(_title))
                {
                    _title = _name;
                }
                OnPropertyChanged(() => this.Title);
            }
        }

        //[XmlIgnore]
        //public string[] MultipleNames
        //{
        //    get
        //    {
        //        string[] names = this.Name.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        //        if (names != null && names.Length > 0)
        //        {
        //            return names;
        //        }
        //        return new string[] { this.Name };
        //    }
        //}

        //[XmlIgnore]
        //public string DisplayName
        //{
        //    get
        //    {
        //        string[] names = this.Name.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        //        if (names != null && names.Length > 0)
        //        {
        //            return names[0];
        //        }
        //        return this.Name;
        //    }
        //}

        [XmlIgnore]
        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                this._description = value;
                OnPropertyChanged(() => this.Description);
            }
        }

        [XmlAttribute]
        [Required(ErrorMessage = "Bắt buộc.")]
        public string TrafficFolderPath
        {
            get
            {
                return _trafficFolderPath;
            }

            set
            {
                this._trafficFolderPath = Common.Utility.TrimText(value);
                OnPropertyChanged(() => this.TrafficFolderPath);
                Validate();
            }
        }

        [XmlAttribute]
        public string TrafficFileFilter
        {
            get
            {
                return _trafficFileFilter;
            }

            set
            {
                this._trafficFileFilter = value;
                OnPropertyChanged(() => this.TrafficFileFilter);
            }
        }

        [XmlIgnore]
        public string LastTrafficUpdateFilePath
        {
            get
            {
                return _lastTrafficUpdateFilePath;
            }

            set
            {
                this._lastTrafficUpdateFilePath = value;
                OnPropertyChanged(() => this.LastTrafficUpdateFilePath);
            }
        }

        [XmlIgnore]
        public DateTime LastTrafficUpdateFileTime
        {
            get
            {
                return _lastTrafficUpdateFileTime;
            }

            set
            {
                this._lastTrafficUpdateFileTime = value;
                OnPropertyChanged(() => this.LastTrafficUpdateFileTime);
            }
        }

        [XmlIgnore]
        public ObservableCollection<TrafficEventModel> TrafficEvents
        {
            get
            {
                return _trafficEvents;
            }

            set
            {
                this._trafficEvents = value;
                OnPropertyChanged(() => this.TrafficEvents);
            }
        }

        [XmlIgnore]
        public TimeSpan TrafficUpdateInterval
        {
            get
            {
                return _trafficUpdateInterval;
            }

            set
            {
                this._trafficUpdateInterval = value;
                OnPropertyChanged(() => this.TrafficUpdateInterval);
            }
        }
        [Browsable(false)]
        [XmlAttribute("TrafficUpdateInterval")]
        public string TrafficUpdateIntervalAsString
        {
            get
            {
                return Common.Utility.GetTimeStringFromTimeSpan(this.TrafficUpdateInterval);
            }
            set
            {
                this.TrafficUpdateInterval = Common.Utility.GetTimeSpanFromString(value);
                OnPropertyChanged(() => this.TrafficUpdateIntervalAsString);
                OnPropertyChanged(() => this.TrafficUpdateInterval);
            }
        }

        public ChannelModel()
        {

        }

        public void StartTrafficProcessor()
        {
            StopTrafficProcessor();
            _isTrafficProcessorThreadRunning = true;
            _trafficProcessorThread = new Thread(new ThreadStart(ExecuteTrafficProcessorThread));
            _trafficProcessorThread.IsBackground = true;
            _trafficProcessorThread.Start();
        }

        private void ExecuteTrafficProcessorThread()
        {
            while (_isTrafficProcessorThreadRunning)
            {
                if (DateTime.Now - _lastTrafficProcessorUpdateTime >= this.TrafficUpdateInterval)
                {
                    UpdateTrafficData();
                    _lastTrafficProcessorUpdateTime = DateTime.Now;
                }
                Thread.Sleep(250);
            }
            _trafficProcessorThread = null;
            GC.Collect();
        }

        public void StopTrafficProcessor()
        {
            _isTrafficProcessorThreadRunning = false;
        }

        public void UpdateTrafficData()
        {
            if (this.TrafficEvents.Count != DataService.Service.Default.CountOfTrafficEvents(this.Name))
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    var trafficEventEntities = DataService.Service.Default.GetTrafficEvents(this.Name);
                    this.TrafficEvents = AppData.Mapper.Map<List<BusinessObjects.TrafficEvent>, ObservableCollection<TrafficEventModel>>(trafficEventEntities);
                });
            }
            var channelEntity = DataService.Service.Default.GetChannel(this.Name);
            if (channelEntity != null)
            {
                AppData.Mapper.Map(channelEntity, this, typeof(BusinessObjects.Channel), typeof(ChannelModel));
            }
            else
            {
                this.LastTrafficUpdateFilePath = "";
                this.LastTrafficUpdateFileTime = DateTime.Now.Date.AddYears(-1);
                channelEntity = AppData.Mapper.Map<ChannelModel, BusinessObjects.Channel>(this);
                DataService.Service.Default.CreateChannel(channelEntity);
            }


            string backupFileName = this.Name + "_TRAFFICLIST" + ".xml";
            string backupFilePath = Path.Combine(this.TrafficFolderPath, backupFileName);

            var di = new DirectoryInfo(this.TrafficFolderPath);
            if (di.Exists)
            {
                var filePaths = di.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly).Where(m => m.Name.Equals(backupFileName, StringComparison.OrdinalIgnoreCase) == false && m.Name.Substring(0, this.TrafficFileFilter.Length).Equals(this.TrafficFileFilter, StringComparison.OrdinalIgnoreCase) && m.LastWriteTime > this.LastTrafficUpdateFileTime).OrderBy(m => m.LastWriteTime).Select(m => m.FullName).ToArray();
                bool hasNew = false;
                for (int i = 0; i < filePaths.Length; i++)
                {
                    if (_isTrafficProcessorThreadRunning == false)
                        return;

                    if (ProcessToUpdateTrafficDataFile(filePaths[i]))
                    {
                        hasNew = true;
                    }
                }
                if (_skippedTrafficFiles != null && _skippedTrafficFiles.Count > 0)
                {
                    Debug.WriteLine(this.Name + " - _skippedTrafficFiles = " + _skippedTrafficFiles.Count);
                    int skippedCount = _skippedTrafficFiles.Count;
                    int j = 0;
                    while (j < skippedCount)
                    {
                        if (_isTrafficProcessorThreadRunning == false)
                            return;

                        if (ProcessToUpdateTrafficDataFile(_skippedTrafficFiles[0]))
                        {
                            hasNew = true;
                        }
                        _skippedTrafficFiles.RemoveAt(0);

                        ++j;
                    }
                    //while (_skippedTrafficFiles.Count > 0 && _isTrafficProcessorThreadRunning)
                    //{
                    //    if (_isTrafficProcessorThreadRunning == false)
                    //        return;

                    //    if (ProcessToUpdateTrafficDataFile(_skippedTrafficFiles[0]))
                    //    {
                    //        hasNew = true;
                    //        _skippedTrafficFiles.RemoveAt(0);
                    //    }
                    //}
                }
                else
                {
                    Debug.WriteLine(this.Name + " - trafficFiles = " + filePaths.Length);
                }

                if (hasNew)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(this.TrafficEvents);
                    if (view != null)
                    {
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
                        view.SortDescriptions.Add(new SortDescription("ProgramCode", ListSortDirection.Ascending));
                    }
                    //SaveTrafficDataToFile(backupFilePath);
                }
            }
        }

        private bool ProcessToUpdateTrafficDataFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var newTrafficEventModels = LoadTrafficDataFromFile(filePath);
                    if (newTrafficEventModels != null)
                    {
                        DateTime updateTime = File.GetLastWriteTime(filePath);

                        var tempAddedEntities = new List<BusinessObjects.TrafficEvent>();
                        for (int i = 0; i < newTrafficEventModels.Count; i++)
                        {
                            var trafficEventEntity = DataService.Service.Default.GetTrafficEvent(this.Name, newTrafficEventModels[i].ProgramCode);
                            if (trafficEventEntity != null)
                            {
                                if (trafficEventEntity.UpdateTime < newTrafficEventModels[i].UpdateTime)
                                {
                                    trafficEventEntity.ProgramTitle1 = newTrafficEventModels[i].ProgramTitle1;
                                    trafficEventEntity.ProgramTitle2 = newTrafficEventModels[i].ProgramTitle2;
                                    trafficEventEntity.UpdateTime = newTrafficEventModels[i].UpdateTime;
                                    DataService.Service.Default.UpdateTrafficEvent(trafficEventEntity);
                                }
                            }
                            else
                            {
                                trafficEventEntity = AppData.Mapper.Map<TrafficEventModel, BusinessObjects.TrafficEvent>(newTrafficEventModels[i]);
                                trafficEventEntity.ChannelName = this.Name;
                                tempAddedEntities.Add(trafficEventEntity);
                            }
                        }
                        if (tempAddedEntities != null && tempAddedEntities.Count > 0)
                        {
                            DataService.Service.Default.CreateTrafficEvents(tempAddedEntities.DistinctBy(m => m.ProgramCode).ToList());
                        }

                        if (updateTime > this.LastTrafficUpdateFileTime)
                        {
                            this.LastTrafficUpdateFileTime = updateTime;
                            this.LastTrafficUpdateFilePath = filePath;
                            var channelEntity = AppData.Mapper.Map<ChannelModel, BusinessObjects.Channel>(this);
                            DataService.Service.Default.UpdateChannel(channelEntity);
                        }

                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            var trafficEventEntities = DataService.Service.Default.GetTrafficEvents(this.Name);
                            this.TrafficEvents = AppData.Mapper.Map<List<BusinessObjects.TrafficEvent>, ObservableCollection<TrafficEventModel>>(trafficEventEntities);
                        });

                        return true;
                    }
                }
            }
            catch (Exception)
            {

            }
            return false;
        }

        private ObservableCollection<TrafficEventModel> LoadTrafficDataFromFile(string filePath)
        {
            ObservableCollection<TrafficEventModel> list = null;
            try
            {
                if (File.Exists(filePath))
                {
                    XmlDocument xmlDocument = FileManager.OpenXML(filePath, 5);
                    if (xmlDocument == null)
                    {
                        if (_skippedTrafficFiles.Contains(filePath) == false)
                        {
                            _skippedTrafficFiles.Add(filePath);
                        }
                    }
                    else
                    {
                        XmlElement nodeData = (XmlElement)xmlDocument.SelectSingleNode("/data");
                        //DateTime.TryParseExact(nodeData.GetAttribute("datetime"), "MM/dd/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out date);

                        XmlElement nodeTrafficXML = (XmlElement)xmlDocument.SelectSingleNode("/data/TRAFFICXML");
                        //string description = nodeTrafficXML.GetAttribute("description");

                        //thêm vào lúc 08:31 2016/09/14
                        //bool containName = false;
                        //for (int i = 0; i < this.MultipleNames.Length; i++)
                        //{
                        //    if (description.ToLower().Contains(this.MultipleNames[i].ToLower()))
                        //    {
                        //        containName = true;
                        //        break;
                        //    }
                        //}

                        //if (description.ToLower().Contains(this.Name.ToLower()))
                        //if (containName)
                        //{
                        list = new ObservableCollection<TrafficEventModel>();
                        DateTime updateTime = File.GetLastWriteTime(filePath);
                        XmlNodeList nodeRowList = nodeTrafficXML.SelectNodes("row");
                        foreach (var item in nodeRowList)
                        {
                            XmlElement nodeRow = item as XmlElement;
                            string programCode = "";
                            string programTitle1 = "";
                            string programTitle2 = "";

                            XmlNodeList nodes = nodeRow.ChildNodes;
                            foreach (var itemNode in nodes)
                            {
                                XmlElement node = itemNode as XmlElement;
                                switch (node.Name)
                                {
                                    //case "Date":
                                    //    date = DateTime.ParseExact(node.InnerText, "dd/MM/yyyy", null).Date;
                                    //    break;
                                    case "COD_PROGRA":
                                        programCode = node.InnerText;
                                        break;
                                    case "TenChuyenMucCongbo":
                                        programTitle1 = node.InnerText;
                                        break;
                                    case "TenCongBo":
                                        programTitle2 = node.InnerText;
                                        break;
                                    case "UpdateTime":
                                        updateTime = DateTime.Parse(node.InnerText);
                                        break;
                                }
                            }

                            //Nếu TenMu và TenCt là rỗng thì là đệm, không phải sự kiện chính nên bỏ qua
                            if (string.IsNullOrEmpty(programTitle1) && string.IsNullOrEmpty(programTitle2))
                            {
                                continue;
                            }

                            list.Add(new TrafficEventModel { ProgramCode = programCode, ProgramTitle1 = programTitle1, ProgramTitle2 = programTitle2, UpdateTime = updateTime });
                        }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.Save("LoadTrafficDataFromFile: " + ex.Message + "\n" + ex.StackTrace);
                //throw ex;
            }
            return list;
        }

        //private bool LoadTrafficDataFromFile(string filePath)
        //{
        //    bool result = false;
        //    try
        //    {
        //        if (File.Exists(filePath))
        //        {
        //            XmlDocument xmlDocument = FileManager.OpenXML(filePath, 5);

        //            if (xmlDocument != null)
        //            {
        //                XmlElement nodeData = (XmlElement)xmlDocument.SelectSingleNode("/data");
        //                //DateTime.TryParseExact(nodeData.GetAttribute("datetime"), "MM/dd/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out date);
        //                DateTime updateTime = File.GetLastWriteTime(filePath);

        //                XmlElement nodeTrafficXML = (XmlElement)xmlDocument.SelectSingleNode("/data/TRAFFICXML");
        //                string description = nodeTrafficXML.GetAttribute("description");
        //                if (description.ToLower().Contains(this.Name.ToLower()))
        //                {
        //                    int count = 0;
        //                    XmlNodeList nodeRowList = nodeTrafficXML.SelectNodes("row");
        //                    foreach (var item in nodeRowList)
        //                    {
        //                        XmlElement nodeRow = item as XmlElement;
        //                        string programCode = "";
        //                        string programTitle1 = "";
        //                        string programTitle2 = "";

        //                        XmlNodeList nodes = nodeRow.ChildNodes;
        //                        foreach (var itemNode in nodes)
        //                        {
        //                            XmlElement node = itemNode as XmlElement;
        //                            switch (node.Name)
        //                            {
        //                                //case "Date":
        //                                //    date = DateTime.ParseExact(node.InnerText, "dd/MM/yyyy", null).Date;
        //                                //    break;
        //                                case "COD_PROGRA":
        //                                    programCode = node.InnerText;
        //                                    break;
        //                                case "TenChuyenMucCongbo":
        //                                    programTitle1 = node.InnerText;
        //                                    break;
        //                                case "TenCongBo":
        //                                    programTitle2 = node.InnerText;
        //                                    break;
        //                                case "UpdateTime":
        //                                    updateTime = DateTime.Parse(node.InnerText);
        //                                    break;
        //                            }
        //                        }

        //                        //Nếu TenMu và TenCt là rỗng thì là đệm, không phải sự kiện chính nên bỏ qua
        //                        if (string.IsNullOrEmpty(programTitle1) && string.IsNullOrEmpty(programTitle2))
        //                        {
        //                            continue;
        //                        }

        //                        //Kiểm tra xem trong TrafficEvents có sự kiện nào trùng với ProgramCode chưa?
        //                        //Nếu có rồi thì sửa, ngược lại thì thêm
        //                        bool isExisted = false;
        //                        if (this.TrafficEvents.Count > 0)
        //                        {
        //                            foreach (var trafficEventModel in this.TrafficEvents)
        //                            {
        //                                if (trafficEventModel.ProgramCode.Equals(programCode, StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    isExisted = true;
        //                                    if (trafficEventModel.UpdateTime < updateTime)
        //                                    {
        //                                        //Sửa
        //                                        trafficEventModel.ProgramTitle1 = programTitle1;
        //                                        trafficEventModel.ProgramTitle2 = programTitle2;
        //                                        trafficEventModel.UpdateTime = updateTime;

        //                                        //Database
        //                                        var trafficEventEntity = Mapper.Map<TrafficEventModel, BusinessObjects.TrafficEvent>(trafficEventModel);
        //                                        //trafficEventEntity.Channel = Mapper.Map<ChannelModel, BusinessObjects.Channel>(this);
        //                                        trafficEventEntity.ChannelName = this.Name;
        //                                        DataService.Service.Default.UpdateTrafficEvent(trafficEventEntity);
        //                                    }
        //                                    break;
        //                                }
        //                            }
        //                        }

        //                        if (isExisted == false)
        //                        {
        //                            //Thêm
        //                            TrafficEventModel trafficEventModel = new TrafficEventModel
        //                            {
        //                                ProgramCode = programCode,
        //                                ProgramTitle1 = programTitle1,
        //                                ProgramTitle2 = programTitle2,
        //                                UpdateTime = updateTime
        //                            };

        //                            //safe thread
        //                            System.Windows.Application.Current.Dispatcher.Invoke(delegate
        //                            {
        //                                this.TrafficEvents.Add(trafficEventModel);
        //                            });

        //                            //Database
        //                            var trafficEventEntity = Mapper.Map<TrafficEventModel, BusinessObjects.TrafficEvent>(trafficEventModel);
        //                            //trafficEventEntity.Channel = Mapper.Map<ChannelModel, BusinessObjects.Channel>(this);
        //                            trafficEventEntity.ChannelName = this.Name;
        //                            DataService.Service.Default.CreateTrafficEvent(trafficEventEntity);
        //                        }
        //                        ++count;
        //                    }
        //                    if (count > 0)
        //                    {
        //                        OnPropertyChanged(() => this.TrafficEvents);
        //                        result = true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogUtility.Save("LoadTrafficDataFromFile: " + ex.Message + "\n" + ex.StackTrace);
        //        //throw ex;
        //    }
        //    return result;
        //}

        public void SaveTrafficDataToFile(string filePath)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();

                XmlElement nodeData = xmlDocument.CreateElement("data");
                xmlDocument.AppendChild(nodeData);

                XmlElement nodeTrafficXML = xmlDocument.CreateElement("TRAFFICXML");
                nodeTrafficXML.SetAttribute("description", "channel=" + this.Name.ToUpper());
                nodeData.AppendChild(nodeTrafficXML);

                foreach (var trafficEvent in this.TrafficEvents)
                {
                    if (trafficEvent.UpdateTime.Date >= DateTime.Now.Date.AddDays(-5) && trafficEvent.UpdateTime.Date <= DateTime.Now.Date.AddDays(5))
                    {
                        XmlElement nodeRow = xmlDocument.CreateElement("row");
                        nodeTrafficXML.AppendChild(nodeRow);

                        //XmlElement nodeDate = xmlDocument.CreateElement("Date");
                        //nodeDate.InnerText = trafficEvent.Date.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                        //nodeRow.AppendChild(nodeDate);

                        XmlElement nodeProgramCode = xmlDocument.CreateElement("COD_PROGRA");
                        nodeProgramCode.InnerText = trafficEvent.ProgramCode;
                        nodeRow.AppendChild(nodeProgramCode);

                        XmlElement nodeTenMu = xmlDocument.CreateElement("TenChuyenMucCongbo");
                        nodeTenMu.InnerText = trafficEvent.ProgramTitle1;
                        nodeRow.AppendChild(nodeTenMu);

                        XmlElement nodeTenCt = xmlDocument.CreateElement("TenCongBo");
                        nodeTenCt.InnerText = trafficEvent.ProgramTitle2;
                        nodeRow.AppendChild(nodeTenCt);

                        XmlElement nodeUpdateTime = xmlDocument.CreateElement("UpdateTime");
                        nodeUpdateTime.InnerText = trafficEvent.UpdateTime.ToString();
                        nodeRow.AppendChild(nodeUpdateTime);
                    }
                }

                //Kiểm tra, nếu file đang được dùng bởi ứng dụng khác thì chờ 1 lúc
                while (FileManager.IsFileLocked(new FileInfo(filePath)))
                {
                    Thread.Sleep(100);
                }

                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    xmlDocument.Save(sw);
                    sw.Close();
                }
            }
            catch (Exception)
            {

            }
        }

    }
}
