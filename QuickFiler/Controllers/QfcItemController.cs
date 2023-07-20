using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using System.Windows.Forms;
using System.Net.Mail;
using System.Collections;
using QuickFiler.Helper_Classes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Xml.Linq;
using System.Diagnostics;

namespace QuickFiler.Controllers
{
    internal class QfcItemController : IQfcItemController
    {
        #region constructors

        public QfcItemController(IApplicationGlobals AppGlobals,
                                 QfcItemViewer itemViewer,
                                 int viewerPosition,
                                 MailItem mailItem,
                                 IQfcKeyboardHandler keyboardHandler,
                                 IQfcCollectionController parent)
        {
            Initialize(AppGlobals, itemViewer, viewerPosition, mailItem, keyboardHandler, parent, async: true);
        }



        public QfcItemController(IApplicationGlobals AppGlobals,
                                 QfcItemViewer itemViewer,
                                 int viewerPosition,
                                 MailItem mailItem,
                                 IQfcKeyboardHandler keyboardHandler,
                                 IQfcCollectionController parent,
                                 bool async)
        {
            Initialize(AppGlobals, itemViewer, viewerPosition, mailItem, keyboardHandler, parent, async);
        }

        

        #endregion

        #region private fields and variables

        private IApplicationGlobals _globals;
        private QfcItemViewer _itemViewer;
        private IQfcCollectionController _parent;
        private IList<IQfcTipsDetails> _listTipsDetails;
        private IQfcTipsDetails _itemPositionTips;
        private MailItem _mailItem;
        private DataFrame _dfConversation;
        private IList<MailItem> _conversationItems;
        private int _viewerPosition;
        private FolderHandler _fldrHandler;
        private IList<Control> _controls;
        private IList<TableLayoutPanel> _tableLayoutPanels;
        private IList<Button> _buttons;
        private IList<CheckBox> _checkBoxes;
        private IList<Label> _labels;
        private bool _expanded = false;
        private bool _activeUI = false;
        private bool _isChild;
        private Dictionary<string,Theme> _themes;
        private string _activeTheme;
        private IQfcKeyboardHandler _keyboardHandler;
        private string _convOriginID = "";
        private bool _suppressEvents = false;
        private int _intEnterCounter = 0;
        private int _intComboRightCtr = 0;

        #endregion

        #region Exposed properties

        public IList<Button> Buttons { get => _buttons; }

        public string ConvOriginID { get => _convOriginID; set => _convOriginID = value; }

        public int CounterEnter { get => _intEnterCounter; set => _intEnterCounter = value; }

        public int CounterComboRight { get => _intComboRightCtr; set => _intComboRightCtr = value; }

        public IList<MailItem> ConversationItems 
        {
            get 
            { 
                if (_conversationItems is null) 
                {
                    _conversationItems = ConvHelper.GetMailItemList(DfConversation,
                                                                   ((Folder)Mail.Parent).StoreID,
                                                                   _globals.Ol.App,
                                                                   true)
                                                   .Cast<MailItem>()
                                                   .ToList();
                }
                return _conversationItems; 
            }
            
            set => _conversationItems = value; 
        }
        
        public DataFrame DfConversation 
        {
            get 
            {
                if ((_dfConversation is null)&&(_mailItem is not null))
                {
                    _dfConversation = Mail.GetConversationDf(true, true);
                }
                return _dfConversation; 
            }
        } 

        public int Height { get => _itemViewer.Height; }

        public bool IsExpanded { get => _expanded; }

        public bool IsChild { get => _isChild; set => _isChild = value; }

        public bool IsActiveUI { get => _activeUI; set => _activeUI = value; }

        public IList<IQfcTipsDetails> ListTipsDetails { get => _listTipsDetails; }

        public MailItem Mail { get => _mailItem; set => _mailItem = value; }

        public IQfcCollectionController Parent { get => _parent; }

        public int Position 
        { 
            get => _viewerPosition;
            set 
            { 
                _viewerPosition = value;
                _itemViewer.LblPos.Text = _viewerPosition.ToString();
            }
        }

        public string SelectedFolder { get => _itemViewer.CboFolders.SelectedItem.ToString(); }

        public string Sender { get => _itemViewer.LblSender.Text; }

        public string SentDate { get => _mailItem.SentOn.ToString("MM/dd/yyyy"); }

        public string SentTime { get => _mailItem.SentOn.ToString("HH:mm"); }

        public string Subject { get => _itemViewer.lblSubject.Text; }

        public bool SuppressEvents { get => _suppressEvents; set => _suppressEvents = value; }

        public string To { get => _mailItem.To; }

        public IList<TableLayoutPanel> TableLayoutPanels { get => _tableLayoutPanels;}

        #endregion

        #region ItemViewer Setup and Disposal

        private void Initialize(IApplicationGlobals AppGlobals,
                                QfcItemViewer itemViewer,
                                int viewerPosition,
                                MailItem mailItem,
                                IQfcKeyboardHandler keyboardHandler,
                                IQfcCollectionController parent,
                                bool async)
        {
            _globals = AppGlobals;

            // Grab handle on viewer and controls
            _itemViewer = itemViewer;
            _itemViewer.Controller = this;

            _viewerPosition = viewerPosition;   // visible position in collection (index is 1 less)
            _mailItem = mailItem;               // handle on underlying Email
            _keyboardHandler = keyboardHandler; // handle keystrokes
            _parent = parent;                   // handle on collection controller
            _themes = ThemeHelper.SetupThemes(this, _itemViewer);

            ResolveControlGroups(itemViewer);

            // Populate placeholder controls with 
            PopulateControls(mailItem, viewerPosition);

            ToggleTips(async: async, desiredState: Enums.ToggleState.Off);
            ToggleNavigation(async: async, desiredState: Enums.ToggleState.Off);

            WireEvents();
        }

        internal void ResolveControlGroups(QfcItemViewer itemViewer)
        {
            var ctrls = itemViewer.GetAllChildren();
            _controls = ctrls.ToList();

            _listTipsDetails = _itemViewer.TipsLabels
                               .Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                               .ToList();

            _itemPositionTips = new QfcTipsDetails(_itemViewer.LblPos);

            _tableLayoutPanels = ctrls.Where(x => x is TableLayoutPanel)
                         .Select(x => (TableLayoutPanel)x)
                         .ToList();

            _buttons = ctrls.Where(x => x is Button)
                            .Select(x => (Button)x)
                            .ToList();

            _labels = ctrls.Where(x => (x is Label) &&
                                       (!itemViewer.TipsLabels.Contains(x)) &&
                                       (x != itemViewer.lblSubject) &&
                                       (x != itemViewer.LblSender))
                           .Select(x => (Label)x)
                           .ToList();

        }

        public void PopulateControls(MailItem mailItem, int viewerPosition)
        {
            var itemInfo = new MailItemInfo(mailItem);
            itemInfo.ExtractBasics();
            
            _itemViewer.BeginInvoke(new System.Action(() => AssignControls(itemInfo, viewerPosition)));
            
            //_itemViewer.LblSender.Text = itemInfo.Sender;
            //_itemViewer.lblSubject.Text = itemInfo.Subject;
            //_itemViewer.TxtboxBody.Text = itemInfo.Body;

            //_itemViewer.LblTriage.Text = itemInfo.Triage;
            //_itemViewer.LblSentOn.Text = itemInfo.SentOn;
            //_itemViewer.LblActionable.Text = itemInfo.Actionable;
            //_itemViewer.LblPos.Text = viewerPosition.ToString();

            //if (_mailItem.UnRead == true)
            //{
            //    _itemViewer.LblSender.Font = new Font(_itemViewer.LblSender.Font, FontStyle.Bold);
            //    _itemViewer.lblSubject.Font = new Font(_itemViewer.lblSubject.Font, FontStyle.Bold);
            //}
        }

        internal void AssignControls(MailItemInfo itemInfo, int viewerPosition)
        {
            _itemViewer.LblSender.Text = itemInfo.Sender;
            _itemViewer.lblSubject.Text = itemInfo.Subject;
            _itemViewer.TxtboxBody.Text = itemInfo.Body;

            _itemViewer.LblTriage.Text = itemInfo.Triage;
            _itemViewer.LblSentOn.Text = itemInfo.SentOn;
            _itemViewer.LblActionable.Text = itemInfo.Actionable;
            _itemViewer.LblPos.Text = viewerPosition.ToString();
        }

        /// <summary>
        /// Gets the Outlook.Conversation from the underlying MailItem
        /// embedded in the class. Conversation details are loaded to 
        /// a Dataframe. Count is inferred from the df rowcount
        /// </summary>
        public void PopulateConversation()
        {
            PopulateConversation(_mailItem.GetConversationDf(true, true));
        }

        /// <summary>
        /// TBD if this overload will be of use. Depends on whether _dfConversation
        /// is needed by any individual element when expanded
        /// </summary>
        /// <param name="df"></param>
        public void PopulateConversation(DataFrame df)
        {
            _dfConversation = df;
            int count = _dfConversation.Rows.Count();
            PopulateConversation(count);
        }

        /// <summary>
        /// Sets the conversation count of the visual without altering the
        /// _dfConversation. Usefull when expanding or collapsing the 
        /// conversation to show how many items will be moved
        /// </summary>
        /// <param name="count"></param>
        public void PopulateConversation(int count)
        {
            _itemViewer.LblConvCt.BeginInvoke(new System.Action(() =>
            {
                _itemViewer.LblConvCt.Text = count.ToString();
                if (count == 0) { _itemViewer.LblConvCt.BackColor = Color.Red; }
            }));
        }
        
        public void PopulateFolderCombobox(object varList = null)
        {
            if (varList is null)
            {
                _fldrHandler = new FolderHandler(
                    _globals, _mailItem, FolderHandler.Options.FromField);
            }
            else
            {
                _fldrHandler = new FolderHandler(
                    _globals, varList, FolderHandler.Options.FromArrayOrString);
            }

            _itemViewer.CboFolders.BeginInvoke(new System.Action(() =>
            {
                _itemViewer.CboFolders.Items.AddRange(_fldrHandler.FolderArray);
                _itemViewer.CboFolders.SelectedIndex = 1;
            }));
            
        }

        public void Cleanup()
        {
            _globals = null;
            _itemViewer = null;
            _parent = null;
            _listTipsDetails = null;
            _mailItem = null;
            _dfConversation = null;
            _fldrHandler = null;
        }

        #endregion

        #region Event Handlers

        internal void WireEvents()
        {
            //Debug.WriteLine($"Wiring keyboard for item {this.Position}, {this.Subject}");
            _itemViewer.ForAllControls(x =>
            {
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_keyboardHandler.KeyboardHandler_PreviewKeyDown);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(_keyboardHandler.KeyboardHandler_KeyDown);
                x.KeyUp += new System.Windows.Forms.KeyEventHandler(_keyboardHandler.KeyboardHandler_KeyUp);
                x.KeyPress += new System.Windows.Forms.KeyPressEventHandler(_keyboardHandler.KeyboardHandler_KeyPress);
                //Debug.WriteLine($"Registered handler for {x.Name}");
            },
            new List<Control> { _itemViewer.CboFolders, _itemViewer.TxtboxSearch });

            _itemViewer.CbxConversation.CheckedChanged += new System.EventHandler(this.CbxConversation_CheckedChanged);

            _itemViewer.BtnFlagTask.Click += new System.EventHandler(this.BtnFlagTask_Click);
            _itemViewer.BtnPopOut.Click += new System.EventHandler(this.BtnPopOut_Click);
            //_itemViewer.BtnPopOut.Click += new System.EventHandler(_keyboardHandler.BtnPopOut_Click);
            _itemViewer.BtnDelItem.Click += new System.EventHandler(this.BtnDelItem_Click);
            _itemViewer.TxtboxSearch.TextChanged += new System.EventHandler(this.TxtboxSearch_TextChanged);
            _itemViewer.TxtboxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtboxSearch_KeyDown);
            //_itemViewer.CboFolders.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CboFolders_KeyDown);
            _itemViewer.CboFolders.KeyDown += new System.Windows.Forms.KeyEventHandler(_keyboardHandler.CboFolders_KeyDown);
        }

        internal void RegisterFocusActions()
        {
            _keyboardHandler.KdKeyActions.Add(
                Keys.Right, (x) => this.ToggleConversationCheckbox(Enums.ToggleState.Off));
            _keyboardHandler.KdKeyActions.Add(
                Keys.Left, (x) => this.ToggleConversationCheckbox(Enums.ToggleState.On));
            _keyboardHandler.KdCharActions.Add('O', (x) => Debug.WriteLine($"{x} keyboardhandler tbd"));
            _keyboardHandler.KdCharActions.Add('C', (x) => this.ToggleConversationCheckbox());
            _keyboardHandler.KdCharActions.Add('A', (x) => this.ToggleSaveAttachments());
            _keyboardHandler.KdCharActions.Add('M', (x) => this.ToggleSaveCopyOfMail());
            _keyboardHandler.KdCharActions.Add('E', (x) => this.ToggleExpansion());
            _keyboardHandler.KdCharActions.Add('S', (x) => this.JumpToSearchTextbox());
            _keyboardHandler.KdCharActions.Add('T', (x) => this.FlagAsTask());
            _keyboardHandler.KdCharActions.Add('P', (x) => this._parent.PopOutControlGroup(Position));
            _keyboardHandler.KdCharActions.Add('R', (x) => this._parent.RemoveSpecificControlGroup(Position));
            _keyboardHandler.KdCharActions.Add('X', (x) => this.MarkItemForDeletion());
            _keyboardHandler.KdCharActions.Add('F', (x) => this.JumpToFolderDropDown());
        }

        internal void UnregisterFocusActions()
        {
            _keyboardHandler.KdKeyActions.Remove(Keys.Right);
            _keyboardHandler.KdKeyActions.Remove(Keys.Left);
            _keyboardHandler.KdCharActions.Remove('O');
            _keyboardHandler.KdCharActions.Remove('C');
            _keyboardHandler.KdCharActions.Remove('A');
            _keyboardHandler.KdCharActions.Remove('M');
            _keyboardHandler.KdCharActions.Remove('E');
            _keyboardHandler.KdCharActions.Remove('S');
            _keyboardHandler.KdCharActions.Remove('T');
            _keyboardHandler.KdCharActions.Remove('P');
            _keyboardHandler.KdCharActions.Remove('R');
            _keyboardHandler.KdCharActions.Remove('X');
            _keyboardHandler.KdCharActions.Remove('F');
        }

        internal void CbxConversation_CheckedChanged(object sender, EventArgs e)
        {
            if (!SuppressEvents)
            {
                if (_itemViewer.CbxConversation.Checked) { CollapseConversation(); }
                else { EnumerateConversation(); }
            }
        }

        internal void BtnFlagTask_Click(object sender, EventArgs e) => FlagAsTask();
        
        internal void BtnPopOut_Click(object sender, EventArgs e) => _parent.PopOutControlGroup(Position);

        internal void BtnDelItem_Click(object sender, EventArgs e) => MarkItemForDeletion();

        internal void TxtboxSearch_TextChanged(object sender, EventArgs e)
        {
            _itemViewer.CboFolders.Items.Clear();
            _itemViewer.CboFolders.Items.AddRange(
                _fldrHandler.FindFolder(SearchString: "*" + 
                _itemViewer.TxtboxSearch.Text + "*",
                ReloadCTFStagingFiles: false,
                ReCalcSuggestions: false,
                objItem: Mail));

            if (_itemViewer.CboFolders.Items.Count >= 2)
                _itemViewer.CboFolders.SelectedIndex = 1;
            _itemViewer.CboFolders.DroppedDown = true;
        }

        internal void TxtboxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                _itemViewer.CboFolders.DroppedDown = true;
                _itemViewer.CboFolders.Focus();
            }
        }

        //internal void CboFolders_KeyDown(object sender, KeyEventArgs e)
        //{
        //    switch (e.KeyCode)
        //    {
        //        case Keys.Escape:
        //            {
        //                _intEnterCounter = 1;
        //                _intComboRightCtr = 0;
        //                break;
        //            }
        //        case Keys.Up:
        //            {
        //                _intEnterCounter = 0;
        //                break;
        //            }
        //        case Keys.Down:
        //            {
        //                _intEnterCounter = 0;
        //                break;
        //            }
        //        case Keys.Right:
        //            {
        //                _intEnterCounter = 0;
        //                switch (_intComboRightCtr)
        //                {
        //                    case 0:
        //                        {
        //                            _itemViewer.CboFolders.DroppedDown = true;
        //                            _intComboRightCtr++;
        //                            break;
        //                        }
        //                    case 1:
        //                        {
        //                            _itemViewer.CboFolders.DroppedDown = false;
        //                            _intComboRightCtr = 0;
        //                            MyBox.ShowDialog("Pop Out Item or Enumerate Conversation?", 
        //                                "Dialog", BoxIcon.Question, RightKeyActions);
        //                            break;
        //                        }
        //                    default:
        //                        {
        //                            MessageBox.Show(
        //                                "Error in intComboRightCtr ... setting to 0 and continuing",
        //                                "Error",
        //                                MessageBoxButtons.OK,
        //                                MessageBoxIcon.Error);
        //                            _intComboRightCtr = 0;
        //                            break;
        //                        }
        //                }
        //                e.SuppressKeyPress = true;
        //                e.Handled = true;
        //                break;
        //            }
        //        case Keys.Left:
        //            {
        //                _intEnterCounter = 0;
        //                _intComboRightCtr = 0;
        //                if (_itemViewer.CboFolders.DroppedDown)
        //                {
        //                    _itemViewer.CboFolders.DroppedDown = false;
        //                    e.SuppressKeyPress = true;
        //                    e.Handled = true;
        //                }
        //                else { _keyboardHandler.KeyboardHandler_KeyDown(sender, e); }
                        
        //                break;
        //            }
        //        case Keys.Return:
        //            {
        //                _intEnterCounter++;
        //                if (_intEnterCounter == 1)
        //                {
        //                    _intEnterCounter = 0;
        //                    _intComboRightCtr = 0;
        //                    _keyboardHandler.KeyboardHandler_KeyDown(sender, e);
        //                }
        //                else
        //                {
        //                    _intEnterCounter = 1;
        //                    _intComboRightCtr = 0;
        //                    e.Handled = true;
        //                }
                        
        //                break;
        //            }
        //    }            
        //}

        #endregion

        #region UI Navigation Methods

        public void ToggleNavigation(bool async)
        {
            _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(true)));
            if (async)
            {
                _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(true)));
            }
            else
            {
                _itemViewer.Invoke(new System.Action(() => _itemPositionTips.Toggle(true)));
            }
        }

        public void ToggleNavigation(bool async, Enums.ToggleState desiredState)
        {
            if (async)
            {
                _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(desiredState, true)));
            }
            else
            {
                _itemViewer.Invoke(new System.Action(() => _itemPositionTips.Toggle(desiredState, true)));
            }
            
        }

        public void ToggleTips(bool async)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async) { _itemViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle())); }
                else { _itemViewer.Invoke(new System.Action(() => tipsDetails.Toggle())); }
            }
        }

        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async) { _itemViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(desiredState))); }
                else { _itemViewer.Invoke(new System.Action(() => tipsDetails.Toggle(desiredState))); }
            }
        }

        public void Accel_FocusToggle(Enums.ToggleState desiredState)
        {
            _itemViewer.Invoke(new System.Action(() =>
            {
                if ((desiredState == Enums.ToggleState.On)&&(!_activeUI))
                {
                    // If not active and we want to turn on, then we are turning on
                    _activeUI = true;
                    if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkActive"; }
                    else { _activeTheme = "LightActive"; }
                    ToggleTips(async: false, desiredState: Enums.ToggleState.On);
                    RegisterFocusActions();
                }
                else if ((desiredState == Enums.ToggleState.Off) && (_activeUI))
                {
                    // If active and we want to turn off, then we are turning off
                    _activeUI = false;
                    if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkNormal"; }
                    else { _activeTheme = "LightNormal"; }
                    ToggleTips(async: false, desiredState: Enums.ToggleState.Off);
                    UnregisterFocusActions();
                }
                _themes[_activeTheme].SetTheme(async: false);
            }));
        }
        
        public void Accel_FocusToggle()
        {
            _itemViewer.Invoke(new System.Action(() => 
            { 
                if (_activeUI) 
                {
                    // If active, then we are turning off
                    _activeUI = false;
                    if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkNormal"; }
                    else { _activeTheme = "LightNormal";}
                    ToggleTips(async: false, desiredState: Enums.ToggleState.Off);
                    UnregisterFocusActions();
                }
                else 
                { 
                    // If not active, then we are turning on
                    _activeUI = true;
                    if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkActive"; }
                    else { _activeTheme = "LightActive"; }
                    ToggleTips(async: false, desiredState: Enums.ToggleState.On);
                    RegisterFocusActions();
                }
                _themes[_activeTheme].SetTheme(async: false);
            }));
        }

        public void Accel_Toggle(bool async)
        {
            if (_activeUI) { Accel_FocusToggle(); }
            ToggleNavigation(async);
        }

        public void ToggleExpansion()
        {
            if (_expanded) { ToggleExpansion(Enums.ToggleState.Off); }
            else { ToggleExpansion(Enums.ToggleState.On); }
        }

        public void ToggleExpansion(Enums.ToggleState desiredState)
        {
            _parent.ToggleExpansionStyle(desiredState);
            if (desiredState == Enums.ToggleState.On) 
            {
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[0].Width = 0;
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[1].Width = 100;
                _itemViewer.TxtboxBody.Visible = false;
                _itemViewer.TopicThread.Visible = true;
                _itemViewer.L0v2_web.Visible = true;
                _itemViewer.L0v2_web.DocumentText = Mail.HTMLBody;
                _expanded = true; 
            }
            else 
            {
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[0].Width = 100;
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[1].Width = 0;
                _itemViewer.TxtboxBody.Visible = true;
                _itemViewer.L0v2_web.Visible = true;
                _expanded = false; 
            }
        }

        public void JumpToFolderDropDown()
        {
            _keyboardHandler.ToggleKeyboardDialog();
            _itemViewer.Invoke(new System.Action(() => 
            { 
                _itemViewer.CboFolders.Focus();
                _itemViewer.CboFolders.DroppedDown = true;
                _intEnterCounter = 0;
            }));
        }

        public void JumpToSearchTextbox()
        {
            _keyboardHandler.ToggleKeyboardDialog();
            _itemViewer.TxtboxSearch.Invoke(new System.Action(() => _itemViewer.TxtboxSearch.Focus()));
        }

        /// <summary>
        /// Function programatically clicks the "Conversation" checkbox
        /// </summary>
        public void ToggleConversationCheckbox()
        {
            _itemViewer.CbxConversation.Invoke(new System.Action(() => 
                _itemViewer.CbxConversation.Checked = 
                !_itemViewer.CbxConversation.Checked));
        }

        /// <summary>
        /// Function programatically sets the "Conversation" checkbox to the desired state 
        /// if it is not already in that state
        /// </summary>
        /// <param name="desiredState">State of checkbox desired</param>
        public void ToggleConversationCheckbox(Enums.ToggleState desiredState)
        {
            _itemViewer.CbxConversation.Invoke(new System.Action(() =>
            {
                switch (desiredState)
                {
                    case Enums.ToggleState.On:
                        if (_itemViewer.CbxConversation.Checked == false)
                            _itemViewer.CbxConversation.Checked = true;
                        break;
                    case Enums.ToggleState.Off:
                        if (_itemViewer.CbxConversation.Checked == true)
                            _itemViewer.CbxConversation.Checked = false;
                        break;
                    default:
                        _itemViewer.CbxConversation.Checked = !_itemViewer.CbxConversation.Checked;
                        break;
                }
            }));
        }

        public void ToggleSaveAttachments()
        {
            _itemViewer.CbxAttachments.Invoke(new System.Action(() => 
                _itemViewer.CbxAttachments.Checked = 
                !_itemViewer.CbxAttachments.Checked));
        }

        public void ToggleSaveCopyOfMail()
        {
            _itemViewer.CbxEmailCopy.Invoke(new System.Action(() => 
                _itemViewer.CbxEmailCopy.Checked = 
                !_itemViewer.CbxEmailCopy.Checked));
        }

        #endregion

        #region UI Visual Helper Methods

        public void SetThemeDark(bool async)
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["DarkNormal"].SetTheme(async);
                _activeTheme = "DarkNormal";
            }
            else
            {
                _themes["DarkActive"].SetTheme(async);
                _activeTheme = "DarkActive";
            }
        }

        public void SetThemeLight(bool async)
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["LightNormal"].SetTheme(async);
                _activeTheme = "LightNormal";
            }
            else
            {
                _themes["LightActive"].SetTheme(async);
                _activeTheme = "LightActive";
            }
        }

        // TODO: Implement ApplyReadEmailFormat
        public void ApplyReadEmailFormat()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Major Action Methods

        internal void CollapseConversation()
        {
            var folderList = _itemViewer.CboFolders.Items.Cast<object>().Select(item => item.ToString()).ToArray();
            var entryID = _convOriginID != "" ? _convOriginID :  Mail.EntryID;
            _parent.ConvToggle_Group(entryID);
        }

        internal void EnumerateConversation() 
        {
            var folderList = _itemViewer.CboFolders.Items.Cast<object>().Select(item => item.ToString()).ToArray();
            _parent.ConvToggle_UnGroup(ConversationItems,
                                       Mail.EntryID,
                                       ConversationItems.Count,
                                       folderList);
        }

        public Dictionary<string, System.Action> RightKeyActions { get => new() 
        {
            { "&Pop Out", ()=>this._parent.PopOutControlGroup(Position)},
            { "&Expand", ()=>{_itemViewer.lblSubject.Focus(); this.EnumerateConversation(); } },
            { "&Cancel", ()=>{ } }
        }; }

        public void MoveMail()
        {
            if (Mail is not null)
            {
                IList<MailItem> selItems = PackageItems();
                bool attchments = (SelectedFolder != "Trash to Delete") ? false : _itemViewer.CbxAttachments.Checked;

                //LoadCTFANDSubjectsANDRecents.Load_CTF_AND_Subjects_AND_Recents();
                SortItemsToExistingFolder.MASTER_SortEmailsToExistingFolder(selItems: selItems,
                                                                            Pictures_Checkbox: false,
                                                                            SortFolderpath: _itemViewer.CboFolders.SelectedItem as string,
                                                                            Save_MSG: _itemViewer.CbxEmailCopy.Checked,
                                                                            Attchments: attchments,
                                                                            Remove_Flow_File: false,
                                                                            AppGlobals: _globals,
                                                                            StrRoot: _globals.Ol.ArchiveRootPath);
                SortItemsToExistingFolder.Cleanup_Files();
                // blDoMove
            }
        }

        internal IList<MailItem> PackageItems()
        {
            if (_itemViewer.CbxConversation.Checked == true)
            {
                var conversationCount = int.Parse(_itemViewer.LblConvCt.Text);
                if ((_conversationItems is not null) && 
                    (_conversationItems.Count == conversationCount) && 
                    (_conversationItems.Count != 0))
                {
                    return _conversationItems;
                }
                else
                {
                    if ((_dfConversation is null) || (_dfConversation.Rows.Count != conversationCount))
                    {
                        _dfConversation = Mail.GetConversationDf(true, true);
                    }
                    _conversationItems = ConvHelper.GetMailItemList(_dfConversation,
                                                                   ((Folder)Mail.Parent).StoreID,
                                                                   _globals.Ol.App,
                                                                   true)
                                                   .Cast<MailItem>().ToList();

                    return _conversationItems;
                }
            }
            else
            {
                return new List<MailItem> { Mail };
            }
        }
               
        // TODO: Implement FlagAsTask
        public void FlagAsTask()
        {
            throw new NotImplementedException();
        }
        
        public void MarkItemForDeletion()
        {
            if (!_itemViewer.CboFolders.Items.Contains("Trash to Delete"))
            {
                _itemViewer.CboFolders.Items.Add("Trash to Delete");
            }
            _itemViewer.CboFolders.SelectedItem = "Trash to Delete";
        }

        #endregion
    }
}
