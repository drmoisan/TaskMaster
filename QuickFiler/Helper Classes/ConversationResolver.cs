using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Extensions;

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

    public partial class ConversationResolver : INotifyPropertyChanged, IConversationResolver
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private static string DescribeSynchronizationContext(SynchronizationContext syncContext)
        {
            return syncContext?.GetType().FullName ?? "null";
        }

        private static string BuildConversationResolverTimingContext()
        {
            return $"threadId={Thread.CurrentThread.ManagedThreadId}; syncContext={DescribeSynchronizationContext(SynchronizationContext.Current)}";
        }

        private static void LogConversationResolverTiming(string phase, string details = null)
        {
            var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
            var phaseLabel = phase.StartsWith(
                "[Conversation resolver timing]",
                StringComparison.Ordinal
            )
                ? phase
                : $"[Conversation resolver timing] {phase}";
            logger.Debug(
                $"{phaseLabel} | {BuildConversationResolverTimingContext()}{detailSegment}"
            );
        }

        #region Constructors and Initializers

        private ConversationResolver() { }

        public ConversationResolver(IApplicationGlobals appGlobals, MailItem mailItem)
        {
            _globals = appGlobals;
            _mailItem = mailItem;
        }

        public ConversationResolver(
            IApplicationGlobals appGlobals,
            MailItem mailItem,
            CancellationTokenSource tokenSource,
            CancellationToken token,
            System.Action<List<MailItemHelper>> updateUI = null
        )
        {
            _globals = appGlobals;
            _tokenSource = tokenSource;
            _token = token;
            _mailItem = mailItem;
            MailHelper = new MailItemHelper(mailItem, _globals); //.LoadPriority(appGlobals, token);
            _updateUI = updateUI;
        }

        public static async Task<ConversationResolver> LoadAsync(
            IApplicationGlobals globals,
            MailItem mailItem,
            CancellationTokenSource tokenSource,
            CancellationToken token,
            bool loadAll,
            System.Action<List<MailItemHelper>> updateUI = null
        )
        {
            var resolver = new ConversationResolver(globals, mailItem);
            resolver.Token = token;
            resolver.TokenSource = tokenSource;

            if (updateUI is not null)
                resolver.UpdateUI = updateUI;

            resolver.MailHelper = await MailItemHelper.FromMailItemAsync(
                mailItem,
                globals,
                token,
                loadAll
            );

            if (loadAll)
            {
                await resolver.LoadDfAsync(token, loadAll);
                await resolver.LoadConversationInfoAsync(token, loadAll);
                await resolver.LoadConversationItemsAsync(token, loadAll);
                resolver.PropertyChanged += resolver.Handler_PropertyChanged;
            }
            else
            {
                // Subscribe after LoadDfAsync so initial dataframe assignment does not trigger background initialization.
                await resolver.LoadDfAsync(token, loadAll);
                resolver.PropertyChanged += resolver.Handler_PropertyChanged;
            }

            return resolver;
        }

        public static async Task<ConversationResolver> LoadAsync(
            IApplicationGlobals globals,
            MailItemHelper helper,
            CancellationTokenSource tokenSource,
            CancellationToken token,
            bool loadAll,
            System.Action<List<MailItemHelper>> updateUI = null
        )
        {
            var resolver = new ConversationResolver();
            resolver._globals = globals;
            resolver.MailHelper = helper;
            resolver.Mail = helper.Item;
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
                // Subscribe after LoadDfAsync so initial dataframe assignment does not trigger background initialization.
                await resolver.LoadDfAsync(token, loadAll);
                resolver.PropertyChanged += resolver.Handler_PropertyChanged;
            }

            return resolver;
        }

        // Constructor designed to reuse class for items that might not be in the same conversation
        // but are in a collection together
        public static async Task<ConversationResolver> LoadAsync(
            IApplicationGlobals globals,
            IEnumerable<MailItem> mailItems,
            CancellationTokenSource tokenSource,
            CancellationToken token,
            System.Action<List<MailItemHelper>> updateUI = null
        )
        {
            var resolver = new ConversationResolver();
            resolver._globals = globals;
            resolver.Token = token;
            resolver.TokenSource = tokenSource;

            if (updateUI is not null)
                resolver.UpdateUI = updateUI;

            var helpers = await mailItems
                .ToAsyncEnumerable()
                .SelectAwaitWithCancellation(
                    async (mail, token) =>
                        await Task.Run(async () =>
                        {
                            var helper = await MailItemHelper.FromMailItemAsync(
                                mail,
                                globals,
                                token,
                                false
                            );
                            _ = helper.Tokens;
                            return helper;
                        })
                )
                .ToListAsync();

            resolver.MailHelper = helpers.First();
            resolver.Mail = resolver.MailHelper.Item;
            resolver.ConversationInfo = new Pair<List<MailItemHelper>>(
                sameFolder: helpers,
                expanded: helpers
            );
            await resolver.LoadConversationItemsAsync(token, true);
            resolver.Count = new Pair<int>(sameFolder: helpers.Count, expanded: helpers.Count);
            resolver.PropertyChanged += resolver.Handler_PropertyChanged;
            return resolver;
        }

        public async Task BackgroundInitInfoItemsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var backgroundInitStopwatch = Stopwatch.StartNew();
            LogConversationResolverTiming(
                "BackgroundInitInfoItemsAsync start | info-item background initialization"
            );

            await LoadConversationInfoAsync(token, true);
            await LoadConversationItemsAsync(token, true);

            LogConversationResolverTiming(
                "BackgroundInitInfoItemsAsync complete | info-item background initialization",
                $"backgroundInitDurationMs={backgroundInitStopwatch.ElapsedMilliseconds}"
            );
        }

        #endregion Constructors and Initializers

        #region Properties

        private CancellationToken _token;
        internal CancellationToken Token
        {
            get => _token;
            set => _token = value;
        }

        private CancellationTokenSource _tokenSource;
        internal CancellationTokenSource TokenSource
        {
            get => _tokenSource;
            set => _tokenSource = value;
        }

        protected IApplicationGlobals _globals;

        protected MailItem _mailItem;
        public MailItem Mail
        {
            get => _mailItem;
            protected set => _mailItem = value;
        }

        private bool _fullyLoaded = false;
        public bool FullyLoaded
        {
            get => _fullyLoaded;
            protected set => _fullyLoaded = value;
        }

        protected System.Action<List<MailItemHelper>> _updateUI;
        public System.Action<List<MailItemHelper>> UpdateUI
        {
            get => _updateUI;
            set =>
                Initializer.SetAndSave(
                    ref _updateUI,
                    value,
                    (x) => NotifyPropertyChanged(nameof(UpdateUI))
                );
        }

        protected MailItemHelper _mailInfo;
        public MailItemHelper MailHelper
        {
            get => _mailInfo;
            set => _mailInfo = value;
        }

        protected object _parent;
        public object Parent
        {
            get => _parent;
            protected internal set => _parent = value;
        }

        private int _uiPublishCount;

        #endregion Properties

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
