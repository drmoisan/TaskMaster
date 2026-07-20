#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace UtilitiesCS.NewtonsoftHelpers
{
    public class KnownTypesBinder : ISerializationBinder
    {
        // Caller-populated public setter; nullable is the honest annotation (it is not
        // initialized by this type).
        public IList<Type>? KnownTypes { get; set; }

        public Type BindToType(string? assemblyName, string typeName)
        {
            // ISerializationBinder.BindToType declares a NON-null Type return, so the
            // contract cannot become Type? (that would be CS8766). The body returns
            // SingleOrDefault(...) which is null on no match; ! preserves the existing
            // runtime behavior (Newtonsoft tolerates a null return via default binding).
            return KnownTypes!.SingleOrDefault(t => t.Name == typeName)!;
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}
