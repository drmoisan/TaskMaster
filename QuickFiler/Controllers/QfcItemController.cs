using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private IDictionary<object, string> _conversation;
        private int _viewerPosition;

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
            _itemViewer.LblConvCt.Text = _mailItem.ConversationCt(true, true).ToString();
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

        public void PopulateFolderCombobox(object varList = null)
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
