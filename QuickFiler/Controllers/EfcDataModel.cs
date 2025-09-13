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
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using System.Net.Mail;

namespace QuickFiler.Controllers
{
    internal class EfcDataModel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Initializers

        public EfcDataModel(IApplicationGlobals globals, MailItem mail, CancellationTokenSource tokenSource, CancellationToken token)
        {
            Globals = globals;
            Token = token;
            TokenSource = tokenSource;
            Mail = mail ?? TryGetFirstInSelection();
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
                dataModel.ConversationResolver.Parent = dataModel;
            }
            else
            {
                dataModel.ConversationResolver = await ConversationResolver.LoadAsync(globals, mailItems[0], tokenSource, token, loadAll);
                dataModel.ConversationResolver.Parent = dataModel;
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

        private FolderPredictor _folderHelper;
        public FolderPredictor FolderHelper 
        {
            get 
            { 
                //_folderHelper ??= new OlFolderHelper(Globals, MailInfo, OlFolderHelper.InitOptions.FromField);
                return _folderHelper; 
            }
            protected set => _folderHelper = value;
        }
        async public Task InitFolderHandlerAsync(object folderList = null)
        {
            if (folderList is null)
            {
                if (MailInfo is null) 
                {
                    FolderHelper = await Task.Run(() => new FolderPredictor(Globals), Token);
                }
                else
                {
                    FolderHelper = await Task.Run(async () => await new FolderPredictor(
                        Globals, MailInfo, FolderPredictor.InitOptions.FromField)
                        .InitAsync(MailInfo, FolderPredictor.InitOptions.FromField), Token);
                }
            }
            else
            {
                FolderHelper = await Task.Run(async () => await new FolderPredictor(
                    Globals, folderList, FolderPredictor.InitOptions.FromArrayOrString)
                    .InitAsync(folderList, FolderPredictor.InitOptions.FromArrayOrString), Token);
            }
        }

        ConversationResolver _conversationResolver;
        public ConversationResolver ConversationResolver { get => _conversationResolver; protected set => _conversationResolver = value; }

        private MailItem _mail;
        public MailItem Mail
        {
            get
            {
                _mail ??= TryGetFirstInSelection();
                return _mail;
            }
            set => _mail = value;
        }
                
        public MailItemHelper MailInfo => ConversationResolver?.MailHelper;

        private MailItem TryGetFirstInSelection() 
        {
            try
            {
                var selection = _globals.Ol.App.ActiveExplorer().Selection;
                if ((selection?.Count ?? 0) > 0)
                {
                    return selection[1] as MailItem;
                }
                else { return null; }
            }
            catch (System.Exception)
            {
                return null;
            }
        }

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
                
                bool attachments = (folderpath != "Trash to Delete") ? saveAttachments : false;
                var mailHelpers = moveConversation ? ConversationResolver.ConversationInfo.SameFolder : new List<MailItemHelper>() { MailInfo };

                if (!Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot)) 
                {
                    logger.Debug($"Cannot sort without OneDrive location");
                    return;
                }
                var config = new EmailFilerConfig()
                {
                    SaveMsg = saveEmail,
                    SaveAttachments = attachments,
                    SavePictures = savePictures,
                    DestinationOlStem = folderpath,
                    Globals = Globals,
                    OlAncestor = Globals.Ol.ArchiveRootPath,
                    FsAncestorEquivalent = folderRoot,
                };
            
                var sorter = new EmailFiler(config);
                await sorter.SortAsync(mailHelpers);

                SortEmail.Cleanup_Files();
            }
        }

        internal async Task OpenOlFolderAsync(string folderpath)
        {
            if (!Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive)) { return; }
              
            var config = new EmailFilerConfig()
            {
                DestinationOlStem = folderpath,
                Globals = Globals,
                OlAncestor = Globals.Ol.ArchiveRootPath,
                FsAncestorEquivalent = oneDrive,
            };

            var sorter = new EmailFiler(config);
            await sorter.OpenOlFolderAsync();
        }

        internal async Task OpenFsFolderAsync(string folderpath)
        {
            if (!Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive)) { return; }
            var config = new EmailFilerConfig()
            {
                DestinationOlStem = folderpath,
                Globals = Globals,
                OlAncestor = Globals.Ol.ArchiveRootPath,
                FsAncestorEquivalent = oneDrive
            };

            var sorter = new EmailFiler(config);
            await sorter.OpenFileSystemFolderAsync();
        }

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
            //_folderHelper.Suggestions.Vlog.SetVerbose(new List<string> { "RefreshSuggestions","AddWordSequenceSuggestions" });
            _folderHelper.RefreshSuggestions(mailItem: Mail);
        }
                

        #endregion Public Methods

    }
}
