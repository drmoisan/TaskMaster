using System;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Narrow post-init WebView2 messaging seam for the breadcrumb bridge (#351 FR-6): inbound JSON
    /// messages from the page surface through <see cref="MessageReceived"/> and outbound JSON is
    /// posted via <see cref="PostJson"/>. All bridge correctness lives behind this seam in
    /// host-neutral, unit-tested types; production is served by <see cref="WebView2Messenger"/>,
    /// a 1:1 forwarder over <c>CoreWebView2.WebMessageReceived</c>/<c>PostWebMessageAsJson</c>,
    /// and tests supply a Moq mock.
    /// </summary>
    public interface IWebViewMessenger
    {
        /// <summary>
        /// Raised once per JSON message received from the page. The payload is the raw JSON string
        /// of one breadcrumb bridge message.
        /// </summary>
        event EventHandler<string> MessageReceived;

        /// <summary>
        /// Posts one JSON message to the page.
        /// </summary>
        /// <param name="json">The serialized bridge message. Must be non-null.</param>
        void PostJson(string json);
    }
}
