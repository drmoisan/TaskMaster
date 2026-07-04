using System;
using System.Threading;
using System.Windows.Threading;
using UtilitiesCS;

namespace QuickFiler
{
    public static class EfcViewerQueue
    {
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

        private static ViewerQueueCore<EfcViewer> CreateProductionCore()
        {
            return CreateProductionCore(
                () => new EfcViewer(),
                action => action(),
                (action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority),
                (action, priority) => action()
            );
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
