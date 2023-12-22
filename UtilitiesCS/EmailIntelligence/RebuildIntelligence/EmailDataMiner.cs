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

namespace UtilitiesCS.EmailIntelligence.RebuildIntelligence
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

        internal IEnumerable<MAPIFolder> QueryOlFolders(OlFolderTree tree)
        {
            var folders = tree.Roots
                              .SelectMany(root => root
                              .FlattenIf(node => !node.Selected))
                              .Select(x => x.OlFolder);
            var ary = folders.Select(x=>x.FolderPath).ToArray();
            return folders;
        }

        internal IEnumerable<MAPIFolder> QueryOlFoldersAsync(OlFolderTree tree)
        {
            var folders = tree.Roots
                              .SelectMany(root => root
                              .FlattenIf(node => !node.Selected))
                              .Select(x => x.OlFolder);
            return folders;
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
                
        public async Task<List<MailItem>> ScrapeEmails(CancellationTokenSource tokenSource)
        {
            var progress = new ProgressTracker(tokenSource);
            List<MailItem> mailItems = null;

            await Task.Factory.StartNew(() =>
            {
                // Query List of Outlook Folders if they are not on the skip list
                progress.Report(0, "Building Outlook Folder Tree");
                var tree = GetOlFolderTree();
                _sw.LogDuration(nameof(GetOlFolderTree));
                progress.Increment(2);

                var folders = QueryOlFolders(tree);
                _sw.LogDuration(nameof(QueryOlFolders));

                // Query MailItems from these folders
                var mailItemsQuery = QueryMailItems(folders);
                _sw.LogDuration(nameof(QueryMailItems));

                // Load to memory
                mailItems = LinqToSimpleEmailList(folders, mailItemsQuery, progress);
                _sw.LogDuration(nameof(LinqToSimpleEmailList));

            }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            progress.Report(100);

            return mailItems;
        }


        #endregion Aquire Emails

        public async Task MineEmails()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            _sw = new SegmentStopWatch();
            _sw.Start();

            var mailItems = await ScrapeEmails(tokenSource);

            var progress = new ProgressTracker(tokenSource);
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

            int chunkNum = 100;
            int chunkSize = mailItems.Count() / chunkNum;
            int lastChunk = mailItems.Count() - (chunkSize * (chunkNum -1));
            List<Task> tasks = [];
            
            for (int i = 0; i < chunkSize; i++)
            {
                tasks.Add(Task.Factory.StartNew(() => 
                {
                    var chunk = (i == chunkSize) ? lastChunk : chunkSize;
                    var chunkEnd = (i == chunkSize) ? mailItems.Count() : chunkSize * (i + 1);
                    
                    for (int j = chunkSize * i; j < chunkEnd; j++)
                    {
                        try
                        {
                            var mailInfo = new MailItemInfo(mailItems[j]);
                            mailInfo.LoadAll(_globals.Ol.EmailPrefixToStrip);
                            mailInfo.LoadTokens();
                            var minedInfo = new MinedMailInfo(mailInfo);
                            var obj = JsonConvert.SerializeObject(minedInfo);
                            mailInfoCollection.Add(minedInfo);
                            Interlocked.Increment(ref complete);
                        }
                        catch (System.Exception)
                        {
                            logger.Debug($"Skipping MailItem from {mailItems[j].SentOn} in folder {((Folder)mailItems[j].Parent).FolderPath}");                            
                        }                        
                    }
                }, 
                token, TaskCreationOptions.LongRunning, TaskScheduler.Default));
            }
            
            //var chunkTasks = Enumerable.Range(0, 100).Select(i => Task.Factory.StartNew(() =>
            //{
            //    var chunk = (i == 99) ? lastChunk : chunkSize;
            //    var mailInfoEnum = mailItems.Skip(i * chunkSize)
            //                            .Take(chunk)
            //                            .Select(x => 
            //                            { 
            //                                var mailInfo = new MailItemInfo(x);
            //                                mailInfo.LoadAll(_globals.Ol.EmailPrefixToStrip);
            //                                mailInfo.LoadTokens();
            //                                Interlocked.Increment(ref complete);
            //                                return mailInfo; 
            //                            }).ToArray();
            //    return mailInfoEnum;
            //}, token, TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray();

            //$"Creating MailItem Info {complete.ToString("N0")} of {count:N0} ({(complete > 0 ? psw.Elapsed.TotalSeconds / complete : 0).ToString("N0")} seconds per mail)"),

            
            //MailItemInfo[][] jagged;
            
            using (new System.Threading.Timer(_ => progress.Report(
                (int)(((double)complete / count) * 100), 
                $"Creating MailItem Info {complete} of {count}"), 
                null, 0, 500))
            {
                //jagged = await Task.WhenAll(chunkTasks);
                await Task.WhenAll(tasks);
            }

            //MailItemInfo[] result = [];
            //jagged.ForEach(x => result = result.Concat(x).ToArray());
            //var minedInfo = result.Select(x => new MinedMailInfo(x)).ToList();
            //ScoCollection<MinedMailInfo> mailInfoCollection = new ScoCollection<MinedMailInfo>(minedInfo);
            
            mailInfoCollection.Serialize();

            progress.Report(100);
            
                                    
        }

    }

    public class MinedMailInfo 
    {
        public MinedMailInfo(MailItemInfo info) 
        {
            Categories = info.Item.Categories;
            Tokens = info.Tokens.ToArray();
            FolderPath = info.Folder;
            ToRecipients = info.ToRecipients.ToArray();
            CcRecipients = info.CcRecipients.ToArray();
            Sender = info.Sender;
            ConversationId = info.Item.ConversationID;
        }

        private string _categories;
        public string Categories { get => _categories; set => _categories = value; }
        
        private string[] _tokens;
        public string[] Tokens { get => _tokens; set => _tokens = value; }
        
        public string _folderPath;
        public string FolderPath { get => _folderPath; set => _folderPath = value; }
        
        private RecipientInfo[] _toRecipients;
        public RecipientInfo[] ToRecipients { get => _toRecipients; set => _toRecipients = value; }
        
        private RecipientInfo[] _ccRecipients;
        public RecipientInfo[] CcRecipients { get => _ccRecipients; set => _ccRecipients = value; }
        
        private RecipientInfo _sender;
        public RecipientInfo Sender { get => _sender; set => _sender = value; }
        
        private string _conversationId;
        public string ConversationId { get => _conversationId; set => _conversationId = value; }
    }
}
