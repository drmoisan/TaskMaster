using System;
using System.Threading.Tasks;

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        /// <summary>
        /// #670 fault-boundary sink: an injectable seam over the static log4net logger declared at
        /// QfcItemController.cs:30. Named distinctly from EfcFormController.BoundaryErrorSink so no
        /// shared contract between the two types is implied.
        /// </summary>
        internal System.Action<
            string,
            System.Exception
        > WebViewInitializationErrorSink { get; set; } =
            (message, exception) => logger.Error(message, exception);

        /// <summary>
        /// #670 fault boundary for InitializeWebViewAsync. Three production call sites discard the
        /// returned task, so a fault there is never observed. This member contains the fault instead
        /// of returning it: the task it returns never transitions to Faulted.
        /// </summary>
        internal async Task InitializeWebViewGuardedAsync()
        {
            try
            {
                await InitializeWebViewAsync();
            }
            catch (OperationCanceledException)
            {
                // Cooperative cancellation during QuickFiler teardown is expected and is not a
                // fault: InitializeWebViewAsync opens with Token.ThrowIfCancellationRequested().
            }
            catch (Exception ex)
            {
                WebViewInitializationErrorSink("WebView2 initialization failed.", ex);
            }
        }
    }
}
