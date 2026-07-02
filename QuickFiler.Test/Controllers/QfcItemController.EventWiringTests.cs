using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Event-wiring cluster tests (research §5.2). Covers focus and expanded keyboard-action
    /// registration/unregistration parity using the reflection-injected keyboard-handler stub.
    /// Lambda bodies are only evaluated when invoked, so registration runs without a live view.
    /// </summary>
    [TestClass]
    public class QfcItemController_EventWiringTests
    {
        private sealed class KbdController : QfcItemController
        {
            internal KbdController(IQfcKeyboardHandler kbdHandler, string entryId)
                : base()
            {
                typeof(QfcItemController)
                    .GetField("_kbdHandler", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, kbdHandler);

                var helper = new MailItemHelper();
                helper.EntryId = entryId;
                ItemHelper = helper;
            }
        }

        private static (
            Mock<IQfcKeyboardHandler> mock,
            KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> keyActionsAsync,
            KbdActions<char, KaCharAsync, Func<char, Task>> charActionsAsync
        ) BuildKbdHandlerStub()
        {
            var mockKbd = new Mock<IQfcKeyboardHandler>();
            var keyActionsAsync = new KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>();
            var charActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();
            mockKbd.Setup(k => k.KeyActionsAsync).Returns(keyActionsAsync);
            mockKbd.Setup(k => k.CharActionsAsync).Returns(charActionsAsync);
            return (mockKbd, keyActionsAsync, charActionsAsync);
        }

        [TestMethod]
        public void RegisterFocusAsyncActions_RegistersExpectedCharActions()
        {
            // Arrange
            var (mockKbd, _, charActionsAsync) = BuildKbdHandlerStub();
            var controller = new KbdController(mockKbd.Object, "entry-focus-char");

            // Act
            controller.RegisterFocusAsyncActions();

            // Assert — a representative subset of the focus char-actions must be registered.
            charActionsAsync.ContainsKey('C').Should().BeTrue();
            charActionsAsync.ContainsKey('O').Should().BeTrue();
            charActionsAsync.ContainsKey('S').Should().BeTrue();
        }

        [TestMethod]
        public void UnregisterFocusAsyncActions_AfterRegister_RemovesCharActions()
        {
            // Arrange
            var (mockKbd, _, charActionsAsync) = BuildKbdHandlerStub();
            var controller = new KbdController(mockKbd.Object, "entry-focus-char-cleanup");
            controller.RegisterFocusAsyncActions();
            charActionsAsync.ContainsKey('C').Should().BeTrue(because: "precondition");

            // Act
            controller.UnregisterFocusAsyncActions();

            // Assert
            charActionsAsync.ContainsKey('C').Should().BeFalse();
            charActionsAsync.ContainsKey('S').Should().BeFalse();
        }

        [TestMethod]
        public void RegisterExpandedAsyncActions_RegistersBAndD()
        {
            // Arrange
            var (mockKbd, _, charActionsAsync) = BuildKbdHandlerStub();
            var controller = new KbdController(mockKbd.Object, "entry-expanded");

            // Act
            controller.RegisterExpandedAsyncActions();

            // Assert
            charActionsAsync.ContainsKey('B').Should().BeTrue();
            charActionsAsync.ContainsKey('D').Should().BeTrue();
        }

        [TestMethod]
        public void UnregisterExpandedAsyncActions_AfterRegister_RemovesBAndD()
        {
            // Arrange
            var (mockKbd, _, charActionsAsync) = BuildKbdHandlerStub();
            var controller = new KbdController(mockKbd.Object, "entry-expanded-cleanup");
            controller.RegisterExpandedAsyncActions();
            charActionsAsync.ContainsKey('B').Should().BeTrue(because: "precondition");

            // Act
            controller.UnregisterExpandedAsyncActions();

            // Assert
            charActionsAsync.ContainsKey('B').Should().BeFalse();
            charActionsAsync.ContainsKey('D').Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // Cycle-2 Phase 5 (AC8) de-exemption coverage: the SYNCHRONOUS registration-membership
        // members RegisterFocusActions / UnregisterFocusActions / UnregisterExpandedActions, mirroring
        // the async membership tests above. Lambda bodies are evaluated only when invoked, so
        // registration runs without a live view.
        // ---------------------------------------------------------------------------

        private static (
            Mock<IQfcKeyboardHandler> mock,
            KbdActions<Keys, KaKey, Action<Keys>> keyActions,
            KbdActions<char, KaChar, Action<char>> charActions
        ) BuildSyncKbdHandlerStub()
        {
            var mockKbd = new Mock<IQfcKeyboardHandler>();
            var keyActions = new KbdActions<Keys, KaKey, Action<Keys>>();
            var charActions = new KbdActions<char, KaChar, Action<char>>();
            mockKbd.Setup(k => k.KeyActions).Returns(keyActions);
            mockKbd.Setup(k => k.CharActions).Returns(charActions);
            return (mockKbd, keyActions, charActions);
        }

        [TestMethod]
        public void RegisterFocusActions_RegistersExpectedSyncKeyAndCharActions()
        {
            // Arrange
            var (mockKbd, keyActions, charActions) = BuildSyncKbdHandlerStub();
            var controller = new KbdController(mockKbd.Object, "entry-sync-focus");

            // Act
            controller.RegisterFocusActions();

            // Assert — representative subset of the sync focus registrations.
            keyActions.ContainsKey(Keys.Right).Should().BeTrue();
            keyActions.ContainsKey(Keys.Left).Should().BeTrue();
            charActions.ContainsKey('C').Should().BeTrue();
            charActions.ContainsKey('O').Should().BeTrue();
            charActions.ContainsKey('F').Should().BeTrue();
        }

        [TestMethod]
        public void UnregisterFocusActions_AfterRegister_RemovesSyncKeyAndCharActions()
        {
            // Arrange
            var (mockKbd, keyActions, charActions) = BuildSyncKbdHandlerStub();
            var controller = new KbdController(mockKbd.Object, "entry-sync-focus-cleanup");
            controller.RegisterFocusActions();
            keyActions.ContainsKey(Keys.Right).Should().BeTrue(because: "precondition");
            charActions.ContainsKey('C').Should().BeTrue(because: "precondition");

            // Act
            controller.UnregisterFocusActions();

            // Assert
            keyActions.ContainsKey(Keys.Right).Should().BeFalse();
            keyActions.ContainsKey(Keys.Left).Should().BeFalse();
            charActions.ContainsKey('C').Should().BeFalse();
            charActions.ContainsKey('F').Should().BeFalse();
        }

        /// <summary>
        /// Cycle-3 P9-T3 (member #20, de-exempted): the registration act itself has no barrier — only
        /// invoking the 'B'/'D' lambda bodies touches the concrete-bound WebView2/TopicThread controls.
        /// Mirrors <see cref="RegisterFocusActions_RegistersExpectedSyncKeyAndCharActions"/>.
        /// </summary>
        [TestMethod]
        public void RegisterExpandedActions_RegistersBAndDWithoutInvokingLambdaBodies()
        {
            // Arrange
            var (mockKbd, _, charActions) = BuildSyncKbdHandlerStub();
            var controller = new KbdController(mockKbd.Object, "entry-sync-expanded");

            // Act
            controller.RegisterExpandedActions();

            // Assert — registration populates both entries without invoking their lambda bodies.
            charActions.ContainsKey('B').Should().BeTrue();
            charActions.ContainsKey('D').Should().BeTrue();
        }

        [TestMethod]
        public void UnregisterExpandedActions_AfterRegister_RemovesSyncBAndD()
        {
            // Arrange — RegisterExpandedActions populates the sync 'B'/'D' entries (its lambda bodies
            // are not invoked at registration), then UnregisterExpandedActions must remove them.
            var (mockKbd, _, charActions) = BuildSyncKbdHandlerStub();
            var controller = new KbdController(mockKbd.Object, "entry-sync-expanded-cleanup");
            controller.RegisterExpandedActions();
            charActions.ContainsKey('B').Should().BeTrue(because: "precondition");

            // Act
            controller.UnregisterExpandedActions();

            // Assert
            charActions.ContainsKey('B').Should().BeFalse();
            charActions.ContainsKey('D').Should().BeFalse();
        }
    }
}
