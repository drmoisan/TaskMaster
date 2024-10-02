using System;
using System.Threading.Tasks;

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
        
    }
}