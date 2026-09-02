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

        // #230: de-exempted. The overload funnels into Initialize(bool); the former barrier was the
        // missing WinForms message pump for that body, not headless construction. Covered by
        // QfcItemController_InitializationTests.InitializeNineArgOverload_ThroughThePumpHost_*.
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

        // #230: de-exempted. The orchestration runs against a real ItemViewer and its tail dispatches
        // InitializeWebViewAsync through the viewer's WPF dispatcher; both require a live message
        // loop, which the WinFormsPumpHost test seam supplies. Covered by
        // QfcItemController_InitializationTests.InitializeBool_ThroughThePumpHost_*.
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
            _ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewGuardedAsync);
            //Task.Run(() => InitializeWebViewAsync());
        }

        // #230: de-exempted. The former barrier was the missing WinForms message pump for this
        // orchestration, not headless construction. Covered by
        // QfcItemController_InitializationTests.InitializeAsync_ThroughThePumpHost_*, which runs
        // every line and asserts the controlled fault at the mocked web-view seam. The terminal
        // `await InitializeWebViewAsync()` is not completable in a unit test (the CoreWebView2
        // runtime is an external process), so this member's coverage is partial by construction.
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

        // #230: de-exempted. The former barrier was the missing WinForms message pump, not headless
        // construction: the orchestration marshals through the concrete ItemViewer's WinForms
        // context. The WinFormsPumpHost test seam supplies that loop, so the member is covered by
        // QfcItemController_InitializationTests.InitializeGraphicsAsync_ThroughThePumpHost_*.
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
            _ = InitializeWebViewGuardedAsync();
        }

        // #230: de-exempted. The former barrier was the missing WinForms message pump, not headless
        // construction: the orchestration marshals through the concrete ItemViewer's WinForms
        // context. The WinFormsPumpHost test seam supplies that loop, so the member is covered by
        // QfcItemController_InitializationTests.InitializeSequentialAsync_ThroughThePumpHost_*.
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

            _ = InitializeWebViewGuardedAsync();
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

        // #230: de-exempted. The optional seam parameters below give the factory the injection point
        // it previously lacked, and the WinFormsPumpHost test seam supplies the message loop
        // InitializeAsync needs. Covered by
        // QfcItemController_SeamFactoryTests.CreateAsync_WithFaultingWebViewSeam_*, which asserts the
        // controlled fault at the mocked web-view seam (the `return controller;` statement is not
        // reachable in a unit test - see the D13 note in the #230 plan).
        public static async Task<QfcItemController> CreateAsync(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            ItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            CancellationToken token,
            UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null,
            QuickFiler.Viewers.IWebViewCoreInitializer webViewInitializer = null,
            Func<MailItem, ConversationResolver> conversationResolverFactory = null
        )
        {
            token.ThrowIfCancellationRequested();

            var controller = new QfcItemController();
            // Store any injected seams before SaveParameters applies the production defaults for the
            // ones left null, mirroring the primary constructor's optional-seam pattern. Non-breaking:
            // every seam parameter is optional and defaults preserve the previous behavior.
            controller._uiDispatcher = uiDispatcher;
            controller._webViewInitializer = webViewInitializer;
            controller._conversationResolverFactory = conversationResolverFactory;
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

        // #230: de-exempted. The optional seam parameters below give the factory the injection point
        // it previously lacked, and the WinFormsPumpHost test seam supplies the message loop
        // InitializeSequentialAsync needs. Covered by
        // QfcItemController_SeamFactoryTests.CreateSequentialAsync_WithInjectedSeams_*.
        public static async Task<QfcItemController> CreateSequentialAsync(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            ItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            CancellationToken token,
            UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null,
            QuickFiler.Viewers.IWebViewCoreInitializer webViewInitializer = null,
            Func<MailItem, ConversationResolver> conversationResolverFactory = null
        )
        {
            token.ThrowIfCancellationRequested();

            var controller = new QfcItemController();
            // Store any injected seams before SaveParameters applies the production defaults for the
            // ones left null, mirroring the primary constructor's optional-seam pattern. Non-breaking:
            // every seam parameter is optional and defaults preserve the previous behavior.
            controller._uiDispatcher = uiDispatcher;
            controller._webViewInitializer = webViewInitializer;
            controller._conversationResolverFactory = conversationResolverFactory;
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
