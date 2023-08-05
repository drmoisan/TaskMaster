using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using ToDoModel;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal class EfcDataModel
    {
        public EfcDataModel(IApplicationGlobals appGlobals, MailItem mail)
        {
            _globals = appGlobals;
            _mail = mail;
            if (Mail is not null)
            {
                _conversationResolver = new ConversationResolver(_globals, Mail);
                _ = _conversationResolver.ResolveItems();
            }
        }

        private IApplicationGlobals _globals;

        ConversationResolver _conversationResolver;
        public ConversationResolver ConversationResolver { get => _conversationResolver; }

        private MailItem _mail;
        public MailItem Mail
        {
            get
            {
                if (_mail is null)
                    _mail = _globals.Ol.App.ActiveExplorer().Selection[1] as MailItem;
                return _mail;
            }
            set => _mail = value;
        }

        public void MoveToFolder(string folderpath, 
                                 bool saveAttachments,
                                 bool saveEmail,
                                 bool savePictures,
                                 bool moveConversation)
        {
            if (Mail is not null)
            {
                IList<MailItem> items = PackageItems(moveConversation);
                bool attchments = (folderpath != "Trash to Delete") ? false : saveAttachments;

                //LoadCTFANDSubjectsANDRecents.Load_CTF_AND_Subjects_AND_Recents();
                SortItemsToExistingFolder.Run(selItems: items,
                                              picturesCheckbox: savePictures,
                                              sortFolderpath: folderpath,
                                              saveMsg: saveEmail,
                                              attchments: attchments,
                                              removeFlowFile: false,
                                              appGlobals: _globals,
                                              strRoot: _globals.Ol.ArchiveRootPath);
                SortItemsToExistingFolder.Cleanup_Files();
                // blDoMove
            }
            //stackMovedItems.Push(grp.MailItem);
        }

        public IList<MailItem> PackageItems(bool moveConversation)
        {
            if (moveConversation) { return _conversationResolver.ConversationItems; }
            else { return new List<MailItem>() { Mail };}
        }



    }
}
