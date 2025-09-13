using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using UtilitiesCS.Extensions;
using System.Collections.Concurrent;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.NewtonsoftHelpers
{
    public class ScDictionaryConverter<TDerived, TKey, TValue>: JsonConverter<TDerived> where TDerived : ScDictionary<TKey, TValue>
    {
        public ScDictionaryConverter() { }

        public override TDerived ReadJson(JsonReader reader, Type typeToConvert, TDerived existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var wrapper = serializer.Deserialize(reader, typeof(WrapperScDictionary<TDerived, TKey, TValue>)) as WrapperScDictionary<TDerived, TKey, TValue>;
            return wrapper?.ToDerived();
        }

        public override void WriteJson(JsonWriter writer, TDerived value, JsonSerializer serializer)
        {
            var wrapper = new WrapperScDictionary<TDerived, TKey, TValue>().ToComposition(value);
            serializer.Serialize(writer, wrapper);
        }
    }
}
