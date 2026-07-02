using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace UtilitiesCS.Threading
{
    /// <summary>
    /// Production adapter (DI-seam "adapter" tier, research §3.2) that forwards every
    /// <see cref="IUiDispatcher"/> member 1:1 to the static WPF <see cref="UiThread.Dispatcher"/>.
    /// The body is a thin forwarding shim over a third-party/static API, so it legitimately carries
    /// <see cref="ExcludeFromCodeCoverage"/>; the isolated one-line forwards exist precisely so that
    /// callers routed through <see cref="IUiDispatcher"/> become unit-testable.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class WpfUiDispatcher : IUiDispatcher
    {
        /// <inheritdoc />
        public void Invoke(Action action) => UiThread.Dispatcher.Invoke(action);

        /// <inheritdoc />
        public Task InvokeAsync(Action action) => UiThread.Dispatcher.InvokeAsync(action).Task;

        /// <inheritdoc />
        public Task InvokeAsync(
            Action action,
            DispatcherPriority priority,
            CancellationToken token
        ) => UiThread.Dispatcher.InvokeAsync(action, priority, token).Task;

        /// <inheritdoc />
        public IAsyncResult BeginInvoke(Action action) =>
            UiThread.Dispatcher.BeginInvoke(action).Task;

        /// <inheritdoc />
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
            UiThread.Dispatcher.InvokeAsync(func).Task;
    }
}
