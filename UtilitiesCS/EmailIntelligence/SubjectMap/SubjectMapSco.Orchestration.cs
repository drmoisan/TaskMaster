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
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS
{
    public partial class SubjectMapSco
    {
        internal IEnumerable<(MAPIFolder Folder, string RelativePath)> QueryOlFolders(
            IApplicationGlobals appGlobals
        )
        {
            var snapshot = GetFolderTreeSnapshot(appGlobals);
            var excluded = new HashSet<string>(
                appGlobals.TD.FilteredFolderScraping.Keys ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase
            );
            var resolver = CreateFolderHandleResolver(appGlobals);
            return snapshot
                .NodesByKey.Values.Where(node => !excluded.Contains(node.RelativePath))
                .Select(node => (Node: node, Folder: ResolveFolder(resolver, node)))
                .Where(tuple => tuple.Folder != null)
                .Select(tuple => (tuple.Folder, tuple.Node.RelativePath));
        }

        internal virtual FolderTreeSnapshot GetFolderTreeSnapshot(IApplicationGlobals appGlobals)
        {
            var archiveRoot = appGlobals.Ol.ArchiveRoot;
            var request = string.IsNullOrWhiteSpace(archiveRoot?.StoreID)
                ? FolderTreeRequest.AllStores(allowStaleSnapshot: true)
                : FolderTreeRequest.ForStore(archiveRoot.StoreID, allowStaleSnapshot: true);
            var snapshot = appGlobals
                .Ol.FolderTreeService.GetSnapshotAsync(request, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            var archiveNode = archiveRoot is null
                ? null
                : snapshot.FindByPath(archiveRoot.StoreID, archiveRoot.FolderPath);
            return archiveNode is null
                ? snapshot
                : FolderTreeSnapshotQueries.CreateSubtreeSnapshot(snapshot, archiveNode);
        }

        [ExcludeFromCodeCoverage]
        internal virtual IFolderHandleResolver CreateFolderHandleResolver(
            IApplicationGlobals appGlobals
        )
        {
            return new OutlookFolderHandleResolver(appGlobals.Ol.NamespaceMAPI);
        }

        private static MAPIFolder ResolveFolder(
            IFolderHandleResolver resolver,
            FolderTreeSnapshotNode node
        )
        {
            return resolver.TryResolve(node, out var folder) && folder is MAPIFolder mapiFolder
                ? mapiFolder
                : null;
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
                // Report progress deterministically per consumed item through the injected
                // tracker. The per-item callback receives the completion percentage; it updates
                // `completed` (preserving the timer's reading) AND reports each step so that
                // progress is observed at least once per item independent of the wall-clock
                // timer's scheduling. This makes the "reports at least twice" contract
                // deterministic rather than dependent on the 500ms timer firing under load.
                list = enumerable
                    .WithProgressReporting(
                        count,
                        (x) =>
                        {
                            completed = x;
                            progress.Report(
                                x,
                                $"Consuming {(int)((double)x * (double)count / 100):N0} of {count:N0}"
                            );
                        }
                    )
                    .ToList();
            }
            return list;
        }

        [ExcludeFromCodeCoverage]
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
                RebuildCore,
                Tuple.Create(appGlobals, progress),
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );

            progress.Report(100);
        }

        [ExcludeFromCodeCoverage]
        private void RebuildCore(object state)
        {
            var rebuildState = (Tuple<IApplicationGlobals, ProgressTracker>)state;
            var appGlobals = rebuildState.Item1;
            var progress = rebuildState.Item2;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            progress.Report(0, "Building Outlook Folder Tree");
            var folders = QueryOlFolders(appGlobals);
            progress.Increment(2);

            var timeFolders = stopwatch.ElapsedMilliseconds;

            var mailItems = QueryMailTuples(folders);
            var timeItems = stopwatch.ElapsedMilliseconds - timeFolders;

            RepopulateSubjectMapEntries(appGlobals, progress, folders, mailItems);
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
