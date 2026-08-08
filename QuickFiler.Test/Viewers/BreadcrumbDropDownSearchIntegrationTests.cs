using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;
using QuickFiler.Controllers.Tests;
using QuickFiler.Viewers;
using UtilitiesCS;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #438: end-to-end contracts for the folder-search presentation path across the
    /// headless <c>ItemViewer</c> seam and a mocked <see cref="IBreadcrumbDropDownHost"/>.
    /// <para>
    /// Reuses the file-scoped <c>internal</c> <c>ItemViewerDropDownHarness</c> and
    /// <c>TrackingMessenger</c> declared in <c>BreadcrumbDropDownIntegrationTests.cs</c> without
    /// modifying that file, which sits exactly at the 500-line ceiling.
    /// </para>
    /// <para>
    /// GUI seam: the harness constructs a <c>UserControl</c>-derived <c>ItemViewer</c> but never
    /// shows it, never creates a window handle, and never runs a message pump. The popup host is a
    /// Moq mock, so no native drop-down is created and no window appears while these tests run.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed partial class BreadcrumbDropDownSearchIntegrationTests
    {
        private static readonly string[] FirstResults = { @"\\A\one", @"\\A\two", @"\\A\three" };
        private static readonly string[] SecondResults = { @"\\A\one", @"\\A\two" };

        /// <summary>
        /// AC-2 (search half): a search-driven present on a closed selector opens the host exactly
        /// once, through the 4-parameter overload, with <c>takeFocus: false</c>.
        /// </summary>
        [TestMethod]
        public void PresentFolderSearchResults_OnAClosedSelector_OpensOnceWithoutFocus()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);

                // Act
                harness.Viewer.PresentFolderSearchResults(FirstResults);

                // Assert
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            harness.AnchorScreenBounds,
                            harness.WorkingArea,
                            It.IsAny<Size>(),
                            false
                        ),
                    Times.Once()
                );
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>()
                        ),
                    Times.Never()
                );
                harness.FocusReturnCount.Should().Be(0, "no focus delegate may fire while typing");
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeTrue();
            }
        }

        /// <summary>
        /// AC-3: two consecutive search refreshes produce exactly one host open and zero closes —
        /// the second refresh must not close and reopen the popup.
        /// </summary>
        [TestMethod]
        public void PresentFolderSearchResults_TwoConsecutiveRefreshes_OpenOnceAndNeverClose()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);

                // Act
                harness.Viewer.PresentFolderSearchResults(FirstResults);
                harness.Viewer.PresentFolderSearchResults(SecondResults);

                // Assert
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>(),
                            It.IsAny<bool>()
                        ),
                    Times.Once()
                );
                harness.Host.Verify(
                    host => host.Close(It.IsAny<BreadcrumbDropDownCloseReason>()),
                    Times.Never()
                );
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeTrue();
            }
        }

        /// <summary>
        /// AC-3 / AC-6: a multi-character query delivered keystroke-by-keystroke still opens the
        /// popup once and never closes it, and the final row set reflects the complete query.
        /// </summary>
        [TestMethod]
        public void PresentFolderSearchResults_KeystrokeByKeystroke_OpensOnceAndTracksEveryRefresh()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);
                string[][] perKeystroke =
                {
                    new[] { @"\\A\i-1", @"\\A\i-2", @"\\A\i-3" },
                    new[] { @"\\A\in-1", @"\\A\in-2" },
                    new[] { @"\\A\inv-1", @"\\A\inv-2" },
                    new[] { @"\\A\invo-1" },
                };

                // Act
                foreach (string[] results in perKeystroke)
                {
                    harness.Viewer.PresentFolderSearchResults(results);
                }

                // Assert
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>(),
                            It.IsAny<bool>()
                        ),
                    Times.Once()
                );
                harness.Host.Verify(
                    host => host.Close(It.IsAny<BreadcrumbDropDownCloseReason>()),
                    Times.Never()
                );
                harness
                    .Viewer.GetFolderItems()
                    .Should()
                    .Equal(perKeystroke[perKeystroke.Length - 1]);
            }
        }

        /// <summary>
        /// AC-8: a refresh while the selector is open preserves the session and emits exactly one
        /// render per surface for that refresh.
        /// </summary>
        [TestMethod]
        public void PresentFolderSearchResults_RefreshWhileOpen_EmitsOneRenderPerSurface()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);
                harness.AttachClosedSurface();
                harness.RaisePopupReady();
                harness.Viewer.PresentFolderSearchResults(FirstResults);
                int closedBefore = CountType(harness.ClosedMessenger.Posted, "render");
                int popupBefore = CountType(harness.PopupMessenger.Posted, "render");

                // Act
                harness.Viewer.PresentFolderSearchResults(SecondResults);

                // Assert
                CountType(harness.ClosedMessenger.Posted, "render").Should().Be(closedBefore + 1);
                CountType(harness.PopupMessenger.Posted, "render").Should().Be(popupBefore + 1);
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeTrue();
            }
        }

        /// <summary>
        /// AC-4 (viewer half): the search highlight publishes no <c>SelectionChanged</c> through the
        /// viewer seam and leaves the committed folder untouched.
        /// </summary>
        [TestMethod]
        public void PresentFolderSearchResults_PublishesNoSelectionChangeAndKeepsCommittedFolder()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);
                int selections = 0;
                harness.Viewer.BreadcrumbCoordinator.SelectionChanged += (sender, args) =>
                    selections++;

                // Act
                harness.Viewer.PresentFolderSearchResults(FirstResults);
                harness.Viewer.PresentFolderSearchResults(SecondResults);

                // Assert
                selections.Should().Be(0, "a search highlight must never commit a selection");
                harness
                    .Viewer.BreadcrumbCoordinator.PendingIdentity.Should()
                    .Be(
                        @"plain:0:\\A\one",
                        "the first selectable row is highlighted, pending-only"
                    );
            }
        }

        /// <summary>
        /// AC-9: an empty result set is a deterministic no-op — no throw, no host open, and no
        /// selection mutation.
        /// </summary>
        [TestMethod]
        public void PresentFolderSearchResults_EmptyResultSet_DoesNotThrowOpenOrMutateSelection()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);
                string committedBefore = harness.Viewer.GetSelectedFolder();

                // Act
                Action act = () => harness.Viewer.PresentFolderSearchResults(Array.Empty<string>());

                // Assert
                act.Should().NotThrow();
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>(),
                            It.IsAny<bool>()
                        ),
                    Times.Never()
                );
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeFalse();
                harness.Viewer.GetFolderItems().Should().BeEmpty();
                committedBefore.Should().Be("A");
            }
        }

        /// <summary>
        /// AC-9: a banner-only result set has no selectable row, so the selector is never opened and
        /// nothing is highlighted.
        /// </summary>
        [TestMethod]
        public void PresentFolderSearchResults_BannerOnlyResultSet_DoesNotOpenOrHighlight()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);

                // Act
                Action act = () =>
                    harness.Viewer.PresentFolderSearchResults(new[] { "==== no matches ====" });

                // Assert
                act.Should().NotThrow();
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>(),
                            It.IsAny<bool>()
                        ),
                    Times.Never()
                );
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeFalse();
                harness.Viewer.BreadcrumbCoordinator.PendingIdentity.Should().BeNull();
            }
        }

        /// <summary>
        /// AC-7: the explicit-gesture path is untouched — <c>SetFolderDroppedDown(true)</c> still
        /// opens through the original focusing 3-parameter overload.
        /// </summary>
        [TestMethod]
        public void SetFolderDroppedDownTrue_StillUsesTheFocusingThreeParameterOverload()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);

                // Act
                harness.Viewer.SetFolderDroppedDown(true);

                // Assert
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            harness.AnchorScreenBounds,
                            harness.WorkingArea,
                            It.IsAny<Size>()
                        ),
                    Times.Once()
                );
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>(),
                            It.IsAny<bool>()
                        ),
                    Times.Never()
                );
            }
        }

        /// <summary>
        /// AC-2 / AC-7: a search present followed by an explicit gesture leaves the gesture's
        /// focusing semantics intact — the latch does not leak past the open it was set for.
        /// </summary>
        [TestMethod]
        public void PresentThenGesture_LeavesTheGestureOpenOnTheFocusingPath()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                RegisterNonFocusingOpen(harness);
                harness.Viewer.PresentFolderSearchResults(FirstResults);
                harness.Viewer.SetFolderDroppedDown(false);
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeFalse();

                // Act
                harness.Viewer.SetFolderDroppedDown(true);

                // Assert
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>()
                        ),
                    Times.Once()
                );
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>(),
                            false
                        ),
                    Times.Once()
                );
            }
        }

        /// <summary>Deterministic per-query result set, distinct for every query length.</summary>
        private static string[] ResultsFor(string query) =>
            new[] { @"\\A\" + query + "-1", @"\\A\" + query + "-2" };

        /// <summary>
        /// Registers the 4-parameter <c>OpenAsync</c> on the harness's loose host mock.
        /// </summary>
        /// <remarks>
        /// The harness configures only the 3-parameter shape, whose <c>.Callback</c> sets the
        /// private <c>_hostOpen</c> field backing <c>Host.IsOpen</c>. A 4-parameter setup cannot
        /// reach that private field, so the harness's <c>SetHostOpen</c> seam is driven from the
        /// callback here instead. Registering the extra setup from this file keeps
        /// <c>BreadcrumbDropDownIntegrationTests.cs</c> byte-unmodified.
        /// </remarks>
        private static void RegisterNonFocusingOpen(ItemViewerDropDownHarness harness)
        {
            harness
                .Host.Setup(host =>
                    host.OpenAsync(
                        It.IsAny<Rectangle>(),
                        It.IsAny<Rectangle>(),
                        It.IsAny<Size>(),
                        It.IsAny<bool>()
                    )
                )
                .Callback<Rectangle, Rectangle, Size, bool>(
                    (anchor, work, desired, takeFocus) => harness.SetHostOpen(true)
                )
                .ReturnsAsync(true);
        }

        private static int CountType(IEnumerable<string> messages, string type) =>
            messages.Count(message => message.Contains("\"type\":\"" + type + "\""));
    }
}
