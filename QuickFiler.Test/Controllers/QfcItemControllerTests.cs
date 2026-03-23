using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;

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
}
