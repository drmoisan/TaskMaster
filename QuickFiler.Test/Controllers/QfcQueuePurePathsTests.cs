using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for the pure, Outlook-free queue-state paths of <see cref="QfcQueue"/>. The
    /// instance is constructed with a null home controller and a mocked
    /// <see cref="IApplicationGlobals"/> (the established pattern used by QfcQueueTests); the
    /// primary constructor merely stores these. Only paths that do not touch the WinForms
    /// TableLayoutPanel graph, MailItems, or the UI dispatcher are exercised: the Count and
    /// JobsRunning accessors, the empty-queue early return of TryDequeueAsync, and the
    /// no-jobs-running fast paths of CompleteAddingAsync and JobsToFinish. The TLP/MailItem/
    /// dispatcher-bound members are out of scope (Outlook/WinForms) per the seam verification.
    /// </summary>
    [TestClass]
    public class QfcQueuePurePathsTests
    {
        private static QfcQueue NewQueue(CancellationToken token)
        {
            var globals = new Mock<IApplicationGlobals>().Object;
            return new QfcQueue(token, (QfcHomeController)null, globals);
        }

        [TestMethod]
        public void NewQueue_HasZeroCountAndZeroJobsRunning()
        {
            // Arrange / Act
            var queue = NewQueue(CancellationToken.None);

            // Assert
            queue.Count.Should().Be(0, "a fresh queue holds no entries");
            queue.JobsRunning.Should().Be(0, "a fresh queue has no jobs running");
        }

        [TestMethod]
        public async Task TryDequeueAsync_EmptyQueueNoJobs_ReturnsDefault()
        {
            // Arrange: empty queue and no jobs trigger the documented early return.
            var queue = NewQueue(CancellationToken.None);

            // Act
            var result = await queue.TryDequeueAsync(CancellationToken.None, timeout: 50);

            // Assert
            result
                .Should()
                .Be(default, "an empty idle queue returns the default tuple immediately");
        }

        [TestMethod]
        public async Task CompleteAddingAsync_NoJobsRunning_CompletesWithoutThrowing()
        {
            // Arrange: with _jobsRunning == 0 the while loop is skipped and CompleteAdding is called.
            var queue = NewQueue(CancellationToken.None);

            // Act
            await queue
                .Awaiting(q => q.CompleteAddingAsync(CancellationToken.None, timeout: 100))
                .Should()
                .NotThrowAsync("no running jobs means the method completes adding immediately");
        }

        [TestMethod]
        public async Task JobsToFinish_NoJobsRunning_CompletesImmediately()
        {
            // Arrange
            var queue = NewQueue(CancellationToken.None);

            // Act
            await queue
                .Awaiting(q => q.JobsToFinish(100, CancellationToken.None))
                .Should()
                .NotThrowAsync("with no jobs running the polling loop exits immediately");
        }
    }
}
