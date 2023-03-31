using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuickFiler.QuickFileController;
using UtilitiesVB;

namespace QuickFiler
{    
    internal class QfcFormController
    {
        public delegate void ParentCleanupMethod();
        
        private IApplicationGlobals _globals;
        private ParentCleanupMethod _parentCleanup;
        private QfcFormViewer _viewer;


        public QfcFormController(
            IApplicationGlobals AppGlobals,
            QfcFormViewer Viewer,
            Queue<MailItem> ListEmailsInFolder,
            ParentCleanupMethod ParentCleanup)
        { 
            _globals = AppGlobals;
        }
    }
}
