using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// UI-idle marshalling part of <see cref="QfcQueue"/>. This part exists because the base file
    /// <c>QfcQueue.cs</c> stood at 507 physical lines, already past the repository's 500-line
    /// ceiling, before issue #871 added any injectable seam to it. The whole
    /// <c>Helper Methods</c> region moved here verbatim; no member was renamed, retyped or
    /// re-signed by the split, and the primary constructor and its field initializers stay on the
    /// base part. Issue #871 then routed the three marshalling members through seam S2, moving
    /// their former bodies verbatim into <see cref="UiThreadIdleDispatcher"/> below.
    /// </summary>
    public partial class QfcQueue
    {
        #region Helper Methods

        private IUiIdleDispatcher _uiIdleDispatcher;

        /// <summary>
        /// Issue #871 injectable seam S2 for UI-idle marshalling. The getter is lazy rather than a
        /// field initializer so that constructing a queue performs no read of the process-wide
        /// dispatcher: the default adapter is built on first read, which no headless test triggers.
        /// The default is <see cref="UiThreadIdleDispatcher"/>, which holds the exact bodies these
        /// three members carried before the seam was introduced, so production behaviour is
        /// unchanged. A test assigns a synchronous fake so the enqueue path is reachable without a
        /// live dispatcher. The member is <c>internal</c> because
        /// <see cref="IUiIdleDispatcher"/> is internal and a public member of an internal type is
        /// an inconsistent-accessibility error.
        /// </summary>
        /// <exception cref="ArgumentNullException">The assigned value is null.</exception>
        internal IUiIdleDispatcher UiIdleDispatcher
        {
            get => _uiIdleDispatcher ??= new UiThreadIdleDispatcher();
            set => _uiIdleDispatcher = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal Task UiIdleCallAsync(System.Action action) =>
            UiIdleDispatcher.InvokeIdleAsync(action);

        internal Task<T> UiIdleCallAsync<T>(Func<T> func) =>
            UiIdleDispatcher.InvokeIdleAsync<T>(func);

        internal Task<T> UiIdleAsyncCallAsync<T>(Func<Task<T>> func) =>
            UiIdleDispatcher.InvokeIdleAsync<T>(func);

        #endregion Helper Methods
    }

    /// <summary>
    /// Production implementation of <see cref="IUiIdleDispatcher"/>: marshals onto the process-wide
    /// WPF dispatcher at context-idle priority. Each of the three members holds, verbatim, the body
    /// the correspondingly shaped <see cref="QfcQueue"/> marshalling member carried before issue
    /// #871 introduced seam S2, so the adapter is a relocation rather than a reimplementation. It is
    /// declared in this file rather than in a file of its own because the only other suitable folder
    /// has a space in its name and a path containing a space is dropped by the downstream
    /// change-footprint tooling. Its three bodies cannot be reached without a live process-wide
    /// dispatcher and are recorded as residual uncovered regions.
    /// </summary>
    internal sealed class UiThreadIdleDispatcher : IUiIdleDispatcher
    {
        public async Task InvokeIdleAsync(System.Action action)
        {
            await UiThread.Dispatcher.InvokeAsync(
                action,
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );
        }

        public async Task<T> InvokeIdleAsync<T>(Func<T> func)
        {
            return await UiThread.Dispatcher.InvokeAsync(
                func,
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );
        }

        public async Task<T> InvokeIdleAsync<T>(Func<Task<T>> func)
        {
            T result = await await UiThread.Dispatcher.InvokeAsync(
                async () =>
                {
                    T result = await func();
                    await Task.Yield();
                    return result;
                },
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );
            return result;
            //return await UiThread.Dispatcher.InvokeAsync(func, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }
}
