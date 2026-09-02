using System;
using System.Threading.Tasks;

namespace QuickFiler.Controllers
{
    internal partial class EfcItemController
    {
        /// <summary>
        /// Issue #726 finding 4: fault-boundary sink for <see cref="InitializeWebViewGuardedAsync"/>,
        /// mirroring <c>QfcItemController.WebViewInitializationErrorSink</c>. Named distinctly so no
        /// shared contract with the QFC sink or with <see cref="EfcFormController.BoundaryErrorSink"/>
        /// is implied.
        /// </summary>
        internal Action<string, Exception> WebViewInitializationErrorSink { get; set; } =
            (message, exception) => logger.Error(message, exception);

        /// <summary>
        /// Issue #726 finding 4: fault boundary for <see cref="InitializeWebViewAsync"/>. Both
        /// production call sites previously discarded the task returned by
        /// <c>Task.Run(() =&gt; InitializeWebViewAsync())</c>, so a fault there was never observed --
        /// under .NET Framework 4.5+, a discarded faulted task is silently finalized with no
        /// diagnostic. This member contains the fault instead of returning it: the task it returns
        /// never transitions to Faulted.
        /// </summary>
        internal async Task InitializeWebViewGuardedAsync()
        {
            try
            {
                await InitializeWebViewAsync();
            }
            catch (OperationCanceledException)
            {
                // Cooperative cancellation during teardown is expected and is not a fault.
            }
            catch (Exception ex)
            {
                // Issue #726 finding 5: guard against a null or throwing sink delegate so a
                // misconfigured sink cannot silently reinstate the unobserved-fault behavior this
                // boundary exists to prevent.
                TryReportWebViewInitializationFault(ex);
            }
        }

        private void TryReportWebViewInitializationFault(Exception ex)
        {
            var sink = WebViewInitializationErrorSink;
            if (sink is null)
            {
                logger.Error("WebView2 initialization failed.", ex);
                return;
            }

            try
            {
                sink("WebView2 initialization failed.", ex);
            }
            catch (Exception sinkException)
            {
                logger.Error(
                    "WebView2 initialization failed, and the error sink itself threw.",
                    sinkException
                );
                logger.Error("Original WebView2 initialization failure.", ex);
            }
        }
    }
}
