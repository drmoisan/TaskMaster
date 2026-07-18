#nullable enable
using System;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Narrow host seam over the breadcrumb WebView2 control (#349). Implemented by the
    /// coverage-exempt <c>WebView2BreadcrumbHost</c> adapter and mocked in router tests, so the
    /// non-exempt bridge router never touches WebView2 types directly.
    /// </summary>
    public interface IBreadcrumbWebHost
    {
        /// <summary>Delivers a full generated HTML document to the hosted control.</summary>
        /// <param name="html">The complete document markup.</param>
        void NavigateToString(string html);

        /// <summary>Posts an outbound bridge payload as JSON to the hosted document.</summary>
        /// <param name="json">The serialized outbound message.</param>
        void PostMessageJson(string json);

        /// <summary>Raised with the raw JSON payload of each inbound web message.</summary>
        event EventHandler<string> MessageReceived;

        /// <summary>True once CoreWebView2 initialization has completed.</summary>
        bool IsCoreInitialized { get; }
    }
}
