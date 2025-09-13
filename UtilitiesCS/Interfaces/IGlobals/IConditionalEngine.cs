using System;
using System.ComponentModel;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public interface IConditionalEngine<T>
    {
        Func<T, Task> AsyncAction { get; }
        Func<object, Task<bool>> AsyncCondition { get; }
        object Engine { get; }
        Func<IApplicationGlobals, Task> EngineInitializer { get; }
        string EngineName { get; }
        string Message { get; }
        T TypedItem { get; set; }
        ISmartSerializableConfig Config { get; }
        void Serialize();
    }
}