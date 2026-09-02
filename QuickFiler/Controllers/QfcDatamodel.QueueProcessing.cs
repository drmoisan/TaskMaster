using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    public partial class QfcDatamodel
    {
        /// <summary>
        /// Issue #424: honest producer-liveness signal. Set <see langword="true"/> immediately before
        /// each <c>RunWorkerAsync()</c> call and cleared in a <c>finally</c> once the awaited
        /// <c>RemainingEmailLoader</c> completes. <c>BackgroundWorker.IsBusy</c> cannot serve this
        /// role: <c>Worker_DoWork</c> is <c>async void</c>, so it returns at its first yielding await
        /// and reports idle while the loader is still producing. Both the dequeue gate's
        /// <c>sourceActive</c> signal and <see cref="WaitForQueue"/> consume this flag. Declared
        /// <c>volatile</c> because it is written on the worker thread and read by dequeue callers.
        /// </summary>
        private volatile bool _remainingLoadActive;

        //TODO: Implement UndoMove()
        public void UndoMove()
        {
            throw new NotImplementedException();
        }

        internal void TryUnhookOrReplace(ref List<MailItem> nodes, int i)
        {
            if (nodes is null || nodes.Count == 0 || nodes.Count < i + 1)
            {
                logger.Error(
                    $"Error unhooking item from move monitor. No items in array or index out of range. nodes.Length = {nodes?.Count ?? 0} but index i = {i}"
                );
                return;
            }
            var node = nodes[i];
            bool processing = true;
            while (processing)
            {
                try
                {
                    _moveMonitor.UnhookItem(node);
                    processing = false;
                }
                catch (System.Exception e)
                {
                    logger.Error(
                        $"Error unhooking item from move monitor. Getting next item from Queue {e.Message}"
                    );
                    nodes.Remove(node);
                    node = _masterQueue.TryTakeFirst();
                    if (node is null)
                    {
                        processing = false;
                    }
                    else
                    {
                        nodes.Insert(i, node);
                    }
                }
            }
        }

        public async Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut)
        {
            // Issue #424: the pre-existing two-argument contract is preserved exactly; it delegates
            // with the default first-batch deadline and no progress sink.
            return await DequeueNextItemGroupAsync(
                quantity,
                timeOut,
                QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
                null
            );
        }

        public async Task<IList<MailItem>> DequeueNextItemGroupAsync(
            int quantity,
            int timeOut,
            TimeSpan firstBatchDeadline,
            Action<int, int, int> progress
        )
        {
            _token.ThrowIfCancellationRequested();

            if (_globals?.QfSettings?.HighConfidenceModeEnabled == true)
            {
                return await DequeueWithHighConfidenceGateAsync(
                    quantity,
                    timeOut,
                    firstBatchDeadline,
                    progress
                );
            }

            // Normal mode neither scores nor reports scanning progress, so both arguments are moot.
            return await DequeueDirectAsync(quantity);
        }

        /// <summary>
        /// Issue #446. Outcome-bearing dequeue. In high-confidence mode the gate's own stop reason
        /// and accepted carriers are propagated verbatim. In normal mode nothing is scored, so
        /// <see cref="QfcDequeueBatch.PreScored"/> is empty and a short batch is reported as
        /// <see cref="QfcDequeueStop.SourceExhausted"/>: the direct path takes whatever the master
        /// queue holds after <see cref="WaitForQueue"/>, so fewer items than requested means the
        /// source could not supply them.
        /// </summary>
        public async Task<QfcDequeueBatch> DequeueNextItemGroupWithOutcomeAsync(
            int quantity,
            int timeOut,
            TimeSpan firstBatchDeadline,
            Action<int, int, int> progress
        )
        {
            _token.ThrowIfCancellationRequested();

            if (_globals?.QfSettings?.HighConfidenceModeEnabled == true)
            {
                return await DequeueWithHighConfidenceGateWithOutcomeAsync(
                    quantity,
                    timeOut,
                    firstBatchDeadline,
                    progress
                );
            }

            IList<MailItem> items = await DequeueDirectAsync(quantity);
            return new QfcDequeueBatch(
                items,
                new List<QfcPreScoredItem>(),
                (items?.Count ?? 0) < quantity
                    ? QfcDequeueStop.SourceExhausted
                    : QfcDequeueStop.QuantitySatisfied
            );
        }

        private async Task<IList<MailItem>> DequeueDirectAsync(int quantity)
        {
            if (_masterQueue.Count < quantity)
                await WaitForQueue(quantity, _token);

            var nodes = _masterQueue.TryTakeFirst(quantity)?.ToList();
            return UnhookDequeuedNodes(nodes);
        }

        private async Task<IList<MailItem>> DequeueWithHighConfidenceGateAsync(
            int quantity,
            int timeOut,
            TimeSpan? firstBatchDeadline = null,
            Action<int, int, int> progress = null
        )
        {
            QfcDequeueBatch batch = await DequeueWithHighConfidenceGateWithOutcomeAsync(
                quantity,
                timeOut,
                firstBatchDeadline,
                progress
            );
            return batch.Items;
        }

        /// <summary>
        /// Issue #446 and Scope 427-A. The high-confidence dequeue with the gate's outcome intact.
        /// <see cref="QfcDequeueBatch.Items"/> is taken from the same accepted set as
        /// <see cref="QfcDequeueBatch.PreScored"/>, after <see cref="UnhookDequeuedNodes"/> has run
        /// over it, so the two collections describe one dequeue rather than two.
        /// </summary>
        private async Task<QfcDequeueBatch> DequeueWithHighConfidenceGateWithOutcomeAsync(
            int quantity,
            int timeOut,
            TimeSpan? firstBatchDeadline = null,
            Action<int, int, int> progress = null
        )
        {
            var gate = new QfcStreamingDequeueConfidenceGate(
                () => _masterQueue.TryTakeFirst(),
                ScoreRemainingQueueMailItemAsync,
                _globals.QfSettings.HighConfidenceThreshold,
                TimeProvider,
                null,
                () => _remainingLoadActive,
                firstBatchDeadline,
                progress,
                onRejected: TryReleaseRejectedHook
            );

            QfcGateBatch batch = await gate.DequeueAsync(quantity, timeOut, _token);
            IList<QfcPreScoredItem> accepted = batch.Accepted;
            var nodes = accepted.Select(x => x.MailItem).ToList();
            return new QfcDequeueBatch(UnhookDequeuedNodes(nodes), accepted, batch.Stop);
        }

        /// <summary>
        /// Issue #426. Releases the <c>EmailMoveMonitor</c> hook of a candidate the high-confidence
        /// gate discarded. The rejected candidate is already out of the master queue and never
        /// reaches <see cref="UnhookDequeuedNodes"/>, so without this its hook and its live COM
        /// reference are retained for the session. Exactly one <c>UnhookItem</c> call per rejected
        /// item preserves the one-marshal-hop-per-operation contract. A monitor failure is logged
        /// and swallowed: the candidate is discarded either way and aborting the scan would strand
        /// the rest of the batch.
        /// </summary>
        private void TryReleaseRejectedHook(MailItem item)
        {
            try
            {
                _moveMonitor.UnhookItem(item);
            }
            catch (System.Exception e)
            {
                logger.Error("Error unhooking rejected item from move monitor", e);
                return;
            }
        }

        public IList<MailItem> DequeueNextItemGroup(int quantity)
        {
            _token.ThrowIfCancellationRequested();

            if (_globals?.QfSettings?.HighConfidenceModeEnabled == true)
            {
                return DequeueWithHighConfidenceGateAsync(quantity, 0).GetAwaiter().GetResult();
            }

            var nodes = _masterQueue.TryTakeFirst(quantity)?.ToList();
            return UnhookDequeuedNodes(nodes);
        }

        private IList<MailItem> UnhookDequeuedNodes(List<MailItem> nodes)
        {
            if (nodes is null)
            {
                return null;
            }

            try
            {
                var max = nodes.Count;
                for (int i = 0; i < max; i++)
                {
                    TryUnhookOrReplace(ref nodes, i);
                }
            }
            catch (System.Exception e)
            {
                logger.Error("Error unhooking items from move monitor", e);
                throw;
            }
            return nodes;
        }

        /// <summary>
        /// Injectable factory for the master-queue admission scorer. Defaults to a fresh
        /// <see cref="FolderScoringService"/> so production behaviour is unchanged; tests assign a
        /// factory returning a mock so <see cref="ScoreRemainingQueueMailItemAsync"/> can be driven
        /// without a live Outlook session, which
        /// <c>.claude/rules/general-unit-test.md</c> UT4 requires.
        /// </summary>
        internal Func<IFolderScoringService> ScoringServiceFactory { get; set; } =
            () => new FolderScoringService();

        private async Task<(
            long Score,
            string TopFolder,
            IFolderSearchHandler Handler
        )> ScoreRemainingQueueMailItemAsync(MailItem mailItem, CancellationToken cancel)
        {
            var scoringService = ScoringServiceFactory();
            var score = await scoringService
                .ScoreAsync(mailItem, _globals, cancel)
                .ConfigureAwait(false);
            logger.Debug(
                $"Probability debug [QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)] "
                    + $"Subject='{mailItem.Subject}' EntryID='{mailItem.EntryID}' Score={score.Score}"
            );
            // Issue #678: forward the initialised handler as the third element so it reaches
            // QfcGateBatch.Accepted and, through it, QfcDequeueBatch.PreScored.
            return (score.Score, score.TopFolder, score.Handler);
        }

        internal async Task WaitForQueue(int quantity, CancellationToken token)
        {
            while (_remainingLoadActive && (_masterQueue?.Count < quantity))
            {
                token.ThrowIfCancellationRequested();
                await TimeProvider.Delay(TimeSpan.FromMilliseconds(200), token);
            }
        }
    }
}
