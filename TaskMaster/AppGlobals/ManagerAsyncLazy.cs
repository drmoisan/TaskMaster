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
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;

namespace TaskMaster.AppGlobals
{
    //public class ManagerAsyncLazy : ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>
    //{
    //    #region ctors

    //    public ManagerAsyncLazy(IApplicationGlobals globals) : base() 
    //    { 
    //        Globals = globals;
    //        ResetConfiguration();
    //    }

    //    public async Task InitAsync() => await ResetLoadManagerLazyAsync();

    //    #endregion ctors

    //    #region Public Properties

    //    protected IApplicationGlobals Globals { get; set; }

    //    public AsyncLazy<ConcurrentDictionary<string, NewSmartSerializableLoader>> ManagerConfiguration { get; protected set; }

    //    #endregion Public Properties

    //    #region Configuration

    //    internal async void Loader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //    {
    //        if (e.PropertyName == nameof(NewSmartSerializableLoader.Activated))
    //        {
    //            var loader = (NewSmartSerializableLoader)sender;
    //            if (loader.Activated && !this.TryGetValue(nameof(loader.Name), out var classifier))
    //            {                    
    //                var classifierGroup = GetClassifierAsyncLazy(loader);
    //                if (classifierGroup != null) { this[loader.Name] = classifierGroup; }
    //            }
    //            else if (!loader.Activated)
    //            {
    //                this.TryRemove(loader.Name, out _);
    //            }
    //            await WriteConfigurationAsync();
    //        }
    //    }

    //    internal async Task<ConcurrentDictionary<string, NewSmartSerializableLoader>> ReadConfiguration()
    //    {
    //        var resourceManager = ManagerResources.ResourceManager;
    //        var resourceSet = resourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
    //        var resourceDictionary = await resourceSet
    //            .Cast<DictionaryEntry>()
    //            .ToDictionary<string, string>()
    //            .ToAsyncEnumerable()
    //            .SelectAwait(async kvp =>
    //            {
    //                var loader = await NewSmartSerializableLoader.DeserializeAsync(Globals, kvp.Value);
    //                loader.PropertyChanged += Loader_PropertyChanged;
    //                return new KeyValuePair<string, NewSmartSerializableLoader>(kvp.Key, loader);
    //            }).ToConcurrentDictionaryAsync();

    //        return resourceDictionary;
    //    }

    //    internal async Task WriteConfigurationAsync() 
    //    {
    //        string assemblyDirectory = Path.GetDirectoryName(typeof(ManagerResources).Assembly.Location);
    //        //string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    //        //string resxFilePath = Path.Combine(assemblyDirectory, "Resources", "ManagerResources.resx");
    //        string resxFilePath = Path.Combine(assemblyDirectory, "ManagerResources.resx");

    //        var configurations = (await ManagerConfiguration)
    //            .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.SerializeToString()))
    //            .ToDictionary();

    //        using (var resxWriter = new ResXResourceWriter(resxFilePath))
    //        {
    //            foreach (var configuration in configurations)
    //            {
    //                resxWriter.AddResource(configuration.Key, configuration.Value);
    //            }
    //            resxWriter.Generate();
    //        }
    //    }

    //    #endregion Configuration

    //    #region AsyncLazy Methods

    //    public AsyncLazy<BayesianClassifierGroup> GetClassifierAsyncLazy(NewSmartSerializableLoader loader)
    //    {
    //        return new AsyncLazy<BayesianClassifierGroup>(async () => await BayesianClassifierGroup.Static.DeserializeAsync(loader));
    //    }

    //    public void ResetConfiguration() => ManagerConfiguration = new(ReadConfiguration);

    //    public async Task ResetLoadManagerLazyAsync()
    //    {
    //        if (ManagerConfiguration is null) { ResetConfiguration(); }
    //        foreach (var configuration in await ManagerConfiguration)
    //        {
    //            if (configuration.Value.Activated)
    //            {
    //                var classifierGroup = GetClassifierAsyncLazy(configuration.Value);
    //                if (classifierGroup != null) { this[configuration.Value.Name] = classifierGroup; }
    //            }
    //            else
    //            {
    //                this.TryRemove(configuration.Key, out _);
    //            }
    //        }
    //    }

    //    #endregion AsyncLazy Methods
    //}
}
