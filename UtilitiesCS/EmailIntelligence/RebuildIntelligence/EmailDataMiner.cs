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

            // Convert to MailItemInfo
            var mailInfoTasks = mailItems.Select(x => MailItemInfo.FromMailItemAsync(x, _globals.Ol.EmailPrefixToStrip, token, true));
            var mailInfoItems = (await Task.WhenAll(mailInfoTasks)).ToList();
            
                        
        }

    }
}
