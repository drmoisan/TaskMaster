using System.Collections.Generic;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic verification of the PostLoad/LoadInboxes attribution formatter (issue #211,
    /// <see cref="TaskMaster.StartupInboxAttributionProbe"/>). The probe's pure line-formatting is
    /// exercised through an injected list-capturing sink so the exact line structure is asserted
    /// without a live appender, live COM, a live timer, or live GC reads. No network/filesystem and
    /// no temporary files are used.
    /// </summary>
    [TestClass]
    public class StartupInboxAttributionProbeTests
    {
        private const string StepToDo = "ToDoFolder.Items";
        private const string StepReminders = "OlReminders";
        private const string StepInboxes = "Inboxes";

        [TestMethod]
        public void FormatReadinessHookupStart_AllThreeSteps_ProducesExactStartLines()
        {
            // Arrange / Act / Assert: each of the three readiness-hookup steps produces its exact
            // START line, with the step name emitted verbatim.
            StartupInboxAttributionProbe
                .FormatReadinessHookupStart(StepToDo)
                .Should()
                .Be("[readiness-hookup] step=ToDoFolder.Items start");
            StartupInboxAttributionProbe
                .FormatReadinessHookupStart(StepReminders)
                .Should()
                .Be("[readiness-hookup] step=OlReminders start");
            StartupInboxAttributionProbe
                .FormatReadinessHookupStart(StepInboxes)
                .Should()
                .Be("[readiness-hookup] step=Inboxes start");
        }

        [TestMethod]
        public void FormatReadinessHookupEnd_AllThreeSteps_ProducesExactEndLinesWithF2Ms()
        {
            // Arrange / Act / Assert: each END line carries the F2 invariant-culture elapsed ms.
            StartupInboxAttributionProbe
                .FormatReadinessHookupEnd(StepToDo, 12.0)
                .Should()
                .Be("[readiness-hookup] step=ToDoFolder.Items end elapsedMs=12.00");
            StartupInboxAttributionProbe
                .FormatReadinessHookupEnd(StepReminders, 3.5)
                .Should()
                .Be("[readiness-hookup] step=OlReminders end elapsedMs=3.50");
            StartupInboxAttributionProbe
                .FormatReadinessHookupEnd(StepInboxes, 121000.7)
                .Should()
                .Be("[readiness-hookup] step=Inboxes end elapsedMs=121000.70");
        }

        [TestMethod]
        public void FormatLoadInboxesStore_IncludedStore_RendersGetDefaultFolderMs()
        {
            // Arrange / Act: an included store carries the GetDefaultFolder timing.
            var line = StartupInboxAttributionProbe.FormatLoadInboxesStore(
                "Mailbox - Dan",
                1.25,
                included: true,
                getDefaultFolderMs: 4.5
            );

            // Assert: exact included-case line shape.
            line.Should()
                .Be(
                    "[loadinboxes] store=Mailbox - Dan shouldIncludeMs=1.25 included=true getDefaultFolderMs=4.50"
                );
        }

        [TestMethod]
        public void FormatLoadInboxesStore_ExcludedStore_RendersGetDefaultFolderMsAsNotApplicable()
        {
            // Arrange / Act: an excluded store never runs GetDefaultFolder, so the field is n/a even
            // when a value is supplied (the value must be ignored on the excluded path).
            var line = StartupInboxAttributionProbe.FormatLoadInboxesStore(
                "Public Folders",
                0.75,
                included: false,
                getDefaultFolderMs: 99.9
            );

            // Assert: exact excluded-case line shape with getDefaultFolderMs=n/a.
            line.Should()
                .Be(
                    "[loadinboxes] store=Public Folders shouldIncludeMs=0.75 included=false getDefaultFolderMs=n/a"
                );
        }

        [TestMethod]
        public void FormatLoadInboxesStore_IncludedWithNullGetDefaultFolderMs_RendersNotApplicable()
        {
            // Arrange / Act: defensive — an included store with a null measured value renders n/a
            // rather than throwing.
            var line = StartupInboxAttributionProbe.FormatLoadInboxesStore(
                "Mailbox",
                2.0,
                included: true,
                getDefaultFolderMs: null
            );

            // Assert.
            line.Should().EndWith("getDefaultFolderMs=n/a");
        }

        [TestMethod]
        public void FormatLoadInboxesStore_FractionalMs_RendersWithInvariantCultureDot()
        {
            // Arrange / Act: invariant-culture formatting must use a dot decimal separator regardless
            // of the host machine's current culture.
            var line = StartupInboxAttributionProbe.FormatLoadInboxesStore(
                "Store",
                1234.56,
                included: true,
                getDefaultFolderMs: 7.89
            );

            // Assert: the fractional values render with a dot, not a comma.
            line.Should().Contain("shouldIncludeMs=1234.56 ");
            line.Should().Contain("getDefaultFolderMs=7.89");
            line.Should().NotContain(",");
        }

        [TestMethod]
        public void EmitReadinessHookupStart_CalledOnce_EmitsExactlyOneStartLine()
        {
            // Arrange
            var emitted = new List<string>();
            var probe = new StartupInboxAttributionProbe(s => emitted.Add(s));

            // Act
            probe.EmitReadinessHookupStart(StepInboxes);

            // Assert: exactly one line, matching the formatter output.
            emitted.Should().ContainSingle();
            emitted[0].Should().Be("[readiness-hookup] step=Inboxes start");
        }

        [TestMethod]
        public void EmitReadinessHookupEnd_CalledOnce_EmitsExactlyOneEndLine()
        {
            // Arrange
            var emitted = new List<string>();
            var probe = new StartupInboxAttributionProbe(s => emitted.Add(s));

            // Act
            probe.EmitReadinessHookupEnd(StepReminders, 6.25);

            // Assert.
            emitted.Should().ContainSingle();
            emitted[0].Should().Be("[readiness-hookup] step=OlReminders end elapsedMs=6.25");
        }

        [TestMethod]
        public void EmitLoadInboxesStore_CalledOnce_EmitsExactlyOneAttributionLine()
        {
            // Arrange
            var emitted = new List<string>();
            var probe = new StartupInboxAttributionProbe(s => emitted.Add(s));

            // Act
            probe.EmitLoadInboxesStore("Archive", 0.5, included: true, getDefaultFolderMs: 3.0);

            // Assert.
            emitted.Should().ContainSingle();
            emitted[0]
                .Should()
                .Be(
                    "[loadinboxes] store=Archive shouldIncludeMs=0.50 included=true getDefaultFolderMs=3.00"
                );
        }

        [TestMethod]
        public void Constructor_NullSink_ThrowsArgumentNullException()
        {
            // Act / Assert: the emit sink is a required collaborator.
            System.Action act = () => new StartupInboxAttributionProbe(null!);
            act.Should().Throw<System.ArgumentNullException>();
        }

        [TestMethod]
        public void EmitPerStoreInboxAttribution_IncludedStore_EmitsIncludedLineAndReturnsFolder()
        {
            // Arrange: an included store; GetDefaultFolder returns a (mocked) MAPIFolder.
            var emitted = new List<string>();
            var probe = new StartupInboxAttributionProbe(s => emitted.Add(s));
            var folder = new Mock<MAPIFolder>().Object;
            var getDefaultFolderCalled = false;

            // Act
            var result = AppOlObjects.EmitPerStoreInboxAttribution(
                shouldInclude: () => true,
                getDefaultFolder: () =>
                {
                    getDefaultFolderCalled = true;
                    return folder;
                },
                readDisplayName: () => "Mailbox - Dan",
                probe: probe
            );

            // Assert: the included store returns the folder, invokes GetDefaultFolder, and emits one
            // line with included=true and a getDefaultFolderMs value.
            result.Should().BeSameAs(folder);
            getDefaultFolderCalled.Should().BeTrue();
            emitted.Should().ContainSingle();
            emitted[0].Should().StartWith("[loadinboxes] store=Mailbox - Dan ");
            emitted[0].Should().Contain("included=true ");
            emitted[0].Should().NotContain("getDefaultFolderMs=n/a");
        }

        [TestMethod]
        public void EmitPerStoreInboxAttribution_ExcludedStore_EmitsNotApplicableAndSkipsGetDefaultFolder()
        {
            // Arrange: an excluded store. GetDefaultFolder must NOT be invoked.
            var emitted = new List<string>();
            var probe = new StartupInboxAttributionProbe(s => emitted.Add(s));
            var getDefaultFolderCalled = false;

            // Act
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

            // Assert: excluded => null result (caller skips add), GetDefaultFolder not called, and the
            // emitted line carries included=false and getDefaultFolderMs=n/a.
            result.Should().BeNull();
            getDefaultFolderCalled.Should().BeFalse();
            emitted.Should().ContainSingle();
            emitted[0].Should().Contain("included=false ");
            emitted[0].Should().EndWith("getDefaultFolderMs=n/a");
        }

        [TestMethod]
        public void EmitPerStoreInboxAttribution_DisplayNameReadThrows_EmitsUnavailableSentinel()
        {
            // Arrange: the caller's guarded DisplayName delegate returns a sentinel when the COM read
            // throws (mirroring LoadInboxes' guarded read).
            var emitted = new List<string>();
            var probe = new StartupInboxAttributionProbe(s => emitted.Add(s));

            string GuardedDisplayName()
            {
                try
                {
                    throw new COMException("DisplayName unavailable");
                }
                catch (COMException)
                {
                    return "<unavailable>";
                }
            }

            // Act
            var result = AppOlObjects.EmitPerStoreInboxAttribution(
                shouldInclude: () => false,
                getDefaultFolder: () => new Mock<MAPIFolder>().Object,
                readDisplayName: GuardedDisplayName,
                probe: probe
            );

            // Assert: the sentinel store name is emitted verbatim.
            result.Should().BeNull();
            emitted.Should().ContainSingle();
            emitted[0].Should().StartWith("[loadinboxes] store=<unavailable> ");
        }

        [TestMethod]
        public void EmitPerStoreInboxAttribution_GetDefaultFolderThrowsComException_PropagatesUnchanged()
        {
            // Arrange: an included store whose GetDefaultFolder throws a COMException. The extracted
            // method must NOT swallow or alter it so the caller's rethrow path is preserved.
            var emitted = new List<string>();
            var probe = new StartupInboxAttributionProbe(s => emitted.Add(s));
            var thrown = new COMException("store not ready");

            // Act
            System.Action act = () =>
                AppOlObjects.EmitPerStoreInboxAttribution(
                    shouldInclude: () => true,
                    getDefaultFolder: () => throw thrown,
                    readDisplayName: () => "Mailbox",
                    probe: probe
                );

            // Assert: the same COMException propagates; no line is emitted because the throw occurs
            // before the included-line emission.
            act.Should().Throw<COMException>().Which.Should().BeSameAs(thrown);
            emitted.Should().BeEmpty();
        }
    }
}
