//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;

//namespace ToDoModel.Data_Model.People
//{
//    public class PeopleScoRemainingObjectConverter : JsonConverter
//    {        
//        public override bool CanConvert(Type objectType)
//        {
//            return objectType == typeof(object);
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            JObject jObject = JObject.Load(reader);
//            return jObject.ToObject<PeopleScoRemainingObject>(serializer);
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            serializer.Serialize(writer, value);
//        }
//    }
//}
