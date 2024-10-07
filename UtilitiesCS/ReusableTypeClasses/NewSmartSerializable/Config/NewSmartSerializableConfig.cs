using Newtonsoft.Json;
using System;
using System.IO;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.Extensions;
using System.ComponentModel;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class NewSmartSerializableConfig : INewSmartSerializableConfig
    {
        public NewSmartSerializableConfig()
        {
            ResetLazy();
            _activeDisk = INewSmartSerializableConfig.ActiveDiskEnum.Neither;
        }

        #region SerializationConfig

        protected FilePathHelper _disk = new FilePathHelper();
        public FilePathHelper Disk { get => _disk; set => _disk = value; }

        public FilePathHelper LocalDisk { get => _localDisk; set => _localDisk = value; }
        private FilePathHelper _localDisk = new FilePathHelper();

        public FilePathHelper NetDisk { get => _netDisk; set => _netDisk = value; }
        private FilePathHelper _netDisk = new();

        [JsonIgnore]
        public DateTime NetworkDate => File.Exists(NetDisk.FilePath) ?
            File.GetLastWriteTimeUtc(NetDisk.FilePath) : default;

        [JsonIgnore]
        public DateTime LocalDate => File.Exists(LocalDisk.FilePath) ?
            File.GetLastWriteTimeUtc(LocalDisk.FilePath) : default;

        private bool _classifierActivated;
        public bool ClassifierActivated { get => _classifierActivated; set => _classifierActivated = value; }

        public void ResetLazy()
        {
            _localJsonSettings = new Lazy<JsonSerializerSettings>(GetDefaultSettings);
            _netJsonSettings = new Lazy<JsonSerializerSettings>(GetDefaultSettings);
            _jsonSettings = new Lazy<JsonSerializerSettings>(GetDefaultSettings);
        }

        public void ResetLazy(
            Lazy<JsonSerializerSettings> localJsonSettings,
            Lazy<JsonSerializerSettings> netJsonSettings,
            Lazy<JsonSerializerSettings> jsonSettings)
        {
            _localJsonSettings = localJsonSettings;
            _netJsonSettings = netJsonSettings;
            _jsonSettings = jsonSettings;
        }

        [JsonIgnore]
        public JsonSerializerSettings JsonSettings { get => _jsonSettings.Value; set { _jsonSettings = value.ToLazy(); Notify(); } }
        protected Lazy<JsonSerializerSettings> _jsonSettings;

        [JsonIgnore]
        public JsonSerializerSettings NetJsonSettings { get => _netJsonSettings.Value; set { _netJsonSettings = value.ToLazy(); Notify(); } }
        protected Lazy<JsonSerializerSettings> _netJsonSettings;

        [JsonIgnore]
        public JsonSerializerSettings LocalJsonSettings { get => _localJsonSettings.Value; set { _localJsonSettings = value.ToLazy(); Notify(); } }
        protected Lazy<JsonSerializerSettings> _localJsonSettings;

        [JsonIgnore]
        public INewSmartSerializableConfig.ActiveDiskEnum ActiveDisk { get => _activeDisk; protected set { _activeDisk = value; Notify(); } } 
        protected INewSmartSerializableConfig.ActiveDiskEnum _activeDisk;

        public static JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
        }

        public void ActivateMostRecent()
        {
            if (NetworkDate != default && (LocalDate == default || NetworkDate > LocalDate))
            {
                ActivateNetDisk();
            }
            else
            {
                ActivateLocalDisk();
            }
        }

        public void ActivateLocalDisk()
        {
            _disk = _localDisk;
            _jsonSettings = _localJsonSettings;
            ActiveDisk = INewSmartSerializableConfig.ActiveDiskEnum.Local;
        }

        public void ActivateNetDisk()
        {
            _disk = _netDisk;
            _jsonSettings = _netJsonSettings;
            ActiveDisk = INewSmartSerializableConfig.ActiveDiskEnum.Net;
        }

        #endregion SerializationConfig

        #region IClonable

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public INewSmartSerializableConfig DeepCopy() 
        { 
            var clone = (NewSmartSerializableConfig)Clone();
            clone._disk = _disk.DeepCopy();
            clone._localDisk = _localDisk.DeepCopy();
            clone._netDisk = _netDisk.DeepCopy();
            clone._jsonSettings = JsonSettings.DeepCopy().ToLazy();
            clone._netJsonSettings = NetJsonSettings.DeepCopy().ToLazy();
            clone._localJsonSettings = LocalJsonSettings.DeepCopy().ToLazy();
            return clone;
        }

        public void CopyFrom(INewSmartSerializableConfig other, bool deep)
        {
            if (deep) { other = other.DeepCopy(); }

            // Using private fields to avoid triggering events recursively
            _classifierActivated = other.ClassifierActivated;
            Disk.CopyFrom(other.Disk);
            LocalDisk.CopyFrom(other.LocalDisk);
            NetDisk.CopyFrom(other.NetDisk);
            _jsonSettings = other.JsonSettings.ToLazy();
            _netJsonSettings = other.NetJsonSettings.ToLazy();
            _localJsonSettings = other.LocalJsonSettings.ToLazy();
            Notify("CopyFrom");
        }

        #endregion IClonable

        #region INotifyPropertyChanged

        public void Notify([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged


    }
}
