using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.SubjectMap;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.ReusableTypeClasses;
using OutlookItems = Microsoft.Office.Interop.Outlook.Items;
using OutlookMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class SubjectMapSco_Orchestration_Tests
    {
        [TestMethod]
        public void QueryOlFolders_WithFakeSnapshot_ExcludesFilteredPathAndUsesResolver()
        {
            var inbox = CreateFolder(itemCount: 0);
            var sent = CreateFolder(itemCount: 0);
            var resolver = new FakeFolderHandleResolver
            {
                HandlesByRelativePath =
                {
                    ["Archive\\Inbox"] = inbox.Object,
                    ["Archive\\Sent"] = sent.Object,
                },
            };
            var map = BuildMap(CreateSnapshot(), resolver);
            var globals = CreateGlobals(filtered: ["Archive\\Inbox"]);

            var folders = InvokeEnumerable(
                map,
                nameof(SubjectMapSco.QueryOlFolders),
                globals.Object
            );

            folders
                .Select(tuple => (string)GetTupleField(tuple, "Item2"))
                .Should()
                .Equal("Archive\\Sent");
            folders.Select(tuple => GetTupleField(tuple, "Item1")).Should().Contain(sent.Object);
            folders
                .Select(tuple => GetTupleField(tuple, "Item1"))
                .Should()
                .NotContain(inbox.Object);
            resolver.TryResolveCalls.Should().Be(2);
        }

        [TestMethod]
        public void QueryOlFolders_WhenResolverCannotResolveHandle_SkipsFolder()
        {
            var resolver = new FakeFolderHandleResolver();
            var map = BuildMap(CreateSnapshot(), resolver);

            var folders = InvokeEnumerable(
                map,
                nameof(SubjectMapSco.QueryOlFolders),
                CreateGlobals().Object
            );

            folders.Should().BeEmpty();
            resolver.TryResolveCalls.Should().Be(3);
        }

        [TestMethod]
        public void GetFolderTreeSnapshot_WithArchiveRoot_UsesCachedRequestAndSubtreeFallback()
        {
            var archivePath = "\\Missing";
            var archiveRoot = new Mock<Folder>(MockBehavior.Strict);
            archiveRoot.SetupGet(x => x.StoreID).Returns("store");
            archiveRoot.SetupGet(x => x.FolderPath).Returns(() => archivePath);
            var snapshot = CreateSnapshot();
            FolderTreeRequest request = null;
            var service = new Mock<IOutlookFolderTreeService>(MockBehavior.Strict);
            service
                .Setup(x =>
                    x.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .Callback<FolderTreeRequest, CancellationToken>((value, _) => request = value)
                .ReturnsAsync(snapshot);
            var globals = CreateGlobals(service: service.Object, archiveRoot: archiveRoot.Object);
            var map = new SubjectMapSco(new SerializableList<string>());

            var fallback = (FolderTreeSnapshot)InvokeSequence(
                map,
                nameof(SubjectMapSco.GetFolderTreeSnapshot),
                globals.Object
            );

            request.AllowStaleSnapshot.Should().BeTrue();
            request.StoreIds.Should().ContainSingle().Which.Should().Be("store");
            fallback.Should().BeSameAs(snapshot);

            archivePath = "\\Archive";
            var subtree = (FolderTreeSnapshot)InvokeSequence(
                map,
                nameof(SubjectMapSco.GetFolderTreeSnapshot),
                globals.Object
            );

            subtree.Should().NotBeSameAs(snapshot);
            subtree.RootKeys.Should().ContainSingle();
            subtree
                .NodesByKey.Values.Select(node => node.RelativePath)
                .Should()
                .BeEquivalentTo("Archive", "Archive\\Inbox", "Archive\\Sent");
        }

        [TestMethod]
        public void QueryMailTuples_WhenFoldersContainMixedItems_ReturnsOnlyMailItems()
        {
            var mailA = new Mock<OutlookMailItem>(MockBehavior.Strict);
            var mailB = new Mock<OutlookMailItem>(MockBehavior.Strict);
            var folder = CreateFolder(3, mailA.Object, new object(), mailB.Object);
            var resolver = new FakeFolderHandleResolver
            {
                HandlesByRelativePath = { ["Archive\\Inbox"] = folder.Object },
            };
            var map = BuildMap(CreateSnapshot(includeSent: false), resolver);

            var folders = InvokeSequence(
                map,
                nameof(SubjectMapSco.QueryOlFolders),
                CreateGlobals().Object
            );
            var tuples = InvokeEnumerable(map, nameof(SubjectMapSco.QueryMailTuples), folders);

            tuples.Should().HaveCount(2);
            tuples
                .Select(tuple => (string)GetTupleField(tuple, "Item2"))
                .Should()
                .OnlyContain(path => path == "Archive\\Inbox");
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

            var consumed = BuildMap().Consume(Enumerable.Range(1, 3), 3, tracker);

            consumed.Should().Equal(1, 2, 3);
            tracker.Reports.Count.Should().BeGreaterThanOrEqualTo(2);
            tracker.Reports.Should().Contain(report => report.JobName.StartsWith("Consuming "));
        }

        [TestMethod]
        public void RebuildEntries_WhenFolderRemapExists_UsesMappedFolderPath()
        {
            var mailItem = new Mock<OutlookMailItem>(MockBehavior.Strict);
            mailItem.SetupGet(x => x.Subject).Returns("meeting");
            var folder = CreateFolder(1, mailItem.Object);
            var globals = CreateGlobals(remap: new() { ["Archive\\Inbox"] = "Archive" });
            var resolver = new FakeFolderHandleResolver
            {
                HandlesByRelativePath = { ["Archive\\Inbox"] = folder.Object },
            };
            var map = BuildMap(CreateSnapshot(includeSent: false), resolver);
            var folders = InvokeSequence(map, nameof(SubjectMapSco.QueryOlFolders), globals.Object);
            var mailTuples = InvokeSequence(map, nameof(SubjectMapSco.QueryMailTuples), folders);

            InvokeVoid(
                map,
                nameof(SubjectMapSco.RebuildEntries),
                globals.Object,
                mailTuples,
                1,
                new RecordingProgressTracker()
            );

            map.Find("meeting", "Archive").Should().NotBeNull();
            map.Find("meeting", "Archive\\Inbox").Should().BeNull();
        }

        [TestMethod]
        public void RepopulateSubjectMapEntries_WhenMailSequenceProvided_RebuildsAndEncodesMap()
        {
            var mailA = CreateMail("meeting");
            var mailB = CreateMail("status");
            var inbox = CreateFolder(1, mailA.Object);
            var sent = CreateFolder(1, mailB.Object);
            var resolver = new FakeFolderHandleResolver
            {
                HandlesByRelativePath =
                {
                    ["Archive\\Inbox"] = inbox.Object,
                    ["Archive\\Sent"] = sent.Object,
                },
            };
            var map = BuildMap(CreateSnapshot(), resolver);
            var encoder = new Mock<ISubjectMapEncoder>(MockBehavior.Strict);
            encoder.Setup(x => x.RebuildEncoding(map));
            var globals = CreateGlobals(
                remap: new() { ["Archive\\Inbox"] = "Archive" },
                encoder: encoder.Object
            );
            var folderTuples = InvokeSequence(
                map,
                nameof(SubjectMapSco.QueryOlFolders),
                globals.Object
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
                globals.Object,
                new RecordingProgressTracker(),
                folderTuples,
                mailTuples
            );

            map.Find("stale", "Old").Should().BeNull();
            map.Find("meeting", "Archive").Should().NotBeNull();
            map.Find("status", "Archive\\Sent").Should().NotBeNull();
            encoder.Verify(x => x.RebuildEncoding(map), Times.Once);
        }

        [TestMethod]
        public void ShowSummaryMetrics_WhenEntriesExist_PopulatesSummaryMetricsAndShowsViewer()
        {
            var map = BuildMap();
            map.Add("meeting", "Inbox");
            map.Add("status", "Inbox");
            map.Add("receipt", "Sent");

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

        private static TestSubjectMapSco BuildMap(
            FolderTreeSnapshot snapshot = null,
            IFolderHandleResolver resolver = null
        )
        {
            return new(snapshot ?? CreateSnapshot(), resolver ?? new FakeFolderHandleResolver());
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            IEnumerable<string> filtered = null,
            ScoDictionaryNew<string, string> remap = null,
            ISubjectMapEncoder encoder = null,
            IOutlookFolderTreeService service = null,
            Folder archiveRoot = null
        )
        {
            var td = new Mock<IToDoObjects>(MockBehavior.Strict);
            var filteredFolders = new ScoDictionaryNew<string, int>();
            foreach (var path in filtered ?? [])
            {
                filteredFolders.TryAdd(path, 1);
            }
            td.SetupGet(x => x.FilteredFolderScraping).Returns(filteredFolders);
            td.SetupGet(x => x.FolderRemap)
                .Returns(remap ?? new ScoDictionaryNew<string, string>());

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.TD).Returns(td.Object);
            if (service != null || archiveRoot != null)
            {
                var ol = new Mock<IOlObjects>(MockBehavior.Strict);
                ol.SetupGet(x => x.FolderTreeService).Returns(service);
                ol.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot);
                globals.SetupGet(x => x.Ol).Returns(ol.Object);
            }
            if (encoder != null)
            {
                var af = new Mock<IAppAutoFileObjects>(MockBehavior.Strict);
                af.SetupGet(x => x.Encoder).Returns(encoder);
                globals.SetupGet(x => x.AF).Returns(af.Object);
            }

            return globals;
        }

        private static Mock<MAPIFolder> CreateFolder(int itemCount, params object[] items)
        {
            var folder = new Mock<MAPIFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Items).Returns(CreateOutlookItems(itemCount, items).Object);
            return folder;
        }

        private static Mock<OutlookItems> CreateOutlookItems(int count, params object[] items)
        {
            var outlookItems = new Mock<OutlookItems>(MockBehavior.Strict);
            var collection = new ArrayList(items ?? Array.Empty<object>());
            outlookItems.SetupGet(x => x.Count).Returns(count);
            outlookItems.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return outlookItems;
        }

        private static Mock<OutlookMailItem> CreateMail(string subject)
        {
            var mail = new Mock<OutlookMailItem>(MockBehavior.Strict);
            mail.SetupGet(x => x.Subject).Returns(subject);
            return mail;
        }

        private static FolderTreeSnapshot CreateSnapshot(bool includeSent = true)
        {
            var rootKey = new FolderTreeNodeKey("store", "archive", "\\Archive");
            var inboxKey = new FolderTreeNodeKey("store", "inbox", "\\Archive\\Inbox");
            var sentKey = new FolderTreeNodeKey("store", "sent", "\\Archive\\Sent");
            var childKeys = includeSent ? new[] { inboxKey, sentKey } : new[] { inboxKey };
            var nodes = new List<FolderTreeSnapshotNode>
            {
                CreateNode(rootKey, "Archive", null, "Archive", childKeys),
                CreateNode(inboxKey, "Inbox", rootKey, "Archive\\Inbox"),
            };
            if (includeSent)
            {
                nodes.Add(CreateNode(sentKey, "Sent", rootKey, "Archive\\Sent"));
            }

            return new(new[] { rootKey }, nodes);
        }

        private static FolderTreeSnapshotNode CreateNode(
            FolderTreeNodeKey key,
            string name,
            FolderTreeNodeKey parent,
            string relativePath,
            params FolderTreeNodeKey[] children
        )
        {
            return new(
                key,
                name,
                key.StoreId,
                key.EntryId,
                parent,
                key.FolderPath,
                relativePath,
                children,
                false,
                string.Empty
            );
        }

        private sealed class TestSubjectMapSco : SubjectMapSco
        {
            private readonly FolderTreeSnapshot _snapshot;
            private readonly IFolderHandleResolver _resolver;

            public TestSubjectMapSco(FolderTreeSnapshot snapshot, IFolderHandleResolver resolver)
                : base(new SerializableList<string>())
            {
                _snapshot = snapshot;
                _resolver = resolver;
            }

            internal override FolderTreeSnapshot GetFolderTreeSnapshot(
                IApplicationGlobals appGlobals
            ) => _snapshot;

            internal override IFolderHandleResolver CreateFolderHandleResolver(
                IApplicationGlobals appGlobals
            ) => _resolver;
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

        private sealed class FakeFolderHandleResolver : IFolderHandleResolver
        {
            public Dictionary<string, object> HandlesByRelativePath { get; } =
                new(StringComparer.OrdinalIgnoreCase);

            public int TryResolveCalls { get; private set; }

            public object Resolve(FolderTreeSnapshotNode node)
            {
                return HandlesByRelativePath[node.RelativePath];
            }

            public bool TryResolve(FolderTreeSnapshotNode node, out object folder)
            {
                TryResolveCalls++;
                return HandlesByRelativePath.TryGetValue(node.RelativePath, out folder);
            }
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
