using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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
                                 MailItem mailItem,
                                 IQfcCollectionController parent) 
        {
            _globals = AppGlobals;
            _itemViewer = itemViewer;
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

        internal void PopulateControls()
        {
            string[] emailDetails = CaptureEmailDetailsModule.CaptureEmailDetails(_mailItem, _globals.Ol.EmailRootPath);
            _itemViewer.LblSender.Text = emailDetails[4];
            _itemViewer.lblSubject.Text = emailDetails[7];
            _itemViewer.TxtboxBody.Text = emailDetails[8];
            _itemViewer.LblTriage.Text = emailDetails[1];
            _itemViewer.LblSentOn.Text = emailDetails[3];
            _itemViewer.LblActionable.Text = emailDetails[13];

            //strAry(2) = GetEmailFolderPath(OlMail, emailRootFolder)
            //strAry(5) = recipients.recipientsTo
            //strAry(6) = recipients.recipientsCC
            //strAry(9) = Right(strAry(4), Len(strAry(4)) - InStr(strAry(4), "@"))
            //strAry(10) = OlMail.ConversationID
            //strAry(11) = OlMail.EntryID
            //strAry(12) = GetAttachmentNames(OlMail)
            
            
            //_itemViewer.LblConvCt
            //_itemViewer.LblPos
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
