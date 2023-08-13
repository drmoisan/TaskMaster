using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                _ = Task.Run(async ()=> _conversationResolver.ConversationItems = await _conversationResolver.ResolveItems());
            }
        }

        private IApplicationGlobals _globals;
        
        private FolderHandler _folderHandler;
        public FolderHandler FolderHandler { get => _folderHandler; }
        async public Task InitFolderHandler(object folderList = null)
        {
            if (folderList is null)
            {
                _folderHandler = await Task.Run(() => new FolderHandler(
                    _globals, _mail, FolderHandler.InitOptions.FromField));
            }
            else
            {
                _folderHandler = await Task.Run(() => new FolderHandler(
                    _globals, folderList, FolderHandler.InitOptions.FromArrayOrString));
            }
        }

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

        async public Task MoveToFolder(string folderpath, 
                                       bool saveAttachments,
                                       bool saveEmail,
                                       bool savePictures,
                                       bool moveConversation)
        {
            if (Mail is not null)
            {
                IList<MailItem> items = PackageItems(moveConversation);
                bool attchments = (folderpath != "Trash to Delete") ? saveAttachments : false;

                //LoadCTFANDSubjectsANDRecents.Load_CTF_AND_Subjects_AND_Recents();
                await SortEmail.Run(mailItems: items,
                                                    savePictures: savePictures,
                                                    destinationOlStem: folderpath,
                                                    saveMsg: saveEmail,
                                                    saveAttachments: attchments,
                                                    removePreviousFsFiles: false,
                                                    appGlobals: _globals,
                                                    olAncestor: _globals.Ol.ArchiveRootPath,
                                                    fsAncestorEquivalent: _globals.FS.FldrRoot);
                SortEmail.Cleanup_Files();
                // blDoMove
            }
            //stackMovedItems.Push(grp.MailItem);
        }

        async public Task MoveToFolder(MAPIFolder folder,
                                       string olAncestor,
                                       bool saveAttachments,
                                       bool saveEmail,
                                       bool savePictures,
                                       bool moveConversation)
        {
                var folderpath = folder.FolderPath.Replace(olAncestor,"");
                await MoveToFolder(folderpath, saveAttachments, saveEmail, savePictures, moveConversation);
        }

        public IList<MailItem> PackageItems(bool moveConversation)
        {
            if (moveConversation) { return _conversationResolver.ConversationItems; }
            else { return new List<MailItem>() { Mail };}
        }

        public string[] FindMatches(string searchText)
        {
            if (searchText != "")
            {
                searchText = "*" + searchText + "*";
            }

            return _folderHandler.FindFolder(
                        searchString: searchText,
                        reloadCTFStagingFiles: false,
                        recalcSuggestions: false,
                        objItem: _mail);
        }

        public void RefreshSuggestions()
        {
            _folderHandler.Suggestions.RefreshSuggestions(Mail, _globals, false);
        }
    }
}
