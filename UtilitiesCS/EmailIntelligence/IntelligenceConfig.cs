using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// The most recent per-resource timing breakdown rendered by
        /// <see cref="ReadConfigurationAsync"/>, exposed for test observability of the diagnostic
        /// instrumentation (Issue #207). Equals the table text emitted to the log4net logger.
        /// <see langword="null"/> until <see cref="ReadConfigurationAsync"/> has run once.
        /// Consumed only within the assembly and its test assembly via InternalsVisibleTo.
        /// </summary>
        internal string LastResourceTimingBreakdown { get; private set; }

        internal virtual async Task<
            ConcurrentDictionary<string, SmartSerializableLoader>
        > ReadConfigurationAsync()
        {
            // Diagnostic instrumentation (Issue #207): record per-resource deserialization timing
            // to localize the dominant IntelConfig startup cost. This list collects one row per
            // enumerated resource entry. It does not alter deserialization control flow.
            var timingRows = new List<ResourceTimingRow>();

            var resourceDictionary = await GetSerializedConfigurations()
                .ToAsyncEnumerable()
                .SelectAwait(async kvp =>
                {
                    // Measure only the DeserializeLoaderAsync call per resource entry using
                    // Stopwatch (no DateTime/clock APIs). Payload size is the UTF-8 byte count
                    // of the serialized loader string.
                    var sizeBytes = kvp.Value is null ? 0 : Encoding.UTF8.GetByteCount(kvp.Value);
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var loader = await DeserializeLoaderAsync(kvp.Value);
                    stopwatch.Stop();
                    timingRows.Add(
                        new ResourceTimingRow(
                            kvp.Key,
                            sizeBytes,
                            stopwatch.Elapsed.TotalMilliseconds
                        )
                    );

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

            // Emit the per-resource breakdown exactly once as a single consolidated block,
            // consistent in style with the existing [Startup timing] table. The same rendered
            // text is retained on LastResourceTimingBreakdown for test observability.
            LastResourceTimingBreakdown = FormatResourceTimingBreakdown(timingRows);
            logger.Info($"[IntelConfig timing]\n{LastResourceTimingBreakdown}");

            return resourceDictionary;
        }

        /// <summary>
        /// One per-resource deserialization measurement captured by
        /// <see cref="ReadConfigurationAsync"/>: the resource key, the serialized payload size in
        /// UTF-8 bytes, and the <see cref="System.Diagnostics.Stopwatch"/>-measured elapsed
        /// milliseconds of the <c>DeserializeLoaderAsync</c> call for that resource.
        /// </summary>
        /// <remarks>
        /// Declared as a plain <c>readonly struct</c> rather than a positional <c>record struct</c>
        /// because the positional record's compiler-generated <c>init</c> accessors require
        /// <c>System.Runtime.CompilerServices.IsExternalInit</c>, which is not available on this
        /// .NET Framework target (CS0518). A constructor-initialized struct avoids that dependency.
        /// </remarks>
        private readonly struct ResourceTimingRow
        {
            public ResourceTimingRow(string resourceKey, int sizeBytes, double elapsedMs)
            {
                ResourceKey = resourceKey;
                SizeBytes = sizeBytes;
                ElapsedMs = elapsedMs;
            }

            public string ResourceKey { get; }

            public int SizeBytes { get; }

            public double ElapsedMs { get; }
        }

        /// <summary>
        /// Renders the per-resource timing rows as a single formatted table using the same
        /// <see cref="UtilitiesCS.PrettyPrinters.ToFormattedText(string[][], string[], Enums.Justification[], string)"/>
        /// column-alignment helper that <c>StartupTimingRecorder.FormatTable</c> uses, so the
        /// breakdown matches the existing <c>[Startup timing]</c> table style. Pure: builds and
        /// returns a string with no logging or I/O.
        /// </summary>
        /// <param name="rows">The per-resource measurements to render, in capture order.</param>
        /// <returns>A formatted table string with columns Duration (ms), SizeBytes, and ResourceKey.</returns>
        private static string FormatResourceTimingBreakdown(IReadOnlyList<ResourceTimingRow> rows)
        {
            var jagged = rows.Select(r =>
                    new[]
                    {
                        r.ElapsedMs.ToString("F2", CultureInfo.InvariantCulture),
                        r.SizeBytes.ToString(CultureInfo.InvariantCulture),
                        r.ResourceKey,
                    }
                )
                .ToArray();

            return jagged.ToFormattedText(
                ["Duration", "SizeBytes", "ResourceKey"],
                [Enums.Justification.Right, Enums.Justification.Right, Enums.Justification.Left]
            );
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
