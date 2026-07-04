using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler;

namespace QuickFiler.Test.HelperClasses
{
    [DoNotParallelize]
    [TestClass]
    public class ViewerQueueStaticWrapperTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            EfcViewerQueue.ResetProductionCoreDefaultsForTesting();
            EfcViewerQueue.ResetCoreForTesting();
            ItemViewerQueue.ResetProductionCoreDefaultsForTesting();
            ItemViewerQueue.ResetCoreForTesting();
        }

        [TestMethod]
        public void EfcViewerQueue_BuildQueue_DelegatesToInjectedCore()
        {
            var created = 0;
            var scheduledPriorities = new List<DispatcherPriority>();
            var core = CreateEfcCore(
                () =>
                {
                    created++;
                    return CreateUninitialized<EfcViewer>();
                },
                priorityScheduler: (action, priority) =>
                {
                    scheduledPriorities.Add(priority);
                    action();
                }
            );
            EfcViewerQueue.SetCoreForTesting(core);

            EfcViewerQueue.BuildQueue(2);

            created.Should().Be(2);
            core.Count.Should().Be(2);
            scheduledPriorities
                .Should()
                .Equal(DispatcherPriority.Background, DispatcherPriority.Background);
        }

        [TestMethod]
        public void EfcViewerQueue_Dequeue_UsesInjectedCoreAndRestoresReplacementCount()
        {
            var created = 0;
            var scheduledPriorities = new List<DispatcherPriority>();
            var blockingPriorities = new List<DispatcherPriority>();
            var core = CreateEfcCore(
                () =>
                {
                    created++;
                    return CreateUninitialized<EfcViewer>();
                },
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
            EfcViewerQueue.SetCoreForTesting(core);

            EfcViewer viewer = EfcViewerQueue.Dequeue();

            viewer.Should().NotBeNull();
            created.Should().Be(3);
            core.Count.Should().Be(2);
            blockingPriorities.Should().Equal(DispatcherPriority.Render);
            scheduledPriorities
                .Should()
                .Equal(DispatcherPriority.Background, DispatcherPriority.Background);
        }

        [TestMethod]
        public void ItemViewerQueue_BuildMethods_DelegateToInjectedCore()
        {
            var created = 0;
            var scheduledPriorities = new List<DispatcherPriority>();
            var core = CreateItemCore(
                () =>
                {
                    created++;
                    return CreateUninitialized<ItemViewer>();
                },
                priorityScheduler: (action, priority) =>
                {
                    scheduledPriorities.Add(priority);
                    action();
                }
            );
            ItemViewerQueue.SetCoreForTesting(core);

            ItemViewerQueue.BuildQueueWhenIdle(1);
            ItemViewerQueue.BuildQueueBackground(1);
            ItemViewerQueue.BuildQueue(1);

            created.Should().Be(3);
            core.Count.Should().Be(3);
            scheduledPriorities
                .Should()
                .Equal(DispatcherPriority.ContextIdle, DispatcherPriority.Background);
        }

        [TestMethod]
        public void ItemViewerQueue_DequeueAndChunk_DelegateToInjectedCore()
        {
            var created = 0;
            var scheduledPriorities = new List<DispatcherPriority>();
            var blockingPriorities = new List<DispatcherPriority>();
            var core = CreateItemCore(
                () =>
                {
                    created++;
                    return CreateUninitialized<ItemViewer>();
                },
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
            ItemViewerQueue.SetCoreForTesting(core);

            ItemViewer viewer = ItemViewerQueue.Dequeue(CancellationToken.None);
            IEnumerable<ItemViewer> chunk = ItemViewerQueue.DequeueChunk(2);

            viewer.Should().NotBeNull();
            chunk.Should().HaveCount(2);
            created.Should().Be(4);
            core.Count.Should().Be(1);
            blockingPriorities.Should().Equal(DispatcherPriority.Render, DispatcherPriority.Render);
            scheduledPriorities
                .Should()
                .Equal(DispatcherPriority.ContextIdle, DispatcherPriority.ContextIdle);
        }

        [TestMethod]
        public void EfcViewerQueue_CreateProductionCore_UsesProvidedDelegates()
        {
            var created = 0;
            var scheduledPriorities = new List<DispatcherPriority>();
            var blockingPriorities = new List<DispatcherPriority>();
            var core = EfcViewerQueue.CreateProductionCore(
                () =>
                {
                    created++;
                    return CreateUninitialized<EfcViewer>();
                },
                action => action(),
                (action, priority) =>
                {
                    scheduledPriorities.Add(priority);
                    action();
                },
                (action, priority) =>
                {
                    blockingPriorities.Add(priority);
                    action();
                }
            );

            EfcViewer viewer = core.Dequeue(
                CancellationToken.None,
                DispatcherPriority.Render,
                1,
                2,
                DispatcherPriority.Background
            );

            viewer.Should().NotBeNull();
            created.Should().Be(3);
            core.Count.Should().Be(2);
            blockingPriorities.Should().Equal(DispatcherPriority.Render);
            scheduledPriorities
                .Should()
                .Equal(DispatcherPriority.Background, DispatcherPriority.Background);
        }

        [TestMethod]
        public void ItemViewerQueue_CreateProductionCore_UsesProvidedDelegates()
        {
            var created = 0;
            var scheduledPriorities = new List<DispatcherPriority>();
            var blockingPriorities = new List<DispatcherPriority>();
            var core = ItemViewerQueue.CreateProductionCore(
                () =>
                {
                    created++;
                    return CreateUninitialized<ItemViewer>();
                },
                action => action(),
                (action, priority) =>
                {
                    scheduledPriorities.Add(priority);
                    action();
                },
                (action, priority) =>
                {
                    blockingPriorities.Add(priority);
                    action();
                }
            );

            ItemViewer viewer = core.Dequeue(
                CancellationToken.None,
                DispatcherPriority.Render,
                1,
                1,
                DispatcherPriority.ContextIdle
            );

            viewer.Should().NotBeNull();
            created.Should().Be(2);
            core.Count.Should().Be(1);
            blockingPriorities.Should().Equal(DispatcherPriority.Render);
            scheduledPriorities.Should().Equal(DispatcherPriority.ContextIdle);
        }

        [TestMethod]
        public void EfcViewerQueue_ResetCoreForTesting_UsesResettableProductionDefaults()
        {
            var created = 0;
            var scheduledPriorities = new List<DispatcherPriority>();
            var blockingPriorities = new List<DispatcherPriority>();
            EfcViewerQueue.ProductionViewerFactory = () =>
            {
                created++;
                return CreateUninitialized<EfcViewer>();
            };
            EfcViewerQueue.ProductionPriorityScheduler = (action, priority) =>
            {
                scheduledPriorities.Add(priority);
                action();
            };
            EfcViewerQueue.ProductionBlockingPriorityScheduler = (action, priority) =>
            {
                blockingPriorities.Add(priority);
                action();
            };

            EfcViewerQueue.ResetCoreForTesting();
            EfcViewer viewer = EfcViewerQueue.Dequeue();

            viewer.Should().NotBeNull();
            created.Should().Be(3);
            blockingPriorities.Should().Equal(DispatcherPriority.Render);
            scheduledPriorities
                .Should()
                .Equal(DispatcherPriority.Background, DispatcherPriority.Background);
        }

        [TestMethod]
        public void ItemViewerQueue_ResetCoreForTesting_UsesResettableProductionDefaults()
        {
            var created = 0;
            var scheduledPriorities = new List<DispatcherPriority>();
            var blockingPriorities = new List<DispatcherPriority>();
            ItemViewerQueue.ProductionViewerFactory = () =>
            {
                created++;
                return CreateUninitialized<ItemViewer>();
            };
            ItemViewerQueue.ProductionPriorityScheduler = (action, priority) =>
            {
                scheduledPriorities.Add(priority);
                action();
            };
            ItemViewerQueue.ProductionBlockingPriorityScheduler = (action, priority) =>
            {
                blockingPriorities.Add(priority);
                action();
            };

            ItemViewerQueue.ResetCoreForTesting();
            ItemViewer viewer = ItemViewerQueue.Dequeue(CancellationToken.None);

            viewer.Should().NotBeNull();
            created.Should().Be(2);
            blockingPriorities.Should().Equal(DispatcherPriority.Render);
            scheduledPriorities.Should().Equal(DispatcherPriority.ContextIdle);
        }

        private static ViewerQueueCore<EfcViewer> CreateEfcCore(
            System.Func<EfcViewer> factory,
            System.Action<System.Action, DispatcherPriority> priorityScheduler = null,
            System.Action<System.Action, DispatcherPriority> blockingPriorityScheduler = null
        )
        {
            return new ViewerQueueCore<EfcViewer>(
                factory,
                action => action(),
                priorityScheduler ?? ((action, priority) => action()),
                blockingPriorityScheduler
            );
        }

        private static ViewerQueueCore<ItemViewer> CreateItemCore(
            System.Func<ItemViewer> factory,
            System.Action<System.Action, DispatcherPriority> priorityScheduler = null,
            System.Action<System.Action, DispatcherPriority> blockingPriorityScheduler = null
        )
        {
            return new ViewerQueueCore<ItemViewer>(
                factory,
                action => action(),
                priorityScheduler ?? ((action, priority) => action()),
                blockingPriorityScheduler
            );
        }

        private static TViewer CreateUninitialized<TViewer>()
            where TViewer : class
        {
            return (TViewer)FormatterServices.GetUninitializedObject(typeof(TViewer));
        }
    }
}
