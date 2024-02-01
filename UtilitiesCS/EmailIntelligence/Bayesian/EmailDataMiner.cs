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
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;
using UtilitiesCS.Properties;
using UtilitiesCS.Extensions;
using System.Runtime.InteropServices;

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

        #region ETL Mined Emails

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
            var (tokenSource, cancel, progress, sw) = await ProgressPackage.CreateAsTupleAsync(
                screen: _globals.Ol.GetExplorerScreen());
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

        internal struct FolderStruct(OlFolderInfo folderInfo, long cumulativeSize, long chunkNumber, int cumulativeCount)
        {
            public OlFolderInfo FolderInfo { get; set; } = folderInfo;
            public long CumulativeSize { get; set; } = cumulativeSize;
            public long ChunkNumber { get; set; } = chunkNumber;
            public int CumulativeCount { get; set; } = cumulativeCount;
        }

        internal FolderStruct[] AddRollingMeasures(long availableRAM, OlFolderInfo[] folders)
        {
            var folderRecords = folders
                .Scan(new FolderStruct(default(OlFolderInfo), 0L, 0L, 0),
                (current, next) => new FolderStruct
                {
                    FolderInfo = next,
                    CumulativeSize = current.CumulativeSize + (next.FolderSize),
                    ChunkNumber = (current.CumulativeSize + (next.FolderSize) + availableRAM - 1L) / availableRAM,
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

        internal async Task<bool> ResolveMapiFolderHandles(OlFolderInfo[] folders)
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

                    folder.OlRoot = handle.OlRoot;
                    folder.OlFolder = handle.OlFolder;
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
                folders = Deserialize<OlFolderInfo[]>("StagingFolderRecords", "");
                reload = !await ResolveMapiFolderHandles(folders);
            }
            if (reload || folders is null)
            {
                folders = await GetInitializedFolderInfo();
                SerializeAndSave(folders, "StagingFolderRecords", "");
            }

            var availableRam = Convert.ToInt64(ComputerInfo.AvailablePhysicalMemory);
            var maxChunkSize = Math.Min(availableRam, MaxObjectSize);
            logger.Debug($"Available RAM {availableRam / (double)1000000000:N2} GB");
            logger.Debug($"Max Obj Size  {MaxObjectSize / (double)1000000000:N2} GB");
            logger.Debug($"Min(RAM, Max) {maxChunkSize / (double)1000000000:N2} GB");


            var folderRecords = AddRollingMeasures(maxChunkSize, folders);

            var folderChunks = folderRecords
                .GroupBy(x => x.ChunkNumber)
                .Select(group => group
                .Select(x => x.FolderInfo)
                .ToArray())
                .ToArray();

            var last = folderRecords.Last();
            var (totalSize, totalCount) = (last.CumulativeSize, last.CumulativeCount);

            LogFolderChunkMetrics(maxChunkSize, folderChunks, totalSize, totalCount);

            return folderChunks;
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

        public async Task<ScBag<MinedMailInfo>> LoadAndConsolidateMinedMail()
        {
            var chunks = Deserialize<int>("FolderChunkCompleted", "");
            ScBag<MinedMailInfo> minedBag = [];

            for (int i = 0; i < chunks; i++)
            {
                var bag = await Task.Run(() => Deserialize<ScBag<MinedMailInfo>>($"MinedMailInfo_{i:000}", ""));
                ((IList<MinedMailInfo>)minedBag).AddRange(bag);
            }
            return minedBag;
        }

        public async Task TransformToMinedMail(OlFolderInfo[][] folderChunks)
        {
            var (tokenSource, token, progress, psw) = await ProgressPackage.CreateAsTupleAsync(screen: _globals.Ol.GetExplorerScreen());
            var chunkCount = folderChunks.Count();
            var progressPerChunk = 100 / (double)chunkCount;

            var latestChunk = Deserialize<int>("FolderChunkCompleted", "");
            progress.Report(latestChunk * progressPerChunk);

            for (int i = latestChunk; i < chunkCount; i++)
            {
                await MineFolderGroup(
                    folderChunks[i], i, chunkCount, progress.SpawnChild(progressPerChunk), token);

                SerializeAndSave(i + 1, "FolderChunkCompleted", "");
            }

            progress.Report(100);
        }

        public async Task<MinedMailInfo> ProcessMailItem(MailItem mailItem, CancellationToken cancel)
        {
            var mailInfo = await Task.Run(async () => await MailItemHelper.FromMailItemAsync(
                mailItem, _globals.Ol.EmailPrefixToStrip, cancel, true));

            await mailInfo.TokenizeAsync();

            var minedInfo = new MinedMailInfo(mailInfo);
            return minedInfo;
        }

        public async Task MineFolderGroup(
            OlFolderInfo[] olFolderInfos,
            int batch,
            int totalBatches,
            ProgressTracker progress,
            CancellationToken token)
        {
            var mailItems = QueryMailItems(olFolderInfos.Select(x => x.OlFolder)).ToArray();

            var count = mailItems.Count();
            if (count == 0)
            {
                progress.Report(100);
                return;
            }

            var cBag = await AsyncMultiTasker.AsyncMultiTaskChunker(
                mailItems,
                async (mailItem) => await ProcessMailItem(mailItem, token),
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

        public async Task<ScBag<MinedMailInfo>> MineEmails()
        {
            if (SynchronizationContext.Current is null) { SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext()); }

            var folderChunks = await Task.Run(async () => await ExtractOlFolderChunks());
            await TransformToMinedMail(folderChunks);
            return await LoadAndConsolidateMinedMail();
        }

        #endregion ETL Mined Emails

        #region Testing Sizing and Serialization Methods

        private T Deserialize<T>(string fileNameSeed, string fileNameSuffix)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
            disk.FileName = $"{fileNameSeed}_{fileNameSuffix}.json";
            if (File.Exists(disk.FilePath))
            {
                var item = JsonConvert.DeserializeObject<T>(
                    File.ReadAllText(disk.FilePath), jsonSettings);
                return item;
            }
            else { return default(T); }
        }

        private void SerializeAndSave<T>(T obj, string fileNameSeed, string fileNameSuffix)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            var disk = new FilePathHelper();
            disk.FolderPath = _globals.FS.FldrAppData;
            disk.FileName = $"{fileNameSeed}_{fileNameSuffix}.json";
            SerializeAndSave(obj, serializer, disk);
        }

        private void SerializeAndSave<T>(T obj, JsonSerializer serializer, FilePathHelper disk)
        {
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, obj);
                disk.FileName = null;
            }
        }

        private void SerializeFsSave<T>(T obj, string objName, JsonSerializer serializer, FilePathHelper disk)
        {
            disk.FileName = $"{objName}_Example.json";
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, obj);
                sw.Close();
                disk.FileName = null;
            }
        }

        private void LogSizeComparison(string m1, long s1, string m2, long s2, string objectName)
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

        public void SerializeActiveItem()
        {
            var (mailItem, s1) = TryLoadObjectAndGetMemorySize(() => _globals.Ol.App.ActiveExplorer().Selection[1]);
            var s2 = 0; //ObjectSize(mailItem);

            LogSizeComparison("GC Allocation", s1, "Serialization", s2, "MailItem");

            if (mailItem is not null) { SerializeMailInfo(mailItem); }

        }

        public void SerializeMailInfo(MailItem mailItem)
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
                new MailItemHelper(mailItem).LoadAll(_globals.Ol.EmailPrefixToStrip, true));
            var sizeMailInfo2 = 0; // ObjectSize(mailInfo);
            LogSizeComparison("GC Allocation", sizeMailInfo1, "Serialization", sizeMailInfo2, "MailItemInfo");
            SerializeFsSave(mailInfo, "MailItemInfo", serializer, disk);



            var (minedInfo, sizeMinedInfo1) = TryLoadObjectAndGetMemorySize(() =>
                new MinedMailInfo(mailInfo));
            var sizeMinedInfo2 = 0; // ObjectSize(minedInfo);
            LogSizeComparison("GC Allocation", sizeMinedInfo1, "Serialization", sizeMinedInfo2, "MinedMailInfo");
            SerializeFsSave(minedInfo, "MinedMailInfo", serializer, disk);

        }

        private (T Object, long Size) TryLoadObjectAndGetMemorySize<T>(Func<T> loader, int copiesToLoad = 1)
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

        internal JsonSerializer GetSerializer()
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            return serializer;
        }

        public void SerializeChunk(MinedMailInfo[] chunk, JsonSerializer serializer, FilePathHelper disk, int i) 
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
                
        #endregion Testing Sizing and Serialization Methods
                
        private string GetReportMessage(int complete, int count, Stopwatch sw)
        {
            double seconds = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"Completed {complete} of {count} ({seconds:N2} spm) " +
                $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        public async Task<ScoCollection<MinedMailInfo>> LoadStaging() 
        {
            _mailInfoCollection = await Task.Run(
                () => new ScoCollection<MinedMailInfo>(
                    _globals.FS.Filenames.EmailInfoStagingFile,
                    _globals.FS.FldrPythonStaging));
            
            return _mailInfoCollection;
        }

        private ScoCollection<MinedMailInfo> _mailInfoCollection;
        
        public ConcurrentDictionary<string, DedicatedToken> GetDedicated(ConcurrentBag<MinedMailInfo> collection)
        {
            var dedicated = collection.Select(x =>
                x.Tokens.Select(y =>
                (Token: y, FolderPath: x.FolderPath))
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
        
        public async Task BuildClassifierAsync()
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
                        var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
                        group.Classifiers[folderPath] = await group.ToClassifierAsync(folderPath, positiveTokens, token);
                        Interlocked.Increment(ref completed);
                        progress.Report(
                            (int)(((double)completed / count) * 100),
                            GetReportMessage(completed, count, psw));
                    }
                },token));

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
                _globals.AF.Manager["Folder"] = group;
                _globals.AF.Manager.ActivateLocalDisk();
                _globals.AF.Manager.Serialize();
            }
        }

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
                    var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
                    var negativeTokens = collection.Where(x => x.FolderPath != folderPath).SelectMany(x => x.Tokens).ToList();
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
                        GetReportMessage(completed, count, psw));
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
                _globals.AF.Manager["Folder"] = group;
                _globals.AF.Manager.Serialize();
            }




        }

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
                    var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
                    var negativeTokens = collection.Where(x => x.FolderPath != folderPath).SelectMany(x => x.Tokens).ToList();
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
                        GetReportMessage(completed, count, psw));
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
                _globals.AF.Manager["Folder"] = group;
                _globals.AF.Manager.Serialize();
            }

            
            
            
        }

    }

}
