using log4net.Repository.Hierarchy;
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
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Initializers

        public ConversationResolver(IApplicationGlobals appGlobals, MailItem mailItem) 
        { 
            _globals = appGlobals;
            _mailItem = mailItem;
        }

        public ConversationResolver(IApplicationGlobals appGlobals,
                                    MailItem mailItem,
                                    CancellationTokenSource tokenSource,
                                    CancellationToken token,
                                    System.Action<List<MailItemHelper>> updateUI = null)
        {
            _globals = appGlobals;
            _tokenSource = tokenSource;
            _token = token;
            _mailItem = mailItem;
            MailInfo = new MailItemHelper(mailItem).LoadPriority(appGlobals, token);
            _updateUI = updateUI;
            PropertyChanged += Handler_PropertyChanged;
        }

        public async static Task<ConversationResolver> LoadAsync(IApplicationGlobals appGlobals,
                                      MailItem mailItem,
                                      CancellationTokenSource tokenSource,
                                      CancellationToken token,
                                      bool loadAll,
                                      System.Action<List<MailItemHelper>> updateUI = null)
        {
            var resolver = new ConversationResolver(appGlobals, mailItem);
            resolver.Token = token;
            resolver.TokenSource = tokenSource;

            if (updateUI is not null)
                resolver.UpdateUI = updateUI;

            resolver.MailInfo = await MailItemHelper.FromMailItemAsync(mailItem, appGlobals, token, loadAll);

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

        #endregion Constructors and Initializers

        #region Properties

        private CancellationToken _token;
        internal CancellationToken Token { get => _token; set => _token = value; }

        private CancellationTokenSource _tokenSource;
        internal CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        protected IApplicationGlobals _globals;
        
        protected MailItem _mailItem;
        public MailItem Mail { get => _mailItem; protected set => _mailItem = value; }

        private bool _fullyLoaded = false;
        public bool FullyLoaded { get => _fullyLoaded; protected set => _fullyLoaded = value; }

        protected System.Action<List<MailItemHelper>> _updateUI;
        public System.Action<List<MailItemHelper>> UpdateUI 
        { 
            get => _updateUI; 
            set => Initializer.SetAndSave(ref _updateUI, value, (x) => NotifyPropertyChanged(nameof(UpdateUI))); 
        }

        protected MailItemHelper _mailInfo;
        public MailItemHelper MailInfo { get => _mailInfo; set => _mailInfo = value; }

        #region ConversationInfo

        private Pair<List<MailItemHelper>> _convInfoFields;
        public Pair<List<MailItemHelper>> ConversationInfo
        {
            get => Initializer.GetOrLoad(ref _convInfoFields, LoadConversationInfo, (x) => NotifyPropertyChanged(nameof(ConversationInfo)), false, _mailItem);
            set { _convInfoFields = value; NotifyPropertyChanged(); }
        }
        internal Pair<List<MailItemHelper>> LoadConversationInfo()
        {
            if (Count.Expanded <= 0) 
            { 
                throw new InvalidOperationException(
                    $"{ConversationInfo} cannot be loaded if {Df} cannot be resolved");
            }
            
            var df = Df.Expanded;
            var olNs = _globals.Ol.App.GetNamespace("MAPI");
            var convInfoExpanded = Enumerable
                .Range(0, Count.Expanded)
                .Select(indexRow => MailItemHelper.FromDf(df, indexRow, _globals, Token))
                .OrderByDescending(itemInfo => itemInfo.ConversationID)
                .ToList();

            var convInfoSameFolder = convInfoExpanded.Where(
                itemInfo => itemInfo.FolderName == ((Folder)_mailItem.Parent).Name).ToList();

            return new Pair<List<MailItemHelper>>(sameFolder: convInfoSameFolder, expanded: convInfoExpanded);
            
        }
        public async Task<Pair<List<MailItemHelper>>> LoadConversationInfoAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();

            //TaskScheduler priority = backgroundLoad ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;
            TaskCreationOptions options = backgroundLoad ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;

            var olNs = _globals.Ol.App.GetNamespace("MAPI");

            var tasksConvInfoExp = Enumerable
                .Range(0, Count.Expanded)
                .Select(indexRow =>
                {
                    var entryId = (string)Df.Expanded["EntryID"][indexRow];
                    if (entryId == MailInfo.EntryId)
                    {
                        return Task.FromResult(this.MailInfo);
                    }
                    else
                    {
                        return MailItemHelper.FromDfAsync(Df.Expanded, indexRow, _globals, token, backgroundLoad);
                    }
                });

            var convInfoExpanded = (await Task.WhenAll(tasksConvInfoExp))
                                   .OrderByDescending(itemInfo => itemInfo.ConversationID)
                                   .ToList();

            if (UpdateUI is not null)
            {
                token.ThrowIfCancellationRequested();
                await UiThread.Dispatcher.InvokeAsync(() => UpdateUI(ConversationInfo.Expanded));
            }

            var convInfoSameFolder = convInfoExpanded.Where(
                itemInfo => itemInfo.FolderName == ((Folder)_mailItem.Parent).Name).ToList();

            var pair = new Pair<List<MailItemHelper>>(sameFolder: convInfoSameFolder, expanded: convInfoExpanded);
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
            return new Pair<IList<MailItem>>(sameFolder: sameFolder, expanded: expanded);
        }
        public async Task LoadConversationItemsAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();

            //TaskScheduler priority = backgroundLoad ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;
            TaskCreationOptions options = backgroundLoad ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;

            await Task.Run(() => ConversationItems = LoadConversationItems(),
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
            set => Initializer.SetAndSave(ref _df, value, (x) => NotifyPropertyChanged(nameof(Df)));
        }

        internal Pair<DataFrame> LoadDf() 
        {                        
            var dfExpanded = _mailItem.GetConversation()
                                      .GetConversationDf()
                                      .FilterConversation(
                                            ((Folder)_mailItem.Parent).Name, 
                                            false, 
                                            true);

            var dfSameFolder = dfExpanded.FilterConversation(((Folder)_mailItem.Parent).Name, true, true);
            logger.Debug($"Source mail: {_mailItem.EntryID}");
            logger.Debug(dfExpanded.PrettyText());
            return new Pair<DataFrame>(sameFolder: dfSameFolder, expanded: dfExpanded); 
            
        }
        internal void DfNotifyIfNotNull(Pair<DataFrame> df)
        {
            if (df.SameFolder is not null && df.Expanded is not null) { NotifyPropertyChanged(nameof(Df)); }     
        }
        public async Task LoadDfAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();
            
            var dfRaw = await _mailItem.GetConversationDfAsync(Token).ConfigureAwait(false);
            var dfExpanded = dfRaw.FilterConversation(((Folder)_mailItem.Parent).Name, false, true);
            dfExpanded = dfExpanded.Filter(dfExpanded["SentOn"].ElementwiseNotEquals<string>(""));
            var dfSameFolder = dfExpanded.FilterConversation(((Folder)_mailItem.Parent).Name, true, true);
            
            Df = new Pair<DataFrame>(sameFolder: dfSameFolder, expanded: dfExpanded);
            
            
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

        #endregion Properties

        #region INotifyPropertyChanged implementation

        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Df))
            {
                FullyLoaded = false;
                try
                {
                    await BackgroundInitInfoItemsAsync(_token).ConfigureAwait(false);
                    FullyLoaded = true;
                }
                catch (OperationCanceledException)
                {
                    logger.Debug("Background load of ConversationResolver cancelled"); 
                }
            }
            else if (e.PropertyName == nameof(UpdateUI))
            {
                if (FullyLoaded)
                {
                    await UiThread.Dispatcher.InvokeAsync(() => UpdateUI(ConversationInfo.Expanded));
                }
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
