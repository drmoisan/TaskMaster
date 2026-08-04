#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Web.WebView2.Core;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Production adapter that forwards every <see cref="IWebViewMessenger"/> member 1:1 to the
    /// WebView2 SDK: <see cref="CoreWebView2.WebMessageReceived"/> is re-raised as
    /// <see cref="MessageReceived"/> (string payload via <c>TryGetWebMessageAsString</c>, falling
    /// back to the raw JSON), and <see cref="PostJson"/> forwards to
    /// <see cref="CoreWebView2.PostWebMessageAsJson"/>. Every SDK request runs inside the captured
    /// UI dispatcher. The body remains a forwarding shim over a third-party API (matching the
    /// <see cref="WebView2CoreInitializer"/> exempt-forwarder pattern), so it carries
    /// <see cref="ExcludeFromCodeCoverage"/>; message handling correctness lives in the
    /// host-neutral router/coordinator and dispatcher seams.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class WebView2Messenger : IWebViewMessenger, IDisposable
    {
        private readonly CoreWebView2 _coreWebView;
        private readonly BreadcrumbUiDispatcher _dispatcher;
        private int _disposeRequested;
        private bool _subscribed;

        /// <summary>
        /// Wraps the initialized <paramref name="coreWebView"/>.
        /// </summary>
        /// <param name="coreWebView">The initialized CoreWebView2. Required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="coreWebView"/> is null.</exception>
        public WebView2Messenger(CoreWebView2 coreWebView)
            : this(coreWebView, CaptureProductionDispatcher(coreWebView)) { }

        internal WebView2Messenger(CoreWebView2 coreWebView, BreadcrumbUiDispatcher dispatcher)
        {
            _coreWebView = coreWebView ?? throw new ArgumentNullException(nameof(coreWebView));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _ = _dispatcher.Dispatch(() =>
            {
                if (IsDisposalRequested())
                {
                    return;
                }
                _coreWebView.WebMessageReceived += OnWebMessageReceived;
                _subscribed = true;
            });
        }

        /// <inheritdoc />
        public event EventHandler<string>? MessageReceived;

        /// <inheritdoc />
        public void PostJson(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }
            ThrowIfDisposed();
            _ = _dispatcher.Dispatch(() =>
            {
                if (!IsDisposalRequested())
                {
                    _coreWebView.PostWebMessageAsJson(json);
                }
            });
        }

        /// <summary>Detaches the SDK event handler on the captured UI boundary.</summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (Interlocked.Exchange(ref _disposeRequested, 1) != 0)
            {
                return;
            }

            _ = _dispatcher.Dispatch(() =>
            {
                try
                {
                    if (_subscribed)
                    {
                        _coreWebView.WebMessageReceived -= OnWebMessageReceived;
                    }
                }
                finally
                {
                    _subscribed = false;
                    MessageReceived = null;
                }
            });
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (IsDisposalRequested())
            {
                return;
            }

            _ = _dispatcher.Dispatch(() =>
            {
                if (IsDisposalRequested())
                {
                    return;
                }

                string payload;
                try
                {
                    payload = e.TryGetWebMessageAsString();
                }
                catch (ArgumentException)
                {
                    // The page posts JSON objects (not plain strings); use the raw JSON.
                    payload = e.WebMessageAsJson;
                }
                MessageReceived?.Invoke(this, payload ?? e.WebMessageAsJson);
            });
        }

        private bool IsDisposalRequested()
        {
            return Volatile.Read(ref _disposeRequested) != 0;
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposalRequested())
            {
                throw new ObjectDisposedException(nameof(WebView2Messenger));
            }
        }

        private static BreadcrumbUiDispatcher CaptureProductionDispatcher(CoreWebView2 coreWebView)
        {
            if (coreWebView == null)
            {
                throw new ArgumentNullException(nameof(coreWebView));
            }
            return BreadcrumbUiDispatcher.CaptureCurrent();
        }
    }
}
