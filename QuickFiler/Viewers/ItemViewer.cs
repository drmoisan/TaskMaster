using SVGControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Threading;
using UtilitiesCS;

namespace QuickFiler
{
    public partial class ItemViewer : UserControl, IItemViewer
    {
        public ItemViewer()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            InitControlGroups();
        }

        //private IList<Control> _rightControls;

        private IList<Label> _tipsLabels;
        public IList<Label> TipsLabels { get => _tipsLabels; }

        private IList<Label> _leftTipsLabels;
        public IList<Label> LeftTipsLabels { get => _leftTipsLabels; }

        private IList<Label> _expandedTipsLabels;
        public IList<Label> ExpandedTipsLabels { get => _expandedTipsLabels; }

        private IItemControler _controller;
        public IItemControler Controller { get => _controller; set => _controller = value; }

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler { get => _uiScheduler; }

        private Dispatcher _uiDispatcher;
        public Dispatcher UiDispatcher { get => _uiDispatcher; }

        public void RemoveControlsColsRightOf(Control furthestRight)
        {
            if (furthestRight.Parent is TableLayoutPanel)
            {
                var tlp = (TableLayoutPanel)furthestRight.Parent;
                var columnNumber = tlp.GetColumn(furthestRight);
                tlp.SetColumnSpan(L0v2h2_WebView2, 10);

                if (++columnNumber < tlp.ColumnCount)
                {
                    var columnsToRemove = tlp.ColumnCount - columnNumber;
                    tlp.RemoveSpecificColumn(columnNumber, columnsToRemove);

                }
            }
            else
            {
                RemoveControlsRightOf(furthestRight);
            }
        }

        private void RemoveControlsRightOf(Control furthestRight)
        {
            var controlsToRemove = ControlsRightOf(furthestRight);
            for (int i = controlsToRemove.Count - 1; i >= 0; i--)
            {
                var control = controlsToRemove[i];
                control.Parent.Controls.Remove(control);
                controlsToRemove.RemoveAt(i);
                control.Dispose();
            }
        }

        private void InitControlGroups()
        {
            _tipsLabels = new List<Label>
            {
                // TODO: Add new labels for reply, reply all, forward
                LblAcOpen,
                LblAcPopOut,
                LblAcTask,
                LblAcDelete,
                LblAcMoveOptions,
                //LblAcAttachments,
                //LblAcConversation,
                //LblAcEmail,
                LblAcFolder,
                LblAcSearch,
                LblAcReply,
                LblAcReplyAll,
                LblAcFwd,
                LblAcBody,
            };

            _leftTipsLabels = new List<Label>
            {
                LblAcOpen,
                LblAcBody,
            };

            //_rightControls = ControlsRightOf(this.LblConvCt);

            _expandedTipsLabels = new List<Label>
            {
                LblAcBody,
            };
        }

        private List<Control> ControlsRightOf(Control furthestRight)
        {
            var controlLocation = new List<(Control Control, Point Point)>();
            this.ForAllControls(new Point(0, 0), (Control control, Point point) =>
            {
                var trueLocation = control.Location + new Size(point);
                controlLocation.Add((control, trueLocation));
                return trueLocation;
            });

            Point limit;
            if (controlLocation.Any(tup => tup.Control == furthestRight))
            {
                var tup = controlLocation.First(tup => tup.Control == furthestRight);
                limit = tup.Point + furthestRight.Size;
            }
            else
            {
                limit = furthestRight.Location + furthestRight.Size;
            }
            return controlLocation.Where(tup => tup.Point.X > limit.X).Select(tup => tup.Control).ToList();
        }

        private void L0v2h2_WebView2_ParentChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Parent Changed");
        }

        private void MenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            MenuItem_CheckedChanged(menuItem);
        }

        private void MenuItem_CheckedChanged(ToolStripMenuItem menuItem)
        {
            if (menuItem.Checked)
            {
                menuItem.Image = global::QuickFiler.Properties.Resources.CheckBoxChecked;
            }
            else
            {
                menuItem.Image = null;
            }
        }

        public List<Component> MenuItems => Initializer.GetOrLoad(ref _menuItems, LoadMenuItems);
        private List<Component> _menuItems;
        private List<Component> LoadMenuItems()
        {
            var menuItems = new List<Component>
            {
                this.MoveOptionsMenu,
                this.ConversationMenuItem,
                this.SaveAttachmentsMenuItem,
                this.SaveEmailMenuItem,
                this.SavePicturesMenuItem,
            };
            return menuItems;
        }

        private void MoveOptionsMenu_Click(object sender, EventArgs e)
        {

        }

        #region Field to Property for Interface

        public System.Windows.Forms.Label LblItemNumber { get => _lblItemNumber; set => _lblItemNumber = value; }
        public System.Windows.Forms.Label LblSender { get => _lblSender; set => _lblSender = value; }
        public System.Windows.Forms.Label LblCaptionTriage { get => _lblCaptionTriage; set => _lblCaptionTriage = value; }
        public System.Windows.Forms.Label LblTriage { get => _lblTriage; set => _lblTriage = value; }
        public System.Windows.Forms.Label LblCaptionPredicted { get => _lblCaptionPredicted; set => _lblCaptionPredicted = value; }
        public System.Windows.Forms.Label LblActionable { get => _lblActionable; set => _lblActionable = value; }
        public System.Windows.Forms.Label LblSentOn { get => _lblSentOn; set => _lblSentOn = value; }
        public System.Windows.Forms.Label LblSubject { get => _lblSubject; set => _lblSubject = value; }
        public System.Windows.Forms.Label LblConvCt { get => _lblConvCt; set => _lblConvCt = value; }
        public System.Windows.Forms.Label LblAcOpen { get => _lblAcOpen; set => _lblAcOpen = value; }
        public System.Windows.Forms.Label LblFolder { get => _lblFolder; set => _lblFolder = value; }
        public System.Windows.Forms.Label LblAcSearch { get => _lblAcSearch; set => _lblAcSearch = value; }
        public System.Windows.Forms.Label LblSearch { get => _lblSearch; set => _lblSearch = value; }
        public System.Windows.Forms.Label LblAcFolder { get => _lblAcFolder; set => _lblAcFolder = value; }
        public System.Windows.Forms.TextBox TxtboxBody { get => _txtboxBody; set => _txtboxBody = value; }
        public BrightIdeasSoftware.FastObjectListView TopicThread { get => _topicThread; set => _topicThread = value; }
        public BrightIdeasSoftware.OLVColumn Sender { get => _sender; set => _sender = value; }
        public BrightIdeasSoftware.OLVColumn SentDate { get => _sentDate; set => _sentDate = value; }
        public BrightIdeasSoftware.OLVColumn Infolder { get => _infolder; set => _infolder = value; }
        public System.Windows.Forms.Label LblAcBody { get => _lblAcBody; set => _lblAcBody = value; }
        public Microsoft.Web.WebView2.WinForms.WebView2 L0v2h2_WebView2 { get => _l0v2h2_WebView2; set => _l0v2h2_WebView2 = value; }
        public System.Windows.Forms.TableLayoutPanel L0vh_Tlp { get => _l0vh_Tlp; set => _l0vh_Tlp = value; }
        public System.Windows.Forms.TableLayoutPanel L1h0L2hv3h_TlpBodyToggle { get => _l1h0L2hv3h_TlpBodyToggle; set => _l1h0L2hv3h_TlpBodyToggle = value; }
        public System.Windows.Forms.Panel L1h1L2v1h3Panel { get => _l1h1L2v1h3Panel; set => _l1h1L2v1h3Panel = value; }
        public System.Windows.Forms.Label LblAcDelete { get => _lblAcDelete; set => _lblAcDelete = value; }
        public ButtonSVG BtnDelItem { get => _btnDelItem; set => _btnDelItem = value; }
        public System.Windows.Forms.Label LblAcPopOut { get => _lblAcPopOut; set => _lblAcPopOut = value; }
        public ButtonSVG BtnPopOut { get => _btnPopOut; set => _btnPopOut = value; }
        public System.Windows.Forms.Label LblAcTask { get => _lblAcTask; set => _lblAcTask = value; }
        public ButtonSVG BtnFlagTask { get => _btnFlagTask; set => _btnFlagTask = value; }
        public ButtonSVG BtnForward { get => _btnForward; set => _btnForward = value; }
        public System.Windows.Forms.Label LblAcReply { get => _lblAcReply; set => _lblAcReply = value; }
        public System.Windows.Forms.Label LblAcReplyAll { get => _lblAcReplyAll; set => _lblAcReplyAll = value; }
        public System.Windows.Forms.Label LblAcFwd { get => _lblAcFwd; set => _lblAcFwd = value; }
        public ButtonSVG BtnReply { get => _btnReply; set => _btnReply = value; }
        public ButtonSVG BtnReplyAll { get => _btnReplyAll; set => _btnReplyAll = value; }
        public System.Windows.Forms.ComboBox CboFolders { get => _cboFolders; set => _cboFolders = value; }
        public System.Windows.Forms.TextBox TxtboxSearch { get => _txtboxSearch; set => _txtboxSearch = value; }
        public System.Windows.Forms.MenuStrip MoveOptionsStrip { get => _moveOptionsStrip; set => _moveOptionsStrip = value; }
        public System.Windows.Forms.ToolStripMenuItem MoveOptionsMenu { get => _moveOptionsMenu; set => _moveOptionsMenu = value; }
        public Viewers.ToolStripMenuItemCb ConversationMenuItem { get => _conversationMenuItem; set => _conversationMenuItem = value; }
        public Viewers.ToolStripMenuItemCb SaveAttachmentsMenuItem { get => _saveAttachmentsMenuItem; set => _saveAttachmentsMenuItem = value; }
        public Viewers.ToolStripMenuItemCb SaveEmailMenuItem { get => _saveEmailMenuItem; set => _saveEmailMenuItem = value; }
        public Viewers.ToolStripMenuItemCb SavePicturesMenuItem { get => _savePicturesMenuItem; set => _savePicturesMenuItem = value; }
        public System.Windows.Forms.Label LblAcMoveOptions { get => _lblAcMoveOptions; set => _lblAcMoveOptions = value; }

        #endregion Field to Property for Interface
    }
}

