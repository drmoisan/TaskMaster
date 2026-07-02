using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Production adapter (DI-seam "adapter" tier, research §3.3) that forwards every
    /// <see cref="IWebViewCoreInitializer"/> member 1:1 to the WebView2 SDK. The body is a thin
    /// forwarding shim over a third-party API, so it legitimately carries
    /// <see cref="ExcludeFromCodeCoverage"/>; the isolated forwards exist precisely so that
    /// <c>InitializeWebViewAsync</c> becomes routing-testable.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class WebView2CoreInitializer : IWebViewCoreInitializer
    {
        /// <inheritdoc />
        public Task<CoreWebView2Environment> CreateEnvironmentAsync(
            string cacheFolder,
            CoreWebView2EnvironmentOptions options
        ) => CoreWebView2Environment.CreateAsync(null, cacheFolder, options);

        /// <inheritdoc />
        public Task EnsureCoreWebView2Async(
            WebView2 control,
            CoreWebView2Environment environment
        ) => control.EnsureCoreWebView2Async(environment);
    }
}
