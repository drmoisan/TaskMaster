using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Narrow WebView2 core-init seam abstracting the two third-party WebView2 SDK calls issued
    /// during item-viewer initialization so the surrounding logic can be unit-tested with a mock.
    /// Production is served by <see cref="WebView2CoreInitializer"/>.
    /// </summary>
    /// <remarks>
    /// The concrete implementation is deliberately not a mechanical member-for-member forward of the
    /// SDK surface: it narrows <c>CoreWebView2Environment.CreateAsync</c> by not surfacing that
    /// method's <c>browserExecutableFolder</c> argument. See
    /// <see cref="CreateEnvironmentAsync(string, CoreWebView2EnvironmentOptions)"/> for what that
    /// costs and what changing it would require.
    /// </remarks>
    public interface IWebViewCoreInitializer
    {
        /// <summary>
        /// Creates a <see cref="CoreWebView2Environment"/> backed by the supplied
        /// <paramref name="cacheFolder"/> and <paramref name="options"/>.
        /// </summary>
        /// <param name="cacheFolder">
        /// The user-data folder the environment writes to. Must be non-null and must not be empty or
        /// whitespace.
        /// </param>
        /// <param name="options">
        /// Environment options, forwarded to the SDK unguarded because the SDK's null tolerance for
        /// this argument is unverified. Both in-repo callers supply a non-null value.
        /// </param>
        /// <returns>The created environment.</returns>
        /// <remarks>
        /// The SDK's <c>browserExecutableFolder</c> argument is passed as <c>null</c>
        /// unconditionally, and this interface does not surface it. That is a deliberate
        /// Evergreen-only decision: every caller of this seam is pinned to the Evergreen WebView2
        /// runtime. Selecting a fixed-version WebView2 distribution therefore requires a change to
        /// this contract, not merely to the implementation, and would additionally have to take in
        /// the SDK call sites that bypass this seam.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cacheFolder"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="cacheFolder"/> is empty or consists only of whitespace.
        /// </exception>
        Task<CoreWebView2Environment> CreateEnvironmentAsync(
            string cacheFolder,
            CoreWebView2EnvironmentOptions options
        );

        /// <summary>
        /// Ensures the CoreWebView2 of <paramref name="control"/> is initialized using
        /// <paramref name="environment"/>.
        /// </summary>
        /// <param name="control">The control whose CoreWebView2 is initialized. Must be non-null.</param>
        /// <param name="environment">
        /// The environment to initialize against, or null to let the SDK create a default
        /// environment. Null is a valid input and is not guarded.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="control"/> is null.</exception>
        Task EnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment environment);
    }
}
