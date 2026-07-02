using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        protected QfcItemController() { }

        public QfcItemController(
            IApplicationGlobals appGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            IItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null,
            QuickFiler.Viewers.IWebViewCoreInitializer webViewInitializer = null,
            QuickFiler.Interfaces.IMailItemActions mailActions = null,
            Func<MailItem, ConversationResolver> conversationResolverFactory = null,
            Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks> flagTasksFactory =
                null,
            Func<EmailFilerConfig, EmailFiler> emailFilerFactory = null,
            Func<
                IApplicationGlobals,
                object,
                FolderPredictor.InitOptions,
                FolderPredictor
            > folderPredictorFactory = null,
            Func<IApplicationGlobals, FolderPredictor> folderPredictorEmptyFactory = null
        )
        {
            //TraceUtility.LogMethodCall(appGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);
            // Store any injected seams before SaveParameters applies the production defaults for the
            // ones left null (see SaveParameters). Non-breaking: all seam parameters are optional.
            _uiDispatcher = uiDispatcher;
            _webViewInitializer = webViewInitializer;
            _mailActions = mailActions;
            _conversationResolverFactory = conversationResolverFactory;
            _flagTasksFactory = flagTasksFactory;
            _emailFilerFactory = emailFilerFactory;
            _folderPredictorFactory = folderPredictorFactory;
            _folderPredictorEmptyFactory = folderPredictorEmptyFactory;
            SaveParameters(
                appGlobals,
                homeController,
                parent,
                itemViewer,
                viewerPosition,
                itemNumberDigits,
                mailItem,
                tlpStates
            );
        }

        /// <summary>
        /// High-confidence (Issue #171) constructor overload. Behaves identically to the primary
        /// constructor but records the predetermined high-confidence folder so
        /// <see cref="AssignFolderComboBox"/> preselects that folder instead of selecting by index.
        /// </summary>
        /// <param name="predeterminedFolder">
        /// The predetermined top-suggestion folder path, or null for the standard (non-high-confidence)
        /// path in which the index-based selection is used.
        /// </param>
        public QfcItemController(
            IApplicationGlobals appGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            IItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            string predeterminedFolder
        )
        {
            SaveParameters(
                appGlobals,
                homeController,
                parent,
                itemViewer,
                viewerPosition,
                itemNumberDigits,
                mailItem,
                tlpStates
            );
            _predeterminedFolder = predeterminedFolder;
        }

        public QfcItemController(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            IItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            bool async
        )
        {
            SaveParameters(
                AppGlobals,
                homeController,
                parent,
                itemViewer,
                viewerPosition,
                itemNumberDigits,
                mailItem,
                tlpStates
            );
        }

        // Residual (bucket-iii): orchestration overload that funnels into Initialize(bool), whose body
        // drives concrete control-tree construction (ResolveControlGroups/SetupThemes/WireEvents against
        // the live ItemViewer). Not unit-reachable without a real ItemViewer under Option A.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void Initialize(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            IItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            bool async
        )
        {
            SaveParameters(
                AppGlobals,
                homeController,
                parent,
                itemViewer,
                viewerPosition,
                itemNumberDigits,
                mailItem,
                tlpStates
            );

            Initialize(async);
        }

        // Residual (bucket-iii): orchestrates concrete control-tree construction — calls
        // ResolveControlGroups((ItemViewer)_itemViewer), QfcThemeHelper.SetupThemes((ItemViewer)...),
        // and WireEvents (ForAllControls) — all requiring a live ItemViewer. Not unit-reachable.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void Initialize(bool async)
        {
            // Group controls into collections
            ResolveControlGroups((ItemViewer)_itemViewer); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init

            // Setup the theme dictionary (Note: need control groups established prior to this)
            _themes = QfcThemeHelper.SetupThemes(
                this,
                (ItemViewer)_itemViewer,
                this.HtmlDarkConverter,
                _uiDispatcher
            ); // concrete-bound seam (P2-T5): SetupThemes requires concrete control objects

            // Populate placeholder controls with values
            PopulateControls(Mail, ItemNumber);

            // Adjust item viewer for desired state
            ToggleTips(async: async, desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
            ToggleNavigation(async: async, desiredState: Enums.ToggleState.Off);

            // Activate event management
            WireEvents();

            // Fire and forget WebView initialization
            _ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);
            //Task.Run(() => InitializeWebViewAsync());
        }

        // Residual (bucket-iii): async orchestration of concrete control-tree construction
        // (ResolveControlGroupsAsync((ItemViewer)...), SetupThemes((ItemViewer)...),
        // InitializeWebViewAsync, WireEvents). Not unit-reachable without a live ItemViewer.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task InitializeAsync()
        {
            //TraceUtility.LogMethodCall();

            // Group controls into collections
            Token.ThrowIfCancellationRequested();
            await ResolveControlGroupsAsync((ItemViewer)_itemViewer); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init

            _themes = QfcThemeHelper.SetupThemes(
                this,
                (ItemViewer)_itemViewer,
                this.HtmlDarkConverter,
                _uiDispatcher
            ); // concrete-bound seam (P2-T5): SetupThemes requires concrete control objects
            if (_globals.Ol.DarkMode)
            {
                SetThemeDark(async: true);
            }
            else
            {
                SetThemeLight(async: true);
            }

            await PopulateControlsAsync(Mail, ItemNumber, false);
            await ToggleTipsAsync(desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
            await ToggleNavigationAsync(desiredState: Enums.ToggleState.Off);

            //WireEvents();

            //// Parallel won't work because everything uses the UI thread
            //var tasks = new List<Task>
            //{
            //    Task.Run(()=>
            //    {
            //        _themes = QfcThemeHelper.SetupThemes(this, _itemViewer, this.HtmlDarkConverter);
            //        if (_globals.Ol.DarkMode) { SetThemeDark(async: true); }
            //        else { SetThemeLight(async: true); }
            //    },Token),
            //    PopulateControlsAsync(Mail, ItemNumber, true),
            //    ToggleTipsAsync(desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force),
            //    ToggleNavigationAsync(desiredState: Enums.ToggleState.Off),
            //};
            //
            //await Task.WhenAll(tasks);

            //tasks = new List<Task>
            //{
            await PopulateConversationAsync(_tokenSource, Token, false);
            await PopulateFolderComboBoxAsync(default, null);
            //};

            //await Task.WhenAll(tasks);
            await Task.Run(() => WireEvents());

            await InitializeWebViewAsync();
        }

        // Residual (bucket-iii): same concrete control-tree orchestration as InitializeAsync
        // (ResolveControlGroups/SetupThemes against the live ItemViewer). Not unit-reachable.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task InitializeGraphicsAsync()
        {
            // Group controls into collections
            await Task.Run(() => ResolveControlGroups((ItemViewer)_itemViewer)); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init

            _themes = await Task.Run(() =>
                QfcThemeHelper.SetupThemes(
                    this,
                    (ItemViewer)_itemViewer,
                    this.HtmlDarkConverter,
                    _uiDispatcher
                ) // concrete-bound seam (P2-T5): SetupThemes requires concrete control objects
            );

            if (_globals.Ol.DarkMode)
            {
                SetThemeDark(async: false);
            }
            else
            {
                SetThemeLight(async: false);
            }
            await ToggleTipsAsync(desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
            await ToggleNavigationAsync(desiredState: Enums.ToggleState.Off);
            WireEvents();
            _ = InitializeWebViewAsync();
        }

        // Residual (bucket-iii): same concrete control-tree orchestration as InitializeAsync
        // (ResolveControlGroups/SetupThemes against the live ItemViewer). Not unit-reachable.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task InitializeSequentialAsync()
        {
            Token.ThrowIfCancellationRequested();

            // Group controls into collections
            await Task.Run(() => ResolveControlGroups((ItemViewer)_itemViewer)); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init

            _themes = QfcThemeHelper.SetupThemes(
                this,
                (ItemViewer)_itemViewer,
                this.HtmlDarkConverter,
                _uiDispatcher
            ); // concrete-bound seam (P2-T5): SetupThemes requires concrete control objects
            if (_globals.Ol.DarkMode)
            {
                SetThemeDark(async: true);
            }
            else
            {
                SetThemeLight(async: true);
            }

            await PopulateControlsAsync(Mail, ItemNumber, false);

            await ToggleTipsAsync(desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
            //ToggleTips(async: true, desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
            await ToggleNavigationAsync(desiredState: Enums.ToggleState.Off);
            WireEvents();

            _ = InitializeWebViewAsync();
        }

        //public async Task InitializeSequentialAsync()
        //{
        //    _token.ThrowIfCancellationRequested();

        //    // Group controls into collections
        //    ResolveControlGroups(_itemViewer);

        //    _themes = QfcThemeHelper.SetupThemes(this, _itemViewer, this.HtmlDarkConverter);
        //    if (_globals.Ol.DarkMode) { SetThemeDark(async: true); }
        //    else { SetThemeLight(async: true); }

        //    await PopulateControlsAsync(Mail, ItemNumber, false);

        //    await ToggleTipsAsync(desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
        //    //ToggleTips(async: true, desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
        //    await ToggleNavigationAsync(desiredState: Enums.ToggleState.Off);
        //    WireEvents();

        //    _ = InitializeWebViewAsync();

        //}

        internal void SaveParameters(
            IApplicationGlobals appGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            IItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates
        )
        {
            //TraceUtility.LogMethodCall(appGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);

            // Save parameters to private fields
            _globals = appGlobals;
            _homeController = homeController;
            _parent = parent;
            _itemViewer = itemViewer;
            _mailItem = mailItem;
            _tlpStates = tlpStates;
            _itemNumberDigits = itemNumberDigits;
            //ItemNumber = viewerPosition;
            _itemNumber = viewerPosition;

            // Set references to other controllers
            _itemViewer.Controller = this;
            _kbdHandler = _homeController.KeyboardHandler;
            _explorerController = _homeController.ExplorerController;
            Token = _homeController.Token;
            _tokenSource = _homeController.TokenSource;

            // Apply production defaults for the Phase 6 behavioral seams on the single construction
            // path every route hits (all public ctors + the CreateAsync/CreateSequentialAsync factory
            // path funnel through here). Any seam already supplied via the constructor is preserved.
            _uiDispatcher ??= new UtilitiesCS.Threading.WpfUiDispatcher();
            _webViewInitializer ??= new QuickFiler.Viewers.WebView2CoreInitializer();
            _conversationResolverFactory ??= mail => new ConversationResolver(
                _globals,
                mail,
                _tokenSource,
                Token,
                SetTopicThread
            );
            _flagTasksFactory ??= (globals, itemList, blFile, hWndCaller) =>
                new FlagTasks(globals, itemList, blFile, hWndCaller);
            _emailFilerFactory ??= config => new EmailFiler(config);
            _mailActions ??= mailItem is null
                ? null
                : new QuickFiler.Interfaces.MailItemActionsAdapter(mailItem);
            _folderPredictorFactory ??= (globals, objItem, options) =>
                new FolderPredictor(globals, objItem, options);
            _folderPredictorEmptyFactory ??= globals => new FolderPredictor(globals);
        }

        // Residual (bucket-iii): static factory that constructs the controller and awaits
        // InitializeAsync (concrete control-tree orchestration). Barrier is inherited from the async
        // init it drives; not unit-reachable without a live ItemViewer.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static async Task<QfcItemController> CreateAsync(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            ItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var controller = new QfcItemController();
            controller.SaveParameters(
                AppGlobals,
                homeController,
                parent,
                itemViewer,
                viewerPosition,
                itemNumberDigits,
                mailItem,
                tlpStates
            );
            await controller.InitializeAsync();
            return controller;
        }

        // Residual (bucket-iii): static factory that constructs the controller and awaits
        // InitializeSequentialAsync (concrete control-tree orchestration). Barrier inherited from the
        // async init it drives; not unit-reachable without a live ItemViewer.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static async Task<QfcItemController> CreateSequentialAsync(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            ItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var controller = new QfcItemController();
            controller.SaveParameters(
                AppGlobals,
                homeController,
                parent,
                itemViewer,
                viewerPosition,
                itemNumberDigits,
                mailItem,
                tlpStates
            );
            await controller.InitializeSequentialAsync();
            return controller;
        }
    }
}
