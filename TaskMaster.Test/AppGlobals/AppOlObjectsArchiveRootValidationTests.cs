using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Issue #614 tests for defect D6: the archive-root validation that replaced
    /// <c>AppOlObjects.ArchiveRootPath</c>'s unverified, default-store-scoped string combine.
    /// The decision logic lives in the pure <see cref="ArchiveRootPathGuard"/> helper, so these
    /// tests need no Outlook COM object and no live Outlook process; consumer-side behaviour is
    /// exercised through the mockable <see cref="IOlObjects"/> seam.
    /// </summary>
    [TestClass]
    public class AppOlObjectsArchiveRootValidationTests
    {
        private const string ComposedRoot = @"\\mailbox@example.com\Archive";

        [TestMethod]
        public void RequireResolvedArchiveRoot_ResolvedRootMatchesComposedPath_ReturnsIt()
        {
            // Arrange
            var diagnostics = new List<string>();

            // Act
            string actual = ArchiveRootPathGuard.RequireResolvedArchiveRoot(
                ComposedRoot,
                ComposedRoot,
                diagnostics.Add
            );

            // Assert
            actual.Should().Be(ComposedRoot);
            diagnostics.Should().BeEmpty("a resolvable archive root emits no diagnostic");
        }

        [TestMethod]
        public void RequireResolvedArchiveRoot_CaseDifferingResolvedRoot_ReturnsComposedPath()
        {
            // Arrange
            var diagnostics = new List<string>();

            // Act
            string actual = ArchiveRootPathGuard.RequireResolvedArchiveRoot(
                ComposedRoot,
                @"\\MAILBOX@EXAMPLE.COM\aRcHiVe",
                diagnostics.Add
            );

            // Assert: Outlook path comparison is case-insensitive.
            actual.Should().Be(ComposedRoot);
            diagnostics.Should().BeEmpty();
        }

        [TestMethod]
        public void RequireResolvedArchiveRoot_UnresolvableRoot_ThrowsAndDiagnosesWithoutTheValue()
        {
            // Arrange: no folder resolved for the composed archive root.
            var diagnostics = new List<string>();
            Action act = () =>
                ArchiveRootPathGuard.RequireResolvedArchiveRoot(
                    ComposedRoot,
                    null,
                    diagnostics.Add
                );

            // Act
            InvalidOperationException thrown = act.Should()
                .Throw<InvalidOperationException>()
                .Which;

            // Assert
            thrown.Message.Should().Contain("could not be resolved");
            thrown.Message.Should().NotContain("mailbox@example.com");
            diagnostics.Should().ContainSingle();
            diagnostics[0].Should().NotContain("mailbox@example.com");
        }

        [TestMethod]
        public void RequireResolvedArchiveRoot_CrossStoreRoot_ThrowsAndDiagnosesWithoutTheValue()
        {
            // Arrange: the folder that resolved lives in a DIFFERENT store.
            var diagnostics = new List<string>();
            Action act = () =>
                ArchiveRootPathGuard.RequireResolvedArchiveRoot(
                    ComposedRoot,
                    @"\\other@example.org\Archive",
                    diagnostics.Add
                );

            // Act
            InvalidOperationException thrown = act.Should()
                .Throw<InvalidOperationException>()
                .Which;

            // Assert
            thrown.Message.Should().Contain("cross-store");
            thrown.Message.Should().NotContain("mailbox@example.com");
            thrown.Message.Should().NotContain("other@example.org");
            diagnostics.Should().ContainSingle();
        }

        [TestMethod]
        public void RequireResolvedArchiveRoot_EmptyComposedPath_Throws()
        {
            // Arrange
            Action act = () =>
                ArchiveRootPathGuard.RequireResolvedArchiveRoot(string.Empty, ComposedRoot, null);

            // Act / Assert: a null diagnostic sink must not mask the failure.
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void ConsumerSeam_ArchiveRootPath_IsReadThroughTheMockableInterface()
        {
            // Arrange: every production consumer reads the property through IOlObjects, so the
            // validated value is what downstream code observes.
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(objects => objects.ArchiveRootPath).Returns(ComposedRoot);

            // Act
            string actual = olObjects.Object.ArchiveRootPath;

            // Assert
            actual.Should().Be(ComposedRoot);
            olObjects.VerifyGet(objects => objects.ArchiveRootPath, Times.Once);
        }
    }
}
