using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.Threading;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Behavior-preserving verification of the issue #264 <see cref="CurrentStoreContext"/> wrap
    /// added to <see cref="AppOlObjects.EmitPerStoreInboxAttribution"/>. The method is
    /// <c>internal static</c> and COM-free via injected delegates, so the attribution wrap is tested
    /// without live COM. Marked <c>[DoNotParallelize]</c> because it reads the process-global
    /// <see cref="CurrentStoreContext"/>. Each test captures and asserts revert-to-prior so it does
    /// not depend on a specific starting global state.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class AppOlObjectsAttributionContextTests
    {
        [TestMethod]
        public void EmitPerStoreInboxAttribution_SetsCurrentStoreContext_DuringGetDefaultFolder_ThenReverts()
        {
            // Arrange
            var prior = CurrentStoreContext.Current;
            var probe = new StartupInboxAttributionProbe(_ => { });
            var folder = new Mock<MAPIFolder>().Object;
            string observedDuringCall = null;

            // Act
            var result = AppOlObjects.EmitPerStoreInboxAttribution(
                shouldInclude: () => true,
                getDefaultFolder: () =>
                {
                    observedDuringCall = CurrentStoreContext.Current;
                    return folder;
                },
                readDisplayName: () => "Mailbox - Dan",
                probe: probe
            );

            // Assert: the context carried the displayName during the blocking call, then reverted, and
            // the attribution outcome is unchanged (folder returned).
            observedDuringCall.Should().Be("Mailbox - Dan");
            CurrentStoreContext.Current.Should().Be(prior);
            result.Should().BeSameAs(folder);
        }

        [TestMethod]
        public void EmitPerStoreInboxAttribution_ExcludedStore_NeverOpensContext_AndReturnsNull()
        {
            // Arrange
            var prior = CurrentStoreContext.Current;
            var probe = new StartupInboxAttributionProbe(_ => { });
            var getDefaultFolderCalled = false;

            // Act: an excluded store never invokes getDefaultFolder, so the scope is never entered.
            var result = AppOlObjects.EmitPerStoreInboxAttribution(
                shouldInclude: () => false,
                getDefaultFolder: () =>
                {
                    getDefaultFolderCalled = true;
                    return new Mock<MAPIFolder>().Object;
                },
                readDisplayName: () => "Public Folders",
                probe: probe
            );

            // Assert: outcome unchanged (null, GetDefaultFolder skipped) and context untouched.
            result.Should().BeNull();
            getDefaultFolderCalled.Should().BeFalse();
            CurrentStoreContext.Current.Should().Be(prior);
        }

        [TestMethod]
        public void EmitPerStoreInboxAttribution_RevertsContext_EvenWhenGetDefaultFolderThrows()
        {
            // Arrange
            var prior = CurrentStoreContext.Current;
            var probe = new StartupInboxAttributionProbe(_ => { });
            var thrown = new COMException("store not ready");
            string observedDuringCall = null;

            // Act: an included store whose blocking call throws. The using-scope must revert the
            // context on the exception path, and the COMException must propagate unchanged.
            System.Action act = () =>
                AppOlObjects.EmitPerStoreInboxAttribution(
                    shouldInclude: () => true,
                    getDefaultFolder: () =>
                    {
                        observedDuringCall = CurrentStoreContext.Current;
                        throw thrown;
                    },
                    readDisplayName: () => "Mailbox",
                    probe: probe
                );

            // Assert
            act.Should().Throw<COMException>().Which.Should().BeSameAs(thrown);
            observedDuringCall.Should().Be("Mailbox");
            CurrentStoreContext.Current.Should().Be(prior);
        }
    }
}
