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
        private SegmentStopWatch _sw;

        #endregion Constructors and private fields

        #region Scrape Emails

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
            //var ary = folders.Select(x=>x.FolderPath).ToArray();
            return folders;
        }

        //internal IEnumerable<MAPIFolder> QueryOlFoldersAsync(OlFolderTree tree)
        //{
        //    var folders = tree.Roots
        //                      .SelectMany(root => root
        //                      .FlattenIf(node => !node.Selected))
        //                      .Select(x => x.OlFolder);
        //    return folders;
        //}

        internal IEnumerable<MailItem> QueryMailItems(IEnumerable<MAPIFolder> folders)
        {
            var mailItems = folders
                .SelectMany(folder => folder
                            .Items.Cast<object>()
                            .Where(obj => obj is MailItem)
                            .Cast<MailItem>());
            return mailItems;
        }

        internal List<MailItem> LinqToSimpleEmailList(
            IEnumerable<MAPIFolder> folders, 
            IEnumerable<MailItem> mailItems, 
            ProgressTracker progress)
        {
            var prelimCount = folders.Select(folder => folder.Items.Count).Sum();
            _sw.LogDuration("Get Preliminary Count");

            var mailList = mailItems.ToList(prelimCount, progress);
            _sw.LogDuration("Load MailItems");

            return mailList;
        }

        //public async Task<List<MailItem>> ScrapeEmails(CancellationTokenSource tokenSource)
        public async Task<IEnumerable<MailItem>> ScrapeEmails(CancellationTokenSource tokenSource, ProgressTracker progress)
        {
            //List<MailItem> mailItems = null;
            IEnumerable<MailItem> mailItemsQuery = null;

            await Task.Factory.StartNew(() =>
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
                //mailItems = LinqToSimpleEmailList(folders, mailItemsQuery, progress);
                //_sw.LogDuration(nameof(LinqToSimpleEmailList));
                _sw.WriteToLog(clear: false);
            }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            //progress.Report(100);

            //return mailItems;
            return mailItemsQuery;
        }


        #endregion Aquire Emails

        public async Task MineEmails()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource);

            _sw = new SegmentStopWatch();
            _sw.Start();

            var mailItems = await ScrapeEmails(tokenSource, progress);

            
            progress = new ProgressTracker(tokenSource);
            var count = mailItems.Count();

            //var mailInfo = await mailItems.ToAsyncEnumerable().SelectAwait(async x => await MailItemInfo
            //                        .FromMailItemAsync(x, _globals.Ol.EmailPrefixToStrip, token, true))
            //                        .WithProgressReporting(count, (x) => progress.Report(x)).ToListAsync();

            int complete = 0;
            progress.Report(0, $"Creating MailItem Info {complete:N0} of {count:N0}");

            var psw = new Stopwatch();
            psw.Start();

            //var mailTasks = mailItems.Select(x => Task.Factory.StartNew(() =>
            //{
            //    var mailInfo = new MailItemInfo(x);
            //    mailInfo.LoadAll(_globals.Ol.EmailPrefixToStrip);
            //    mailInfo.LoadTokens();
            //    Interlocked.Increment(ref complete);
            //    return mailInfo;
            //},token,TaskCreationOptions.LongRunning, TaskScheduler.Default));
            
            ScoCollection<MinedMailInfo> mailInfoCollection = [];
            mailInfoCollection.FilePath = "C:\\Temp\\emailInfo.json";

            
            int chunkNum = 7;
            int chunkSize = count / chunkNum;
            List<Task> tasks = [];
            
            var chunks = mailItems.Chunk(chunkSize);

            foreach (var c in chunks)
            //for (int i = 0; i < chunkNum; i++)
            {
                //await Task.Factory.StartNew(() =>
                tasks.Add(Task.Factory.StartNew(() => 
                {
                    foreach (var mailItem in c)
                    //var endIter = i == (chunkNum - 1) ? count : chunkSize * (chunkNum + 1);
                    //for (int j = chunkNum*chunkSize; j < endIter; j++)
                    {
                        //var mailItem = mailItems.ElementAt(j);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            var mailInfo = new MailItemInfo(mailItem);
                            token.ThrowIfCancellationRequested();
                            mailInfo.LoadAll(_globals.Ol.EmailPrefixToStrip);
                            token.ThrowIfCancellationRequested();
                            mailInfo.LoadTokens();
                            var minedInfo = new MinedMailInfo(mailInfo);
                            var obj = JsonConvert.SerializeObject(minedInfo);
                            mailInfoCollection.Add(minedInfo);
                            Interlocked.Increment(ref complete);
                            //progress.Report((int)(((double)complete / (double)count) * 100), $"Creating MailItem Info {complete} of {count}");
                        }
                        catch (OperationCanceledException)
                        {
                            logger.Debug("Request to cancel task was received");
                            break;
                        }
                        catch (System.Exception)
                        {
                            logger.Debug($"Skipping MailItem from {mailItem.SentOn} in folder {((Folder)mailItem.Parent).FolderPath}");
                        }
                    }
                },
                token, TaskCreationOptions.None, TaskScheduler.Default));
            }

            //await Task.WhenAll(tasks);
            using (new System.Threading.Timer(_ => progress.Report(
                (int)(((double)complete / count) * 100),
                GetReportMessage(complete, count, psw)),
                //$"Creating MailItem Info {complete} of {count} ({complete > 0 ? psw.Elapsed.TotalSeconds/complete}"),
                null, 0, 1000))
            {
                try
                {
                    await Task.WhenAll(tasks);
                    mailInfoCollection.Serialize();
                }
                catch (TaskCanceledException)
                {
                    logger.Debug("Request to cancel task was received");
                }
                
            }

            //MailItemInfo[] result = [];
            //jagged.ForEach(x => result = result.Concat(x).ToArray());
            //var minedInfo = result.Select(x => new MinedMailInfo(x)).ToList();
            //ScoCollection<MinedMailInfo> mailInfoCollection = new ScoCollection<MinedMailInfo>(minedInfo);

            

            progress.Report(100);
            
                                    
        }

        private string GetReportMessage(int complete, int count, Stopwatch psw)
        {
            double seconds = complete > 0 ? psw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"Creating Info {complete} of {count} ({seconds:N2} spm) ({ts:c} remaining)";
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

        public async Task BuildClassifierAsync()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource);
            
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
                        
            var allTokens = collection.SelectMany(x => x.Tokens).ToList();
            Corpus tokenBase = new();
            tokenBase.AddOrIncrementTokens(allTokens);
            sw.LogDuration("Create Token Base");
            sw.WriteToLog(clear: false);

            var group = new ClassifierGroup();
            group.TokenBase = tokenBase;

            int completed = 0;
            //folderPaths = folderPaths.Take(3).ToList();
            int count = folderPaths.Count();

            Stopwatch psw = new Stopwatch();
            psw.Start();

            //var folderPath = folderPaths[0];
            //var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
            //var classifier = tokenBase.ToClassifier(folderPath, positiveTokens);

            var tasks = folderPaths.Select(folderPath =>
            {
                return Task.Run(async () =>
                {
                    var positiveTokens = collection.Where(x => x.FolderPath == folderPath).SelectMany(x => x.Tokens).ToList();
                    group.Classifiers[folderPath] = await tokenBase.ToClassifierAsync(folderPath, positiveTokens, token);
                    Interlocked.Increment(ref completed);
                    progress.Report(
                        (int)(((double)completed / count) * 100),
                        GetReportMessage(completed, count, psw));
                }, token);
            });

            bool success = false;
            Task entireTask = Task.WhenAll(tasks);

            try
            {
                await entireTask;
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
            group.TokenBase = tokenBase;

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource);


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
            group.TokenBase = tokenBase;

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource);

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
