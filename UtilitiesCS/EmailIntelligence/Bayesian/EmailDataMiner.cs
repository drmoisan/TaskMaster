using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.HelperClasses;
using System.Windows;
using Newtonsoft.Json;
using System.Numerics;
using System.Collections.Concurrent;
using VBFunctions;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;
using UtilitiesCS.Extensions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;



namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class EmailDataMiner
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and private fields

        public EmailDataMiner(IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
        }

        private IApplicationGlobals _globals;
        private SegmentStopWatch _sw = default;
        internal const int MaxObjectSize = 2000000000;

        #endregion Constructors and private fields

        #region ETL - Extract, Transform, Load For Data Mining

        public async Task<ScBag<MinedMailInfo>> MineEmails()
        {
            if (SynchronizationContext.Current is null) { SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext()); }

            var offline = await ToggleOfflineMode(_globals.Ol.NamespaceMAPI.Offline);

            var folderGroups = await Task.Run(async () => await ExtractOlFolderChunks());

            await Transform(folderGroups, ToIItemInfoArray, withValidation: false);
            await Transform<IItemInfo[], MinedMailInfo[]>(ToMinedMail);
            await Transform<MinedMailInfo[], MinedMailInfo[]>(Consolidate);

            await ToggleOfflineMode(offline);
            return new ScBag<MinedMailInfo>(await Load<MinedMailInfo[]>());
        }

        #region ETL - EXTRACT Folders and Emails

        internal struct FolderStruct(OlFolderInfo folderInfo, long cumulativeSize, long chunkNumber, int cumulativeCount)
        {
            public OlFolderInfo FolderInfo { get; set; } = folderInfo;
            public long CumulativeSize { get; set; } = cumulativeSize;
            public long ChunkNumber { get; set; } = chunkNumber;
            public int CumulativeCount { get; set; } = cumulativeCount;
        }

        internal OlFolderTree GetOlFolderTree()
        {
            var tree = new OlFolderTree(_globals.Ol.ArchiveRoot, _globals.TD.FilteredFolderScraping.Keys.ToList());
            return tree;
        }

        internal OlFolderTree GetOlFolderTree(ProgressTracker progress)
        {
            var tree = new OlFolderTree(_globals.Ol.ArchiveRoot, _globals.TD.FilteredFolderScraping.Keys.ToList(), progress);
            return tree;
        }

        internal IEnumerable<MAPIFolder> QueryOlFolders(OlFolderTree tree)
        {
            var folders = tree.Roots
                              .SelectMany(root => root
                              .FlattenIf(node => !node.Selected))
                              .Select(x => x.OlFolder);
            return folders;
        }

        internal IEnumerable<OlFolderInfo> QueryOlFolderInfo(OlFolderTree tree)
        {
            var folders = tree.Roots
                              .SelectMany(root => root
                              .FlattenIf(node => !node.Selected));
            return folders;
        }

        internal async Task<OlFolderInfo[]> GetInitializedFolderInfo()
        {
            var (tokenSource, cancel, progress, sw) = await ProgressPackage.CreateAsTupleAsync();
            //screen: _globals.Ol.GetExplorerScreen());
            OlFolderInfo[] folders = null;

            await Task.Run(
                () =>
                {
                    progress.Report(0, "Getting Folders");
                    var tree = GetOlFolderTree();
                    folders = QueryOlFolderInfo(tree).ToArray();
                    var count = folders.Count();
                    if (count == 0) { return; }

                    progress.Report(0, "Getting Counts/Sizes");
                }, cancel);

            await AsyncMultiTasker.AsyncMultiTaskChunker(folders, async (folder) =>
            {
                await folder.LoadLazyAsync();
            }, progress, "Getting Counts/Sizes", cancel);

            progress.Report(100);

            return folders.Where(x => x.ItemCount > 0).ToArray();
        }

        internal FolderStruct[] AddRollingMeasures(long maxChunkSize, OlFolderInfo[] folders)
        {
            var folderRecords = folders
                .Scan(new FolderStruct(default(OlFolderInfo), 0L, 0L, 0),
                (current, next) => new FolderStruct
                {
                    FolderInfo = next,
                    CumulativeSize = current.CumulativeSize + (next.FolderSize) < maxChunkSize ? current.CumulativeSize + (next.FolderSize) : next.FolderSize,
                    ChunkNumber = current.CumulativeSize + (next.FolderSize) < maxChunkSize ? current.ChunkNumber : current.ChunkNumber + 1,
                    CumulativeCount = current.CumulativeCount + (next.ItemCount)
                })
                .ToArray();
            return folderRecords;
        }

        private static void LogFolderChunkMetrics(long availableRAM, OlFolderInfo[][] folderChunks, long totalSize, int totalCount)
        {
            logger.Debug($"Available RAM {availableRAM / (double)1000000:N0} MG");
            logger.Debug($"Max Object Size in VSTO {MaxObjectSize / (double)1000000000:N1} GB");
            logger.Debug($"Total Size: {totalSize / (double)1000000:N0} MG");
            logger.Debug($"Total Item Count: {totalCount:N0}");
            logger.Debug($"Average Item Size: {(totalSize / (double)totalCount) / 1000:N0} K");
            logger.Debug($"Total Chunk Count: {folderChunks.Count():N0}");
        }

        internal async Task<bool> TryResolveMapiHandles(OlFolderInfo[] folders)
        {
            return await Task.Run(() =>
            {
                if (folders is null) { return false; }
                var handles = GetOlFolderTree().Roots.SelectMany(root => root.Flatten()).ToList();
                int last = -1;
                OlFolderInfo handle = null;

                foreach (var folder in folders)
                {
                    if (++last >= 0 && last < handles.Count() &&
                        handles[last].RelativePath == folder.RelativePath)
                    {
                        handle = handles[last];
                    }
                    else
                    {
                        last = handles.FindIndex(x => x.RelativePath == folder.RelativePath);
                        if (last == -1)
                        {
                            logger.Warn($"Failed to resolve folder handle for {folder.Name}. Terminating and rebuilding.");
                            return false;
                        }
                        handle = handles[last];
                    }

                    var subscriptions = folder.SubscriptionStatus;

                    folder.UnSubscribeToPropertyChanged(
                        OlFolderInfo.PropertyEnum.OlRoot |
                        OlFolderInfo.PropertyEnum.OlFolder);

                    folder.OlRoot = handle.OlRoot;
                    folder.OlFolder = handle.OlFolder;

                    folder.SubscribeToPropertyChanged(subscriptions);
                }
                return true;
            });
        }

        internal async Task<OlFolderInfo[][]> ExtractOlFolderChunks(bool reload = false)
        {
            // Grab selected OlFolderInfo objects from a OlFolderTree, flatten to an array, and initialize
            OlFolderInfo[] folders = null;
            if (!reload)
            {
                folders = Deserialize<OlFolderInfo[]>("StagingFolderRecords");
            }

            if (!reload && folders is not null && await TryResolveMapiHandles(folders))
            {
                await folders.ToAsyncEnumerable().ForEachAwaitAsync(x => x.LoadLazyAsync()); //.Select(x => x.LoadLazyAsync());
            }

            else
            {
                folders = await GetInitializedFolderInfo();
                SerializeAndSave(folders, "StagingFolderRecords");
            }

            var availableRam = Convert.ToInt64(ComputerInfo.AvailablePhysicalMemory);
            var maxChunkSize = Math.Min(availableRam, MaxObjectSize) * 95 / 100;
            logger.Debug($"Available RAM {availableRam / (double)1000000000:N2} GB");
            logger.Debug($"Max Obj Size  {MaxObjectSize / (double)1000000000:N2} GB");
            logger.Debug($"Min(RAM, Max) {maxChunkSize / (double)1000000000:N2} GB");

            var folderRecords = AddRollingMeasures(maxChunkSize, folders);
            SerializeAndSave(folderRecords, "StagingFolderRecordsWithTotals");

            var folderChunks = folderRecords
                .GroupBy(x => x.ChunkNumber)
                .Select(group => group
                .Select(x => x.FolderInfo)
                .ToArray())
                .ToArray();

            var groupSummary = folderChunks
                .Select((x, i) => new
                {
                    Group = i,
                    Size = x.Sum(y => y.FolderSize),
                    Folders = x.Count(),
                    Items = x.Sum(z => z.ItemCount)
                }).ToArray();

            var summaryText = groupSummary
                .Select(x => new string[]
                {
                    $"{x.Group:N0}",
                    $"{x.Size / (double)1000000000:N2} GB",
                    $"{x.Folders:N0}",
                    $"{x.Items:N0}"
                })
                .ToArray()
                .ToFormattedText(
                    ["Group", "Size", "Folders", "Count"],
                    [
                        Enums.Justification.Center,
                        Enums.Justification.Right,
                        Enums.Justification.Right,
                        Enums.Justification.Right
                    ],
                    "Summary Metrics");

            logger.Debug($"Summary data on folder chunking\n{summaryText}");

            SerializeAndSave(folderChunks, "StagingFolderChunks");

            var totalSize = groupSummary.Sum(x => x.Size);
            var totalCount = groupSummary.Sum(x => x.Items);

            LogFolderChunkMetrics(maxChunkSize, folderChunks, totalSize, totalCount);

            return folderChunks;
        }

        internal IEnumerable<(MailItem Mail, OlFolderInfo FolderInfo)> QueryMailTuples(IEnumerable<OlFolderInfo> folders)
        {
            var mailTuples = folders
                .Select(folderInfo => (folderInfo.OlFolder, folderInfo))
                .SelectMany(tup => tup.OlFolder
                                      .Items
                                      .Cast<object>()
                                      .Where(obj => obj is MailItem)
                                      .Cast<MailItem>()
                                      .Select(mail => (mail, tup.folderInfo)));

            return mailTuples;
        }

        internal IEnumerable<MailItem> QueryMailItems(IEnumerable<MAPIFolder> folders)
        {
            var mailItems = folders
                .SelectMany(folder => folder
                            .Items.Cast<object>()
                            .Where(obj => obj is MailItem)
                            .Cast<MailItem>());
            return mailItems;
        }

        internal List<MailItem> ConsumeLinq(IEnumerable<MAPIFolder> folders, IEnumerable<MailItem> mailItems, ProgressTracker progress)
        {
            var prelimCount = folders.Select(folder => folder.Items.Count).Sum();
            _sw.LogDuration("Get Preliminary Count");

            var mailList = mailItems.ToList(prelimCount, progress);
            _sw.LogDuration("Load MailItems");

            return mailList;
        }

        internal async Task<IEnumerable<MailItem>> ScrapeEmails(CancellationTokenSource tokenSource)
        {
            //List<MailItem> mailItems = null;
            IEnumerable<MailItem> mailItemsQuery = null;

            await Task.Run(() =>
            {
                // Query List of Outlook Folders if they are not on the skip list
                var tree = GetOlFolderTree();
                _sw.LogDuration(nameof(GetOlFolderTree));

                var folders = QueryOlFolders(tree);
                _sw.LogDuration(nameof(QueryOlFolders));

                // Query MailItems from these folders
                mailItemsQuery = QueryMailItems(folders);
                _sw.LogDuration(nameof(QueryMailItems));

                //// Load to memory
                //mailItems = ConsumeLinq(folders, mailItemsQuery, progress);
                //_sw.LogDuration(nameof(LinqToSimpleEmailList));
                _sw.WriteToLog(clear: false);
            }, tokenSource.Token);

            return mailItemsQuery;
        }

        internal async Task<IEnumerable<MailItem>> ScrapeEmails(CancellationTokenSource tokenSource, ProgressTracker progress)
        {
            //List<MailItem> mailItems = null;
            IEnumerable<MailItem> mailItemsQuery = null;

            await Task.Run(() =>
            {
                // Query List of Outlook Folders if they are not on the skip list
                progress.Report(0, "Building Outlook Folder Tree");
                var tree = GetOlFolderTree(progress);
                _sw.LogDuration(nameof(GetOlFolderTree));

                var folders = QueryOlFolders(tree);
                _sw.LogDuration(nameof(QueryOlFolders));

                // Query MailItems from these folders
                mailItemsQuery = QueryMailItems(folders);
                _sw.LogDuration(nameof(QueryMailItems));

                //// Load to memory
                //mailItems = ConsumeLinq(folders, mailItemsQuery, progress);
                //_sw.LogDuration(nameof(LinqToSimpleEmailList));
                _sw.WriteToLog(clear: false);
            }, tokenSource.Token);

            //progress.Report(100);

            //return mailItems;
            return mailItemsQuery;
        }

        #endregion ETL - EXTRACT Folders and Emails

        #region ETL - TRANSFORM For Data Mining

        public delegate Task<T> FolderGroupTransformer<T>(OlFolderInfo[] folders, int batch, int totalBatches, ProgressTrackerPane progress, CancellationToken token);

        public async Task Transform(OlFolderInfo[][] folderChunks, FolderGroupTransformer<IItemInfo[]> transformer, bool withValidation)
        {
            var (_, token, progress, _) = await ProgressPackage
                .CreateAsTuplePaneAsync(progressTrackerPane: _globals.AF.ProgressTracker).ConfigureAwait(false);
            _globals.AF.ProgressPane.Visible = true;
            var message = $"Transforming from {typeof(OlFolderInfo[][]).Name} to {typeof(IItemInfo[])}";
            progress.Report(0, message);

            var (completed, chunkCount) = await DeserializeAsync<(int, int)>("FolderGroupCompleted");
            if (folderChunks.Count() != chunkCount)
            {
                logger.Debug($"FolderChunks count {folderChunks.Count()} does not match chunkCount {chunkCount}. Restarting transformation with new data");
                chunkCount = folderChunks.Count();
                completed = 0;
            }
            var progressPerChunk = 100 / (double)chunkCount;

            for (int i = 0; i < chunkCount; i++)
            {
                if (i < completed)
                {
                    if (withValidation)
                    {
                        if (await ValidateJson<IItemInfo[]>(typeof(IItemInfo[]).Name, i.ToString("0000")))
                        {
                            progress.Report((int)((i + 1) * progressPerChunk), $"Validated group {i + 1} of {chunkCount}");
                            continue;
                        }
                    }
                    else
                    {
                        progress.Report((int)((i + 1) * progressPerChunk), $"Skipping group {i + 1} of {chunkCount}");
                        continue;
                    }
                }


                var result = await transformer(
                    folderChunks[i], i, chunkCount, progress.SpawnChild(progressPerChunk), token);
                SerializeAndSave(result, result.GetType().Name, i.ToString("0000"));

                var processed = (completed: i + 1, chunkCount);
                SerializeAndSave(processed, "FolderGroupCompleted");
            }

            progress.Report(100);
            _globals.AF.ProgressPane.Visible = false;
        }

        public async Task<IItemInfo[]> ToIItemInfoArray(
            OlFolderInfo[] folders,
            int batch,
            int totalBatches,
            ProgressTrackerPane progress,
            CancellationToken token)
        {
            var sw = await Task.Run(() => new SegmentStopWatch().Start());
            var mailTuples = QueryMailTuples(folders).ToArray();
            sw.LogDuration("QueryMailTuples");

            var count = mailTuples.Count();
            if (count == 0)
            {
                progress.Report(100);
                return default;
            }

            var cBag = await AsyncMultiTasker.AsyncMultiTaskChunker(
                mailTuples,
                async (mailTuple) => await ToIItemInfo(mailTuple, token),
                progress,
                $"Mining Mail Batch {batch} of {totalBatches} ",
                token);

            cBag.ForEach(x =>
            {
                sw.MergeDurations(x.Sw.Durations);
                x.Sw.Stop();
                x.Sw = null;
            });
            sw.WriteToLog(clear: true);

            progress.Report(100);

            return cBag.ToArray();

        }

        public async Task<IItemInfo> ToIItemInfo((MailItem Mail, OlFolderInfo FolderInfo) mailTuple, CancellationToken cancel)
        {
            var mailInfo = await Task.Run(async () => await MailItemHelper.FromMailItemAsync(
                mailTuple.Mail, _globals, cancel, true));

            mailInfo.FolderInfo = mailTuple.FolderInfo;

            await mailInfo.TokenizeAsync();
            var serializable = mailInfo.ToSerializableObject();
            serializable.Sw = mailInfo.Sw;
            serializable.Sw.LogDuration("ToSerializableObject");

            foreach (var attachment in serializable.AttachmentsInfo)
            {
                if (!attachment.IsImage)
                {
                    attachment.AttachmentData = null;
                }
            }
            return serializable;
        }

        public async Task Transform<Tin, Tout>(Func<Tin, Task<Tout>> transformer)
        {
            var (_, token, progress, _) = await ProgressPackage
                .CreateAsTuplePaneAsync(progressTrackerPane: _globals.AF.ProgressTracker).ConfigureAwait(false);
            _globals.AF.ProgressPane.Visible = true;
            var message = $"Transforming from {typeof(Tin).Name} to {typeof(Tout)}";
            progress.Report(0, message);

            var tInName = FolderConverter.SanitizeFilename(typeof(Tin).Name);
            var tOutName = FolderConverter.SanitizeFilename(typeof(Tout).Name);
            (_, var count) = await DeserializeAsync<(int, int)>("FolderGroupCompleted").ConfigureAwait(false);
            var completed = await DeserializeAsync<int>($"{tOutName}Completed").ConfigureAwait(false);
            var completedPerChunk = 100 / (double)count;

            for (int i = 0; i < count; i++)
            {
                if (i < completed)
                {
                    try
                    {
                        var objOut = await DeserializeAsync<Tout>($"{tOutName}_{i:0000}").ConfigureAwait(false);
                        progress.Report((int)((i + 1) * completedPerChunk), $"{message}. Validated {i + 1} of {count}");
                        continue;
                    }
                    catch (System.Exception e)
                    {
                        logger.Error($"Error deserializing {tOutName}_{i:0000}.json. Rebuilding ...\n{e.Message}", e);
                    }
                }

                Tin obj = await DeserializeAsync<Tin>($"{tInName}_{i:0000}").ConfigureAwait(false);
                Tout result = await transformer(obj);
                if (count == 1)
                    SerializeAndSave(result, tOutName);
                else
                    SerializeAndSave(result, $"{tOutName}_{i:0000}");
                SerializeAndSave(i + 1, $"{tOutName}Completed");
                progress.Report((int)((i + 1) * completedPerChunk), $"{message}. Transformed {i + 1} of {count}");
            }

            progress.Report(100);
            _globals.AF.ProgressPane.Visible = false;
        }

        public async Task<MinedMailInfo[]> ToMinedMail(IItemInfo[] items)
        {
            return await Task.Run(() => items.Select(item => new MinedMailInfo(item)).ToArray());
        }

        public async Task<MinedMailInfo[]> FilterExcluded(MinedMailInfo[] items)
        {
            return await Task.Run(() => items
                .Where(x => 
                    !_globals.TD.FilteredFolderScraping.ContainsKey(x.FolderInfo.RelativePath))
                .ToArray());
        }

        public async Task<MinedMailInfo[]> RemapFolderPaths(MinedMailInfo[] items)
        {
            await Task.Run(() =>
            {
                foreach (var item in items)
                {
                    if (_globals.TD.DictRemap.ContainsKey(item.FolderInfo.RelativePath))
                    {
                        item.FolderInfo.RelativePath = _globals.TD.DictRemap[item.FolderInfo.RelativePath];
                    }
                }
                //items.ForEach(x => x.FolderPath = _globals.TD.DictRemap.ContainsKey(x.FolderPath) ?
                //           _globals.TD.DictRemap[x.FolderPath] : x.FolderPath);
            });
            return items;
        }

        public async Task<MinedMailInfo> ToMinedMail(MailItem mailItem, CancellationToken cancel)
        {
            var mailInfo = await Task.Run(async () => await MailItemHelper.FromMailItemAsync(
                mailItem, _globals, cancel, true));

            await mailInfo.TokenizeAsync();

            var minedInfo = new MinedMailInfo(mailInfo);
            return minedInfo;
        }

        public async Task Transform<Tin, Tout>(Func<Tin[], Task<Tout>> transformer)
        {
            var (_, token, progress, _) = await ProgressPackage
                .CreateAsTuplePaneAsync(progressTrackerPane: _globals.AF.ProgressTracker).ConfigureAwait(false);
            _globals.AF.ProgressPane.Visible = true;
            var message = $"Transforming from {typeof(Tin).Name} to {typeof(Tout)}";
            progress.Report(0, message);

            var tInName = FolderConverter.SanitizeFilename(typeof(Tin).Name);
            var tOutName = FolderConverter.SanitizeFilename(typeof(Tout).Name);
            var (_, count) = Deserialize<(int, int)>("FolderGroupCompleted");
            List<Tin> list = [];
            for (int i = 0; i < count; i++)
            {
                Tin obj = await Task.Run(() => Deserialize<Tin>($"{tInName}_{i:0000}"));
                list.Add(obj);
            }
            Tout result = await transformer([.. list]);
            SerializeAndSave(result, tOutName);

            progress.Report(100);
            _globals.AF.ProgressPane.Visible = false;
        }

        public async Task<MinedMailInfo[]> Consolidate(MinedMailInfo[][] jagged)
        {
            var combined = await Task.Run(() => jagged.SelectMany(x => x).ToArray());
            combined = await Task.Run(() => FilterExcluded(combined));
            combined = await Task.Run(() => RemapFolderPaths(combined));
            return combined;
        }

        public async Task ToMinedMail(
            OlFolderInfo[] folders,
            int batch,
            int totalBatches,
            ProgressTracker progress,
            CancellationToken token)
        {

            var mailItems = QueryMailItems(folders.Select(x => x.OlFolder)).ToArray();

            var count = mailItems.Count();
            if (count == 0)
            {
                progress.Report(100);
                return;
            }

            var cBag = await AsyncMultiTasker.AsyncMultiTaskChunker(
                mailItems,
                async (mailItem) => await ToMinedMail(mailItem, token),
                progress,
                $"Mining Mail Batch {batch} of {totalBatches} ",
                token);

            progress.Report(100);

            var minedBag = new ScBag<MinedMailInfo>(cBag)
            {
                FolderPath = _globals.FS.FldrAppData,
                FileName = $"MinedMailInfo_{batch:000}.json"
            };

            minedBag.Serialize();
        }

        #endregion ETL - TRANSFORM For Data Mining

        #region ETL - LOAD To Data Mining

        public async Task<T> Load<T>(string fileName = "")
        {
            var tName = FolderConverter.SanitizeFilename(typeof(T).Name);
            if (fileName.IsNullOrEmpty()) { fileName = tName; }
            T result = await DeserializeAsync<T>(fileName);

            return result;
        }

        #endregion ETL - LOAD To Data Mining

        #endregion ETL - Extract, Transform, Load For Data Mining

        #region Build Classifiers

        public virtual async Task<ScoCollection<MinedMailInfo>> LoadStaging()
        {
            _mailInfoCollection = await Task.Run(
                () => new ScoCollection<MinedMailInfo>(
                    _globals.FS.Filenames.EmailInfoStagingFile,
                    _globals.FS.FldrPythonStaging));

            return _mailInfoCollection;
        }

        protected ScoCollection<MinedMailInfo> _mailInfoCollection;

        public virtual ConcurrentDictionary<string, DedicatedToken> GetDedicated(ConcurrentBag<MinedMailInfo> collection)
        {
            var dedicated = collection.Select(x =>
                x.Tokens.Select(y =>
                (Token: y, FolderPath: x.FolderInfo.RelativePath))
                .GroupBy(x => x.Token)
                .Select(grp => new DedicatedToken(
                    grp.Key,
                    grp.ToList().First().FolderPath,
                    grp.Count())))
                .SelectMany(x => x)
                .GroupBy(x => x.Token)
                .Where(grp => grp.Count() == 1)
                .SelectMany(x => x)
                .Select(x => new KeyValuePair<string, DedicatedToken>(x.Token, x));
            //.ToArray();
            var dict = new ConcurrentDictionary<string, DedicatedToken>(dedicated);

            return dict;
        }

        public virtual async Task<BayesianClassifierGroup> GetOrCreateClassifierGroupAsync(MinedMailInfo[] collection)
        {
            collection.ThrowIfNull();

            var group = await Task.Run(() => Deserialize<BayesianClassifierGroup>("StagingClassifierGroup"));
            if (group is null)
            {
                group = await CreateClassifierGroupAsync(collection);
                SerializeAndSave(group, "StagingClassifierGroup");
            }
            return group;
        }

        public virtual async Task<BayesianClassifierGroup> CreateClassifierGroupAsync(
            MinedMailInfo[] collection)
        {
            return await Task.Run(() =>
            {
                var group = new BayesianClassifierGroup
                {
                    TotalEmailCount = collection.Count(),
                    SharedTokenBase = new Corpus(
                        collection.SelectMany(x => x.Tokens).GroupAndCount())
                };
                return group;
            });
        }

        public virtual async Task BuildClassifierAsync(
            IGrouping<string, MinedMailInfo> group,
            BayesianClassifierGroup classifierGroup,
            CancellationToken cancel)
        {
            var matchFrequency = group.Select(minedMail => minedMail.Tokens)
                                      .SelectMany(x => x)
                                      .GroupAndCount();

            var matchCorpus = new Corpus(matchFrequency);
            var matchEmailCount = group.Count();
            await classifierGroup.RebuildClassifier(
                group.Key, matchFrequency, matchEmailCount, cancel);
        }

        
    public async Task<bool> BuildFolderClassifiersAsync(BayesianClassifierGroup classifierGroup, MinedMailInfo[] collection, ProgressPackage ppkg)
        {
            var groups = collection.GroupBy(x => x.FolderInfo.RelativePath);
            var sw = ppkg.StopWatch;

            bool success = false;
            try
            {
                await AsyncMultiTasker.AsyncMultiTaskChunker(groups, async (group) =>
                {
                    await BuildClassifierAsync(group, classifierGroup, ppkg.Cancel);
                }, ppkg.ProgressTrackerPane, "Building Classifiers", ppkg.Cancel);
                sw.LogDuration("Build Classifiers");
                sw.WriteToLog(clear: false);
                success = true;
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
            return success;
        }

        public async Task BuildFolderClassifiersAsync()
        {
            _globals.AF.Manager.Clear();

            var ppkg = await ProgressPackage //.CreateAsTupleAsync(screen: _globals.Ol.GetExplorerScreen());
                .CreateAsTuplePaneAsync(progressTrackerPane: _globals.AF.ProgressTracker).ConfigureAwait(false);
            var sw = ppkg.StopWatch;
            _globals.AF.ProgressPane.Visible = true;
            ppkg.ProgressTrackerPane.Report(0, "Building Folder Classifier -> Load Mined Mail Info");
                        
            var collection = await Load<MinedMailInfo[]>();
            collection.ThrowIfNullOrEmpty();
            sw.LogDuration("Load Staging");
            
            ppkg.ProgressTrackerPane.Report(10, "Building Folder Classifier -> Getting Folder Paths");

            var folderPaths = QueryOlFolderInfo(GetOlFolderTree()).Select(x => x.RelativePath).ToList();
            sw.LogDuration("Get Folder Paths");

            ppkg.ProgressTrackerPane.Report(20, "Building Folder Classifier -> Creating Classifier Group");
            var classifierGroup = await GetOrCreateClassifierGroupAsync(collection);
            sw.LogDuration("Get or Create Classifier Group and shared token base");
            sw.WriteToLog(clear: false);
            ppkg.ProgressTrackerPane.Report(30, "Building Folder Classifier -> Building Classifiers");

            var childPpkg = await new ProgressPackage()
                .InitializeAsync(ppkg.CancelSource, ppkg.Cancel, ppkg.ProgressTrackerPane.SpawnChild(), ppkg.StopWatch)
                .ConfigureAwait(false);
            
            if (await BuildFolderClassifiersAsync(classifierGroup, collection, childPpkg))
            {
                _globals.AF.Manager["Folder"] = classifierGroup;
                _globals.AF.Manager.Serialize();
            }
        }

        [Obsolete]
        public async Task BuildClassifierAsync1()
        {
            var collection = await LoadStaging();

            var tree = GetOlFolderTree();
            var folders = QueryOlFolders(tree).ToList();
            var folderPaths = folders.Select(x => x.FolderPath.Replace(_globals.Ol.ArchiveRootPath + "\\", "")).ToList();

            var allTokens = collection.SelectMany(x => x.Tokens).ToList();
            Corpus tokenBase = new();
            tokenBase.AddOrIncrementTokens(allTokens);

            var group = new ClassifierGroup();
            group.SharedTokenBase = tokenBase;

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource).Initialize();


            int completed = 0;
            folderPaths = folderPaths.Take(3).ToList();
            int count = folderPaths.Count();

            Stopwatch psw = new Stopwatch();
            psw.Start();
                        
            var tasks = folderPaths.Select(folderPath =>
            {
                return Task.Run(() =>
                {
                    var positiveTokens = collection.Where(x => x.FolderInfo.RelativePath == folderPath).SelectMany(x => x.Tokens).ToList();
                    var negativeTokens = collection.Where(x => x.FolderInfo.RelativePath != folderPath).SelectMany(x => x.Tokens).ToList();
                    if (positiveTokens.Count() > 0 && negativeTokens.Count() > 0)
                    {
                        group.ForceClassifierUpdate(folderPath, positiveTokens, negativeTokens);
                    }
                    Interlocked.Increment(ref completed);
                }, token);
            });

            bool success = false;
            Task entireTask = Task.WhenAll(tasks);

            try
            {
                while (await Task.WhenAny(entireTask, Task.Delay(1000)) != entireTask)
                {
                    progress.Report(
                        (int)(((double)completed / count) * 100),
                        GetProgressMessage(completed, count, psw));
                }
                success = true;
            }
            catch (OperationCanceledException)
            {
                logger.Debug("Classifier calculation canceled");
            }

            progress.Report(100);

            if (success)
            {
                //Disabled to prevent compiler error
                _globals.AF.Manager["Folder"] = null; // group;
                _globals.AF.Manager.Serialize();
            }
        }

        [Obsolete]
        public async Task BuildClassifierAsync2() 
        {
            var collection = await LoadStaging();
            
            var tree = GetOlFolderTree();
            var folders = QueryOlFolders(tree).ToList();
            var folderPaths = folders.Select(x => x.FolderPath.Replace(_globals.Ol.ArchiveRootPath + "\\", "")).ToList();

            var allTokens = collection.SelectMany(x => x.Tokens).ToList();
            Corpus tokenBase = new();
            tokenBase.AddOrIncrementTokens(allTokens);

            var group = new ClassifierGroup();
            group.SharedTokenBase = tokenBase;

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource).Initialize();

            //collection.ForEach(x => 
            //{
            //    x.FolderPath = folders.Find(y => y.Name == x.FolderPath).FolderPath.Replace(_globals.Ol.ArchiveRootPath + "\\", "");
            //});
            //collection.Serialize();

            //foreach (var folderPath in folderPaths)
            //{
            //    var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
            //    var negativeTokens = collection.Where(x => x.FolderPath != folderPath).SelectMany(x => x.Tokens).ToList();
            //    if (positiveTokens.Count() > 0 && negativeTokens.Count() > 0)
            //        group.ForceClassifierUpdate(folderPath, positiveTokens, negativeTokens);
            //}

            int completed = 0;
            int count = folderPaths.Count();

            Stopwatch psw = new Stopwatch();
            psw.Start();

            var tasks = folderPaths.Select(folderPath => 
            {
                return Task.Run(() =>
                {
                    var positiveTokens = collection.Where(x => x.FolderInfo.RelativePath == folderPath).SelectMany(x => x.Tokens).ToList();
                    var negativeTokens = collection.Where(x => x.FolderInfo.RelativePath != folderPath).SelectMany(x => x.Tokens).ToList();
                    if (positiveTokens.Count() > 0 && negativeTokens.Count() > 0)
                        group.ForceClassifierUpdate(folderPath, positiveTokens, negativeTokens);
                    Interlocked.Increment(ref completed);
                }, token);
            });

            bool success = false;
            Task entireTask = Task.WhenAll(tasks);

            try
            {
                while (await Task.WhenAny(entireTask, Task.Delay(1000)) != entireTask)
                {
                    progress.Report(
                        (int)(((double)completed / count) * 100),
                        GetProgressMessage(completed, count, psw));
                }
                success = true;
            }
            catch (OperationCanceledException)
            {
                logger.Debug("Classifier calculation canceled");
            }
            



            //System.Timers.Timer progressTimer = new System.Timers.Timer(500);
            //progressTimer.AutoReset = true;
            //progressTimer.Elapsed += (sender, e) =>
            //{
            //    progress.Report(
            //        (int)(((double)completed / count) * 100),
            //        GetReportMessage(completed, count, psw));
            //};
            //progressTimer.SynchronizingObject = progress.ProgressViewer;

            //try
            //{
            //    progressTimer.Start();
            //    await Task.WhenAll(tasks);
            //    success = true;
            //}
            //catch (OperationCanceledException)
            //{
            //    progressTimer.Stop();
            //    progressTimer.Dispose();
            //    logger.Debug("Classifier calculation canceled");
            //}

            progress.Report(100);

            if (success)
            {
                _globals.AF.Manager["Folder"] = null; // group;
                _globals.AF.Manager.Serialize();
            }
        }

        [Obsolete]
        public async Task BuildClassifierAsync3()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            //var progress = new ProgressTracker(tokenSource);

            _globals.AF.Manager.Clear();

            var sw = new SegmentStopWatch();
            sw.Start();

            var tmp = await LoadStaging();
            var collection = new ConcurrentBag<MinedMailInfo>(tmp);
            tmp = null;
            sw.LogDuration("Load Staging");

            var tree = GetOlFolderTree();
            var folders = QueryOlFolders(tree).ToList();
            var folderPaths = folders.Select(x => x.FolderPath.Replace(_globals.Ol.ArchiveRootPath + "\\", "")).ToList();
            sw.LogDuration("Get Folder Paths");

            var group = new ClassifierGroup();

            var allTokens = collection.SelectMany(x => x.Tokens).ToList();
            group.TotalTokenCount = allTokens.Count();
            sw.LogDuration("Capture all tokens and count");

            var dedicated = GetDedicated(collection);
            var dedicatedTokens = dedicated.Select(x => x.Key).ToArray();
            group.DedicatedTokens = dedicated;
            sw.LogDuration("Identify Dedicated Tokens");

            Corpus sharedTokenBase = new();
            sharedTokenBase.AddOrIncrementTokens(allTokens);
            dedicatedTokens.ForEach(x => sharedTokenBase.TokenFrequency.TryRemove(x, out _));
            group.SharedTokenBase = sharedTokenBase;
            sw.LogDuration("Create Shared Token Base");
            sw.WriteToLog(clear: false);


            int completed = 0;
            int count = folderPaths.Count();

            var processors = Math.Max(Environment.ProcessorCount - 2, 1);
            var chunkSize = (int)Math.Round((double)count / (double)processors, 0);
            var chunks = folderPaths.Chunk(chunkSize);

            Stopwatch psw = new Stopwatch();
            psw.Start();

            //var folderPath = folderPaths[0];
            //var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
            //var classifier = tokenBase.ToClassifier(folderPath, positiveTokens);

            var progress = new ProgressTracker(tokenSource).Initialize();

            var tasks = chunks.Select(
                chunk => Task.Run(async () =>
                {
                    foreach (var folderPath in chunk)
                    {
                        var positiveTokens = collection.Where(x => x.FolderInfo.RelativePath == folderPath).SelectMany(x => x.Tokens).ToList();
                        group.Classifiers[folderPath] = await group.ToClassifierAsync(folderPath, positiveTokens, token);
                        Interlocked.Increment(ref completed);
                        progress.Report(
                            (int)(((double)completed / count) * 100),
                            GetProgressMessage(completed, count, psw));
                    }
                }, token));

            //var tasks = chunks.Select(
            //    chunk => Task.Run(async () => await
            //        chunk.ToAsyncEnumerable()
            //        .ForEachAsync(async (folderPath) => 
            //        {
            //            var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
            //            group.Classifiers[folderPath] = await group.ToClassifierAsync(folderPath, positiveTokens, token);
            //            Interlocked.Increment(ref completed);
            //            progress.Report(
            //                (int)(((double)completed / count) * 100),
            //                GetReportMessage(completed, count, psw));
            //        }), 
            //        token));

            //var tasks = folderPaths.Select(folderPath =>
            //{
            //    return Task.Run(async () =>
            //    {
            //        var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
            //        group.Classifiers[folderPath] = await group.ToClassifierAsync(folderPath, positiveTokens, token);
            //        Interlocked.Increment(ref completed);
            //        progress.Report(
            //            (int)(((double)completed / count) * 100),
            //            GetReportMessage(completed, count, psw));
            //    }, token);
            //});

            bool success = false;

            try
            {
                await Task.WhenAll(tasks);
                success = true;
            }
            catch (OperationCanceledException)
            {
                logger.Debug("Classifier calculation canceled");
            }

            progress.Report(100);

            if (success)
            {
                // Set to null to prevent compiler error with type change
                _globals.AF.Manager["Folder"] = null; //group;
                _globals.AF.Manager.ActivateLocalDisk();
                _globals.AF.Manager.Serialize();
            }
        }

        #endregion Build Classifiers

        #region Test Classifiers
                
        #endregion Test Classifiers

        #region Testing Sizing and Serialization Methods

        internal virtual T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            if (File.Exists(disk.FilePath))
            {
                var item = JsonConvert.DeserializeObject<T>(
                    File.ReadAllText(disk.FilePath), jsonSettings);
                return item;
            }
            else { return default(T); }
        }

        internal async virtual Task<T> DeserializeAsync<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            if (File.Exists(disk.FilePath))
            {
                string fileText = null;
                using (var reader = File.OpenText(disk.FilePath))
                {
                    fileText = await reader.ReadToEndAsync();
                }

                var item = JsonConvert.DeserializeObject<T>(fileText, jsonSettings);
                return item;
            }
            else { return default(T); }
        }

        internal virtual void SerializeAndSave<T>(T obj, string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            SerializeAndSave(obj, serializer, disk);
        }

        internal virtual void SerializeAndSave<T>(T obj, JsonSerializer serializer, FilePathHelper disk)
        {
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, obj);
                disk.FileName = null;
            }
        }

        internal virtual void SerializeFsSave<T>(T obj, string objName, JsonSerializer serializer, FilePathHelper disk)
        {
            disk.FileName = $"{objName}_Example.json";
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, obj);
                sw.Close();
                disk.FileName = null;
            }
        }

        internal virtual void LogSizeComparison(string m1, long s1, string m2, long s2, string objectName)
        {
            var jagged = new string[][]
            {
                [m1, $"{s1:N0}"],
                [m2, $"{s2:N0}"],
            };

            var text = jagged.ToFormattedText(
                ["Method", "Size"],
                [Enums.Justification.Left, Enums.Justification.Right],
                $"{objectName} Size");

            logger.Debug($"Object size calculations:\n{text}");
        }

        public virtual void SerializeActiveItem()
        {
            var (mailItem, s1) = TryLoadObjectAndGetMemorySize(() => _globals.Ol.App.ActiveExplorer().Selection[1]);
            var s2 = 0; //ObjectSize(mailItem);

            LogSizeComparison("GC Allocation", s1, "Serialization", s2, "MailItem");

            if (mailItem is not null) { SerializeMailInfo(mailItem); }

        }

        internal virtual void SerializeMailInfo(MailItem mailItem)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);

            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;

            SerializeFsSave(mailItem, "MailItem", serializer, disk);


            var (mailInfo, sizeMailInfo1) = TryLoadObjectAndGetMemorySize(() =>
                new MailItemHelper(mailItem).LoadAll(_globals, _globals.Ol.ArchiveRoot,true));
            var sizeMailInfo2 = 0; // ObjectSize(mailInfo);
            LogSizeComparison("GC Allocation", sizeMailInfo1, "Serialization", sizeMailInfo2, "MailItemInfo");
            SerializeFsSave(mailInfo, "MailItemInfo", serializer, disk);



            var (minedInfo, sizeMinedInfo1) = TryLoadObjectAndGetMemorySize(() =>
                new MinedMailInfo(mailInfo));
            var sizeMinedInfo2 = 0; // ObjectSize(minedInfo);
            LogSizeComparison("GC Allocation", sizeMinedInfo1, "Serialization", sizeMinedInfo2, "MinedMailInfo");
            SerializeFsSave(minedInfo, "MinedMailInfo", serializer, disk);

        }

        internal virtual (T Object, long Size) TryLoadObjectAndGetMemorySize<T>(Func<T> loader, int copiesToLoad = 1)
        {
            loader.ThrowIfNull();
            if (copiesToLoad < 1) { throw new ArgumentOutOfRangeException(nameof(copiesToLoad), $"{nameof(copiesToLoad)} must be greater than 0"); }
            var start = GC.GetTotalMemory(true);
            long end = 0;
            
            T obj = loader();
            
            if (copiesToLoad > 1)
            {
                GCHandle[] objects = new GCHandle[copiesToLoad];
                try
                {
                    for (int i = 1; i < copiesToLoad; i++)
                    {
                        obj = loader();
                        var handle = GCHandle.Alloc(obj);
                        objects[i] = handle;
                    }
                    end = GC.GetTotalMemory(true);

                }
                catch (System.Exception e)
                {
                    logger.Error($"Error loading object of type {typeof(T).Name}\n{e.Message}", e);
                    return (default, 0);
                }
                finally 
                { 
                    for (int i = 1; i < copiesToLoad; i++)
                    {
                        if (objects[i].IsAllocated) { objects[i].Free(); }
                    }
                }
            }
            var size = (end - start) / copiesToLoad;

            return (obj, size);
        }

        internal virtual JsonSerializer GetSerializer()
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            return serializer;
        }

        public virtual void SerializeChunk(MinedMailInfo[] chunk, JsonSerializer serializer, FilePathHelper disk, int i) 
        { 
            disk.FileName = $"MinedMailInfo_{i:000}.json";
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, chunk);
                sw.Close();
                disk.FileName = null;
            }
            disk.FileName = null;
        }

        public async virtual Task<bool> ValidateJson<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            try
            {
                T obj = await DeserializeAsync<T>(fileNameSeed, fileNameSuffix);
                if (obj != null)
                    return true;
                else
                    return false;
            }
            catch (System.Exception e)
            {
                if (fileNameSuffix.IsNullOrEmpty())
                    logger.Error($"Error deserializing {typeof(T).Name}.json. \n{e.Message}", e);
                else
                    logger.Error($"Error deserializing {typeof(T).Name}_{fileNameSuffix}.json. \n{e.Message}", e);
                return false;
            }
            
        }

        #endregion Testing Sizing and Serialization Methods

        #region Helper Methods

        private string GetProgressMessage(int complete, int count, Stopwatch sw)
        {
            double seconds = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"Completed {complete} of {count} ({seconds:N2} spm) " +
                $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        /// <summary>
        /// If Outlook is not in offline mode, save the state and toggle it to offline mode
        /// </summary>
        /// <param name="offline"></param>
        /// <returns></returns>
        private async Task<bool> ToggleOfflineMode(bool offline)
        {
            if (!offline)
            {
                var commandBars = _globals.Ol.App.ActiveExplorer().CommandBars;
                if (!offline) { commandBars.ExecuteMso("ToggleOnline"); }
                await Task.Delay(5);
            }
            return offline;
        }

        #endregion Helper Methods

    }

}
