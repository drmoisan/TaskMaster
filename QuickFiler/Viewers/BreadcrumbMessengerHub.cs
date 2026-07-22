#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
            public BreadcrumbSelectorViewMode Mode { get; }
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
        /// a no-op that preserves its original mode, subscription, and replay state.
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
                if (_attachments.ContainsKey(messenger))
                {
                    return false;
                }

                EventHandler<string> handler = OnSurfaceMessageReceived;
                var attachment = new Attachment(messenger, mode, handler);
                _attachments.Add(messenger, attachment);
                try
                {
                    messenger.MessageReceived += handler;
                    ReplayCachedState(attachment);
                    return true;
                }
                catch
                {
                    _attachments.Remove(messenger);
                    SafeUnsubscribe(attachment);
                    throw;
                }
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

                _attachments.Remove(messenger);
                SafeUnsubscribe(attachment);
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

                _disposed = true;
                foreach (Attachment attachment in _attachments.Values)
                    SafeUnsubscribe(attachment);
                _attachments.Clear();
                _cachedStates.Clear();
            }
            GC.SuppressFinalize(this);
        }

        private void OnSurfaceMessageReceived(object? sender, string json)
        {
            EventHandler<string>? handler;
            lock (_sync)
            {
                if (
                    _disposed
                    || !(sender is IWebViewMessenger messenger)
                    || !_attachments.ContainsKey(messenger)
                )
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

        private static void SafeUnsubscribe(Attachment attachment)
        {
            try
            {
                attachment.Messenger.MessageReceived -= attachment.Handler;
            }
            catch (Exception exception)
            {
                log4net
                    .LogManager.GetLogger(typeof(BreadcrumbMessengerHub))
                    .Error("Breadcrumb surface detachment failed.", exception);
            }
        }
    }

    /// <summary>Attaches one collapsed candidate only after its exact navigation is ready.</summary>
    internal sealed class BreadcrumbCollapsedAttachment : IDisposable
    {
        private readonly BreadcrumbMessengerHub _hub;
        private readonly BreadcrumbCollapsedSurfaceController _controller;
        private IWebViewMessenger? _pendingMessenger;
        private BreadcrumbNavigationReadiness? _pendingReadiness;
        private Task<bool>? _pendingAttachment;
        private IWebViewMessenger? _readyMessenger;
        private long _generation;
        private bool _disposed;

        internal BreadcrumbCollapsedAttachment(
            BreadcrumbMessengerHub hub,
            BreadcrumbCollapsedSurfaceController controller
        )
        {
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        internal Task<bool> AttachAsync(
            Func<Tuple<IWebViewMessenger, BreadcrumbNavigationReadiness>> candidateFactory
        )
        {
            if (candidateFactory == null)
                throw new ArgumentNullException(nameof(candidateFactory));

            ThrowIfDisposed();
            if (_readyMessenger != null)
                return Task.FromResult(true);
            Task<bool>? pending = _pendingAttachment;
            if (pending?.IsCompleted == false)
                return pending;

            long generation = ++_generation;
            TaskCompletionSource<bool> completion = NewCompletionSource();
            _pendingAttachment = completion.Task;
            IWebViewMessenger? messenger = null;
            BreadcrumbNavigationReadiness? readiness = null;
            try
            {
                Tuple<IWebViewMessenger, BreadcrumbNavigationReadiness>? candidate =
                    candidateFactory();
                messenger = candidate?.Item1;
                readiness = candidate?.Item2;
                if (messenger == null || readiness == null)
                    throw new InvalidOperationException(
                        "Collapsed attachment did not provide a messenger and readiness lease."
                    );
                if (_disposed || generation != _generation)
                {
                    readiness.Dispose();
                    (messenger as IDisposable)?.Dispose();
                    completion.TrySetResult(false);
                    return completion.Task;
                }

                _pendingMessenger = messenger;
                _pendingReadiness = readiness;
                _ = CompleteAsync(messenger, readiness, generation, completion);
            }
            catch (Exception exception)
            {
                readiness?.Dispose();
                (messenger as IDisposable)?.Dispose();
                if (generation == _generation)
                    _pendingAttachment = null;
                completion.TrySetException(exception);
            }
            return completion.Task;
        }

        internal void Reset() => Release(dispose: false);

        public void Dispose()
        {
            Release(dispose: true);
            GC.SuppressFinalize(this);
        }

        private async Task CompleteAsync(
            IWebViewMessenger messenger,
            BreadcrumbNavigationReadiness readiness,
            long generation,
            TaskCompletionSource<bool> completion
        )
        {
            bool attached = false;
            try
            {
                // Preserve the ItemViewer synchronization context for the hub subscription/replay.
                bool ready = await _controller.AttachAsync(messenger, readiness);
                if (
                    ready
                    && IsCurrent(generation, messenger)
                    && ReferenceEquals(_controller.ReadyMessenger, messenger)
                )
                {
                    _hub.Attach(messenger, BreadcrumbSelectorViewMode.Collapsed);
                    _readyMessenger = messenger;
                    attached = true;
                }
            }
            catch (Exception exception)
            {
                if (IsCurrent(generation, messenger))
                    _controller.Reset();
                completion.TrySetException(exception);
                return;
            }
            finally
            {
                if (IsCurrent(generation, messenger))
                {
                    _pendingMessenger = null;
                    _pendingReadiness = null;
                    _pendingAttachment = null;
                }
            }
            completion.TrySetResult(attached);
        }

        private void Release(bool dispose)
        {
            IWebViewMessenger? ready;
            if (_disposed)
                return;
            _disposed = dispose;
            _generation++;
            ready = _readyMessenger;
            _readyMessenger = null;
            _pendingMessenger = null;
            _pendingReadiness = null;
            _pendingAttachment = null;

            if (ready != null)
                _hub.Detach(ready);
            if (dispose)
                _controller.Dispose();
            else
                _controller.Reset();
        }

        private bool IsCurrent(long generation, IWebViewMessenger messenger) =>
            !_disposed
            && generation == _generation
            && ReferenceEquals(_pendingMessenger, messenger);

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BreadcrumbCollapsedAttachment));
        }

        private static TaskCompletionSource<bool> NewCompletionSource() =>
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    /// <summary>Runs the ItemViewer breadcrumb cleanup from its component lifetime.</summary>
    internal sealed class BreadcrumbResourceOwner : Component
    {
        private Action? _dispose;

        internal BreadcrumbResourceOwner(Action dispose)
        {
            _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Action? dispose = _dispose;
                _dispose = null;
                dispose?.Invoke();
            }
            base.Dispose(disposing);
        }
    }
}
