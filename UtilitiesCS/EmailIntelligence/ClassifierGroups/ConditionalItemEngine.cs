using log4net.Repository.Hierarchy;
using System;
using System.Threading.Tasks;
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
            string message)
        {
            Engine = engine;
            EngineName = engineName;
            AsyncCondition = asyncCondition.ThrowIfNull();
            AsyncAction = asyncAction.ThrowIfNull();
            Message = message.ThrowIfNull();
        }

        public Func<object, Task<bool>> AsyncCondition { get; set; }
        public Func<T, Task> AsyncAction { get; set; }
        public string Message { get; set; }
        public object Engine { get; set; }
        public Func<IApplicationGlobals, Task> EngineInitializer { get; set; }
        public string EngineName { get; set; }
        public T TypedItem { get; set; }
        public ISmartSerializableConfig Config { get; set; }
        public System.Action SerializationEngine { get; set; }
        public void Serialize() 
        { 
            if (SerializationEngine is not null)
            {
                SerializationEngine();
            }
        }
    }
}
