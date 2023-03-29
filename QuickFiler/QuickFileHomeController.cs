using ToDoModel;
using UtilitiesVB;
using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using UtilitiesCS;

namespace QuickFiler
{

    public class QuickFileHomeController
    {
        private QuickFileViewer _viewer;
        private QuickFileController _controller;
        private IApplicationGlobals _globals;
        public delegate void ParentCleanupFunction();
        private ParentCleanupFunction _parentCleanup;

        public QuickFileHomeController(IApplicationGlobals AppGlobals, ParentCleanupFunction ParentCleanup)
        {
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _viewer = new QuickFileViewer();
            
            var listEmailsInFolder = FolderSuggestionsModule.LoadEmailDataBase(_globals.Ol.App.ActiveExplorer()); //as List<MailItem>;
            Queue<MailItem> MasterQueue = new Queue<MailItem>();
            foreach (MailItem email in listEmailsInFolder) 
            { 
                MasterQueue.Enqueue(email);
            }
            _controller = new QuickFileController(_globals, _viewer, MasterQueue, Cleanup);
        }

        public void Run()
        {
            // _viewer.Show()
        }

        public bool Loaded
        {
            get
            {
                if (_viewer is not null)
                {
                    // If _viewer.IsDisposed = False Then
                    return true;
                }
                // Else
                // Return False
                // End If
                else
                {
                    return false;
                }
            }
        }

        internal void Cleanup()
        {
            _viewer = null;
            _controller = null;
            _globals = null;
            _parentCleanup.Invoke();
        }
    }
}