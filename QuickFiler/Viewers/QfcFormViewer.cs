using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler
{
    [ExcludeFromCodeCoverage]
    public partial class QfcFormViewer : Form, IQfcFormViewer
    {
        public QfcFormViewer()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //this.KeyPreview = true;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );
        private IFilerFormController _formController;
        private IQfcKeyboardHandler _keyboardHandler;

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext
        {
            get => _context;
        }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler
        {
            get => _uiScheduler;
        }

        public virtual void SetController(IFilerFormController controller)
        {
            _formController = controller;
        }

        public virtual void SetKeyboardHandler(IQfcKeyboardHandler keyboardHandler)
        {
            _keyboardHandler = keyboardHandler;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (
                (_keyboardHandler is not null)
                && Controllers.QfcFormKeyHandler.IsAltKeyCommand(keyData)
            )
            {
                SynchronizationContext.SetSynchronizationContext(UiSyncContext);
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                //_keyboardHandler.ToggleKeyboardDialog(sender, e);
                e.Handled = true;
                _ = _keyboardHandler.ToggleKeyboardDialogAsync();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private List<Control> _panels;
        public virtual List<Control> Panels => Initializer.GetOrLoad(ref _panels, LoadPanels);

        private List<Control> LoadPanels()
        {
            var panels = new List<Control>
            {
                this._l1v_TableLayout,
                this.L1v1L2h_TableLayout,
                this._l1v0L2L3v_TableLayout,
                this._l1v0L2_PanelMain,
            };
            return panels;
        }

        private List<Control> _buttons;
        public virtual List<Control> Buttons => Initializer.GetOrLoad(ref _buttons, LoadButtons);

        private List<Control> LoadButtons()
        {
            var buttons = new List<Control>
            {
                this._l1v1L2h2_ButtonOK,
                this._l1v1L2h3_ButtonCancel,
                this._l1v1L2h4_ButtonUndo,
                this.ButtonFilters,
                this._l1v1L2h5_BtnSkip,
            };
            return buttons;
        }

        #region IQfcFormViewer

        public BackgroundWorker Worker => WorkerInternal;

        // Seam C — get-only TLP property over the private backing field; swap goes through SwapItemTableLayout
        public TableLayoutPanel L1v0L2L3v_TableLayout => _l1v0L2L3v_TableLayout;
        public TableLayoutPanel L1v_TableLayout => _l1v_TableLayout;
        public Panel L1v0L2_PanelMain => _l1v0L2_PanelMain;

        /// <summary>
        /// Replaces the active item TableLayoutPanel displayed in the main panel: removes the
        /// current TLP from the panel controls, re-parents the new TLP, and makes it visible.
        /// </summary>
        public void SwapItemTableLayout(TableLayoutPanel newTlp)
        {
            _l1v0L2_PanelMain.Controls.Remove(_l1v0L2L3v_TableLayout);
            _l1v0L2L3v_TableLayout = newTlp;
            _l1v0L2L3v_TableLayout.Parent = _l1v0L2_PanelMain;
            _l1v0L2L3v_TableLayout.Visible = true;
        }

        // Seam B — intent command events forwarded to the backing Designer controls
        public event EventHandler OkClicked
        {
            add => _l1v1L2h2_ButtonOK.Click += value;
            remove => _l1v1L2h2_ButtonOK.Click -= value;
        }
        public event EventHandler CancelClicked
        {
            add => _l1v1L2h3_ButtonCancel.Click += value;
            remove => _l1v1L2h3_ButtonCancel.Click -= value;
        }
        public event EventHandler UndoClicked
        {
            add => _l1v1L2h4_ButtonUndo.Click += value;
            remove => _l1v1L2h4_ButtonUndo.Click -= value;
        }
        public event EventHandler SkipClicked
        {
            add => _l1v1L2h5_BtnSkip.Click += value;
            remove => _l1v1L2h5_BtnSkip.Click -= value;
        }

        // Seam B — skip button state
        public string SkipButtonText
        {
            get => _l1v1L2h5_BtnSkip.Text;
            set => _l1v1L2h5_BtnSkip.Text = value;
        }
        public bool SkipButtonEnabled
        {
            get => _l1v1L2h5_BtnSkip.Enabled;
            set => _l1v1L2h5_BtnSkip.Enabled = value;
        }

        // Seam B — items-per-load spinner state/event
        public decimal ItemsPerLoadValue
        {
            get => _l1v1L2h5_SpnEmailPerLoad.Value;
            set => _l1v1L2h5_SpnEmailPerLoad.Value = value;
        }
        public event EventHandler ItemsPerLoadValueChanged
        {
            add => _l1v1L2h5_SpnEmailPerLoad.ValueChanged += value;
            remove => _l1v1L2h5_SpnEmailPerLoad.ValueChanged -= value;
        }
        public bool ItemsPerLoadEnabled
        {
            get => _l1v1L2h5_SpnEmailPerLoad.Enabled;
            set => _l1v1L2h5_SpnEmailPerLoad.Enabled = value;
        }

        // Seam B — issue #677 deactivation intents.

        /// <summary>Forwards the WinForms <see cref="Form.Deactivate"/> event to the controller.</summary>
        public event EventHandler FormDeactivated
        {
            add => this.Deactivate += value;
            remove => this.Deactivate -= value;
        }

        /// <summary>
        /// Walks the ActiveControl chain to its leaf and reports whether that leaf is a WebView2.
        /// A container's ActiveControl is itself a container until the chain bottoms out, so the
        /// walk — not a single-level check — is what identifies the control that actually holds
        /// Win32 keyboard focus.
        /// </summary>
        public bool IsWebView2Focused
        {
            get
            {
                Control active = this.ActiveControl;
                while (active is ContainerControl container && container.ActiveControl != null)
                {
                    active = container.ActiveControl;
                }
                return active is Microsoft.Web.WebView2.WinForms.WebView2;
            }
        }

        /// <summary>
        /// Parks focus on the OK button, a benign non-WebView2 Seam-B designer control, so no
        /// WebView2 child window retains the shared Outlook UI thread's keyboard focus.
        /// </summary>
        public void ParkFocusOffWebView2() => this.ActiveControl = _l1v1L2h2_ButtonOK;

        // Seam D — collapsed item-viewer template margin
        public Padding ItemViewerTemplateMargin => _QfcItemViewerTemplate?.Margin ?? default;

        // Seam D — controls excluded from keyboard-event wiring (the collapsed item-viewer template)
        public IReadOnlyList<Control> GetKeyEventExclusionControls() =>
            new List<Control> { _QfcItemViewerTemplate };

        // Seam D — snapshots the item-viewer template cell states for both display states.
        // Returns null if either template is not yet initialized (form not yet shown).
        public TlpCellStates CaptureTlpCellStates()
        {
            if (_qfcItemViewerExpandedTemplate is null || _QfcItemViewerTemplate is null)
            {
                return null;
            }

            return new TlpCellStates(
                new List<KeyValuePair<string, List<TlpCellSnapShot>>>()
                {
                    new KeyValuePair<string, List<TlpCellSnapShot>>(
                        "Expanded",
                        new List<TlpCellSnapShot>()
                        {
                            new TlpCellSnapShot(
                                _qfcItemViewerExpandedTemplate.L0vh_Tlp,
                                _qfcItemViewerExpandedTemplate.L1h0L2hv3h_TlpBodyToggle
                            ),
                            new TlpCellSnapShot(
                                _qfcItemViewerExpandedTemplate.L1h0L2hv3h_TlpBodyToggle,
                                _qfcItemViewerExpandedTemplate.TxtboxBody
                            ),
                            new TlpCellSnapShot(
                                _qfcItemViewerExpandedTemplate.L1h0L2hv3h_TlpBodyToggle,
                                _qfcItemViewerExpandedTemplate.TopicThread
                            ),
                            new TlpCellSnapShot(
                                _qfcItemViewerExpandedTemplate.L0vh_Tlp,
                                _qfcItemViewerExpandedTemplate.L0v2h2_WebView2
                            ),
                            new TlpCellSnapShot(
                                _qfcItemViewerExpandedTemplate.L0vh_Tlp,
                                _qfcItemViewerExpandedTemplate.LblAcOpen
                            ),
                            new TlpCellSnapShot(
                                _qfcItemViewerExpandedTemplate.L0vh_Tlp,
                                _qfcItemViewerExpandedTemplate.LblAcBody
                            ),
                        }
                    ),
                    new KeyValuePair<string, List<TlpCellSnapShot>>(
                        "Compressed",
                        new List<TlpCellSnapShot>()
                        {
                            new TlpCellSnapShot(
                                _QfcItemViewerTemplate.L0vh_Tlp,
                                _QfcItemViewerTemplate.L1h0L2hv3h_TlpBodyToggle
                            ),
                            new TlpCellSnapShot(
                                _QfcItemViewerTemplate.L1h0L2hv3h_TlpBodyToggle,
                                _QfcItemViewerTemplate.TxtboxBody
                            ),
                            new TlpCellSnapShot(
                                _QfcItemViewerTemplate.L1h0L2hv3h_TlpBodyToggle,
                                _QfcItemViewerTemplate.TopicThread
                            ),
                            new TlpCellSnapShot(
                                _QfcItemViewerTemplate.L0vh_Tlp,
                                _QfcItemViewerTemplate.L0v2h2_WebView2
                            ),
                            new TlpCellSnapShot(
                                _QfcItemViewerTemplate.L0vh_Tlp,
                                _QfcItemViewerTemplate.LblAcOpen
                            ),
                            new TlpCellSnapShot(
                                _QfcItemViewerTemplate.L0vh_Tlp,
                                _QfcItemViewerTemplate.LblAcBody
                            ),
                        }
                    ),
                }
            );
        }
        #endregion IQfcFormViewer
    }
}
