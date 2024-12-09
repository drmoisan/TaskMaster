using AngleSharp.Common;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS;
using System.IO;
using System.Resources;
using UtilitiesCS.Extensions;


namespace UtilitiesCS.EmailIntelligence
{
    public class IntelligenceConfig(IApplicationGlobals globals)
    {        
        public static async Task<IntelligenceConfig> LoadAsync(IApplicationGlobals globals)
        {
            return await new IntelligenceConfig(globals).InitAsync();
        }

        public async Task<IntelligenceConfig> InitAsync()
        {
            Config = await ReadConfigurationAsync();
            return this;
        }

        internal IApplicationGlobals Globals { get; } = globals;

        public ConcurrentDictionary<string, SmartSerializableLoader> Config { get; protected set; }

        internal async Task<ConcurrentDictionary<string, SmartSerializableLoader>> ReadConfigurationAsync()
        {
            var resourceManager = IntelligenceResources.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
            var resourceDictionary = await resourceSet
                .Cast<DictionaryEntry>()
                .ToDictionary<string, string>()
                .ToAsyncEnumerable()
                .SelectAwait(async kvp =>
                {
                    var loader = await SmartSerializableLoader.DeserializeAsync(Globals, kvp.Value);
                    loader.PropertyChanged += Loader_PropertyChanged;
                    return new KeyValuePair<string, SmartSerializableLoader>(kvp.Key, loader);
                }).ToConcurrentDictionaryAsync();

            return resourceDictionary;
        }

        internal void Loader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains(nameof(SmartSerializableLoader.Config.ClassifierActivated)))
            {
                var loader = (SmartSerializableLoader)sender;
                
                WriteConfiguration();
            }
        }

        internal void WriteConfiguration()
        {
            string assemblyDirectory = Path.GetDirectoryName(typeof(IntelligenceResources).Assembly.Location);
            string resxFilePath = Path.Combine(assemblyDirectory, "IntelligenceResources.resx");

            var configurations = Config
                .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.SerializeToString()))
                .ToDictionary();

            using (var resxWriter = new ResXResourceWriter(resxFilePath))
            {
                foreach (var configuration in configurations)
                {
                    resxWriter.AddResource(configuration.Key, configuration.Value);
                }
                resxWriter.Generate();
            }
        }
    }
}
