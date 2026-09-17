using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #742 regression tests. Five QuickFiler production files render dates and times with an
    /// uncultured <c>ToString(format)</c> call or an interpolated format specifier, both of which
    /// resolve the <c>/</c> and <c>:</c> placeholders against
    /// <see cref="CultureInfo.CurrentCulture"/>. Under an operator locale whose date or time
    /// separator is not <c>/</c> or <c>:</c>, the rendered output changes shape: the session-metrics
    /// CSV gains or loses field boundaries, and the user-facing summaries and diagnostics no longer
    /// match the format the rest of the system assumes.
    /// <para>
    /// Each test activates a culture whose <c>DateSeparator</c> and <c>TimeSeparator</c> are
    /// explicit sentinel characters that are neither <c>/</c> nor <c>:</c>, exercises one production
    /// file, and asserts that the rendered output still carries the invariant separators. A named
    /// culture is deliberately not used: a sentinel makes the assertion independent of any
    /// particular locale's separator choices, and of any future ICU data change.
    /// </para>
    /// <para>
    /// Every test restores the original culture in a <c>finally</c> block, mirroring the two
    /// existing de-DE tests in <c>QfcHomeControllerMetricsTests</c> and
    /// <c>EfcHomeControllerMetricsTests</c>. MSTest does not reset
    /// <see cref="CultureInfo.CurrentCulture"/> between test methods, so an unrestored culture would
    /// leak into whichever sibling test next runs on the same worker thread.
    /// </para>
    /// <para>
    /// No test here touches a temporary file, sleeps, or delays.
    /// </para>
    /// </summary>
    [TestClass]
    public class QuickFilerInvariantCultureIssue742Tests
    {
        private const string DateSeparatorSentinel = "#";
        private const string TimeSeparatorSentinel = "~";
        private const string FakeDocumentsRoot = @"C:\FakeDocs";

        /// <summary>A fixed instant whose date and time parts both render with a separator.</summary>
        private static readonly DateTime SentInstant = new DateTime(2024, 3, 4, 14, 30, 45);

        /// <summary>
        /// Builds a culture whose date and time separators are sentinel characters. Cloning the
        /// invariant culture keeps every other formatting rule invariant, so a failure is
        /// attributable to the separator alone.
        /// </summary>
        private static CultureInfo SentinelSeparatorCulture()
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.DateTimeFormat.DateSeparator = DateSeparatorSentinel;
            culture.DateTimeFormat.TimeSeparator = TimeSeparatorSentinel;
            return culture;
        }

        private static string ExpectedDate(DateTime value) =>
            value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

        private static string ExpectedTime(DateTime value) =>
            value.ToString("HH:mm", CultureInfo.InvariantCulture);

        /// <summary>
        /// Sets a non-public instance field, asserting first that the field exists so a renamed
        /// member fails loudly instead of leaving the field at its default.
        /// </summary>
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Builds a <see cref="MailItemHelper"/> directly, with only the fields the rendering paths
        /// read. None of the assigned text contains <c>/</c> or <c>:</c>, so either separator
        /// appearing in a rendered result is attributable to the date/time formatting under test.
        /// </summary>
        private static MailItemHelper BuildItemHelper() =>
            new MailItemHelper
            {
                SentDate = SentInstant,
                Subject = "Subject text",
                SenderName = "Sender name",
                ToRecipientsName = "Recipient name",
            };

        /// <summary>
        /// Builds a <see cref="QfcHomeController"/> with loose mocks wired so the synchronous
        /// metrics path runs to its <c>GetMoveDiagnostics</c> call without touching live COM: the
        /// special-folders map carries MyDocuments, the calendar root enumerates no subfolders so
        /// the calendar resolves to null, and the diagnostics call returns an empty array so the
        /// downstream write iterates nothing. This mirrors <c>BuildLooseMetricsController</c> in
        /// <c>QfcHomeControllerMetricsTests</c>, with the diagnostics setup changed to capture the
        /// <c>dataLineBeg</c> argument this test asserts on.
        /// </summary>
        private static QfcHomeController BuildMetricsController(
            List<string> capturedDataLineBeginnings
        )
        {
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Loose);

            var specialFolders = new ConcurrentDictionary<string, string>();
            specialFolders["MyDocuments"] = FakeDocumentsRoot;
            var mockFs = new Mock<IFileSystemFolderPaths>(MockBehavior.Loose);
            mockFs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);

            var mockFolders = new Mock<Folders>(MockBehavior.Loose);
            mockFolders
                .Setup(x => x.GetEnumerator())
                .Returns(() => new ArrayList().GetEnumerator());
            var mockCalendarRoot = new Mock<Folder>(MockBehavior.Loose);
            mockCalendarRoot.SetupGet(x => x.Folders).Returns(mockFolders.Object);
            var mockSession = new Mock<NameSpace>(MockBehavior.Loose);
            mockSession
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderCalendar))
                .Returns(mockCalendarRoot.Object);
            var mockOlApp = new Mock<Outlook.Application>(MockBehavior.Loose);
            mockOlApp.SetupGet(x => x.Session).Returns(mockSession.Object);
            var mockOl = new Mock<IOlObjects>(MockBehavior.Loose);
            mockOl.SetupGet(x => x.App).Returns(mockOlApp.Object);
            mockGlobals.SetupGet(x => x.Ol).Returns(mockOl.Object);

            var mockGroups = new Mock<IQfcCollectionController>(MockBehavior.Loose);
            mockGroups.SetupGet(x => x.EmailsToMove).Returns(1);
            mockGroups
                .Setup(x =>
                    x.GetMoveDiagnostics(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<double>(),
                        Capture.In(capturedDataLineBeginnings),
                        It.IsAny<DateTime>(),
                        ref It.Ref<AppointmentItem>.IsAny
                    )
                )
                .Returns(Array.Empty<string>());
            var mockFormController = new Mock<IQfcFormController>(MockBehavior.Loose);
            mockFormController.SetupGet(x => x.Groups).Returns(mockGroups.Object);

            mockGlobals.SetupGet(x => x.AF.CancelToken).Returns(CancellationToken.None);

            var controller = new QfcHomeController(mockGlobals.Object, () => { });
            controller.CreateCancellationToken();
            controller.MetricsFileWriter = (filename, lines, folderRoot, token) =>
                Task.FromResult(true);
            SetPrivateField(controller, "_formController", mockFormController.Object);
            SetPrivateField(controller, "_stopWatchMoved", new Stopwatch());
            return controller;
        }

        /// <summary>
        /// Test 1 of 5, for <c>QuickFiler/Controllers/QfcHomeController.Metrics.cs</c>.
        /// <para>
        /// Scenario: run <c>QuickFileMetrics_WRITE</c> under a sentinel-separator culture with an
        /// injected fixed clock. Expected outcome: the <c>dataLineBeg</c> argument handed to
        /// <c>GetMoveDiagnostics</c> is the invariant rendering of the injected instant, so it
        /// carries <c>/</c> and <c>:</c> rather than the sentinel characters.
        /// </para>
        /// <para>
        /// Before the fix the beginning of the data line is built from an interpolated format
        /// specifier, which resolves both separators against the current culture, so the
        /// machine-read metrics CSV changes shape with the operator's locale.
        /// </para>
        /// </summary>
        [TestMethod]
        public void QuickFileMetricsWrite_UnderSentinelSeparatorCulture_RendersInvariantDataLineBeginning()
        {
            // Arrange
            var captured = new List<string>();
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = SentinelSeparatorCulture();
                var controller = BuildMetricsController(captured);
                var fake = new FakeTimeProvider(
                    new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero)
                );
                controller.TimeProvider = fake;
                var expectedLocal = fake.GetLocalNow().LocalDateTime;
                var expected =
                    ExpectedDate(expectedLocal) + "," + ExpectedTime(expectedLocal) + ",";

                // Act
                controller.QuickFileMetrics_WRITE("issue742-metrics.csv");

                // Assert
                captured
                    .Should()
                    .ContainSingle(
                        because: "the metrics path calls GetMoveDiagnostics exactly once per write"
                    );
                captured[0]
                    .Should()
                    .Be(
                        expected,
                        because: "issue #742 requires the data line to be rendered with the invariant culture"
                    )
                    .And.Contain("/")
                    .And.Contain(":");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Test 2 of 5, for <c>QuickFiler/Controllers/EfcHomeController.Metrics.cs</c>.
        /// <para>
        /// Scenario: call <c>BuildQuickFileMetricLines</c> directly under a sentinel-separator
        /// culture with a literal instant and a one-item moved list. Expected outcome: the returned
        /// line carries the invariant rendering of both the leading date/time fields and the
        /// trailing sent-date fields.
        /// </para>
        /// </summary>
        [TestMethod]
        public void BuildQuickFileMetricLines_UnderSentinelSeparatorCulture_RendersInvariantDateAndTime()
        {
            // Arrange
            var moved = new List<MailItemHelper> { BuildItemHelper() };
            var currentDateTime = new DateTime(2024, 1, 15, 14, 30, 45);
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = SentinelSeparatorCulture();

                // Act
                string[] lines = EfcHomeController.BuildQuickFileMetricLines(
                    currentDateTime,
                    60d,
                    "Destination folder",
                    moved
                );

                // Assert
                lines.Should().ContainSingle(because: "the moved list carries exactly one item");
                lines[0]
                    .Should()
                    .Contain("/")
                    .And.Contain(":")
                    .And.Contain(
                        ExpectedDate(currentDateTime),
                        because: "the leading date field must be rendered with the invariant culture"
                    )
                    .And.Contain(
                        ExpectedDate(SentInstant),
                        because: "the trailing sent-date field must be rendered with the invariant culture"
                    )
                    .And.NotContain(DateSeparatorSentinel)
                    .And.NotContain(TimeSeparatorSentinel);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Test 3 of 5, for <c>QuickFiler/Controllers/QfcItemController.ViewerSetup.cs</c>.
        /// <para>
        /// Scenario: read <c>GetItemSummary()</c> from a controller allocated without its
        /// WinForms-dependent constructor under a sentinel-separator culture. Expected outcome: the
        /// summary carries the invariant rendering of the sent date and time.
        /// </para>
        /// <para>
        /// The assertion names the expected rendered substrings rather than only the separator
        /// characters, because the summary's own literal prefix already contains a colon and a bare
        /// colon assertion would therefore pass on the unfixed code.
        /// </para>
        /// </summary>
        [TestMethod]
        public void GetItemSummary_UnderSentinelSeparatorCulture_RendersInvariantDateAndTime()
        {
            // Arrange
            var controller = (QfcItemController)
                FormatterServices.GetUninitializedObject(typeof(QfcItemController));
            controller.ItemHelper = BuildItemHelper();
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = SentinelSeparatorCulture();

                // Act
                string summary = controller.GetItemSummary();

                // Assert
                summary
                    .Should()
                    .Contain("/")
                    .And.Contain(":")
                    .And.Contain(
                        ExpectedDate(SentInstant),
                        because: "issue #742 requires the sent date to be rendered with the invariant culture"
                    )
                    .And.Contain(
                        ExpectedTime(SentInstant),
                        because: "issue #742 requires the sent time to be rendered with the invariant culture"
                    );
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Test 4 of 5, for <c>QuickFiler/Controllers/QfcCollectionController.cs</c>.
        /// <para>
        /// Scenario: drive the file's three rendering sites from one controller allocated without
        /// its WinForms-dependent constructor under a sentinel-separator culture: the move-readiness
        /// notification, the expansion-guard exception message, and the move-diagnostics data line.
        /// Expected outcome: each output carries the invariant rendering.
        /// </para>
        /// <para>
        /// The move-readiness notification is asserted on the date only. That site renders a date
        /// and no time, so a colon assertion against it could not pass after the fix either.
        /// </para>
        /// </summary>
        [TestMethod]
        public void QfcCollectionControllerRenderingSites_UnderSentinelSeparatorCulture_RenderInvariantDateAndTime()
        {
            // Arrange
            var helper = BuildItemHelper();
            var mockMail = new Mock<Outlook.MailItem>(MockBehavior.Loose);
            mockMail.SetupGet(x => x.SentOn).Returns(SentInstant);
            mockMail.SetupGet(x => x.Subject).Returns("Subject text");
            var mockItemController = new Mock<IQfcItemController>(MockBehavior.Loose);
            mockItemController.SetupGet(x => x.Mail).Returns(mockMail.Object);
            mockItemController.SetupGet(x => x.ItemHelper).Returns(helper);
            mockItemController.SetupGet(x => x.ItemNumber).Returns(1);
            mockItemController.SetupGet(x => x.IsActiveUI).Returns(false);
            var group = new QfcItemGroup { ItemController = mockItemController.Object };

            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_itemGroups",
                new List<QfcItemGroup> { group }
            );
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_itemGroupsToMove",
                new List<QfcItemGroup> { group }
            );

            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = SentinelSeparatorCulture();

                // Act
                controller.TryGetMoveReadiness(out string notifications);
                System.Action expandInactiveItem = () =>
                    controller.ToggleExpansionStyle(0, Enums.ToggleState.On);
                AppointmentItem appointment = null;
                string[] diagnostics = controller.GetMoveDiagnostics(
                    "0",
                    "0.00",
                    0d,
                    "beg,",
                    SentInstant,
                    ref appointment
                );

                // Assert
                notifications
                    .Should()
                    .Contain("/")
                    .And.Contain(
                        ExpectedDate(SentInstant),
                        because: "the move-readiness notification must render its date with the invariant culture"
                    );
                expandInactiveItem
                    .Should()
                    .Throw<InvalidOperationException>()
                    .Which.Message.Should()
                    .Contain("/")
                    .And.Contain(":")
                    .And.Contain(ExpectedDate(SentInstant))
                    .And.Contain(ExpectedTime(SentInstant));
                diagnostics.Should().ContainSingle(because: "exactly one group is cached for move");
                diagnostics[0]
                    .Should()
                    .Contain("/")
                    .And.Contain(":")
                    .And.Contain(ExpectedDate(SentInstant))
                    .And.Contain(ExpectedTime(SentInstant));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Test 5 of 5, for <c>QuickFiler/Controllers/EfcItemController.cs</c>.
        /// <para>
        /// Scenario: read the <c>SentDate</c> and <c>SentTime</c> properties from a controller
        /// allocated without its constructor, with the cached mail-item model injected by
        /// reflection, under a sentinel-separator culture. Expected outcome: <c>SentDate</c> carries
        /// the invariant date separator and <c>SentTime</c> the invariant time separator.
        /// </para>
        /// </summary>
        [TestMethod]
        public void EfcItemControllerSentDateAndSentTime_UnderSentinelSeparatorCulture_RenderInvariantSeparators()
        {
            // Arrange
            var controller = (EfcItemController)
                FormatterServices.GetUninitializedObject(typeof(EfcItemController));
            SetPrivateField(controller, "_itemInfo", BuildItemHelper());
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = SentinelSeparatorCulture();

                // Act
                string sentDate = controller.SentDate;
                string sentTime = controller.SentTime;

                // Assert
                sentDate
                    .Should()
                    .Contain("/")
                    .And.Be(
                        ExpectedDate(SentInstant),
                        because: "issue #742 requires SentDate to be rendered with the invariant culture"
                    );
                sentTime
                    .Should()
                    .Contain(":")
                    .And.Be(
                        ExpectedTime(SentInstant),
                        because: "issue #742 requires SentTime to be rendered with the invariant culture"
                    );
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }
}
