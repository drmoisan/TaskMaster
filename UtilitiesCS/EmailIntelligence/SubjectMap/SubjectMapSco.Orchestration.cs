using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence.SubjectMap;
using UtilitiesCS.Extensions;

namespace UtilitiesCS
{
    public partial class SubjectMapSco
    {
        internal IEnumerable<(MAPIFolder Folder, string RelativePath)> QueryOlFolders(
            IApplicationGlobals appGlobals
        )
        {
            var tree = new FolderTree(
                appGlobals.Ol.ArchiveRoot,
                appGlobals.TD.FilteredFolderScraping.Keys.ToList()
            );
            var folders = tree
                .Roots.SelectMany(root => root.FlattenIf(node => !node.Selected))
                .Select(x => (x.OlFolder, x.RelativePath));
            return folders;
        }

        internal IEnumerable<(MailItem Item, string RelativePath)> QueryMailTuples(
            IEnumerable<(MAPIFolder Folder, string RelativePath)> folders
        )
        {
            var mailItems = folders.SelectMany<
                (MAPIFolder Folder, string RelativePath),
                (MailItem Item, string RelativePath)
            >(tup =>
                tup.Folder.Items.Cast<object>()
                    .Where(obj => obj is MailItem)
                    .Cast<MailItem>()
                    .Select(item => (item, tup.RelativePath))
            );
            return mailItems;
        }

        internal List<T> Consume<T>(IEnumerable<T> enumerable, int count, ProgressTracker progress)
        {
            int completed = 0;
            List<T> list = null;
            progress.Report(0, $"Consuming {0:N0} of {count:N0}");

            using (
                new System.Threading.Timer(
                    _ =>
                        progress.Report(
                            completed,
                            $"Consuming {(int)((double)completed * (double)count / 100):N0} of {count:N0}"
                        ),
                    null,
                    0,
                    500
                )
            )
            {
                list = enumerable.WithProgressReporting(count, (x) => completed = x).ToList();
            }
            return list;
        }

        public void ShowSummaryMetrics()
        {
            ShowSummaryMetrics(metrics =>
            {
                var smm = new SubjectMapMetrics(metrics);
                smm.Show();
            });
        }

        /// <summary>
        /// Populates <see cref="summaryMetrics"/> from the current entries and passes the
        /// result to <paramref name="showViewer"/>. The overload exists to allow unit tests
        /// to inject a no-op action so that no real window is opened.
        /// </summary>
        /// <param name="showViewer">
        /// Action invoked with the computed metrics. Production code passes a lambda that
        /// creates and shows <see cref="SubjectMapMetrics"/>; tests pass a no-op or a
        /// capturing lambda.
        /// </param>
        internal void ShowSummaryMetrics(Action<IEnumerable<SummaryMetric>> showViewer)
        {
            summaryMetrics = this.GroupBy(x => x.Folderpath)
                .Select(grp => new SummaryMetric
                {
                    FolderName = grp.First().Foldername,
                    FolderPath = grp.First().Folderpath,
                    SubjectCount = grp.Count(),
                    EmailCount = grp.Sum(x => x.EmailSubjectCount),
                })
                .ToList();
            showViewer(summaryMetrics);
        }

        internal void RepopulateSubjectMapEntries(
            IApplicationGlobals appGlobals,
            ProgressTracker progress,
            IEnumerable<(MAPIFolder Folder, string RelativePath)> folderTuples,
            IEnumerable<(MailItem Item, string RelativePath)> mailIEnumerable
        )
        {
            this.Clear();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var prelimCount = folderTuples.Select(folder => folder.Folder.Items.Count).Sum();

            var mailTuples = Consume(mailIEnumerable, prelimCount, progress.SpawnChild(27));
            var timeConsuming = stopwatch.ElapsedMilliseconds;

            var count = mailTuples.Count();
            var timeCounting = stopwatch.ElapsedMilliseconds - timeConsuming;

            RebuildEntries(appGlobals, mailTuples, count, progress.SpawnChild(70));
            var timeRebuilding = stopwatch.ElapsedMilliseconds - timeCounting;

            progress.Increment(0, "Encoding Subject Map");
            appGlobals.AF.Encoder.RebuildEncoding(this);
            var timeEncoding = stopwatch.ElapsedMilliseconds - timeRebuilding;

            logger.Info(
                $"Time Metrics => Repopulate Subject Map Entries \nConsume: "
                    + $"{timeConsuming}\nCount: {timeCounting}\nRebuild: {timeRebuilding}\n"
                    + $"Encoding: {timeEncoding}"
            );
        }

        internal void RebuildEntries(
            IApplicationGlobals appGlobals,
            IEnumerable<(MailItem Item, string RelativePath)> mailTuples,
            int count,
            ProgressTracker progress
        )
        {
            int i = 0;
            foreach (var tuple in mailTuples)
            {
                var subject = tuple.Item.Subject;
                var folderPath = tuple.RelativePath;
                var remappedPath = appGlobals.TD.FolderRemap.ContainsKey(folderPath)
                    ? appGlobals.TD.FolderRemap[folderPath]
                    : folderPath;
                this.Add(subject, remappedPath);
                progress.Report(
                    (int)(((double)++i / count) * 100),
                    $"Creating Subject Map Entry {i:N0} of {count:N0}"
                );
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task RebuildAsync(IApplicationGlobals appGlobals)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource).Initialize();

            await Task.Factory.StartNew(
                () =>
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    progress.Report(0, "Building Outlook Folder Tree");
                    var folders = QueryOlFolders(appGlobals);
                    progress.Increment(2);

                    var timeFolders = stopwatch.ElapsedMilliseconds;

                    var mailItems = QueryMailTuples(folders);
                    var timeItems = stopwatch.ElapsedMilliseconds - timeFolders;

                    RepopulateSubjectMapEntries(appGlobals, progress, folders, mailItems);
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );

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
