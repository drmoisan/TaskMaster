using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.Extensions;

namespace QuickFiler.Helper_Classes
{
    public partial class ConversationResolver
    {
        #region ConversationInfo

        private Pair<List<MailItemHelper>> _convInfoFields;
        public Pair<List<MailItemHelper>> ConversationInfo
        {
            get =>
                Initializer.GetOrLoad(
                    ref _convInfoFields,
                    LoadConversationInfo,
                    (x) => NotifyPropertyChanged(nameof(ConversationInfo)),
                    false,
                    _mailItem
                );
            set
            {
                _convInfoFields = value;
                NotifyPropertyChanged();
            }
        }

        internal Pair<List<MailItemHelper>> LoadConversationInfo()
        {
            if (Count.Expanded <= 0)
            {
                // When the expanded conversation DataFrame is empty (e.g., Junk E-mail where
                // FilterConversation removes all rows), return a single-item fallback containing
                // the current mail item. Throwing here propagated an unhandled exception to the
                // VSTO UI thread for a recoverable scenario.
                //
                // Do NOT access ConversationInfo or Df in this path: they are lazy properties
                // backed by this same loader and would recurse back into LoadConversationInfo().
                logger.Error(
                    $"{nameof(ConversationInfo)} cannot be resolved: {nameof(Count)}.Expanded = {Count.Expanded}. Returning single-item fallback."
                );
                var fallbackList = new List<MailItemHelper> { MailHelper };
                return new Pair<List<MailItemHelper>>(
                    sameFolder: fallbackList,
                    expanded: fallbackList
                );
            }

            var df = Df.Expanded;
            var convInfoExpanded = Enumerable
                .Range(0, Count.Expanded)
                .Select(indexRow => MailItemHelper.FromDf(df, indexRow, _globals, Token))
                .OrderByDescending(itemInfo => itemInfo.ConversationID)
                .ToList();

            var convInfoSameFolder = convInfoExpanded
                .Where(itemInfo => itemInfo.FolderName == ((Folder)_mailItem.Parent).Name)
                .ToList();

            return new Pair<List<MailItemHelper>>(
                sameFolder: convInfoSameFolder,
                expanded: convInfoExpanded
            );
        }

        public async Task<Pair<List<MailItemHelper>>> LoadConversationInfoAsync(
            CancellationToken token,
            bool backgroundLoad
        )
        {
            token.ThrowIfCancellationRequested();

            //TaskScheduler priority = backgroundLoad ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;
            TaskCreationOptions options = backgroundLoad
                ? TaskCreationOptions.LongRunning
                : TaskCreationOptions.None;

            var tasksConvInfoExp = Enumerable
                .Range(0, Count.Expanded)
                .Select(indexRow =>
                {
                    var entryId = (string)Df.Expanded["EntryID"][indexRow];
                    if (entryId == MailHelper.EntryId)
                    {
                        return Task.FromResult(this.MailHelper);
                    }
                    else
                    {
                        return MailItemHelper.FromDfAsync(
                            Df.Expanded,
                            indexRow,
                            _globals,
                            token,
                            backgroundLoad
                        );
                    }
                });

            var convInfoExpanded = (await Task.WhenAll(tasksConvInfoExp))
                .OrderBy(x => x.ConversationID)
                .ToList();

            if (convInfoExpanded?.Count > 0)
            {
                var idx = convInfoExpanded.FindIndex(x => x.EntryId == MailHelper.EntryId);
                if (idx > -1)
                {
                    convInfoExpanded[idx] = MailHelper;
                }
            }
            else
            {
                convInfoExpanded = [MailHelper];
            }

            var convInfoSameFolder = convInfoExpanded
                .Where(itemInfo => itemInfo.FolderName == ((Folder)_mailItem.Parent).Name)
                .ToList();

            var pair = new Pair<List<MailItemHelper>>(
                sameFolder: convInfoSameFolder,
                expanded: convInfoExpanded
            );

            // Assign ConversationInfo before calling UpdateUI so that any subsequent read
            // of ConversationInfo.Expanded returns the cached value rather than re-entering
            // the synchronous LoadConversationInfo(), which throws when Count.Expanded == 0
            // (e.g. items in Junk E-mail where FilterConversation removes all rows).
            ConversationInfo = pair;

            if (UpdateUI is not null)
            {
                token.ThrowIfCancellationRequested();
                var uiPublishCount = Interlocked.Increment(ref _uiPublishCount);
                LogConversationResolverTiming(
                    "LoadConversationInfoAsync UI publication cadence | ui publication",
                    $"uiPublishCount={uiPublishCount}; repeated ui publishes tracked during background initialization"
                );
                // Pass pair.Expanded directly to avoid triggering the lazy property getter
                // and the associated synchronous LoadConversationInfo() call.
                await UiThread.Dispatcher.InvokeAsync(() => UpdateUI(pair.Expanded));
            }

            return pair;
        }

        #endregion

        #region ConversationItems

        private Pair<IList<MailItem>> _conversationItems;
        public Pair<IList<MailItem>> ConversationItems
        {
            get =>
                Initializer.GetOrLoad(
                    ref _conversationItems,
                    LoadConversationItems,
                    (x) => NotifyPropertyChanged(nameof(ConversationItems)),
                    false,
                    _mailItem
                );
            set
            {
                _conversationItems = value;
                NotifyPropertyChanged();
            }
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
            TaskCreationOptions options = backgroundLoad
                ? TaskCreationOptions.LongRunning
                : TaskCreationOptions.None;

            await Task.Run(() => ConversationItems = LoadConversationItems(), token);
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
            var dfExpanded = _mailItem
                .GetConversation()
                .GetConversationDf()
                .FilterConversation(((Folder)_mailItem.Parent).Name, false, true);

            var dfSameFolder = dfExpanded.FilterConversation(
                ((Folder)_mailItem.Parent).Name,
                true,
                true
            );
            return new Pair<DataFrame>(sameFolder: dfSameFolder, expanded: dfExpanded);
        }

        internal void DfNotifyIfNotNull(Pair<DataFrame> df)
        {
            if (df.SameFolder is not null && df.Expanded is not null)
            {
                NotifyPropertyChanged(nameof(Df));
            }
        }

        public async Task LoadDfAsync(CancellationToken token, bool backgroundLoad)
        {
            token.ThrowIfCancellationRequested();
            _mailItem.ThrowIfNull();

            var loadDfStopwatch = Stopwatch.StartNew();
            LogConversationResolverTiming(
                "[Conversation resolver timing] LoadDfAsync dataframe load start | dataframe load",
                "conversation snapshots captured before repeated ui publishes"
            );

            var dfRaw = await _mailItem.GetConversationDfAsync(Token).ConfigureAwait(false);
            var parent = _mailItem.Parent as Folder;
            var folderName = parent?.Name ?? string.Empty;

            var dfExpanded = dfRaw.FilterConversation(folderName, false, true);
            dfExpanded = dfExpanded.Filter(dfExpanded["SentOn"].ElementwiseNotEquals<string>(""));
            var dfSameFolder = folderName.IsNullOrEmpty()
                ? dfExpanded
                : dfExpanded.FilterConversation(((Folder)_mailItem.Parent).Name, true, true);

            Df = new Pair<DataFrame>(sameFolder: dfSameFolder, expanded: dfExpanded);

            LogConversationResolverTiming(
                "LoadDfAsync dataframe load complete | dataframe load",
                $"conversation snapshots materialized; repeated ui publishes deferred; elapsedMs={loadDfStopwatch.ElapsedMilliseconds}"
            );
        }

        // Sentinel (-1, -1) means "not yet loaded". We cannot rely on default(Pair<int>) == (0,0)
        // as the uninitialized sentinel because a real count of (0,0) – both DataFrames empty –
        // is indistinguishable from it, causing GetOrLoad to call LoadCount on every access.
        private Pair<int> _count = new Pair<int>(-1, -1);

        public Pair<int> Count
        {
            // Use the isInitialized-predicate overload so that a loaded value of (0,0) is
            // correctly treated as initialized. Expanded < 0 means LoadCount has not run yet.
            get => Initializer.GetOrLoad(ref _count, static v => v.Expanded >= 0, LoadCount);
            internal set => _count = value;
        }

        internal Pair<int> LoadCount()
        {
            var count = new Pair<int>(-1, -1);
            var df = Df;
            if (df.SameFolder is not null)
            {
                count.SameFolder = df.SameFolder.Rows.Count();
            }
            if (df.Expanded is not null)
            {
                count.Expanded = df.Expanded.Rows.Count();
            }
            return count;
        }

        #endregion

        #region INotifyPropertyChanged implementation

        protected void NotifyPropertyChanged(
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = ""
        )
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
                catch (OperationCanceledException) { }
            }
            else if (e.PropertyName == nameof(UpdateUI))
            {
                if (FullyLoaded)
                {
                    await UiThread.Dispatcher.InvokeAsync(() =>
                        UpdateUI(ConversationInfo.Expanded)
                    );
                }
            }
        }

        #endregion
    }
}
