using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcHomeControllerDependenciesTestsProductionFactory
    {
        [TestCleanup]
        public void Cleanup()
        {
            EfcHomeControllerDependencies.ResetProductionFactoriesForTesting();
        }

        [TestMethod]
        public void Constructor_WithNoOverrides_UsesResettableProductionFactories()
        {
            var values = CreateValues();
            var asyncMailItems = new List<MailItem>();

            EfcHomeControllerDependencies.ProductionDataModelFactory = (
                globals,
                mail,
                tokenSource,
                token
            ) => values.DataModel;
            EfcHomeControllerDependencies.ProductionAsyncDataModelFactory = (
                globals,
                mailItems,
                tokenSource,
                token,
                loadAll
            ) =>
            {
                mailItems.Should().BeSameAs(asyncMailItems);
                loadAll.Should().BeTrue();
                return Task.FromResult(values.DataModel);
            };
            EfcHomeControllerDependencies.ProductionViewerFactory = () => values.Viewer;
            EfcHomeControllerDependencies.ProductionKeyboardHandlerFactory = (viewer, controller) =>
                values.KeyboardHandler;
            EfcHomeControllerDependencies.ProductionExplorerControllerFactory = (
                initType,
                globals,
                controller
            ) => values.ExplorerController;
            EfcHomeControllerDependencies.ProductionFormControllerWithDataFactory = (
                globals,
                dataModel,
                viewer,
                controller,
                cleanup,
                initType,
                token
            ) => values.FormController;
            EfcHomeControllerDependencies.ProductionFormControllerWithoutDataFactory = (
                globals,
                viewer,
                controller,
                cleanup,
                initType,
                token
            ) => values.FormController;
            EfcHomeControllerDependencies.ProductionInitializeDataFields = (
                controller,
                dataModel
            ) => values.FormController;

            var dependencies = new EfcHomeControllerDependencies();

            using (var tokenSource = new CancellationTokenSource())
            {
                dependencies
                    .DataModelFactory(values.Globals, values.Mail, tokenSource, tokenSource.Token)
                    .Should()
                    .BeSameAs(values.DataModel);
                dependencies
                    .AsyncDataModelFactory(
                        values.Globals,
                        asyncMailItems,
                        tokenSource,
                        tokenSource.Token,
                        true
                    )
                    .GetAwaiter()
                    .GetResult()
                    .Should()
                    .BeSameAs(values.DataModel);
            }

            dependencies.ViewerFactory().Should().BeSameAs(values.Viewer);
            dependencies
                .KeyboardHandlerFactory(values.Viewer, values.HomeController)
                .Should()
                .BeSameAs(values.KeyboardHandler);
            dependencies
                .ExplorerControllerFactory(
                    QfEnums.InitTypeEnum.Find,
                    values.Globals,
                    values.HomeController
                )
                .Should()
                .BeSameAs(values.ExplorerController);
            dependencies
                .FormControllerWithDataFactory(
                    values.Globals,
                    values.DataModel,
                    values.Viewer,
                    values.HomeController,
                    values.Cleanup,
                    QfEnums.InitTypeEnum.Sort,
                    CancellationToken.None
                )
                .Should()
                .BeSameAs(values.FormController);
            dependencies
                .FormControllerWithoutDataFactory(
                    values.Globals,
                    values.Viewer,
                    values.HomeController,
                    values.Cleanup,
                    QfEnums.InitTypeEnum.Find,
                    CancellationToken.None
                )
                .Should()
                .BeSameAs(values.FormController);
            dependencies
                .InitializeDataFields(values.FormController, values.DataModel)
                .Should()
                .BeSameAs(values.FormController);
        }

        [TestMethod]
        public void WithFactoryHelpers_ValidateFactoryArguments()
        {
            var values = CreateValues();

            using (var tokenSource = new CancellationTokenSource())
            {
                VerifyArgumentNull(
                    () =>
                        EfcHomeControllerDependencies.CreateDataModelWithFactory(
                            values.Globals,
                            values.Mail,
                            tokenSource,
                            tokenSource.Token,
                            null
                        ),
                    "factory"
                );
            }
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.CreateKeyboardHandlerWithFactory(
                        values.Viewer,
                        values.HomeController,
                        null
                    ),
                "factory"
            );
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.CreateExplorerControllerWithFactory(
                        QfEnums.InitTypeEnum.Find,
                        values.Globals,
                        values.HomeController,
                        null
                    ),
                "factory"
            );
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.CreateInitializedFormControllerWithDataFactory(
                        values.Globals,
                        values.DataModel,
                        values.Viewer,
                        values.HomeController,
                        values.Cleanup,
                        QfEnums.InitTypeEnum.Sort,
                        CancellationToken.None,
                        null
                    ),
                "factory"
            );
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.CreateInitializedFormControllerWithoutDataFactory(
                        values.Globals,
                        values.Viewer,
                        values.HomeController,
                        values.Cleanup,
                        QfEnums.InitTypeEnum.Find,
                        CancellationToken.None,
                        null
                    ),
                "factory"
            );
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.InitializeFormControllerDataFieldsWithFactory(
                        values.FormController,
                        values.DataModel,
                        null
                    ),
                "factory"
            );
        }

        [TestMethod]
        public void LoadSelection_WithExplicitMail_DoesNotTraverseOutlookSelection()
        {
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict).Object;
            var mail = new Mock<MailItem>(MockBehavior.Loose).Object;

            var result = EfcHomeControllerDependencies.LoadSelection(globals, mail);

            result.Should().Equal(mail);
        }

        [TestMethod]
        public void ConstructorDefaults_InvokeProductionConstructionAdapters()
        {
            var values = CreateValues();
            var withDataInitialized = false;
            var withoutDataInitialized = false;
            var dataFieldsInitialized = false;
            EfcHomeControllerDependencies.ProductionDataModelConstructor = (
                globals,
                mail,
                tokenSource,
                token
            ) => values.DataModel;
            EfcHomeControllerDependencies.ProductionKeyboardHandlerConstructor = (
                viewer,
                homeController
            ) => values.KeyboardHandler;
            EfcHomeControllerDependencies.ProductionExplorerControllerConstructor = (
                initType,
                globals,
                homeController
            ) => values.ExplorerController;
            EfcHomeControllerDependencies.ProductionFormControllerWithDataConstructor = (
                globals,
                dataModel,
                viewer,
                homeController,
                cleanup,
                initType,
                token
            ) => values.FormController;
            EfcHomeControllerDependencies.ProductionFormControllerWithDataInitializer =
                controller =>
                {
                    withDataInitialized = true;
                    return controller;
                };
            EfcHomeControllerDependencies.ProductionFormControllerWithoutDataConstructor = (
                globals,
                viewer,
                homeController,
                cleanup,
                initType,
                token
            ) => values.FormController;
            EfcHomeControllerDependencies.ProductionFormControllerWithoutDataInitializer =
                controller =>
                {
                    withoutDataInitialized = true;
                    return controller;
                };
            EfcHomeControllerDependencies.ProductionDataFieldsInitializer = (
                controller,
                dataModel
            ) =>
            {
                dataFieldsInitialized = true;
                return controller;
            };

            var dependencies = new EfcHomeControllerDependencies();

            using (var tokenSource = new CancellationTokenSource())
            {
                dependencies
                    .DataModelFactory(values.Globals, values.Mail, tokenSource, tokenSource.Token)
                    .Should()
                    .BeSameAs(values.DataModel);
                dependencies
                    .KeyboardHandlerFactory(values.Viewer, values.HomeController)
                    .Should()
                    .BeSameAs(values.KeyboardHandler);
                dependencies
                    .ExplorerControllerFactory(
                        QfEnums.InitTypeEnum.Sort,
                        values.Globals,
                        values.HomeController
                    )
                    .Should()
                    .BeSameAs(values.ExplorerController);
                dependencies
                    .FormControllerWithDataFactory(
                        values.Globals,
                        values.DataModel,
                        values.Viewer,
                        values.HomeController,
                        values.Cleanup,
                        QfEnums.InitTypeEnum.Sort,
                        tokenSource.Token
                    )
                    .Should()
                    .BeSameAs(values.FormController);
                dependencies
                    .FormControllerWithoutDataFactory(
                        values.Globals,
                        values.Viewer,
                        values.HomeController,
                        values.Cleanup,
                        QfEnums.InitTypeEnum.Find,
                        tokenSource.Token
                    )
                    .Should()
                    .BeSameAs(values.FormController);
                dependencies
                    .InitializeDataFields(values.FormController, values.DataModel)
                    .Should()
                    .BeSameAs(values.FormController);
            }

            withDataInitialized.Should().BeTrue();
            withoutDataInitialized.Should().BeTrue();
            dataFieldsInitialized.Should().BeTrue();
        }

        private static DependencyValues CreateValues()
        {
            return new DependencyValues(
                new Mock<IApplicationGlobals>(MockBehavior.Strict).Object,
                new Mock<MailItem>(MockBehavior.Loose).Object,
                CreateUninitialized<EfcDataModel>(),
                CreateUninitialized<EfcViewer>(),
                CreateUninitialized<EfcHomeController>(),
                new Mock<IQfcKeyboardHandler>(MockBehavior.Strict).Object,
                new Mock<IQfcExplorerController>(MockBehavior.Strict).Object,
                CreateUninitialized<EfcFormController>(),
                () => { }
            );
        }

        private static void VerifyArgumentNull(System.Action action, string parameterName)
        {
            action
                .Should()
                .Throw<ArgumentNullException>()
                .Where(exception => exception.ParamName == parameterName);
        }

        private static T CreateUninitialized<T>()
            where T : class
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }

        private sealed class DependencyValues
        {
            internal DependencyValues(
                IApplicationGlobals globals,
                MailItem mail,
                EfcDataModel dataModel,
                EfcViewer viewer,
                EfcHomeController homeController,
                IQfcKeyboardHandler keyboardHandler,
                IQfcExplorerController explorerController,
                EfcFormController formController,
                System.Action cleanup
            )
            {
                Globals = globals;
                Mail = mail;
                DataModel = dataModel;
                Viewer = viewer;
                HomeController = homeController;
                KeyboardHandler = keyboardHandler;
                ExplorerController = explorerController;
                FormController = formController;
                Cleanup = cleanup;
            }

            internal IApplicationGlobals Globals { get; }

            internal MailItem Mail { get; }

            internal EfcDataModel DataModel { get; }

            internal EfcViewer Viewer { get; }

            internal EfcHomeController HomeController { get; }

            internal IQfcKeyboardHandler KeyboardHandler { get; }

            internal IQfcExplorerController ExplorerController { get; }

            internal EfcFormController FormController { get; }

            internal System.Action Cleanup { get; }
        }
    }
}
