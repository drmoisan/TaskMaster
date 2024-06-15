using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Extensions;

namespace QuickFiler.Controllers
{
    internal class EfcDataModel
    {
        #region Constructors and Initializers

        public EfcDataModel(IApplicationGlobals globals, MailItem mail, CancellationTokenSource tokenSource, CancellationToken token)
        {
            Globals = globals;
            Token = token;
            TokenSource = tokenSource;
            Mail = mail ?? Globals.Ol.App.ActiveExplorer().Selection[1] as MailItem; 
            if (Mail is not null)
            {
                ConversationResolver = new ConversationResolver(Globals, Mail, TokenSource, Token);
                _conversationResolver.Df = _conversationResolver.LoadDf(); // Load Synchronously
                
            }
        }

        private EfcDataModel(IApplicationGlobals globals, MailItem mail)
        {
            Globals = globals;
            Mail = mail;
        }

        public async static Task<EfcDataModel> CreateAsync(
            IApplicationGlobals globals, 
            IList<MailItem> mailItems, 
            CancellationTokenSource tokenSource, 
            CancellationToken token, 
            bool loadAll)
        {
            globals.ThrowIfNull(nameof(globals));
            mailItems.ThrowIfNullOrEmpty(nameof(mailItems));

            
            var dataModel = new EfcDataModel(globals, mailItems[0]);
            if (mailItems.Count() > 1)
            {
                dataModel.ConversationResolver = await ConversationResolver.LoadAsync(globals, mailItems, tokenSource, token);
            }
            else
            {
                dataModel.ConversationResolver = await ConversationResolver.LoadAsync(globals, mailItems[0], tokenSource, token, loadAll);
            }
            
            return dataModel;
        }


        #endregion Constructors and Initializers

        #region Public Properties

        private IApplicationGlobals _globals;
        public IApplicationGlobals Globals { get => _globals; protected set => _globals = value; }

        private CancellationToken _token;
        public CancellationToken Token { get => _token; protected set => _token = value; }

        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource { get => _tokenSource; protected set => _tokenSource = value; }

        private OlFolderHelper _folderHelper;
        public OlFolderHelper FolderHelper 
        {
            get 
            { 
                _folderHelper ??= new OlFolderHelper(Globals, MailInfo, OlFolderHelper.InitOptions.FromField);
                return _folderHelper; 
            }
            protected set => _folderHelper = value;
        }
        async public Task InitFolderHandlerAsync(object folderList = null)
        {
            if (folderList is null)
            {
                FolderHelper = await Task.Run(() => new OlFolderHelper(
                    Globals, MailInfo, OlFolderHelper.InitOptions.FromField), Token);
            }
            else
            {
                FolderHelper = await Task.Run(() => new OlFolderHelper(
                    Globals, folderList, OlFolderHelper.InitOptions.FromArrayOrString), Token);
            }
        }

        ConversationResolver _conversationResolver;
        public ConversationResolver ConversationResolver { get => _conversationResolver; protected set => _conversationResolver = value; }

        private MailItem _mail;
        public MailItem Mail
        {
            get
            {
                _mail ??= _globals.Ol.App.ActiveExplorer().Selection[1] as MailItem;
                return _mail;
            }
            set => _mail = value;
        }
                
        public MailItemHelper MailInfo => ConversationResolver.MailInfo;
        

        #endregion Public Properties

        #region Public Methods

        async public Task MoveToFolderAsync(string folderpath, 
                                       bool saveAttachments,
                                       bool saveEmail,
                                       bool savePictures,
                                       bool moveConversation)
        {
            if (MailInfo is not null)
            {
                
                bool attchments = (folderpath != "Trash to Delete") ? saveAttachments : false;
                var mailHelpers = moveConversation ? ConversationResolver.ConversationInfo.SameFolder : new List<MailItemHelper>() { MailInfo };
                await SortEmail.SortAsync(mailHelpers: mailHelpers,
                                         savePictures: savePictures,
                                         destinationOlStem: folderpath,
                                         saveMsg: saveEmail,
                                         saveAttachments: attchments,
                                         removePreviousFsFiles: false,
                                         appGlobals: _globals,
                                         olAncestor: _globals.Ol.ArchiveRootPath,
                                         fsAncestorEquivalent: _globals.FS.FldrRoot);
                SortEmail.Cleanup_Files();
            }
        }

        //async public Task MoveToFolder(string folderpath,
        //                               bool saveAttachments,
        //                               bool saveEmail,
        //                               bool savePictures,
        //                               bool moveConversation)
        //{
        //    if (Mail is not null)
        //    {
        //        IList<MailItem> items = PackageItems(moveConversation);
        //        bool attchments = (folderpath != "Trash to Delete") ? saveAttachments : false;

        //        //LoadCTFANDSubjectsANDRecents.Load_CTF_AND_Subjects_AND_Recents();
        //        await SortEmail.SortAsync(mailItems: items,
        //                                 savePictures: savePictures,
        //                                 destinationOlStem: folderpath,
        //                                 saveMsg: saveEmail,
        //                                 saveAttachments: attchments,
        //                                 removePreviousFsFiles: false,
        //                                 appGlobals: _globals,
        //                                 olAncestor: _globals.Ol.ArchiveRootPath,
        //                                 fsAncestorEquivalent: _globals.FS.FldrRoot);
        //        SortEmail.Cleanup_Files();
        //        // blDoMove
        //    }
        //}

        async public Task MoveToFolder(MAPIFolder folder,
                                       string olAncestor,
                                       bool saveAttachments,
                                       bool saveEmail,
                                       bool savePictures,
                                       bool moveConversation)
        {
            var folderpath = folder.FolderPath.Replace(olAncestor,"");
            if (folderpath.StartsWith(@"\"))
            {
                folderpath = folderpath.Substring(1);
            }
            await MoveToFolderAsync(folderpath, saveAttachments, saveEmail, savePictures, moveConversation);
        }

        public IList<MailItem> PackageItems(bool moveConversation)
        {
            if (moveConversation) { return _conversationResolver.ConversationItems.SameFolder; }
            else { return new List<MailItem>() { Mail };}
        }

        public string[] FindMatches(string searchText)
        {
            if (searchText != "")
            {
                searchText = "*" + searchText + "*";
            }

            return _folderHelper.FindFolder(
                        searchString: searchText,
                        reloadCTFStagingFiles: false,
                        recalcSuggestions: false,
                        objItem: _mail);
        }

        public void RefreshSuggestions()
        {
            _folderHelper.Suggestions.Vlog.SetVerbose(new List<string> { "RefreshSuggestions","AddWordSequenceSuggestions" });
            _folderHelper.RefreshSuggestions(mailItem: Mail, topNfolderKeys: 1);
        }

        #endregion Public Methods

    }
}
