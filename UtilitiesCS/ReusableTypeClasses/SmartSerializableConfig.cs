using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.ReusableTypeClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UtilitiesCS;
    using Newtonsoft.Json;
    using UtilitiesCS.ReusableTypeClasses;
    using global::UtilitiesCS.Extensions;
    using System.IO;

    namespace UtilitiesCS.ReusableTypeClasses
    {
        public class SmartSerializableConfig : SmartSerializable<SmartSerializableConfig>
        {
            public SmartSerializableConfig() { }
            public SmartSerializableConfig(IApplicationGlobals globals)
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

            [JsonIgnore]
            public DateTime NetworkDate => File.Exists(Network.FilePath) ? File.GetLastWriteTimeUtc(Network.FilePath) : default;

            [JsonIgnore]
            public DateTime LocalDate => File.Exists(Local.FilePath) ? File.GetLastWriteTimeUtc(Local.FilePath) : default;

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

            protected bool _active;
            public bool Active { get => _active; set => _active = value; }

            internal IApplicationGlobals Globals { get => _globals; set => _globals = value; }            
            private IApplicationGlobals _globals;

            internal JsonSerializerSettings GetSettings(bool compress)
            {
                Globals.ThrowIfNull();
                var settings = GetDefaultSettings();
                settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
                settings.Converters.Add(new AppGlobalsConverter(Globals));
                settings.Converters.Add(new FilePathHelperConverter(Globals.FS));
                if (compress)
                    settings.ContractResolver = new DoNotSerializeContractResolver("Prob", "NotMatch");
                return settings;
            }

            public SmartSerializableConfig DeserializeJson(string objectText, JsonSerializerSettings settings)
            {
                var instance = JsonConvert.DeserializeObject<SmartSerializableConfig>(
                    objectText, settings);
                instance.Globals = Globals;
                instance.JsonSettings = settings;
                instance.ResetLazy();
                return instance;
            }

        }
    }

}
