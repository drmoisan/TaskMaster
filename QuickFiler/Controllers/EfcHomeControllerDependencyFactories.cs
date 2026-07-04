using System;
using System.Collections.Generic;
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
        internal static Func<
            IApplicationGlobals,
            MailItem,
            CancellationTokenSource,
            CancellationToken,
            EfcDataModel
        > ProductionDataModelFactory { get; set; } = CreateProductionDataModel;

        internal static Func<
            IApplicationGlobals,
            List<MailItem>,
            CancellationTokenSource,
            CancellationToken,
            bool,
            Task<EfcDataModel>
        > ProductionAsyncDataModelFactory { get; set; } = EfcDataModel.CreateAsync;

        internal static Func<EfcViewer> ProductionViewerFactory { get; set; } =
            EfcViewerQueue.Dequeue;

        internal static Func<
            EfcViewer,
            EfcHomeController,
            IQfcKeyboardHandler
        > ProductionKeyboardHandlerFactory { get; set; } = CreateProductionKeyboardHandler;

        internal static Func<
            QfEnums.InitTypeEnum,
            IApplicationGlobals,
            EfcHomeController,
            IQfcExplorerController
        > ProductionExplorerControllerFactory { get; set; } = CreateProductionExplorerController;

        internal static FormControllerWithDataFactoryDelegate ProductionFormControllerWithDataFactory { get; set; } =
            CreateProductionFormControllerWithData;

        internal static FormControllerWithoutDataFactoryDelegate ProductionFormControllerWithoutDataFactory { get; set; } =
            CreateProductionFormControllerWithoutData;

        internal static Func<
            EfcFormController,
            EfcDataModel,
            EfcFormController
        > ProductionInitializeDataFields { get; set; } = CreateProductionDataFields;

        internal static void ResetProductionFactoriesForTesting()
        {
            ProductionDataModelFactory = CreateProductionDataModel;
            ProductionAsyncDataModelFactory = EfcDataModel.CreateAsync;
            ProductionViewerFactory = EfcViewerQueue.Dequeue;
            ProductionKeyboardHandlerFactory = CreateProductionKeyboardHandler;
            ProductionExplorerControllerFactory = CreateProductionExplorerController;
            ProductionFormControllerWithDataFactory = CreateProductionFormControllerWithData;
            ProductionFormControllerWithoutDataFactory = CreateProductionFormControllerWithoutData;
            ProductionInitializeDataFields = CreateProductionDataFields;
        }

        private static EfcDataModel CreateProductionDataModel(
            IApplicationGlobals globals,
            MailItem mail,
            CancellationTokenSource tokenSource,
            CancellationToken token
        )
        {
            return new EfcDataModel(globals, mail, tokenSource, token);
        }

        private static IQfcKeyboardHandler CreateProductionKeyboardHandler(
            EfcViewer viewer,
            EfcHomeController homeController
        )
        {
            return new KeyboardHandler(viewer, homeController);
        }

        private static IQfcExplorerController CreateProductionExplorerController(
            QfEnums.InitTypeEnum initType,
            IApplicationGlobals globals,
            EfcHomeController homeController
        )
        {
            return new QfcExplorerController(initType, globals, homeController);
        }

        private static EfcFormController CreateProductionFormControllerWithData(
            IApplicationGlobals globals,
            EfcDataModel dataModel,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            return new EfcFormController(
                globals,
                dataModel,
                viewer,
                homeController,
                cleanup,
                initType,
                token
            ).Initialize();
        }

        private static EfcFormController CreateProductionFormControllerWithoutData(
            IApplicationGlobals globals,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            return new EfcFormController(
                globals,
                viewer,
                homeController,
                cleanup,
                initType,
                token
            ).InitializeWithoutData();
        }

        private static EfcFormController CreateProductionDataFields(
            EfcFormController controller,
            EfcDataModel dataModel
        )
        {
            return controller.InitializeDataFields(dataModel);
        }
    }
}
