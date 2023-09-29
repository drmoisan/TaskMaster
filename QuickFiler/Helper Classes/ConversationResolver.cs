using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;

namespace QuickFiler.Helper_Classes
{
    public class ConversationResolver : INotifyPropertyChanged, IConversationResolver
    {
        private ConversationResolver(IApplicationGlobals appGlobals,
                                     MailItem mailItem)
        { }

        public ConversationResolver(IApplicationGlobals appGlobals,
                                    MailItem mailItem,
                                    CancellationToken token,
                                    System.Action<List<MailItemInfo>> updateUI = null)
        {
            _globals = appGlobals;
            _token = token;
            _mailItem = mailItem;
            _updateUI = updateUI;
            PropertyChanged += Handler_PropertyChanged;
        }

        public async static Task<ConversationResolver> LoadAsync(IApplicationGlobals appGlobals,
                                      MailItem mailItem,
                                      CancellationToken token,
                                      bool backgroundLoad,
                                      System.Action<List<MailItemInfo>> updateUI = null)
        {
            var resolver = new ConversationResolver(appGlobals, mailItem);
            resolver.Token = token;

            if (updateUI is not null)
                resolver.UpdateUI = updateUI;

            if (backgroundLoad)
            {
                await resolver.LoadDfAsync(token, backgroundLoad);
                await resolver.LoadConversationInfoAsync(token, backgroundLoad);
                await resolver.LoadConversationItemsAsync(token, backgroundLoad);
                resolver.PropertyChanged += resolver.Handler_PropertyChanged;
            }
            else
            {
                resolver.PropertyChanged += resolver.Handler_PropertyChanged;
                _ = resolver.LoadConversationItemsAsync(token, backgroundLoad);
            }

            return resolver;
        }

        private CancellationToken _token;
        internal CancellationToken Token { get => _token; set => _token = value; }

        private IApplicationGlobals _globals;
        private MailItem _mailItem;

        private System.Action<List<MailItemInfo>> _updateUI;
        public System.Action<List<MailItemInfo>> UpdateUI { get => _updateUI; set => _updateUI = value; }

        #region ConversationInfo

        private (List<MailItemInfo> SameFolder, List<MailItemInfo> Expanded) _convInfoFields;
        public (List<MailItemInfo> SameFolder, List<MailItemInfo> Expanded) ConversationInfo
        {
            get => Initializer.GetOrLoad(ref _convInfoFields, LoadConversationInfo, (x) => NotifyPropertyChanged(nameof(ConversationInfo)), false, _mailItem);
            set { _convInfoFields = value; NotifyPropertyChanged(); }
        }
        internal (List<MailItemInfo> SameFolder, List<MailItemInfo> Expanded) LoadConversationInfo()
        {
            var olNs = _globals.Ol.App.GetNamespace("MAPI");
            var convInfoExpanded = Enumerable.Range(0, Count.Expanded)
                                             .Select(indexRow => new MailItemInfo(Df.Expanded, indexRow))
                                             .OrderByDescending(itemInfo => itemInfo.ConversationIndex)
                                             .ToList();

            var convInfoSameFolder = convInfoExpanded.Where(
                itemInfo => itemInfo.Folder == ((Folder)_mailItem.Parent).Name).ToList();

            return (convInfoExpanded, convInfoSameFolder);
        }
        public async Task<(List<MailItemInfo> SameFolder, List<MailItemInfo> Expanded)> LoadConversationInfoAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();

            TaskScheduler priority = backgroundLoad ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;
            TaskCreationOptions options = backgroundLoad ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;

            var olNs = _globals.Ol.App.GetNamespace("MAPI");

            var tasksConvInfoExp = Enumerable.Range(0, Count.Expanded)
                                             .Select(indexRow => MailItemInfo
                                             .FromDfAsync(Df.Expanded, indexRow, olNs, token, backgroundLoad));

            var convInfoExpanded = (await Task.WhenAll(tasksConvInfoExp))
                                   .OrderByDescending(itemInfo => itemInfo.ConversationIndex)
                                   .ToList();

            if (_updateUI is not null)
            {
                token.ThrowIfCancellationRequested();
                await UIThreadExtensions.UiDispatcher.InvokeAsync(() => _updateUI(ConversationInfo.Expanded));
            }

            var convInfoSameFolder = convInfoExpanded.Where(
                itemInfo => itemInfo.Folder == ((Folder)_mailItem.Parent).Name).ToList();

            ConversationInfo = (convInfoSameFolder, convInfoExpanded);
            return (convInfoSameFolder, convInfoExpanded);
        }

        #endregion

        #region ConversationItems

        private (IList<MailItem> SameFolder, IList<MailItem> Expanded) _conversationItems;
        public (IList<MailItem> SameFolder, IList<MailItem> Expanded) ConversationItems
        {
            get => Initializer.GetOrLoad(ref _conversationItems, LoadConversationItems, (x) => NotifyPropertyChanged(nameof(ConversationItems)), false, _mailItem);
            set { _conversationItems = value; NotifyPropertyChanged(); }
        }
        internal (IList<MailItem> SameFolder, IList<MailItem> Expanded) LoadConversationItems()
        {
            var sameFolder = ConversationInfo.SameFolder.Select(itemInfo => itemInfo.Item).ToList();
            var expanded = ConversationInfo.Expanded.Select(itemInfo => itemInfo.Item).ToList();
            return (sameFolder, expanded);
        }
        public async Task LoadConversationItemsAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();

            TaskScheduler priority = backgroundLoad ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;
            TaskCreationOptions options = backgroundLoad ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;

            await Task.Factory.StartNew(() => LoadConversationItems(),
                                        token,
                                        options,
                                        priority);
        }

        #endregion

        #region Df

        private (DataFrame SameFolder, DataFrame Expanded) _df;
        public (DataFrame SameFolder, DataFrame Expanded) Df
        {
            get => Initializer.GetOrLoad(ref _df, LoadDf, (x) => NotifyPropertyChanged(nameof(Df)), false, _mailItem);
        }
        internal (DataFrame SameFolder, DataFrame Expanded) LoadDf()
        {
            var dfExpanded = _mailItem.GetConversation().GetConversationDf().FilterConversation(((Folder)_mailItem.Parent).Name, false, true);
            var dfSameFolder = dfExpanded.FilterConversation(((Folder)_mailItem.Parent).Name, true, true);
            return (dfSameFolder, dfExpanded);
        }
        public async Task LoadDfAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();

            TaskScheduler priority = backgroundLoad ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;
            TaskCreationOptions options = backgroundLoad ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;
            await Task.Factory.StartNew(
                () => LoadDf(),
                token,
                options,
                priority);
        }

        private (int SameFolder, int Expanded) _count;
        public (int SameFolder, int Expanded) Count => Initializer.GetOrLoad(ref _count, () => (Df.SameFolder.Rows.Count(), Df.Expanded.Rows.Count()));

        #endregion

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                NotifyPropertyChanged();
            }
        }

        #region INotifyPropertyChanged implementation

        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Df))
            {
                _ = LoadConversationItemsAsync(_token, true).ConfigureAwait(false);
            }
        }

        [Obsolete("Use LoadConversationInfoAsync instead",true)]
        internal async Task GetConversationInfoAsync()
        {
            //var olNs = _globals.Ol.App.GetNamespace("MAPI");
            //DataFrame df = DfExpanded;

            //// Initialize the ConversationInfo list from the Dataframe with Synchronous code
            //ConvInfoExpanded = Enumerable.Range(0, df.Rows.Count())
            //                           .Select(indexRow => new MailItemInfo(df, indexRow))
            //                           .OrderByDescending(itemInfo => itemInfo.ConversationIndex)
            //                           .ToList();

            //ConvInfoSameFolder = ConversationInfoExpanded.Where(
            //    itemInfo => itemInfo.Folder == ((Folder)_mailItem.Parent).Name).ToList();

            //if (_updateUI is not null)
            //    await UIThreadExtensions.UiDispatcher.InvokeAsync(() => _updateUI(ConversationInfoExpanded));

            //// Run the async code in parallel to resolve the mailitem and load extended properties
            //ConversationItemsSameFolder = Task.WhenAll(ConversationInfoExpanded.Select(async itemInfo =>
            //                        {
            //                            await itemInfo.LoadAsync(olNs, _isDarkMode)
            //                                          .ConfigureAwait(false);
            //                            return itemInfo.Item;
            //                        }))
            //                        .Result
            //                        .ToList();

            // Next line is to facilitate deprecation of old code
            await Task.CompletedTask;
        }

        [Obsolete("Use LoadConversationInfoAsync instead", true)]
        internal async Task GetConversationInfoAsync(DataFrame df, CancellationToken token)
        {
            //var olNs = _globals.Ol.App.GetNamespace("MAPI");

            //var tasksConvInfoExp = Enumerable.Range(0, df.Rows.Count()).Select(indexRow => MailItemInfo.FromDfAsync(df, indexRow, olNs, token));

            //ConvInfoExpanded = (await Task.WhenAll(tasksConvInfoExp)).OrderByDescending(itemInfo => itemInfo.ConversationIndex).ToList();

            //if (_updateUI is not null)
            //    await UIThreadExtensions.UiDispatcher.InvokeAsync(() => _updateUI(ConversationInfoExpanded));

            //var tasks = new List<Task>
            //{
            //    Task.Run(()=>ConvInfoSameFolder = ConversationInfoExpanded.Where(
            //        itemInfo => itemInfo.Folder == ((Folder)_mailItem.Parent).Name).ToList(), token),
            //    Task.Run(()=>ConversationItemsSameFolder = ConversationInfoExpanded.Select(itemInfo => itemInfo.Item).ToList(), token),
            //};

            // Place to facilitate deprecation of code
            await Task.CompletedTask;
        }

        #endregion

        //async internal Task<List<MailItem>> ResolveItemsAsync(DataFrame df, CancellationToken token)
        //{
        //    token.ThrowIfCancellationRequested();

        //    return await Task.Factory.StartNew(
        //        () =>
        //        {
        //            var parentFolder = ((Folder)_mailItem.Parent);
        //            var storeID = parentFolder.StoreID;
        //            return ConvHelper.GetMailItemList(df, storeID, _globals.Ol.App, true).Cast<MailItem>().ToList();
        //        },
        //        token,
        //        TaskCreationOptions.None, PriorityScheduler.BelowNormal);

        //    //return await Task.Run(() =>
        //    //{
        //    //    var parentFolder = ((Folder)_mailItem.Parent);
        //    //    var storeID = parentFolder.StoreID;
        //    //    return ConvHelper.GetMailItemList(df, storeID, _globals.Ol.App, true).Cast<MailItem>().ToList();
        //    //}, token);
        //}

        //async internal Task<List<MailItem>> ResolveItemsAsync(CancellationToken token)
        //{
        //    token.ThrowIfCancellationRequested();

        //    await LoadDfAsync(token, true);

        //    return await ResolveItemsAsync(Df.SameFolder, token);
        //}
    }
}
