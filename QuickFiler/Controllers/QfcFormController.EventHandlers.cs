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
        #region Event Handlers

        internal void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            if (_formViewer?.UiSyncContext is not null)
            {
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
            }

            _darkMode = _globals?.Ol?.DarkMode ?? _darkMode;
            if (DarkMode)
            {
                ActiveTheme = "DarkNormal";
            }
            else
            {
                ActiveTheme = "LightNormal";
            }
        }

        //private void SetDarkMode()
        //{
        //    _formViewer.L1v1L2h2_ButtonOK.BackColor = System.Drawing.Color.DimGray;
        //    _formViewer.L1v1L2h2_ButtonOK.ForeColor = System.Drawing.Color.WhiteSmoke;
        //    _formViewer.L1v1L2h2_ButtonOK.UseVisualStyleBackColor = false;
        //    _formViewer.L1v1L2h3_ButtonCancel.BackColor = System.Drawing.Color.DimGray;
        //    _formViewer.L1v1L2h3_ButtonCancel.ForeColor = System.Drawing.Color.WhiteSmoke;
        //    _formViewer.L1v1L2h3_ButtonCancel.UseVisualStyleBackColor = false;
        //    _formViewer.L1v1L2h4_ButtonUndo.BackColor = System.Drawing.Color.DimGray;
        //    _formViewer.L1v1L2h4_ButtonUndo.ForeColor = System.Drawing.Color.WhiteSmoke;
        //    _formViewer.L1v1L2h5_SpnEmailPerLoad.BackColor = System.Drawing.Color.DimGray;
        //    _formViewer.L1v1L2h5_SpnEmailPerLoad.ForeColor = System.Drawing.Color.Gainsboro;
        //    _formViewer.BackColor = Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
        //}

        //private void SetLightMode()
        //{
        //    _formViewer.L1v1L2h2_ButtonOK.BackColor = System.Drawing.SystemColors.Control;
        //    _formViewer.L1v1L2h2_ButtonOK.ForeColor = System.Drawing.SystemColors.ControlText;
        //    _formViewer.L1v1L2h2_ButtonOK.UseVisualStyleBackColor = true;
        //    _formViewer.L1v1L2h3_ButtonCancel.BackColor = System.Drawing.SystemColors.Control;
        //    _formViewer.L1v1L2h3_ButtonCancel.ForeColor = System.Drawing.SystemColors.ControlText;
        //    _formViewer.L1v1L2h3_ButtonCancel.UseVisualStyleBackColor = true;
        //    _formViewer.L1v1L2h4_ButtonUndo.BackColor = System.Drawing.SystemColors.Control;
        //    _formViewer.L1v1L2h4_ButtonUndo.ForeColor = System.Drawing.SystemColors.ControlText;
        //    _formViewer.L1v1L2h5_SpnEmailPerLoad.BackColor = System.Drawing.SystemColors.Window;
        //    _formViewer.L1v1L2h5_SpnEmailPerLoad.ForeColor = System.Drawing.SystemColors.WindowText;
        //    _formViewer.BackColor = System.Drawing.SystemColors.ControlLightLight;
        //}

        public async void ButtonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
                await ActionCancelAsync();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task ActionCancelAsync()
        {
            _parent?.TokenSource?.Cancel();
            if (_formViewer?.UiSyncContext is not null)
            {
                await _formViewer.UiSyncContext;
            }
            _formViewer?.Hide();
            _groups?.Cleanup();
            Cleanup();
        }

        public async void ButtonOK_Click(object sender, EventArgs e)
        {
            try
            {
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
                await ActionOkAsync();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task ActionOkAsync()
        {
            //TraceUtility.LogMethodCall();

            if (!_initType.HasFlag(QfEnums.InitTypeEnum.Sort))
            {
                throw new NotImplementedException(
                    $"Method {nameof(QfcFormController)}.{nameof(ActionOkAsync)} has not been "
                        + $"implemented for {nameof(_initType)} {_initType}"
                );
            }
            else if (_groups?.ReadyForMove == true)
            {
                //_blRunningModalCode = true;

                if (_parent.KeyboardHandler.KbdActive)
                {
                    _parent.KeyboardHandler.ToggleKeyboardDialog();
                }

                await MoveAndIterate();

                //_blRunningModalCode = false;
            }
        }

        internal async Task LoadUiFromQueue()
        {
            //TraceUtility.LogMethodCall();

            (var tlp, var itemGroups) = await _qfcQueue.TryDequeueAsync(Token, 4000);
            LoadItems(tlp, itemGroups);
            _parent.SwapStopWatch();
        }

        internal async Task MoveAndIterate()
        {
            //TraceUtility.LogMethodCall();

            if (_qfcQueue is null || _groups is null || _parent is null || _formViewer is null)
            {
                return;
            }

            if ((_qfcQueue.Count + _qfcQueue.JobsRunning) > 0)
            {
                _groups.CacheMoveObjects();
                var moveTask = BackGroundMoveAsync();

                try
                {
                    await LoadUiFromQueue();
                    await _parent.IterateQueueAsync();
                }
                catch (System.Exception e)
                {
                    await moveTask;
                    await _parent.FilerQueue.Consumer;
                    log.Error(e.Message, e);
                    log.Debug("Shutting down QuickFiler");
                    await ActionCancelAsync();
                }

                //var iterate = _parent.IterateQueueAsync();

                await moveTask;
                //await iterate;
            }
            else if (_formViewer.Worker?.IsBusy == true)
            {
                MessageBox.Show(
                    "Still loading emails. Please try again in a few seconds.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            else
            {
                // Either end of email database or error loading queue
                _groups.CacheMoveObjects();
                _parent.SwapStopWatch();
                await BackGroundMoveAsync();
                await _parent.FilerQueue.Consumer;

                // If DataModel is not Complete then an error happened loading the queue
                if (!_parent.DataModel.Complete)
                {
                    // Since most common error is cross-thread error, we will try to load the queue again using the Ui Dispatcher
                    await UiThread.Dispatcher.InvokeAsync(_parent.IterateQueueAsync);
                }
                // We have reached the end of the email database
                else
                {
                    MessageBox.Show(
                        "Finished Moving Emails",
                        "Finished",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    await ActionCancelAsync();
                }
            }
        }

        internal async Task BackGroundMoveAsync()
        {
            //TraceUtility.LogMethodCall();

            if (_groups is null || _globals?.FS?.Filenames is null || WriteMetrics is null)
            {
                return;
            }

            // Move emails
            await _groups.MoveEmailsAsync(_movedItems);

            // Write Move Metrics
            await UiThread.Dispatcher.InvokeAsync(
                async () => await WriteMetrics(_globals.FS.Filenames.EmailSession),
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );

            await UiThread.Dispatcher.InvokeAsync(() => _groups.CleanupBackground());
        }

        public void ButtonUndo_Click(object sender, EventArgs e)
        {
            UndoDialog();
        }

        public void ButtonUndo_Click()
        {
            UndoDialog();
            //SortEmail.Undo(_movedItems, _globals.Ol.App);
        }

        public async Task SpnEmailPerLoadHandler(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            while (!_parent.WorkerComplete)
            {
                await Task.Delay(100);
            }

            var count = (int)_formViewer.ItemsPerLoadValue;
            switch (count)
            {
                case int n when n == _itemsPerIteration:
                    // group actions for count equal to _itemsPerIteration. Do nothing.
                    break;
                case int n when n > _itemsPerIteration:
                    // group actions for count greater than _itemsPerIteration
                    _groups.UnregisterNavigation();
                    await _qfcQueue.ChangeIterationSize(
                        (_formViewer.L1v0L2L3v_TableLayout, _groups.ItemGroups),
                        count,
                        _rowStyleTemplate
                    );
                    _groups.RegisterNavigation();
                    _itemsPerIteration = count;
                    break;
                case int n when n > 0:
                    // group actions for count less than _itemsPerIteration but greater than 0
                    break;
                default:
                    // group actions for count less than or equal to 0
                    // invalid value. maintain current setting.
                    _formViewer.ItemsPerLoadValue = _itemsPerIteration;
                    break;
            }
        }

        public async void SpnEmailPerLoad_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                await SpnEmailPerLoadHandler(sender, e);
            }
            catch (System.Exception ex)
            {
                log.Error("Error in SpnEmailPerLoad_ValueChanged", ex);
            }
        }

        internal void AdjustTlp(TableLayoutPanel tlp, int newCount)
        {
            if (tlp is null || _rowStyleTemplate is null)
            {
                return;
            }

            var oldCount = tlp.RowCount - 1;
            if (oldCount != newCount)
            {
                oldCount = Math.Max(0, oldCount);
                var diff = newCount - oldCount;
                if (diff > 0)
                {
                    tlp.InsertSpecificRow(oldCount, _rowStyleTemplate, diff);
                    tlp.MinimumSize = new System.Drawing.Size(
                        tlp.MinimumSize.Width,
                        tlp.MinimumSize.Height + (int)Math.Round(_rowStyleTemplate.Height * diff, 0)
                    );
                }
                else
                {
                    var removeCount = Math.Abs(diff);
                    tlp.RemoveSpecificRow(newCount, removeCount);
                    tlp.MinimumSize = new System.Drawing.Size(
                        tlp.MinimumSize.Width,
                        tlp.MinimumSize.Height
                            - (int)Math.Round(_rowStyleTemplate.Height * removeCount, 0)
                    );
                }
            }
        }

        public async Task ButtonSkipHandler(object sender, EventArgs e)
        {
            if (_formViewer is null)
            {
                await SkipGroupAsync();
                return;
            }

            _formViewer.SkipButtonEnabled = false;
            _formViewer.SkipButtonText = "Skipping...";
            await SkipGroupAsync();
            _formViewer.SkipButtonText = "Skip Group";
            _formViewer.SkipButtonEnabled = true;
        }

        public async void ButtonSkip_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            try
            {
                await ButtonSkipHandler(sender, e);
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task SkipGroupAsync()
        {
            if (_qfcQueue is null)
            {
                return;
            }

            if ((_qfcQueue.Count + _qfcQueue.JobsRunning) > 0)
            {
                (var tlp, var itemGroups) = await _qfcQueue.TryDequeueAsync(Token, 4000);
                LoadItems(tlp, itemGroups);
                _parent?.SwapStopWatch();
                var iterate = _parent?.IterateQueueAsync();
                _groups?.CleanupBackground();
                if (iterate is not null)
                {
                    await iterate;
                }
            }
            else if (_formViewer?.Worker?.IsBusy == true)
            {
                MessageBox.Show(
                    "Still loading emails. Please try again in a few seconds.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            else
            {
                logger.Info(
                    "Skip requested but queue is exhausted; no additional groups are available."
                );
            }
        }

        #endregion Event Handlers
    }
}
