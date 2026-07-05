using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster.Properties;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Unit tests for the high-confidence ribbon helpers on <see cref="RibbonController"/>
    /// (Issue #169). RibbonController reads/writes the high-confidence settings through its
    /// concrete <see cref="ApplicationGlobals"/> (<c>Globals</c>). To exercise the helpers without
    /// constructing the full Outlook-backed globals, an uninitialized <see cref="ApplicationGlobals"/>
    /// is created and its <c>_quickFilerSettings</c> field is set to a real
    /// <see cref="AppQuickFilerSettings"/>; that settings object round-trips through
    /// <see cref="Settings.Default"/>, which is snapshotted in <see cref="TestInitialize"/> and
    /// restored in <see cref="TestCleanup"/> so the tests are independent and leave no machine
    /// state mutated.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class RibbonControllerTests
    {
        private bool _originalModeEnabled;
        private double _originalThreshold;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalModeEnabled = Settings.Default.HighConfidenceModeEnabled;
            _originalThreshold = Settings.Default.HighConfidenceThreshold;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Settings.Default.HighConfidenceModeEnabled = _originalModeEnabled;
            Settings.Default.HighConfidenceThreshold = _originalThreshold;
        }

        /// <summary>
        /// Builds a RibbonController whose Globals is an uninitialized ApplicationGlobals carrying a
        /// real AppQuickFilerSettings, so the high-confidence helpers read/write Settings.Default.
        /// </summary>
        private static RibbonController CreateController()
        {
            var globals = (ApplicationGlobals)
                FormatterServices.GetUninitializedObject(typeof(ApplicationGlobals));
            typeof(ApplicationGlobals)
                .GetField("_quickFilerSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(globals, new AppQuickFilerSettings());

            var controller = new RibbonController();
            typeof(RibbonController)
                .GetProperty(
                    "Globals",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
                )
                .SetValue(controller, globals);

            return controller;
        }

        [TestMethod]
        public void IsHighConfidenceModeActive_ReturnsStoredValue()
        {
            // Arrange
            Settings.Default.HighConfidenceModeEnabled = true;
            var controller = CreateController();

            // Act
            var result = controller.IsHighConfidenceModeActive();

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void ToggleHighConfidenceMode_FlipsStoredValue()
        {
            // Arrange
            Settings.Default.HighConfidenceModeEnabled = false;
            var controller = CreateController();

            // Act
            controller.ToggleHighConfidenceMode();

            // Assert
            controller.IsHighConfidenceModeActive().Should().BeTrue();
        }

        [TestMethod]
        public void SetHighConfidenceModeForLaunch_True_EnablesMode()
        {
            // Arrange: start from the disabled state.
            Settings.Default.HighConfidenceModeEnabled = false;
            var controller = CreateController();

            // Act
            controller.SetHighConfidenceModeForLaunch(true);

            // Assert: the high-confidence launch path enables the mode.
            controller.IsHighConfidenceModeActive().Should().BeTrue();
        }

        [TestMethod]
        public void StandardLaunchAfterHighConfidenceLaunch_DoesNotEnableMode()
        {
            // Arrange: simulate a prior high-confidence launch having enabled the mode.
            var controller = CreateController();
            controller.SetHighConfidenceModeForLaunch(true);

            // Act: a subsequent standard launch (or release) resets the launch-scoped flag.
            controller.SetHighConfidenceModeForLaunch(false);

            // Assert: the standard entry point does not inherit high-confidence mode, so it
            // never filters (AC6).
            controller.IsHighConfidenceModeActive().Should().BeFalse();
        }

        [TestMethod]
        public void GetHighConfidenceThresholdText_ReturnsPercentageForm()
        {
            // Arrange: stored probability 0.9 should render as "90".
            Settings.Default.HighConfidenceThreshold = 0.9;
            var controller = CreateController();

            // Act
            var result = controller.GetHighConfidenceThresholdText();

            // Assert
            result.Should().Be("90");
        }

        [TestMethod]
        public void SetHighConfidenceThresholdText_WithValidPercentage_PersistsProbability()
        {
            // Arrange
            var controller = CreateController();

            // Act
            controller.SetHighConfidenceThresholdText("75");

            // Assert
            Settings.Default.HighConfidenceThreshold.Should().Be(0.75);
        }

        [TestMethod]
        public void SetHighConfidenceThresholdText_WithNonNumericInput_LeavesValueUnchanged()
        {
            // Arrange
            Settings.Default.HighConfidenceThreshold = 0.9;
            var controller = CreateController();

            // Act
            controller.SetHighConfidenceThresholdText("not-a-number");

            // Assert
            Settings.Default.HighConfidenceThreshold.Should().Be(0.9);
        }

        [TestMethod]
        public void SetHighConfidenceThresholdText_WithOutOfRangeInput_LeavesValueUnchanged()
        {
            // Arrange: 150% is out of the [0, 100] range.
            Settings.Default.HighConfidenceThreshold = 0.9;
            var controller = CreateController();

            // Act
            controller.SetHighConfidenceThresholdText("150");

            // Assert
            Settings.Default.HighConfidenceThreshold.Should().Be(0.9);
        }

        [TestMethod]
        public async Task GetFolderTreeSnapshotAsync_UsesInjectedFolderTreeService()
        {
            var expected = new FolderTreeSnapshot(
                System.Array.Empty<FolderTreeNodeKey>(),
                System.Array.Empty<FolderTreeSnapshotNode>()
            );
            var service = new Mock<IOutlookFolderTreeService>(MockBehavior.Strict);
            service
                .Setup(x =>
                    x.GetSnapshotAsync(
                        It.Is<FolderTreeRequest>(request =>
                            request.IsAllStores && request.AllowStaleSnapshot
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(expected);
            var controller = new TestableFolderTreeRibbonController(service.Object);

            var result = await controller.GetFolderTreeSnapshotAsync(
                FolderTreeRequest.AllStores(allowStaleSnapshot: true)
            );

            result.Should().BeSameAs(expected);
            service.VerifyAll();
        }

        [TestMethod]
        public async Task GetFolderTreeSnapshotAsync_WhenFolderStoreMissing_RequestsAllStores()
        {
            FolderTreeRequest captured = null;
            var expected = new FolderTreeSnapshot(
                System.Array.Empty<FolderTreeNodeKey>(),
                System.Array.Empty<FolderTreeSnapshotNode>()
            );
            var service = new Mock<IOutlookFolderTreeService>(MockBehavior.Strict);
            service
                .Setup(x =>
                    x.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .Callback<FolderTreeRequest, CancellationToken>((request, _) => captured = request)
                .ReturnsAsync(expected);
            var controller = new TestableFolderTreeRibbonController(service.Object);
            var folder = CreateFolder(null, "\\Archive");

            var result = await InvokeGetFolderTreeSnapshotAsync(controller, folder.Object);

            result.Should().BeSameAs(expected);
            captured.IsAllStores.Should().BeTrue();
            captured.AllowStaleSnapshot.Should().BeTrue();
        }

        [TestMethod]
        public void CompareFolderSnapshots_UsesScopedCachedSnapshotViews()
        {
            var current = CreateComparisonSnapshot("current", "CurrentOnly");
            var other = CreateComparisonSnapshot("other", "OtherOnly");
            var folder = CreateFolder("store", "\\Archive");

            var result = InvokeCompareFolderSnapshots(current, folder.Object, other, folder.Object);
            var comparedPaths = GetComparedPaths(result);

            comparedPaths.Should().Contain("Archive\\CurrentOnly");
            comparedPaths.Should().Contain("Archive\\OtherOnly");
            comparedPaths.Should().NotContain("External");
        }

        [TestMethod]
        public void CompareFolderSnapshots_WhenFolderRootMissing_ComparesFullSnapshot()
        {
            var current = CreateComparisonSnapshot("current", "CurrentOnly");
            var other = CreateComparisonSnapshot("other", "OtherOnly");
            var folder = CreateFolder("store", "\\Missing");

            var result = InvokeCompareFolderSnapshots(current, folder.Object, other, folder.Object);
            var comparedPaths = GetComparedPaths(result);

            comparedPaths.Should().Contain("External");
        }

        [TestMethod]
        public void GetStats_WithFolders_ReturnsFormattedSizeAndCount()
        {
            var controller = CreateController();
            var nodes = new List<TreeNode<FolderWrapper>>
            {
                CreateTreeNode("Inbox", 3, 1536, "Inbox"),
                CreateTreeNode("Sent", 2, 512, "Sent"),
            };

            var result = controller.GetStats(nodes);

            result.count.Should().Be(5);
            result.size.Should().Be("2.0 KB (2,048)");
        }

        [TestMethod]
        public void GetStats_WhenNodesMissing_ReturnsZero()
        {
            var controller = CreateController();

            var nullResult = controller.GetStats(null);
            var emptyResult = controller.GetStats(new List<TreeNode<FolderWrapper>>());

            nullResult.Should().Be(("0", 0));
            emptyResult.Should().Be(("0", 0));
        }

        [TestMethod]
        public void RibbonFolderOperations_DoNotConstructThrowawayFolderTrees()
        {
            var ribbonFolderSource = File.ReadAllText(
                Path.Combine(
                    FindRepositoryRoot(),
                    "TaskMaster",
                    "Ribbon",
                    "RibbonController.FolderTree.cs"
                )
            );

            ribbonFolderSource.Should().NotContain("new FolderTree(");
            ribbonFolderSource.Should().NotContain("FolderTree.CreateAsync");
            ribbonFolderSource.Should().NotContain("Task.Run(");
        }

        private static object InvokeCompareFolderSnapshots(
            FolderTreeSnapshot current,
            MAPIFolder currentFolder,
            FolderTreeSnapshot other,
            MAPIFolder otherFolder
        )
        {
            return typeof(RibbonController)
                .GetMethod("CompareFolderSnapshots", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { current, currentFolder, other, otherFolder });
        }

        private static async Task<FolderTreeSnapshot> InvokeGetFolderTreeSnapshotAsync(
            RibbonController controller,
            MAPIFolder folder
        )
        {
            var task =
                (Task<FolderTreeSnapshot>)
                    typeof(RibbonController)
                        .GetMethod(
                            "GetFolderTreeSnapshotAsync",
                            BindingFlags.NonPublic | BindingFlags.Instance,
                            null,
                            new[] { typeof(MAPIFolder) },
                            null
                        )
                        .Invoke(controller, new object[] { folder });
            return await task;
        }

        private static IReadOnlyList<string> GetNodePaths(object result, string fieldName)
        {
            var nodes = (IEnumerable)result.GetType().GetField(fieldName).GetValue(result);
            return nodes.Cast<object>().Select(GetRelativePath).ToList();
        }

        private static IReadOnlyList<string> GetComparedPaths(object result)
        {
            return new[] { "Item1", "Item2", "Item3", "Item4", "Item5" }
                .SelectMany(fieldName => GetNodePaths(result, fieldName))
                .ToList();
        }

        private static string GetRelativePath(object node)
        {
            var value = node.GetType().GetProperty("Value").GetValue(node);
            return (string)value.GetType().GetProperty("RelativePath").GetValue(value);
        }

        private static Mock<MAPIFolder> CreateFolder(string storeId, string folderPath)
        {
            var folder = new Mock<MAPIFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.StoreID).Returns(storeId);
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            return folder;
        }

        private static TreeNode<FolderWrapper> CreateTreeNode(
            string name,
            int itemCount,
            long folderSize,
            string relativePath
        )
        {
            return new(new FolderWrapper(false, itemCount, folderSize, name, relativePath));
        }

        private static FolderTreeSnapshot CreateComparisonSnapshot(
            string childEntryId,
            string childName
        )
        {
            var archiveKey = new FolderTreeNodeKey("store", "archive", "\\Archive");
            var childKey = new FolderTreeNodeKey("store", childEntryId, $"\\Archive\\{childName}");
            var externalKey = new FolderTreeNodeKey("store", "external", "\\External");
            return new(
                new[] { archiveKey, externalKey },
                new[]
                {
                    CreateNode(archiveKey, "Archive", null, "Archive", childKey),
                    CreateNode(childKey, childName, archiveKey, $"Archive\\{childName}"),
                    CreateNode(externalKey, "External", null, "External"),
                }
            );
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

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(
                Path.GetDirectoryName(typeof(RibbonController).Assembly.Location)
            );
            while (
                directory is not null
                && !File.Exists(Path.Combine(directory.FullName, "TaskMaster.sln"))
            )
            {
                directory = directory.Parent;
            }

            return directory?.FullName
                ?? throw new InvalidOperationException("Could not locate repository root.");
        }

        private sealed class TestableFolderTreeRibbonController : RibbonController
        {
            private readonly IOutlookFolderTreeService _folderTreeService;

            internal TestableFolderTreeRibbonController(IOutlookFolderTreeService folderTreeService)
            {
                _folderTreeService = folderTreeService;
            }

            protected internal override IOutlookFolderTreeService FolderTreeService =>
                _folderTreeService;
        }
    }
}
