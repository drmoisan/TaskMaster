#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.NewtonsoftHelpers
{
    public class ScDictionaryConverter<TDerived, TKey, TValue> : JsonConverter<TDerived>
        where TDerived : ScDictionary<TKey, TValue>
    {
        public ScDictionaryConverter() { }

        // Registered cross-module contract: ReadJson returns TDerived? (body is wrapper?.ToDerived()).
        public override TDerived? ReadJson(
            JsonReader reader,
            Type typeToConvert,
            TDerived? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            var wrapper =
                serializer.Deserialize(reader, typeof(WrapperScDictionary<TDerived, TKey, TValue>))
                as WrapperScDictionary<TDerived, TKey, TValue>;
            return wrapper?.ToDerived();
        }

        public override void WriteJson(
            JsonWriter writer,
            TDerived? value,
            JsonSerializer serializer
        )
        {
            // value! preserves behavior: Newtonsoft invokes WriteJson with a non-null value for
            // a registered converter; ToComposition requires a non-null instance.
            var wrapper = new WrapperScDictionary<TDerived, TKey, TValue>().ToComposition(value!);
            serializer.Serialize(writer, wrapper);
        }
    }
}
