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

        /// <summary>
        /// Issue #448. Clock seam for <see cref="UndoConsumer"/>. Defaults to
        /// <see cref="System.TimeProvider.System"/> so production behaviour is unchanged; tests
        /// assign a <c>FakeTimeProvider</c> so the ten-second idle threshold can be driven without
        /// a real wall-clock wait, which `.claude/rules/general-unit-test.md` requires.
        /// </summary>
        internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

        /// <summary>
        /// Issue #448. Start seam for the undo consumer. Defaults to <c>Task.Run</c> so production
        /// behaviour is unchanged; tests assign <c>body =&gt; body()</c> to run the consumer inline
        /// and observe its completion deterministically.
        /// </summary>
        internal Func<Func<Task>, Task> UndoConsumerStarter { get; set; } = body => Task.Run(body);

        private Func<IMovedMailInfo, Task> _undoItemProcessor;

        /// <summary>
        /// Issue #448. Per-item seam for the undo consumer's successful-take branch. Defaults to
        /// <see cref="ProcessUndoItemAsync"/>, which holds that branch verbatim, so production
        /// behaviour is byte-for-byte unchanged. The default is resolved lazily rather than in a
        /// property initializer because an instance initializer cannot reference an instance method
        /// (CS0236). Tests assign a fake so no live Outlook COM call and no WinForms dispatcher
        /// call is made, which `.claude/rules/general-unit-test.md` UT4 prohibits in unit tests.
        /// </summary>
        internal Func<IMovedMailInfo, Task> UndoItemProcessor
        {
            get => _undoItemProcessor ??= ProcessUndoItemAsync;
            set => _undoItemProcessor = value;
        }

        /// <summary>
        /// The undo consumer's successful-take branch, extracted verbatim so it can be replaced
        /// wholesale by a test double. Untrains the folder classifier on the moved item, moves the
        /// mail back, and re-adds it to the on-screen group on the UI thread.
        /// </summary>
        private async Task ProcessUndoItemAsync(IMovedMailInfo item)
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

        internal void UndoDialog()
        {
            if (_movedItems is null || _globals?.Ol?.App is null)
            {
                return;
            }

            _undoConsumerTask ??= UndoConsumerStarter(UndoConsumer);
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

        /// <summary>
        /// Issue #448. How long the undo consumer stays alive with nothing to take before it exits.
        /// Preserves the previous ten-second threshold; the change is that it now measures idle time
        /// rather than total session time.
        /// </summary>
        private static readonly TimeSpan UndoConsumerIdleTimeout = TimeSpan.FromSeconds(10);

        internal async Task UndoConsumer()
        {
            long start = TimeProvider.GetTimestamp();
            try
            {
                while (!_undoQueue.IsCompleted)
                {
                    if (_undoQueue.TryTake(out var item))
                    {
                        await UndoItemProcessor(item).ConfigureAwait(false);

                        // Reset on every successful take so the threshold measures idle time. The
                        // previous code started one stopwatch for the whole session, so a consumer
                        // busy for ten seconds exited while items were still arriving.
                        start = TimeProvider.GetTimestamp();
                    }
                    else if (TimeProvider.GetElapsedTime(start) > UndoConsumerIdleTimeout)
                    {
                        break;
                    }
                    else
                    {
                        await TimeProvider
                            .Delay(TimeSpan.FromMilliseconds(200))
                            .ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                // Unconditional so a later UndoDialog() starts a fresh consumer even when this one
                // exited by exception, which disposing _undoQueue mid-take can produce.
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
