using System;
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
        /// Seam over WebView2 environment creation for the EFC item viewer (#792).
        /// <see cref="InitializeWebViewAsync"/> is routed through it so a test can observe the
        /// values handed to the SDK without starting a browser process.
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
        /// The owner of this value is <see cref="WebView2EnvironmentContract"/> (#792); this alias
        /// exists so the #463 pin
        /// <c>EfcItemControllerTests.IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace</c>
        /// keeps asserting the value the preview actually passes.
        /// </remarks>
        internal const string IncognitoArgument =
            WebView2EnvironmentContract.AdditionalBrowserArguments;

        internal async Task InitializeWebViewAsync()
        {
            string cacheFolder = WebView2EnvironmentContract.ResolveUserDataFolder();
            CoreWebView2EnvironmentOptions options = WebView2EnvironmentContract.CreateOptions();

            await _itemViewer.UiSyncContext;

            // Both seam calls are awaited (no detached continuation) so a failure reaches
            // InitializeWebViewGuardedAsync instead of being lost off the awaited path (#792).
            _webViewEnvironment = await WebViewInitializer.CreateEnvironmentAsync(
                cacheFolder,
                options
            );
            await WebViewInitializer.EnsureCoreWebView2Async(
                _itemViewer.L0v2h2_WebView2,
                _webViewEnvironment
            );
        }
    }
}
