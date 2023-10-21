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

namespace QuickFiler.Controllers
{
    public class QfcQueue : INotifyCollectionChanged, INotifyPropertyChanged
    {
        public QfcQueue(CancellationToken token) { _token = token; }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private CancellationToken _token;
        private int jobsRunning = 0;

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

        private BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> _queue = 
            new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>(
                new ConcurrentQueue<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>(),4);
        //public Queue<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> Queue { get => _queue; }

        #region Queue Functions

        public async Task CompleteAddingAsync(CancellationToken token, int timeout) 
        { 
            CancellationTokenSource functionTimeoutSource = new CancellationTokenSource(timeout);
            CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, functionTimeoutSource.Token);

            try
            {
                while (jobsRunning > 0)
                {
                    logger.Debug($"{nameof(CompleteAddingAsync)} waiting for {jobsRunning} jobs to complete");
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
            token.ThrowIfCancellationRequested();

            if (_queue.Count == 0 && jobsRunning == 0) 
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
                while (!_queue.IsCompleted && !token.IsCancellationRequested && result == default)
                {
                    if (!_queue.TryTake(out result, queueTimeout, token))
                    {
                        logger.Debug($"{nameof(TryDequeueAsync)} attempted to take before {jobsRunning} queuing job(s) are complete. Waiting {pollInterval} milliseconds");
                        await Task.Delay(pollInterval, token);
                    }
                }

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
                                       IApplicationGlobals appGlobals,
                                       IFilerHomeController _homeController,
                                       IQfcCollectionController _qfcCollectionController)
        {
            if (items is null) { throw new ArgumentNullException(nameof(items)); }
            if (items.Count == 0) { throw new ArgumentException("items is empty"); }

            Interlocked.Increment(ref jobsRunning);

            var tlp = await UiIdleCallAsync(() => _tlpTemplate.Clone(name: "BackgroundTableLayout"));
            var itemTasks = Enumerable.Range(0, items.Count)
                                      .ToAsyncEnumerable()
                                      .SelectAwait(async i => (i:i, grp: await AddAsync(tlp, items[i], i)))
                                      //.ToListAsync();
                                      .SelectAwait(async x => 
                                      { 
                                          x.grp.ItemController = new QfcItemController(AppGlobals: appGlobals,
                                                                                     homeController: _homeController,
                                                                                     parent: _qfcCollectionController,
                                                                                     itemViewer: x.grp.ItemViewer,
                                                                                     viewerPosition: x.i + 1,
                                                                                     x.grp.MailItem);
                                          await x.grp.ItemController.InitializeAsync();
                                          return x.grp;
                                      }).ToListAsync();
            
            var itemGroups = await itemTasks;
            _queue.Add((tlp, itemGroups));
            Interlocked.Decrement(ref jobsRunning);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _queue));
        }

        internal async Task<QfcItemGroup> AddAsync(TableLayoutPanel tlp, MailItem mailItem, int indexNumber)
        {
            var grp = new QfcItemGroup(mailItem);
            var viewer = ItemViewerQueue.Dequeue(_token);
            grp.ItemViewer = viewer;
            await UiIdleCallAsync(() => AddViewerToTlp(tlp, viewer, indexNumber));
            return grp;
        }

        internal void AddViewerToTlp(TableLayoutPanel tlp, ItemViewer viewer, int indexNumber)
        {
            viewer.Parent = tlp;
            tlp.SetCellPosition(viewer, new TableLayoutPanelCellPosition(0, indexNumber));
            tlp.SetColumnSpan(viewer, 2);
            viewer.AutoSize = true;
            viewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            viewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            viewer.Dock = DockStyle.Fill;
        }

        public int Count => _queue.Count;

        public int JobsRunning => jobsRunning;

        #endregion

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

        internal async Task UiIdleCallAsync(System.Action action)
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(action, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        internal async Task<T> UiIdleCallAsync<T>(Func<T> func)
        {
            return await UIThreadExtensions.UiDispatcher.InvokeAsync(func, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }

}

