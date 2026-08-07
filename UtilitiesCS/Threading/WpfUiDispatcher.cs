#nullable enable
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
    /// against a real, running <see cref="Dispatcher"/> hosted on a dedicated STA
    /// thread, requiring no live WinForms/WPF application host.
    /// </summary>
    public sealed class WpfUiDispatcher : IUiDispatcher
    {
        private readonly Func<Dispatcher> _dispatcherProvider;

        /// <summary>
        /// Initializes a dispatcher adapter backed by the application UI dispatcher.
        /// </summary>
        public WpfUiDispatcher()
            : this(() => UiThread.Dispatcher) { }

        /// <summary>
        /// Initializes a dispatcher adapter for a dedicated STA test dispatcher.
        /// </summary>
        internal WpfUiDispatcher(Dispatcher dispatcher)
            : this(() => dispatcher ?? throw new ArgumentNullException(nameof(dispatcher))) { }

        private WpfUiDispatcher(Func<Dispatcher> dispatcherProvider) =>
            _dispatcherProvider =
                dispatcherProvider ?? throw new ArgumentNullException(nameof(dispatcherProvider));

        private Dispatcher Dispatcher => _dispatcherProvider();

        /// <inheritdoc />
        public void Invoke(Action action) => Dispatcher.Invoke(action);

        /// <inheritdoc />
        public Task InvokeAsync(Action action) => Dispatcher.InvokeAsync(action).Task;

        /// <inheritdoc />
        public Task InvokeAsync(
            Action action,
            DispatcherPriority priority,
            CancellationToken token
        ) => Dispatcher.InvokeAsync(action, priority, token).Task;

        /// <inheritdoc />
        public IAsyncResult BeginInvoke(Action action) => Dispatcher.BeginInvoke(action).Task;

        /// <inheritdoc />
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
            Dispatcher.InvokeAsync(func).Task;

        /// <inheritdoc />
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func) =>
            Dispatcher.InvokeAsync(func).Task.Unwrap();
    }
}
