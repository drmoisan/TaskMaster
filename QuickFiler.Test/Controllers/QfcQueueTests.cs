using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for <see cref="QfcQueue"/>.
    /// </summary>
    [TestClass]
    public class QfcQueueTests
    {
        /// <summary>
        /// Verifies that <see cref="QfcQueue.RemoveItem"/> does not propagate an
        /// <see cref="OperationCanceledException"/> when the instance-level cancellation token
        /// is already cancelled before the call.
        ///
        /// Scenario: <c>_token</c> is pre-cancelled and <c>_jobsRunning</c> is 1, so
        /// <c>JobsToFinish</c> enters the polling loop and immediately calls
        /// <c>token.ThrowIfCancellationRequested()</c>. The fix must catch the exception and
        /// return gracefully rather than letting it bubble to the caller.
        /// </summary>
        [TestMethod]
        public async Task RemoveItem_WhenTokenPreCancelled_DoesNotThrow()
        {
            // Arrange: cancel the source before constructing QfcQueue so _token is already
            // cancelled when RemoveItem is called.
            var cts = new CancellationTokenSource();
            try
            {
                cts.Cancel();

                var appGlobals = new Mock<IApplicationGlobals>().Object;
                var queue = new QfcQueue(cts.Token, (QfcHomeController)null, appGlobals);

                // Set _jobsRunning to 1 so the JobsToFinish polling loop executes and reaches
                // ThrowIfCancellationRequested, reproducing the pre-fix crash path.
                var field = typeof(QfcQueue).GetField(
                    "_jobsRunning",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                field?.SetValue(queue, 1);

                var mockMail = new Mock<Outlook.MailItem>();
                mockMail.Setup(m => m.EntryID).Returns("test-id");

                // Act + Assert: before the fix, OperationCanceledException bubbles out of
                // RemoveItem; after the fix, RemoveItem catches it and returns gracefully.
                await queue
                    .Awaiting(q => q.RemoveItem(mockMail.Object))
                    .Should()
                    .NotThrowAsync<OperationCanceledException>();
            }
            finally
            {
                cts.Dispose();
            }
        }
    }
}
