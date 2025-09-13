using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class DoNotSerializeContractResolver : DefaultContractResolver
    {
        private string[] _propertyNames;

        public DoNotSerializeContractResolver(params string[] propertyNames)
        {
            _propertyNames = propertyNames;   
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // only serializer properties that start with the specified character
            properties = properties.Where(
                p => !_propertyNames.Contains(p.PropertyName))
                .ToList();

            return properties;
        }
    }
}
