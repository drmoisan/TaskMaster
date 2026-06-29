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
        #region Setup and Disposal

        public void CaptureItemSettings()
        {
            if (
                _formViewer?.L1v0L2L3v_TableLayout?.RowStyles is null
                || _formViewer.L1v0L2L3v_TableLayout.RowStyles.Count < 2
            )
            {
                return;
            }

            _formViewer.Show();
            _rowStyleTemplate = _formViewer.L1v0L2L3v_TableLayout.RowStyles[0];
            _rowStyleExpanded = _formViewer.L1v0L2L3v_TableLayout.RowStyles[1];
            _itemMarginTemplate = _formViewer.ItemViewerTemplateMargin;

            _states = _formViewer.CaptureTlpCellStates();

            if (_states is null)
            {
                _formViewer.Hide();
                return;
            }

            _formViewer.Hide();
        }

        public void RemoveTemplatesAndSetupTlp()
        {
            if (
                _formViewer?.L1v0L2L3v_TableLayout is null
                || _qfcQueue is null
                || _rowStyleTemplate is null
            )
            {
                return;
            }

            //ref TableLayoutPanel tlp = ref _formViewer.L1v0L2L3v_TableLayout;
            TableLayoutHelper.RemoveSpecificRow(_formViewer.L1v0L2L3v_TableLayout, 0, 2);

            var count = ItemsPerIteration;
            //_itemsPerIteration = 1;
            //count = 1;
            _formViewer.L1v0L2L3v_TableLayout.InsertSpecificRow(0, _rowStyleTemplate, count);
            _formViewer.L1v0L2L3v_TableLayout.MinimumSize = new System.Drawing.Size(
                _formViewer.L1v0L2L3v_TableLayout.MinimumSize.Width,
                _formViewer.L1v0L2L3v_TableLayout.MinimumSize.Height
                    + (int)Math.Round(_rowStyleTemplate.Height * count, 0)
            );
            _qfcQueue.TlpTemplate = _formViewer.L1v0L2L3v_TableLayout;
            _qfcQueue.TlpStates = _states;
        }

        public void SetupLightDark()
        {
            if (_formViewer?.Panels is null || _formViewer.Buttons is null || _globals?.Ol is null)
            {
                return;
            }

            _themes = QfcThemeHelper.SetupFormThemes(_formViewer.Panels, _formViewer.Buttons);
            _activeTheme = LoadTheme();
            _globals.Ol.PropertyChanged += DarkMode_CheckedChanged;
        }

        public int SpaceForEmail
        {
            get
            {
                if (
                    _formViewer?.L1v_TableLayout?.RowStyles is null
                    || _formViewer.L1v_TableLayout.RowStyles.Count < 2
                )
                {
                    return 0;
                }

                var outerSize = _formViewer.Size;
                var innerSize = _formViewer.ClientSize;
                var frameSize = outerSize - innerSize;
                var _screen = Screen.PrimaryScreen;
                try
                {
                    _screen = _formViewer.GetScreen() ?? Screen.PrimaryScreen;
                }
                catch
                {
                    _screen = Screen.PrimaryScreen;
                }

                int nonEmailSpace =
                    (int)Math.Round(_formViewer.L1v_TableLayout.RowStyles[1].Height, 0)
                    + frameSize.Height;
                int workingSpace = _screen?.WorkingArea.Height ?? 0;
                return workingSpace - nonEmailSpace;
            }
        }

        private int _itemsPerIteration = -1;
        public int ItemsPerIteration
        {
            get =>
                Initializer.GetOrLoad(
                    ref _itemsPerIteration,
                    (x) => x != -1,
                    LoadItemsPerIteration
                );
            set =>
                Initializer.SetAndSave(
                    ref _itemsPerIteration,
                    value,
                    (x) =>
                        _formViewer.Invoke(
                            new System.Action(() => _formViewer.ItemsPerLoadValue = (decimal)x)
                        )
                );
        }

        public int LoadItemsPerIteration()
        {
            var result = (int)Math.Round(SpaceForEmail / _rowStyleTemplate.Height, 0);
            _formViewer.Invoke(
                new System.Action(() => _formViewer.ItemsPerLoadValue = (decimal)result)
            );
            return result;
        }

        public void RegisterFormEventHandlers()
        {
            if (_formViewer?.Controls is null || _parent?.KeyboardHandler is null)
            {
                return;
            }

            _formViewer.Controls.ForAllControls(
                x =>
                {
                    x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(
                        _parent.KeyboardHandler.KeyboardHandler_PreviewKeyDownAsync
                    );
                    //x.KeyDown += new System.Windows.Forms.KeyEventHandler(_parent.KeyboardHndlr.KeyboardHandler_KeyDown);
                    x.KeyDown += new System.Windows.Forms.KeyEventHandler(
                        _parent.KeyboardHandler.KeyboardHandler_KeyDownAsync
                    );
                },
                _formViewer.GetKeyEventExclusionControls().ToList()
            );

            _formViewer.OkClicked += this.ButtonOK_Click;
            _formViewer.CancelClicked += this.ButtonCancel_Click;
            _formViewer.UndoClicked += this.ButtonUndo_Click;
            _formViewer.ItemsPerLoadValueChanged += this.SpnEmailPerLoad_ValueChanged;
            _formViewer.SkipClicked += this.ButtonSkip_Click;
        }

        public void UnregisterFormEventHandlers()
        {
            if (_formViewer?.Controls is null || _parent?.KeyboardHandler is null)
            {
                return;
            }

            _formViewer.Controls.ForAllControls(
                x =>
                {
                    x.PreviewKeyDown -= new System.Windows.Forms.PreviewKeyDownEventHandler(
                        _parent.KeyboardHandler.KeyboardHandler_PreviewKeyDownAsync
                    );
                    //x.KeyDown += new System.Windows.Forms.KeyEventHandler(_parent.KeyboardHndlr.KeyboardHandler_KeyDown);
                    x.KeyDown -= new System.Windows.Forms.KeyEventHandler(
                        _parent.KeyboardHandler.KeyboardHandler_KeyDownAsync
                    );
                },
                _formViewer.GetKeyEventExclusionControls().ToList()
            );

            _formViewer.OkClicked -= this.ButtonOK_Click;
            _formViewer.CancelClicked -= this.ButtonCancel_Click;
            _formViewer.UndoClicked -= this.ButtonUndo_Click;
            _formViewer.ItemsPerLoadValueChanged -= this.SpnEmailPerLoad_ValueChanged;
            _formViewer.SkipClicked -= this.ButtonSkip_Click;
        }

        /// <summary>
        /// Release all resources and call the parent cleanup
        /// </summary>
        public void Cleanup()
        {
            if (_globals?.Ol is not null)
            {
                _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged;
            }

            UnregisterFormEventHandlers();
            _undoQueue?.Dispose();
            _globals = null;
            _formViewer?.Dispose();
            _formViewer = null;
            _groups = null;
            _rowStyleTemplate = null;
            _parent = null;
            _movedItems = null;
            WriteMetrics = null;
            Iterate = null;
            _parentCleanup?.Invoke();
            _parentCleanup = null;
        }

        #endregion
    }
}
