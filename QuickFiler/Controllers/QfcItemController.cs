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
            _globals = AppGlobals;

            // Grab handle on viewer and controls
            _itemViewer = itemViewer;
            ResolveControlGroups(itemViewer);

            _viewerPosition = viewerPosition;   // visible position in collection (index is 1 less)
            _mailItem = mailItem;               // handle on underlying Email
            _keyboardHandler = keyboardHandler; // handle keystrokes
            _parent = parent;                   // handle on collection controller

            // Populate placeholder controls with 
            PopulateControls(mailItem, viewerPosition);
            
            _themes = ThemeHelper.SetupThemes(this, _itemViewer);
            
            ToggleTips(Enums.ToggleState.Off);
            ToggleNavigation(Enums.ToggleState.Off);
            WireEvents();
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
        private bool _active = false;
        private bool _blIsChild;
        private Dictionary<string,Theme> _themes;
        private string _activeTheme;
        private IQfcKeyboardHandler _keyboardHandler;
        private string _convOriginID = "";
        private bool _suppressEvents = false;
        private int _intEnterCounter = 0;
        private int _intComboRightCtr = 0;

        #endregion

        #region Exposed properties

        public bool BlExpanded { get => _expanded; }

        public bool BlIsChild { get => _blIsChild; set => _blIsChild = value; }

        public IList<Button> Buttons { get => _buttons; }

        public string ConvOriginID { get => _convOriginID; set => _convOriginID = value; }

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

        public IList<IQfcTipsDetails> ListTipsDetails { get => _listTipsDetails; }

        public MailItem Mail { get => _mailItem; set => _mailItem = value; }

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
            _itemViewer.LblSender.Text = CaptureEmailDetailsModule.GetSenderName(mailItem);
            _itemViewer.lblSubject.Text = mailItem.Subject;
            if (_mailItem.UnRead == true)
            {
                _itemViewer.LblSender.Font = new Font(_itemViewer.LblSender.Font, FontStyle.Bold);
                _itemViewer.lblSubject.Font = new Font(_itemViewer.lblSubject.Font, FontStyle.Bold);
            }
            _itemViewer.TxtboxBody.Text = CompressPlainText(mailItem.Body);
            _itemViewer.LblTriage.Text = CaptureEmailDetailsModule.GetTriage(mailItem);
            _itemViewer.LblSentOn.Text = mailItem.SentOn.ToString("g");
            _itemViewer.LblActionable.Text = CaptureEmailDetailsModule.GetActionTaken(mailItem);
            _itemViewer.LblPos.Text = viewerPosition.ToString();
            //_itemViewer.LblConvCt.Text 
            //_itemViewer.LblSearch
            //_itemViewer.BtnDelItem
            //_itemViewer.BtnPopOut
            //_itemViewer.BtnFlagTask
            //_itemViewer.LblFolder
            //_itemViewer.CboFolders
            //_itemViewer.CbxConversation
            //_itemViewer.CbxEmailCopy
            //_itemViewer.CbxAttachments
        }

        /// <summary>
        /// TBD if this overload will be of use. Depends on whether _dfConversation
        /// is needed by any individual element when expanded
        /// </summary>
        /// <param name="df"></param>
        public void PopulateConversation(DataFrame df)
        {
            _dfConversation = df;
            _itemViewer.LblConvCt.Text = _dfConversation.Rows.Count.ToString();
        }

        /// <summary>
        /// Sets the conversation count of the visual without altering the
        /// _dfConversation. Usefull when expanding or collapsing the 
        /// conversation to show how many items will be moved
        /// </summary>
        /// <param name="countOnly"></param>
        public void PopulateConversation(int countOnly)
        {
            _itemViewer.LblConvCt.Text = countOnly.ToString();
        }

        /// <summary>
        /// Gets the Outlook.Conversation from the underlying MailItem
        /// embedded in the class. Conversation details are loaded to 
        /// a Dataframe. Count is inferred from the df rowcount
        /// </summary>
        public void PopulateConversation()
        {
            _dfConversation = _mailItem.GetConversationDf(true, true);
            int count = _dfConversation.Rows.Count();
            _itemViewer.LblConvCt.Text = count.ToString();
            if (count == 0) { _itemViewer.LblConvCt.BackColor = Color.Red; }
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

            _itemViewer.CboFolders.Items.AddRange(_fldrHandler.FolderArray);
            _itemViewer.CboFolders.SelectedIndex = 1;
        }

        // TODO: Implement ctrlsRemove
        public void ctrlsRemove()
        {
            throw new NotImplementedException();
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
            _itemViewer.BtnDelItem.Click += new System.EventHandler(this.BtnDelItem_Click);
            _itemViewer.TxtboxSearch.TextChanged += new System.EventHandler(this.TxtboxSearch_TextChanged);
            _itemViewer.TxtboxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtboxSearch_KeyDown);
            _itemViewer.CboFolders.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CboFolders_KeyDown);
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
            _keyboardHandler.KdCharActions.Add('E', (x) => this.ExpandCtrls1());
            _keyboardHandler.KdCharActions.Add('S', (x) => this.JumpToSearchTextbox());
            _keyboardHandler.KdCharActions.Add('T', (x) => this.FlagAsTask());
            _keyboardHandler.KdCharActions.Add('P', (x) => this.PopOutItem());
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
        
        internal void BtnPopOut_Click(object sender, EventArgs e) => PopOutItem();

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

        internal void CboFolders_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    {
                        _intEnterCounter = 1;
                        _intComboRightCtr = 0;
                        break;
                    }
                case Keys.Up:
                    {
                        _intEnterCounter = 0;
                        break;
                    }
                case Keys.Down:
                    {
                        _intEnterCounter = 0;
                        break;
                    }
                case Keys.Right:
                    {
                        _intEnterCounter = 0;
                        switch (_intComboRightCtr)
                        {
                            case 0:
                                {
                                    _itemViewer.CboFolders.DroppedDown = true;
                                    _intComboRightCtr++;
                                    break;
                                }
                            case 1:
                                {
                                    _itemViewer.CboFolders.DroppedDown = false;
                                    _intComboRightCtr = 0;
                                    MyBox.ShowDialog("Pop Out Item or Enumerate Conversation?", 
                                        "Dialog", BoxIcon.Question, rightKeyActions);
                                    break;
                                }
                            default:
                                {
                                    MessageBox.Show(
                                        "Error in intComboRightCtr ... setting to 0 and continuing",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                    _intComboRightCtr = 0;
                                    break;
                                }
                        }
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        break;
                    }
                case Keys.Left:
                    {
                        _intEnterCounter = 0;
                        _intComboRightCtr = 0;
                        if (_itemViewer.CboFolders.DroppedDown)
                        {
                            _itemViewer.CboFolders.DroppedDown = false;
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        else { _keyboardHandler.KeyboardHandler_KeyDown(sender, e); }
                        
                        break;
                    }
                case Keys.Return:
                    {
                        _intEnterCounter++;
                        if (_intEnterCounter == 1)
                        {
                            _intEnterCounter = 0;
                            _intComboRightCtr = 0;
                            _keyboardHandler.KeyboardHandler_KeyDown(sender, e);
                        }
                        else
                        {
                            _intEnterCounter = 1;
                            _intComboRightCtr = 0;
                            e.Handled = true;
                        }
                        
                        break;
                    }
            }            
        }

        #endregion

        #region UI Navigation Methods

        public void ToggleNavigation()
        {
            _itemPositionTips.Toggle(true);
        }

        public void ToggleNavigation(Enums.ToggleState desiredState)
        {
            _itemPositionTips.Toggle(desiredState, true);
        }

        public void ToggleTips()
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                tipsDetails.Toggle();
            }
        }

        public void ToggleTips(Enums.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                tipsDetails.Toggle(desiredState);
            }
        }

        public void Accel_FocusToggle()
        {
            if (_active) 
            {
                // If active, then we are turning off
                _active = false;
                if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkNormal"; }
                else { _activeTheme = "LightNormal";}
                ToggleTips(Enums.ToggleState.Off);
                UnregisterFocusActions();
            }
            else 
            { 
                // If not active, then we are turning on
                _active = true;
                if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkActive"; }
                else { _activeTheme = "LightActive"; }
                ToggleTips(Enums.ToggleState.On);
                RegisterFocusActions();
            }
            _themes[_activeTheme].SetTheme();
        }

        public void Accel_Toggle()
        {
            if (_active) { Accel_FocusToggle(); }
            ToggleNavigation();
        }

        // TODO: Implement ExpandCtrls1
        public void ExpandCtrls1()
        {
            throw new NotImplementedException();
        }

        public void JumpToFolderDropDown()
        {
            _keyboardHandler.ToggleKeyboardDialog();
            _parent.ToggleOffNavigation();
            _itemViewer.CboFolders.Focus();
            _itemViewer.CboFolders.DroppedDown = true;
            _intEnterCounter = 0;
        }

        public void JumpToSearchTextbox()
        {
            _keyboardHandler.ToggleKeyboardDialog();
            _itemViewer.TxtboxSearch.Focus();
        }

        /// <summary>
        /// Function programatically clicks the "Conversation" checkbox
        /// </summary>
        public void ToggleConversationCheckbox()
        {
            _itemViewer.CbxConversation.Checked = !_itemViewer.CbxConversation.Checked;
        }

        /// <summary>
        /// Function programatically sets the "Conversation" checkbox to the desired state 
        /// if it is not already in that state
        /// </summary>
        /// <param name="desiredState">State of checkbox desired</param>
        public void ToggleConversationCheckbox(Enums.ToggleState desiredState)
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
        }

        // TODO: Implement ToggleDeleteFlow
        public void ToggleDeleteFlow()
        {
            throw new NotImplementedException();
        }

        public void ToggleSaveAttachments()
        {
            _itemViewer.CbxAttachments.Checked = !_itemViewer.CbxAttachments.Checked;
        }

        public void ToggleSaveCopyOfMail()
        {
            _itemViewer.CbxEmailCopy.Checked = !_itemViewer.CbxEmailCopy.Checked;
        }

        #endregion

        #region UI Visual Helper Methods

        public void SetThemeDark()
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["DarkNormal"].SetTheme();
                _activeTheme = "DarkNormal";
            }
            else
            {
                _themes["DarkActive"].SetTheme();
                _activeTheme = "DarkActive";
            }
        }

        public void SetThemeLight()
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["LightNormal"].SetTheme();
                _activeTheme = "LightNormal";
            }
            else
            {
                _themes["LightActive"].SetTheme();
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

        internal string CompressPlainText(string text)
        {
            //text = text.Replace(System.Environment.NewLine, " ");
            text = text.Replace(Properties.Resources.Email_Prefix_To_Strip, "");
            text = Regex.Replace(text, @"<https://[^>]+>", " <link> "); //Strip links
            text = Regex.Replace(text, @"[\s]", " ");
            text = Regex.Replace(text, @"[ ]{2,}", " ");
            text = text.Trim();
            text += " <EOM>";
            return text;
        }

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

        internal Dictionary<string, System.Action> rightKeyActions { get => new() 
        {
            { "&Pop Out", ()=>this.PopOutItem()},
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
        
        public void PopOutItem() => _parent.PopOutControlGroup(Position);
        
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
