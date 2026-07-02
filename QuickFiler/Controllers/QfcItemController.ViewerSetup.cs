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

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        // Residual (bucket-iii, reclassified in Phase 6): the WebView2 SDK calls are now isolated behind
        // the injected IWebViewCoreInitializer seam (P6-T4), but this method still performs the
        // concrete-bound control access ((ItemViewer)_itemViewer).L0v2h2_WebView2 and awaits
        // _itemViewer.UiSyncContext on the live UI thread. IItemViewer intentionally exposes no
        // WebView-core-init intent member (cycle-1 narrowing retained the raw control here, per
        // IItemViewer.cs), so the concrete cast cannot execute against a Mock<IItemViewer>; the method
        // is not unit-reachable under Option A. The SDK dependency itself lives only in the exempt
        // WebView2CoreInitializer adapter.
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
            //var task = CoreWebView2Environment.CreateAsync(null, cacheFolder, options);

            //await task.ContinueWith(t =>
            //{
            //    _webViewEnvironment = task.Result;
            //    _itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);
            //}, Token, TaskContinuationOptions.OnlyOnRanToCompletion, ui);
        }

        // Residual (bucket-iii): the method's entire purpose is concrete control-tree traversal — it
        // takes a concrete ItemViewer and calls GetAllChildren()/reads Designer controls (P2-T4 seam).
        // Not unit-reachable without a live ItemViewer.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        // Residual (bucket-iii): async control-tree traversal counterpart of ResolveControlGroups;
        // takes a concrete ItemViewer and walks its Designer controls. Not unit-reachable.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

            _listTipsDetails = await _itemViewer
                .TipsLabels.ToAsyncEnumerable()
                .SelectAwait(x => QfcTipsDetails.CreateAsync(x, _itemViewer.UiSyncContext, Token))
                .ToListAsync();

            _listTipsExpanded = await _itemViewer
                .ExpandedTipsLabels.ToAsyncEnumerable()
                .SelectAwait(x => QfcTipsDetails.CreateAsync(x, _itemViewer.UiSyncContext, Token))
                .ToListAsync();

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
