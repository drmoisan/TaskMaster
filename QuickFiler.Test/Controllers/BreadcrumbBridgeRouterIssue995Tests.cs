using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>Issue 995 confirmation behavior through in-memory bridge collaborators.</summary>
    [TestClass]
    public class BreadcrumbBridgeRouterIssue995Tests
    {
        private const string Root = @"\Archive";
        private const string Target = @"Clients\North";
        private BreadcrumbBridgeRouter router;
        private int confirmations;

        [TestInitialize]
        public void Setup()
        {
            var provider = new Mock<IFolderHierarchyProvider>();
            var host = new Mock<IBreadcrumbWebHost>();
            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync((string path, CancellationToken token) => Key(path));
            provider
                .Setup(value =>
                    value.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new[]
                    {
                        Segment(Root, "Archive", true),
                        Segment(Root + @"\Clients", "Clients", true),
                        Segment(Root + @"\" + Target, "North", true),
                    }
                );
            provider
                .Setup(value =>
                    value.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(new[] { Segment(Root + @"\" + Target + @"\Child", "Child", false) });
            router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );
            router.ConfirmationRequested += (sender, args) => confirmations++;
            confirmations = 0;
        }

        [DataTestMethod]
        [DataRow(Target)]
        [DataRow("A")]
        [DataRow("Trash to Delete")]
        public async Task ValidSelectedRow_ConfirmsExactlyOnce(string target)
        {
            await Bind(target);
            await Inbound("rowSelected", "row-1");
            int selectionChanges = 0;
            router.SelectedFolderPathChanged += (sender, path) => selectionChanges++;

            await Inbound("rowConfirmed", "row-1");

            confirmations.Should().Be(1);
            router.SelectedFolderPath.Should().Be(target);
            selectionChanges.Should().Be(0, "confirmation must retain the current destination");
        }

        [TestMethod]
        public async Task RowSelected_DoesNotConfirm()
        {
            await Bind(Target);

            await Inbound("rowSelected", "row-1");

            confirmations.Should().Be(0);
            router.SelectedFolderPath.Should().Be(Target);
        }

        [TestMethod]
        public async Task MissingUnknownBannerOrMismatchedRow_DoesNotConfirm()
        {
            await Bind(Target, "Other");
            await Inbound("rowConfirmed", "row-1");
            await Inbound("rowSelected", "row-1");
            await Inbound("rowConfirmed", "missing");
            await Inbound("rowConfirmed", "row-0");
            await Inbound("rowConfirmed", "row-2");
            Func<Task> missingId = () => router.ProcessInboundAsync("{\"type\":\"rowConfirmed\"}");

            await missingId.Should().ThrowAsync<BreadcrumbMessageException>();
            confirmations.Should().Be(0);
            router.SelectedFolderPath.Should().Be(Target);
        }

        [DataTestMethod]
        [DataRow("===invalid")]
        [DataRow(@"\Other\Outside")]
        public async Task InvalidDestination_DoesNotConfirm(string target)
        {
            await Bind(target);
            await Inbound("rowSelected", "row-1");

            await Inbound("rowConfirmed", "row-1");

            confirmations.Should().Be(0);
        }

        [TestMethod]
        public async Task RebindThenConfirm_DoesNotUseStaleSelection()
        {
            await Bind(Target);
            await Inbound("rowSelected", "row-1");
            await Bind("Replacement");

            await Inbound("rowConfirmed", "row-1");

            confirmations.Should().Be(0);
            router.SelectedFolderPath.Should().BeNull();
        }

        [TestMethod]
        public async Task ConfirmAfterSegmentActivation_PreservesTarget()
        {
            await BindHierarchy();
            await router.ProcessInboundAsync(
                "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":1}"
            );

            await Inbound("rowConfirmed", "row-0");

            confirmations.Should().Be(1);
            router.SelectedFolderPath.Should().Be("Clients");
        }

        [TestMethod]
        public async Task ConfirmAfterChildActivation_PreservesTarget()
        {
            await BindHierarchy();
            await Inbound("rowSelected", "row-0");
            await Inbound("leafExpandToggle", "row-0");
            await router.ProcessInboundAsync(
                "{\"type\":\"renderedChildActivate\",\"rowId\":\"row-0\",\"childIndex\":0}"
            );

            await Inbound("rowConfirmed", "row-0");

            confirmations.Should().Be(1);
            router.SelectedFolderPath.Should().Be(Target + @"\Child");
        }

        private Task Bind(params string[] targets)
        {
            var rows = new List<string> { "==== SUGGESTIONS ====" };
            rows.AddRange(targets);
            return router.BindRowsAsync(rows, Array.Empty<FolderScore>(), CancellationToken.None);
        }

        private Task BindHierarchy() =>
            router.BindRowsAsync(
                new[] { Target },
                Array.Empty<FolderScore>(),
                Root,
                CancellationToken.None
            );

        private Task Inbound(string type, string rowId) =>
            router.ProcessInboundAsync("{\"type\":\"" + type + "\",\"rowId\":\"" + rowId + "\"}");

        private static FolderTreeNodeKey Key(string path) =>
            new FolderTreeNodeKey("store", path, path);

        private static FolderBreadcrumbSegment Segment(
            string path,
            string name,
            bool hasChildren
        ) => new FolderBreadcrumbSegment(Key(path), name, path, hasChildren);
    }
}
