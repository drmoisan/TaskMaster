using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;


namespace QuickFiler.Controllers
{
    internal class QfcItemController : IQfcItemController
    {
        public QfcItemController(QfcItemViewer itemViewer, 
                                 QfcTipsDetails tipsDetails, 
                                 IQfcCollectionController parent) 
        { 
            _itemViewer = itemViewer;
            _tipsDetails = tipsDetails;
            _parent = parent;
        }
        
        private QfcItemViewer _itemViewer;
        private IQfcCollectionController _parent;
        private QfcTipsDetails _tipsDetails;


        internal void ResolveControlAssignments()
        {

        }
        
        public bool BlExpanded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Height => throw new NotImplementedException();

        public bool BlHasChild { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object ObjItem { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
