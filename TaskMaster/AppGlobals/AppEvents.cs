using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using TaskMaster.Properties;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Fields;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace TaskMaster
{
    public class AppEvents : IAppEvents
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private static string DescribeSynchronizationContext(SynchronizationContext syncContext)
        {
            return syncContext?.GetType().FullName ?? "null";
        }

        private static string BuildStartupTimingContext(bool startupActive)
        {
            var syncContext = SynchronizationContext.Current;
            return $"threadId={Thread.CurrentThread.ManagedThreadId}; syncContext={DescribeSynchronizationContext(syncContext)}; startup-active={startupActive}";
        }

        private static void LogStartupTiming(
            string phase,
            bool startupActive,
            string details = null
        )
        {
            var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
            var phaseLabel = phase.StartsWith("[Startup timing]", StringComparison.Ordinal)
                ? phase
                : $"[Startup timing] {phase}";
            logger.Debug(
                $"{phaseLabel} | {BuildStartupTimingContext(startupActive)}{detailSegment}"
            );
        }

        public AppEvents(IApplicationGlobals globals)
        {
            Globals = globals;
        }

        //public ConcurrentBag<IConditionalEngine<MailItemHelper>> InboxEngines {get; protected set; } = [];

        internal async Task<AppEvents> LoadAsync()
        {
            var loadStopwatch = Stopwatch.StartNew();
            LogStartupTiming(
                "[Startup timing] LoadAsync start | startup-active status",
                true,
                "deferred processing window pending"
            );

            if (Settings.Default.EventsHooked)
            {
                LogStartupTiming("LoadAsync startup hook dispatch | startup hook", true);
                Hook();
                LogStartupTiming("LoadAsync startup hook complete | startup hook", true);
            }
            else
            {
                LogStartupTiming(
                    "LoadAsync startup hook skipped | startup hook",
                    true,
                    "EventsHooked=false"
                );
            }

            LogStartupTiming(
                "[Startup timing] LoadAsync entering deferred processing window before await ProcessNewInboxItemsAsync()",
                true,
                "deferred processing window"
            );
            await ProcessNewInboxItemsAsync();
            LogStartupTiming(
                "LoadAsync complete | startup-active status",
                false,
                $"elapsedMs={loadStopwatch.ElapsedMilliseconds}"
            );
            return this;
        }

        internal IApplicationGlobals Globals { get; set; }

        private Items _olToDoItems;
        public Items OlToDoItems
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _olToDoItems; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set
            {
                if (_olToDoItems != null)
                {
                    _olToDoItems.ItemAdd -= OlToDoItems_ItemAdd;
                    _olToDoItems.ItemChange -= OlToDoItems_ItemChange;
                }

                _olToDoItems = value;
                if (_olToDoItems != null)
                {
                    _olToDoItems.ItemAdd += OlToDoItems_ItemAdd;
                    _olToDoItems.ItemChange += OlToDoItems_ItemChange;
                }
            }
        }

        //private Items _olInboxItems;
        //private Items OlInboxItems
        //{
        //    [MethodImpl(MethodImplOptions.Synchronized)]
        //    get
        //    {
        //        return _olInboxItems;
        //    }

        //    [MethodImpl(MethodImplOptions.Synchronized)]
        //    set
        //    {
        //        if (_olInboxItems != null)
        //        {
        //            _olInboxItems.ItemAdd -= OlInboxItems_ItemAdd;
        //        }
        //        _olInboxItems = value;
        //        if (_olInboxItems != null)
        //        {
        //            _olInboxItems.ItemAdd += OlInboxItems_ItemAdd;
        //        }
        //    }
        //}

        internal LockingLinkedList<Items> OlInboxes { get; set; } = new();

        private Reminders _olReminders;
        private Reminders OlReminders
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _olReminders; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { _olReminders = value; }
        }

        #region Events

        public void Hook()
        {
            LogStartupTiming("Hook start | startup hook", true);

            // Issue #207 corrective fix: the three readiness-dependent COM hookups are no longer
            // run synchronously/unconditionally at Hook() entry (which could block the STA for a
            // prolonged period during a cold start before the store is ready). Instead they are run
            // by PerformReadinessHookup() exactly once, driven by a DispatcherTimer poll through the
            // pure HookReadinessCoordinator once the OutlookReadinessGate reports the store ready.
            // The Dispatcher keeps pumping between ticks, so the STA is never blocked.
            var gate = new OutlookReadinessGate(Globals.Ol.App);
            var coordinator = new HookReadinessCoordinator(gate, PerformReadinessHookup);

            const int initialIntervalSeconds = 1;
            const int backoffIntervalSeconds = 5;
            const int backoffAfterTicks = 10;

            var pollTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(initialIntervalSeconds),
            };
            int tickCount = 0;
            pollTimer.Tick += (sender, e) =>
            {
                tickCount++;
                if (coordinator.Tick() == HookReadinessTickResult.Completed)
                {
                    pollTimer.Stop();
                    return;
                }

                // Cadence backoff: after a threshold of not-ready ticks, slow the poll so an
                // extended not-ready startup does not poll aggressively. Polling never gives up.
                if (tickCount == backoffAfterTicks)
                {
                    pollTimer.Interval = TimeSpan.FromSeconds(backoffIntervalSeconds);
                }
            };
            pollTimer.Start();
        }

        /// <summary>
        /// Performs the three readiness-dependent COM hookups synchronously on the STA in their
        /// original order: subscribe to the ToDo folder items, capture the reminders collection,
        /// and subscribe each inbox's <c>ItemAdd</c>. Invoked exactly once by
        /// <see cref="HookReadinessCoordinator"/> when the <see cref="OutlookReadinessGate"/>
        /// reports the store ready. Retains the increment-1/2 per-operation
        /// <see cref="Stopwatch"/> timing and the consolidated <c>Hook complete</c>
        /// <c>[Startup timing]</c> log line. A transient not-ready <see cref="System.Runtime.InteropServices.COMException"/>
        /// raised here is routed back to the coordinator as a retry; the subscription is never
        /// silently dropped.
        /// </summary>
        private void PerformReadinessHookup()
        {
            var hookStopwatch = Stopwatch.StartNew();

            var toDoItemsStopwatch = Stopwatch.StartNew();
            OlToDoItems = Globals.Ol.ToDoFolder.Items;
            toDoItemsStopwatch.Stop();

            var remindersStopwatch = Stopwatch.StartNew();
            OlReminders = Globals.Ol.OlReminders;
            remindersStopwatch.Stop();

            var inboxSubscribeStopwatch = Stopwatch.StartNew();
            Globals.Ol.Inboxes.ForEach(x =>
                OlInboxes.AddLast(x.Items, items => items.ItemAdd += OlInboxItems_ItemAdd)
            );
            inboxSubscribeStopwatch.Stop();

            LogStartupTiming(
                "Hook complete | startup hook",
                true,
                $"elapsedMs={hookStopwatch.ElapsedMilliseconds}; inboxSubscriptions={OlInboxes.Count()}; "
                    + $"toDoItemsMs={toDoItemsStopwatch.Elapsed.TotalMilliseconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}; "
                    + $"remindersMs={remindersStopwatch.Elapsed.TotalMilliseconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}; "
                    + $"inboxSubscribeMs={inboxSubscribeStopwatch.Elapsed.TotalMilliseconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}"
            );
        }

        public void Unhook()
        {
            OlToDoItems = null;
            OlReminders = null;
            OlInboxes.Clear(items => items.ItemAdd -= OlInboxItems_ItemAdd);
        }

        internal async Task LogAsync(string message)
        {
            await Task.Run(() => logger.Debug(message));
        }

        private void OlToDoItems_ItemAdd(object item)
        {
            ToDoEvents.OlToDoItems_ItemAdd(item, Globals);
        }

        private async void OlToDoItems_ItemChange(object item)
        {
            try
            {
                await ToDoEvents.OlToDoItems_ItemChange(item, OlToDoItems, Globals);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        internal async void OlInboxItems_ItemAdd(object item)
        {
            try
            {
                await ProcessMailItemAsync(item);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<bool> ProcessMailItemAsync(object item)
        {
            if (item is MailItem mailItem)
            {
                var enginesAvailable = Globals
                    .Engines.InboxEngines.Where(kvp => kvp.Value is not null)
                    .ToArray();
                var enginesApplicable = await enginesAvailable
                    .ToAsyncEnumerable()
                    .Where(kvp => kvp.Value is not null)
                    .WhereAwait(async kvp => await kvp.Value.AsyncCondition(mailItem))
                    .Where(kvp => kvp.Value.Engine is not null)
                    .ToArrayAsync();

                if (!enginesAvailable.Any())
                {
                    logger.Debug("No engines available");
                    return false;
                }
                else if (enginesApplicable.Length > 0)
                {
                    // STA primitive extraction (Issue #207, AC5): materialize the COM-backed helper
                    // and its tokenized projection on the STA before any pure-compute offload. The
                    // FromMailItemAsync materialization touches live COM and must stay on the STA;
                    // the tokenization compute over the already-extracted values is offloaded to a
                    // worker via Task.Run.
                    var helper = await MailItemHelper.FromMailItemAsync(
                        mailItem,
                        Globals,
                        default,
                        false
                    );
                    await Task.Run(() => _ = helper.Tokens);

                    // Release the STA so the message pump keeps running while the engine
                    // condition/action compute proceeds; only pure compute on already-extracted
                    // primitives runs off the STA. Engine AsyncCondition/AsyncAction calls that
                    // touch live COM members on mailItem/helper.Item marshal back to the STA.
                    await Task.Yield();
                    await enginesApplicable
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async e => await e.Value.AsyncAction(helper));

                    // Marshal back to the STA for the COM write that marks the item processed.
                    helper.Item.SetUdf("AutoProcessed", true, OlUserPropertyType.olYesNo);
                    return true;
                }
                else
                {
                    logger.Debug(
                        $"No applicable engines for item with Subject: {mailItem.Subject}"
                    );
                    mailItem.SetUdf("AutoProcessed", true, OlUserPropertyType.olYesNo);
                }
            }
            else
            {
                var olItem = new OutlookItem(item);
                logger.Debug(
                    $"Skipping item of type {olItem.Try().GetOlItemType()} with Subject: {olItem.Try().Subject}"
                );
            }
            return false;
        }

        public async Task ProcessNewInboxItemsAsync()
        {
            var processingStopwatch = Stopwatch.StartNew();
            LogStartupTiming("ProcessNewInboxItemsAsync start | startup-active status", true);

            if (!OlInboxes.IsNullOrEmpty())
            {
                // Restrict to unprocessed items
                string filter = $"@SQL=\"{MAPIFields.Schemas.CustomPrefix}AutoProcessed\" is null";
                var unprocessedQueue = new ConcurrentQueue<object>();
                var inboxIndex = 0;

                foreach (var inbox in OlInboxes)
                {
                    inboxIndex++;
                    var restrictionStopwatch = Stopwatch.StartNew();
                    LogStartupTiming(
                        "ProcessNewInboxItemsAsync startup inbox restriction start | inbox restriction",
                        true,
                        $"inboxIndex={inboxIndex}; filter={filter}"
                    );

                    var olMailItems = inbox.Restrict("[MessageClass] = 'IPM.Note'");
                    var unprocessedItems = olMailItems
                        ?.Restrict(filter)
                        ?.Cast<object>()
                        .Where(x => x is MailItem)
                        .Cast<MailItem>()
                        .Where(x => x.UserProperties.Find("AutoProcessed") is null)
                        .ToArray();

                    LogStartupTiming(
                        "ProcessNewInboxItemsAsync startup inbox restriction complete | inbox restriction",
                        true,
                        $"inboxIndex={inboxIndex}; restrictedCount={(unprocessedItems?.Length ?? 0)}; elapsedMs={restrictionStopwatch.ElapsedMilliseconds}"
                    );

                    //var unprocessedItems = olMailItems?.Restrict("[AutoProcessed] Is Null")?.Cast<object>();
                    if (unprocessedItems is null)
                    {
                        continue;
                    }
                    unprocessedItems.ForEach(x => unprocessedQueue.Enqueue(x));
                }

                int errors = 0;
                int success = 0;
                var unprocessedCount = unprocessedQueue.Count();
                const int startupBatchSize = 10;
                logger.Debug($"Unprocessed queue has {unprocessedCount} items");

                LogStartupTiming(
                    "ProcessNewInboxItemsAsync interactive checkpoint | startup-active status",
                    true,
                    $"interactive checkpoint; unprocessedCount={unprocessedCount}"
                );
                var batchNumber = 0;

                while (unprocessedQueue.Count > 0)
                {
                    batchNumber++;
                    var batchStopwatch = Stopwatch.StartNew();
                    var batchRemainingAtStart = unprocessedQueue.Count();
                    var batchTarget = Math.Min(startupBatchSize, batchRemainingAtStart);
                    LogStartupTiming(
                        "ProcessNewInboxItemsAsync startup inbox batch start | batch processing",
                        true,
                        $"batch={batchNumber}; remaining={batchRemainingAtStart}; batchSize={batchTarget}"
                    );

                    var processedThisBatch = 0;
                    while (processedThisBatch < batchTarget && unprocessedQueue.Count > 0)
                    {
                        var remaining = unprocessedQueue.Count();
                        if (
                            unprocessedQueue.TryDequeue(out var item)
                            && await ProcessMailItemAsync(item)
                        )
                        {
                            success++;
                            processedThisBatch++;
                            logger.Debug(
                                $"Successfully processed item {success + errors} of {unprocessedCount} in the unprocessed Queue"
                            );
                        }
                        else if (++errors == 3)
                        {
                            var response = MyBox.ShowDialog(
                                $"Tried to process remaining {remaining} unprocessed "
                                    + $"items 3 times without success. Continue trying?",
                                "Error",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Hand
                            );

                            if (response == DialogResult.No)
                            {
                                logger.Warn(
                                    $"Tried to process remaining {remaining} unprocessed items 3 times without success. Exiting loop."
                                );
                                break;
                            }
                        }
                        else
                        {
                            logger.Debug(
                                $"Error processing item {success + errors} of {unprocessedCount} in the unprocessed Queue"
                            );
                            // Issue #207 (AC10): non-blocking, pump-independent replacement for the
                            // banned Task.Delay. NonBlockingDelay completes on a one-shot
                            // System.Threading.Timer callback, so the STA keeps pumping during the
                            // retry pause and the wait completes even without a running Dispatcher
                            // (required so the gated MSTest host does not hang).
                            await NonBlockingDelay.WaitAsync(TimeSpan.FromMilliseconds(100));
                        }
                    }

                    LogStartupTiming(
                        "ProcessNewInboxItemsAsync startup inbox batch complete | batch processing",
                        true,
                        $"batch={batchNumber}; processed={processedThisBatch}; remaining={unprocessedQueue.Count()}; success={success}; errors={errors}; elapsedMs={batchStopwatch.ElapsedMilliseconds}"
                    );

                    if (unprocessedQueue.Count > 0)
                    {
                        LogStartupTiming(
                            "ProcessNewInboxItemsAsync deferred continuation checkpoint | batch processing",
                            true,
                            $"batch={batchNumber}; remaining={unprocessedQueue.Count()}"
                        );
                        await Task.Yield();
                    }
                }
                logger.Debug(
                    $"Successfully processed {success} of {unprocessedCount} items in the "
                        + $"unprocessed Queue with {errors} errors"
                );

                logger.Debug("Finished processing new inbox items");
            }

            LogStartupTiming(
                "ProcessNewInboxItemsAsync complete | startup-active status",
                false,
                $"elapsedMs={processingStopwatch.ElapsedMilliseconds}"
            );
        }

        #endregion
    }
}
