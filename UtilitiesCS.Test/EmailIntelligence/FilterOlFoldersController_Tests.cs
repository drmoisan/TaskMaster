using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.ReusableTypeClasses;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class FilterOlFoldersController_Tests
    {
        [STATestMethod]
        public async Task Constructor_WithFakeSnapshot_UsesCallerLocalSelectionAndWiresViewer()
        {
            var service = new FakeFolderTreeService(CreateSnapshot());
            var scraping = new ScoDictionaryNew<string, int>();
            scraping.TryAdd("Archive\\Filtered", 1);
            var viewer = new FakeFilterViewer();
            var globals = CreateGlobals(service, scraping);

            var controller = await FilterOlFoldersController.CreateAsync(
                globals.Object,
                () => viewer,
                new FilterOlFoldersControllerRefreshDisposalTests.RecordingInlineUiDispatcher()
            );

            controller.FilterSelected(true).Should().ContainSingle();
            controller.FilterSelected(true)[0].Value.RelativePath.Should().Be("Archive\\Filtered");
            Flatten(controller.FilterSelected(false))
                .Should()
                .Contain(wrapper => wrapper.RelativePath == "Archive\\Visible");
            viewer.Controller.Should().BeSameAs(controller);
            viewer.TlvNotFiltered.CheckStateGetter.Should().NotBeNull();
            viewer.TlvFiltered.CheckStatePutter.Should().NotBeNull();
            service.SnapshotChangedSubscriberCount.Should().Be(1);
        }

        [STATestMethod]
        public void FilterSelected_WithSameSnapshot_KeepsSelectionCallerLocal()
        {
            var first = CreateControllerWithView(
                CreateCompatibilityView(CreateSnapshot(), "Archive\\Filtered")
            );
            var second = CreateControllerWithView(CreateCompatibilityView(CreateSnapshot()));

            first.FilterSelected(true).Should().ContainSingle();
            second.FilterSelected(true).Should().BeEmpty();

            first.FolderTreeView.Roots[0].Children[0].Value.Selected = false;

            first.FilterSelected(true).Should().BeEmpty();
            Flatten(second.FilterSelected(false))
                .Should()
                .Contain(wrapper => wrapper.RelativePath == "Archive\\Filtered");
        }

        [STATestMethod]
        public async Task ViewerClose_DisposesViewAndRemovesServiceHandler()
        {
            var service = new FakeFolderTreeService(CreateSnapshot());
            var viewer = new FakeFilterViewer();
            var controller = await FilterOlFoldersController.CreateAsync(
                CreateGlobals(service, new ScoDictionaryNew<string, int>()).Object,
                () => viewer,
                new FilterOlFoldersControllerRefreshDisposalTests.RecordingInlineUiDispatcher()
            );
            var view = controller.FolderTreeView;

            viewer.Close();

            service.SnapshotChangedSubscriberCount.Should().Be(0);
            view.SubscriptionCount.Should().Be(0);
        }

        [STATestMethod]
        public async Task ConstructorAndRefresh_UseCachedArchiveSnapshotAndReplaceView()
        {
            var archivePath = "\\Missing";
            var archiveRoot = new Mock<Outlook.Folder>();
            archiveRoot.SetupGet(x => x.StoreID).Returns("store");
            archiveRoot.SetupGet(x => x.FolderPath).Returns(() => archivePath);
            var service = new FakeFolderTreeService(CreateSnapshot());
            var scraping = new ScoDictionaryNew<string, int>();
            scraping.TryAdd("Archive\\Visible", 1);

            var controller = await FilterOlFoldersController.CreateAsync(
                CreateGlobals(service, scraping, archiveRoot.Object).Object,
                () => new FakeFilterViewer(),
                new FilterOlFoldersControllerRefreshDisposalTests.RecordingInlineUiDispatcher()
            );

            service.LastRequest.AllowStaleSnapshot.Should().BeTrue();
            service.LastRequest.StoreIds.Should().ContainSingle().Which.Should().Be("store");
            controller.FolderTreeView.Snapshot.Should().BeSameAs(service.Snapshot);

            archivePath = "\\Archive";
            service.PublishSnapshotChanged();

            controller.FolderTreeView.Roots.Should().ContainSingle();
            controller.FolderTreeView.Roots[0].Value.RelativePath.Should().Be("Archive");
            controller.FilterSelected(true).Should().ContainSingle();
            controller.FilterSelected(true)[0].Value.RelativePath.Should().Be("Archive\\Visible");
        }

        [STATestMethod]
        public void Save_WhenSelectionChanges_RemovesDeselectedKeysAndAddsSelectedKeys()
        {
            var scraping = new ScoDictionaryNew<string, int>();
            scraping.TryAdd("RemoveMe", 1);
            var view = CreateCompatibilityView(CreateSnapshot(), "Archive\\Visible");
            var viewer = new FakeFilterViewer();
            var controller = CreateControllerWithView(view, viewer, CreateGlobals(scraping).Object);

            controller.Save();

            scraping.ContainsKey("RemoveMe").Should().BeFalse();
            scraping.ContainsKey("Archive\\Visible").Should().BeTrue();
            viewer.CloseCount.Should().Be(1);
        }

        [STATestMethod]
        public void Discard_ClosesViewer()
        {
            var viewer = new FakeFilterViewer();
            var controller = CreateControllerWithView(
                CreateCompatibilityView(CreateSnapshot()),
                viewer,
                CreateGlobals(new ScoDictionaryNew<string, int>()).Object
            );

            controller.Discard();

            viewer.CloseCount.Should().Be(1);
        }

        [STATestMethod]
        public void OlFolderTree_PropertyChangedInternal_RefreshesFilteredAndUnfilteredRoots()
        {
            var viewer = new FakeFilterViewer();
            viewer.TlvNotFiltered.ExpandedObjects = new List<object>();
            viewer.TlvFiltered.ExpandedObjects = new List<object>();
            var controller = CreateControllerWithView(
                CreateCompatibilityView(CreateSnapshot(), "Archive\\Filtered"),
                viewer,
                CreateGlobals(new ScoDictionaryNew<string, int>()).Object
            );

            controller.OlFolderTree_PropertyChangedInternal(
                controller,
                new PropertyChangedEventArgs(nameof(controller.FolderTreeView))
            );

            viewer.TlvFiltered.Roots.Should().NotBeNull();
            viewer.TlvNotFiltered.Roots.Should().NotBeNull();
        }

        [STATestMethod]
        public void OlFolderTree_PropertyChanged_OnSameThread_RefreshesViewerWithoutInvoke()
        {
            var viewer = new FakeFilterViewer();
            viewer.TlvNotFiltered.ExpandedObjects = new List<object>();
            viewer.TlvFiltered.ExpandedObjects = new List<object>();
            var controller = CreateControllerWithView(
                CreateCompatibilityView(CreateSnapshot()),
                viewer,
                CreateGlobals(new ScoDictionaryNew<string, int>()).Object
            );

            Action act = () =>
                controller.OlFolderTree_PropertyChanged(
                    controller,
                    new PropertyChangedEventArgs(nameof(controller.FolderTreeView))
                );

            act.Should().NotThrow();
            viewer.InvokeCount.Should().Be(0);
        }

        [STATestMethod]
        public void PutCheckedStateMethod_Collapsed_ChecksNodeAndDescendants()
        {
            var controller = CreateUninitializedController();
            var parent = CreateNode("Parent", "Parent", selected: false);
            var child = parent.AddChild(new FolderWrapper(false, 0, 0, "Child", "Parent\\Child"));

            var result = controller.PutCheckedStateMethod(
                parent,
                CheckState.Checked,
                new TreeListView()
            );

            result.Should().Be(CheckState.Checked);
            parent.Value.Selected.Should().BeTrue();
            child.Value.Selected.Should().BeTrue();
        }

        [STATestMethod]
        public void PutCheckedStateMethod_Expanded_UpdatesOnlyCurrentNode()
        {
            var controller = CreateUninitializedController();
            var parent = CreateNode("Parent", "Parent", selected: false);
            var child = parent.AddChild(new FolderWrapper(false, 0, 0, "Child", "Parent\\Child"));
            var tree = new TreeListView { Roots = new List<TreeNode<FolderWrapper>> { parent } };
            tree.ExpandedObjects = new List<object> { parent };

            var result = controller.PutCheckedStateMethod(parent, CheckState.Checked, tree);

            result.Should().Be(CheckState.Checked);
            parent.Value.Selected.Should().BeTrue();
            child.Value.Selected.Should().BeFalse();
        }

        [STATestMethod]
        public void PutCheckedStateMethodForwarders_UseAssignedViewerTrees()
        {
            var viewer = new FakeFilterViewer();
            var controller = CreateControllerWithView(
                CreateCompatibilityView(CreateSnapshot()),
                viewer,
                CreateGlobals(new ScoDictionaryNew<string, int>()).Object
            );
            var filtered = CreateNode("Filtered", "Filtered", selected: false);
            var notFiltered = CreateNode("NotFiltered", "NotFiltered", selected: true);

            var filteredResult = controller.PutCheckedStateMethodFiltered(
                filtered,
                CheckState.Checked
            );
            var notFilteredResult = controller.PutCheckedStateMethodNotFiltered(
                notFiltered,
                CheckState.Unchecked
            );

            filteredResult.Should().Be(CheckState.Checked);
            filtered.Value.Selected.Should().BeTrue();
            notFilteredResult.Should().Be(CheckState.Unchecked);
            notFiltered.Value.Selected.Should().BeFalse();
        }

        private static FilterOlFoldersController CreateControllerWithView(
            FolderTreeCompatibilityView view,
            IFilterOlFoldersViewer viewer = null,
            IApplicationGlobals globals = null
        )
        {
            var controller = CreateUninitializedController();
            SetField(controller, "_folderTreeView", view);
            SetField(controller, "_viewer", viewer ?? new FakeFilterViewer());
            SetField(
                controller,
                "_globals",
                globals ?? CreateGlobals(new ScoDictionaryNew<string, int>()).Object
            );
            return controller;
        }

        private static FilterOlFoldersController CreateUninitializedController()
        {
            return (FilterOlFoldersController)
                FormatterServices.GetUninitializedObject(typeof(FilterOlFoldersController));
        }

        private static void SetField(object instance, string name, object value)
        {
            instance
                .GetType()
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(instance, value);
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            ScoDictionaryNew<string, int> scraping
        )
        {
            return CreateGlobals(new FakeFolderTreeService(CreateSnapshot()), scraping);
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            IOutlookFolderTreeService service,
            ScoDictionaryNew<string, int> scraping,
            Outlook.Folder archiveRoot = null
        )
        {
            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(x => x.ArchiveRoot).Returns(() => archiveRoot);
            ol.SetupGet(x => x.FolderTreeService).Returns(service);

            var td = new Mock<IToDoObjects>(MockBehavior.Strict);
            td.SetupGet(x => x.FilteredFolderScraping).Returns(scraping);

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.Ol).Returns(ol.Object);
            globals.SetupGet(x => x.TD).Returns(td.Object);
            return globals;
        }

        private static FolderTreeCompatibilityView CreateCompatibilityView(
            FolderTreeSnapshot snapshot,
            params string[] selectedPaths
        )
        {
            return new(snapshot, new FolderTreeSelectionOverlay(selectedPaths));
        }

        private static FolderTreeSnapshot CreateSnapshot()
        {
            var rootKey = new FolderTreeNodeKey("store", "archive", "\\Archive");
            var filteredKey = new FolderTreeNodeKey("store", "filtered", "\\Archive\\Filtered");
            var visibleKey = new FolderTreeNodeKey("store", "visible", "\\Archive\\Visible");
            return new(
                new[] { rootKey },
                new[]
                {
                    new FolderTreeSnapshotNode(
                        rootKey,
                        "Archive",
                        "store",
                        "archive",
                        null,
                        "\\Archive",
                        "Archive",
                        new[] { filteredKey, visibleKey },
                        false,
                        string.Empty
                    ),
                    new FolderTreeSnapshotNode(
                        filteredKey,
                        "Filtered",
                        "store",
                        "filtered",
                        rootKey,
                        "\\Archive\\Filtered",
                        "Archive\\Filtered",
                        Array.Empty<FolderTreeNodeKey>(),
                        false,
                        string.Empty
                    ),
                    new FolderTreeSnapshotNode(
                        visibleKey,
                        "Visible",
                        "store",
                        "visible",
                        rootKey,
                        "\\Archive\\Visible",
                        "Archive\\Visible",
                        Array.Empty<FolderTreeNodeKey>(),
                        false,
                        string.Empty
                    ),
                }
            );
        }

        private static TreeNode<FolderWrapper> CreateNode(
            string name,
            string relativePath,
            bool selected
        )
        {
            return new(new FolderWrapper(selected, 0, 0, name, relativePath));
        }

        private static IEnumerable<FolderWrapper> Flatten(
            IEnumerable<TreeNode<FolderWrapper>> roots
        )
        {
            foreach (var root in roots)
            {
                foreach (var wrapper in root.Flatten())
                {
                    yield return wrapper;
                }
            }
        }

        private sealed class FakeFolderTreeService : IOutlookFolderTreeService
        {
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _snapshotChanged;

            public FakeFolderTreeService(FolderTreeSnapshot snapshot)
            {
                Snapshot = snapshot;
            }

            public int SnapshotChangedSubscriberCount { get; private set; }

            public FolderTreeRequest LastRequest { get; private set; }

            public FolderTreeSnapshot Snapshot { get; set; }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs> SnapshotChanged
            {
                add
                {
                    _snapshotChanged += value;
                    SnapshotChangedSubscriberCount++;
                }
                remove
                {
                    _snapshotChanged -= value;
                    SnapshotChangedSubscriberCount--;
                }
            }

            public Task<FolderTreeSnapshot> GetSnapshotAsync(
                FolderTreeRequest request,
                CancellationToken cancellationToken
            )
            {
                LastRequest = request;
                return Task.FromResult(Snapshot);
            }

            public void PublishSnapshotChanged()
            {
                _snapshotChanged?.Invoke(
                    this,
                    new FolderTreeSnapshotChangedEventArgs(
                        Snapshot,
                        FolderTreeRefreshReason.ManualRefresh,
                        new[] { "store" }
                    )
                );
            }

            public void MarkStale(string storeId, FolderTreeRefreshReason reason) { }

            public void Dispose()
            {
                _snapshotChanged = null;
                SnapshotChangedSubscriberCount = 0;
            }
        }

        private sealed class FakeFilterViewer : IFilterOlFoldersViewer
        {
            public event FormClosedEventHandler FormClosed;

            public TreeListView TlvNotFiltered { get; } = new();

            public TreeListView TlvFiltered { get; } = new();

            public bool InvokeRequired { get; set; }

            public int CloseCount { get; private set; }

            public int InvokeCount { get; private set; }

            public FilterOlFoldersController Controller { get; private set; }

            public void SetController(FilterOlFoldersController controller)
            {
                Controller = controller;
            }

            public void Show() { }

            public void Close()
            {
                CloseCount++;
                FormClosed?.Invoke(this, new FormClosedEventArgs(CloseReason.UserClosing));
            }

            public object Invoke(Delegate method)
            {
                InvokeCount++;
                return method.DynamicInvoke();
            }

            public void Dispose()
            {
                TlvNotFiltered.Dispose();
                TlvFiltered.Dispose();
            }
        }
    }
}
