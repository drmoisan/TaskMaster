#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Fans one logical breadcrumb message stream out to the closed and expanded WebView surfaces
    /// while merging their inbound messages into one coordinator subscription.
    /// </summary>
    public sealed class BreadcrumbMessengerHub : IWebViewMessenger, IDisposable
    {
        private sealed class Attachment
        {
            public Attachment(
                IWebViewMessenger messenger,
                BreadcrumbSelectorViewMode mode,
                EventHandler<string> handler
            )
            {
                Messenger = messenger;
                Mode = mode;
                Handler = handler;
            }

            public IWebViewMessenger Messenger { get; }
            public BreadcrumbSelectorViewMode Mode { get; set; }
            public EventHandler<string> Handler { get; }
        }

        private sealed class CachedState
        {
            public CachedState(long sequence, string json)
            {
                Sequence = sequence;
                Json = json;
            }

            public long Sequence { get; }
            public string Json { get; }
        }

        private readonly object _sync = new object();
        private readonly Dictionary<IWebViewMessenger, Attachment> _attachments =
            new Dictionary<IWebViewMessenger, Attachment>();
        private readonly Dictionary<string, CachedState> _cachedStates = new Dictionary<
            string,
            CachedState
        >(StringComparer.Ordinal);
        private long _sequence;
        private bool _disposed;

        /// <inheritdoc />
        public event EventHandler<string>? MessageReceived;

        /// <summary>
        /// Attaches one page surface in its presentation mode. Reattaching the same messenger is
        /// idempotent and changing its mode does not add a second inbound subscription.
        /// </summary>
        public bool Attach(IWebViewMessenger messenger, BreadcrumbSelectorViewMode mode)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            lock (_sync)
            {
                ThrowIfDisposed();
                if (_attachments.TryGetValue(messenger, out Attachment? existing))
                {
                    if (existing.Mode == mode)
                    {
                        return false;
                    }
                    existing.Mode = mode;
                    ReplayCachedState(existing);
                    return false;
                }

                EventHandler<string> handler = OnSurfaceMessageReceived;
                var attachment = new Attachment(messenger, mode, handler);
                _attachments.Add(messenger, attachment);
                messenger.MessageReceived += handler;
                ReplayCachedState(attachment);
                return true;
            }
        }

        /// <summary>Detaches a page surface and its exact inbound handler.</summary>
        public bool Detach(IWebViewMessenger messenger)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            lock (_sync)
            {
                if (!_attachments.TryGetValue(messenger, out Attachment? attachment))
                {
                    return false;
                }

                messenger.MessageReceived -= attachment.Handler;
                _attachments.Remove(messenger);
                return true;
            }
        }

        /// <inheritdoc />
        public void PostJson(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            lock (_sync)
            {
                ThrowIfDisposed();
                string? type = MessageType(json);
                CacheState(type, json);
                foreach (Attachment attachment in _attachments.Values)
                {
                    PostToSurface(attachment, json, type);
                }
            }
        }

        /// <summary>Detaches every surface so no page callback survives disposal.</summary>
        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                foreach (Attachment attachment in _attachments.Values)
                {
                    attachment.Messenger.MessageReceived -= attachment.Handler;
                }
                _attachments.Clear();
                _cachedStates.Clear();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        private void OnSurfaceMessageReceived(object? sender, string json)
        {
            EventHandler<string>? handler;
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }
                handler = MessageReceived;
            }
            handler?.Invoke(this, json);
        }

        private void ReplayCachedState(Attachment attachment)
        {
            foreach (CachedState state in _cachedStates.Values.OrderBy(value => value.Sequence))
            {
                PostToSurface(attachment, state.Json, MessageType(state.Json));
            }
        }

        private void CacheState(string? type, string json)
        {
            if (type != "render" && type != "themeChange" && type != "selectorView")
            {
                return;
            }

            _cachedStates[type] = new CachedState(++_sequence, json);
        }

        private static void PostToSurface(Attachment attachment, string json, string? type)
        {
            if (type == "selectorView")
            {
                try
                {
                    json = RewriteSelectorMode(json, attachment.Mode);
                }
                catch (FormatException)
                {
                    // Preserve invalid outbound JSON verbatim; validation belongs to its producer.
                }
            }
            attachment.Messenger.PostJson(json);
        }

        private static string RewriteSelectorMode(
            string json,
            BreadcrumbSelectorViewMode targetMode
        )
        {
            var view = (BreadcrumbSelectorViewMessage)
                BreadcrumbSelectorMessageSerializer.Parse(json);
            if (view.Mode == targetMode)
            {
                return json;
            }

            const string marker = "\"mode\"";
            int markerIndex = json.IndexOf(marker, StringComparison.Ordinal);
            int colonIndex = markerIndex < 0 ? -1 : json.IndexOf(':', markerIndex + marker.Length);
            int valueStart = colonIndex < 0 ? -1 : json.IndexOf('"', colonIndex + 1);
            int valueEnd = valueStart < 0 ? -1 : json.IndexOf('"', valueStart + 1);
            if (valueEnd <= valueStart)
            {
                return json;
            }

            string mode =
                targetMode == BreadcrumbSelectorViewMode.Collapsed ? "collapsed" : "expanded";
            return json.Substring(0, valueStart + 1) + mode + json.Substring(valueEnd);
        }

        private static string? MessageType(string json)
        {
            const string marker = "\"type\"";
            int markerIndex = json.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                return null;
            }

            int colonIndex = json.IndexOf(':', markerIndex + marker.Length);
            int valueStart = colonIndex < 0 ? -1 : json.IndexOf('"', colonIndex + 1);
            int valueEnd = valueStart < 0 ? -1 : json.IndexOf('"', valueStart + 1);
            return valueEnd > valueStart
                ? json.Substring(valueStart + 1, valueEnd - valueStart - 1)
                : null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BreadcrumbMessengerHub));
            }
        }
    }
}
