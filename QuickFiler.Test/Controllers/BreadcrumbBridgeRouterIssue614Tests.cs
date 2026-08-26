using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #614 filing-boundary regression tests for <see cref="BreadcrumbBridgeRouter"/>
    /// (defects D1, D2, D3, D9). Every collaborator is a Moq seam; no test creates a form,
    /// WebView2 control, Outlook COM object, message pump, temporary file, or wall-clock wait.
    /// </summary>
    [TestClass]
    public class BreadcrumbBridgeRouterIssue614Tests
    {
        private const string MailboxRoot = @"\\mailbox@example.com";
        private const string ArchiveRoot = @"\\mailbox@example.com\Archive";
        private const string CrossStoreBranch = @"\\other@example.org\Archive\Clients";
        private const string RelativeTarget = @"Clients\North";
        private const string HierarchyTarget = @"\\mailbox@example.com\Archive\Clients\North";
        private const string AncestorBranch = @"\\mailbox@example.com\Archive\Clients";

        private Mock<IFolderHierarchyProvider> _provider;
        private Mock<IBreadcrumbWebHost> _host;
        private BreadcrumbBridgeRouter _router;
        private MemoryAppender _appender;

        [TestInitialize]
        public void Setup()
        {
            _provider = new Mock<IFolderHierarchyProvider>();
            _host = new Mock<IBreadcrumbWebHost>();
            _host.SetupGet(host => host.IsCoreInitialized).Returns(true);
            _router = new BreadcrumbBridgeRouter(
                _provider.Object,
                _host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(_host.Object)
            );
            _appender = AttachMemoryAppender(typeof(BreadcrumbBridgeRouter));
        }

        [TestCleanup]
        public void Cleanup()
        {
            DetachMemoryAppender(typeof(BreadcrumbBridgeRouter), _appender);
        }

        [TestMethod]
        public void SegmentActivate_StoreRootAncestor_LeavesSelectionUnchangedAndDiagnoses()
        {
            // Arrange: a prior valid selection establishes the value that must survive (AC20).
            BindStandardChain();
            Inbound(RowSelected(0));
            _router.SelectedFolderPath.Should().Be(RelativeTarget);
            _appender.Clear();

            // Act: activate segment 0, the mailbox store root ABOVE the bound archive root.
            Inbound(SegmentActivate(0));

            // Assert: unchanged, non-null, and diagnosed without leaking the path.
            _router.SelectedFolderPath.Should().Be(RelativeTarget);
            _router.SelectedFolderPath.Should().NotBeNull();
            RenderedMessages().Should().ContainSingle(message => message.Contains("rejected"));
            RenderedMessages().Should().NotContain(message => message.Contains("mailbox@"));
        }

        [TestMethod]
        public void SegmentActivate_CrossStoreAncestor_LeavesSelectionUnchangedAndDiagnoses()
        {
            // Arrange: the resolved chain is rooted in a DIFFERENT store than the bound root.
            BindChain(
                RelativeTarget,
                HierarchyTarget,
                Segment(@"\\other@example.org", "Other Mailbox", true),
                Segment(CrossStoreBranch, "Clients", true),
                Segment(HierarchyTarget, "North", false)
            );
            Inbound(RowSelected(0));
            _appender.Clear();

            // Act
            Inbound(SegmentActivate(1));

            // Assert
            _router.SelectedFolderPath.Should().Be(RelativeTarget);
            RenderedMessages().Should().ContainSingle(message => message.Contains("rejected"));
        }

        [TestMethod]
        public void SegmentActivate_ArchiveRootExactly_IsTreatedAsNonSelection()
        {
            // Arrange
            BindStandardChain();
            Inbound(RowSelected(0));
            _appender.Clear();

            // Act: activate the archive root itself, which is not a filing destination (D9).
            Inbound(SegmentActivate(1));

            // Assert
            _router.SelectedFolderPath.Should().Be(RelativeTarget);
            RenderedMessages().Should().ContainSingle(message => message.Contains("rejected"));
        }

        [TestMethod]
        public void SegmentActivate_UnderRootAncestor_SetsTheRelativeStem()
        {
            // Arrange
            BindStandardChain();

            // Act: activate the valid under-root ancestor segment.
            Inbound(SegmentActivate(2));

            // Assert
            _router.SelectedFolderPath.Should().Be("Clients");
        }

        [TestMethod]
        public void RenderedChildActivate_UnderRootChild_SetsTheRelativeStem()
        {
            // Arrange: expand the active ancestor so its rendered children are activatable.
            BindStandardChain();
            _provider
                .Setup(provider =>
                    provider.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(new[] { Segment(AncestorBranch + @"\South", "South", false) });
            Inbound(SegmentActivate(2));
            Inbound(LeafExpandToggle(0));

            // Act
            Inbound(ChildActivate(0));

            // Assert
            _router.SelectedFolderPath.Should().Be(@"Clients\South");
        }

        [TestMethod]
        public void SegmentActivate_LeafSegment_RemainsNonActivatable()
        {
            // Arrange
            BindStandardChain();
            Inbound(RowSelected(0));
            _appender.Clear();

            // Act: the leaf segment index is never activatable (BreadcrumbRow.cs unchanged).
            Inbound(SegmentActivate(3));

            // Assert: rejected by the row itself, so the selection is untouched.
            _router.SelectedFolderPath.Should().Be(RelativeTarget);
            RenderedMessages()
                .Should()
                .ContainSingle(message => message.Contains("activation rejected"));
        }

        [TestMethod]
        public void RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath()
        {
            // Arrange: the presented suggestion is itself a full Outlook path outside the root.
            BindChain(
                CrossStoreBranch,
                CrossStoreBranch,
                Segment(@"\\other@example.org", "Other Mailbox", true),
                Segment(CrossStoreBranch, "Clients", false)
            );

            // Act
            Inbound(RowSelected(0));

            // Assert: the out-of-root full path is never stored (D3).
            _router.SelectedFolderPath.Should().BeNull();
            _router.SelectedFolderPath.Should().NotBe(CrossStoreBranch);
        }

        [TestMethod]
        public void SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode()
        {
            // Arrange: the public no-root overload is used outside the EFC filing chain, where
            // presented values are already the filing targets. That mode is deliberately
            // unchanged by #614; the filing boundary protects filing independently.
            const string fullTarget = @"\Archive\Clients\North";
            FolderTreeNodeKey key = Key(fullTarget);
            _provider
                .Setup(provider =>
                    provider.ResolveLeafKeyAsync(fullTarget, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(key);
            _provider
                .Setup(provider =>
                    provider.GetAncestorChainAsync(key, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    new[]
                    {
                        Segment(@"\Archive", "Archive", true),
                        Segment(@"\Archive\Clients", "Clients", true),
                        Segment(fullTarget, "North", false),
                    }
                );
            _router
                .BindRowsAsync(new[] { fullTarget }, new FolderScore[0], CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            // Act
            Inbound(SegmentActivate(0));

            // Assert: the full hierarchy path passes through verbatim in no-root mode.
            _router.SelectedFolderPath.Should().Be(@"\Archive");
        }

        private void BindStandardChain()
        {
            BindChain(
                RelativeTarget,
                HierarchyTarget,
                Segment(MailboxRoot, "Mailbox", true),
                Segment(ArchiveRoot, "Archive", true),
                Segment(AncestorBranch, "Clients", true),
                Segment(HierarchyTarget, "North", false)
            );
        }

        private void BindChain(
            string presentedTarget,
            string hierarchyPath,
            params FolderBreadcrumbSegment[] chain
        )
        {
            FolderTreeNodeKey key = Key(hierarchyPath);
            _provider
                .Setup(provider =>
                    provider.ResolveLeafKeyAsync(hierarchyPath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(key);
            _provider
                .Setup(provider =>
                    provider.GetAncestorChainAsync(key, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(chain);
            _router
                .BindRowsAsync(
                    new[] { presentedTarget },
                    new[] { new FolderScore(presentedTarget, 730, 0.73) },
                    ArchiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
        }

        private void Inbound(string json)
        {
            _router.ProcessInboundAsync(json).GetAwaiter().GetResult();
        }

        private static string SegmentActivate(int segmentIndex)
        {
            return "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":"
                + segmentIndex
                + "}";
        }

        private static string ChildActivate(int childIndex)
        {
            return "{\"type\":\"renderedChildActivate\",\"rowId\":\"row-0\",\"childIndex\":"
                + childIndex
                + "}";
        }

        private static string LeafExpandToggle(int rowIndex)
        {
            return "{\"type\":\"leafExpandToggle\",\"rowId\":\"row-" + rowIndex + "\"}";
        }

        private static string RowSelected(int rowIndex)
        {
            return "{\"type\":\"rowSelected\",\"rowId\":\"row-" + rowIndex + "\"}";
        }

        private string[] RenderedMessages()
        {
            return _appender.GetEvents().Select(entry => entry.RenderedMessage).ToArray();
        }

        private static FolderTreeNodeKey Key(string path)
        {
            return new FolderTreeNodeKey("archive-store", path, path);
        }

        private static FolderBreadcrumbSegment Segment(string path, string name, bool hasChildren)
        {
            return new FolderBreadcrumbSegment(Key(path), name, path, hasChildren);
        }

        private static MemoryAppender AttachMemoryAppender(Type targetType)
        {
            var appender = new MemoryAppender();
            appender.ActivateOptions();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = (Logger)hierarchy.GetLogger(targetType.FullName);
            logger.Level = log4net.Core.Level.Debug;
            logger.AddAppender(appender);
            logger.Repository.Configured = true;
            return appender;
        }

        private static void DetachMemoryAppender(Type targetType, MemoryAppender appender)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = (Logger)hierarchy.GetLogger(targetType.FullName);
            logger.RemoveAppender(appender);
        }
    }
}
