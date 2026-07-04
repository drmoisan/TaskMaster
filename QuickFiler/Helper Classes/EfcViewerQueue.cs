using System;
using System.Threading;
using System.Windows.Threading;
using UtilitiesCS;

namespace QuickFiler
{
    public static class EfcViewerQueue
    {
        internal static Func<EfcViewer> ProductionViewerFactory { get; set; } =
            CreateProductionViewer;

        internal static Action<Action> ProductionSynchronousScheduler { get; set; } =
            action => action();

        internal static Action<
            Action,
            DispatcherPriority
        > ProductionPriorityScheduler { get; set; } =
            (action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority);

        internal static Action<
            Action,
            DispatcherPriority
        > ProductionBlockingPriorityScheduler { get; set; } = (action, priority) => action();

        private static ViewerQueueCore<EfcViewer> _core = CreateProductionCore();

        public static void BuildQueue(int count)
        {
            _core.BuildQueue(count, DispatcherPriority.Background);
        }

        public static EfcViewer Dequeue()
        {
            return _core.Dequeue(
                CancellationToken.None,
                DispatcherPriority.Render,
                1,
                2,
                DispatcherPriority.Background
            );
        }

        /// <summary>
        /// Replaces the production queue core for deterministic unit tests.
        /// </summary>
        internal static void SetCoreForTesting(ViewerQueueCore<EfcViewer> core)
        {
            _core = core ?? throw new System.ArgumentNullException(nameof(core));
        }

        /// <summary>
        /// Restores the production queue core after deterministic unit tests.
        /// </summary>
        internal static void ResetCoreForTesting()
        {
            _core.Reset();
            _core = CreateProductionCore();
        }

        internal static void ResetProductionCoreDefaultsForTesting()
        {
            ProductionViewerFactory = CreateProductionViewer;
            ProductionSynchronousScheduler = action => action();
            ProductionPriorityScheduler = (action, priority) =>
                _ = UiThread.Dispatcher.InvokeAsync(action, priority);
            ProductionBlockingPriorityScheduler = (action, priority) => action();
        }

        private static ViewerQueueCore<EfcViewer> CreateProductionCore()
        {
            return CreateProductionCore(
                ProductionViewerFactory,
                ProductionSynchronousScheduler,
                ProductionPriorityScheduler,
                ProductionBlockingPriorityScheduler
            );
        }

        private static EfcViewer CreateProductionViewer()
        {
            return new EfcViewer();
        }

        internal static ViewerQueueCore<EfcViewer> CreateProductionCore(
            Func<EfcViewer> viewerFactory,
            Action<Action> synchronousScheduler,
            Action<Action, DispatcherPriority> priorityScheduler,
            Action<Action, DispatcherPriority> blockingPriorityScheduler
        )
        {
            return new ViewerQueueCore<EfcViewer>(
                viewerFactory,
                synchronousScheduler,
                priorityScheduler,
                blockingPriorityScheduler
            );
        }
    }
}
