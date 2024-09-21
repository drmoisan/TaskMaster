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
    using System.Runtime.CompilerServices;
    using System.ComponentModel;

    namespace UtilitiesCS.ReusableTypeClasses
    {
        public class SmartSerializableConfig : SmartSerializable<SmartSerializableConfig>, INotifyPropertyChanged
        {
            private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public SmartSerializableConfig() { }
            public SmartSerializableConfig(IApplicationGlobals globals)
            {
                Globals = globals;
                //ResetLazy();
            }

            private new void ResetLazy()
            {
                base._localJsonSettings = new Lazy<JsonSerializerSettings>(GetSettings);
                base._netJsonSettings = new Lazy<JsonSerializerSettings>(GetSettings);
            }

            protected bool _activated;
            public bool Activated { get => _activated; set { _activated = value; Notify(); }}

            [JsonProperty]
            internal IApplicationGlobals Globals { get => _globals; set => _globals = value; }
            private IApplicationGlobals _globals;

            public string Name { get => _name; set { _name = value; Notify(); }}
            private string _name;

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
                instance.ResetLazy();
                instance.ActivateMostRecent();
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

            #region INotifyPropertyChanged

            public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e) => Serialize();

            public event PropertyChangedEventHandler PropertyChanged;

            public void Notify([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion INotifyPropertyChanged
        }
    }

}
