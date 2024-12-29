using UtilitiesCS.ReusableTypeClasses;
using Newtonsoft.Json;
using System;

namespace UtilitiesCS.NewtonsoftHelpers.Sco
{
    public class ScoDictionaryConverter<TDerived, TKey, TValue> : JsonConverter<TDerived> where TDerived : ScoDictionaryNew<TKey, TValue>
    {
        public ScoDictionaryConverter() { }

        public override TDerived ReadJson(JsonReader reader, Type typeToConvert, TDerived existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            WrapperScoDictionary<TDerived, TKey, TValue> dictionary = serializer.Deserialize(reader) as WrapperScoDictionary<TDerived, TKey, TValue>;            
            return dictionary?.ToDerived();
            
        }

        public override void WriteJson(JsonWriter writer, TDerived value, JsonSerializer serializer)
        {
            var wrapper = new WrapperScoDictionary<TDerived, TKey, TValue>().ToComposition(value);
            serializer.Serialize(writer, wrapper);
        }

        public override bool CanRead => base.CanRead;


    }
}
