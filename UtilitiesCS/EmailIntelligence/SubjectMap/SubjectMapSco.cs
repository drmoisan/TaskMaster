using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.SubjectMap;
using UtilitiesCS.HelperClasses;
using static Deedle.FrameBuilder;



namespace UtilitiesCS
{
    /// <summary>
    /// A serializable list of ISubjectMapEntry. See <see cref="ISubjectMapEntry"/>.
    /// </summary>
    public class SubjectMapSco : ScoCollection<SubjectMapEntry>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SubjectMapSco(ISerializableList<string> commonWords) : base() { _commonWords = commonWords; }

        public SubjectMapSco(List<SubjectMapEntry> listOfT,
                            ISerializableList<string> commonWords) : base(listOfT) { _commonWords = commonWords; }

        public SubjectMapSco(IEnumerable<SubjectMapEntry> IEnumerableOfT,
                            ISerializableList<string> commonWords) : base(IEnumerableOfT) { _commonWords = commonWords; }

        public SubjectMapSco(string filename,
                            string folderpath,
                            ISerializableList<string> commonWords) : base(filename, folderpath) { _commonWords = commonWords; }

        /// <summary>
        /// Constructor that takes the filename and folderpath for the primary file as well as a backup loader and backup location. 
        /// </summary>
        /// <param name="filename">Filename of the primary json serialized object</param>
        /// <param name="folderpath">Location of the serialized object</param>
        /// <param name="backupLoader">Delegate function <see cref="CSVLoader{T}"/> that 
        /// returns an <seealso cref="IList{T}"/> where T : <see cref="ISubjectMapEntry"/></param>
        /// <param name="backupFilepath">Fully qualified filepath to backup file</param>
        /// <param name="askUserOnError">Determines whether to ask the user for direction if initial load fails. If false, 
        /// procedure will automatically use the backup loader if the primary laoder fails</param>
        public SubjectMapSco(
            string filename,
            string folderpath,
            ScoCollection<SubjectMapEntry>.AltListLoader backupLoader,
            string backupFilepath,
            bool askUserOnError,
            ISerializableList<string> commonWords) : 
            base(filename,
                 folderpath,
                 backupLoader,
                 backupFilepath,
                 askUserOnError)
        { _commonWords = commonWords; }

        private ISerializableList<string> _commonWords;
        
        private Regex _tokenizerRegex = Tokenizer.GetRegex();
        public void SetTokenizerRegex(Regex tokenizerRegex) => _tokenizerRegex = tokenizerRegex;

        public void EncodeAll(ISubjectMapEncoder encoder, Regex tokenizerRegex)
        {
            _tokenizerRegex = tokenizerRegex;
            EncodeAll(encoder);
        }

        public void EncodeAll(ISubjectMapEncoder encoder)
        {
            base.ToList().AsParallel().Select(entry => { entry.Encode(encoder, _tokenizerRegex); return entry; });
        }

        /// <summary>
        /// Adds a Subject Map Entry to the list. If it already exists, the count is increased
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="folderName"></param>
        public void Add(string subject, string folderName)
        {
            int idx = base.FindIndex(entry => (entry.EmailSubject == subject) && (entry.Folderpath == folderName));

            // If it doesn't exist, add an entry. If it does exist, increase the count
            if (idx == -1)
            {
                try
                {
                    var sme = new SubjectMapEntry(emailFolder: folderName,
                                              emailSubject: subject,
                                              emailSubjectCount: 1,
                                              commonWords: _commonWords);
                    base.Add(sme);
                }
                catch (ArgumentNullException e)
                {
                    logger.Error($"Error adding {nameof(SubjectMapEntry)}. Skipping entry. {e.Message}");
                }
                catch (InvalidOperationException e) 
                {
                    logger.Error($"Error adding {nameof(SubjectMapEntry)}. Skipping entry. {e.Message}");
                }
                
            }
            else
            {
                base[idx].EmailSubjectCount += 1;
            }
        }

        /// <summary>
        /// Finds a subject map entry by the subject
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public SubjectMapEntry Find(string subject, string folderName)
        {
            int idx = base.FindIndex(entry => (entry.EmailSubject == subject) && (entry.Folderpath == folderName));
            if (idx != -1) { return base[idx]; }
            return null;
        }

        /// <summary>
        /// Find elements in the list that match the given key. 
        /// </summary>
        /// <param name="key">String to match. For EmailSubject, key is standardized. For Folderpath, key is matched literally</param>
        /// <param name="findBy"><inheritdoc cref="FindBy"/></param>
        /// <returns>List of matching subject map entries</returns>
        public IList<SubjectMapEntry> Find(string key, Enums.FindBy findBy)
        {
            switch (findBy)
            {
                case Enums.FindBy.Subject:
                    key = key.StripCommonWords(_commonWords).ToLower();
                    return base.ToList().Where(entry => entry.EmailSubject == key).ToList();

                default:
                    return base.ToList().Where(entry => entry.Folderpath == key).ToList();
            }
        }
    
        public bool TryRepair(SubjectMapEntry entry)
        {
            var idx = this.FindIndex(x => x == entry);
            if (idx == -1) { return false;  }
            var result = this[idx].TryRepair(true);
            if (!result) { return false; }
            this.Serialize();
            return true;
        }

        internal IEnumerable<(MAPIFolder Folder, string RelativePath)> QueryOlFolders(IApplicationGlobals appGlobals)
        {
            var tree = new OlFolderTree(appGlobals.Ol.ArchiveRoot, appGlobals.TD.FilteredFolderScraping.Keys.ToList());
            var folders = tree.Roots
                              .SelectMany(root => root
                              .FlattenIf(node => !node.Selected))
                              .Select(x => (x.OlFolder,x.RelativePath));
            return folders;
        }

        internal IEnumerable<(MailItem Item, string RelativePath)> QueryMailTuples(IEnumerable<(MAPIFolder Folder, string RelativePath)> folders)
        {
            var mailItems = folders
                .SelectMany<(MAPIFolder Folder, string RelativePath), 
                            (MailItem Item, string RelativePath)>(tup => tup
                            .Folder.Items.Cast<object>()
                            .Where(obj => obj is MailItem)
                            .Cast<MailItem>()
                            .Select(item => (item, tup.RelativePath)));
            return mailItems;
        }
                
        internal List<T> Consume<T>(IEnumerable<T> enumerable, int count, ProgressTracker progress)
        {
            int completed = 0;
            List<T> list = null;
            progress.Report(0, $"Consuming {0:N0} of {count:N0}");

            using (new System.Threading.Timer(_ => progress.Report(
                    completed,
                    $"Consuming {(int)((double)completed * (double)count / 100):N0} of {count:N0}"),
                    null, 0, 500))
            {
                list = enumerable.WithProgressReporting(count, (x) => completed = x).ToList();
            }
            return list;
        }

        public void ShowSummaryMetrics()
        {
            summaryMetrics = this
                .GroupBy(x => x.Folderpath)
                .Select(grp => new SummaryMetric 
                { 
                    FolderName = grp.First().Foldername,
                    FolderPath = grp.First().Folderpath,
                    SubjectCount = grp.Count(),
                    EmailCount = grp.Sum(x=>x.EmailSubjectCount)
                })
                .ToList();
            var smm = new SubjectMapMetrics(summaryMetrics);
            smm.Show();
        }
        
        internal void RepopulateSubjectMapEntries(
            IApplicationGlobals appGlobals, 
            ProgressTracker progress,
            IEnumerable<(MAPIFolder Folder, string RelativePath)> folderTuples,
            IEnumerable<(MailItem Item, string RelativePath)> mailIEnumerable)
        {
            this.Clear();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var prelimCount = folderTuples.Select(folder => folder.Folder.Items.Count).Sum();

            var mailTuples = Consume(mailIEnumerable, prelimCount, progress.SpawnChild(27));
            //var mailTuples = mailIEnumerable.ToList();
            var timeConsuming = stopwatch.ElapsedMilliseconds;

            var count = mailTuples.Count();
            var timeCounting = stopwatch.ElapsedMilliseconds - timeConsuming;
            
            RebuildEntries(appGlobals, mailTuples, count, progress.SpawnChild(70));
            var timeRebuilding = stopwatch.ElapsedMilliseconds - timeCounting;

            progress.Increment(0, "Encoding Subject Map");
            appGlobals.AF.Encoder.RebuildEncoding(this);
            var timeEncoding = stopwatch.ElapsedMilliseconds - timeRebuilding;

            logger.Debug($"Time Metrics => Repopulate Subject Map Entries \nConsume: " +
                $"{timeConsuming}\nCount: {timeCounting}\nRebuild: {timeRebuilding}\n" +
                $"Encoding: {timeEncoding}");
        }

        internal void RebuildEntries(
            IApplicationGlobals appGlobals,
            IEnumerable<(MailItem Item, string RelativePath)> mailTuples,
            int count,
            ProgressTracker progress)
        {
            int i = 0;
            foreach (var tuple in mailTuples)
            {
                var subject = tuple.Item.Subject;
                var folderPath = tuple.RelativePath;
                var remappedPath = appGlobals.TD.FolderRemap.ContainsKey(folderPath) ? appGlobals.TD.FolderRemap[folderPath] : folderPath;
                this.Add(subject, remappedPath);
                progress.Report((int)(((double)++i / count) * 100),$"Creating Subject Map Entry {i:N0} of {count:N0}");
            }
        }

        public async Task RebuildAsync(IApplicationGlobals appGlobals)
        {
            // Set up environment
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource).Initialize();

            await Task.Factory.StartNew(() =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                // Query List of Outlook Folders if they are not on the skip list
                progress.Report(0, "Building Outlook Folder Tree");
                var folders = QueryOlFolders(appGlobals);
                progress.Increment(2);

                var timeFolders = stopwatch.ElapsedMilliseconds;
                logger.Debug($"Time querying folders {timeFolders}");

                // Query MailItems from these folders
                var mailItems = QueryMailTuples(folders);
                var timeItems = stopwatch.ElapsedMilliseconds - timeFolders;
                logger.Debug($"Time querying items {timeItems}");

                // Convert MailItems to SubjectMapEntries
                RepopulateSubjectMapEntries(appGlobals, progress, folders, mailItems);
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            progress.Report(100);

                              
        }

        internal List<SummaryMetric> summaryMetrics;
        internal class SummaryMetric
        {
            public string FolderName;
            public string FolderPath;
            public int SubjectCount;
            public int EmailCount;
        }
    }
}
