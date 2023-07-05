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

namespace QuickFiler.Controllers
{
    internal class QfcItemController : IQfcItemController
    {
        #region constructors

        public QfcItemController(IApplicationGlobals AppGlobals,
                                 QfcItemViewer itemViewer,
                                 int viewerPosition,
                                 MailItem mailItem,
                                 IQfcCollectionController parent)
        {
            _globals = AppGlobals;

            // Grab handle on viewer and controls
            _itemViewer = itemViewer;
            ResolveControlGroups(itemViewer);

            _viewerPosition = viewerPosition;   // visible position in collection (index is 1 less)
            _mailItem = mailItem;               // handle on underlying Email
            _parent = parent;                   // handle on collection controller

            // Populate placeholder controls with 
            PopulateControls(mailItem, viewerPosition);
            
            _themes = ThemeHelper.SetupThemes(this, _itemViewer);

            ToggleTips(IQfcTipsDetails.ToggleState.Off);

        }

        #endregion

        #region private fields and variables

        private IApplicationGlobals _globals;
        private QfcItemViewer _itemViewer;
        private IQfcCollectionController _parent;
        private IList<IQfcTipsDetails> _listTipsDetails;
        private MailItem _mailItem;
        private DataFrame _dfConversation;
        private IList _conversationItems;
        public DataFrame DfConversation { get { return _dfConversation; } }
        private int _viewerPosition;
        private FolderHandler _fldrHandler;
        private IList<Control> _controls;
        private IList<TableLayoutPanel> _tlps;
        private IList<Button> _buttons;
        private IList<CheckBox> _checkBoxes;
        private IList<Label> _labels;
        private bool _expanded = false;
        private bool _blHasChild;
        private Dictionary<string,Theme> _themes;

        #endregion

        #region Exposed properties

        public string SelectedFolder { get => _itemViewer.CboFolders.SelectedItem.ToString(); }

        public int Position { get => _viewerPosition; set => _viewerPosition = value; }

        public string Subject { get => _itemViewer.lblSubject.Text; }

        public string To { get => _mailItem.To; }

        public string Sender { get => _itemViewer.LblSender.Text; }

        public string SentDate { get => _mailItem.SentOn.ToString("MM/dd/yyyy"); }

        public string SentTime { get => _mailItem.SentOn.ToString("HH:mm"); }

        public bool BlExpanded { get => _expanded; }

        public bool BlHasChild { get => _blHasChild; set => _blHasChild = value; }

        public int Height { get => _itemViewer.Height; }

        public MailItem Mail { get => _mailItem; set => _mailItem = value; }

        public IList<TableLayoutPanel> Tlps { get => _tlps;}

        public IList<Button> Buttons { get => _buttons;}

        public IList<IQfcTipsDetails> ListTipsDetails { get => _listTipsDetails;}

        #endregion

        #region completed functions

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

        internal void ResolveControlGroups(QfcItemViewer itemViewer)
        {
            var ctrls = itemViewer.GetAllChildren();
            _controls = ctrls.ToList();
                        
            _listTipsDetails = _itemViewer.TipsLabels
                               .Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                               .ToList();

            _tlps = ctrls.Where(x => x is TableLayoutPanel)
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
        //    Type columnDataType = ColumnData[0].GetType();
        //    // Use reflection to create an instance of the PrimitiveDataFrameColumn<T> class with the correct type parameter
        //    Type columnType = typeof(PrimitiveDataFrameColumn<>).MakeGenericType(columnDataType);
        //    column = (DataFrameColumn)Activator.CreateInstance(columnType, ColumnName);
        //    for (int i = 0; i < ColumnData.Length; i++)
        //    {
        //        column[i] = Convert.ChangeType(ColumnData[i], columnDataType);
        //    }

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

            _itemViewer.CboFolders.Items.AddRange(_fldrHandler.FolderList);
            _itemViewer.CboFolders.SelectedIndex = 1;
        }

        public void MoveMail()
        {
            if (Mail is not null)
            {
                IList selItems = PackageItems();
                bool Attchments = (SelectedFolder != "Trash to Delete") ? false : _itemViewer.CbxAttachments.Checked;

                //LoadCTFANDSubjectsANDRecents.Load_CTF_AND_Subjects_AND_Recents();
                SortItemsToExistingFolder.MASTER_SortEmailsToExistingFolder(selItems: selItems,
                                                                            Pictures_Checkbox: false,
                                                                            SortFolderpath: _itemViewer.CboFolders.SelectedItem as string,
                                                                            Save_MSG: _itemViewer.CbxEmailCopy.Checked,
                                                                            Attchments: Attchments,
                                                                            Remove_Flow_File: false,
                                                                            AppGlobals: _globals,
                                                                            StrRoot: _globals.Ol.ArchiveRootPath);
                SortItemsToExistingFolder.Cleanup_Files();
                // blDoMove
            }
        }

        internal IList PackageItems()
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
                                                                   true);

                    return _conversationItems;
                }
            }
            else
            {
                return new List<MailItem> { Mail };
            }
        }

        public void SetThemeDark()
        {
            _themes["DarkNormal"].SetTheme();
        }

        public void SetThemeLight()
        {
            _themes["LightNormal"].SetTheme();
        }

        public void ToggleTips()
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                tipsDetails.Toggle();
            }
        }
        
        public void ToggleTips(IQfcTipsDetails.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                tipsDetails.Toggle(desiredState);
            }
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

        // TODO: Implement Accel_FocusToggle
        public void Accel_FocusToggle()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement Accel_Toggle
        public void Accel_Toggle()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement ApplyReadEmailFormat
        public void ApplyReadEmailFormat()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement ctrlsRemove
        public void ctrlsRemove()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement ExpandCtrls1
        public void ExpandCtrls1()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement FlagAsTask
        public void FlagAsTask()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement JumpToFolderDropDown
        public void JumpToFolderDropDown()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement JumpToSearchTextbox
        public void JumpToSearchTextbox()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement MarkItemForDeletion
        public void MarkItemForDeletion()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement ToggleConversationCheckbox
        public void ToggleConversationCheckbox()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement ToggleDeleteFlow
        public void ToggleDeleteFlow()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement ToggleSaveAttachments
        public void ToggleSaveAttachments()
        {
            throw new NotImplementedException();
        }

        // TODO: Implement ToggleSaveCopyOfMail
        public void ToggleSaveCopyOfMail()
        {
            throw new NotImplementedException();
        }

    }
}
