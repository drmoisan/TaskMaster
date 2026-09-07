using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for the issue #799 seams on <see cref="OutlookFolderHierarchyProvider"/>: the
    /// archive-root chain trim (AC1, AC2) and the label-absence classification (AC7). The folder
    /// tree is a hand-built <see cref="FolderTreeSnapshot"/> returned by a Moq
    /// <see cref="IOutlookFolderTreeService"/>, and diagnostics are observed through the provider's
    /// injected error sink rather than by attaching a log4net appender, so no test mutates the
    /// process-global logger repository. No live Outlook process, COM, or temporary file is used.
    /// </summary>
    [TestClass]
    public sealed class OutlookFolderHierarchyProviderTrimTests
    {
        private const string StorePath = "\\\\Mailbox - User";
        private const string ArchivePath = "\\\\Mailbox - User\\Archive";
        private const string ProjectsPath = "\\\\Mailbox - User\\Archive\\Projects";
        private const string AlphaPath = "\\\\Mailbox - User\\Archive\\Projects\\Alpha";
        private const string InboxPath = "\\\\Mailbox - User\\Inbox";
        private const string InboxProjectsPath = "\\\\Mailbox - User\\Inbox\\Projects";
        private const string InboxAlphaPath = "\\\\Mailbox - User\\Inbox\\Projects\\Alpha";
        private const string PresentedStem = "Projects\\Alpha";
        private const string MissingStem = "Missing\\Folder";

        private static readonly FolderTreeNodeKey StoreKey = Key("store", StorePath);
        private static readonly FolderTreeNodeKey ArchiveKey = Key("archive", ArchivePath);
        private static readonly FolderTreeNodeKey ProjectsKey = Key("projects", ProjectsPath);
        private static readonly FolderTreeNodeKey AlphaKey = Key("alpha", AlphaPath);
        private static readonly FolderTreeNodeKey InboxKey = Key("inbox", InboxPath);
        private static readonly FolderTreeNodeKey InboxProjectsKey = Key(
            "inbox-projects",
            InboxProjectsPath
        );
        private static readonly FolderTreeNodeKey InboxAlphaKey = Key(
            "inbox-alpha",
            InboxAlphaPath
        );

        /// <summary>
        /// AC1: with a root accessor configured, the presented lineage begins at the first segment
        /// BELOW the archive root. Neither the store node nor the archive-root node may appear.
        /// </summary>
        [TestMethod]
        public async Task GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot()
        {
            // Arrange
            var provider = ProviderOver(ArchiveSnapshot(), () => ArchivePath);

            // Act
            var chain = await provider.GetAncestorChainAsync(AlphaKey, CancellationToken.None);

            // Assert
            chain.Select(s => s.FolderPath).Should().Equal(ProjectsPath, AlphaPath);
            chain
                .Select(s => s.FolderPath)
                .Should()
                .NotContain(StorePath, "the mailbox segment is never presented");
            chain
                .Select(s => s.FolderPath)
                .Should()
                .NotContain(ArchivePath, "the Archive segment is never presented");
        }

        /// <summary>
        /// Constructing the provider WITHOUT a root accessor is the effective off switch: the chain
        /// is returned exactly as the snapshot walk produced it.
        /// </summary>
        [TestMethod]
        public async Task GetAncestorChainAsync_WithoutRootAccessor_ReturnsTheUntrimmedChain()
        {
            // Arrange
            var provider = new OutlookFolderHierarchyProvider(
                ServiceReturning(ArchiveSnapshot()).Object
            );

            // Act
            var chain = await provider.GetAncestorChainAsync(AlphaKey, CancellationToken.None);

            // Assert
            chain
                .Select(s => s.FolderPath)
                .Should()
                .Equal(StorePath, ArchivePath, ProjectsPath, AlphaPath);
        }

        /// <summary>
        /// AC2: a resolved chain that does not pass through the archive root is logged exactly once
        /// and yields an empty segment list, which routes each surface into its existing
        /// single-segment fallback. The emitted text must name the archive root so the failure is
        /// diagnosable from the log alone.
        /// </summary>
        [TestMethod]
        public async Task GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty()
        {
            // Arrange
            var errors = new List<string>();
            var provider = ProviderOver(ArchiveSnapshot(), () => StorePath + "\\Elsewhere");
            provider.ErrorSink = message => errors.Add(message);

            // Act
            var chain = await provider.GetAncestorChainAsync(AlphaKey, CancellationToken.None);

            // Assert
            chain.Should().BeEmpty("the caller falls back to single-segment rendering");
            errors.Should().ContainSingle("the AC2 diagnostic is emitted exactly once");
            errors[0].Should().Contain(StorePath + "\\Elsewhere");
        }

        /// <summary>
        /// When the LEAF is the archive root itself there is nothing below it to render, so the
        /// same AC2 diagnostic and empty result apply.
        /// </summary>
        [TestMethod]
        public async Task GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty()
        {
            // Arrange
            var errors = new List<string>();
            var provider = ProviderOver(ArchiveSnapshot(), () => ArchivePath);
            provider.ErrorSink = message => errors.Add(message);

            // Act
            var chain = await provider.GetAncestorChainAsync(ArchiveKey, CancellationToken.None);

            // Assert
            chain.Should().BeEmpty();
            errors.Should().ContainSingle();
        }

        /// <summary>
        /// The accessor is a delegate precisely because the underlying archive-root property throws
        /// when the root is unresolvable. A throwing accessor means "no trim configured" and must
        /// not propagate, because two of the three construction sites are outside any try block.
        /// </summary>
        [TestMethod]
        public async Task GetAncestorChainAsync_RootAccessorThrows_DoesNotThrowAndReturnsTheUntrimmedChain()
        {
            // Arrange
            var provider = ProviderOver(
                ArchiveSnapshot(),
                () => throw new InvalidOperationException("archive root unresolvable")
            );

            // Act
            var chain = await provider.GetAncestorChainAsync(AlphaKey, CancellationToken.None);

            // Assert
            chain
                .Select(s => s.FolderPath)
                .Should()
                .Equal(StorePath, ArchivePath, ProjectsPath, AlphaPath);
        }

        /// <summary>
        /// AC7 logging half: an unresolvable label is reported once per label per provider
        /// instance, not once per render, and the label is classified as absent.
        /// </summary>
        [TestMethod]
        public async Task ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence()
        {
            // Arrange
            var errors = new List<string>();
            var provider = ProviderOver(ArchiveSnapshot(), () => ArchivePath);
            provider.ErrorSink = message => errors.Add(message);

            // Act
            await provider.ResolveLeafKeyAsync(MissingStem, CancellationToken.None);
            await provider.ResolveLeafKeyAsync(MissingStem, CancellationToken.None);

            // Assert
            errors
                .Should()
                .ContainSingle("the gate is once per label per session, not per render");
            provider.IsAbsentLabel(MissingStem).Should().BeTrue();
        }

        /// <summary>
        /// Decision D-B restricts the AC7 suppression signal to the ZERO-candidate cause. An
        /// ambiguous label is still logged, but it is not absent: the folder exists, and more than
        /// one candidate matched.
        /// </summary>
        [TestMethod]
        public async Task ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence()
        {
            // Arrange
            var errors = new List<string>();
            var provider = ProviderOver(DecoySnapshot(), () => ArchivePath);
            provider.ErrorSink = message => errors.Add(message);

            // Act
            var resolved = await provider.ResolveLeafKeyAsync(
                PresentedStem,
                CancellationToken.None
            );

            // Assert
            resolved.Should().BeNull("an ambiguous stem is never resolved to either candidate");
            errors.Should().ContainSingle();
            provider
                .IsAbsentLabel(PresentedStem)
                .Should()
                .BeFalse("ambiguity is not absence; the folder does exist");
        }

        /// <summary>
        /// The absence signal must RESET, or a label that becomes resolvable after a snapshot
        /// refresh would stay suppressed for the life of the viewer. The service returns a snapshot
        /// missing the leaf on the first call and the complete snapshot on the second.
        /// </summary>
        [TestMethod]
        public async Task ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport()
        {
            // Arrange
            var service = new Mock<IOutlookFolderTreeService>();
            service
                .SetupSequence(s =>
                    s.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(SnapshotWithoutLeaf())
                .ReturnsAsync(ArchiveSnapshot());
            var provider = new OutlookFolderHierarchyProvider(service.Object, () => ArchivePath);

            // Act
            await provider.ResolveLeafKeyAsync(PresentedStem, CancellationToken.None);
            bool absentBefore = provider.IsAbsentLabel(PresentedStem);
            var resolved = await provider.ResolveLeafKeyAsync(
                PresentedStem,
                CancellationToken.None
            );

            // Assert
            absentBefore.Should().BeTrue("the first snapshot had no node for the label");
            resolved.Should().Be(AlphaKey);
            provider
                .IsAbsentLabel(PresentedStem)
                .Should()
                .BeFalse("the label resolved, so the suppression signal must clear");
        }

        private static OutlookFolderHierarchyProvider ProviderOver(
            FolderTreeSnapshot snapshot,
            Func<string> archiveRootAccessor
        )
        {
            return new OutlookFolderHierarchyProvider(
                ServiceReturning(snapshot).Object,
                archiveRootAccessor
            );
        }

        private static Mock<IOutlookFolderTreeService> ServiceReturning(FolderTreeSnapshot snapshot)
        {
            var service = new Mock<IOutlookFolderTreeService>();
            service
                .Setup(s =>
                    s.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(snapshot);
            return service;
        }

        /// <summary>Store, Archive, Projects, Alpha: a store-rooted three-level Archive chain.</summary>
        private static FolderTreeSnapshot ArchiveSnapshot()
        {
            return new FolderTreeSnapshot(
                new[] { StoreKey },
                new[]
                {
                    Node(StoreKey, "Mailbox - User", null, ArchiveKey),
                    Node(ArchiveKey, "Archive", StoreKey, ProjectsKey),
                    Node(ProjectsKey, "Projects", ArchiveKey, AlphaKey),
                    Node(AlphaKey, "Alpha", ProjectsKey),
                }
            );
        }

        /// <summary>The Archive chain with the Alpha leaf absent, so the stem has zero candidates.</summary>
        private static FolderTreeSnapshot SnapshotWithoutLeaf()
        {
            return new FolderTreeSnapshot(
                new[] { StoreKey },
                new[]
                {
                    Node(StoreKey, "Mailbox - User", null, ArchiveKey),
                    Node(ArchiveKey, "Archive", StoreKey, ProjectsKey),
                    Node(ProjectsKey, "Projects", ArchiveKey),
                }
            );
        }

        /// <summary>
        /// The Archive chain plus an Inbox chain whose leaf shares the last two segments, so a
        /// suffix match on the presented stem is ambiguous rather than absent.
        /// </summary>
        private static FolderTreeSnapshot DecoySnapshot()
        {
            return new FolderTreeSnapshot(
                new[] { StoreKey },
                new[]
                {
                    Node(StoreKey, "Mailbox - User", null, ArchiveKey, InboxKey),
                    Node(ArchiveKey, "Archive", StoreKey, ProjectsKey),
                    Node(ProjectsKey, "Projects", ArchiveKey, AlphaKey),
                    Node(AlphaKey, "Alpha", ProjectsKey),
                    Node(InboxKey, "Inbox", StoreKey, InboxProjectsKey),
                    Node(InboxProjectsKey, "Projects", InboxKey, InboxAlphaKey),
                    Node(InboxAlphaKey, "Alpha", InboxProjectsKey),
                }
            );
        }

        private static FolderTreeNodeKey Key(string entryId, string folderPath)
        {
            return new FolderTreeNodeKey("store-a", entryId, folderPath);
        }

        private static FolderTreeSnapshotNode Node(
            FolderTreeNodeKey key,
            string displayName,
            FolderTreeNodeKey parentKey,
            params FolderTreeNodeKey[] childKeys
        )
        {
            return new FolderTreeSnapshotNode(
                key,
                displayName,
                key.StoreId,
                key.EntryId,
                parentKey,
                key.FolderPath,
                displayName,
                childKeys,
                false,
                string.Empty
            );
        }
    }
}
