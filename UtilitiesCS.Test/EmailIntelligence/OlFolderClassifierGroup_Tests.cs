using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;
using OutlookItems = Microsoft.Office.Interop.Outlook.Items;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class OlFolderClassifierGroup_AdditionalTests
    {
        private Func<MyBoxViewer, DialogResult> _originalDialogInvoker;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalDialogInvoker = MyBox.DialogInvoker;
            MyBox.DialogInvoker = _ => DialogResult.OK;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MyBox.DialogInvoker = _originalDialogInvoker;
        }

        [TestMethod]
        public async Task BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier()
        {
            var mockGlobals = CreateMockGlobals();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var appDataRoot = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "UtilitiesCS.Test",
                    "EmailIntelligence",
                    "TestData",
                    "OlFolderClassifierGroup"
                )
            );
            mockFs
                .SetupGet(x => x.SpecialFolders)
                .Returns(new ConcurrentDictionary<string, string> { ["AppData"] = appDataRoot });
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);

            var progressPane = new Mock<CustomTaskPane>();
            progressPane.SetupProperty(x => x.Visible, false);
            var mockAf = new Mock<IAppAutoFileObjects>();
            var folderLoader = new SmartSerializableLoader(mockGlobals.Object) { Name = "Folder" };
            folderLoader.Config.ClassifierActivated = true;
            var manager = new StubManagerAsyncLazy(mockGlobals.Object, ("Folder", folderLoader));
            mockAf.SetupGet(x => x.Manager).Returns(manager);
            mockAf.SetupGet(x => x.ProgressTracker).Returns(CreateHeadlessProgressTrackerPane());
            mockAf.SetupGet(x => x.ProgressPane).Returns(progressPane.Object);
            mockGlobals.SetupGet(x => x.AF).Returns(mockAf.Object);

            var mockRoot = CreateOutlookFolder("Archive", 0);
            var mockOl = new Mock<IOlObjects>();
            mockOl.SetupGet(x => x.ArchiveRoot).Returns(mockRoot.Object);
            mockGlobals.SetupGet(x => x.Ol).Returns(mockOl.Object);

            var mockTd = new Mock<IToDoObjects>();
            mockTd
                .SetupGet(x => x.FilteredFolderScraping)
                .Returns(new ScoDictionary<string, int>());
            mockGlobals.SetupGet(x => x.TD).Returns(mockTd.Object);

            var classifierGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 2,
                SharedTokenBase = new Corpus(new Dictionary<string, int> { ["alpha"] = 2 }),
            };
            var group = new TrackingOlFolderClassifierGroup(mockGlobals.Object, classifierGroup);

            await group.BuildClassifiersAsync();

            progressPane.Object.Visible.Should().BeFalse();
            group.BuiltGroupingKeys.Should().Contain(new[] { "Inbox", "Projects" });
            manager.ContainsKey("Folder").Should().BeTrue();
            classifierGroup.Classifiers.Should().ContainKey("Inbox");
            classifierGroup.Classifiers.Should().ContainKey("Projects");
        }

        [TestMethod]
        public async Task CreateSpamClassifiersAsync_WithSpamConfig_AssignsManagerEntryAndCopiesConfig()
        {
            var mockGlobals = CreateMockGlobals();
            var spamLoader = new SmartSerializableLoader(mockGlobals.Object) { Name = "Spam" };
            spamLoader.Config.ClassifierActivated = true;
            var manager = new StubManagerAsyncLazy(mockGlobals.Object, ("Spam", spamLoader));
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockAf.SetupGet(x => x.Manager).Returns(manager);
            mockGlobals.SetupGet(x => x.AF).Returns(mockAf.Object);

            var group = new OlFolderClassifierGroup(mockGlobals.Object);

            await group.CreateSpamClassifiersAsync();

            manager.TryGetValue("Spam", out var lazyGroup).Should().BeTrue();
            var spamGroup = await lazyGroup;
            spamGroup.SharedTokenBase.Should().NotBeNull();
            spamGroup.TotalEmailCount.Should().Be(0);
            spamGroup.Config.Should().BeSameAs(spamLoader.Config);
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var mockTd = new Mock<IToDoObjects>();

            mockGlobals.SetupGet(x => x.Ol).Returns(mockOl.Object);
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);
            mockGlobals.SetupGet(x => x.AF).Returns(mockAf.Object);
            mockGlobals.SetupGet(x => x.TD).Returns(mockTd.Object);
            return mockGlobals;
        }

        private static ProgressTrackerPane CreateHeadlessProgressTrackerPane(double progress = 0)
        {
            var pane = (ProgressTrackerPane)
                FormatterServices.GetUninitializedObject(typeof(ProgressTrackerPane));
            var parentProgressType = typeof(ProgressTrackerPane)
                .Assembly.GetType("UtilitiesCS.ParentProgress`1")!
                .MakeGenericType(typeof(ValueTuple<int, string>));
            var parentProgress = Activator.CreateInstance(
                parentProgressType,
                new Progress<(int Value, string JobName)>(_ => { }),
                100,
                0
            );

            typeof(ProgressTrackerPane)
                .GetField(
                    "_parent",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )!
                .SetValue(pane, parentProgress);
            typeof(ProgressTrackerPane)
                .GetField(
                    "_progress",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )!
                .SetValue(pane, progress);
            typeof(ProgressTrackerPane)
                .GetField(
                    "_isRoot",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )!
                .SetValue(pane, false);
            typeof(ProgressTrackerPane)
                .GetField(
                    "_jobName",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )!
                .SetValue(pane, "Test");
            return pane;
        }

        private static Mock<OutlookFolder> CreateOutlookFolder(
            string folderPath,
            int itemCount,
            params OutlookFolder[] children
        )
        {
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(children).Object);
            folder.SetupGet(x => x.Items).Returns(CreateItems(itemCount).Object);
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

        private static Mock<OutlookItems> CreateItems(int count = 0)
        {
            var items = new Mock<OutlookItems>(MockBehavior.Strict);
            var collection = new ArrayList();
            items.SetupGet(x => x.Count).Returns(count);
            items.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return items;
        }

        private static string GetLeafName(string folderPath) =>
            folderPath.Split('\\').Last(segment => !string.IsNullOrWhiteSpace(segment));

        private sealed class TrackingOlFolderClassifierGroup(
            IApplicationGlobals globals,
            BayesianClassifierGroup classifierGroup
        ) : OlFolderClassifierGroup(globals)
        {
            private readonly BayesianClassifierGroup _classifierGroup = classifierGroup;

            // BuildClassifierAsync runs concurrently via AsyncMultiTasker.AsyncMultiTaskChunker,
            // so the key-tracking store must be thread-safe. A plain List<T>.Add from multiple
            // threads corrupts the backing array (observed as a null slot and a dropped element).
            // ConcurrentBag<string> serves the FluentAssertions Contain assertion as IEnumerable.
            private readonly ConcurrentBag<string> _builtGroupingKeys = new();

            public IEnumerable<string> BuiltGroupingKeys => _builtGroupingKeys;

            public override Task<BayesianClassifierGroup> GetOrCreateClassifierGroupAsync(
                MinedMailInfo[] collection
            ) => Task.FromResult(_classifierGroup);

            public override Task BuildClassifierAsync(
                IGrouping<string, MinedMailInfo> group,
                BayesianClassifierGroup classifierGroup,
                CancellationToken cancel
            )
            {
                _builtGroupingKeys.Add(group.Key);

                // classifierGroup.Classifiers is a ConcurrentDictionary, so the indexer
                // assignment below is already thread-safe and needs no additional guard.
                classifierGroup.Classifiers[group.Key] = new BayesianClassifierShared(
                    group.Key,
                    classifierGroup
                )
                {
                    MatchEmailCount = group.Count(),
                };
                return Task.CompletedTask;
            }
        }

        private sealed class StubManagerAsyncLazy : ManagerAsyncLazy
        {
            public StubManagerAsyncLazy(
                IApplicationGlobals globals,
                params (string Key, SmartSerializableLoader Loader)[] loaders
            )
                : base(globals)
            {
                var configuration = new ConcurrentDictionary<string, SmartSerializableLoader>();
                foreach (var (key, loader) in loaders)
                {
                    configuration[key] = loader;
                }

                Configuration = new AsyncLazy<
                    ConcurrentDictionary<string, SmartSerializableLoader>
                >(() => Task.FromResult(configuration));
            }
        }
    }
}
