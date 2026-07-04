using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler;

namespace QuickFiler.Test.HelperClasses
{
    [TestClass]
    public class ViewerQueueCoreTests
    {
        [TestMethod]
        public void BuildQueue_WithSynchronousScheduler_CreatesRequestedViewers()
        {
            var created = 0;
            var core = CreateCore(() => new FakeViewer(++created));

            int count = core.BuildQueue(3);

            count.Should().Be(3);
            created.Should().Be(3);
            core.Count.Should().Be(3);
        }

        [TestMethod]
        public void Dequeue_WithCachedViewer_ReturnsCachedAndSchedulesReplacement()
        {
            var scheduledPriorities = new List<DispatcherPriority>();
            var created = 0;
            var core = CreateCore(
                () => new FakeViewer(++created),
                priorityScheduler: (action, priority) =>
                {
                    scheduledPriorities.Add(priority);
                    action();
                }
            );
            core.BuildQueue(1);

            FakeViewer viewer = core.Dequeue(
                CancellationToken.None,
                DispatcherPriority.Render,
                1,
                2,
                DispatcherPriority.Background
            );

            viewer.Id.Should().Be(1);
            created.Should().Be(2);
            core.Count.Should().Be(1);
            scheduledPriorities.Should().Equal(DispatcherPriority.Background);
        }

        [TestMethod]
        public void Dequeue_WithEmptyQueue_CreatesViewerAndSchedulesConfiguredReplacementCount()
        {
            var scheduledPriorities = new List<DispatcherPriority>();
            var blockingPriorities = new List<DispatcherPriority>();
            var created = 0;
            var core = CreateCore(
                () => new FakeViewer(++created),
                priorityScheduler: (action, priority) =>
                {
                    scheduledPriorities.Add(priority);
                    action();
                },
                blockingPriorityScheduler: (action, priority) =>
                {
                    blockingPriorities.Add(priority);
                    action();
                }
            );

            FakeViewer viewer = core.Dequeue(
                CancellationToken.None,
                DispatcherPriority.Render,
                1,
                2,
                DispatcherPriority.ContextIdle
            );

            viewer.Id.Should().Be(1);
            created.Should().Be(3);
            core.Count.Should().Be(2);
            blockingPriorities.Should().Equal(DispatcherPriority.Render);
            scheduledPriorities
                .Should()
                .Equal(DispatcherPriority.ContextIdle, DispatcherPriority.ContextIdle);
        }

        [TestMethod]
        public void Dequeue_WithCanceledToken_ThrowsBeforeCreatingViewer()
        {
            var created = 0;
            using (var source = new CancellationTokenSource())
            {
                source.Cancel();
                var core = CreateCore(() => new FakeViewer(++created));

                Action act = () =>
                    core.Dequeue(
                        source.Token,
                        DispatcherPriority.Render,
                        1,
                        1,
                        DispatcherPriority.Background
                    );

                act.Should().Throw<OperationCanceledException>();
                created.Should().Be(0);
                core.Count.Should().Be(0);
            }
        }

        [TestMethod]
        public void DequeueChunk_WhenQueueIsShort_FillsShortfallAndSchedulesOriginalCountReplacement()
        {
            var scheduledPriorities = new List<DispatcherPriority>();
            var blockingPriorities = new List<DispatcherPriority>();
            var created = 0;
            var core = CreateCore(
                () => new FakeViewer(++created),
                priorityScheduler: (action, priority) =>
                {
                    scheduledPriorities.Add(priority);
                    action();
                },
                blockingPriorityScheduler: (action, priority) =>
                {
                    blockingPriorities.Add(priority);
                    action();
                }
            );
            core.BuildQueue(1);

            IReadOnlyList<FakeViewer> viewers = core.DequeueChunk(
                3,
                DispatcherPriority.Render,
                DispatcherPriority.ContextIdle
            );

            viewers.Select(viewer => viewer.Id).Should().Equal(1, 2, 3);
            created.Should().Be(4);
            core.Count.Should().Be(1);
            blockingPriorities.Should().Equal(DispatcherPriority.Render);
            scheduledPriorities.Should().Equal(DispatcherPriority.ContextIdle);
        }

        [TestMethod]
        public void Reset_DisposesQueuedViewersAndClearsQueue()
        {
            var disposed = new List<int>();
            var created = 0;
            var core = CreateCore(
                () => new FakeViewer(++created),
                disposeViewer: viewer => disposed.Add(viewer.Id)
            );
            core.BuildQueue(2);

            core.Reset();

            core.Count.Should().Be(0);
            disposed.Should().Equal(1, 2);
        }

        private static ViewerQueueCore<FakeViewer> CreateCore(
            Func<FakeViewer> factory,
            Action<Action, DispatcherPriority> priorityScheduler = null,
            Action<Action, DispatcherPriority> blockingPriorityScheduler = null,
            Action<FakeViewer> disposeViewer = null
        )
        {
            return new ViewerQueueCore<FakeViewer>(
                factory,
                action => action(),
                priorityScheduler ?? ((action, priority) => action()),
                blockingPriorityScheduler,
                disposeViewer
            );
        }

        private sealed class FakeViewer
        {
            internal FakeViewer(int id)
            {
                Id = id;
            }

            internal int Id { get; }
        }
    }
}
