using Newtonsoft.Json;
using System;
using System.IO;
using UtilitiesCS.Extensions.Lazy;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class NewSmartSerializableConfig
    {
        public NewSmartSerializableConfig() 
        {
            ResetLazy();
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
        public JsonSerializerSettings JsonSettings { get => _jsonSettings.Value; set => _jsonSettings = value.ToLazy(); }
        protected Lazy<JsonSerializerSettings> _jsonSettings;

        [JsonIgnore]
        public JsonSerializerSettings NetJsonSettings { get => _netJsonSettings.Value; set => _netJsonSettings = value.ToLazy(); }
        protected Lazy<JsonSerializerSettings> _netJsonSettings;

        [JsonIgnore]
        public JsonSerializerSettings LocalJsonSettings { get => _localJsonSettings.Value; set => _localJsonSettings = value.ToLazy(); }
        protected Lazy<JsonSerializerSettings> _localJsonSettings;

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
        }

        public void ActivateNetDisk()
        {
            _disk = _netDisk;
            _jsonSettings = _netJsonSettings;
        }


        #endregion SerializationConfig
    }
}
