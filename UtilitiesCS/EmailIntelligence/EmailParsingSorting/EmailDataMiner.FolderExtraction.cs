#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic.Devices;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public partial class EmailDataMiner
    {
        internal struct FolderStruct(
            FolderWrapper folderInfo,
            long cumulativeSize,
            long chunkNumber,
            int cumulativeCount
        )
        {
            public FolderWrapper FolderInfo { get; set; } = folderInfo;
            public long CumulativeSize { get; set; } = cumulativeSize;
            public long ChunkNumber { get; set; } = chunkNumber;
            public int CumulativeCount { get; set; } = cumulativeCount;
        }

        [ExcludeFromCodeCoverage]
        internal virtual FolderTree GetOlFolderTree()
        {
            return GetOlFolderTree(null);
        }

        [ExcludeFromCodeCoverage]
        internal virtual FolderTree GetOlFolderTree(ProgressTracker? progress)
        {
            var snapshot = GetOlFolderSnapshotAsync(progress).GetAwaiter().GetResult();
            var selectionOverlay = new FolderTreeSelectionOverlay(
                _globals.TD.FilteredFolderScraping.Keys
            );
            using var view = new FolderTreeCompatibilityView(snapshot, selectionOverlay);
            return FolderTree.FromRoots(view.Roots);
        }

        [ExcludeFromCodeCoverage]
        internal virtual IEnumerable<MAPIFolder> QueryOlFolders(FolderTree tree)
        {
            var folders = tree
                .Roots.SelectMany(root => root.FlattenIf(node => !node.Selected))
                .Select(x => x.OlFolder!);
            return folders;
        }

        internal virtual IEnumerable<MAPIFolder> QueryOlFolders(FolderTreeSnapshot snapshot)
        {
            var selectionOverlay = new FolderTreeSelectionOverlay(
                _globals.TD.FilteredFolderScraping.Keys
            );
            var resolver = CreateFolderHandleResolver();
            return snapshot
                .NodesByKey.Values.Where(node => !selectionOverlay.IsSelected(node))
                .Select(node => resolver.TryResolve(node, out var folder) ? folder : null)
                .OfType<MAPIFolder>();
        }

        [ExcludeFromCodeCoverage]
        internal virtual IEnumerable<FolderWrapper> QueryOlFolderInfo(FolderTree tree)
        {
            var folders = tree.Roots.SelectMany(root => root.FlattenIf(node => !node.Selected));
            return folders;
        }

        internal virtual async Task<FolderTreeSnapshot> GetOlFolderSnapshotAsync(
            ProgressTracker? progress = null
        )
        {
            progress?.Report(0, "Getting cached folders");
            var archiveRoot = _globals.Ol.ArchiveRoot;
            var request = string.IsNullOrWhiteSpace(archiveRoot?.StoreID)
                ? FolderTreeRequest.AllStores(allowStaleSnapshot: true)
                : FolderTreeRequest.ForStore(archiveRoot!.StoreID, allowStaleSnapshot: true);
            var snapshot = await _globals
                .Ol.FolderTreeService.GetSnapshotAsync(request, CancellationToken.None)
                .ConfigureAwait(false);
            var archiveNode = archiveRoot is null
                ? null
                : snapshot.FindByPath(archiveRoot.StoreID, archiveRoot.FolderPath);
            return archiveNode is null
                ? snapshot
                : FolderTreeSnapshotQueries.CreateSubtreeSnapshot(snapshot, archiveNode);
        }

        [ExcludeFromCodeCoverage]
        internal virtual IFolderHandleResolver CreateFolderHandleResolver()
        {
            return new OutlookFolderHandleResolver(_globals.Ol.NamespaceMAPI);
        }

        internal virtual IEnumerable<FolderWrapper> QueryOlFolderInfo(FolderTreeSnapshot snapshot)
        {
            var excluded = new HashSet<string>(
                _globals.TD.FilteredFolderScraping.Keys ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase
            );
            var resolver = CreateFolderHandleResolver();
            var archiveRoot = _globals.Ol.ArchiveRoot;
            return snapshot
                .NodesByKey.Values.Where(node => !excluded.Contains(node.RelativePath))
                .Select(node => CreateFolderWrapper(node, resolver, archiveRoot));
        }

        private static FolderWrapper CreateFolderWrapper(
            FolderTreeSnapshotNode node,
            IFolderHandleResolver resolver,
            MAPIFolder archiveRoot
        )
        {
            return resolver.TryResolve(node, out var folder) && folder is MAPIFolder mapiFolder
                ? new FolderWrapper(mapiFolder, archiveRoot)
                : new FolderWrapper(false, 0, 0, node.DisplayName, node.RelativePath);
        }

        [ExcludeFromCodeCoverage]
        internal async Task<FolderWrapper[]> GetInitializedFolderInfo()
        {
            var (tokenSource, cancel, progress, sw) = await ProgressPackage.CreateAsTupleAsync();
            //screen: _globals.Ol.GetExplorerScreen());
            FolderWrapper[]? folders = null;

            progress!.Report(0, "Getting Folders");
            var snapshot = await GetOlFolderSnapshotAsync(progress).ConfigureAwait(false);
            folders = QueryOlFolderInfo(snapshot).ToArray();
            var count = folders.Count();
            if (count == 0)
            {
                return Array.Empty<FolderWrapper>();
            }

            progress.Report(0, "Getting Counts/Sizes");

            await AsyncMultiTasker.AsyncMultiTaskChunker(
                folders,
                async (folder) =>
                {
                    await folder.LoadLazyAsync();
                },
                progress,
                "Getting Counts/Sizes",
                cancel
            );

            progress.Report(100);

            return folders.Where(x => x.ItemCount > 0).ToArray();
        }

        internal FolderStruct[] AddRollingMeasures(long maxChunkSize, FolderWrapper[] folders)
        {
            var folderRecords = folders
                .Scan(
                    new FolderStruct(default(FolderWrapper)!, 0L, 0L, 0),
                    (current, next) =>
                        new FolderStruct
                        {
                            FolderInfo = next,
                            CumulativeSize =
                                current.CumulativeSize + (next.FolderSize) < maxChunkSize
                                    ? current.CumulativeSize + (next.FolderSize)
                                    : next.FolderSize,
                            ChunkNumber =
                                current.CumulativeSize + (next.FolderSize) < maxChunkSize
                                    ? current.ChunkNumber
                                    : current.ChunkNumber + 1,
                            CumulativeCount = current.CumulativeCount + (next.ItemCount),
                        }
                )
                .ToArray();
            return folderRecords;
        }

        private static void LogFolderChunkMetrics(
            long availableRAM,
            FolderWrapper[][] folderChunks,
            long totalSize,
            int totalCount
        )
        {
            //logger.Debug($"Available RAM {availableRAM / (double)1000000:N0} MG");
            //logger.Debug($"Max Object Size in VSTO {MaxObjectSize / (double)1000000000:N1} GB");
            //logger.Debug($"Total Size: {totalSize / (double)1000000:N0} MG");
            //logger.Debug($"Total Item Count: {totalCount:N0}");
            //logger.Debug($"Average Item Size: {(totalSize / (double)totalCount) / 1000:N0} K");
            //logger.Debug($"Total Chunk Count: {folderChunks.Count():N0}");
        }

        [ExcludeFromCodeCoverage]
        internal virtual async Task<bool> TryResolveMapiHandles(FolderWrapper[] folders)
        {
            var snapshot = await GetOlFolderSnapshotAsync().ConfigureAwait(false);
            return TryResolveMapiHandles(
                snapshot,
                folders,
                CreateFolderHandleResolver(),
                _globals.Ol.ArchiveRoot
            );
        }

        internal static bool TryResolveMapiHandles(
            FolderTreeSnapshot snapshot,
            FolderWrapper[] folders,
            IFolderHandleResolver resolver,
            MAPIFolder archiveRoot
        )
        {
            if (snapshot is null || folders is null || resolver is null)
            {
                return false;
            }

            var handles = snapshot.NodesByKey.Values.ToDictionary(
                node => node.RelativePath,
                StringComparer.OrdinalIgnoreCase
            );
            foreach (var folder in folders)
            {
                if (!handles.TryGetValue(folder.RelativePath!, out var node))
                {
                    logger.Warn(
                        $"Failed to resolve folder handle for {folder.Name}. Terminating and rebuilding."
                    );
                    return false;
                }

                if (
                    !resolver.TryResolve(node, out var folderHandle)
                    || folderHandle is not MAPIFolder mapiFolder
                )
                {
                    logger.Warn(
                        $"Failed to resolve folder handle for {folder.Name}. Terminating and rebuilding."
                    );
                    return false;
                }

                var subscriptions = folder.SubscriptionStatus;
                folder.UnSubscribeToPropertyChanged(
                    IFolderWrapper.PropertyEnum.OlRoot | IFolderWrapper.PropertyEnum.OlFolder
                );
                folder.OlRoot = archiveRoot;
                folder.OlFolder = mapiFolder;
                folder.SubscribeToPropertyChanged(subscriptions);
            }

            return true;
        }

        internal static bool TryResolveMapiHandles(FolderTree tree, FolderWrapper[] folders)
        {
            if (folders is null)
            {
                return false;
            }

            var handles = tree.Roots.SelectMany(root => root.Flatten()).ToList();
            int last = -1;
            FolderWrapper? handle = null;

            foreach (var folder in folders)
            {
                if (
                    ++last >= 0
                    && last < handles.Count()
                    && handles[last].RelativePath == folder.RelativePath
                )
                {
                    handle = handles[last];
                }
                else
                {
                    last = handles.FindIndex(x => x.RelativePath == folder.RelativePath);
                    if (last == -1)
                    {
                        logger.Warn(
                            $"Failed to resolve folder handle for {folder.Name}. Terminating and rebuilding."
                        );
                        return false;
                    }
                    handle = handles[last];
                }

                var subscriptions = folder.SubscriptionStatus;

                folder.UnSubscribeToPropertyChanged(
                    IFolderWrapper.PropertyEnum.OlRoot | IFolderWrapper.PropertyEnum.OlFolder
                );

                folder.OlRoot = handle.OlRoot;
                folder.OlFolder = handle.OlFolder;

                folder.SubscribeToPropertyChanged(subscriptions);
            }

            return true;
        }

        [ExcludeFromCodeCoverage]
        internal async Task<FolderWrapper[][]> ExtractOlFolderChunks(bool reload = false)
        {
            // Grab selected OlFolderInfo objects from a OlFolderTree, flatten to an array, and initialize
            FolderWrapper[]? folders = null;
            if (!reload)
            {
                folders = Deserialize<FolderWrapper[]>("StagingFolderRecords");
            }

            if (!reload && folders is not null && await TryResolveMapiHandles(folders))
            {
                // ForEachAwaitAsync is obsolete (CS0618) per the framework's migration
                // guidance ("Use the language support for async foreach instead"), but
                // replacing it with `await foreach` here is a control-flow change to a
                // production async method, not an annotation-only edit. Suppressing narrowly
                // preserves the exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
                await folders.ToAsyncEnumerable().ForEachAwaitAsync(x => x.LoadLazyAsync()); //.Select(x => x.LoadLazyAsync());
#pragma warning restore CS0618
            }
            else
            {
                folders = await GetInitializedFolderInfo();
                SerializeAndSave(folders, "StagingFolderRecords");
            }

            var availableRam = GetAvailablePhysicalMemory();
            var maxChunkSize = Math.Min(availableRam, MaxObjectSize) * 95 / 100;
            //logger.Debug($"Available RAM {availableRam / (double)1000000000:N2} GB");
            //logger.Debug($"Max Obj Size  {MaxObjectSize / (double)1000000000:N2} GB");
            //logger.Debug($"Min(RAM, Max) {maxChunkSize / (double)1000000000:N2} GB");

            var folderRecords = AddRollingMeasures(maxChunkSize, folders);
            SerializeAndSave(folderRecords, "StagingFolderRecordsWithTotals");

            var folderChunks = folderRecords
                .GroupBy(x => x.ChunkNumber)
                .Select(group => group.Select(x => x.FolderInfo).ToArray())
                .ToArray();

            var groupSummary = folderChunks
                .Select(
                    (x, i) =>
                        new
                        {
                            Group = i,
                            Size = x.Sum(y => y.FolderSize),
                            Folders = x.Count(),
                            Items = x.Sum(z => z.ItemCount),
                        }
                )
                .ToArray();

            var summaryText = groupSummary
                .Select(x =>
                    new string[]
                    {
                        $"{x.Group:N0}",
                        $"{x.Size / (double)1000000000:N2} GB",
                        $"{x.Folders:N0}",
                        $"{x.Items:N0}",
                    }
                )
                .ToArray()
                .ToFormattedText(
                    ["Group", "Size", "Folders", "Count"],
                    [
                        Enums.Justification.Center,
                        Enums.Justification.Right,
                        Enums.Justification.Right,
                        Enums.Justification.Right,
                    ],
                    "Summary Metrics"
                );

            //logger.Debug($"Summary data on folder chunking\n{summaryText}");

            SerializeAndSave(folderChunks, "StagingFolderChunks");

            var totalSize = groupSummary.Sum(x => x.Size);
            var totalCount = groupSummary.Sum(x => x.Items);

            LogFolderChunkMetrics(maxChunkSize, folderChunks, totalSize, totalCount);

            return folderChunks;
        }

        [ExcludeFromCodeCoverage]
        internal IEnumerable<(MailItem Mail, FolderWrapper FolderInfo)> QueryMailTuples(
            IEnumerable<FolderWrapper> folders
        )
        {
            var mailTuples = folders
                .Select(folderInfo => (folderInfo.OlFolder, folderInfo))
                .SelectMany(tup =>
                    tup.OlFolder!.Items.Cast<object>()
                        .Where(obj => obj is MailItem)
                        .Cast<MailItem>()
                        .Select(mail => (mail, tup.folderInfo))
                );

            return mailTuples;
        }

        [ExcludeFromCodeCoverage]
        internal virtual IEnumerable<MailItem> QueryMailItems(IEnumerable<MAPIFolder> folders)
        {
            var mailItems = folders.SelectMany(folder =>
                folder.Items.Cast<object>().Where(obj => obj is MailItem).Cast<MailItem>()
            );
            return mailItems;
        }

        [ExcludeFromCodeCoverage]
        internal List<MailItem> ConsumeLinq(
            IEnumerable<MAPIFolder> folders,
            IEnumerable<MailItem> mailItems,
            ProgressTracker progress
        )
        {
            var prelimCount = folders.Select(folder => folder.Items.Count).Sum();
            _sw!.LogDuration("Get Preliminary Count");

            var mailList = mailItems.ToList(prelimCount, progress);
            _sw!.LogDuration("Load MailItems");

            return mailList;
        }

        [ExcludeFromCodeCoverage]
        internal async Task<IEnumerable<MailItem>> ScrapeEmails(CancellationTokenSource tokenSource)
        {
            return await Task.Run(ScrapeEmailsCore, tokenSource.Token);
        }

        [ExcludeFromCodeCoverage]
        internal async Task<IEnumerable<MailItem>> ScrapeEmails(
            CancellationTokenSource tokenSource,
            ProgressTracker progress
        )
        {
            return await Task.Run(() => ScrapeEmailsCore(progress), tokenSource.Token);
        }

        internal IEnumerable<MailItem> ScrapeEmailsCore()
        {
            var snapshot = GetOlFolderSnapshotAsync().GetAwaiter().GetResult();
            _sw!.LogDuration(nameof(GetOlFolderSnapshotAsync));

            var folders = QueryOlFolders(snapshot);
            _sw!.LogDuration(nameof(QueryOlFolders));

            var mailItemsQuery = QueryMailItems(folders);
            _sw!.LogDuration(nameof(QueryMailItems));

            _sw!.WriteToLog(clear: false);
            return mailItemsQuery;
        }

        internal IEnumerable<MailItem> ScrapeEmailsCore(ProgressTracker progress)
        {
            progress.Report(0, "Building Outlook Folder Tree");
            var snapshot = GetOlFolderSnapshotAsync(progress).GetAwaiter().GetResult();
            _sw!.LogDuration(nameof(GetOlFolderSnapshotAsync));

            var folders = QueryOlFolders(snapshot);
            _sw!.LogDuration(nameof(QueryOlFolders));

            var mailItemsQuery = QueryMailItems(folders);
            _sw!.LogDuration(nameof(QueryMailItems));

            _sw!.WriteToLog(clear: false);
            return mailItemsQuery;
        }

        [ExcludeFromCodeCoverage]
        private static long GetAvailablePhysicalMemory()
        {
            return Convert.ToInt64(new ComputerInfo().AvailablePhysicalMemory);
        }
    }
}
