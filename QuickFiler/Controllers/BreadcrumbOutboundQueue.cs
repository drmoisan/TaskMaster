#nullable enable
using System;
using System.Collections.Generic;
using QuickFiler.Viewers;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Buffers outbound breadcrumb bridge payloads while the host reports
    /// <see cref="IBreadcrumbWebHost.IsCoreInitialized"/> false, and flushes them in order when
    /// initialization completes (#349). Event-driven only — no polling, no timers, no delays.
    /// Holds no WebView2 types; the host is reached solely through the
    /// <see cref="IBreadcrumbWebHost"/> seam.
    /// </summary>
    public sealed class BreadcrumbOutboundQueue
    {
        private readonly IBreadcrumbWebHost _host;
        private readonly Queue<string> _pending = new Queue<string>();

        /// <summary>Creates the queue over the host seam.</summary>
        /// <param name="host">The breadcrumb web host. Required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is null.</exception>
        public BreadcrumbOutboundQueue(IBreadcrumbWebHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        /// <summary>Number of payloads currently buffered.</summary>
        public int PendingCount => _pending.Count;

        /// <summary>
        /// Posts the payload immediately when the host core is initialized; otherwise buffers it
        /// for the initialization-completed flush.
        /// </summary>
        /// <param name="json">The serialized outbound message. Required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
        public void PostOrQueue(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (_host.IsCoreInitialized)
            {
                _host.PostMessageJson(json);
            }
            else
            {
                _pending.Enqueue(json);
            }
        }

        /// <summary>
        /// Flushes every buffered payload to the host in enqueue order. Called on
        /// CoreWebView2 initialization completion; idempotent (a duplicate completion with an
        /// empty buffer is a no-op, matching the pooled-viewer re-init lifecycle).
        /// </summary>
        public void OnInitializationCompleted()
        {
            while (_pending.Count > 0)
            {
                _host.PostMessageJson(_pending.Dequeue());
            }
        }
    }
}
