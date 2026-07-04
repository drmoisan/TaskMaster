using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler
{
    internal sealed partial class EfcHomeControllerDependencies
    {
        internal delegate EfcFormController FormControllerWithDataFactoryDelegate(
            IApplicationGlobals globals,
            EfcDataModel dataModel,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        );

        internal delegate EfcFormController FormControllerWithoutDataFactoryDelegate(
            IApplicationGlobals globals,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        );

        internal EfcHomeControllerDependencies(
            Func<
                IApplicationGlobals,
                MailItem,
                CancellationTokenSource,
                CancellationToken,
                EfcDataModel
            > dataModelFactory = null,
            Func<
                IApplicationGlobals,
                List<MailItem>,
                CancellationTokenSource,
                CancellationToken,
                bool,
                Task<EfcDataModel>
            > asyncDataModelFactory = null,
            Func<EfcViewer> viewerFactory = null,
            Func<EfcViewer, EfcHomeController, IQfcKeyboardHandler> keyboardHandlerFactory = null,
            Func<
                QfEnums.InitTypeEnum,
                IApplicationGlobals,
                EfcHomeController,
                IQfcExplorerController
            > explorerControllerFactory = null,
            FormControllerWithDataFactoryDelegate formControllerWithDataFactory = null,
            FormControllerWithoutDataFactoryDelegate formControllerWithoutDataFactory = null,
            Func<EfcFormController, EfcDataModel, EfcFormController> initializeDataFields = null,
            Func<IApplicationGlobals, MailItem, List<MailItem>> selectionLoader = null,
            Func<DateTime> metricsNowFactory = null,
            Action<string, string[], string> metricsLineWriter = null
        )
        {
            DataModelFactory = dataModelFactory ?? CreateDataModel;
            AsyncDataModelFactory = asyncDataModelFactory ?? ProductionAsyncDataModelFactory;
            ViewerFactory = viewerFactory ?? ProductionViewerFactory;
            KeyboardHandlerFactory = keyboardHandlerFactory ?? CreateKeyboardHandler;
            ExplorerControllerFactory = explorerControllerFactory ?? CreateExplorerController;
            FormControllerWithDataFactory =
                formControllerWithDataFactory ?? CreateInitializedFormControllerWithData;
            FormControllerWithoutDataFactory =
                formControllerWithoutDataFactory ?? CreateInitializedFormControllerWithoutData;
            InitializeDataFields = initializeDataFields ?? InitializeFormControllerDataFields;
            SelectionLoader = selectionLoader ?? LoadSelection;
            MetricsNowFactory = metricsNowFactory ?? (() => DateTime.Now);
            MetricsLineWriter = metricsLineWriter ?? FileIO2.WriteTextFile;
        }

        internal Func<
            IApplicationGlobals,
            MailItem,
            CancellationTokenSource,
            CancellationToken,
            EfcDataModel
        > DataModelFactory { get; }

        internal Func<
            IApplicationGlobals,
            List<MailItem>,
            CancellationTokenSource,
            CancellationToken,
            bool,
            Task<EfcDataModel>
        > AsyncDataModelFactory { get; }

        internal Func<EfcViewer> ViewerFactory { get; }

        internal Func<
            EfcViewer,
            EfcHomeController,
            IQfcKeyboardHandler
        > KeyboardHandlerFactory { get; }

        internal Func<
            QfEnums.InitTypeEnum,
            IApplicationGlobals,
            EfcHomeController,
            IQfcExplorerController
        > ExplorerControllerFactory { get; }

        internal FormControllerWithDataFactoryDelegate FormControllerWithDataFactory { get; }

        internal FormControllerWithoutDataFactoryDelegate FormControllerWithoutDataFactory { get; }

        internal Func<
            EfcFormController,
            EfcDataModel,
            EfcFormController
        > InitializeDataFields { get; }

        internal Func<IApplicationGlobals, MailItem, List<MailItem>> SelectionLoader { get; }

        internal Func<DateTime> MetricsNowFactory { get; }

        internal Action<string, string[], string> MetricsLineWriter { get; }

        private static EfcDataModel CreateDataModel(
            IApplicationGlobals globals,
            MailItem mail,
            CancellationTokenSource tokenSource,
            CancellationToken token
        )
        {
            return CreateDataModelWithFactory(
                globals,
                mail,
                tokenSource,
                token,
                ProductionDataModelFactory
            );
        }

        internal static EfcDataModel CreateDataModelWithFactory(
            IApplicationGlobals globals,
            MailItem mail,
            CancellationTokenSource tokenSource,
            CancellationToken token,
            Func<
                IApplicationGlobals,
                MailItem,
                CancellationTokenSource,
                CancellationToken,
                EfcDataModel
            > factory
        )
        {
            if (globals is null)
            {
                throw new ArgumentNullException(nameof(globals));
            }
            if (tokenSource is null)
            {
                throw new ArgumentNullException(nameof(tokenSource));
            }
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return factory(globals, mail, tokenSource, token);
        }

        private static IQfcKeyboardHandler CreateKeyboardHandler(
            EfcViewer viewer,
            EfcHomeController homeController
        )
        {
            return CreateKeyboardHandlerWithFactory(
                viewer,
                homeController,
                ProductionKeyboardHandlerFactory
            );
        }

        internal static IQfcKeyboardHandler CreateKeyboardHandlerWithFactory(
            EfcViewer viewer,
            EfcHomeController homeController,
            Func<EfcViewer, EfcHomeController, IQfcKeyboardHandler> factory
        )
        {
            if (viewer is null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            if (homeController is null)
            {
                throw new ArgumentNullException(nameof(homeController));
            }
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return factory(viewer, homeController);
        }

        private static IQfcExplorerController CreateExplorerController(
            QfEnums.InitTypeEnum initType,
            IApplicationGlobals globals,
            EfcHomeController homeController
        )
        {
            return CreateExplorerControllerWithFactory(
                initType,
                globals,
                homeController,
                ProductionExplorerControllerFactory
            );
        }

        internal static IQfcExplorerController CreateExplorerControllerWithFactory(
            QfEnums.InitTypeEnum initType,
            IApplicationGlobals globals,
            EfcHomeController homeController,
            Func<
                QfEnums.InitTypeEnum,
                IApplicationGlobals,
                EfcHomeController,
                IQfcExplorerController
            > factory
        )
        {
            if (globals is null)
            {
                throw new ArgumentNullException(nameof(globals));
            }
            if (homeController is null)
            {
                throw new ArgumentNullException(nameof(homeController));
            }
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return factory(initType, globals, homeController);
        }

        private static EfcFormController CreateInitializedFormControllerWithData(
            IApplicationGlobals globals,
            EfcDataModel dataModel,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            return CreateInitializedFormControllerWithDataFactory(
                globals,
                dataModel,
                viewer,
                homeController,
                cleanup,
                initType,
                token,
                ProductionFormControllerWithDataFactory
            );
        }

        internal static EfcFormController CreateInitializedFormControllerWithDataFactory(
            IApplicationGlobals globals,
            EfcDataModel dataModel,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token,
            FormControllerWithDataFactoryDelegate factory
        )
        {
            if (globals is null)
            {
                throw new ArgumentNullException(nameof(globals));
            }
            if (dataModel is null)
            {
                throw new ArgumentNullException(nameof(dataModel));
            }
            if (viewer is null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            if (homeController is null)
            {
                throw new ArgumentNullException(nameof(homeController));
            }
            if (cleanup is null)
            {
                throw new ArgumentNullException(nameof(cleanup));
            }
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return factory(globals, dataModel, viewer, homeController, cleanup, initType, token);
        }

        private static EfcFormController CreateInitializedFormControllerWithoutData(
            IApplicationGlobals globals,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            return CreateInitializedFormControllerWithoutDataFactory(
                globals,
                viewer,
                homeController,
                cleanup,
                initType,
                token,
                ProductionFormControllerWithoutDataFactory
            );
        }

        internal static EfcFormController CreateInitializedFormControllerWithoutDataFactory(
            IApplicationGlobals globals,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token,
            FormControllerWithoutDataFactoryDelegate factory
        )
        {
            if (globals is null)
            {
                throw new ArgumentNullException(nameof(globals));
            }
            if (viewer is null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }
            if (homeController is null)
            {
                throw new ArgumentNullException(nameof(homeController));
            }
            if (cleanup is null)
            {
                throw new ArgumentNullException(nameof(cleanup));
            }
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return factory(globals, viewer, homeController, cleanup, initType, token);
        }

        private static EfcFormController InitializeFormControllerDataFields(
            EfcFormController controller,
            EfcDataModel dataModel
        )
        {
            return InitializeFormControllerDataFieldsWithFactory(
                controller,
                dataModel,
                ProductionInitializeDataFields
            );
        }

        internal static EfcFormController InitializeFormControllerDataFieldsWithFactory(
            EfcFormController controller,
            EfcDataModel dataModel,
            Func<EfcFormController, EfcDataModel, EfcFormController> factory
        )
        {
            if (controller is null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            if (dataModel is null)
            {
                throw new ArgumentNullException(nameof(dataModel));
            }
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return factory(controller, dataModel);
        }

        internal static List<MailItem> LoadSelection(IApplicationGlobals globals, MailItem mail)
        {
            if (globals is null)
            {
                throw new ArgumentNullException(nameof(globals));
            }

            List<MailItem> mailItems = [];

            if (mail is not null)
            {
                mailItems.Add(mail);
                return mailItems;
            }

            var selection = globals.Ol.App.ActiveExplorer().Selection;
            if (selection.Count > 0)
            {
                mailItems = selection
                    .Cast<object>()
                    .Where(x => x is MailItem)
                    .Cast<MailItem>()
                    .ToList();
            }

            return mailItems;
        }
    }
}
