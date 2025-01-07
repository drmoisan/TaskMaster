using UtilitiesCS.ReusableTypeClasses;
using Newtonsoft.Json;
using System;
using UtilitiesCS.Properties;
using System.IO;
using System.Text;
using System.Runtime;

namespace UtilitiesCS.NewtonsoftHelpers.Sco
{
    public class ScoDictionaryConverter<TDerived, TKey, TValue> : JsonConverter<TDerived> where TDerived : ScoDictionaryNew<TKey, TValue>
    {
        public ScoDictionaryConverter() { }

        public override TDerived ReadJson(JsonReader reader, Type typeToConvert, TDerived existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var wrapper = serializer.Deserialize(reader, typeof(WrapperScoDictionary<TDerived, TKey, TValue>)) as WrapperScoDictionary<TDerived, TKey, TValue>;
            return wrapper?.ToDerived();            
        }
                
        public override void WriteJson(JsonWriter writer, TDerived value, JsonSerializer serializer)
        {
            var wrapper = new WrapperScoDictionary<TDerived, TKey, TValue>().ToComposition(value);
            serializer.Serialize(writer, wrapper);
        }

        public override bool CanRead => base.CanRead;


    }
}
