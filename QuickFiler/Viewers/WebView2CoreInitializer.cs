using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Production adapter (DI-seam "adapter" tier) over the two WebView2 SDK calls issued during
    /// item-viewer initialization. The seam exists so that the surrounding routing logic becomes
    /// unit-testable against a mock.
    /// </summary>
    /// <remarks>
    /// Coverage exemption rationale. The two SDK calls this type issues cannot be executed in a unit
    /// test, and the ground is a policy prohibition rather than mere difficulty:
    /// <list type="bullet">
    /// <item><description>
    /// Both calls require the external Evergreen WebView2 runtime, which is a separate process and
    /// is therefore prohibited by the external-dependency rule for unit tests.
    /// </description></item>
    /// <item><description>
    /// <c>CreateEnvironmentAsync</c> additionally creates a user-data folder on disk, which the
    /// no-temporary-files rule prohibits.
    /// </description></item>
    /// </list>
    /// This type does not surface the SDK's <c>browserExecutableFolder</c> argument, so the exemption
    /// does not rest on any claim of a mechanical member-for-member forward. The argument guards
    /// added for issue #477 are pure validation with no SDK dependency, so they are a testable seam,
    /// are not exempt, and are measured; only the two extracted SDK forwards carry the attribute.
    /// </remarks>
    public sealed class WebView2CoreInitializer : IWebViewCoreInitializer
    {
        /// <inheritdoc />
        public Task<CoreWebView2Environment> CreateEnvironmentAsync(
            string cacheFolder,
            CoreWebView2EnvironmentOptions options
        )
        {
            if (cacheFolder == null)
            {
                throw new ArgumentNullException(nameof(cacheFolder));
            }

            if (string.IsNullOrWhiteSpace(cacheFolder))
            {
                throw new ArgumentException(
                    "The user-data folder path must not be empty or whitespace.",
                    nameof(cacheFolder)
                );
            }

            // options is forwarded unguarded: whether the SDK tolerates null is unverified, and
            // guarding an unverified contract would narrow behaviour on unmeasured grounds.
            return ForwardCreateEnvironmentAsync(cacheFolder, options);
        }

        /// <summary>The unavoidable SDK call behind <see cref="CreateEnvironmentAsync"/>.</summary>
        /// <remarks>
        /// Exempt from coverage because it requires the external Evergreen WebView2 runtime, a
        /// separate process, and additionally creates a user-data folder on disk; a unit test may do
        /// neither. Extracted so the argument guards stay measured. The SDK's
        /// <c>browserExecutableFolder</c> argument is passed as null unconditionally, which is the
        /// deliberate Evergreen-only decision documented on the interface.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        private static Task<CoreWebView2Environment> ForwardCreateEnvironmentAsync(
            string cacheFolder,
            CoreWebView2EnvironmentOptions options
        )
        {
            return CoreWebView2Environment.CreateAsync(null, cacheFolder, options);
        }

        /// <inheritdoc />
        public Task EnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment environment)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            // environment is deliberately not guarded: null is a valid SDK input meaning "create a
            // default environment".
            return ForwardEnsureCoreWebView2Async(control, environment);
        }

        /// <summary>The unavoidable SDK call behind <see cref="EnsureCoreWebView2Async"/>.</summary>
        /// <remarks>
        /// Exempt from coverage because it requires the external Evergreen WebView2 runtime, a
        /// separate process, which a unit test may not depend on. Extracted so the argument guard
        /// stays measured.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        private static Task ForwardEnsureCoreWebView2Async(
            WebView2 control,
            CoreWebView2Environment environment
        )
        {
            return control.EnsureCoreWebView2Async(environment);
        }
    }
}
