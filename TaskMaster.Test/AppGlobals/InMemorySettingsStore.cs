using System.Collections.Generic;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Per-test, dictionary-backed <see cref="ISettingsStore"/>. Values are seeded through the
    /// indexer, <see cref="Save"/> only counts calls, and nothing is read from or written to the
    /// per-host settings file. Reading an unseeded key throws so a missing arrangement is explicit.
    /// </summary>
    internal sealed class InMemorySettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>Gets the number of <see cref="Save"/> calls made on this store.</summary>
        public int SaveCount { get; private set; }

        /// <inheritdoc />
        /// <exception cref="KeyNotFoundException">The key has not been seeded.</exception>
        public object this[string propertyName]
        {
            get
            {
                if (!_values.TryGetValue(propertyName, out var value))
                {
                    throw new KeyNotFoundException(
                        $"The setting '{propertyName}' was not seeded in the in-memory store."
                    );
                }

                return value;
            }
            set => _values[propertyName] = value;
        }

        /// <inheritdoc />
        public void Save() => SaveCount++;
    }
}
