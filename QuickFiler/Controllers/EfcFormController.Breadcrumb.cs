using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    internal partial class EfcFormController
    {
        /// <summary>Fixed number of host initialization attempts before the failure is reported (#792).</summary>
        internal const int BreadcrumbInitializationAttemptLimit = 3;

        /// <summary>Folder-area label text shown when initialization finally fails (#792).</summary>
        internal const string FolderAreaInitializationFailedText =
            "Matched Folders: unavailable (breadcrumb initialization failed)";

        /// <summary>Seam for the host initialization call; null selects the production host (#792).</summary>
        internal Func<Task> BreadcrumbHostInitializer { get; set; }

        // Wiring-only breadcrumb setup (#349): constructs the exempt WebView2 host adapter and the
        // non-exempt router where ConfigureFolderTreeView previously wired the TreeListView, then
        // connects the router's events back to the form. All breadcrumb logic lives in the router.
        private void ConfigureBreadcrumbControl()
        {
            _breadcrumbHost = new WebView2BreadcrumbHost(
                _formViewer.BreadcrumbWebView,
                new WebView2CoreInitializer()
            );
            var provider = new UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider(
                _globals.Ol.FolderTreeService,
                () => _globals.Ol.ArchiveRootPath
            );
            _router = new BreadcrumbBridgeRouter(
                provider,
                _breadcrumbHost,
                new UtilitiesCS.OutlookObjects.Folder.BreadcrumbMessageCodec(),
                new UtilitiesCS.OutlookObjects.Folder.BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(_breadcrumbHost)
            );
            _breadcrumbHost.CoreInitialized += (s, e) => _router.NotifyCoreInitialized();
            _router.FocusSearchRequested += (s, e) => _formViewer?.SearchText.Select();
            _router.ApplyTheme(DarkMode);
            _ = InitializeBreadcrumbHostAsync();
        }

        // Fire-and-forget host initialization with an error boundary (the router queues every
        // outbound payload until CoreWebView2InitializationCompleted fires).
        internal async Task InitializeBreadcrumbHostAsync()
        {
            try
            {
                await _breadcrumbHost.InitializeAsync(_formViewer.UiSyncContext);
            }
            catch (System.Exception ex)
            {
                logger.Error($"Breadcrumb WebView2 initialization failed: {ex.Message}", ex);
            }
        }

        // Presentation only. #465 C (RC9) removed the _folderRows write-back: neither assigns
        // nor reads the field.
        private void BindFolderRows(string[] rows)
        {
            var formViewer = _formViewer;
            if (formViewer == null || _router == null)
            {
                return;
            }

            _ = BindBreadcrumbRowsAsync(rows ?? Array.Empty<string>());
        }

        // Retention plus presentation for the three source paths; retaining here rather than in
        // BindFolderRows is what stops the delete gesture accumulating.
        private void BindSourceFolderRows(string[] rows)
        {
            var formViewer = _formViewer;
            if (formViewer == null || _router == null)
            {
                return;
            }

            _folderRows = rows ?? Array.Empty<string>();
            BindFolderRows(_folderRows);
        }

        // Async bind boundary: joins the feature-324 score projection and delegates to the router.
        internal async Task BindBreadcrumbRowsAsync(string[] rows)
        {
            try
            {
                var scores =
                    _dataModel?.FolderHelper?.Suggestions?.ToScoredArray()
                    ?? Array.Empty<FolderScore>();
                await _router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token);
            }
            catch (OperationCanceledException)
            {
                logger.Debug("Breadcrumb bind canceled.");
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault($"Breadcrumb bind failed: {ex.Message}", ex);
            }
        }
    }
}
