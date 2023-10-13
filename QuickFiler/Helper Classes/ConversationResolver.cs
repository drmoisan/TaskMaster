using log4net.Repository.Hierarchy;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json.Linq;
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
    public struct Pair<T>
    {
        public Pair(T sameFolder, T expanded)
        {
            SameFolder = sameFolder;
            Expanded = expanded;
        }

        public T SameFolder { get; set; }
        public T Expanded { get; set; }
    }

    public class ConversationResolver : INotifyPropertyChanged, IConversationResolver
    {
        public ConversationResolver(IApplicationGlobals appGlobals, MailItem mailItem) 
        { 
            _globals = appGlobals;
            _mailItem = mailItem;
        }

        public ConversationResolver(IApplicationGlobals appGlobals,
                                    MailItem mailItem,
                                    CancellationTokenSource tokenSource,
                                    CancellationToken token,
                                    System.Action<List<MailItemInfo>> updateUI = null)
        {
            _globals = appGlobals;
            _tokenSource = tokenSource;
            _token = token;
            _mailItem = mailItem;
            _updateUI = updateUI;
            PropertyChanged += Handler_PropertyChanged;
        }

        public async static Task<ConversationResolver> LoadAsync(IApplicationGlobals appGlobals,
                                      MailItem mailItem,
                                      CancellationTokenSource tokenSource,
                                      CancellationToken token,
                                      bool loadAll,
                                      System.Action<List<MailItemInfo>> updateUI = null)
        {
            var resolver = new ConversationResolver(appGlobals, mailItem);
            resolver.Token = token;
            resolver.TokenSource = tokenSource;

            if (updateUI is not null)
                resolver.UpdateUI = updateUI;

            if (loadAll)
            {
                await resolver.LoadDfAsync(token, loadAll);
                await resolver.LoadConversationInfoAsync(token, loadAll);
                await resolver.LoadConversationItemsAsync(token, loadAll);
                resolver.PropertyChanged += resolver.Handler_PropertyChanged;
            }
            else
            {
                resolver.PropertyChanged += resolver.Handler_PropertyChanged;
                // Not sure why this was here. Going to comment it out for now
                //_ = resolver.LoadConversationItemsAsync(token, backgroundLoad);
                await resolver.LoadDfAsync(token, loadAll);
            }

            return resolver;
        }

        public async Task BackgroundInitInfoItemsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await LoadConversationInfoAsync(token, true);
            await LoadConversationItemsAsync(token, true);
        }

        private CancellationToken _token;
        internal CancellationToken Token { get => _token; set => _token = value; }

        private CancellationTokenSource _tokenSource;
        internal CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        private IApplicationGlobals _globals;
        private MailItem _mailItem;

        private System.Action<List<MailItemInfo>> _updateUI;
        public System.Action<List<MailItemInfo>> UpdateUI { get => _updateUI; set => _updateUI = value; }

        #region ConversationInfo

        private Pair<List<MailItemInfo>> _convInfoFields;
        public Pair<List<MailItemInfo>> ConversationInfo
        {
            get => Initializer.GetOrLoad(ref _convInfoFields, LoadConversationInfo, (x) => NotifyPropertyChanged(nameof(ConversationInfo)), false, _mailItem);
            set { _convInfoFields = value; NotifyPropertyChanged(); }
        }
        internal Pair<List<MailItemInfo>> LoadConversationInfo()
        {
            if (Count.Expanded <= 0) 
            { 
                throw new InvalidOperationException(
                    $"{ConversationInfo} cannot be loaded if {Df} cannot be resolved");
            }
            
            var df = Df.Expanded;
            var olNs = _globals.Ol.App.GetNamespace("MAPI");
            var convInfoExpanded = Enumerable.Range(0, Count.Expanded)
                                                .Select(indexRow => MailItemInfo.FromDf(df, indexRow, olNs, Token))
                                                .OrderByDescending(itemInfo => itemInfo.ConversationIndex)
                                                .ToList();

            var convInfoSameFolder = convInfoExpanded.Where(
                itemInfo => itemInfo.Folder == ((Folder)_mailItem.Parent).Name).ToList();

            return new Pair<List<MailItemInfo>>(convInfoExpanded, convInfoSameFolder);
            
        }
        public async Task<Pair<List<MailItemInfo>>> LoadConversationInfoAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();

            //TaskScheduler priority = backgroundLoad ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;
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

            var pair = new Pair<List<MailItemInfo>>(convInfoSameFolder, convInfoExpanded);
            ConversationInfo = pair;
            return pair;
        }

        #endregion

        #region ConversationItems

        private Pair<IList<MailItem>> _conversationItems;
        public Pair<IList<MailItem>> ConversationItems
        {
            get => Initializer.GetOrLoad(ref _conversationItems, LoadConversationItems, (x) => NotifyPropertyChanged(nameof(ConversationItems)), false, _mailItem);
            set { _conversationItems = value; NotifyPropertyChanged(); }
        }
        internal Pair<IList<MailItem>> LoadConversationItems()
        {
            var sameFolder = ConversationInfo.SameFolder.Select(itemInfo => itemInfo.Item).ToList();
            var expanded = ConversationInfo.Expanded.Select(itemInfo => itemInfo.Item).ToList();
            return new Pair<IList<MailItem>>(sameFolder, expanded);
        }
        public async Task LoadConversationItemsAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();

            //TaskScheduler priority = backgroundLoad ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;
            TaskCreationOptions options = backgroundLoad ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;

            await Task.Factory.StartNew(() => ConversationItems = LoadConversationItems(),
                                        token);//,
                                        //options,
                                        //priority);
        }

        #endregion

        #region Df

        private Pair<DataFrame> _df;
        public Pair<DataFrame> Df
        {
            get => Initializer.GetOrLoad(ref _df, LoadDf, DfNotifyIfNotNull, false, _mailItem);
            set => _df = value;
        }

        internal Pair<DataFrame> LoadDf() 
        { 
        //    var priority = PriorityScheduler.AboveNormal;
        //    var options = TaskCreationOptions.None;
        //    return LoadDf(priority, options);
        //}
        //internal Pair<DataFrame> LoadDf(TaskScheduler priority, TaskCreationOptions options)
        //{
            // Attempt to call async synchronously caused dealock
            //var dfRaw = _mailItem.GetConversationDfAsync(TokenSource, Token, 1000, options, priority).GetAwaiter().GetResult();
                        
            var dfExpanded = _mailItem.GetConversation()
                                      .GetConversationDf()
                                      .FilterConversation(
                                            ((Folder)_mailItem.Parent).Name, 
                                            false, 
                                            true);

            //Console.WriteLine(((Folder)_mailItem.Parent).Name);
            var dfSameFolder = dfExpanded.FilterConversation(((Folder)_mailItem.Parent).Name, true, true);
            
            return new Pair<DataFrame>(dfSameFolder, dfExpanded); 
            
        }
        internal void DfNotifyIfNotNull(Pair<DataFrame> df)
        {
            if (df.SameFolder is not null && df.Expanded is not null) { NotifyPropertyChanged(nameof(Df)); }     
        }
        public async Task LoadDfAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();
            
            TaskCreationOptions options = backgroundLoad ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;
            var dfRaw = await _mailItem.GetConversationDfAsync(Token).ConfigureAwait(false);
            var dfExpanded = dfRaw.FilterConversation(((Folder)_mailItem.Parent).Name, false, true);
            var dfSameFolder = dfExpanded.FilterConversation(((Folder)_mailItem.Parent).Name, true, true);
            Df = new Pair<DataFrame>(dfSameFolder, dfExpanded);
            
            
        }

        private Pair<int> _count;
        public Pair<int> Count => Initializer.GetOrLoad(ref _count, LoadCount);
        internal Pair<int> LoadCount()
        {
            var count = new Pair<int>(-1, -1);
            var df = Df;
            if (df.SameFolder is not null) { count.SameFolder = df.SameFolder.Rows.Count(); }
            if (df.Expanded is not null) { count.Expanded = df.Expanded.Rows.Count(); }
            return count;
        }

        #endregion

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
                _ = BackgroundInitInfoItemsAsync(_token).ConfigureAwait(false);
            }
        }

        #endregion

        #region Obsolete

        [Obsolete("Use LoadConversationInfoAsync instead", true)]
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
    }
}
