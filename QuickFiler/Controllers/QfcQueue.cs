using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace QuickFiler.Controllers
{
    public partial class QfcQueue(
        CancellationToken token,
        QfcHomeController homeController,
        IApplicationGlobals appGlobals
    ) : IQfcQueue
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Constructors and Private Members

        private CancellationToken _token = token;
        private QfcHomeController _homeController = homeController;
        private IQfcCollectionController _qfcCollectionController;
        private IApplicationGlobals _globals = appGlobals;

        private int _jobsRunning = 0;
        private BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> _queue =
        [];

        // Deliberately one monitor instance per owner, not a shared singleton: EmailMoveMonitor.BeforeItemMove dispatches at most one action per MailItem via FirstOrDefault, and UnhookAll is instance-scoped and clears the whole hook list, so a shared instance would both drop sibling owners' actions and unhook them all on any one owner's teardown (issue #731 finding 1, issue #620).
        private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();

        /// <summary>
        /// Issue #871 injectable seam S1 for the move monitor every enqueued item is hooked into.
        /// The default remains the per-owner <see cref="EmailMoveMonitor"/> instance the field
        /// initializer immediately above creates, so production behaviour is unchanged; a test
        /// assigns a substitute so the hook call can be asserted without a live Outlook process.
        /// The member is <c>internal</c> rather than public because
        /// <see cref="IEmailMoveMonitor"/> is itself internal and a public member of an internal
        /// type is an inconsistent-accessibility error. The backing field is retained rather than
        /// converted to an auto-property because six existing tests resolve it by reflection under
        /// its current name, which an auto-property would rename to a compiler-generated one.
        /// </summary>
        /// <exception cref="ArgumentNullException">The assigned value is null.</exception>
        internal IEmailMoveMonitor MoveMonitor
        {
            get => _moveMonitor;
            set => _moveMonitor = value ?? throw new ArgumentNullException(nameof(value));
        }

        #endregion Constructors and Private Members

        #region Queue Functions

        public async Task CompleteAddingAsync(CancellationToken token, int timeout)
        {
            CancellationTokenSource functionTimeoutSource = new CancellationTokenSource(timeout);
            CancellationTokenSource linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(token, functionTimeoutSource.Token);

            try
            {
                while (_jobsRunning > 0)
                {
                    //logger.Debug($"{nameof(CompleteAddingAsync)} waiting for {_jobsRunning} jobs to complete");
                    await Task.Delay(100, linkedTokenSource.Token);
                }
                _queue.CompleteAdding();
            }
            catch (OperationCanceledException e)
            {
                if (!token.IsCancellationRequested)
                {
                    logger.Info(
                        $"{nameof(CompleteAddingAsync)} timed out after {timeout} milliseconds"
                    );
                }
                throw e;
            }
        }

        public (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) Dequeue()
        {
            (TableLayoutPanel tlp, List<QfcItemGroup> itemGroups) = _queue.Take();
            itemGroups.ForEach(group => _moveMonitor.UnhookItem(group.MailItem));
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _queue)
            );
            return (tlp, itemGroups);
        }

        public async Task<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> TryDequeueAsync(
            CancellationToken token,
            int timeout
        )
        {
            //TraceUtility.LogMethodCall(token, timeout);

            token.ThrowIfCancellationRequested();

            if (_queue.Count == 0 && _jobsRunning == 0)
            {
                //logger.Debug($"{nameof(TryDequeueAsync)} attempted with no jobs running and nothing in the queue. Returning default.");
                return default;
            }

            var functionTimeoutSource = new CancellationTokenSource(timeout);
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                token,
                functionTimeoutSource.Token
            );

            int queueTimeout = Math.Min(timeout, 100);
            int pollInterval = 100;

            (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) result = default;
            try
            {
                while (
                    !_queue.IsCompleted
                    && !token.IsCancellationRequested
                    && result == default
                    && _queue.Count + _jobsRunning > 0
                )
                {
                    if (!_queue.TryTake(out result, queueTimeout, token))
                    {
                        //logger.Debug($"{nameof(TryDequeueAsync)} attempted to take before {_jobsRunning} queuing job(s) are complete. Waiting {pollInterval} milliseconds");
                        await Task.Delay(pollInterval, token);
                    }
                }
                if (_queue.IsCompleted && _queue.Count > 0)
                {
                    _queue.TryTake(out result, queueTimeout, token);
                }
                if (result != default)
                {
                    result.ItemGroups.ForEach(group => _moveMonitor.UnhookItem(group.MailItem));
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            _queue
                        )
                    );
                }
            }
            catch (OperationCanceledException)
            {
                if (!token.IsCancellationRequested)
                {
                    logger.Debug(
                        $"{nameof(TryDequeueAsync)} timed out after {timeout} milliseconds"
                    );
                }
                else
                {
                    logger.Debug($"{nameof(TryDequeueAsync)} was cancelled");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(
                    $"{nameof(TryDequeueAsync)} failed to dequeue. \n {e.Message}\n{e.StackTrace}"
                );
            }

            return result;
        }

        public async Task RemoveItem(MailItem mailItem)
        {
            //TraceUtility.LogMethodCall(mailItem);

            BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> bc;

            // Wait for all jobs to finish to prevent conflicts.
            // Guard against a pre-cancelled _token: when the instance is being torn down the
            // token may already be cancelled before the move-monitor callback fires, causing
            // JobsToFinish to throw. In that case cleanup is moot — exit gracefully.
            try
            {
                await JobsToFinish(100, _token);
            }
            catch (OperationCanceledException) when (_token.IsCancellationRequested)
            {
                logger.Debug(
                    $"{nameof(RemoveItem)} exiting early: instance token is already cancelled"
                );
                return;
            }

            bc = Interlocked.Exchange(ref _queue, []);

            Interlocked.Increment(ref _jobsRunning);

            var list = bc.ToList();

            foreach (var entry in list)
            {
                if (entry.ItemGroups.Any(group => group.MailItem.EntryID == mailItem.EntryID))
                {
                    await UiIdleCallAsync(() =>
                    {
                        var idx = entry.ItemGroups.FindIndex(group =>
                            group.MailItem.EntryID == mailItem.EntryID
                        );
                        entry.Tlp.RemoveSpecificRow(idx);
                        entry.ItemGroups.RemoveAt(idx);
                        RenumberGroups(entry.ItemGroups);
                    });
                }
                _queue.Add(entry);
            }

            Interlocked.Decrement(ref _jobsRunning);
        }

        // EnqueueAsync lives in the partial part QfcQueue.Enqueue.cs; see that file for the reason.

        public async Task JobsToFinish(int pollInterval, CancellationToken token)
        {
            while (JobsRunning > 0)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(pollInterval, token);
            }
        }

        public int Count => _queue.Count;

        public int JobsRunning => _jobsRunning;

        #endregion Queue Functions

        // The Tlp Manipulation region lives in the partial part QfcQueue.Tlp.cs; see that file.

        #region INotify

        protected void NotifyPropertyChanged(
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = ""
        )
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        // The Helper Methods region lives in the partial part QfcQueue.UiIdle.cs; see that file.
    }
}
