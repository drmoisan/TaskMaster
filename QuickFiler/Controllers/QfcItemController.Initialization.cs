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
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected QfcItemController() { }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public QfcItemController(
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
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void Initialize(bool async)
        {
            // Group controls into collections
            ResolveControlGroups((ItemViewer)_itemViewer); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init

            // Setup the theme dictionary (Note: need control groups established prior to this)
            _themes = QfcThemeHelper.SetupThemes(
                this,
                (ItemViewer)_itemViewer,
                this.HtmlDarkConverter
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
                this.HtmlDarkConverter
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task InitializeGraphicsAsync()
        {
            // Group controls into collections
            await Task.Run(() => ResolveControlGroups((ItemViewer)_itemViewer)); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init

            _themes = await Task.Run(() =>
                QfcThemeHelper.SetupThemes(this, (ItemViewer)_itemViewer, this.HtmlDarkConverter) // concrete-bound seam (P2-T5): SetupThemes requires concrete control objects
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task InitializeSequentialAsync()
        {
            Token.ThrowIfCancellationRequested();

            // Group controls into collections
            await Task.Run(() => ResolveControlGroups((ItemViewer)_itemViewer)); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init

            _themes = QfcThemeHelper.SetupThemes(
                this,
                (ItemViewer)_itemViewer,
                this.HtmlDarkConverter
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
        }

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
