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
            MailItem,
            CancellationTokenSource,
            CancellationToken,
            EfcDataModel
        > ProductionDataModelConstructor { get; set; } = CreateProductionDataModelInstance;

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
            EfcViewer,
            EfcHomeController,
            IQfcKeyboardHandler
        > ProductionKeyboardHandlerConstructor { get; set; } =
            CreateProductionKeyboardHandlerInstance;

        internal static Func<
            QfEnums.InitTypeEnum,
            IApplicationGlobals,
            EfcHomeController,
            IQfcExplorerController
        > ProductionExplorerControllerFactory { get; set; } = CreateProductionExplorerController;

        internal static Func<
            QfEnums.InitTypeEnum,
            IApplicationGlobals,
            EfcHomeController,
            IQfcExplorerController
        > ProductionExplorerControllerConstructor { get; set; } =
            CreateProductionExplorerControllerInstance;

        internal static FormControllerWithDataFactoryDelegate ProductionFormControllerWithDataFactory { get; set; } =
            CreateProductionFormControllerWithData;

        internal static FormControllerWithDataFactoryDelegate ProductionFormControllerWithDataConstructor { get; set; } =
            CreateProductionFormControllerWithDataInstance;

        internal static Func<
            EfcFormController,
            EfcFormController
        > ProductionFormControllerWithDataInitializer { get; set; } =
            controller => controller.Initialize();

        internal static FormControllerWithoutDataFactoryDelegate ProductionFormControllerWithoutDataFactory { get; set; } =
            CreateProductionFormControllerWithoutData;

        internal static FormControllerWithoutDataFactoryDelegate ProductionFormControllerWithoutDataConstructor { get; set; } =
            CreateProductionFormControllerWithoutDataInstance;

        internal static Func<
            EfcFormController,
            EfcFormController
        > ProductionFormControllerWithoutDataInitializer { get; set; } =
            controller => controller.InitializeWithoutData();

        internal static Func<
            EfcFormController,
            EfcDataModel,
            EfcFormController
        > ProductionInitializeDataFields { get; set; } = CreateProductionDataFields;

        internal static Func<
            EfcFormController,
            EfcDataModel,
            EfcFormController
        > ProductionDataFieldsInitializer { get; set; } =
            (controller, dataModel) => controller.InitializeDataFields(dataModel);

        internal static void ResetProductionFactoriesForTesting()
        {
            ProductionDataModelFactory = CreateProductionDataModel;
            ProductionDataModelConstructor = CreateProductionDataModelInstance;
            ProductionAsyncDataModelFactory = EfcDataModel.CreateAsync;
            ProductionViewerFactory = EfcViewerQueue.Dequeue;
            ProductionKeyboardHandlerFactory = CreateProductionKeyboardHandler;
            ProductionKeyboardHandlerConstructor = CreateProductionKeyboardHandlerInstance;
            ProductionExplorerControllerFactory = CreateProductionExplorerController;
            ProductionExplorerControllerConstructor = CreateProductionExplorerControllerInstance;
            ProductionFormControllerWithDataFactory = CreateProductionFormControllerWithData;
            ProductionFormControllerWithDataConstructor =
                CreateProductionFormControllerWithDataInstance;
            ProductionFormControllerWithDataInitializer = controller => controller.Initialize();
            ProductionFormControllerWithoutDataFactory = CreateProductionFormControllerWithoutData;
            ProductionFormControllerWithoutDataConstructor =
                CreateProductionFormControllerWithoutDataInstance;
            ProductionFormControllerWithoutDataInitializer = controller =>
                controller.InitializeWithoutData();
            ProductionInitializeDataFields = CreateProductionDataFields;
            ProductionDataFieldsInitializer = (controller, dataModel) =>
                controller.InitializeDataFields(dataModel);
        }

        private static EfcDataModel CreateProductionDataModelInstance(
            IApplicationGlobals globals,
            MailItem mail,
            CancellationTokenSource tokenSource,
            CancellationToken token
        )
        {
            return new EfcDataModel(globals, mail, tokenSource, token);
        }

        private static IQfcKeyboardHandler CreateProductionKeyboardHandlerInstance(
            EfcViewer viewer,
            EfcHomeController homeController
        )
        {
            return new KeyboardHandler(viewer, homeController);
        }

        private static IQfcExplorerController CreateProductionExplorerControllerInstance(
            QfEnums.InitTypeEnum initType,
            IApplicationGlobals globals,
            EfcHomeController homeController
        )
        {
            return new QfcExplorerController(initType, globals, homeController);
        }

        private static EfcFormController CreateProductionFormControllerWithDataInstance(
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
            );
        }

        private static EfcFormController CreateProductionFormControllerWithoutDataInstance(
            IApplicationGlobals globals,
            EfcViewer viewer,
            EfcHomeController homeController,
            System.Action cleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            return new EfcFormController(globals, viewer, homeController, cleanup, initType, token);
        }

        private static EfcDataModel CreateProductionDataModel(
            IApplicationGlobals globals,
            MailItem mail,
            CancellationTokenSource tokenSource,
            CancellationToken token
        )
        {
            return ProductionDataModelConstructor(globals, mail, tokenSource, token);
        }

        private static IQfcKeyboardHandler CreateProductionKeyboardHandler(
            EfcViewer viewer,
            EfcHomeController homeController
        )
        {
            return ProductionKeyboardHandlerConstructor(viewer, homeController);
        }

        private static IQfcExplorerController CreateProductionExplorerController(
            QfEnums.InitTypeEnum initType,
            IApplicationGlobals globals,
            EfcHomeController homeController
        )
        {
            return ProductionExplorerControllerConstructor(initType, globals, homeController);
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
            var controller = ProductionFormControllerWithDataConstructor(
                globals,
                dataModel,
                viewer,
                homeController,
                cleanup,
                initType,
                token
            );
            return ProductionFormControllerWithDataInitializer(controller);
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
            var controller = ProductionFormControllerWithoutDataConstructor(
                globals,
                viewer,
                homeController,
                cleanup,
                initType,
                token
            );
            return ProductionFormControllerWithoutDataInitializer(controller);
        }

        private static EfcFormController CreateProductionDataFields(
            EfcFormController controller,
            EfcDataModel dataModel
        )
        {
            return ProductionDataFieldsInitializer(controller, dataModel);
        }
    }
}
