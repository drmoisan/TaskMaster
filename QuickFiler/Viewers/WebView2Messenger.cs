using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Web.WebView2.Core;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Production adapter that forwards every <see cref="IWebViewMessenger"/> member 1:1 to the
    /// WebView2 SDK: <see cref="CoreWebView2.WebMessageReceived"/> is re-raised as
    /// <see cref="MessageReceived"/> (string payload via <c>TryGetWebMessageAsString</c>, falling
    /// back to the raw JSON), and <see cref="PostJson"/> forwards to
    /// <see cref="CoreWebView2.PostWebMessageAsJson"/>. The body is a thin forwarding shim over a
    /// third-party API with no branching logic of its own (matching the
    /// <see cref="WebView2CoreInitializer"/> exempt-forwarder pattern), so it legitimately carries
    /// <see cref="ExcludeFromCodeCoverage"/>; nothing testable is exempted here — all message
    /// handling correctness lives in the host-neutral router/coordinator, which tests drive
    /// through a Moq <see cref="IWebViewMessenger"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class WebView2Messenger : IWebViewMessenger
    {
        private readonly CoreWebView2 _coreWebView;

        /// <summary>
        /// Wraps the initialized <paramref name="coreWebView"/>.
        /// </summary>
        /// <param name="coreWebView">The initialized CoreWebView2. Required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="coreWebView"/> is null.</exception>
        public WebView2Messenger(CoreWebView2 coreWebView)
        {
            _coreWebView = coreWebView ?? throw new ArgumentNullException(nameof(coreWebView));
            _coreWebView.WebMessageReceived += OnWebMessageReceived;
        }

        /// <inheritdoc />
        public event EventHandler<string> MessageReceived;

        /// <inheritdoc />
        public void PostJson(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }
            _coreWebView.PostWebMessageAsJson(json);
        }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string payload;
            try
            {
                payload = e.TryGetWebMessageAsString();
            }
            catch (ArgumentException)
            {
                // The page posts JSON objects (not plain strings); fall back to the raw JSON.
                payload = e.WebMessageAsJson;
            }
            MessageReceived?.Invoke(this, payload ?? e.WebMessageAsJson);
        }
    }
}
