using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

namespace QuickFiler
{
    internal sealed class ViewerQueueCore<TViewer>
        where TViewer : class
    {
        private readonly Func<TViewer> _viewerFactory;
        private readonly Action<Action> _synchronousScheduler;
        private readonly Action<Action, DispatcherPriority> _priorityScheduler;
        private readonly Action<Action, DispatcherPriority> _blockingPriorityScheduler;
        private readonly Action<TViewer> _disposeViewer;
        private readonly Queue<TViewer> _queue = new Queue<TViewer>();

        internal ViewerQueueCore(
            Func<TViewer> viewerFactory,
            Action<Action> synchronousScheduler,
            Action<Action, DispatcherPriority> priorityScheduler,
            Action<Action, DispatcherPriority> blockingPriorityScheduler = null,
            Action<TViewer> disposeViewer = null
        )
        {
            _viewerFactory =
                viewerFactory ?? throw new ArgumentNullException(nameof(viewerFactory));
            _synchronousScheduler =
                synchronousScheduler
                ?? throw new ArgumentNullException(nameof(synchronousScheduler));
            _priorityScheduler =
                priorityScheduler ?? throw new ArgumentNullException(nameof(priorityScheduler));
            _blockingPriorityScheduler = blockingPriorityScheduler ?? _priorityScheduler;
            _disposeViewer = disposeViewer;
        }

        internal int Count => _queue.Count;

        internal int BuildQueue(int count)
        {
            ValidateCount(count);

            // Synchronous builds are used by callers that must have queued viewers available immediately.
            for (int i = 0; i < count; i++)
            {
                EnqueueWith(_synchronousScheduler);
            }

            return _queue.Count;
        }

        internal void BuildQueue(int count, DispatcherPriority priority)
        {
            ValidateCount(count);

            // Priority builds preserve production dispatcher behavior while tests can supply a deterministic scheduler.
            for (int i = 0; i < count; i++)
            {
                _priorityScheduler(() => _queue.Enqueue(_viewerFactory()), priority);
            }
        }

        internal TViewer Dequeue(
            CancellationToken cancellationToken,
            DispatcherPriority emptyQueuePriority,
            int cachedReplacementCount,
            int emptyReplacementCount,
            DispatcherPriority replacementPriority
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateCount(cachedReplacementCount);
            ValidateCount(emptyReplacementCount);

            if (_queue.Count > 0)
            {
                TViewer cachedViewer = _queue.Dequeue();
                BuildQueue(cachedReplacementCount, replacementPriority);
                return cachedViewer;
            }

            TViewer createdViewer = CreateWithPriority(emptyQueuePriority, cancellationToken);
            BuildQueue(emptyReplacementCount, replacementPriority);
            return createdViewer;
        }

        internal IReadOnlyList<TViewer> DequeueChunk(
            int count,
            DispatcherPriority missingViewerPriority,
            DispatcherPriority replacementPriority
        )
        {
            ValidateCount(count);

            int originalCount = _queue.Count;
            if (originalCount < count)
            {
                _blockingPriorityScheduler(
                    () => BuildQueue(count - originalCount),
                    missingViewerPriority
                );
            }

            BuildQueue(originalCount, replacementPriority);

            List<TViewer> viewers = new List<TViewer>();
            // Chunk dequeue returns the requested number after filling any shortfall synchronously.
            for (int i = 0; i < count; i++)
            {
                viewers.Add(_queue.Dequeue());
            }

            return viewers;
        }

        internal void Reset()
        {
            // Reset owns cleanup for static-wrapper tests so queued viewer instances do not leak between tests.
            while (_queue.Count > 0)
            {
                TViewer viewer = _queue.Dequeue();
                _disposeViewer?.Invoke(viewer);
            }
        }

        private TViewer CreateWithPriority(
            DispatcherPriority priority,
            CancellationToken cancellationToken
        )
        {
            TViewer viewer = null;
            _blockingPriorityScheduler(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    viewer = _viewerFactory();
                },
                priority
            );

            return viewer;
        }

        private void EnqueueWith(Action<Action> scheduler)
        {
            scheduler(() => _queue.Enqueue(_viewerFactory()));
        }

        private static void ValidateCount(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    count,
                    "Queue counts cannot be negative."
                );
            }
        }
    }
}
