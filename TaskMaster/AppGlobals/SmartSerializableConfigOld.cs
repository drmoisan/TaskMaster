using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using Newtonsoft.Json;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;

namespace TaskMaster.AppGlobals
{
    public class SmartSerializableConfigOld: ISmartSerializable<SmartSerializableConfigOld>
    {
        public SmartSerializableConfigOld() { }
        public SmartSerializableConfigOld(IApplicationGlobals globals)
        {
            Globals = globals;
            ResetLazy();
        }

        private void ResetLazy()
        {
            _localSettings = new Lazy<JsonSerializerSettings>(() => GetSettings(false));
            _networkSettings = new Lazy<JsonSerializerSettings>(() => GetSettings(true));
        }

        protected FilePathHelper _local;
        public FilePathHelper Local { get => _local; set => _local = value; }

        protected FilePathHelper _network;
        public FilePathHelper Network { get => _network; set => _network = value; }

        [JsonIgnore]
        public JsonSerializerSettings LocalSettings => _localSettings?.Value;
        private Lazy<JsonSerializerSettings> _localSettings;

        [JsonIgnore]
        public JsonSerializerSettings NetworkSettings => _networkSettings?.Value;
        private Lazy<JsonSerializerSettings> _networkSettings;

        protected bool _active;
        public bool Active { get => _active; set => _active = value; }

        internal IApplicationGlobals Globals { get => _globals; set => _globals = value; }
        public FilePathHelper Disk { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public JsonSerializerSettings JsonSettings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public FilePathHelper LocalDisk { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public JsonSerializerSettings LocalJsonSettings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public FilePathHelper NetDisk { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public JsonSerializerSettings NetJsonSettings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private IApplicationGlobals _globals;

        internal JsonSerializerSettings GetSettings(bool compress)
        {
            Globals.ThrowIfNull();
            var settings = ManagerClass.Static.GetDefaultSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            settings.Converters.Add(new AppGlobalsConverter(Globals));
            settings.Converters.Add(new FilePathHelperConverter(Globals.FS));
            if (compress)
                settings.ContractResolver = new DoNotSerializeContractResolver("Prob", "NotMatch");
            return settings;
        }

        public void ActivateLocalDisk()
        {
            throw new NotImplementedException();
        }

        public void ActivateNetDisk()
        {
            throw new NotImplementedException();
        }

        public SmartSerializableConfigOld Deserialize(string fileName, string folderPath)
        {
            throw new NotImplementedException();
        }

        public SmartSerializableConfigOld Deserialize(string fileName, string folderPath, bool askUserOnError)
        {
            throw new NotImplementedException();
        }

        public SmartSerializableConfigOld Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings)
        {
            throw new NotImplementedException();
        }

        public void Serialize()
        {
            throw new NotImplementedException();
        }

        public void Serialize(string filePath)
        {
            throw new NotImplementedException();
        }

        public void SerializeThreadSafe(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
