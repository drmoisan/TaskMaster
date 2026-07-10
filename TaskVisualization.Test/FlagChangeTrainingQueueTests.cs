using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using UtilitiesCS.Interfaces;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for <see cref="FlagChangeTrainingQueue"/>. The 500ms timer is never
    /// awaited; the Immediate path is driven synchronously via the internal Consumer
    /// task and the Timed path is asserted via enqueue state. No wall-clock wait,
    /// sleep, form, popup, or temp file is used.
    /// </summary>
    [TestClass]
    public class FlagChangeTrainingQueueTests
    {
        [TestMethod]
        public void Init_ReturnsSelf_AndSetsConsumerTimer()
        {
            var queue = new FlagChangeTrainingQueue();

            var result = queue.Init();

            result.Should().BeSameAs(queue);
            queue.ConsumerTimer.Should().NotBeNull();
        }

        [TestMethod]
        public async Task Enqueue_Immediate_InvokesProcessGroup_DrainsQueue()
        {
            var queue = new FlagChangeTrainingQueue
            {
                Options = IFlagChangeTrainingQueue.QueueOptions.Immediate,
            };
            var group = new Mock<IFlagChangeGroup>();
            group
                .Setup(x => x.ProcessGroupAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            queue.Enqueue(group.Object);
            await queue.Consumer;

            group.Verify(x => x.ProcessGroupAsync(It.IsAny<CancellationToken>()), Times.Once);
            queue.Queue.Should().BeEmpty();
        }

        [TestMethod]
        public void Enqueue_Timed_AddsItemToQueue()
        {
            // ConsumerTimer is intentionally left uninitialized (Init not called) so
            // no wall-clock timer is scheduled, keeping the test deterministic. The
            // Timed branch still executes (null-conditional RequestOrResetTask no-op).
            var queue = new FlagChangeTrainingQueue
            {
                Options = IFlagChangeTrainingQueue.QueueOptions.Timed,
            };
            var group = new Mock<IFlagChangeGroup>().Object;

            queue.Enqueue(group);

            queue.Queue.Should().ContainSingle().Which.Should().BeSameAs(group);
        }
    }
}
