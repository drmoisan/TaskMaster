using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;

namespace QuickFiler.Controllers
{
    internal class EfcDataModel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private static string DescribeSynchronizationContext(SynchronizationContext syncContext)
        {
            return syncContext?.GetType().FullName ?? "null";
        }

        private static string BuildDataModelTimingContext()
        {
            return $"threadId={Thread.CurrentThread.ManagedThreadId}; syncContext={DescribeSynchronizationContext(SynchronizationContext.Current)}";
        }

        private static void LogDataModelTiming(string phase, string details = null)
        {
            var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
            var phaseLabel = phase.StartsWith("[Data model timing]", StringComparison.Ordinal)
                ? phase
                : $"[Data model timing] {phase}";
            logger.Debug($"{phaseLabel} | {BuildDataModelTimingContext()}{detailSegment}");
        }

        #region Constructors and Initializers

        public EfcDataModel(
            IApplicationGlobals globals,
            MailItem mail,
            CancellationTokenSource tokenSource,
            CancellationToken token
        )
        {
            var constructorStopwatch = Stopwatch.StartNew();
            LogDataModelTiming("EfcDataModel constructor load start | constructor load");
            Globals = globals;
            Token = token;
            TokenSource = tokenSource;
            Mail = mail ?? TryGetFirstInSelection();
            if (Mail is not null)
            {
                LogDataModelTiming(
                    "EfcDataModel constructor snapshot load start | constructor load",
                    "constructor snapshot load"
                );
                ConversationResolver = new ConversationResolver(Globals, Mail, TokenSource, Token);
                _conversationResolver.Df = _conversationResolver.LoadDf(); // Load Synchronously
                LogDataModelTiming(
                    "EfcDataModel constructor snapshot load complete | constructor load",
                    $"constructor snapshot load elapsedMs={constructorStopwatch.ElapsedMilliseconds}"
                );
            }

            LogDataModelTiming(
                "EfcDataModel constructor load complete | constructor load",
                $"mailLoaded={Mail is not null}; elapsedMs={constructorStopwatch.ElapsedMilliseconds}"
            );
        }

        private EfcDataModel(IApplicationGlobals globals, MailItem mail)
        {
            Globals = globals;
            Mail = mail;
        }

        public static async Task<EfcDataModel> CreateAsync(
            IApplicationGlobals globals,
            IList<MailItem> mailItems,
            CancellationTokenSource tokenSource,
            CancellationToken token,
            bool loadAll
        )
        {
            globals.ThrowIfNull(nameof(globals));
            mailItems.ThrowIfNullOrEmpty(nameof(mailItems));
            var mailSelectionSnapshot = mailItems.ToArray();

            var createStopwatch = Stopwatch.StartNew();
            LogDataModelTiming(
                "[Data model timing] CreateAsync snapshot load stage | snapshot load",
                $"mailItemCount={mailSelectionSnapshot.Length}"
            );

            var dataModel = new EfcDataModel(globals, mailSelectionSnapshot[0]);
            LogDataModelTiming(
                "[Data model timing] CreateAsync background initialization stage | background initialization",
                $"mailItemCount={mailSelectionSnapshot.Length}; loadAll={loadAll}"
            );
            // Freeze the selection membership during the snapshot load stage so the later
            // background initialization stage does not have to re-enumerate the live selection.
            if (mailSelectionSnapshot.Length > 1)
            {
                dataModel.ConversationResolver = await ConversationResolver.LoadAsync(
                    globals,
                    mailSelectionSnapshot,
                    tokenSource,
                    token
                );
                dataModel.ConversationResolver.Parent = dataModel;
            }
            else
            {
                dataModel.ConversationResolver = await ConversationResolver.LoadAsync(
                    globals,
                    mailSelectionSnapshot[0],
                    tokenSource,
                    token,
                    loadAll
                );
                dataModel.ConversationResolver.Parent = dataModel;
            }

            LogDataModelTiming(
                "CreateAsync model-ready publication | model-ready publication",
                $"mailItemCount={mailSelectionSnapshot.Length}; elapsedMs={createStopwatch.ElapsedMilliseconds}"
            );

            return dataModel;
        }

        #endregion Constructors and Initializers

        #region Public Properties

        private IApplicationGlobals _globals;
        public IApplicationGlobals Globals
        {
            get => _globals;
            protected set => _globals = value;
        }

        private CancellationToken _token;
        public CancellationToken Token
        {
            get => _token;
            protected set => _token = value;
        }

        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource
        {
            get => _tokenSource;
            protected set => _tokenSource = value;
        }

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

        public async Task InitFolderHandlerAsync(object folderList = null)
        {
            if (folderList is null)
            {
                if (MailInfo is null)
                {
                    FolderHelper = await Task.Run(() => new FolderPredictor(Globals), Token);
                }
                else
                {
                    FolderHelper = await Task.Run(
                        async () =>
                            await new FolderPredictor(
                                Globals,
                                MailInfo,
                                FolderPredictor.InitOptions.FromField
                            ).InitAsync(MailInfo, FolderPredictor.InitOptions.FromField),
                        Token
                    );
                }
            }
            else
            {
                FolderHelper = await Task.Run(
                    async () =>
                        await new FolderPredictor(
                            Globals,
                            folderList,
                            FolderPredictor.InitOptions.FromArrayOrString
                        ).InitAsync(folderList, FolderPredictor.InitOptions.FromArrayOrString),
                    Token
                );
            }
        }

        ConversationResolver _conversationResolver;
        public ConversationResolver ConversationResolver
        {
            get => _conversationResolver;
            protected set => _conversationResolver = value;
        }

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
                else
                {
                    return null;
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        #endregion Public Properties

        #region Public Methods

        async public Task<bool> MoveToFolderAsync(
            string folderpath,
            bool saveAttachments,
            bool saveEmail,
            bool savePictures,
            bool moveConversation
        )
        {
            if (MailInfo is null)
            {
                return false;
            }

            bool attachments = (folderpath != "Trash to Delete") ? saveAttachments : false;
            var mailHelpers = moveConversation
                ? ConversationResolver.ConversationInfo.SameFolder
                : new List<MailItemHelper>() { MailInfo };

            if (!Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
            {
                logger.Warn($"Cannot sort without OneDrive location");
                return false;
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
            var result = await sorter.SortAsync(mailHelpers);
            SortEmail.Cleanup_Files();
            return result;
        }

        internal async Task OpenOlFolderAsync(string folderpath)
        {
            if (!Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive))
            {
                return;
            }

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
            if (!Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive))
            {
                return;
            }
            var config = new EmailFilerConfig()
            {
                DestinationOlStem = folderpath,
                Globals = Globals,
                OlAncestor = Globals.Ol.ArchiveRootPath,
                FsAncestorEquivalent = oneDrive,
            };

            var sorter = new EmailFiler(config);
            await sorter.OpenFileSystemFolderAsync();
        }

        public async Task MoveToFolderAsync(
            MAPIFolder folder,
            string olAncestor,
            bool saveAttachments,
            bool saveEmail,
            bool savePictures,
            bool moveConversation
        )
        {
            var folderpath = folder.FolderPath.Replace(olAncestor, "");
            if (folderpath.StartsWith(@"\"))
            {
                folderpath = folderpath.Substring(1);
            }
            var result = await MoveToFolderAsync(
                folderpath,
                saveAttachments,
                saveEmail,
                savePictures,
                moveConversation
            );
            if (!result)
            {
                MessageBox.Show($"Cannot move to folderpath {folderpath}");
            }
        }

        public IList<MailItem> PackageItems(bool moveConversation)
        {
            if (moveConversation)
            {
                return _conversationResolver.ConversationItems.SameFolder;
            }
            else
            {
                return new List<MailItem>() { Mail };
            }
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
                objItem: _mail
            );
        }

        public void RefreshSuggestions()
        {
            //_folderHelper.Suggestions.Vlog.SetVerbose(new List<string> { "RefreshSuggestions","AddWordSequenceSuggestions" });
            _folderHelper.RefreshSuggestions(mailItem: Mail);
        }

        #endregion Public Methods
    }
}
