using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
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

        private CancellationToken _token;

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

        private Queue<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> _queue = new();
        //public Queue<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> Queue { get => _queue; }

        #region Queue Functions

        public (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) Dequeue()
        {
            (TableLayoutPanel tlp, List<QfcItemGroup> itemGroups) = _queue.Dequeue();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _queue));
            return (tlp, itemGroups);
        }

        
        public async Task EnqueueAsync(IList<MailItem> items,
                                       IApplicationGlobals appGlobals,
                                       IFilerHomeController _homeController,
                                       IQfcCollectionController _qfcCollectionController)
        {
            if (items is null) { throw new ArgumentNullException(nameof(items)); }
            if (items.Count == 0) { throw new ArgumentException("items is empty"); }

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
            _queue.Enqueue((tlp, itemGroups));
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

