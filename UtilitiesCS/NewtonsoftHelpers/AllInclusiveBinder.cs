#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.NewtonsoftHelpers
{
    public class AllInclusiveBinder
    {
        // Deliberate contract decision: this is an unused stub whose body returns null,
        // so the return type is annotated Assembly[]? to reflect the actual null behavior
        // (plain class, no ISerializationBinder constraint to satisfy).
        public Assembly[]? GetAssemblies()
        {
            //var dataAssembly = typeof(AnClassInDataLayer).Assembly;
            //var businessAssembly = typeof(ACLassInBusinessLayer).Assembly;
            //var webApiAssembly = typeof(Startup).Assembly;
            //return new Assembly[] { businessAssembly, dataAssembly, webApiAssembly };
            return null;
        }
    }
}
