using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.FolderRemap;
using UtilitiesCS.ReusableTypeClasses;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Second partial file for FolderRemapController unit tests.
    ///
    /// Purpose:
    ///     Covers HandleModelDropped (Background + MappedTo branch), MakeCheckedStatePutter,
    ///     and the non-capturing delegate field bodies (GetMappedCheckedState,
    ///     PutMappedCheckedState, GetCheckedState) via the compiler-generated companion class.
    ///
    /// Flow:
    ///     1. Behavioural tests (P14-T15 through P14-T18) bypass COMs via reflection.
    ///     2. Companion-class tests (P14-T19 through P14-T25) invoke the static methods
    ///        on the <>c nested type so the delegate body source lines register as covered.
    /// </summary>
    public partial class FolderRemapController_Tests
    {
        // ---------------------------------------------------------------------------
        // Companion-class helpers: invoke non-capturing delegate field bodies via <>c
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Invokes a compiler-generated static getter method (CheckState ← object) from the
        /// <>c companion class. Tries each matching method and returns the first that does not
        /// throw an InvalidCastException, allowing OlFolderRemap and TreeNode inputs to route
        /// to the correct underlying delegate body.
        /// </summary>
        private static CheckState InvokeCompanionGetter(object rowObject)
        {
            // Locate the <>c nested type that holds static non-capturing lambda bodies.
            var compType = typeof(FolderRemapController)
                .GetNestedTypes(BindingFlags.NonPublic)
                .First(t => t.Name == "<>c");
            var singleton = compType
                .GetField("<>9", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);

            // Iterate all (object) → CheckState methods and return the first that accepts the input type.
            foreach (var m in compType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var ps = m.GetParameters();
                if (m.ReturnType != typeof(CheckState) || ps.Length != 1)
                    continue;
                try
                {
                    return (CheckState)m.Invoke(singleton, new[] { rowObject });
                }
                catch (TargetInvocationException ex)
                    when (ex.InnerException is InvalidCastException) { }
            }

            throw new InvalidOperationException(
                "No companion getter method matched the input type."
            );
        }

        /// <summary>
        /// Invokes the single compiler-generated static PutterDelegate body (CheckState ← object,
        /// CheckState) from the <>c companion class.
        /// </summary>
        private static CheckState InvokeCompanionPutter(object rowObject, CheckState newValue)
        {
            var compType = typeof(FolderRemapController)
                .GetNestedTypes(BindingFlags.NonPublic)
                .First(t => t.Name == "<>c");
            var singleton = compType
                .GetField("<>9", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);
            var method = compType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.ReturnType == typeof(CheckState) && x.GetParameters().Length == 2);
            return (CheckState)method.Invoke(singleton, new object[] { rowObject, newValue });
        }

        // ---------------------------------------------------------------------------
        // P14-T15: HandleModelDropped — Background location is a no-op
        // ---------------------------------------------------------------------------

        /// <summary>Verifies the Background switch case completes without mutating Mappings2.</summary>
        [TestMethod]
        public void HandleModelDropped_WithBackgroundLocation_DoesNotAlterMappings()
        {
            var controller = CreateController(
                null,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                new Mock<IApplicationGlobals>().Object
            );
            var args = new ModelDropEventArgs();
            args.DropTargetLocation = DropTargetLocation.Background;

            Action act = () => controller.HandleModelDropped(null, args);

            // Background case is a no-op; Mappings2 stays empty and no exception is raised.
            act.Should().NotThrow();
            controller.Mappings2.Should().BeEmpty();
        }

        // ---------------------------------------------------------------------------
        // P14-T16: HandleModelDropped item drop — target with existing MappedTo
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies MoveObjectsToChildren redirects source to target's existing MappedTo
        /// when target.Value.MappedTo is already populated.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void HandleModelDropped_WhenTargetHasMappedTo_SetsSourceMappedToTargetsMappedTo()
        {
            var finalDest = new OlFolderRemap();
            var targetRemap = new OlFolderRemap();
            targetRemap.MappedTo = finalDest;
            var sourceRemap = new OlFolderRemap();
            var sourceNode = new TreeNode<OlFolderRemap>(sourceRemap);
            var targetNode = new TreeNode<OlFolderRemap>(targetRemap);
            var viewer = new FolderRemapViewer();
            var remapTree = CreateRemapTree(
                new List<TreeNode<OlFolderRemap>> { sourceNode, targetNode }
            );
            var controller = CreateController(
                viewer,
                remapTree,
                new Mock<IApplicationGlobals>().Object
            );
            var args = new ModelDropEventArgs();
            typeof(ModelDropEventArgs)
                .GetField("targetModel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(args, targetNode);
            typeof(ModelDropEventArgs)
                .GetField("dragModels", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(args, new System.Collections.ArrayList { sourceNode });
            args.DropTargetLocation = DropTargetLocation.Item;

            controller.HandleModelDropped(null, args);

            // Source maps to target's existing MappedTo — the MappedTo-override branch (line 182).
            sourceRemap.MappedTo.Should().BeSameAs(finalDest);
        }

        // ---------------------------------------------------------------------------
        // P14-T17: MakeCheckedStatePutter — factory method returns a non-null delegate
        // ---------------------------------------------------------------------------

        /// <summary>Verifies MakeCheckedStatePutter() returns a callable CheckStatePutterDelegate.</summary>
        [STAThread]
        [TestMethod]
        public void MakeCheckedStatePutter_ReturnsDelegateInstance()
        {
            var viewer = new FolderRemapViewer();
            var controller = CreateController(
                viewer,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                new Mock<IApplicationGlobals>().Object
            );

            var putter = controller.MakeCheckedStatePutter();

            putter.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // P14-T18: MakeCheckedStatePutter delegate — Unchecked clears MappedTo
        // ---------------------------------------------------------------------------

        /// <summary>Verifies the Unchecked branch of the returned delegate sets MappedTo to null.</summary>
        [STAThread]
        [TestMethod]
        public void MakeCheckedStatePutter_Delegate_WhenUnchecked_ClearsMappedToAndReturnsUnchecked()
        {
            var viewer = new FolderRemapViewer();
            var remap = new OlFolderRemap();
            remap.MappedTo = new OlFolderRemap();
            var node = new TreeNode<OlFolderRemap>(remap);
            var controller = CreateController(
                viewer,
                CreateRemapTree(new List<TreeNode<OlFolderRemap>>()),
                new Mock<IApplicationGlobals>().Object
            );
            var putter = controller.MakeCheckedStatePutter();

            var result = putter(node, CheckState.Unchecked);

            remap.MappedTo.Should().BeNull();
            result.Should().Be(CheckState.Unchecked);
        }

        // ---------------------------------------------------------------------------
        // P14-T19: GetMappedCheckedState body — Checked path (MappedTo not null)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies GetMappedCheckedState returns Checked when the OlFolderRemap has a MappedTo.
        /// Invokes the delegate body via the compiler-generated <>c companion class.
        /// </summary>
        [TestMethod]
        public void GetMappedCheckedState_WhenRemapHasMappedTo_ReturnsChecked()
        {
            var remap = new OlFolderRemap();
            remap.MappedTo = new OlFolderRemap();

            var result = InvokeCompanionGetter(remap);

            result.Should().Be(CheckState.Checked);
        }

        // ---------------------------------------------------------------------------
        // P14-T20: GetMappedCheckedState body — Unchecked path (MappedTo null)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies GetMappedCheckedState returns Unchecked when MappedTo is null.
        /// </summary>
        [TestMethod]
        public void GetMappedCheckedState_WhenRemapHasNoMappedTo_ReturnsUnchecked()
        {
            var result = InvokeCompanionGetter(new OlFolderRemap());

            result.Should().Be(CheckState.Unchecked);
        }

        // ---------------------------------------------------------------------------
        // P14-T21: PutMappedCheckedState body — returns Checked when state is Checked
        // ---------------------------------------------------------------------------

        /// <summary>Verifies PutMappedCheckedState returns Checked when newValue is Checked.</summary>
        [TestMethod]
        public void PutMappedCheckedState_WhenNewValueIsChecked_ReturnsChecked()
        {
            var result = InvokeCompanionPutter(new OlFolderRemap(), CheckState.Checked);

            result.Should().Be(CheckState.Checked);
        }

        // ---------------------------------------------------------------------------
        // P14-T22: PutMappedCheckedState body — Unchecked clears MappedTo
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies PutMappedCheckedState sets MappedTo to null and returns Unchecked
        /// when newValue is Unchecked.
        /// </summary>
        [TestMethod]
        public void PutMappedCheckedState_WhenNewValueIsUnchecked_ClearsMappedToAndReturnsUnchecked()
        {
            var remap = new OlFolderRemap();
            remap.MappedTo = new OlFolderRemap();

            var result = InvokeCompanionPutter(remap, CheckState.Unchecked);

            remap.MappedTo.Should().BeNull();
            result.Should().Be(CheckState.Unchecked);
        }

        // ---------------------------------------------------------------------------
        // P14-T23: GetCheckedState body — Checked path (node.Value.MappedTo not null)
        // ---------------------------------------------------------------------------

        /// <summary>Verifies GetCheckedState returns Checked when the node's own Value.MappedTo is set.</summary>
        [TestMethod]
        public void GetCheckedState_WhenNodeValueHasMappedTo_ReturnsChecked()
        {
            var remap = new OlFolderRemap();
            remap.MappedTo = new OlFolderRemap();
            var node = new TreeNode<OlFolderRemap>(remap);

            var result = InvokeCompanionGetter(node);

            result.Should().Be(CheckState.Checked);
        }

        // ---------------------------------------------------------------------------
        // P14-T24: GetCheckedState body — Indeterminate path (descendant has MappedTo)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies GetCheckedState returns Indeterminate when the node itself has no MappedTo
        /// but a child's Value.MappedTo is set (descendant coverage via Flatten).
        /// </summary>
        [TestMethod]
        public void GetCheckedState_WhenChildHasMappedTo_ReturnsIndeterminate()
        {
            // Parent node has no MappedTo; child does.
            var parentRemap = new OlFolderRemap();
            var parentNode = new TreeNode<OlFolderRemap>(parentRemap);
            var childRemap = new OlFolderRemap();
            childRemap.MappedTo = new OlFolderRemap();
            var childNode = new TreeNode<OlFolderRemap>(childRemap);
            childNode.Parent = parentNode;
            parentNode.Children.Add(childNode);

            var result = InvokeCompanionGetter(parentNode);

            result.Should().Be(CheckState.Indeterminate);
        }

        // ---------------------------------------------------------------------------
        // P14-T25: GetCheckedState body — Unchecked path (no MappedTo anywhere)
        // ---------------------------------------------------------------------------

        /// <summary>Verifies GetCheckedState returns Unchecked when neither the node nor its descendants have MappedTo.</summary>
        [TestMethod]
        public void GetCheckedState_WhenNoneHaveMappedTo_ReturnsUnchecked()
        {
            var node = new TreeNode<OlFolderRemap>(new OlFolderRemap());

            var result = InvokeCompanionGetter(node);

            result.Should().Be(CheckState.Unchecked);
        }

        // ---------------------------------------------------------------------------
        // Constructor helpers for P14-T26
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Creates a mock Outlook Folder with the given path and an empty Folders
        /// collection.
        ///
        /// Purpose:
        ///     Satisfies OlFolderRemap constructor access to FolderPath, Name, and
        ///     Folders without requiring a live COM Outlook session.
        /// </summary>
        private static Mock<OutlookFolder> CreateMockOutlookFolder(string folderPath)
        {
            var mockFolders = CreateMockEmptyFoldersCollection();
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Name).Returns(folderPath.Split('\\').Last(s => s.Length > 0));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Folders).Returns(mockFolders.Object);
            return folder;
        }

        /// <summary>
        /// Creates a mock Outlook Folders collection with zero items.
        ///
        /// Purpose:
        ///     Provides a safe GetEnumerator (returns empty) and Count=0 so
        ///     FolderRemapTree.InitializeChildren iterates over nothing without COM access.
        /// </summary>
        private static Mock<OutlookFolders> CreateMockEmptyFoldersCollection()
        {
            var emptyList = new System.Collections.ArrayList();
            var mockFolders = new Mock<OutlookFolders>(MockBehavior.Strict);
            mockFolders.SetupGet(x => x.Count).Returns(0);
            mockFolders.Setup(x => x.GetEnumerator()).Returns(() => emptyList.GetEnumerator());
            return mockFolders;
        }

        // ---------------------------------------------------------------------------
        // P14-T26: Constructor test — covers lines 16-35 and field-init lines
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the constructor initializes all delegate fields and the remap
        /// tree when provided with a mocked Outlook root folder and an empty FolderRemap
        /// dictionary.
        ///
        /// Purpose:
        ///     Covers the 20 constructor-body lines (16-35) and 6 field-initialization
        ///     lines (192, 205-208, 222) that cannot be reached by tests which bypass the
        ///     constructor via FormatterServices.GetUninitializedObject.
        ///
        /// Flow:
        ///     1. Build mocked IOlObjects and IToDoObjects using strict Moq.
        ///     2. Call the real constructor.
        ///     3. Assert delegate properties are non-null and tree/mappings are populated.
        ///     4. Discard (close) the modeless viewer form.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Constructor_WithMockedOutlookFolder_InitializesAllFieldsAndDelegates()
        {
            // Arrange: build a mock Outlook root folder with no children
            var archiveRoot = CreateMockOutlookFolder("\\Archive");
            var mockOl = new Mock<IOlObjects>(MockBehavior.Strict);
            mockOl.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot.Object);

            var mockTd = new Mock<IToDoObjects>(MockBehavior.Strict);
            mockTd.SetupGet(x => x.FolderRemap).Returns(new ScoDictionary<string, string>());

            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            mockGlobals.SetupGet(x => x.Ol).Returns(mockOl.Object);
            mockGlobals.SetupGet(x => x.TD).Returns(mockTd.Object);

            // Real controls are needed so that delegate assignments can be verified.
            using var tlvOriginal = new TreeListView();
            using var olvMap = new FastObjectListView();

            var mockViewer = new Mock<IFolderRemapViewer>(MockBehavior.Strict);
            mockViewer.SetupGet(v => v.TlvOriginal).Returns(tlvOriginal);
            mockViewer.SetupGet(v => v.OlvMap).Returns(olvMap);
            mockViewer.Setup(v => v.SetController(It.IsAny<FolderRemapController>()));
            mockViewer.Setup(v => v.Refresh());
            mockViewer.Setup(v => v.Close());
            mockViewer.Setup(v => v.Dispose());

            // Act: invoke the internal constructor with the mock viewer — no real window opens
            var controller = new FolderRemapController(mockGlobals.Object, mockViewer.Object);

            // Assert: all delegate fields and tree properties are initialized
            controller.RemapTree.Should().NotBeNull();
            controller.Mappings2.Should().NotBeNull();
            controller.GetMappedCheckedState.Should().NotBeNull();
            controller.PutMappedCheckedState.Should().NotBeNull();
            controller.GetCheckedState.Should().NotBeNull();

            // Show() is never called on the mock — verified by MockBehavior.Strict.
            mockViewer.Verify(v => v.Show(), Times.Never);

            // Teardown
            controller.Discard();
        }
    }
}
