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
    /// base part.
    /// </summary>
    public partial class QfcQueue
    {
        #region Helper Methods

        internal async Task UiIdleCallAsync(System.Action action)
        {
            await UiThread.Dispatcher.InvokeAsync(
                action,
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );
        }

        internal async Task<T> UiIdleCallAsync<T>(Func<T> func)
        {
            return await UiThread.Dispatcher.InvokeAsync(
                func,
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );
        }

        internal async Task<T> UiIdleAsyncCallAsync<T>(Func<Task<T>> func)
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

        #endregion Helper Methods
    }
}
