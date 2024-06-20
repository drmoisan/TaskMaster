using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using Newtonsoft.Json;
using UtilitiesCS.Extensions;

namespace TaskMaster.AppGlobals
{
    public class ManagerConfig
    {
        public ManagerConfig(IApplicationGlobals globals)
        {
            Globals = globals;
            ResetLazy();
        }

        private void ResetLazy()
        {
            _localSettings = new Lazy<JsonSerializerSettings>(() => GetSettings(false));
            _networkSettings = new Lazy<JsonSerializerSettings>(() => GetSettings(true));
        }

        public FilePathHelper Local { get; set; }
        public FilePathHelper Network { get; set; }

        [JsonIgnore]
        public JsonSerializerSettings LocalSettings => _localSettings?.Value;
        private Lazy<JsonSerializerSettings> _localSettings;

        [JsonIgnore]
        public JsonSerializerSettings NetworkSettings => _networkSettings?.Value;
        private Lazy<JsonSerializerSettings> _networkSettings;

        public bool Active { get; set; }

        internal IApplicationGlobals Globals { get => _globals; set => _globals = value; }
        private IApplicationGlobals _globals;

        internal JsonSerializerSettings GetSettings(bool compress)
        {
            Globals.ThrowIfNull();
            var settings = ManagerClass.Static.GetDefaultSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            settings.Converters.Add(new AppGlobalsConverter(Globals));
            if (compress)
                settings.ContractResolver = new DoNotSerializeContractResolver("Prob", "NotMatch");
            return settings;
        }
    }
}
