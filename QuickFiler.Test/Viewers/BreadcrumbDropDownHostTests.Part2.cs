using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #438: real-host focus-delegate contracts for the additive <c>takeFocus</c> intent.
    /// <para>
    /// A partial extension of <see cref="BreadcrumbDropDownHostTests"/> so the 499-line primary file
    /// is untouched apart from its one-token <c>partial</c> keyword. No <c>[TestClass]</c> attribute
    /// is repeated here: <c>TestClassAttribute</c> is <c>AllowMultiple = false</c> and declaring it
    /// on both parts is a CS0579 duplicate-attribute error.
    /// </para>
    /// <para>
    /// These cases exercise the real <c>BreadcrumbDropDownHost</c> through the existing private
    /// <c>Harness</c>, whose <c>focusPending</c>, <c>focusAnchor</c>, and <c>showPopup</c> operations
    /// are injected counting delegates. No popup is ever shown and no window handle is created, so
    /// the loose-mock concern that shapes the open-coordinator dispatch does not apply here and no
    /// GUI surface appears while the tests run.
    /// </para>
    /// </summary>
    public sealed partial class BreadcrumbDropDownHostTests
    {
        private static readonly Rectangle Anchor = new Rectangle(100, 100, 200, 25);
        private static readonly Rectangle Work = new Rectangle(0, 0, 800, 600);
        private static readonly Size Desired = new Size(300, 200);

        /// <summary>
        /// AC-2: a fresh search-driven open completes normally but invokes neither focus delegate,
        /// so the caret stays in the search textbox.
        /// </summary>
        [TestMethod]
        public void OpenAsync_FreshOpenWithoutFocus_InvokesNeitherFocusDelegate()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act
                bool opened = OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false);

                // Assert
                opened.Should().BeTrue("a non-focusing open still reports the same open result");
                harness.FocusPendingCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(0);
                Property<bool>(harness.Host, "IsOpen").Should().BeTrue();
            }
        }

        /// <summary>
        /// AC-2 / AC-3: re-issuing a non-focusing open on an already-open popup must not schedule the
        /// focus-pending delegate, and must not recreate or re-show the surface. This is the
        /// per-keystroke steady state once the drop-down is open.
        /// </summary>
        [TestMethod]
        public void OpenAsync_ReissuedWithoutFocusWhileOpen_DoesNotScheduleFocusPending()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();
                harness.FocusPendingCount.Should().Be(0);

                // Act
                bool reopened = OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false);

                // Assert
                reopened.Should().BeTrue();
                harness
                    .FocusPendingCount.Should()
                    .Be(0, "the already-open branch must not re-focus");
                harness.FocusAnchorCount.Should().Be(0);
                harness.FactoryCount.Should().Be(1, "the surface is reused, not recreated");
                harness.ShowCount.Should().Be(1, "the popup is not re-shown");
            }
        }

        /// <summary>
        /// AC-2 / AC-3: repeated non-focusing opens simulating consecutive keystrokes never focus and
        /// never re-show, so there is no popup churn while typing.
        /// </summary>
        [TestMethod]
        public void OpenAsync_ConsecutiveNonFocusingOpens_NeverFocusAndNeverReshow()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act — one open per simulated keystroke.
                for (int keystroke = 0; keystroke < 5; keystroke++)
                {
                    OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false)
                        .Should()
                        .BeTrue();
                }

                // Assert
                harness.FocusPendingCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(0);
                harness.FactoryCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.CancelCount.Should().Be(0);
            }
        }

        /// <summary>
        /// AC-2 (gesture half): an explicit open through the additive overload with
        /// <c>takeFocus: true</c> still focuses the popup exactly once, preserving issue #400 AC-13.
        /// </summary>
        [TestMethod]
        public void OpenAsync_FreshOpenWithFocus_InvokesFocusPendingExactlyOnce()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act
                bool opened = OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, true);

                // Assert
                opened.Should().BeTrue();
                harness.FocusPendingCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(0);
            }
        }

        /// <summary>
        /// AC-2 / AC-10 (gesture half): the pre-existing 3-parameter overload keeps its exact
        /// focus-on-open semantics, because it delegates with <c>takeFocus: true</c>.
        /// </summary>
        [TestMethod]
        public void OpenAsync_ThreeParameterOverload_StillFocusesPendingExactlyOnce()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act
                bool opened = Open(harness.Host, Anchor, Work, Desired);

                // Assert
                opened.Should().BeTrue();
                harness.FocusPendingCount.Should().Be(1, "the default overload takes focus");
            }
        }

        /// <summary>
        /// AC-2 / AC-7: a non-focusing search open followed by an explicit gesture open focuses
        /// exactly once — the gesture's focus is not suppressed by the earlier search intent.
        /// </summary>
        [TestMethod]
        public void OpenAsync_NonFocusingThenGestureOpen_FocusesOnlyForTheGesture()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();
                harness.FocusPendingCount.Should().Be(0);

                // Act — an explicit gesture re-issued on the already-open popup.
                Open(harness.Host, Anchor, Work, Desired).Should().BeTrue();

                // Assert
                harness.FocusPendingCount.Should().Be(1);
            }
        }

        /// <summary>
        /// AC-2: the close-side focus return is unchanged for a popup opened without focus, so an
        /// explicit close still hands focus back to the collapsed anchor exactly once.
        /// </summary>
        [TestMethod]
        public void Close_AfterANonFocusingOpen_StillReturnsFocusToTheAnchorOnce()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();

                // Act
                Close(harness.Host, "Uncommitted").Should().BeTrue();

                // Assert
                harness.FocusAnchorCount.Should().Be(1);
                harness.CancelCount.Should().Be(1);
                Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
            }
        }

        /// <summary>
        /// AC-2: a failed non-focusing open follows the unchanged rollback contract — the selection
        /// is cancelled, focus returns to the anchor, and the failure is retained.
        /// </summary>
        [TestMethod]
        public void OpenAsync_NonFocusingOpenWithZeroWorkingArea_RollsBackUnchanged()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act
                bool opened = OpenWithFocusIntent(
                    harness.Host,
                    Anchor,
                    Rectangle.Empty,
                    Desired,
                    false
                );

                // Assert
                opened.Should().BeFalse();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(0);
            }
        }

        /// <summary>
        /// Issue #680: a non-focusing (search-driven) open must present the popup with
        /// <c>AutoClose == false</c>, which is the WinForms framework's own opt-out from
        /// <c>ModalMenuFilter</c> menu-mode entry. Menu mode is entered inside
        /// <c>SetVisibleCore(true)</c>, so the property must already be false when the show delegate
        /// runs — observing it at that instant is the only way to pin the ordering.
        /// </summary>
        [TestMethod]
        public void ShowPopup_NonFocusingOpen_RunsTheShowDelegateWithAutoCloseFalse()
        {
            // Arrange
            var observed = new List<bool>();
            Action<ToolStripDropDown, Control, Point> show = (dropDown, owner, point) =>
                observed.Add(dropDown.AutoClose);
            using (Harness harness = CreateHarness(show))
            {
                // Act
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();

                // Assert
                observed.Should().Equal(new[] { false });
            }
        }

        /// <summary>
        /// Issue #680 (gesture control): the pre-existing 3-parameter open is an explicit gesture and
        /// keeps standard popup semantics, so the show delegate must see <c>AutoClose == true</c>.
        /// </summary>
        [TestMethod]
        public void ShowPopup_GestureOpen_RunsTheShowDelegateWithAutoCloseTrue()
        {
            // Arrange
            var observed = new List<bool>();
            Action<ToolStripDropDown, Control, Point> show = (dropDown, owner, point) =>
                observed.Add(dropDown.AutoClose);
            using (Harness harness = CreateHarness(show))
            {
                // Act
                Open(harness.Host, Anchor, Work, Desired).Should().BeTrue();

                // Assert
                observed.Should().Equal(new[] { true });
            }
        }

        /// <summary>
        /// Issue #680 (guard): close completion restores the <c>AutoClose = true</c> default, so the
        /// next lifecycle always starts from standard popup semantics regardless of how the previous
        /// one was opened.
        /// </summary>
        [TestMethod]
        public void Close_AfterANonFocusingOpen_RestoresAutoCloseTrue()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();

                // Act
                Close(harness.Host, "Uncommitted").Should().BeTrue();

                // Assert
                Property<ToolStripDropDown>(harness.Host, "DropDown")
                    .AutoClose.Should()
                    .BeTrue("close completion must restore the default for the next lifecycle");
            }
        }

        /// <summary>
        /// Issue #680 (guard): a <c>takeFocus: true</c> reopen on a popup that was shown
        /// non-capturing is the Down-arrow handoff. Standard popup semantics resume there, so
        /// <c>AutoClose</c> returns to <c>true</c> and the focus-pending delegate runs exactly once.
        /// </summary>
        [TestMethod]
        public void OpenAsync_TakeFocusReopenOnANonFocusingOpen_RestoresAutoCloseTrue()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();

                // Act
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, true).Should().BeTrue();

                // Assert
                Property<ToolStripDropDown>(harness.Host, "DropDown")
                    .AutoClose.Should()
                    .BeTrue("the gesture handoff resumes standard popup semantics");
                harness.FocusPendingCount.Should().Be(1);
            }
        }

        /// <summary>
        /// Issue #680 (edge): a gesture open issued immediately after a completed non-capturing cycle
        /// must still show with <c>AutoClose == true</c>. Only the second show is asserted, because
        /// the first show's value is already the subject of the non-focusing-open test above.
        /// </summary>
        [TestMethod]
        public void ShowPopup_GestureOpenAfterANonFocusingCycle_RunsTheShowDelegateWithAutoCloseTrue()
        {
            // Arrange
            var observed = new List<bool>();
            Action<ToolStripDropDown, Control, Point> show = (dropDown, owner, point) =>
                observed.Add(dropDown.AutoClose);
            using (Harness harness = CreateHarness(show))
            {
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();
                Close(harness.Host, "Uncommitted").Should().BeTrue();

                // Act
                Open(harness.Host, Anchor, Work, Desired).Should().BeTrue();

                // Assert
                harness
                    .ShowCount.Should()
                    .Be(2, "the closed popup is shown again for the gesture");
                observed[1].Should().BeTrue("the gesture open restores standard popup semantics");
            }
        }

        /// <summary>
        /// Issue #680: two consecutive non-capturing opens ride the existing already-open latch, so
        /// the popup is shown once, is never re-focused, and that single show sees
        /// <c>AutoClose == false</c>. This is the host-level companion to the coordinator's
        /// mocked-host refresh test, which cannot observe <c>AutoClose</c> at all.
        /// </summary>
        [TestMethod]
        public void ShowPopup_TwoConsecutiveNonFocusingOpens_ShowOnceWithAutoCloseFalse()
        {
            // Arrange
            var observed = new List<bool>();
            Action<ToolStripDropDown, Control, Point> show = (dropDown, owner, point) =>
                observed.Add(dropDown.AutoClose);
            using (Harness harness = CreateHarness(show))
            {
                // Act
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();
                OpenWithFocusIntent(harness.Host, Anchor, Work, Desired, false).Should().BeTrue();

                // Assert
                harness
                    .ShowCount.Should()
                    .Be(1, "the already-open latch suppresses the second show");
                harness.FocusPendingCount.Should().Be(0);
                observed.Should().Equal(new[] { false });
            }
        }

        /// <summary>
        /// Invokes the additive 4-parameter <c>OpenAsync</c> through
        /// <see cref="IBreadcrumbDropDownHost"/>, where it is declared. The primary file reaches the
        /// host by reflection because it was authored failure-first before the type existed; the
        /// intent-carrying overload is an explicit interface implementation, so a typed interface
        /// call is both the supported access path and clearer than reflection.
        /// </summary>
        private static bool OpenWithFocusIntent(
            object host,
            Rectangle anchor,
            Rectangle work,
            Size desired,
            bool takeFocus
        ) =>
            ((IBreadcrumbDropDownHost)host)
                .OpenAsync(anchor, work, desired, takeFocus)
                .GetAwaiter()
                .GetResult();
    }
}
