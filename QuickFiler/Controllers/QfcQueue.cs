using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace QuickFiler.Controllers
{
    public class QfcQueue : INotifyCollectionChanged, INotifyPropertyChanged
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Private Members

        public QfcQueue(CancellationToken token,
                        QfcHomeController homeController,
                        IApplicationGlobals appGlobals) 
        { 
            _token = token; 
            _homeController = homeController;
            _globals = appGlobals;
        }

        private CancellationToken _token;
        private QfcHomeController _homeController;
        private IQfcCollectionController _qfcCollectionController;
        private IApplicationGlobals _globals;

        private int _jobsRunning = 0;
        private BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> _queue = 
            new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>(
                new ConcurrentQueue<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>());

        #endregion Constructors and Private Members

        #region Queue Functions

        public async Task CompleteAddingAsync(CancellationToken token, int timeout) 
        { 
            CancellationTokenSource functionTimeoutSource = new CancellationTokenSource(timeout);
            CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, functionTimeoutSource.Token);

            try
            {
                while (_jobsRunning > 0)
                {
                    logger.Debug($"{nameof(CompleteAddingAsync)} waiting for {_jobsRunning} jobs to complete");
                    await Task.Delay(100, linkedTokenSource.Token);
                }
                _queue.CompleteAdding();
            }
            catch (OperationCanceledException e)
            {
                if (!token.IsCancellationRequested) { logger.Debug($"{nameof(CompleteAddingAsync)} timed out after {timeout} milliseconds"); }
                throw e;
            }
            
        }
        
        public (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) Dequeue()
        {
            (TableLayoutPanel tlp, List<QfcItemGroup> itemGroups) = _queue.Take();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _queue));
            return (tlp, itemGroups);
        }

        public async Task<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> TryDequeueAsync(CancellationToken token, int timeout)
        {
            TraceUtility.LogMethodCall(token, timeout);

            token.ThrowIfCancellationRequested();

            if (_queue.Count == 0 && _jobsRunning == 0) 
            {
                logger.Debug($"{nameof(TryDequeueAsync)} attempted with no jobs running and nothing in the queue. Returning default.");
                return default; 
            }

            var functionTimeoutSource = new CancellationTokenSource(timeout);
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, functionTimeoutSource.Token);
            
            int queueTimeout = Math.Min(timeout, 100);
            int pollInterval = 100;

            (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) result = default;
            try 
            {
                while (!_queue.IsCompleted && !token.IsCancellationRequested && result == default && _queue.Count + _jobsRunning > 0)
                {
                    if (!_queue.TryTake(out result, queueTimeout, token))
                    {
                        logger.Debug($"{nameof(TryDequeueAsync)} attempted to take before {_jobsRunning} queuing job(s) are complete. Waiting {pollInterval} milliseconds");
                        await Task.Delay(pollInterval, token);
                    }
                }
                if (result != default)
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _queue));
                
            }
            catch (OperationCanceledException)
            {
                if (!token.IsCancellationRequested)
                {
                    logger.Debug($"{nameof(TryDequeueAsync)} timed out after {timeout} milliseconds");
                }
            }
            
            return result;

        }                

        public async Task EnqueueAsync(IList<MailItem> items,
                                       IQfcCollectionController qfcCollectionController)
        {
            TraceUtility.LogMethodCall(items, qfcCollectionController);

            if (items is null) { throw new ArgumentNullException(nameof(items)); }
            if (items.Count == 0) { throw new ArgumentException("items is empty"); }
            _qfcCollectionController = qfcCollectionController;

            Interlocked.Increment(ref _jobsRunning);
            logger.Debug($"{nameof(EnqueueAsync)} called and jobsRunning increased to {_jobsRunning}");
            
            var tlp = await UiIdleCallAsync(() => _tlpTemplate.Clone(name: "BackgroundTableLayout"));

            try
            {
                var itemGroups = await LoadControllersViewersAsync(items, _globals, 
                    _homeController, qfcCollectionController, tlp, 0);
                _queue.Add((tlp, itemGroups));
            }
            catch (System.Exception e)
            {
                logger.Error($"{nameof(EnqueueAsync)} failed to load controllers and viewers. \n {e.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref _jobsRunning);
                logger.Debug($"{nameof(EnqueueAsync)} completed and jobsRunning decreased to {_jobsRunning}");
            
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _queue));
            }
        }

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

        #region Tlp Manipulation

        private TableLayoutPanel _tlpTemplate;
        public TableLayoutPanel TlpTemplate 
        { 
            get => _tlpTemplate;
            set
            {
                _tlpTemplate = value.Clone();
                _tlpTemplate.Name = "TemplateTableLayout";
            }
        }

        private TlpCellStates _tlpStates;
        public TlpCellStates TlpStates { get => _tlpStates; set => _tlpStates = value; }

        internal async Task<QfcItemGroup> AddAsync(TableLayoutPanel tlp, MailItem mailItem, int indexNumber)
        {
            TraceUtility.LogMethodCall(tlp, mailItem, indexNumber);

            var grp = new QfcItemGroup(mailItem);
            var viewer = ItemViewerQueue.Dequeue(_token);
            grp.ItemViewer = viewer;
            await UiIdleCallAsync(() => AddViewerToTlp(tlp, viewer, indexNumber));
            return grp;
        }

        internal void AddViewerToTlp(TableLayoutPanel tlp, ItemViewer viewer, int indexNumber)
        {
            TraceUtility.LogMethodCall(tlp, viewer, indexNumber);

            viewer.Parent = tlp;
            tlp.SetCellPosition(viewer, new TableLayoutPanelCellPosition(0, indexNumber));
            tlp.SetColumnSpan(viewer, 2);
            viewer.AutoSize = true;
            viewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            viewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            viewer.Dock = DockStyle.Fill;
        }

        internal void AdjustTlp(TableLayoutPanel tlp, int newRowCount, RowStyle rowStyleTemplate)
        {
            var oldRowCount = tlp.RowCount - 1;
            if (oldRowCount != newRowCount)
            {
                var diff = newRowCount - Math.Max(0, oldRowCount);
                if (diff > 0)
                {
                    tlp.InsertSpecificRow(oldRowCount, rowStyleTemplate, diff);
                    tlp.MinimumSize = new System.Drawing.Size(
                        tlp.MinimumSize.Width,
                        tlp.MinimumSize.Height +
                        (int)Math.Round(rowStyleTemplate.Height * diff, 0));
                }
                else
                {
                    tlp.RemoveSpecificRow(newRowCount, diff);
                    tlp.MinimumSize = new System.Drawing.Size(
                        tlp.MinimumSize.Width,
                        tlp.MinimumSize.Height -
                        (int)Math.Round(rowStyleTemplate.Height * diff, 0));
                }
            }
        }

        private ValueTask<List<QfcItemGroup>> LoadControllersViewersAsync(
            IList<MailItem> items, 
            IApplicationGlobals appGlobals, 
            IFilerHomeController homeController, 
            IQfcCollectionController qfcCollectionController, 
            TableLayoutPanel tlp,
            int start)
        {
            TraceUtility.LogMethodCall(items, appGlobals, homeController, qfcCollectionController, tlp, start);

            var digits = start + items.Count >= 10 ? 2:1;
            var itemTasks = Enumerable.Range(start, items.Count)
                    .ToAsyncEnumerable()
                    .SelectAwait(async i => (i: i, grp: await AddAsync(tlp, items[i-start], i)))
                    //.ToListAsync();
                    .SelectAwait(async x =>
                    {
                        x.grp.ItemController = new QfcItemController(
                            AppGlobals: appGlobals,
                            homeController: homeController,
                            parent: qfcCollectionController,
                            itemViewer: x.grp.ItemViewer,
                            viewerPosition: x.i + 1,
                            itemNumberDigits: digits,
                            x.grp.MailItem,
                            TlpStates);
                        await x.grp.ItemController.InitializeAsync();
                        return x.grp;
                    })
                    .ToListAsync();
            return itemTasks;
        }

        public async Task ChangeIterationSize(
            (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) entry,
            int newRowCount, 
            RowStyle rowStyleTemplate)
        {
            // Wait for all jobs to finish to prevent conflicts
            await JobsToFinish(100, _token);

            // Adjust template for future jobs
            AdjustTlp(TlpTemplate, newRowCount, rowStyleTemplate);
            
            // Cache old queue in private collection, 
            var oldQueue = _queue;
            
            // Externally visible queue is now empty, but job is marked as running
            _queue = new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>(
                new ConcurrentQueue<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>()); 
            
            logger.Debug($"{nameof(ChangeIterationSize)} called and jobsRunning increased to {_jobsRunning}");
            Interlocked.Increment(ref _jobsRunning);

            var queue = new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>(
                new ConcurrentQueue<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>());


            while (oldQueue.Count > 0)
            {
                var nextEntry = oldQueue.Take();
                GrowEntry(ref entry, ref nextEntry, newRowCount, rowStyleTemplate);
                if (entry.ItemGroups.Count == newRowCount)
                {
                    RenumberGroups(entry.ItemGroups);
                    queue.Add(entry);
                    if (nextEntry.ItemGroups.Count > 0) { entry = nextEntry; }
                    else 
                    { 
                        if (oldQueue.Count > 0) { entry = oldQueue.Take(); } 
                        else { entry = default; }
                    }
                }
            }
                
            if (entry != default) 
            {
                var items = await _homeController.DataModel.DequeueNextItemGroupAsync(newRowCount - entry.ItemGroups.Count, 1000);
                if (items.Count > 0)
                {
                    AdjustTlp(entry.Tlp, newRowCount, rowStyleTemplate);
                    var extraGroups = await LoadControllersViewersAsync(items, _globals,
                        _homeController, _qfcCollectionController, entry.Tlp, entry.ItemGroups.Count);
                    extraGroups.ForEach(group => entry.ItemGroups.Add(group));
                }
                RenumberGroups(entry.ItemGroups);
                queue.Add(entry);
            }
            
            // Discard top element in queue which will always be a duplicate
            _ = queue.Take();

            // Set the externally visible queue to the new queue and mark the job as complete
            _queue = queue;
            Interlocked.Decrement(ref _jobsRunning);
            logger.Debug($"{nameof(ChangeIterationSize)} completed and jobsRunning decreased to {_jobsRunning}");
        }

        public void RenumberGroups(List<QfcItemGroup> itemGroups)
        {
            var digits = itemGroups.Count >= 10 ? 2 : 1;
            for (int i = 0; i < itemGroups.Count; i++)
            {
                itemGroups[i].ItemController.ItemNumberDigits = digits;
                itemGroups[i].ItemController.ItemNumber = i + 1;
            }
        }

        public void GrowEntry(
            ref (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) target,
            ref (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) source,
            int newRowCount, 
            RowStyle rowStyleTemplate)
        {
            var currentCount = target.ItemGroups.Count;
            var grow = Math.Min(newRowCount - currentCount, source.ItemGroups.Count);
            
            AdjustTlp(target.Tlp, newRowCount, rowStyleTemplate);
            
            if (grow == 0) { return; }

            for (int i = 0; i < grow; i++) 
            {
                var itemViewer = source.Tlp.Controls[i];
                var position = source.Tlp.GetCellPosition(itemViewer);
                itemViewer.Parent = target.Tlp;
                target.Tlp.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(position.Column, currentCount + i));
                var group = source.ItemGroups[0];
                target.ItemGroups.Add(group);
                source.ItemGroups.RemoveAt(0);
                group.ItemController.ItemNumber = currentCount + i + 1;
            }

            source.Tlp.RemoveSpecificRow(0, grow);

            source.Tlp.MinimumSize = new System.Drawing.Size(
                source.Tlp.MinimumSize.Width,
                source.Tlp.MinimumSize.Height -
                (int)Math.Round(rowStyleTemplate.Height * grow, 0));
        }

        #endregion Tlp Manipulation

        #region INotify

        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Helper Methods

        internal async Task UiIdleCallAsync(System.Action action)
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(action, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        internal async Task<T> UiIdleCallAsync<T>(Func<T> func)
        {
            return await UIThreadExtensions.UiDispatcher.InvokeAsync(func, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }



        #endregion Helper Methods

    }

}

