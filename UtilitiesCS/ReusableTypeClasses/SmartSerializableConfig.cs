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
    using System.Threading;

    namespace UtilitiesCS.ReusableTypeClasses
    {
        public class SmartSerializableConfig : SmartSerializable<SmartSerializableConfig>
        {
            private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public SmartSerializableConfig() { }
            public SmartSerializableConfig(IApplicationGlobals globals)
            {
                Globals = globals;
                //ResetLazy();
            }

            //private void ResetLazy()
            //{
            //    _localSettings = new Lazy<JsonSerializerSettings>(() => GetSettings(false));
            //    _networkSettings = new Lazy<JsonSerializerSettings>(() => GetSettings(true));
            //}

            //protected FilePathHelper _local;
            //public FilePathHelper Local { get => _local; set => _local = value; }

            //protected FilePathHelper _network;
            //public FilePathHelper Network { get => _network; set => _network = value; }

            //[JsonIgnore]
            //public JsonSerializerSettings LocalSettings => _localSettings?.Value;
            //private Lazy<JsonSerializerSettings> _localSettings;

            //[JsonIgnore]
            //public JsonSerializerSettings NetworkSettings => _networkSettings?.Value;
            //private Lazy<JsonSerializerSettings> _networkSettings;

            //[JsonIgnore]
            //public DateTime NetworkDate => File.Exists(Network.FilePath) ? File.GetLastWriteTimeUtc(Network.FilePath) : default;

            //[JsonIgnore]
            //public DateTime LocalDate => File.Exists(Local.FilePath) ? File.GetLastWriteTimeUtc(Local.FilePath) : default;

            //public void ActivateMostRecent()
            //{
            //    if (NetworkDate != default && (LocalDate == default || NetworkDate > LocalDate))
            //    {
            //        ActivateNetDisk();
            //    }
            //    else
            //    {
            //        ActivateLocalDisk();
            //    }
            //}

            protected bool _active;
            public bool Active { get => _active; set => _active = value; }

            internal IApplicationGlobals Globals { get => _globals; set => _globals = value; }
            private IApplicationGlobals _globals;

            private JsonSerializerSettings GetSettings()
            {
                Globals.ThrowIfNull();
                var settings = GetDefaultSettings();
                settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
                settings.Converters.Add(new AppGlobalsConverter(Globals));
                settings.Converters.Add(new FilePathHelperConverter(Globals.FS));
                return settings;
            }
            
            public static async Task<SmartSerializableConfig> DeserializeAsync(
                IApplicationGlobals globals, byte[] binary, CancellationToken cancel = default)
            {                
                try
                {
                    if (globals is null) { throw new ArgumentNullException(nameof(globals)); }
                    var loader = new SmartSerializableConfig(globals);
                    return await Task.Run(() => loader.DeserializeConfig(binary), cancel);
                }
                catch (ArgumentNullException e)
                {
                    logger.Error($"Error in {nameof(DeserializeAsync)}. {nameof(globals)} cannot be null\n{e.Message}", e);
                    throw;
                }

                catch (TaskCanceledException)
                {
                    logger.Warn("Task was cancelled.");
                    return null;
                }
                catch (Exception e)
                {
                    logger.Error($"Error in {nameof(DeserializeAsync)}.\n{e.Message}", e);
                    throw;
                }                
            }

            //internal static SmartSerializableConfig DeserializeConfig(IApplicationGlobals globals, byte[] binary)
            //{                
            //    var loader = new SmartSerializableConfig(globals);
            //    var jsonObject = loader.TryConvertBinaryToJson(binary);
            //    if (jsonObject.IsNullOrEmpty()) { return null; }
            //    else { return loader.DeserializeConfig(jsonObject); }                
            //}

            internal SmartSerializableConfig DeserializeConfig(byte[] binary)
            {
                var jsonObject = TryConvertBinaryToJson(binary);
                if (jsonObject.IsNullOrEmpty())
                {
                    return null;
                }
                else
                {
                    return DeserializeConfig(jsonObject);
                }
            }

            internal SmartSerializableConfig DeserializeConfig(string jsonObject)
            {
                var settings = GetSettings();
                SmartSerializableConfig instance = null;
                try
                {
                    instance = JsonConvert.DeserializeObject<SmartSerializableConfig>(
                        jsonObject, settings);
                }
                catch (Exception e)
                {
                    logger.Error($"Error in {nameof(DeserializeConfig)}.\n{e.Message}", e);
                    return null;
                }
                
                instance.Globals = Globals;
                instance.ActivateMostRecent();
                instance.JsonSettings = settings;
                //instance.ResetLazy();
                return instance;
            }

            internal string TryConvertBinaryToJson(byte[] binary)
            {
                try
                {
                    var jsonObject = System.Text.Encoding.UTF8.GetString(binary);
                    return jsonObject;
                }
                catch (Exception e)
                {
                    logger.Error($"Error in {nameof(TryConvertBinaryToJson)}.\n{e.Message}",e);
                    return null;
                }
            }
        }
    }

}
