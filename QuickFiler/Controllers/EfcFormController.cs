using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal class EfcFormController : IFilerFormController
    {
        #region Constructors, Initializers, and Destructors

        public EfcFormController(IApplicationGlobals AppGlobals,
                                 MailItem _mailItem,
                                 EfcViewer formViewer,
                                 System.Action ParentCleanup,
                                 Enums.InitTypeEnum initType)
        {
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _formViewer = formViewer;
        }

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private EfcViewer _formViewer;

        public void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _parentCleanup.Invoke();
        }

        #endregion

        #region Public Properties

        public IntPtr FormHandle => throw new NotImplementedException();


        #endregion

        #region Event Handlers

        public void ButtonCancel_Click()
        {
            throw new NotImplementedException();
        }

        public void ButtonCancel_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void ButtonOK_Click()
        {
            throw new NotImplementedException();
        }

        public void ButtonOK_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper Methods

        public void MaximizeQfcFormViewer()
        {
            throw new NotImplementedException();
        }

        public void MinimizeQfcFormViewer()
        {
            throw new NotImplementedException();
        }

        public void ToggleOffNavigation(bool async)
        {
            throw new NotImplementedException();
        }

        public void ToggleOnNavigation(bool async)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}