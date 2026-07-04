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
    public class EfcHomeControllerDependenciesTests
    {
        [TestMethod]
        public void Constructor_WithNoOverrides_InstallsProductionDefaults()
        {
            var dependencies = new EfcHomeControllerDependencies();

            dependencies.DataModelFactory.Should().NotBeNull();
            dependencies.AsyncDataModelFactory.Should().NotBeNull();
            dependencies.ViewerFactory.Should().NotBeNull();
            dependencies.KeyboardHandlerFactory.Should().NotBeNull();
            dependencies.ExplorerControllerFactory.Should().NotBeNull();
            dependencies.FormControllerWithDataFactory.Should().NotBeNull();
            dependencies.FormControllerWithoutDataFactory.Should().NotBeNull();
            dependencies.InitializeDataFields.Should().NotBeNull();
            dependencies.SelectionLoader.Should().NotBeNull();
            dependencies.MetricsNowFactory.Should().NotBeNull();
            dependencies.MetricsLineWriter.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithOverrides_PreservesInjectedDelegates()
        {
            var dataModel = CreateUninitialized<EfcDataModel>();
            var viewer = CreateUninitialized<EfcViewer>();
            var formController = CreateUninitialized<EfcFormController>();
            var keyboardHandler = new Mock<IQfcKeyboardHandler>(MockBehavior.Strict).Object;
            var explorerController = new Mock<IQfcExplorerController>(MockBehavior.Strict).Object;
            var now = new DateTime(2026, 7, 4, 12, 0, 0);
            var writeCalls = new List<string>();
            Func<
                IApplicationGlobals,
                MailItem,
                CancellationTokenSource,
                CancellationToken,
                EfcDataModel
            > dataModelFactory = (globals, mail, tokenSource, token) => dataModel;
            Func<
                IApplicationGlobals,
                List<MailItem>,
                CancellationTokenSource,
                CancellationToken,
                bool,
                Task<EfcDataModel>
            > asyncDataModelFactory = (globals, mailItems, tokenSource, token, loadAll) =>
                Task.FromResult(dataModel);
            Func<EfcViewer> viewerFactory = () => viewer;
            Func<EfcViewer, EfcHomeController, IQfcKeyboardHandler> keyboardFactory = (
                factoryViewer,
                homeController
            ) => keyboardHandler;
            Func<
                QfEnums.InitTypeEnum,
                IApplicationGlobals,
                EfcHomeController,
                IQfcExplorerController
            > explorerFactory = (initType, globals, homeController) => explorerController;
            EfcHomeControllerDependencies.FormControllerWithDataFactoryDelegate formWithDataFactory =
                (globals, model, factoryViewer, homeController, cleanup, initType, token) =>
                    formController;
            EfcHomeControllerDependencies.FormControllerWithoutDataFactoryDelegate formWithoutDataFactory =
                (globals, factoryViewer, homeController, cleanup, initType, token) =>
                    formController;
            Func<EfcFormController, EfcDataModel, EfcFormController> initializeDataFields = (
                controller,
                model
            ) => controller;
            Func<IApplicationGlobals, MailItem, List<MailItem>> selectionLoader = (globals, mail) =>
                new List<MailItem>();
            Func<DateTime> metricsNowFactory = () => now;
            Action<string, string[], string> metricsLineWriter = (filename, lines, folderRoot) =>
                writeCalls.Add($"{filename}:{folderRoot}:{lines.Length}");

            var dependencies = new EfcHomeControllerDependencies(
                dataModelFactory: dataModelFactory,
                asyncDataModelFactory: asyncDataModelFactory,
                viewerFactory: viewerFactory,
                keyboardHandlerFactory: keyboardFactory,
                explorerControllerFactory: explorerFactory,
                formControllerWithDataFactory: formWithDataFactory,
                formControllerWithoutDataFactory: formWithoutDataFactory,
                initializeDataFields: initializeDataFields,
                selectionLoader: selectionLoader,
                metricsNowFactory: metricsNowFactory,
                metricsLineWriter: metricsLineWriter
            );

            dependencies.DataModelFactory.Should().BeSameAs(dataModelFactory);
            dependencies.AsyncDataModelFactory.Should().BeSameAs(asyncDataModelFactory);
            dependencies.ViewerFactory.Should().BeSameAs(viewerFactory);
            dependencies.KeyboardHandlerFactory.Should().BeSameAs(keyboardFactory);
            dependencies.ExplorerControllerFactory.Should().BeSameAs(explorerFactory);
            dependencies.FormControllerWithDataFactory.Should().BeSameAs(formWithDataFactory);
            dependencies.FormControllerWithoutDataFactory.Should().BeSameAs(formWithoutDataFactory);
            dependencies.InitializeDataFields.Should().BeSameAs(initializeDataFields);
            dependencies.SelectionLoader.Should().BeSameAs(selectionLoader);
            dependencies.MetricsNowFactory().Should().Be(now);

            dependencies.MetricsLineWriter("metrics.csv", new[] { "line" }, "root");

            writeCalls.Should().Equal("metrics.csv:root:1");
        }

        [TestMethod]
        public void LoadSelection_WithExplicitMail_ReturnsOnlyExplicitMail()
        {
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict).Object;
            var mail = new Mock<MailItem>(MockBehavior.Loose).Object;

            var result = EfcHomeControllerDependencies.LoadSelection(globals, mail);

            result.Should().Equal(mail);
        }

        [TestMethod]
        public void CreateDataModelWithFactory_ValidatesAndForwardsArguments()
        {
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict).Object;
            var mail = new Mock<MailItem>(MockBehavior.Loose).Object;
            using (var tokenSource = new CancellationTokenSource())
            {
                var token = tokenSource.Token;
                var expected = CreateUninitialized<EfcDataModel>();

                var result = EfcHomeControllerDependencies.CreateDataModelWithFactory(
                    globals,
                    mail,
                    tokenSource,
                    token,
                    (factoryGlobals, factoryMail, factoryTokenSource, factoryToken) =>
                    {
                        factoryGlobals.Should().BeSameAs(globals);
                        factoryMail.Should().BeSameAs(mail);
                        factoryTokenSource.Should().BeSameAs(tokenSource);
                        factoryToken.Should().Be(token);
                        return expected;
                    }
                );

                result.Should().BeSameAs(expected);
                VerifyArgumentNull(
                    () =>
                        EfcHomeControllerDependencies.CreateDataModelWithFactory(
                            null,
                            mail,
                            tokenSource,
                            token,
                            (factoryGlobals, factoryMail, factoryTokenSource, factoryToken) =>
                                expected
                        ),
                    "globals"
                );
                VerifyArgumentNull(
                    () =>
                        EfcHomeControllerDependencies.CreateDataModelWithFactory(
                            globals,
                            mail,
                            null,
                            token,
                            (factoryGlobals, factoryMail, factoryTokenSource, factoryToken) =>
                                expected
                        ),
                    "tokenSource"
                );
            }
        }

        [TestMethod]
        public void CreateKeyboardHandlerWithFactory_ValidatesViewerAndHomeController()
        {
            var viewer = CreateUninitialized<EfcViewer>();
            var homeController = CreateUninitialized<EfcHomeController>();
            var expected = new Mock<IQfcKeyboardHandler>(MockBehavior.Strict).Object;

            var result = EfcHomeControllerDependencies.CreateKeyboardHandlerWithFactory(
                viewer,
                homeController,
                (factoryViewer, factoryHomeController) =>
                {
                    factoryViewer.Should().BeSameAs(viewer);
                    factoryHomeController.Should().BeSameAs(homeController);
                    return expected;
                }
            );

            result.Should().BeSameAs(expected);
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.CreateKeyboardHandlerWithFactory(
                        null,
                        homeController,
                        (factoryViewer, factoryHomeController) => expected
                    ),
                "viewer"
            );
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.CreateKeyboardHandlerWithFactory(
                        viewer,
                        null,
                        (factoryViewer, factoryHomeController) => expected
                    ),
                "homeController"
            );
        }

        [TestMethod]
        public void CreateExplorerControllerWithFactory_ValidatesGlobalsAndHomeController()
        {
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict).Object;
            var homeController = CreateUninitialized<EfcHomeController>();
            var expected = new Mock<IQfcExplorerController>(MockBehavior.Strict).Object;

            var result = EfcHomeControllerDependencies.CreateExplorerControllerWithFactory(
                QfEnums.InitTypeEnum.Find,
                globals,
                homeController,
                (initType, factoryGlobals, factoryHomeController) =>
                {
                    initType.Should().Be(QfEnums.InitTypeEnum.Find);
                    factoryGlobals.Should().BeSameAs(globals);
                    factoryHomeController.Should().BeSameAs(homeController);
                    return expected;
                }
            );

            result.Should().BeSameAs(expected);
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.CreateExplorerControllerWithFactory(
                        QfEnums.InitTypeEnum.Find,
                        null,
                        homeController,
                        (initType, factoryGlobals, factoryHomeController) => expected
                    ),
                "globals"
            );
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.CreateExplorerControllerWithFactory(
                        QfEnums.InitTypeEnum.Find,
                        globals,
                        null,
                        (initType, factoryGlobals, factoryHomeController) => expected
                    ),
                "homeController"
            );
        }

        [TestMethod]
        public void CreateInitializedFormControllerWithDataFactory_ValidatesRequiredArguments()
        {
            var values = CreateFormValues();

            var result =
                EfcHomeControllerDependencies.CreateInitializedFormControllerWithDataFactory(
                    values.Globals,
                    values.DataModel,
                    values.Viewer,
                    values.HomeController,
                    values.Cleanup,
                    QfEnums.InitTypeEnum.Sort,
                    values.Token,
                    (globals, dataModel, viewer, homeController, cleanup, initType, token) =>
                    {
                        dataModel.Should().BeSameAs(values.DataModel);
                        cleanup.Should().BeSameAs(values.Cleanup);
                        return values.FormController;
                    }
                );

            result.Should().BeSameAs(values.FormController);
            VerifyArgumentNull(() => CreateFormControllerWithData(values, "globals"), "globals");
            VerifyArgumentNull(
                () => CreateFormControllerWithData(values, "dataModel"),
                "dataModel"
            );
            VerifyArgumentNull(() => CreateFormControllerWithData(values, "viewer"), "viewer");
            VerifyArgumentNull(
                () => CreateFormControllerWithData(values, "homeController"),
                "homeController"
            );
            VerifyArgumentNull(() => CreateFormControllerWithData(values, "cleanup"), "cleanup");
        }

        [TestMethod]
        public void CreateInitializedFormControllerWithoutDataFactory_ValidatesRequiredArguments()
        {
            var values = CreateFormValues();

            var result =
                EfcHomeControllerDependencies.CreateInitializedFormControllerWithoutDataFactory(
                    values.Globals,
                    values.Viewer,
                    values.HomeController,
                    values.Cleanup,
                    QfEnums.InitTypeEnum.Find,
                    values.Token,
                    (globals, viewer, homeController, cleanup, initType, token) =>
                    {
                        viewer.Should().BeSameAs(values.Viewer);
                        homeController.Should().BeSameAs(values.HomeController);
                        return values.FormController;
                    }
                );

            result.Should().BeSameAs(values.FormController);
            VerifyArgumentNull(() => CreateFormControllerWithoutData(values, "globals"), "globals");
            VerifyArgumentNull(() => CreateFormControllerWithoutData(values, "viewer"), "viewer");
            VerifyArgumentNull(
                () => CreateFormControllerWithoutData(values, "homeController"),
                "homeController"
            );
            VerifyArgumentNull(() => CreateFormControllerWithoutData(values, "cleanup"), "cleanup");
        }

        [TestMethod]
        public void InitializeFormControllerDataFieldsWithFactory_ValidatesArguments()
        {
            var formController = CreateUninitialized<EfcFormController>();
            var dataModel = CreateUninitialized<EfcDataModel>();

            var result =
                EfcHomeControllerDependencies.InitializeFormControllerDataFieldsWithFactory(
                    formController,
                    dataModel,
                    (controller, model) =>
                    {
                        controller.Should().BeSameAs(formController);
                        model.Should().BeSameAs(dataModel);
                        return formController;
                    }
                );

            result.Should().BeSameAs(formController);
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.InitializeFormControllerDataFieldsWithFactory(
                        null,
                        dataModel,
                        (controller, model) => formController
                    ),
                "controller"
            );
            VerifyArgumentNull(
                () =>
                    EfcHomeControllerDependencies.InitializeFormControllerDataFieldsWithFactory(
                        formController,
                        null,
                        (controller, model) => formController
                    ),
                "dataModel"
            );
        }

        private static EfcFormController CreateFormControllerWithData(
            FormValues values,
            string nullParameter
        )
        {
            return EfcHomeControllerDependencies.CreateInitializedFormControllerWithDataFactory(
                nullParameter == "globals" ? null : values.Globals,
                nullParameter == "dataModel" ? null : values.DataModel,
                nullParameter == "viewer" ? null : values.Viewer,
                nullParameter == "homeController" ? null : values.HomeController,
                nullParameter == "cleanup" ? null : values.Cleanup,
                QfEnums.InitTypeEnum.Sort,
                values.Token,
                (
                    factoryGlobals,
                    factoryDataModel,
                    factoryViewer,
                    controller,
                    factoryCleanup,
                    initType,
                    token
                ) => values.FormController
            );
        }

        private static EfcFormController CreateFormControllerWithoutData(
            FormValues values,
            string nullParameter
        )
        {
            return EfcHomeControllerDependencies.CreateInitializedFormControllerWithoutDataFactory(
                nullParameter == "globals" ? null : values.Globals,
                nullParameter == "viewer" ? null : values.Viewer,
                nullParameter == "homeController" ? null : values.HomeController,
                nullParameter == "cleanup" ? null : values.Cleanup,
                QfEnums.InitTypeEnum.Find,
                values.Token,
                (factoryGlobals, factoryViewer, controller, factoryCleanup, initType, token) =>
                    values.FormController
            );
        }

        private static FormValues CreateFormValues()
        {
            return new FormValues(
                new Mock<IApplicationGlobals>(MockBehavior.Strict).Object,
                CreateUninitialized<EfcDataModel>(),
                CreateUninitialized<EfcViewer>(),
                CreateUninitialized<EfcHomeController>(),
                () => { },
                CancellationToken.None,
                CreateUninitialized<EfcFormController>()
            );
        }

        private static void VerifyArgumentNull<T>(Func<T> action, string parameterName)
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

        private sealed class FormValues
        {
            internal FormValues(
                IApplicationGlobals globals,
                EfcDataModel dataModel,
                EfcViewer viewer,
                EfcHomeController homeController,
                System.Action cleanup,
                CancellationToken token,
                EfcFormController formController
            )
            {
                Globals = globals;
                DataModel = dataModel;
                Viewer = viewer;
                HomeController = homeController;
                Cleanup = cleanup;
                Token = token;
                FormController = formController;
            }

            internal IApplicationGlobals Globals { get; }

            internal EfcDataModel DataModel { get; }

            internal EfcViewer Viewer { get; }

            internal EfcHomeController HomeController { get; }

            internal System.Action Cleanup { get; }

            internal CancellationToken Token { get; }

            internal EfcFormController FormController { get; }
        }
    }
}
