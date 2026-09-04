using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Issue #736 finding 1: the <c>AppOlObjects.ArchiveRootPath</c> getter evaluated two live
    /// Outlook COM reads while composing the arguments it handed to the validation guard, so a
    /// transient <see cref="COMException"/> escaped a member whose documented contract admits only
    /// <see cref="InvalidOperationException"/>. These tests drive the delegate-driven core
    /// <c>AppOlObjects.ResolveValidatedArchiveRootPath</c> directly, so they need no Outlook COM
    /// object and no live Outlook process. No test here constructs an <c>AppOlObjects</c> instance,
    /// creates a temporary file, or touches live Outlook.
    /// </summary>
    [TestClass]
    public class AppOlObjectsArchiveRootComGuardTests
    {
        private const string ComposedRoot = @"\\mailbox@example.com\Archive";
        private const string MailboxAddress = "mailbox@example.com";

        /// <summary>
        /// A COM failure on the composed-path read must surface as the documented
        /// <see cref="InvalidOperationException"/>, with the original <see cref="COMException"/>
        /// preserved as <c>InnerException</c> so log-level diagnosis is still possible.
        /// </summary>
        [TestMethod]
        public void ResolveValidatedArchiveRootPath_WhenComposedReadThrowsComException_NormalizesToInvalidOperation()
        {
            // Arrange
            var comFailure = new COMException("Outlook is busy.");
            var diagnostics = new List<string>();
            Action act = () =>
                AppOlObjects.ResolveValidatedArchiveRootPath(
                    () => throw comFailure,
                    () => ComposedRoot,
                    diagnostics.Add
                );

            // Act
            InvalidOperationException thrown = act.Should()
                .Throw<InvalidOperationException>()
                .Which;

            // Assert
            thrown
                .InnerException.Should()
                .BeSameAs(
                    comFailure,
                    "the original COM failure is preserved for log-level diagnosis"
                );
        }

        /// <summary>
        /// The resolved-folder read is a second, distinct COM crossing, so it must normalize the
        /// same way the composed-path read does.
        /// </summary>
        [TestMethod]
        public void ResolveValidatedArchiveRootPath_WhenResolvedReadThrowsComException_NormalizesToInvalidOperation()
        {
            // Arrange
            var comFailure = new COMException("The folder collection is unavailable.");
            var diagnostics = new List<string>();
            Action act = () =>
                AppOlObjects.ResolveValidatedArchiveRootPath(
                    () => ComposedRoot,
                    () => throw comFailure,
                    diagnostics.Add
                );

            // Act
            InvalidOperationException thrown = act.Should()
                .Throw<InvalidOperationException>()
                .Which;

            // Assert
            thrown.InnerException.Should().BeSameAs(comFailure);
        }

        /// <summary>
        /// Issue #602's redaction rule holds for the new failure mode: the normalized message names
        /// the rule and withholds both the archive path and the mailbox address it contains. The
        /// exception-type assertion is required, not incidental — a version inspecting whatever
        /// exception happened to escape would read the pre-fix <see cref="COMException"/>'s own
        /// message, which carries no path either, and would pass before the fix landed.
        /// </summary>
        [TestMethod]
        public void ResolveValidatedArchiveRootPath_WhenComReadFails_MessageWithholdsPathAndMailboxAddress()
        {
            // Arrange
            var diagnostics = new List<string>();
            Action act = () =>
                AppOlObjects.ResolveValidatedArchiveRootPath(
                    () => throw new COMException("Outlook is busy."),
                    () => ComposedRoot,
                    diagnostics.Add
                );

            // Act
            InvalidOperationException thrown = act.Should()
                .Throw<InvalidOperationException>()
                .Which;

            // Assert
            thrown.Message.Should().NotContain(ComposedRoot);
            thrown.Message.Should().NotContain(MailboxAddress);
            diagnostics.Should().ContainSingle("the diagnostic is emitted once, before the throw");
            diagnostics[0].Should().NotContain(ComposedRoot);
            diagnostics[0].Should().NotContain(MailboxAddress);
        }

        /// <summary>
        /// The success path is unchanged: both reads resolve, the validated path is returned
        /// unchanged, and no diagnostic is emitted.
        /// </summary>
        [TestMethod]
        public void ResolveValidatedArchiveRootPath_WhenBothReadsResolve_ReturnsPathAndEmitsNoDiagnostic()
        {
            // Arrange
            var diagnostics = new List<string>();

            // Act
            string actual = AppOlObjects.ResolveValidatedArchiveRootPath(
                () => ComposedRoot,
                () => ComposedRoot,
                diagnostics.Add
            );

            // Assert
            actual.Should().Be(ComposedRoot);
            diagnostics.Should().BeEmpty("a resolvable archive root emits no diagnostic");
        }

        /// <summary>
        /// The frozen guard's own unresolvable branch still reaches the caller untouched: no COM
        /// failure occurred, so there is no inner exception to attach.
        /// </summary>
        [TestMethod]
        public void ResolveValidatedArchiveRootPath_WhenResolvedFolderIsNull_ThrowsUnresolvableWithNoInnerException()
        {
            // Arrange
            var diagnostics = new List<string>();
            Action act = () =>
                AppOlObjects.ResolveValidatedArchiveRootPath(
                    () => ComposedRoot,
                    () => null,
                    diagnostics.Add
                );

            // Act
            InvalidOperationException thrown = act.Should()
                .Throw<InvalidOperationException>()
                .Which;

            // Assert
            thrown.Message.Should().Contain("could not be resolved");
            thrown.InnerException.Should().BeNull();
            diagnostics.Should().ContainSingle();
        }

        /// <summary>
        /// A failed resolution is not cached. Invoking the core twice with a composed-path read
        /// that fails on both calls must re-read the composed path each time; a cached failure
        /// would re-throw without re-reading. The resolved-folder read is never reached, because a
        /// COM failure on the first read short-circuits the second inside the single guarded block.
        /// </summary>
        [TestMethod]
        public void ResolveValidatedArchiveRootPath_WhenComReadFailsTwice_ReReadsTheComposedPathOnTheSecondCall()
        {
            // Arrange
            int composedReads = 0;
            int resolvedReads = 0;
            var diagnostics = new List<string>();
            Func<string> readComposed = () =>
            {
                composedReads++;
                throw new COMException("Outlook is busy.");
            };
            Func<string> readResolved = () =>
            {
                resolvedReads++;
                return ComposedRoot;
            };
            Action act = () =>
                AppOlObjects.ResolveValidatedArchiveRootPath(
                    readComposed,
                    readResolved,
                    diagnostics.Add
                );

            // Act / Assert: each call surfaces the documented exception type.
            act.Should().Throw<InvalidOperationException>();
            act.Should().Throw<InvalidOperationException>();

            // Assert: the composed path was read once per call, proving nothing was cached.
            composedReads.Should().Be(2);
            resolvedReads
                .Should()
                .Be(
                    0,
                    "a COM failure on the composed read short-circuits the resolved read inside the single guarded block"
                );
        }
    }
}
