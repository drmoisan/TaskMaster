using System;
using System.Reflection;
using System.Threading;
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
    /// Regression tests for the cancellation-flow bug in QfcItemController.
    ///
    /// Root cause: LoadConversationResolverAsync caught all exceptions including
    /// OperationCanceledException and suppressed them, leaving ConversationResolver null.
    /// PopulateConversationAsync then dereferenced the null property and crashed with
    /// NullReferenceException instead of propagating the OperationCanceledException.
    /// </summary>
    [TestClass]
    public class QfcItemControllerTests
    {
        // ---------------------------------------------------------------------------
        // Test double: subclass that overrides the static-call seam so tests do not
        // require WinForms infrastructure (ItemViewer, MailItem, etc.).
        // ---------------------------------------------------------------------------
        private sealed class TestableQfcItemController : QfcItemController
        {
            private readonly Func<Task<ConversationResolver>> _loadCore;

            /// <param name="loadCore">
            /// Delegate executed in place of ConversationResolver.LoadAsync.
            /// Throw OperationCanceledException to simulate a mid-load cancellation.
            /// Throw any other exception to simulate a non-cancellation load failure.
            /// </param>
            internal TestableQfcItemController(Func<Task<ConversationResolver>> loadCore)
                : base()
            {
                _loadCore = loadCore;
            }

            protected override Task<ConversationResolver> DoLoadConversationResolverCoreAsync(
                CancellationTokenSource tokenSource,
                CancellationToken token,
                bool loadAll
            ) => _loadCore();
        }

        // ---------------------------------------------------------------------------
        // LoadConversationResolverAsync tests
        // ---------------------------------------------------------------------------

        [TestMethod]
        public async Task LoadConversationResolverAsync_WhenLoadThrowsOperationCanceled_PropagatesCancellation()
        {
            // Arrange
            // Simulates OperationCanceledException thrown from inside ConversationResolver.LoadAsync
            // (e.g., from TimeOutTask.RunWithTimeout -> GetConversationDfAsync) while the token
            // was canceled during the async operation.
            // A separate CTS is used for the method call so the pre-guard passes; the seam
            // throws unconditionally to reproduce the bug scenario.
            var callCts = new CancellationTokenSource();
            var callToken = callCts.Token; // not canceled; pre-guard passes

            var controller = new TestableQfcItemController(() =>
                throw new OperationCanceledException(callToken)
            );

            // Act
            Func<Task> act = () =>
                controller.LoadConversationResolverAsync(callCts, callToken, false);

            // Assert — before the fix this call completed silently (exception was suppressed)
            await act.Should()
                .ThrowAsync<OperationCanceledException>(
                    because: "cancellation during load must propagate, not be swallowed"
                );

            callCts.Dispose();
        }

        [TestMethod]
        public async Task LoadConversationResolverAsync_WhenLoadThrowsNonCancellation_DoesNotThrow()
        {
            // Arrange — non-cancellation exceptions (e.g., COM errors) must still be
            // suppressed and logged so the overall populate flow can continue gracefully.
            var callCts = new CancellationTokenSource();
            var callToken = callCts.Token;

            var controller = new TestableQfcItemController(() =>
                throw new InvalidOperationException("simulated non-cancel load failure")
            );

            // Act
            Func<Task> act = () =>
                controller.LoadConversationResolverAsync(callCts, callToken, false);

            // Assert — non-OCE must still be swallowed; behaviour is unchanged from before fix
            await act.Should()
                .NotThrowAsync(
                    because: "non-cancellation load failures must be suppressed and logged"
                );

            callCts.Dispose();
        }

        // ---------------------------------------------------------------------------
        // PopulateConversationAsync tests
        // ---------------------------------------------------------------------------

        [TestMethod]
        public async Task PopulateConversationAsync_WhenLoadCanceledDuringAsync_ThrowsOperationCanceledNotNullRef()
        {
            // Arrange
            // This is the exact regression scenario: token cancelled during LoadAsync,
            // OCE was swallowed, ConversationResolver was null, and PopulateConversationAsync
            // crashed with NullReferenceException on ConversationResolver.Count.SameFolder.
            var callCts = new CancellationTokenSource();
            var callToken = callCts.Token;

            var controller = new TestableQfcItemController(() =>
                throw new OperationCanceledException(callToken)
            );

            // Act
            Func<Task> act = () => controller.PopulateConversationAsync(callCts, callToken, false);

            // Assert — before the fix this threw NullReferenceException;
            //           after the fix it propagates OperationCanceledException.
            await act.Should()
                .ThrowAsync<OperationCanceledException>(
                    because: "a mid-load cancellation must surface as OperationCanceledException, "
                        + "not crash with NullReferenceException on the null ConversationResolver"
                );

            callCts.Dispose();
        }

        [TestMethod]
        public async Task PopulateConversationAsync_WhenLoadFailsWithNonCancellation_ReturnsWithoutCrash()
        {
            // Arrange — verifies the null guard added to PopulateConversationAsync:
            // if a non-cancellation exception causes the resolver to remain null,
            // the method must return cleanly rather than dereference null.
            var callCts = new CancellationTokenSource();
            var callToken = callCts.Token;

            var controller = new TestableQfcItemController(() =>
                throw new InvalidOperationException("simulated non-cancel load failure")
            );

            // Act
            Func<Task> act = () => controller.PopulateConversationAsync(callCts, callToken, false);

            // Assert — ConversationResolver is null after the suppressed load failure;
            //           the null guard must prevent a NullReferenceException.
            await act.Should()
                .NotThrowAsync(
                    because: "when load fails silently and ConversationResolver is null, "
                        + "PopulateConversationAsync must return without crashing"
                );

            callCts.Dispose();
        }
    }

    /// <summary>
    /// Regression tests for Issue #96: Right arrow key does not expand conversation messages.
    ///
    /// Root cause: RegisterFocusAsyncActions() never registered Keys.Right in
    /// KeyActionsAsync. The handler was commented out during the async migration and not
    /// restored. As a result, Right-arrow key presses fell through to the focused WinForms
    /// control and activated the sender's mailto: address instead of expanding the
    /// conversation view.
    ///
    /// Fix: Add Keys.Right → ToggleExpansionAsync(On) in RegisterFocusAsyncActions() and
    /// remove it in UnregisterFocusAsyncActions().
    /// </summary>
    [TestClass]
    public class QfcItemController_KeyboardRegistrationTests
    {
        // ---------------------------------------------------------------------------
        // Test double: minimal subclass that injects a stub keyboard handler and a
        // MailItemHelper with a known EntryId. No WinForms infrastructure is required
        // because the lambda bodies are only evaluated when invoked, not at registration.
        // ---------------------------------------------------------------------------
        private sealed class KeyboardRegistrationQfcItemController : QfcItemController
        {
            /// <param name="kbdHandler">
            /// Stub keyboard handler whose KbdActions collections receive the Add/Remove calls
            /// made by RegisterFocusAsyncActions and UnregisterFocusAsyncActions.
            /// </param>
            /// <param name="entryId">
            /// String used as the sourceId in KbdActions registrations; must be unique
            /// within each collection.
            /// </param>
            internal KeyboardRegistrationQfcItemController(
                IQfcKeyboardHandler kbdHandler,
                string entryId
            )
                : base()
            {
                // Inject keyboard handler via reflection (field is private in production code).
                typeof(QfcItemController)
                    .GetField("_kbdHandler", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, kbdHandler);

                // Set ItemHelper with a known EntryId so sourceId assignments are predictable.
                var helper = new MailItemHelper();
                helper.EntryId = entryId;
                ItemHelper = helper;
            }
        }

        // ---------------------------------------------------------------------------
        // Helper: build a minimal stub keyboard handler whose KbdActions properties
        // return real (but empty) collection instances so that Add/Remove calls succeed.
        // Only CharActionsAsync and KeyActionsAsync are needed by RegisterFocusAsyncActions.
        // ---------------------------------------------------------------------------
        private static (
            Mock<IQfcKeyboardHandler> mock,
            KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> keyActionsAsync,
            KbdActions<char, KaCharAsync, Func<char, Task>> charActionsAsync
        ) BuildKbdHandlerStub()
        {
            var mockKbd = new Mock<IQfcKeyboardHandler>();

            var keyActionsAsync = new KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>();
            var charActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();

            // Route property accesses to the real collections so Add/Remove mutate them.
            mockKbd.Setup(k => k.KeyActionsAsync).Returns(keyActionsAsync);
            mockKbd.Setup(k => k.CharActionsAsync).Returns(charActionsAsync);

            return (mockKbd, keyActionsAsync, charActionsAsync);
        }

        // ---------------------------------------------------------------------------
        // Regression test — P1-T1
        // This test MUST FAIL before the fix and PASS after.
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void RegisterFocusAsyncActions_RightArrowKey_IsRegisteredInKeyActionsAsync()
        {
            // Arrange
            // Build a stub keyboard handler with real KbdActions collections.
            // RegisterFocusAsyncActions must add Keys.Right to KeyActionsAsync so that
            // KeyDownTaskAsync intercepts and suppresses the key press instead of letting it
            // fall through to the focused mailto: control.
            var (mockKbd, keyActionsAsync, _) = BuildKbdHandlerStub();
            var controller = new KeyboardRegistrationQfcItemController(
                mockKbd.Object,
                "test-entry-id-right-key"
            );

            // Act
            controller.RegisterFocusAsyncActions();

            // Assert — before the fix this fails because Keys.Right was not registered
            keyActionsAsync
                .ContainsKey(Keys.Right)
                .Should()
                .BeTrue(
                    because: "Keys.Right must be registered in KeyActionsAsync so that the keyboard "
                        + "handler intercepts the key press and expands the conversation instead of "
                        + "routing it to the mailto: control"
                );
        }

        // ---------------------------------------------------------------------------
        // Regression test — P1-T2
        // Verifies that the Right-arrow registration is cleaned up on focus loss.
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowFromKeyActionsAsync()
        {
            // Arrange
            // First register, then unregister. The Right-arrow entry must be absent
            // after unregistration so that nav outside the keyboard-active item does not
            // capture Right-arrow presses that belong to a different item's handler.
            var (mockKbd, keyActionsAsync, _) = BuildKbdHandlerStub();
            var controller = new KeyboardRegistrationQfcItemController(
                mockKbd.Object,
                "test-entry-id-right-key-cleanup"
            );

            controller.RegisterFocusAsyncActions();

            // Precondition: Right must be registered (asserted in the previous test).
            keyActionsAsync
                .ContainsKey(Keys.Right)
                .Should()
                .BeTrue(because: "precondition — right key must be registered before cleanup");

            // Act
            controller.UnregisterFocusAsyncActions();

            // Assert — the entry must be removed on unregister
            keyActionsAsync
                .ContainsKey(Keys.Right)
                .Should()
                .BeFalse(
                    because: "Keys.Right handler must be removed from KeyActionsAsync when focus "
                        + "actions are unregistered, otherwise stale registrations accumulate "
                        + "across focus changes"
                );
        }
    }
}
