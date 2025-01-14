using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using log4net.Repository.Hierarchy;
using Microsoft.Graph.Security.AttackSimulation.Trainings.Item.LanguageDetails;
using Newtonsoft.Json;

using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS
{
    public class ManagerAsyncLazy : ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>
    {
        #region ctors

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ManagerAsyncLazy(IApplicationGlobals globals) : base() 
        { 
            Globals = globals;
            ResetConfigAsyncLazy();
        }

        public async Task InitAsync() => await ResetLoadManagerAsyncLazy();

        #endregion ctors

        protected IApplicationGlobals Globals { get; set; }

        #region Configuration

        public AsyncLazy<ConcurrentDictionary<string, SmartSerializableLoader>> Configuration { get; protected set; }
        private ConcurrentDictionary<string, SmartSerializableLoader> _privateConfig;

        internal async Task<ConcurrentDictionary<string, SmartSerializableLoader>> ReadConfiguration()
        {
            var resourceManager = ManagerResources.ResourceManager;
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

        public void ResetConfigAsyncLazy() => Configuration = new(ReadConfiguration);

        internal async Task WriteConfigurationAsync() 
        {
            string assemblyDirectory = Path.GetDirectoryName(typeof(ManagerResources).Assembly.Location);
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

        internal async void Loader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains(nameof(SmartSerializableLoader.Config.ClassifierActivated)))
            {
                var loader = (SmartSerializableLoader)sender;
                if (loader.Config.ClassifierActivated && !this.TryGetValue(nameof(loader.Name), out var classifier))
                {                    
                    var classifierGroup = GetAsyncLazyClassifierLoader(loader);
                    if (classifierGroup != null) 
                    { 
                        this[loader.Name] = classifierGroup; 
                        await Globals.Engines.RestartEngineAsync(loader.Name);
                    }
                }
                else if (!loader.Config.ClassifierActivated)
                {
                    this.TryRemove(loader.Name, out _);
                    Globals.Engines.InboxEngines.TryRemove(loader.Name, out _);
                }
                await WriteConfigurationAsync();
            }
            else if (e.PropertyName.Contains(nameof(SmartSerializableLoader.T))) 
            { await WriteConfigurationAsync(); }
        }

        internal async void Config_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // using reflection because the sender is a smart serializable object of base type T
            Type sst = sender.GetType();
            PropertyInfo nameProperty = sst.GetProperty("Name");
            string name = nameProperty.GetValue(sender).ToString();

            PropertyInfo configProperty = sst.GetProperty("Config");
            var config = (ISmartSerializableConfig)configProperty.GetValue(sender);
            //var configValue = configProperty.GetValue(sender);
            //Type configType = configValue.GetType();
            //PropertyInfo activeDiskProperty = configType.GetProperty("ActiveDisk");
            //var activeDisk = activeDiskProperty.GetValue(configValue);

            var local = config.ActiveDisk == ISmartSerializableConfig.ActiveDiskEnum.Local;

            var configurations = (await Configuration);
            _privateConfig = configurations;

            if (!configurations.TryGetValue(name, out var loader)) { return; }

            await UpdateLoaderConfigAsync(config, loader);
            await ChangeDiskCallbackAsync(sender, e, sst, local, name, loader);
        }

        private async Task UpdateLoaderConfigAsync(ISmartSerializableConfig config, SmartSerializableLoader loader)
        {
            // Unwire the event handler that synchronizes base item since action  
            // is generated by the base item itself
            loader.PropertyChanged -= Loader_PropertyChanged;
            loader.Config.CopyChanged(config, true);
            loader.PropertyChanged += Loader_PropertyChanged;
            await WriteConfigurationAsync();
        }

        private async Task ChangeDiskCallbackAsync(object sender, PropertyChangedEventArgs e, Type sst, bool local, string name, SmartSerializableLoader loader)
        {
            if (e.PropertyName.Contains("ActiveDisk"))
            {
                var response = MyBox.ShowDialog($"SpamBayes is now using {(local ? "local" : "network")} disk. Would you like to save the current classifier?",
                            "Save Configuration",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                //var response = MessageBox.Show($"SpamBayes is now using {(local ? "local" : "network")} disk. Would you like to save the current classifier?",
                //            "Save Configuration",
                //            MessageBoxButtons.YesNo,
                //            MessageBoxIcon.Question);

                if (response == DialogResult.Yes)
                {
                    MethodInfo serializeMethod = sst.GetMethod("Serialize", []);
                    serializeMethod.Invoke(sender, null);
                }

                else
                {
                    response = MyBox.ShowDialog($"Would you like to reload the classifier from {(local ? "local" : "network")}", "Reload Classifier",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    //response = MessageBox.Show($"Would you like to reload the classifier from {(local ? "local" : "network")}", "Reload Classifier",
                    //    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (response == DialogResult.Yes)
                    {
                        ResetLoadClassifierAsyncLazy(name, loader);
                        await Globals.Engines.RestartEngineAsync(name);
                    }
                }
            }
        }

        #endregion Configuration

        #region Manager Initialization

        internal AsyncLazy<BayesianClassifierGroup> GetAsyncLazyClassifierLoader(SmartSerializableLoader loader)
        {
            return new AsyncLazy<BayesianClassifierGroup>(async () => 
            {
                //Type outerClassType = loader.T;
                //Type staticClassType = loader.T.GetNestedType("Static", BindingFlags.Static | BindingFlags.Public);
                //if (staticClassType != null)
                //{
                //    // Get the MethodInfo of the static method
                //    MethodInfo staticMethod = staticClassType.GetMethod("DeserializeAsync", BindingFlags.Static | BindingFlags.Public);

                //    if (staticMethod != null)
                //    {
                //        // Make the method generic
                //        MethodInfo genericMethod = staticMethod.MakeGenericMethod(loader.T);

                //        // Invoke the static async method
                //        Task task = (Task)genericMethod.Invoke(null, new object[] { loader, true });
                //        await task.ConfigureAwait(false);

                //        // Get the result of the task
                //        PropertyInfo resultProperty = task.GetType().GetProperty("Result");
                //        var classifier = resultProperty.GetValue(task) as BayesianClassifierGroup;
                //        if (classifier is null) { logger.Error($"{loader.T.Name}.Static.DeserializeAsync returned null."); return null; }
                //        classifier.PropertyChanged += Config_PropertyChanged;
                //        return classifier;
                //    }
                //    else
                //    {
                //        logger.Error($"{loader.T.Name}.Static.DeserializeAsync not found.");
                //        return null;
                //    }
                //}
                //else
                //{
                //    logger.Error($"{loader.T.Name}.Static nested static class was not found.");
                //    return null;
                //}
                
                var classifier = await BayesianClassifierGroup.Static.DeserializeAsync(loader, true, GetAltLoader(loader));
                classifier.PropertyChanged += Config_PropertyChanged;
                return classifier;
            });
        }

        private Func<BayesianClassifierGroup> GetAltLoader(SmartSerializableLoader loader) 
        {
            // Get the MethodInfo of the static method
            MethodInfo staticMethod = null;
            if(loader.T is not null)
            {
                staticMethod = loader.T.GetMethod("CreateNewClassifier", BindingFlags.Static | BindingFlags.Public);
            }
            
            Func<BayesianClassifierGroup> altLoader = staticMethod is null ? null : () => staticMethod.Invoke(null,null) as BayesianClassifierGroup;

            return altLoader;
        }

        public async Task ResetLoadManagerAsyncLazy()
        {
            if (Configuration is null) { ResetConfigAsyncLazy(); }
            foreach (var configuration in await Configuration)
            {
                ResetLoadClassifierAsyncLazy(configuration.Key, configuration.Value);
            }
        }

        public void ResetLoadClassifierAsyncLazy(string name, SmartSerializableLoader loader) 
        {
            if (loader.Config.ClassifierActivated)
            {
                var classifierGroup = GetAsyncLazyClassifierLoader(loader);
                if (classifierGroup != null) { this[name] = classifierGroup; }
            }
            else
            {
                this.TryRemove(name, out _);
            }
        }

        #endregion Manager Initialization

    }
}
