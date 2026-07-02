using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Narrow WebView2 core-init seam (research §3.3) abstracting the two third-party WebView2 SDK
    /// calls issued during item-viewer initialization so the surrounding logic can be unit-tested with
    /// a mock. Production is served by <see cref="WebView2CoreInitializer"/>, which forwards 1:1 to the
    /// WebView2 SDK.
    /// </summary>
    public interface IWebViewCoreInitializer
    {
        /// <summary>
        /// Creates a <see cref="CoreWebView2Environment"/> backed by the supplied
        /// <paramref name="cacheFolder"/> and <paramref name="options"/>.
        /// </summary>
        Task<CoreWebView2Environment> CreateEnvironmentAsync(
            string cacheFolder,
            CoreWebView2EnvironmentOptions options
        );

        /// <summary>
        /// Ensures the CoreWebView2 of <paramref name="control"/> is initialized using
        /// <paramref name="environment"/>.
        /// </summary>
        Task EnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment environment);
    }
}
