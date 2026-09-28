using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #792 AC-U3 on the pop-out carry: the folder handler and mail
    /// helper of the popped-out item are read from its group before the group is torn down, the
    /// pop-out home controller is built through a named factory seam, and the home controller
    /// deposits the carry on the data model before the form controller (whose factory fires the
    /// carry consumer) is constructed. Controllers are allocated without a constructor, so no
    /// Outlook COM context and no WinForms window is required.
    /// </summary>
    [TestClass]
    public sealed class QfcCollectionControllerIssue792PopOutTests
    {
        private const string ProductionFactoryName = "CreatePopOutHomeController";

        private static T CreateUninitialized<T>()
            where T : class
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }

        /// <summary>
        /// A concrete item controller exposes both its folder handler and its mail helper.
        /// Before the fix the stub returns a null pair, so the first assertion fails.
        /// </summary>
        [TestMethod]
        public void ReadPopOutCarry_WithConcreteItemController_ReturnsHandlerAndHelper()
        {
            // Arrange
            var handler = new Mock<IFolderSearchHandler>().Object;
            var helper = new Mock<MailItemHelper>(MockBehavior.Loose).Object;
            var itemController = CreateUninitialized<QfcItemController>();
            SetPrivateField(itemController, "_folderHandler", handler);
            SetPrivateField(itemController, "_itemInfo", helper);
            var group = new QfcItemGroup { ItemController = itemController };

            // Act
            var carry = QfcCollectionController.ReadPopOutCarry(group);

            // Assert
            carry
                .FolderHandler.Should()
                .BeSameAs(handler, "the concrete controller's folder handler must be carried");
            carry.MailHelper.Should().BeSameAs(helper, "the item helper must be carried");
        }

        /// <summary>
        /// An interface-only controller has no folder-handler accessor, so only the helper is
        /// carried. Before the fix the stub returns a null pair, so the helper assertion fails.
        /// </summary>
        [TestMethod]
        public void ReadPopOutCarry_WithInterfaceOnlyController_ReturnsNullHandlerAndTheHelper()
        {
            // Arrange
            var helper = new Mock<MailItemHelper>(MockBehavior.Loose).Object;
            var itemController = new Mock<IQfcItemController>();
            itemController.SetupGet(c => c.ItemHelper).Returns(helper);
            var group = new QfcItemGroup { ItemController = itemController.Object };

            // Act
            var carry = QfcCollectionController.ReadPopOutCarry(group);

            // Assert
            carry
                .FolderHandler.Should()
                .BeNull("the interface exposes no folder handler to carry");
            carry.MailHelper.Should().BeSameAs(helper, "the interface helper must be carried");
        }

        /// <summary>Control: a group with no controller carries nothing.</summary>
        [TestMethod]
        public void ReadPopOutCarry_WithNullController_ReturnsNulls()
        {
            // Arrange
            var group = new QfcItemGroup();

            // Act
            var carry = QfcCollectionController.ReadPopOutCarry(group);

            // Assert
            carry.FolderHandler.Should().BeNull("there is no controller to read from");
            carry.MailHelper.Should().BeNull("there is no controller to read from");
        }

        /// <summary>
        /// Control: the factory seam defaults to the named production factory, accepts a
        /// substitute by reference, and reverts to the named default when reset to null.
        /// </summary>
        [TestMethod]
        public void PopOutHomeControllerFactory_DefaultIsTheNamedProductionFactory()
        {
            // Arrange
            var controller = CreateUninitialized<QfcCollectionController>();
            Func<
                IApplicationGlobals,
                System.Action,
                MailItem,
                IFolderSearchHandler,
                MailItemHelper,
                EfcHomeController
            > recording = (globals, cleanup, mail, handler, helper) => null;

            // Act and Assert
            controller
                .PopOutHomeControllerFactory.Method.Name.Should()
                .Be(ProductionFactoryName, "the default must be the named production factory");
            controller.PopOutHomeControllerFactory = recording;
            controller
                .PopOutHomeControllerFactory.Should()
                .BeSameAs(recording, "a substituted factory must be returned by reference");
            controller.PopOutHomeControllerFactory = null;
            controller
                .PopOutHomeControllerFactory.Method.Name.Should()
                .Be(ProductionFactoryName, "resetting to null must restore the named default");
        }

        /// <summary>
        /// Control after Phase 2 (mutation-proven in Phase 5): the home controller deposits the
        /// carried handler and helper on the data model before the form-controller factory runs,
        /// so the factory observes both at call time.
        /// </summary>
        [TestMethod]
        public void EfcHomeController_DepositsTheCarryOnTheDataModelBeforeConstructingTheFormController()
        {
            // Arrange
            var mail = new Mock<MailItem>(MockBehavior.Loose).Object;
            var handler = new Mock<IFolderSearchHandler>().Object;
            var helper = new Mock<MailItemHelper>(MockBehavior.Loose).Object;
            var fileSystem = new Mock<IFileSystemFolderPaths>();
            fileSystem
                .SetupGet(f => f.SpecialFolders)
                .Returns(new ConcurrentDictionary<string, string>());
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.FS).Returns(fileSystem.Object);
            bool formFactoryCalled = false;
            IFolderSearchHandler capturedHandler = null;
            MailItemHelper capturedHelper = null;
            var dependencies = new EfcHomeControllerDependencies(
                dataModelFactory: (g, selectedMail, tokenSource, token) =>
                {
                    var dataModel = CreateUninitialized<EfcDataModel>();
                    SetPrivateField(dataModel, "_mail", selectedMail);
                    return dataModel;
                },
                viewerFactory: () => CreateUninitialized<EfcViewer>(),
                keyboardHandlerFactory: (viewer, controller) =>
                    new Mock<IQfcKeyboardHandler>(MockBehavior.Loose).Object,
                explorerControllerFactory: (initType, g, controller) =>
                    new Mock<IQfcExplorerController>(MockBehavior.Loose).Object,
                formControllerWithDataFactory: (
                    g,
                    dataModel,
                    viewer,
                    controller,
                    cleanup,
                    initType,
                    token
                ) =>
                {
                    formFactoryCalled = true;
                    capturedHandler = dataModel.CarriedFolderHandler;
                    capturedHelper = dataModel.CarriedMailHelper;
                    return CreateUninitialized<EfcFormController>();
                }
            );

            // Act
            var homeController = new EfcHomeController(
                globals.Object,
                () => { },
                dependencies,
                mail,
                handler,
                helper
            );

            // Assert
            formFactoryCalled
                .Should()
                .BeTrue("a data model with mail must build the form controller");
            capturedHandler
                .Should()
                .BeSameAs(handler, "the carried handler must be on the data model at factory time");
            capturedHelper
                .Should()
                .BeSameAs(helper, "the carried helper must be on the data model at factory time");
            homeController
                .DataModel.CarriedFolderHandler.Should()
                .BeSameAs(handler, "the deposit must persist on the constructed controller");
        }
    }
}
