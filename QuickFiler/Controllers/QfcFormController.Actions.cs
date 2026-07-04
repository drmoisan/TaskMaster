using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Extensions;
using UtilitiesCS.Interfaces.IWinForm;

namespace QuickFiler.Controllers
{
    internal partial class QfcFormController
    {
        #region Major Actions

        public void LoadItems(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups)
        {
            if (_groups is null || tlp is null || itemGroups is null)
            {
                return;
            }

            _groups.LoadControlsAndHandlers_01(tlp, itemGroups);
        }

        public void LoadItems(IList<MailItem> listObjects)
        {
            if (
                listObjects is null
                || _globals is null
                || _formViewer is null
                || _parent is null
                || _tokenSource is null
                || _states is null
            )
            {
                return;
            }

            _helperTasks = listObjects
                .Select(x => MailItemHelper.FromMailItemAsync(x, _globals, Token, false))
                .ToList();
            _groups = new QfcCollectionController(
                AppGlobals: _globals,
                viewerInstance: _formViewer,
                InitType: QfEnums.InitTypeEnum.Sort,
                homeController: _parent,
                parent: this,
                tokenSource: TokenSource,
                token: Token,
                _states
            );
            _groups.LoadControlsAndHandlers_01(listObjects, _rowStyleTemplate, _rowStyleExpanded);
        }

        public async Task LoadItemsAsync(IList<MailItem> listObjects)
        {
            await LoadItemsAsync(listObjects, null);
        }

        public async Task LoadItemsAsync(IList<MailItem> listObjects, ProgressTracker progress)
        {
            if (
                listObjects is null
                || _globals is null
                || _formViewer is null
                || _parent is null
                || _tokenSource is null
                || _states is null
            )
            {
                return;
            }

            Token.ThrowIfCancellationRequested();

            _groups = new QfcCollectionController(
                AppGlobals: _globals,
                viewerInstance: _formViewer,
                InitType: QfEnums.InitTypeEnum.Sort,
                homeController: _parent,
                parent: this,
                tokenSource: TokenSource,
                token: Token,
                _states
            );
            await _groups.LoadControlsAndHandlers_01Async(
                listObjects,
                _rowStyleTemplate,
                _rowStyleExpanded
            );
            progress?.Report(100);

            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            _formViewer.Show();
            _formViewer.Refresh();

            await _groups.LoadSecondaryAsync();
        }

        /// <summary>
        /// Dormant high-confidence (Issue #171) carrier-list load path. Constructs UI item controllers only
        /// for the already-filtered survivors carried in <paramref name="preScored"/>, each with its
        /// predetermined folder. This path does not invoke a post-UI removal pass because the
        /// below-threshold items were removed before UI construction. Issue #233 enforces
        /// confidence at dequeue time instead.
        /// </summary>
        public async Task LoadItemsAsync(IList<QfcPreScoredItem> preScored)
        {
            await LoadItemsAsync(preScored, null);
        }

        /// <inheritdoc cref="LoadItemsAsync(IList{QfcPreScoredItem})"/>
        public async Task LoadItemsAsync(
            IList<QfcPreScoredItem> preScored,
            ProgressTracker progress
        )
        {
            if (
                preScored is null
                || _globals is null
                || _formViewer is null
                || _parent is null
                || _tokenSource is null
                || _states is null
            )
            {
                return;
            }

            Token.ThrowIfCancellationRequested();

            _groups = new QfcCollectionController(
                AppGlobals: _globals,
                viewerInstance: _formViewer,
                InitType: QfEnums.InitTypeEnum.Sort,
                homeController: _parent,
                parent: this,
                tokenSource: TokenSource,
                token: Token,
                _states
            );
            await _groups.LoadControlsAndHandlers_01Async(
                preScored,
                _rowStyleTemplate,
                _rowStyleExpanded
            );
            progress?.Report(100);

            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            _formViewer.Show();
            _formViewer.Refresh();

            await _groups.LoadSecondaryAsync();

            // The issue #171 carrier-list path remains dormant; issue #233 uses dequeue-time
            // enforcement before items reach the UI.
        }

        /// <summary>
        /// Dormant issue #171 post-display threshold helper. Issue #233 does not call this method
        /// for live high-confidence enforcement; live filtering occurs in the datamodel dequeue
        /// layer before items are surfaced.
        /// </summary>
        internal async Task ApplyHighConfidenceFilterAsync(IQfcCollectionController groups)
        {
            if (groups is null || _globals?.QfSettings is null)
            {
                return;
            }

            if (_globals.QfSettings.HighConfidenceModeEnabled)
            {
                await groups.RemoveBelowThresholdAsync(_globals.QfSettings.HighConfidenceThreshold);
            }
        }

        /// <summary>
        /// Maximizes the QfcFormViewer
        /// </summary>
        public void MaximizeFormViewer()
        {
            _formViewer.Invoke(
                new System.Action(() => _formViewer.WindowState = FormWindowState.Maximized)
            );
        }

        /// <summary>
        /// Minimizes the QfcFormViewer
        /// </summary>
        public void MinimizeFormViewer()
        {
            _formViewer.Invoke(
                new System.Action(() => _formViewer.WindowState = FormWindowState.Minimized)
            );
        }

        internal void UndoDialog()
        {
            if (_movedItems is null || _globals?.Ol?.App is null)
            {
                return;
            }

            _undoConsumerTask ??= Task.Run(UndoConsumer);
            var olApp = _globals.Ol.App;
            DialogResult repeatResponse = DialogResult.Yes;
            var i = 0;

            while (i < _movedItems.Count && repeatResponse == DialogResult.Yes)
            {
                var message = _movedItems[i].UndoMoveMessage(olApp);
                if (message is null)
                {
                    i++;
                }
                else
                {
                    var undoResponse = MessageBox.Show(
                        message,
                        "Undo Dialog",
                        MessageBoxButtons.YesNo
                    );
                    if (undoResponse == DialogResult.Yes)
                    {
                        _undoQueue.Add(_movedItems.Pop(i));
                    }
                    else
                    {
                        i++;
                    }
                    repeatResponse = MessageBox.Show(
                        "Continue Undoing Moves?",
                        "Undo Dialog",
                        MessageBoxButtons.YesNo
                    );
                }
            }

            if (repeatResponse == DialogResult.Yes)
            {
                MessageBox.Show("Nothing to undo");
            }
            _movedItems.Serialize();
        }

        internal async Task UndoConsumer()
        {
            var sw = new Stopwatch();
            sw.Start();
            bool exit = false;
            while (!_undoQueue.IsCompleted || exit)
            {
                if (_undoQueue.TryTake(out var item))
                {
                    var helper = await MailItemHelper.FromMailItemAsync(
                        item.MailItem,
                        _globals,
                        default,
                        true
                    );
                    (await _globals.AF.Manager["Folder"]).UnTrain(
                        helper.FolderInfo.RelativePath,
                        helper.Tokens,
                        1
                    );
                    var mail = item.UndoMove();
                    await UiThread.Dispatcher.InvokeAsync(
                        () => _groups.AddItemGroup(mail),
                        System.Windows.Threading.DispatcherPriority.ContextIdle
                    );
                }
                else if (sw.ElapsedMilliseconds > 10000)
                {
                    exit = true;
                }
                else
                {
                    await Task.Delay(200);
                }
            }
            if (exit)
            {
                _undoConsumerTask = null;
            }
        }

        // TODO: Implement Viewer_Activate
        public void Viewer_Activate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
