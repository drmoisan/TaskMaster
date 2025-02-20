//using UtilitiesCS.ReusableTypeClasses;
//using Newtonsoft.Json;
//using System;
//using UtilitiesCS.Properties;
//using System.IO;
//using System.Text;
//using System.Runtime;

//namespace ToDoModel.Data_Model.People
//{
//    public class PeopleScoConverter : JsonConverter<PeopleScoDictionaryNew> 
//    {
//        public PeopleScoConverter() { }

//        public override PeopleScoDictionaryNew ReadJson(JsonReader reader, Type typeToConvert, PeopleScoDictionaryNew existingValue, bool hasExistingValue, JsonSerializer serializer)
//        {
//            var wrapper = serializer.Deserialize(reader, typeof(WrapperPeopleScoDictionaryNew)) as WrapperPeopleScoDictionaryNew;
//            return wrapper?.ToDerived();
//        }

//        public override void WriteJson(JsonWriter writer, PeopleScoDictionaryNew value, JsonSerializer serializer)
//        {
//            var wrapper = new WrapperPeopleScoDictionaryNew().ToComposition(value);
//            serializer.Serialize(writer, wrapper);
//        }

//        public override bool CanRead => base.CanRead;

//    }

//    //public class ScoDictionaryConverter : JsonConverter
//    //{
//    //    public ScoDictionaryConverter() : base() { }

//    //    public override bool CanConvert(Type objectType)
//    //    {
//    //        return objectType.IsDerivedFrom_ScoDictionaryNew();
//    //        //return objectType.IsGenericType && (objectType.GetGenericTypeDefinition() == typeof(ScoDictionaryNew<,>) || objectType.GetGenericTypeDefinition() == typeof(WrapperScoDictionary<,,>));
//    //    }

//    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//    //    {
//    //        // Create the generic type WrapperScoDictionary<TDerived, TKey, TValue> at runtime
//    //        //Type[] genericArguments = objectType.GetScoDictionaryNewGenerics();
//    //        //Type wrapperType = typeof(WrapperScoDictionary<,,>).MakeGenericType(objectType, genericArguments[0], genericArguments[1]);
//    //        Type wrapperType = typeof(WrapperPeopleScoDictionaryNew);
//    //        var wrapper = serializer.Deserialize(reader, wrapperType);
//    //        return wrapperType.GetMethod("ToDerived", [])?.Invoke(wrapper, null);

//    //    }

//    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//    //    {
//    //        //Type valueType = value.GetType();
//    //        //Type[] genericArguments = valueType.GetScoDictionaryNewGenerics();            
//    //        //Type wrapperType = typeof(WrapperScoDictionary<,,>).MakeGenericType(valueType, genericArguments[0], genericArguments[1]);
//    //        Type wrapperType = typeof(WrapperPeopleScoDictionaryNew);
//    //        var wrapper = Activator.CreateInstance(wrapperType);
//    //        var toComposition = wrapperType.GetMethod("ToComposition");
//    //        toComposition?.Invoke(wrapper, [value]);
//    //        serializer.Serialize(writer, wrapper, wrapperType);

//    //        //var wrapper2 = new WrapperScoDictionary<ScoDictionaryNew<string,string>,string,string> ();

//    //    }
//    //}
//}
