using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace UtilitiesCS.Threading
{
    /// <summary>
    /// Narrow UI-dispatch seam (research §3.2) abstracting the static
    /// <see cref="UiThread.Dispatcher"/> so callers can be unit-tested with a mock that executes the
    /// supplied delegate synchronously. Production is served by <see cref="WpfUiDispatcher"/>, which
    /// forwards each member 1:1 to the underlying WPF <see cref="Dispatcher"/>.
    /// </summary>
    public interface IUiDispatcher
    {
        /// <summary>Synchronously executes <paramref name="action"/> on the UI thread.</summary>
        void Invoke(Action action);

        /// <summary>Asynchronously executes <paramref name="action"/> on the UI thread.</summary>
        Task InvokeAsync(Action action);

        /// <summary>
        /// Asynchronously executes <paramref name="action"/> on the UI thread at the supplied
        /// <paramref name="priority"/>, observing <paramref name="token"/>.
        /// </summary>
        Task InvokeAsync(Action action, DispatcherPriority priority, CancellationToken token);

        /// <summary>Posts <paramref name="action"/> to the UI thread without waiting for completion.</summary>
        IAsyncResult BeginInvoke(Action action);

        /// <summary>
        /// Asynchronously executes <paramref name="func"/> on the UI thread and returns its result.
        /// </summary>
        Task<TResult> InvokeAsync<TResult>(Func<TResult> func);
    }
}
