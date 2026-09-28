#nullable enable
using System;
using System.Threading.Tasks;

namespace QuickFiler.Interfaces
{
    /// <summary>
    /// Issue #871 seam abstraction for the three UI-idle marshalling shapes the QuickFiler queue
    /// uses. A narrow interface is introduced here rather than reusing the existing dispatcher
    /// abstraction because that abstraction expresses no priority for two of the three shapes and
    /// its adapter forwards them at the framework default, which would silently promote two call
    /// sites and change when background page construction runs. The production implementation
    /// marshals onto the process-wide dispatcher at context-idle priority; a test supplies a
    /// synchronous fake so the enqueue path is reachable in a headless test host.
    /// </summary>
    internal interface IUiIdleDispatcher
    {
        /// <summary>
        /// Marshals <paramref name="action"/> and awaits its completion.
        /// </summary>
        Task InvokeIdleAsync(Action action);

        /// <summary>
        /// Marshals <paramref name="func"/> and awaits the value it returns.
        /// </summary>
        Task<T> InvokeIdleAsync<T>(Func<T> func);

        /// <summary>
        /// Marshals <paramref name="func"/>, awaits the task it returns, and yields its result. The
        /// parameter is a function returning a task rather than a plain function so the marshalled
        /// work can itself be asynchronous without the caller double-awaiting at the call site.
        /// </summary>
        Task<T> InvokeIdleAsync<T>(Func<Task<T>> func);
    }
}
