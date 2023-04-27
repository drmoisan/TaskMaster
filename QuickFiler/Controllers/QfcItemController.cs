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
using UtilitiesVB;

namespace QuickFiler.Controllers
{
    internal class QfcItemController : IQfcItemController
    {
        public QfcItemController(IApplicationGlobals AppGlobals, 
                                 QfcItemViewer itemViewer,
                                 int viewerPosition,
                                 MailItem mailItem,
                                 IQfcCollectionController parent) 
        {
            _globals = AppGlobals;
            _itemViewer = itemViewer;
            _viewerPosition = viewerPosition;
            _mailItem = mailItem;
            _parent = parent;
            _listTipsDetails = _itemViewer.TipsLabels.Select(x => (IQfcTipsDetails)new QfcTipsDetails(x)).ToList();
            PopulateControls();
            ToggleTips(IQfcTipsDetails.ToggleState.Off);
        }

        private IApplicationGlobals _globals;
        private QfcItemViewer _itemViewer;
        private IQfcCollectionController _parent;
        private IList<IQfcTipsDetails> _listTipsDetails;
        private MailItem _mailItem;
        private DataFrame _dfConversation;
        public DataFrame DfConversation { get { return _dfConversation; } }
        private int _viewerPosition;
        private FolderHandler _fldrHandler;

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

        internal void PopulateControls()
        {
            _itemViewer.LblSender.Text = CaptureEmailDetailsModule.GetSenderName(_mailItem);
            _itemViewer.lblSubject.Text = _mailItem.Subject;
            _itemViewer.TxtboxBody.Text = CompressPlainText(_mailItem.Body);
            _itemViewer.LblTriage.Text = CaptureEmailDetailsModule.GetTriage(_mailItem);
            _itemViewer.LblSentOn.Text = _mailItem.SentOn.ToString("g");
            _itemViewer.LblActionable.Text = CaptureEmailDetailsModule.GetActionTaken(_mailItem);
            //_itemViewer.LblConvCt.Text 
            _itemViewer.LblPos.Text = _viewerPosition.ToString();
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

        public bool BlExpanded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Height => throw new NotImplementedException();

        public bool BlHasChild { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object ObjItem { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public void Accel_FocusToggle()
        {
            throw new NotImplementedException();
        }

        public void Accel_Toggle()
        {
            throw new NotImplementedException();
        }

        public void ApplyReadEmailFormat()
        {
            throw new NotImplementedException();
        }

        public void ctrlsRemove()
        {
            throw new NotImplementedException();
        }

        public void ExpandCtrls1()
        {
            throw new NotImplementedException();
        }

        public void FlagAsTask()
        {
            throw new NotImplementedException();
        }

        public void JumpToFolderDropDown()
        {
            throw new NotImplementedException();
        }

        public void JumpToSearchTextbox()
        {
            throw new NotImplementedException();
        }

        public void MarkItemForDeletion()
        {
            throw new NotImplementedException();
        }

        public void ToggleConversationCheckbox()
        {
            throw new NotImplementedException();
        }

        public void ToggleDeleteFlow()
        {
            throw new NotImplementedException();
        }

        public void ToggleSaveAttachments()
        {
            throw new NotImplementedException();
        }

        public void ToggleSaveCopyOfMail()
        {
            throw new NotImplementedException();
        }
    }
}
