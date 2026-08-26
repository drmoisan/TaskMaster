using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        private ItemViewer _breadcrumbViewer;

        // #481: the WebResourceRequested subscription is made inside InitializeWebViewAsync, not in a
        // wire method, so the delegate and its source are captured for the matching -= at teardown.
        // Cleanup() nulls _itemViewer, so the source cannot be re-derived after that point.
        private EventHandler<CoreWebView2WebResourceRequestedEventArgs> _webResourceRequestedHandler;
        private CoreWebView2 _coreWebView2;

        // Residual, retained. #230 resolved the pump barrier: the `await _itemViewer.UiSyncContext`
        // on line 55 is now drainable by the WinFormsPumpHost test seam, and tests do reach the
        // IWebViewCoreInitializer seam call. The RESIDUAL barrier is the
        // ((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2 dependency below, which is null
        // unless the real WebView2 runtime initialized the control - an external process barred by
        // the repository unit-test policy. With the mocked IWebViewCoreInitializer execution must
        // stop at the seam call (controlled fault), so the member cannot be meaningfully covered
        // end-to-end and keeps this attribute. The separate concrete-accessor barrier (IItemViewer
        // intentionally exposes no WebView-core-init intent member, so the concrete cast cannot
        // execute against a Mock<IItemViewer>) is tracked separately per issue #230. The SDK
        // dependency itself lives only in the exempt WebView2CoreInitializer adapter.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async Task InitializeWebViewAsync()
        {
            //TraceUtility.LogMethodCall();

            Token.ThrowIfCancellationRequested();

            // Create the cache directory
            string localAppData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );
            string cacheFolder = Path.Combine(localAppData, "WindowsFormsWebView2");

            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
            CoreWebView2EnvironmentOptions options = new("–incognito ");

            // Switch to UI Thread
            await _itemViewer.UiSyncContext;

            // Do this so the task is continued on the UI Thread
            TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();

            // Create the environment manually (WebView2 SDK call isolated behind the injected seam)
            _webViewEnvironment = await _webViewInitializer.CreateEnvironmentAsync(
                cacheFolder,
                options
            );
            await _webViewInitializer.EnsureCoreWebView2Async(
                ((ItemViewer)_itemViewer).L0v2h2_WebView2, // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
                _webViewEnvironment
            );

            // Inline cid: image resolution (issue #326): rewritten cid: references resolve to
            // https://{CidImageResolver.DefaultVirtualHost}/<id> (see MailItemHelper.Html.cs
            // GetHtml()). Intercept those sub-resource requests here and serve the matching
            // attachment's bytes from memory. The attachment map is rebuilt at request time (not at
            // registration time) so it always reflects whichever mail item is currently loaded into
            // this pooled ItemViewer.
            _coreWebView2 = ((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2;
            _coreWebView2.AddWebResourceRequestedFilter(
                $"https://{CidImageResolver.DefaultVirtualHost}/*",
                CoreWebView2WebResourceContext.Image
            );
            // #485 defect 3: ItemHelper is null after Cleanup() and the subscription outlived the
            // controller before #481, so the null-conditional read is load-bearing here.
            _webResourceRequestedHandler = (sender, e) =>
            {
                var map = CidImageResolver.BuildContentIdMap(ItemHelper?.AttachmentsInfo);
                if (!TryResolveCidResource(e.Request.Uri, map, out var payload, out var mimeType))
                {
                    return;
                }

                e.Response = _webViewEnvironment.CreateWebResourceResponse(
                    new MemoryStream(payload),
                    200,
                    "OK",
                    $"Content-Type: {mimeType}"
                );
            };
            _coreWebView2.WebResourceRequested += _webResourceRequestedHandler;

            // #351: initialize the breadcrumb WebView2 through the same injected seam and the
            // same CoreWebView2Environment/options object created above for the message-body
            // pane (G7); no second environment is negotiated against the user-data folder.
            EnsureBreadcrumbPipeline();
            await _webViewInitializer.EnsureCoreWebView2Async(
                ((ItemViewer)_itemViewer).L0vhBreadcrumb_WebView2,
                _webViewEnvironment
            );
            var breadcrumbViewer = (ItemViewer)_itemViewer;
            await ConfigureAndAttachBreadcrumbAsync(
                breadcrumbViewer,
                _webViewEnvironment,
                breadcrumbViewer.AttachBreadcrumbWebViewAsync
            );
            //var task = CoreWebView2Environment.CreateAsync(null, cacheFolder, options);

            //await task.ContinueWith(t =>
            //{
            //    _webViewEnvironment = task.Result;
            //    _itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);
            //}, Token, TaskContinuationOptions.OnlyOnRanToCompletion, ui);
        }

        // #351: idempotently creates the host-neutral breadcrumb pipeline on the concrete viewer
        // so folder population/selection are correct even before WebView2 core init completes.
        // The 9101 provider is DI-resolved from the injected globals' folder-tree service seam —
        // no live Outlook query is issued inside breadcrumb code (G6). Skipped for mock viewers
        // (unit tests drive the coordinator directly through its own seams).
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void EnsureBreadcrumbPipeline()
        {
            if (!(_itemViewer is ItemViewer viewer))
            {
                return;
            }

            if (viewer.BreadcrumbCoordinator == null)
            {
                var provider = new UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider(
                    _globals.Ol.FolderTreeService
                );
                viewer.InitializeBreadcrumbPipeline(provider);
            }

            if (!ReferenceEquals(_breadcrumbViewer, viewer))
            {
                if (_breadcrumbViewer != null)
                {
                    _breadcrumbViewer.BreadcrumbUnhandledArrow -= OnBreadcrumbUnhandledArrow;
                }
                _breadcrumbViewer = viewer;
                _breadcrumbViewer.BreadcrumbUnhandledArrow -= OnBreadcrumbUnhandledArrow;
                _breadcrumbViewer.BreadcrumbUnhandledArrow += OnBreadcrumbUnhandledArrow;
            }
        }

        /// <summary>Configures the lazy popup with the existing environment and active theme.</summary>
        internal void ConfigureBreadcrumbDropDown(
            ItemViewer viewer,
            CoreWebView2Environment environment
        )
        {
            viewer.ConfigureBreadcrumbDropDown(environment, _webViewInitializer);
            viewer.SetBreadcrumbTheme(_globals.Ol.DarkMode ? "dark" : "light");
        }

        /// <summary>Caches the current theme before collapsed navigation can complete.</summary>
        internal Task<bool> ConfigureAndAttachBreadcrumbAsync(
            ItemViewer viewer,
            CoreWebView2Environment environment,
            Func<Task<bool>> attachCollapsed
        )
        {
            if (attachCollapsed == null)
                throw new ArgumentNullException(nameof(attachCollapsed));
            ConfigureBreadcrumbDropDown(viewer, environment);
            return attachCollapsed();
        }

        private void OnBreadcrumbUnhandledArrow(object sender, BreadcrumbArrowDirection direction)
        {
            if (sender is ItemViewer viewer)
            {
                _kbdHandler?.BreadcrumbArrowFallThrough(viewer, direction);
            }
        }

        // Minimal in-memory extension-to-MIME-type lookup for the WebResourceRequested handler
        // above; defaults to a generic octet stream for unrecognized/absent extensions rather than
        // failing the intercepted request.
        private static string ResolveImageMimeType(string fileExtension) =>
            fileExtension?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream",
            };

        /// <summary>
        /// Pure decision half of the <c>WebResourceRequested</c> handler (issue #485): resolves an
        /// intercepted URI to the bytes and MIME type to serve, or returns false to ignore the
        /// request with both <c>out</c> values null. Takes plain values so it is unit-testable
        /// without a WebView2 runtime; the SDK response construction stays in the lambda adapter.
        /// </summary>
        internal static bool TryResolveCidResource(
            string requestedUri,
            IReadOnlyDictionary<string, IAttachment> contentIdMap,
            out byte[] payload,
            out string mimeType
        )
        {
            payload = null;
            mimeType = null;

            // #485 defect 1: the URI is untrusted external input. UriKind must be Absolute, not
            // RelativeOrAbsolute: Uri.Segments throws InvalidOperationException on a relative Uri,
            // which would move the throw one line later rather than removing it.
            if (!Uri.TryCreate(requestedUri, UriKind.Absolute, out var uri))
            {
                logger.Debug($"Ignoring cid: request with unparsable URI '{requestedUri}'.");
                return false;
            }

            var requestedId = uri.Segments.LastOrDefault()?.Trim('/');
            if (string.IsNullOrEmpty(requestedId) || contentIdMap is null)
            {
                return false;
            }

            if (!contentIdMap.TryGetValue(requestedId, out var match) || match is null)
            {
                return false;
            }

            // #485 defect 2: BuildContentIdMap does not filter on AttachmentData, so a map hit does
            // not imply a payload. Logged so this is diagnosable rather than an ArgumentNullException.
            if (match.AttachmentData is null)
            {
                logger.Debug($"Attachment '{requestedId}' has no data payload; skipping.");
                return false;
            }

            payload = match.AttachmentData;
            mimeType = ResolveImageMimeType(match.FileExtension);
            return true;
        }

        // De-exempted cycle-5 (R1): covered by a headless real-ItemViewer test, QfcItemController.ViewerSetupTests.cs.
        internal void ResolveControlGroups(ItemViewer itemViewer)
        {
            //if (itemViewer.InvokeRequired)
            //{
            //    itemViewer.Invoke(() => ResolveControlGroups(itemViewer));
            //    return;
            //}

            var controls = itemViewer.GetAllChildren();

            _listTipsDetails = _itemViewer
                .TipsLabels.Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                .ToList();

            _listTipsExpanded = _itemViewer
                .ExpandedTipsLabels.Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                .ToList();

            _itemPositionTips = new QfcTipsDetails(itemViewer.LblItemNumber);

            var navColNum = _itemPositionTips.ColumnNumber;

            _listTipsDetails.ForEach(x =>
            {
                if (x.ColumnNumber == navColNum)
                {
                    x.IsNavColumn = true;
                }
            });

            _listTipsExpanded.ForEach(x =>
            {
                if (x.ColumnNumber == navColNum)
                {
                    x.IsNavColumn = true;
                }
            });

            _tableLayoutPanels = controls
                .Where(x => x is TableLayoutPanel)
                .Select(x => (TableLayoutPanel)x)
                .ToList();

            Buttons = controls.Where(x => x is Button).Select(x => (Button)x).ToList();
        }

        // #230: de-exempted. The former barrier was the missing WinForms message pump - the member
        // awaits itemViewer.UiSyncContext, which never resumes on a thread-pool MSTest thread. The
        // WinFormsPumpHost test seam supplies that loop, so the member is now covered by
        // QfcItemController_ViewerSetupTests.ResolveControlGroupsAsync_ThroughThePumpHost_*.
        internal async Task ResolveControlGroupsAsync(ItemViewer itemViewer)
        {
            Token.ThrowIfCancellationRequested();

            _itemPositionTips = await QfcTipsDetails.CreateAsync(
                itemViewer.LblItemNumber,
                _itemViewer.UiSyncContext,
                Token
            );
            var navColNum = _itemPositionTips.ColumnNumber;

            await itemViewer.UiSyncContext;
            var controls = itemViewer.GetAllChildren();

            // SelectAwait (System.Linq.Async) is obsolete (CS0618) per the framework's migration
            // guidance ("Use Select... the SelectAwait functionality now exists as overloads of
            // Select"), but migrating to the new overload signature is a call-shape change to
            // production code, not an annotation-only edit. Suppressing narrowly preserves the
            // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
            _listTipsDetails = await _itemViewer
                .TipsLabels.ToAsyncEnumerable()
                .SelectAwait(x => QfcTipsDetails.CreateAsync(x, _itemViewer.UiSyncContext, Token))
                .ToListAsync();

            _listTipsExpanded = await _itemViewer
                .ExpandedTipsLabels.ToAsyncEnumerable()
                .SelectAwait(x => QfcTipsDetails.CreateAsync(x, _itemViewer.UiSyncContext, Token))
                .ToListAsync();
#pragma warning restore CS0618

            _listTipsDetails.ForEach(x =>
            {
                if (x.ColumnNumber == navColNum)
                {
                    x.IsNavColumn = true;
                }
            });

            _listTipsExpanded.ForEach(x =>
            {
                if (x.ColumnNumber == navColNum)
                {
                    x.IsNavColumn = true;
                }
            });

            _tableLayoutPanels = controls
                .Where(x => x is TableLayoutPanel)
                .Select(x => (TableLayoutPanel)x)
                .ToList();

            Buttons = controls.Where(x => x is Button).Select(x => (Button)x).ToList();
        }

        public void PopulateControls(MailItem mailItem, int viewerPosition)
        {
            ItemHelper = new MailItemHelper(mailItem, _globals);
            //_itemInfo.LoadPriority(_globals, _token);
            AssignControls(ItemHelper, viewerPosition);
        }

        public void PopulateControls(MailItemHelper helper, int viewerPosition)
        {
            ItemHelper = helper;
            AssignControls(ItemHelper, viewerPosition);
        }

        internal async Task PopulateControlsAsync(
            MailItem mailItem,
            int viewerPosition,
            bool loadAll
        )
        {
            //TraceUtility.LogMethodCall(mailItem, viewerPosition, loadAll);

            Token.ThrowIfCancellationRequested();

            ItemHelper = await MailItemHelper.FromMailItemAsync(mailItem, _globals, Token, loadAll);

            //AssignControls(ItemHelper, viewerPosition);
            await AssignControlsAsync(ItemHelper, viewerPosition);
        }

        internal async Task AssignControlsAsync(MailItemHelper itemInfo, int viewerPosition)
        {
            //if (_itemViewer.InvokeRequired)
            //{
            //    //await Task.Factory.StartNew(() => AssignControls(itemInfo, viewerPosition), _token, TaskCreationOptions.None, _itemViewer.UiScheduler);
            //    await _itemViewer.UiDispatcher.InvokeAsync(() => AssignControls(itemInfo, viewerPosition));
            //}
            //else
            //{
            //    AssignControls(itemInfo, viewerPosition);
            //}
            await _itemViewer.UiDispatcher.InvokeAsync(() =>
                AssignControls(itemInfo, viewerPosition)
            );
        }

        internal void AssignControls(MailItemHelper itemInfo, int viewerPosition)
        {
            //TraceUtility.LogMethodCall(itemInfo, viewerPosition);
            if (_itemViewer.InvokeRequired)
            {
                _itemViewer.Invoke(() => AssignControls(itemInfo, viewerPosition));
                return;
            }

            _itemViewer.SenderText = itemInfo.SenderName;
            _itemViewer.SubjectText = itemInfo.Subject;
            _itemViewer.BodyText = itemInfo.Body;
            _itemViewer.TriageText = itemInfo.Triage;
            _itemViewer.SentOnText = itemInfo.SentOn;
            _itemViewer.ActionableText = itemInfo.Actionable;
            if (itemInfo.IsTaskFlagSet)
            {
                _itemViewer.FlagTaskDialogResult = DialogResult.OK;
            }
            else
            {
                _itemViewer.FlagTaskDialogResult = DialogResult.Cancel;
            }
            _itemViewer.ItemNumberText = viewerPosition.ToString();

            _optionConversationChecked = _globals.QfSettings.MoveEntireConversation;
            _itemViewer.ConversationModeChecked = _optionConversationChecked;

            _optionEmailCopy = _globals.QfSettings.SaveEmailCopy;
            _itemViewer.EmailCopyChecked = _optionEmailCopy;

            _optionAttachments = _globals.QfSettings.SaveAttachments;
            _itemViewer.AttachmentsChecked = _optionAttachments;

            _optionsPictures = _globals.QfSettings.SavePictures;
            _itemViewer.PicturesChecked = _optionsPictures;
        }

        public void Cleanup()
        {
            // #351: clear the breadcrumb rows/selection before releasing the pooled viewer, in
            // step with the _webViewEnvironment clearing below.
            (_itemViewer as ItemViewer)?.ResetBreadcrumb();
            if (_breadcrumbViewer != null)
            {
                _breadcrumbViewer.BreadcrumbUnhandledArrow -= OnBreadcrumbUnhandledArrow;
                _breadcrumbViewer = null;
            }
            _globals = null;
            _itemViewer = null;
            _parent = null;
            _listTipsDetails = null;
            _mailItem = null;
            //_dfConversation = null;
            _folderHandler = null;
            _webViewEnvironment = null;
            _themes = null;
            _folderHandler = null;
            _tableLayoutPanels = null;
            _explorerController = null;
            //_formController = null;
            _homeController = null;
            _kbdHandler = null;
            _itemPositionTips = null;
            ItemHelper = null;
            _itemViewer = null;
            _emailIsReadTimer = null;
        }

        internal string GetItemSummary() =>
            $"Subject: {ItemHelper.Subject} sent on {ItemHelper.SentDate.ToString("MM/dd/yyyy")} at {ItemHelper.SentDate.ToString("HH:mm")} by {ItemHelper.SenderName}";
    }
}
