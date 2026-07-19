using System;
using System.Collections.Generic;
using System.Windows.Forms;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler
{
    // Breadcrumb WinForms glue (#351). The host-neutral seams (BreadcrumbBridgeCoordinator ->
    // FolderBreadcrumbBridgeRouter/BreadcrumbStateModel/BreadcrumbSelectionMap in UtilitiesCS) own all
    // correctness; this partial holds only the WinForms glue (WebView2 property exposure, page
    // load via NavigateToString, focus hand-off, and the pre-init message relay). The whole
    // ItemViewer type is [ExcludeFromCodeCoverage] via its primary partial in ItemViewer.cs
    // (the attribute is not repeated here to avoid duplicate-attribute CS0579 on the partial type).
    public partial class ItemViewer
    {
        private BreadcrumbMessengerRelay _breadcrumbRelay;
        private WebView2Messenger _breadcrumbMessenger;

        /// <summary>The Designer-declared breadcrumb WebView2 occupying the old CboFolders cell.</summary>
        public Microsoft.Web.WebView2.WinForms.WebView2 L0vhBreadcrumb_WebView2
        {
            get => _l0vhBreadcrumb_WebView2;
            set => _l0vhBreadcrumb_WebView2 = value;
        }

        /// <summary>
        /// The breadcrumb coordinator once the pipeline is initialized; null on a bare viewer
        /// (folder intent members are inert no-ops until the controller initializes the pipeline).
        /// </summary>
        internal BreadcrumbBridgeCoordinator BreadcrumbCoordinator { get; private set; }

        /// <summary>
        /// Raised when the breadcrumb reports an arrow it could not consume, so the keyboard
        /// handler can run the legacy fall-through behavior (FR-6).
        /// </summary>
        internal event EventHandler<BreadcrumbArrowDirection> BreadcrumbUnhandledArrow;

        // Multicast backing delegates for the IItemViewer folder events (raised synthetically from
        // the coordinator; add/remove implemented in ItemViewer.FolderSearch.cs).
        private EventHandler _folderSelectionChangedHandlers;
        private KeyEventHandler _folderKeyDownHandlers;

        /// <summary>
        /// Creates the host-neutral breadcrumb pipeline (relay messenger + coordinator) so folder
        /// population and selection are correct even before the WebView2 core init completes.
        /// Idempotent; called by the controller with the DI-resolved 9101 provider (G6).
        /// </summary>
        internal void InitializeBreadcrumbPipeline(IFolderHierarchyProvider provider)
        {
            if (BreadcrumbCoordinator != null)
            {
                return;
            }

            _breadcrumbRelay = new BreadcrumbMessengerRelay();
            BreadcrumbCoordinator = new BreadcrumbBridgeCoordinator(_breadcrumbRelay, provider);
            BreadcrumbCoordinator.SelectionChanged += (s, e) =>
                _folderSelectionChangedHandlers?.Invoke(this, EventArgs.Empty);
            BreadcrumbCoordinator.FolderArrowKeyDown += (s, direction) =>
                _folderKeyDownHandlers?.Invoke(
                    this,
                    new KeyEventArgs(
                        direction == BreadcrumbArrowDirection.Right ? Keys.Right : Keys.Left
                    )
                );
            BreadcrumbCoordinator.UnhandledArrow += (s, direction) =>
                BreadcrumbUnhandledArrow?.Invoke(this, direction);
        }

        /// <summary>
        /// Attaches the initialized breadcrumb CoreWebView2: loads the self-contained page via
        /// NavigateToString (mirroring ItemViewer.WebViewThread.cs) and flushes every buffered
        /// bridge message through the real messenger.
        /// </summary>
        internal void AttachBreadcrumbWebView()
        {
            if (_breadcrumbRelay == null || _breadcrumbMessenger != null)
            {
                return;
            }

            _l0vhBreadcrumb_WebView2.NavigateToString(Properties.Resources.FolderBreadcrumb);
            _breadcrumbMessenger = new WebView2Messenger(_l0vhBreadcrumb_WebView2.CoreWebView2);
            _breadcrumbRelay.Attach(_breadcrumbMessenger);
        }

        /// <summary>
        /// Focus glue for FocusFolderDropDown()/SetFolderDroppedDown(true): keyboard focus lands
        /// in the breadcrumb WebView2 and the page focuses its list container on window focus.
        /// </summary>
        internal void FocusBreadcrumb()
        {
            _l0vhBreadcrumb_WebView2.Focus();
        }

        /// <summary>Clears breadcrumb rows/selection when the pooled viewer is recycled.</summary>
        internal void ResetBreadcrumb()
        {
            BreadcrumbCoordinator?.Clear();
        }

        /// <summary>
        /// Buffering relay implementing the messenger seam before the WebView2 core exists:
        /// outbound JSON is queued and flushed on <see cref="Attach"/>; inbound messages from the
        /// real messenger are forwarded 1:1. Pure glue with no message interpretation.
        /// </summary>
        internal sealed class BreadcrumbMessengerRelay : IWebViewMessenger
        {
            private readonly Queue<string> _pending = new Queue<string>();
            private IWebViewMessenger _real;

            public event EventHandler<string> MessageReceived;

            public void PostJson(string json)
            {
                if (json == null)
                {
                    throw new ArgumentNullException(nameof(json));
                }

                if (_real != null)
                {
                    _real.PostJson(json);
                    return;
                }
                _pending.Enqueue(json);
            }

            public void Attach(IWebViewMessenger real)
            {
                _real = real ?? throw new ArgumentNullException(nameof(real));
                _real.MessageReceived += (s, json) => MessageReceived?.Invoke(this, json);
                while (_pending.Count > 0)
                {
                    _real.PostJson(_pending.Dequeue());
                }
            }
        }
    }
}
