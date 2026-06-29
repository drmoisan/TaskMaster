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
    }
}
