using System.Drawing;
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
