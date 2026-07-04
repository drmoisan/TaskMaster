using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using UtilitiesCS;

namespace QuickFiler
{
    public static class ItemViewerQueue
    {
        private static ViewerQueueCore<ItemViewer> _core = CreateProductionCore();

        public static void BuildQueueWhenIdle(int count)
        {
            _core.BuildQueue(count, DispatcherPriority.ContextIdle);
        }

        public static void BuildQueueBackground(int count)
        {
            _core.BuildQueue(count, DispatcherPriority.Background);
        }

        public static void BuildQueue(int count)
        {
            _core.BuildQueue(count);
        }

        public static ItemViewer Dequeue(CancellationToken token)
        {
            return _core.Dequeue(
                token,
                DispatcherPriority.Render,
                1,
                1,
                DispatcherPriority.ContextIdle
            );
        }

        public static IEnumerable<ItemViewer> DequeueChunk(int count)
        {
            return _core.DequeueChunk(
                count,
                DispatcherPriority.Render,
                DispatcherPriority.ContextIdle
            );
        }

        /// <summary>
        /// Replaces the production queue core for deterministic unit tests.
        /// </summary>
        internal static void SetCoreForTesting(ViewerQueueCore<ItemViewer> core)
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

        private static ViewerQueueCore<ItemViewer> CreateProductionCore()
        {
            return CreateProductionCore(
                () => new ItemViewer(),
                action => action(),
                (action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority),
                (action, priority) => UiThread.Dispatcher.Invoke(action, priority)
            );
        }

        internal static ViewerQueueCore<ItemViewer> CreateProductionCore(
            Func<ItemViewer> viewerFactory,
            Action<Action> synchronousScheduler,
            Action<Action, DispatcherPriority> priorityScheduler,
            Action<Action, DispatcherPriority> blockingPriorityScheduler
        )
        {
            return new ViewerQueueCore<ItemViewer>(
                viewerFactory,
                synchronousScheduler,
                priorityScheduler,
                blockingPriorityScheduler
            );
        }
    }
}
