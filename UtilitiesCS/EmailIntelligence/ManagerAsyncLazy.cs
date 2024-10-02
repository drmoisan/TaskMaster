using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BrightIdeasSoftware;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Newtonsoft.Json;

using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public class ManagerAsyncLazy : ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>
    {
        #region ctors

        public ManagerAsyncLazy(IApplicationGlobals globals) : base() 
        { 
            Globals = globals;
            ResetConfigAsyncLazy();
        }

        public async Task InitAsync() => await ResetLoadManagerAsyncLazy();

        #endregion ctors

        protected IApplicationGlobals Globals { get; set; }

        #region Configuration

        public AsyncLazy<ConcurrentDictionary<string, NewSmartSerializableLoader>> Configuration { get; protected set; }

        #region Configuration

        internal async Task<ConcurrentDictionary<string, NewSmartSerializableLoader>> ReadConfiguration()
        {
            var resourceManager = ManagerResources.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
            var resourceDictionary = await resourceSet
                .Cast<DictionaryEntry>()
                .ToDictionary<string, string>()
                .ToAsyncEnumerable()
                .SelectAwait(async kvp =>
                {
                    var loader = await NewSmartSerializableLoader.DeserializeAsync(Globals, kvp.Value);
                    loader.PropertyChanged += Loader_PropertyChanged;
                    return new KeyValuePair<string, NewSmartSerializableLoader>(kvp.Key, loader);
                }).ToConcurrentDictionaryAsync();

            return resourceDictionary;
        }

        public void ResetConfigAsyncLazy() => Configuration = new(ReadConfiguration);

        internal async Task WriteConfigurationAsync() 
        {
            string assemblyDirectory = Path.GetDirectoryName(typeof(ManagerResources).Assembly.Location);
            //string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //string resxFilePath = Path.Combine(assemblyDirectory, "Resources", "ManagerResources.resx");
            string resxFilePath = Path.Combine(assemblyDirectory, "ManagerResources.resx");

            var configurations = (await Configuration)
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

        #endregion Serialization of Configuration

        internal async void Loader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NewSmartSerializableLoader.Activated))
            {
                var loader = (NewSmartSerializableLoader)sender;
                if (loader.Activated && !this.TryGetValue(nameof(loader.Name), out var classifier))
                {                    
                    var classifierGroup = ResetLoadClassifierAsyncLazy(loader);
                    if (classifierGroup != null) { this[loader.Name] = classifierGroup; }
                }
                else if (!loader.Activated)
                {
                    this.TryRemove(loader.Name, out _);
                }
                await WriteConfigurationAsync();
            }
        }
        
        #endregion Configuration

        #region Manager Initialization

        public AsyncLazy<BayesianClassifierGroup> ResetLoadClassifierAsyncLazy(NewSmartSerializableLoader loader)
        {
            return new AsyncLazy<BayesianClassifierGroup>(async () => await BayesianClassifierGroup.Static.DeserializeAsync(loader));
        }

        public async Task ResetLoadManagerAsyncLazy()
        {
            if (Configuration is null) { ResetConfigAsyncLazy(); }
            foreach (var configuration in await Configuration)
            {
                if (configuration.Value.Activated)
                {
                    var classifierGroup = ResetLoadClassifierAsyncLazy(configuration.Value);
                    if (classifierGroup != null) { this[configuration.Value.Name] = classifierGroup; }
                }
                else
                {
                    this.TryRemove(configuration.Key, out _);
                }
            }
        }

        #endregion Manager Initialization

    }
}
