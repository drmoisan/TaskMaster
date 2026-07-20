#nullable enable
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
        } = null!;

        /// <summary>
        /// The most recent per-resource timing breakdown rendered by
        /// <see cref="ReadConfigurationAsync"/>, exposed for test observability of the diagnostic
        /// instrumentation (Issue #207). Equals the table text emitted to the log4net logger.
        /// <see langword="null"/> until <see cref="ReadConfigurationAsync"/> has run once.
        /// Consumed only within the assembly and its test assembly via InternalsVisibleTo.
        /// </summary>
        internal string? LastResourceTimingBreakdown { get; private set; }

        internal virtual async Task<
            ConcurrentDictionary<string, SmartSerializableLoader>
        > ReadConfigurationAsync()
        {
            // Diagnostic instrumentation (Issue #207): record per-resource deserialization timing
            // to localize the dominant IntelConfig startup cost. This list collects one row per
            // enumerated resource entry. It does not alter deserialization control flow.
            var timingRows = new List<ResourceTimingRow>();

            // Diagnostic instrumentation (Issue #207, increment 2): measure the serialized-payload
            // read (GetSerializedConfigurations) separately from the per-resource deserialize timing
            // so the read-versus-deserialize split is visible in the emitted breakdown. Uses
            // Stopwatch only (no DateTime/clock APIs). The read result is materialized to a list so
            // the read cost is fully captured before deserialization begins; this does not change
            // the enumeration, the entries, or the deserialize control flow below.
            var readStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var serializedConfigurations = GetSerializedConfigurations().ToList();
            readStopwatch.Stop();
            var readElapsedMs = readStopwatch.Elapsed.TotalMilliseconds;
            var readEntryCount = serializedConfigurations.Count;

            // SelectAwait is obsolete (CS0618) per the framework's migration guidance ("Use
            // Select ... overloads of Select"), but the replacement overload requires adding a
            // CancellationToken parameter to the lambda. Suppressing narrowly preserves the
            // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
            var resourceDictionary = await serializedConfigurations
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
                        return new KeyValuePair<string, SmartSerializableLoader>(kvp.Key, null!);
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
#pragma warning restore CS0618

            // Emit the read-versus-deserialize breakdown exactly once as a single consolidated
            // block, consistent in style with the existing [Startup timing] table. The labeled
            // GetSerializedConfigurations read line precedes the per-resource deserialize rows so
            // the split is visible. The same rendered text is retained on
            // LastResourceTimingBreakdown for test observability.
            LastResourceTimingBreakdown = FormatResourceTimingBreakdown(
                readElapsedMs,
                readEntryCount,
                timingRows
            );
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
        /// Renders the read-versus-deserialize timing breakdown (Issue #207, increment 2) as a
        /// single formatted block. A labeled <c>GetSerializedConfigurations</c> read line precedes
        /// the per-resource deserialize table, which is rendered through the same
        /// <see cref="UtilitiesCS.PrettyPrinters.ToFormattedText(string[][], string[], Enums.Justification[], string)"/>
        /// column-alignment helper that <c>StartupTimingRecorder.FormatTable</c> uses, so the
        /// breakdown matches the existing <c>[Startup timing]</c> table style. Pure: builds and
        /// returns a string with no logging or I/O.
        /// </summary>
        /// <param name="readElapsedMs">The Stopwatch-measured elapsed milliseconds of the <c>GetSerializedConfigurations()</c> serialized-payload read.</param>
        /// <param name="readEntryCount">The number of serialized resource entries returned by the read.</param>
        /// <param name="rows">The per-resource deserialize measurements to render, in capture order.</param>
        /// <returns>A block whose first line reports the read measurement, followed by a formatted table with columns Duration (ms), SizeBytes, and ResourceKey.</returns>
        private static string FormatResourceTimingBreakdown(
            double readElapsedMs,
            int readEntryCount,
            IReadOnlyList<ResourceTimingRow> rows
        )
        {
            var readLine = string.Format(
                CultureInfo.InvariantCulture,
                "GetSerializedConfigurations read: durationMs={0}; entries={1}",
                readElapsedMs.ToString("F2", CultureInfo.InvariantCulture),
                readEntryCount.ToString(CultureInfo.InvariantCulture)
            );

            var jagged = rows.Select(r =>
                    new[]
                    {
                        r.ElapsedMs.ToString("F2", CultureInfo.InvariantCulture),
                        r.SizeBytes.ToString(CultureInfo.InvariantCulture),
                        r.ResourceKey,
                    }
                )
                .ToArray();

            var deserializeTable = jagged.ToFormattedText(
                ["Duration", "SizeBytes", "ResourceKey"],
                [Enums.Justification.Right, Enums.Justification.Right, Enums.Justification.Left]
            );

            return $"{readLine}\n{deserializeTable}";
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
            string? serializedLoader
        )
        {
            // SmartSerializableLoader.DeserializeAsync's declared Task<SmartSerializableLoader?>
            // return type is a defensive contract for its own internal TaskCanceledException
            // path; this method's own signature (unchanged, pre-existing) declares the
            // non-nullable Task<SmartSerializableLoader>. Suppressing narrowly preserves the
            // exact pre-existing behavior (no behavior change per AC7) rather than widening this
            // method's own return-type contract.
#pragma warning disable CS8619
            return SmartSerializableLoader.DeserializeAsync(Globals, serializedLoader!);
#pragma warning restore CS8619
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
