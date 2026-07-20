#nullable enable
using System;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public class ConditionalItemEngine<T> : IConditionalEngine<T>
    {
        public ConditionalItemEngine() { }

        public ConditionalItemEngine(
            object engine,
            string engineName,
            Func<object, Task<bool>> asyncCondition,
            Func<T, Task> asyncAction,
            string message
        )
        {
            Engine = engine;
            EngineName = engineName;
            AsyncCondition = asyncCondition.ThrowIfNull();
            AsyncAction = asyncAction.ThrowIfNull();
            Message = message.ThrowIfNull();
        }

        // Populated by the functional constructor or by a builder; the parameterless constructor
        // (deserialization) leaves them unset. null! preserves the non-null posture used by callers.
        public Func<object, Task<bool>> AsyncCondition { get; set; } = null!;
        public Func<T, Task> AsyncAction { get; set; } = null!;
        public string Message { get; set; } = null!;
        public object Engine { get; set; } = null!;
        public Func<IApplicationGlobals, Task> EngineInitializer { get; set; } = null!;
        public string EngineName { get; set; } = null!;
        public T TypedItem { get; set; } = default!;
        public ISmartSerializableConfig Config { get; set; } = null!;
        public System.Action SerializationEngine { get; set; } = null!;

        public void Serialize()
        {
            if (SerializationEngine is not null)
            {
                SerializationEngine();
            }
        }
    }
}
