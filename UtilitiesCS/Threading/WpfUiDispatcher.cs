using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace UtilitiesCS.Threading
{
    /// <summary>
    /// Production adapter (DI-seam "adapter" tier, research §3.2) that forwards every
    /// <see cref="IUiDispatcher"/> member 1:1 to the static WPF <see cref="UiThread.Dispatcher"/>.
    /// The isolated one-line forwards exist precisely so that callers routed through
    /// <see cref="IUiDispatcher"/> become unit-testable; the forwarding body itself is exercised
    /// (cycle-3, P9-T7) against a real, running <see cref="Dispatcher"/> hosted on a dedicated STA
    /// thread, requiring no live WinForms/WPF application host.
    /// </summary>
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
