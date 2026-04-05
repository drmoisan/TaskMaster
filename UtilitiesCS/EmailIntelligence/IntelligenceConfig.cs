using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Common;
using ToDoModel.Data_Model.People;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence
{
    internal interface IIntelligenceConfigResourceWriter : IDisposable
    {
        void AddResource(string name, string value);

        void Generate();
    }

    internal sealed class IntelligenceConfigResourceWriter : IIntelligenceConfigResourceWriter
    {
        private readonly ResXResourceWriter _writer;

        public IntelligenceConfigResourceWriter(string filePath)
        {
            _writer = new ResXResourceWriter(filePath);
        }

        public void AddResource(string name, string value) => _writer.AddResource(name, value);

        public void Generate() => _writer.Generate();

        public void Dispose() => _writer.Dispose();
    }

    public class IntelligenceConfig(IApplicationGlobals globals)
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

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

        public virtual ConcurrentDictionary<string, SmartSerializableLoader> Config
        {
            get;
            protected set;
        }

        internal virtual async Task<
            ConcurrentDictionary<string, SmartSerializableLoader>
        > ReadConfigurationAsync()
        {
            var resourceDictionary = await GetSerializedConfigurations()
                .ToAsyncEnumerable()
                .SelectAwait(async kvp =>
                {
                    var loader = await DeserializeLoaderAsync(kvp.Value);
                    if (loader is null)
                    {
                        logger.Error(
                            $"Error in {nameof(ReadConfigurationAsync)}. Loader for {kvp.Key} is null"
                        );
                        return new KeyValuePair<string, SmartSerializableLoader>(kvp.Key, null);
                    }
                    if (loader.T is not null)
                    {
                        if (loader.T == typeof(PeopleScoDictionaryNew))
                        {
                            loader.Config.JsonSettings.Converters.Add(new PeopleScoConverter());
                        }
                        else if (IsDerivedFromScoDictionaryNew(loader.T))
                        {
                            loader.Config.JsonSettings.Converters.Add(
                                new NewtonsoftHelpers.Sco.ScoDictionaryConverter()
                            );
                        }
                    }

                    loader.PropertyChanged += Loader_PropertyChanged;
                    return new KeyValuePair<string, SmartSerializableLoader>(kvp.Key, loader);
                })
                .Where(kvp => kvp.Value is not null)
                .ToConcurrentDictionaryAsync();

            return resourceDictionary;
        }

        internal virtual IDictionary<string, string> GetSerializedConfigurations()
        {
            var resourceManager = IntelligenceResources.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(
                System.Globalization.CultureInfo.CurrentCulture,
                true,
                true
            );
            return resourceSet.Cast<DictionaryEntry>().ToDictionary<string, string>();
        }

        internal virtual Task<SmartSerializableLoader> DeserializeLoaderAsync(
            string serializedLoader
        )
        {
            return SmartSerializableLoader.DeserializeAsync(Globals, serializedLoader);
        }

        internal void Loader_PropertyChanged(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e
        )
        {
            if (e.PropertyName.Contains(nameof(SmartSerializableLoader.Config.ClassifierActivated)))
            {
                var loader = (SmartSerializableLoader)sender;

                WriteConfiguration();
            }
        }

        internal virtual void WriteConfiguration()
        {
            string assemblyDirectory = Path.GetDirectoryName(
                typeof(IntelligenceResources).Assembly.Location
            );
            string resxFilePath = Path.Combine(assemblyDirectory, "IntelligenceResources.resx");

            var configurations = Config
                .Select(kvp => new KeyValuePair<string, string>(
                    kvp.Key,
                    kvp.Value.SerializeToString()
                ))
                .ToDictionary();

            using (var resxWriter = CreateResourceWriter(resxFilePath))
            {
                foreach (var configuration in configurations)
                {
                    resxWriter.AddResource(configuration.Key, configuration.Value);
                }
                resxWriter.Generate();
            }
        }

        internal virtual IIntelligenceConfigResourceWriter CreateResourceWriter(
            string resourceFilePath
        )
        {
            return new IntelligenceConfigResourceWriter(resourceFilePath);
        }

        private static bool IsDerivedFromScoDictionaryNew(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type baseType = typeof(ScoDictionaryNew<,>);
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == baseType)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }
    }
}
