using System;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #810 (AC7): coverage for <see cref="BreadcrumbPopupOwnerRegistry"/>, the extracted
    /// derivation behind <c>QfcFormViewer.IsDeactivationSelfInflictedByOwnPopup</c>.
    /// <para>
    /// The producer of that predicate was previously inline on a form-derived class carrying a
    /// class-level <c>[ExcludeFromCodeCoverage]</c>, so it emitted no Cobertura class element and
    /// could not be measured. Extracting it to a plain class makes the derivation measurable and
    /// testable without a form hierarchy. Each key is a plain <see cref="Control"/>, which creates
    /// no window handle, so no window is shown and no Outlook COM is touched.
    /// </para>
    /// </summary>
    [TestClass]
    public class BreadcrumbPopupOwnerRegistryTests
    {
        /// <summary>
        /// Scenario: nothing has been registered. Expected outcome: the registry reports no popup
        /// open. This is the genuine case and the load-bearing polarity: a form with no registered
        /// popup owner must report false, so the issue-677 deactivation contract stays in force for
        /// every deactivation that is not self-inflicted.
        /// </summary>
        [TestMethod]
        public void AnyOpen_WithNoRegistration_ReportsFalse()
        {
            // Arrange
            var registry = new BreadcrumbPopupOwnerRegistry();

            // Act
            bool anyOpen = registry.AnyOpen;

            // Assert
            anyOpen
                .Should()
                .BeFalse("a form with no registered popup owner must report no popup open");
        }

        /// <summary>
        /// Scenario: one owner is registered and its predicate reports its popup closed. Expected
        /// outcome: the registry reports no popup open.
        /// </summary>
        [TestMethod]
        public void AnyOpen_SingleOwnerReportingClosed_ReportsFalse()
        {
            // Arrange
            var registry = new BreadcrumbPopupOwnerRegistry();
            using (var owner = new Control())
            {
                registry.Register(owner, () => false);

                // Act
                bool anyOpen = registry.AnyOpen;

                // Assert
                anyOpen.Should().BeFalse("the only registered owner reports its popup closed");
            }
        }

        /// <summary>
        /// Scenario: one owner is registered and its predicate reports its popup open. Expected
        /// outcome: the registry reports a popup open.
        /// </summary>
        [TestMethod]
        public void AnyOpen_SingleOwnerReportingOpen_ReportsTrue()
        {
            // Arrange
            var registry = new BreadcrumbPopupOwnerRegistry();
            using (var owner = new Control())
            {
                registry.Register(owner, () => true);

                // Act
                bool anyOpen = registry.AnyOpen;

                // Assert
                anyOpen.Should().BeTrue("the only registered owner reports its popup open");
            }
        }

        /// <summary>
        /// Scenario: two owners are registered and exactly one reports its popup open. Expected
        /// outcome: the registry reports a popup open, because the derivation is a disjunction over
        /// every registered owner rather than a report on any single one.
        /// </summary>
        [TestMethod]
        public void AnyOpen_TwoOwnersOneReportingOpen_ReportsTrue()
        {
            // Arrange
            var registry = new BreadcrumbPopupOwnerRegistry();
            using (var closedOwner = new Control())
            using (var openOwner = new Control())
            {
                registry.Register(closedOwner, () => false);
                registry.Register(openOwner, () => true);

                // Act
                bool anyOpen = registry.AnyOpen;

                // Assert
                anyOpen.Should().BeTrue("one owner reporting its popup open is enough");
            }
        }

        /// <summary>
        /// Scenario: the same control is registered twice with different predicates. Expected
        /// outcome: the second registration replaces the first rather than appending beside it, so
        /// the superseded predicate is never consulted again. An appending registry would keep
        /// reporting true from a stale predicate after its owner had closed its popup.
        /// </summary>
        [TestMethod]
        public void Register_SameControlTwice_ReplacesRatherThanAppends()
        {
            // Arrange
            var registry = new BreadcrumbPopupOwnerRegistry();
            var superseded = new Mock<Func<bool>>();
            superseded.Setup(predicate => predicate()).Returns(true);
            var current = new Mock<Func<bool>>();
            current.Setup(predicate => predicate()).Returns(false);
            using (var owner = new Control())
            {
                registry.Register(owner, superseded.Object);
                registry.Register(owner, current.Object);

                // Act
                bool anyOpen = registry.AnyOpen;

                // Assert
                anyOpen
                    .Should()
                    .BeFalse("the replacing predicate reports closed and it is the only entry");
                superseded.Verify(
                    predicate => predicate(),
                    Times.Never,
                    "a replaced predicate must not survive beside its replacement"
                );
                current.Verify(predicate => predicate(), Times.Once);
            }
        }

        /// <summary>
        /// Scenario: a registration is attempted with a null control, and another with a null
        /// predicate. Expected outcome: each is rejected with an <see cref="ArgumentNullException"/>
        /// naming the offending parameter, and the registry is provably unchanged afterwards.
        /// Issue #823 (R3): both parameters are declared non-nullable, and the sole production call
        /// site passes <c>this</c> and a lambda literal, so a null argument is a contract breach
        /// rather than an ordinary condition and the boundary rejects it explicitly.
        /// </summary>
        [TestMethod]
        public void Register_NullControlOrNullPredicate_IsRejected()
        {
            // Arrange
            var registry = new BreadcrumbPopupOwnerRegistry();
            using (var owner = new Control())
            {
                // Act
                Action registerNullControl = () => registry.Register(null, () => true);
                Action registerNullPredicate = () => registry.Register(owner, null);

                // Assert
                registerNullControl
                    .Should()
                    .Throw<ArgumentNullException>("a null owner is rejected, not ignored")
                    .WithParameterName("itemViewer");
                registerNullPredicate
                    .Should()
                    .Throw<ArgumentNullException>("a null predicate is rejected, not ignored")
                    .WithParameterName("popupIsOpen");
                registry
                    .AnyOpen.Should()
                    .BeFalse("the registry is provably unchanged by a rejected registration");
            }
        }
    }
}
