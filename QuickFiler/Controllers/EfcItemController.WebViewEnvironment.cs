using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal partial class EfcItemController
    {
        private IWebViewCoreInitializer _webViewInitializer;

        /// <summary>
        /// Seam over WebView2 environment creation for the EFC item viewer (#792). Not yet
        /// consumed: <see cref="InitializeWebViewAsync"/> is routed through it in Phase 4.
        /// </summary>
        internal IWebViewCoreInitializer WebViewInitializer
        {
            get => _webViewInitializer ??= new WebView2CoreInitializer();
            set => _webViewInitializer = value;
        }

        /// <summary>
        /// The additional browser argument handed to <see cref="CoreWebView2EnvironmentOptions"/>
        /// so that the item preview keeps no browsing data.
        /// </summary>
        /// <remarks>
        /// Hoisted to a constant so the value has exactly one owner and can be asserted directly.
        /// A direct assertion is the only instrument available for it: the enclosing member needs
        /// the real WebView2 runtime, so it cannot be executed under the unit-test policy.
        /// </remarks>
        internal const string IncognitoArgument = "--incognito ";

        internal async Task InitializeWebViewAsync()
        {
            // Create the cache directory
            string localAppData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );
            string cacheFolder = Path.Combine(localAppData, "WindowsFormsWebView2");

            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions(
                IncognitoArgument
            );

            await _itemViewer.UiSyncContext;
            //logger.Debug($"Ui Thread Id: {Thread.CurrentThread.ManagedThreadId}");
            // Create the environment manually
            Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(
                null,
                cacheFolder,
                options
            );

            // Do this so the task is continued on the UI Thread
            TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();

            await task.ContinueWith(
                t =>
                {
                    _webViewEnvironment = task.Result;
                    _itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);
                },
                ui
            );
        }
    }
}
