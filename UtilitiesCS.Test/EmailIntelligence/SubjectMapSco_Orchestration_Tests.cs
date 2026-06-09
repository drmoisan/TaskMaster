using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.SubjectMap;
using UtilitiesCS.ReusableTypeClasses;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;
using OutlookItems = Microsoft.Office.Interop.Outlook.Items;
using OutlookMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class SubjectMapSco_Orchestration_Tests
    {
        private static SubjectMapSco BuildEmptyMap() =>
            new SubjectMapSco(new SerializableList<string>());

        private static Mock<OutlookItems> CreateOutlookItems(int count, params object[] items)
        {
            var outlookItems = new Mock<OutlookItems>(MockBehavior.Strict);
            var collection = new ArrayList(items ?? Array.Empty<object>());
            outlookItems.SetupGet(x => x.Count).Returns(count);
            outlookItems.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return outlookItems;
        }

        private static Mock<OutlookFolder> CreateFolder(
            string folderPath,
            int itemCount = 0,
            object[] items = null,
            params OutlookFolder[] children
        )
        {
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(children).Object);
            folder
                .SetupGet(x => x.Items)
                .Returns(CreateOutlookItems(itemCount, items ?? []).Object);
            return folder;
        }

        private static Mock<OutlookFolders> CreateFoldersCollection(params OutlookFolder[] children)
        {
            var folders = new Mock<OutlookFolders>(MockBehavior.Strict);
            var enumerableChildren = children ?? [];
            var collection = new ArrayList(enumerableChildren);
            folders.SetupGet(x => x.Count).Returns(enumerableChildren.Length);
            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return folders;
        }

        private static string GetLeafName(string folderPath) =>
            folderPath.Split(['\\'], StringSplitOptions.RemoveEmptyEntries)[^1];

        [TestMethod]
        public void QueryOlFolders_WhenSelectedRelativePathIsConfigured_ExcludesSelectedNode()
        {
            var inbox = CreateFolder(@"\\Archive\Inbox");
            var sent = CreateFolder(@"\\Archive\Sent");
            var root = CreateFolder(@"\\Archive", children: [inbox.Object, sent.Object]);
            var filteredFolderScraping = new ScoDictionary<string, int> { ["Inbox"] = 1 };

            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(x => x.ArchiveRoot).Returns(root.Object);

            var td = new Mock<IToDoObjects>(MockBehavior.Strict);
            td.SetupGet(x => x.FilteredFolderScraping).Returns(filteredFolderScraping);

            var appGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            appGlobals.SetupGet(x => x.Ol).Returns(ol.Object);
            appGlobals.SetupGet(x => x.TD).Returns(td.Object);

            var folders = InvokeEnumerable(
                BuildEmptyMap(),
                nameof(SubjectMapSco.QueryOlFolders),
                appGlobals.Object
            );

            folders.Select(tuple => (string)GetTupleField(tuple, "Item2")).Should().Contain("Sent");
            folders
                .Select(tuple => (string)GetTupleField(tuple, "Item2"))
                .Should()
                .NotContain("Inbox");
        }

        [TestMethod]
        public void QueryMailTuples_WhenFoldersContainMixedItems_ReturnsOnlyMailItems()
        {
            var mailA = new Mock<OutlookMailItem>(MockBehavior.Strict);
            var mailB = new Mock<OutlookMailItem>(MockBehavior.Strict);
            var folder = CreateFolder(
                @"\\Archive\Inbox",
                itemCount: 3,
                items: [mailA.Object, new object(), mailB.Object]
            );
            var root = CreateFolder(@"\\Archive", children: [folder.Object]);

            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(x => x.ArchiveRoot).Returns(root.Object);

            var td = new Mock<IToDoObjects>(MockBehavior.Strict);
            td.SetupGet(x => x.FilteredFolderScraping).Returns(new ScoDictionary<string, int>());

            var appGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            appGlobals.SetupGet(x => x.Ol).Returns(ol.Object);
            appGlobals.SetupGet(x => x.TD).Returns(td.Object);

            var folders = InvokeSequence(
                BuildEmptyMap(),
                nameof(SubjectMapSco.QueryOlFolders),
                appGlobals.Object
            );

            var tuples = InvokeEnumerable(
                BuildEmptyMap(),
                nameof(SubjectMapSco.QueryMailTuples),
                folders
            );

            tuples.Should().HaveCount(2);
            tuples
                .Select(tuple => (string)GetTupleField(tuple, "Item2"))
                .Should()
                .OnlyContain(path => path == "Inbox");
            tuples
                .Select(tuple => GetTupleField(tuple, "Item1"))
                .Should()
                .Contain(mailA.Object)
                .And.Contain(mailB.Object);
        }

        [TestMethod]
        public void Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress()
        {
            var tracker = new RecordingProgressTracker();
            var sequence = Enumerable.Range(1, 3);

            // Consume reports progress synchronously per consumed item (the #181 per-item hook in
            // WithProgressReporting) plus an initial report, so at least two reports accumulate
            // deterministically during enumeration without a wall-clock sleep or spin-wait.
            var consumed = BuildEmptyMap().Consume(sequence, 3, tracker);

            consumed.Should().Equal(1, 2, 3);
            tracker.Reports.Count.Should().BeGreaterThanOrEqualTo(2);
            tracker.Reports.Should().Contain(report => report.JobName.StartsWith("Consuming "));
        }

        [TestMethod]
        public void RebuildEntries_WhenFolderRemapExists_UsesMappedFolderPath()
        {
            var mailItem = new Mock<OutlookMailItem>(MockBehavior.Strict);
            mailItem.SetupGet(x => x.Subject).Returns("meeting");
            var folder = CreateFolder(@"\\Archive\Inbox", itemCount: 1, items: [mailItem.Object]);
            var root = CreateFolder(@"\\Archive", children: [folder.Object]);

            var folderRemap = new ScoDictionary<string, string> { ["Inbox"] = "Archive" };
            var td = new Mock<IToDoObjects>(MockBehavior.Strict);
            td.SetupGet(x => x.FilteredFolderScraping).Returns(new ScoDictionary<string, int>());
            td.SetupGet(x => x.FolderRemap).Returns(folderRemap);

            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(x => x.ArchiveRoot).Returns(root.Object);

            var appGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            appGlobals.SetupGet(x => x.Ol).Returns(ol.Object);
            appGlobals.SetupGet(x => x.TD).Returns(td.Object);

            var tracker = new RecordingProgressTracker();
            var map = BuildEmptyMap();
            var folders = InvokeSequence(
                map,
                nameof(SubjectMapSco.QueryOlFolders),
                appGlobals.Object
            );
            var mailTuples = InvokeSequence(map, nameof(SubjectMapSco.QueryMailTuples), folders);

            InvokeVoid(
                map,
                nameof(SubjectMapSco.RebuildEntries),
                appGlobals.Object,
                mailTuples,
                1,
                tracker
            );

            map.Find("meeting", "Archive").Should().NotBeNull();
            map.Find("meeting", "Inbox").Should().BeNull();
            tracker.Reports.Should().Contain(report => report.Value == 100);
        }

        [TestMethod]
        public void RepopulateSubjectMapEntries_WhenMailSequenceProvided_RebuildsAndEncodesMap()
        {
            var mailA = new Mock<OutlookMailItem>(MockBehavior.Strict);
            var mailB = new Mock<OutlookMailItem>(MockBehavior.Strict);
            mailA.SetupGet(x => x.Subject).Returns("meeting");
            mailB.SetupGet(x => x.Subject).Returns("status");
            var folderA = CreateFolder(@"\\Archive\Inbox", itemCount: 1, items: [mailA.Object]);
            var folderB = CreateFolder(@"\\Archive\Sent", itemCount: 1, items: [mailB.Object]);
            var root = CreateFolder(@"\\Archive", children: [folderA.Object, folderB.Object]);

            var folderRemap = new ScoDictionary<string, string> { ["Inbox"] = "Archive" };
            var td = new Mock<IToDoObjects>(MockBehavior.Strict);
            td.SetupGet(x => x.FilteredFolderScraping).Returns(new ScoDictionary<string, int>());
            td.SetupGet(x => x.FolderRemap).Returns(folderRemap);

            var encoder = new Mock<ISubjectMapEncoder>(MockBehavior.Strict);
            var map = BuildEmptyMap();
            encoder.Setup(x => x.RebuildEncoding(map));

            var af = new Mock<IAppAutoFileObjects>(MockBehavior.Strict);
            af.SetupGet(x => x.Encoder).Returns(encoder.Object);

            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(x => x.ArchiveRoot).Returns(root.Object);

            var appGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            appGlobals.SetupGet(x => x.Ol).Returns(ol.Object);
            appGlobals.SetupGet(x => x.TD).Returns(td.Object);
            appGlobals.SetupGet(x => x.AF).Returns(af.Object);
            var folderTuples = InvokeSequence(
                map,
                nameof(SubjectMapSco.QueryOlFolders),
                appGlobals.Object
            );
            var mailTuples = InvokeSequence(
                map,
                nameof(SubjectMapSco.QueryMailTuples),
                folderTuples
            );

            map.Add("stale", "Old");
            InvokeVoid(
                map,
                nameof(SubjectMapSco.RepopulateSubjectMapEntries),
                appGlobals.Object,
                new RecordingProgressTracker(),
                folderTuples,
                mailTuples
            );

            map.Find("stale", "Old").Should().BeNull();
            map.Find("meeting", "Archive").Should().NotBeNull();
            map.Find("status", "Sent").Should().NotBeNull();
            encoder.Verify(x => x.RebuildEncoding(map), Times.Once);
        }

        [TestMethod]
        public void RebuildAsync_CallbackBody_WhenArchiveContainsMailItems_PopulatesMap()
        {
            var mailA = new Mock<OutlookMailItem>(MockBehavior.Strict);
            var mailB = new Mock<OutlookMailItem>(MockBehavior.Strict);
            mailA.SetupGet(x => x.Subject).Returns("meeting");
            mailB.SetupGet(x => x.Subject).Returns("status");

            var folderA = CreateFolder(@"\\Archive\Inbox", itemCount: 1, items: [mailA.Object]);
            var folderB = CreateFolder(@"\\Archive\Sent", itemCount: 1, items: [mailB.Object]);
            var root = CreateFolder(@"\\Archive", children: [folderA.Object, folderB.Object]);

            var folderRemap = new ScoDictionary<string, string> { ["Inbox"] = "Archive" };
            var td = new Mock<IToDoObjects>(MockBehavior.Strict);
            td.SetupGet(x => x.FilteredFolderScraping).Returns(new ScoDictionary<string, int>());
            td.SetupGet(x => x.FolderRemap).Returns(folderRemap);

            var encoder = new Mock<ISubjectMapEncoder>(MockBehavior.Strict);
            var map = BuildEmptyMap();
            encoder.Setup(x => x.RebuildEncoding(map));

            var af = new Mock<IAppAutoFileObjects>(MockBehavior.Strict);
            af.SetupGet(x => x.Encoder).Returns(encoder.Object);

            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(x => x.ArchiveRoot).Returns(root.Object);

            var appGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            appGlobals.SetupGet(x => x.Ol).Returns(ol.Object);
            appGlobals.SetupGet(x => x.TD).Returns(td.Object);
            appGlobals.SetupGet(x => x.AF).Returns(af.Object);

            CreateRebuildAsyncCallback(map, appGlobals.Object).Invoke();

            map.Find("meeting", "Archive").Should().NotBeNull();
            map.Find("status", "Sent").Should().NotBeNull();
            encoder.Verify(x => x.RebuildEncoding(map), Times.Once);
        }

        [TestMethod]
        public void ShowSummaryMetrics_WhenEntriesExist_PopulatesSummaryMetricsAndShowsViewer()
        {
            var map = BuildEmptyMap();
            map.Add("meeting", "Inbox");
            map.Add("status", "Inbox");
            map.Add("receipt", "Sent");

            // Use the internal overload so no real WinForms window is opened.
            map.ShowSummaryMetrics(_ => { });

            map.summaryMetrics.Should().HaveCount(2);
            map.summaryMetrics.Should()
                .Contain(metric =>
                    metric.FolderPath == "Inbox"
                    && metric.SubjectCount == 2
                    && metric.EmailCount == 2
                );
            map.summaryMetrics.Should()
                .Contain(metric =>
                    metric.FolderPath == "Sent"
                    && metric.SubjectCount == 1
                    && metric.EmailCount == 1
                );
        }

        private static System.Action CreateRebuildAsyncCallback(
            SubjectMapSco map,
            IApplicationGlobals appGlobals
        )
        {
            var displayClassType = typeof(SubjectMapSco)
                .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(type =>
                    type.GetMethod(
                        "<RebuildAsync>b__0",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    ) != null
                    && type.GetField(
                        "appGlobals",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    ) != null
                );
            var closure = Activator.CreateInstance(displayClassType);

            displayClassType
                .GetField(
                    "progress",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                )
                .SetValue(closure, new RecordingProgressTracker());
            displayClassType
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(field => field.Name.EndsWith("__this"))
                .SetValue(closure, map);
            displayClassType
                .GetField(
                    "appGlobals",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                )
                .SetValue(closure, appGlobals);

            return (System.Action)
                Delegate.CreateDelegate(
                    typeof(System.Action),
                    closure,
                    displayClassType.GetMethod(
                        "<RebuildAsync>b__0",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    )
                );
        }

        private static object InvokeSequence(
            object target,
            string methodName,
            params object[] args
        ) => ResolveMethod(target, methodName, args).Invoke(target, args);

        private static object[] InvokeEnumerable(
            object target,
            string methodName,
            params object[] args
        )
        {
            var result = ResolveMethod(target, methodName, args).Invoke(target, args);
            return ((IEnumerable)result).Cast<object>().ToArray();
        }

        private static void InvokeVoid(object target, string methodName, params object[] args)
        {
            ResolveMethod(target, methodName, args).Invoke(target, args);
        }

        private static object GetTupleField(object tuple, string fieldName) =>
            tuple.GetType().GetField(fieldName).GetValue(tuple);

        private static MethodInfo ResolveMethod(object target, string methodName, object[] args)
        {
            for (
                var currentType = target.GetType();
                currentType is not null;
                currentType = currentType.BaseType
            )
            {
                var match = currentType
                    .GetMethods(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    )
                    .SingleOrDefault(method =>
                        method.Name == methodName && method.GetParameters().Length == args.Length
                    );
                if (match is not null)
                {
                    return match;
                }
            }

            throw new InvalidOperationException($"No overload found for {methodName}.");
        }

        private sealed class RecordingProgressTracker : ProgressTracker
        {
            private readonly List<(double Value, string JobName)> _reports;

            public RecordingProgressTracker(List<(double Value, string JobName)> reports = null)
                : base(new CancellationTokenSource())
            {
                _reports = reports ?? [];
            }

            public IReadOnlyList<(double Value, string JobName)> Reports => _reports;

            public override ProgressTracker SpawnChild(int allocation) =>
                new RecordingProgressTracker(_reports);

            public override ProgressTracker SpawnChild(double allocation) =>
                new RecordingProgressTracker(_reports);

            public override ProgressTracker Increment(double value, string jobName)
            {
                _reports.Add((value, jobName));
                return this;
            }

            public override ProgressTracker Increment(double value)
            {
                _reports.Add((value, string.Empty));
                return this;
            }

            public override void Report((int Value, string JobName) report) =>
                _reports.Add((report.Value, report.JobName));

            public override void Report(double value, string jobName) =>
                _reports.Add((value, jobName));

            public override void Report(double value) => _reports.Add((value, string.Empty));
        }
    }
}
