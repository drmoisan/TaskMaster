using System;
using System.Collections.Generic;
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
    [TestClass]
    public class BreadcrumbBridgeRouterIssue637Tests
    {
        private const string MailboxRoot = @"\\mailbox@example.com";
        private const string ArchiveRoot = @"\\mailbox@example.com\Archive";
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
        public void RowSelected_ArchiveRootExactFilingTarget_IsNotSelected()
        {
            BindRows(ArchiveRoot, ArchiveRoot);

            Inbound(RowSelected(0));

            _router.SelectedFolderPath.Should().BeNull();
        }

        [TestMethod]
        public void RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection()
        {
            BindRows(ArchiveRoot, @"Clients\North", ArchiveRoot);
            Inbound(RowSelected(0));

            Inbound(RowSelected(1));

            _router.SelectedFolderPath.Should().Be(@"Clients\North");
        }

        [TestMethod]
        public void RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem()
        {
            BindRows(ArchiveRoot, ArchiveRoot + @"\Clients\North");

            Inbound(RowSelected(0));

            _router.SelectedFolderPath.Should().Be(@"Clients\North");
        }

        [TestMethod]
        public void RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem()
        {
            const string configuredRoot = @"\\MAILBOX@EXAMPLE.COM\ARCHIVE\";
            const string target = @"\\mailbox@example.com\aRcHiVe\Clients\South";
            BindRows(configuredRoot, target);

            Inbound(RowSelected(0));

            _router.SelectedFolderPath.Should().Be(@"Clients\South");
        }

        [TestMethod]
        public void RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim()
        {
            BindRows(ArchiveRoot, @"Clients\North");

            Inbound(RowSelected(0));

            _router.SelectedFolderPath.Should().Be(@"Clients\North");
        }

        [TestMethod]
        public void RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim()
        {
            BindRows(ArchiveRoot, BreadcrumbRowBuilder.TrashRowText);

            Inbound(RowSelected(0));

            _router.SelectedFolderPath.Should().Be(BreadcrumbRowBuilder.TrashRowText);
        }

        [TestMethod]
        public void RowSelected_OutOfRootRootedTarget_IsStillRejected()
        {
            BindRows(ArchiveRoot, @"\\other@example.org\Archive\Clients");

            Inbound(RowSelected(0));

            _router.SelectedFolderPath.Should().BeNull();
        }

        [TestMethod]
        public void RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected()
        {
            BindRows(ArchiveRoot, @"\\mailbox@example.com\Archive2\Clients");

            Inbound(RowSelected(0));

            _router.SelectedFolderPath.Should().BeNull();
        }

        [TestMethod]
        public void RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim()
        {
            const string target = @"\Archive\Clients\North";
            BindRows(@"\", target);

            Inbound(RowSelected(0));

            _router.SelectedFolderPath.Should().Be(target);
        }

        [TestMethod]
        public void SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem()
        {
            BindRows(ArchiveRoot, ArchiveRoot + @"\Clients\North");

            _router.SelectFirstRow();

            _router.SelectedFolderPath.Should().Be(@"Clients\North");
        }

        private void BindRows(string archiveRoot, params string[] presentedRows)
        {
            foreach (string presentedTarget in presentedRows)
            {
                if (BreadcrumbRowBuilder.Classify(presentedTarget) != BreadcrumbRowKind.Suggestion)
                {
                    continue;
                }

                string hierarchyPath = ToHierarchyPath(presentedTarget, archiveRoot);
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
                    .ReturnsAsync(Chain(hierarchyPath, archiveRoot));
            }

            _router
                .BindRowsAsync(
                    presentedRows,
                    new[] { new FolderScore(presentedRows[0], 730, 0.73) },
                    archiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
        }

        private static string ToHierarchyPath(string presentedTarget, string archiveRoot)
        {
            string boundRoot = archiveRoot.TrimEnd('\\', '/');
            if (boundRoot.Length == 0 || ArchiveStemContract.IsFullOutlookPath(presentedTarget))
            {
                return presentedTarget;
            }

            return boundRoot + "\\" + presentedTarget;
        }

        private static IReadOnlyList<FolderBreadcrumbSegment> Chain(
            string hierarchyPath,
            string archiveRoot
        )
        {
            string boundRoot = archiveRoot.TrimEnd('\\', '/');
            var chain = new List<FolderBreadcrumbSegment>();
            if (boundRoot.Length != 0)
            {
                chain.Add(Segment(MailboxRoot, "Mailbox", true));
                chain.Add(Segment(boundRoot, "Archive", true));
            }

            chain.Add(Segment(hierarchyPath, "Target", false));
            return chain;
        }

        private void Inbound(string json)
        {
            _router.ProcessInboundAsync(json).GetAwaiter().GetResult();
        }

        private static string RowSelected(int rowIndex)
        {
            return "{\"type\":\"rowSelected\",\"rowId\":\"row-" + rowIndex + "\"}";
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
