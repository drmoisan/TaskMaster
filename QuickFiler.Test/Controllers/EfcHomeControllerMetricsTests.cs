using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Helper_Classes;
using TaskMaster;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcHomeControllerMetricsTests
    {
        private const string MailEntryId = "entry-1";
        private const string DocumentsRoot = "Documents";

        private static readonly DateTime MetricsNow = new DateTime(2026, 7, 4, 13, 5, 0);

        [TestMethod]
        public void BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines()
        {
            EfcHomeController
                .BuildQuickFileMetricLines(MetricsNow, 120, "Archive/Target", null)
                .Should()
                .BeEmpty();
            EfcHomeController
                .BuildQuickFileMetricLines(
                    MetricsNow,
                    120,
                    "Archive/Target",
                    new List<MailItemHelper>()
                )
                .Should()
                .BeEmpty();
        }

        [TestMethod]
        public void BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine()
        {
            var result = Build(120, MovedItems(1));

            result
                .Should()
                .Equal(
                    "07/04/2026,01:05,Quarterly Update,SingleSorted,120,2.00,Recipient,Sender,Email,Archive/Target,06/30/2026,09:45:10"
                );
        }

        /// <summary>
        /// The metrics row is a 12-field CSV record. A missing separator between the recipient and
        /// sender fields collapses two fields into one and yields 11.
        /// </summary>
        [TestMethod]
        public void BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields()
        {
            var result = Build(120, MovedItems(1));

            result.Should().ContainSingle();
            result[0]
                .Split(',')
                .Should()
                .HaveCount(12, "the session-metrics row carries exactly twelve fields");
        }

        /// <summary>
        /// Free-text fields must be sanitized before they reach the CSV row, so a comma embedded in
        /// a recipient, sender, or folder name must not add a field.
        /// </summary>
        [TestMethod]
        public void BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields()
        {
            var result = EfcHomeController.BuildQuickFileMetricLines(
                MetricsNow,
                120,
                "Archive, Target",
                MovedItems(1, "Doe, Jane", "Roe, Richard")
            );

            result.Should().ContainSingle();
            result[0]
                .Split(',')
                .Should()
                .HaveCount(
                    12,
                    "commas embedded in free-text fields must be sanitized, not add fields"
                );
        }

        /// <summary>
        /// Pins the deliberate behavior change from integer to real division: 8 seconds over 3 moved
        /// items is 2.6667, which renders as 3 seconds and 0.04 minutes, not as 2 and 0.03.
        /// </summary>
        [TestMethod]
        public void BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding()
        {
            var result = Build(8, MovedItems(3));

            result.Should().HaveCount(3);
            result
                .Should()
                .OnlyContain(
                    line => line.Contains(",3,0.04,"),
                    "8 seconds over 3 items is 2.6667, which rounds to 3 seconds and 0.04 minutes"
                );
        }

        /// <summary>
        /// A deliberate pin rather than a regression: it passes both before and after the fix,
        /// because the 0-59 truncation defect lives where the TimeSpan component is read rather than
        /// inside this method.
        /// </summary>
        [TestMethod]
        public void BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration()
        {
            var result = Build(90, MovedItems(1));

            result.Should().ContainSingle();
            result[0]
                .Should()
                .Contain(",90,1.50,", "90 seconds must render as 90, not as a 0-59 component");
        }

        /// <summary>
        /// The metrics file is machine-read, so its numeric fields must not follow the operator's
        /// locale. Under de-DE the pre-fix source renders 2,00 and splits the row into 13 fields.
        /// </summary>
        [TestMethod]
        public void BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                var result = Build(120, MovedItems(1));

                result.Should().ContainSingle();
                result[0]
                    .Should()
                    .Contain(",2.00,", "numeric fields must use the invariant decimal separator");
                result[0]
                    .Split(',')
                    .Should()
                    .HaveCount(12, "a locale decimal comma must not add a field");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        public void QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter()
        {
            var recorder = new WriteRecorder();
            var controller = CreateController(WithDocuments(), recorder.Write);

            controller.QuickFileMetrics_WRITE("metrics.csv", "Archive", MovedItems(1), 60);

            recorder.Writes.Should().ContainSingle();
            recorder.Writes[0].Filename.Should().Be("metrics.csv");
            recorder.Writes[0].FolderRoot.Should().Be(DocumentsRoot);
            recorder.Writes[0].Lines.Should().ContainSingle().Which.Should().Contain("Quarterly");
        }

        [TestMethod]
        public void QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter()
        {
            var recorder = new WriteRecorder();
            var controller = CreateController(NoSpecialFolders(), recorder.Write);

            controller.QuickFileMetrics_WRITE("metrics.csv", "Archive", MovedItems(1), 60);

            recorder.Writes.Should().BeEmpty();
        }

        [TestMethod]
        public void QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter()
        {
            var recorder = new WriteRecorder();
            var controller = CreateController(WithDocuments(), recorder.Write);

            controller.QuickFileMetrics_WRITE("metrics.csv", "Archive", null, 60);
            controller.QuickFileMetrics_WRITE(
                "metrics.csv",
                "Archive",
                new List<MailItemHelper>(),
                60
            );

            recorder.Writes.Should().BeEmpty();
        }

        /// <summary>
        /// A stopwatch that is allocated but never started reports a zero interval forever, which is
        /// what makes every EmailFiler duration zero. Construction must leave it running.
        /// </summary>
        [TestMethod]
        public void StopWatch_AfterControllerConstruction_IsRunning()
        {
            var controller = CreateController(
                WithDocuments(),
                new WriteRecorder().Write,
                withMail: true
            );

            controller
                .StopWatch.Should()
                .NotBeNull("the constructor must reach the stopwatch construction site");
            controller
                .StopWatch.IsRunning.Should()
                .BeTrue("a stopwatch that is never started measures nothing");
        }

        /// <summary>
        /// The single-argument interface overload must be a silent no-op when its prerequisites are
        /// absent, matching the existing guard precedent, rather than throwing at the caller.
        /// </summary>
        [TestMethod]
        public void QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow()
        {
            var recorder = new WriteRecorder();
            var controller = CreateController(NoSpecialFolders(), recorder.Write);

            System.Action act = () => controller.QuickFileMetrics_WRITE("metrics.csv");

            act.Should()
                .NotThrow("absent prerequisites must produce a silent no-op, not an exception");
            recorder.Writes.Should().BeEmpty();
        }

        /// <summary>
        /// With its prerequisites present the single-argument overload must delegate to the
        /// three-argument overload, and the delegation is observable through the line-writer seam.
        /// </summary>
        [TestMethod]
        public void QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload()
        {
            var recorder = new WriteRecorder();
            var controller = CreateController(
                WithDocuments(),
                recorder.Write,
                withMail: true,
                conversationSameFolder: MovedItems(1)
            );

            controller.QuickFileMetrics_WRITE("metrics.csv");

            recorder.Writes.Should().ContainSingle("the overload must delegate, not no-op");
            recorder.Writes[0].Filename.Should().Be("metrics.csv");
            recorder.Writes[0].FolderRoot.Should().Be(DocumentsRoot);
            recorder.Writes[0].Lines.Should().ContainSingle();
        }

        /// <summary>
        /// The guard must refuse re-entry. The assertion is deliberately sequential: a concurrent
        /// assertion on a compare-and-swap is not deterministic and must not be attempted.
        /// </summary>
        [TestMethod]
        public void TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse()
        {
            var controller = CreateController(NoSpecialFolders(), new WriteRecorder().Write);

            controller.TryBeginExecuteMoves().Should().BeTrue("the first call must take the guard");
            controller
                .TryBeginExecuteMoves()
                .Should()
                .BeFalse("a second call before reset must be refused");
        }

        /// <summary>
        /// Resetting the guard must re-arm it, so a later move can begin.
        /// </summary>
        [TestMethod]
        public void TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue()
        {
            var controller = CreateController(NoSpecialFolders(), new WriteRecorder().Write);

            controller.TryBeginExecuteMoves();
            controller.ResetExecuteMovesState();

            controller
                .TryBeginExecuteMoves()
                .Should()
                .BeTrue("resetting the guard must allow a later move to begin");
        }

        // elapsedSeconds is declared int so this helper compiles against both the pre-fix int
        // parameter and the post-fix double parameter of BuildQuickFileMetricLines.
        private static string[] Build(int elapsedSeconds, List<MailItemHelper> moved)
        {
            return EfcHomeController.BuildQuickFileMetricLines(
                MetricsNow,
                elapsedSeconds,
                "Archive/Target",
                moved
            );
        }

        private static Dictionary<string, string> WithDocuments()
        {
            return new Dictionary<string, string> { ["MyDocuments"] = DocumentsRoot };
        }

        private static Dictionary<string, string> NoSpecialFolders()
        {
            return new Dictionary<string, string>();
        }

        private static List<MailItemHelper> MovedItems(
            int count,
            string toRecipientsName = "Recipient",
            string senderName = "Sender"
        )
        {
            var items = new List<MailItemHelper>();
            for (var index = 0; index < count; index++)
            {
                items.Add(
                    new MailItemHelper
                    {
                        Subject = "Quarterly Update",
                        ToRecipientsName = toRecipientsName,
                        SenderName = senderName,
                        SentDate = new DateTime(2026, 6, 30, 9, 45, 10),
                        EntryId = MailEntryId,
                    }
                );
            }

            return items;
        }

        /// <summary>
        /// Builds a controller whose every collaborator is a headless stub, so no window handle is
        /// created and no Outlook process is required. When <paramref name="withMail"/> is true the
        /// constructor takes its mail-bearing branch and reaches the stopwatch construction site.
        /// </summary>
        private static EfcHomeController CreateController(
            IDictionary<string, string> specialFolders,
            System.Action<string, string[], string> writer,
            bool withMail = false,
            List<MailItemHelper> conversationSameFolder = null
        )
        {
            var globals = new FakeApplicationGlobals(
                new FakeFileSystemFolderPaths(
                    new ConcurrentDictionary<string, string>(specialFolders)
                )
            );
            var dataModel = withMail
                ? CreateDataModelWithMail(conversationSameFolder)
                : CreateDataModelWithoutMail();
            var dependencies = new EfcHomeControllerDependencies(
                dataModelFactory: (factoryGlobals, mail, tokenSource, token) => dataModel,
                viewerFactory: CreateHeadless<EfcViewer>,
                keyboardHandlerFactory: (viewer, home) => null,
                explorerControllerFactory: (initType, factoryGlobals, home) => null,
                formControllerWithDataFactory: (g, model, viewer, home, cleanup, initType, token) =>
                    CreateHeadless<EfcFormController>(),
                metricsNowFactory: () => MetricsNow,
                metricsLineWriter: writer
            );

            return new EfcHomeController(globals, () => { }, dependencies);
        }

        private static EfcDataModel CreateDataModelWithoutMail()
        {
            var dataModel = CreateHeadless<EfcDataModel>();
            dataModel.Mail = null;
            return dataModel;
        }

        private static EfcDataModel CreateDataModelWithMail(
            List<MailItemHelper> conversationSameFolder
        )
        {
            var mail = new Mock<MailItem>(MockBehavior.Loose);
            mail.Setup(item => item.EntryID).Returns(MailEntryId);
            var dataModel = CreateHeadless<EfcDataModel>();
            dataModel.Mail = mail.Object;

            var resolver = CreateHeadless<ConversationResolver>();
            var items = conversationSameFolder ?? new List<MailItemHelper>();
            SetPrivateField(resolver, "_mailItem", mail.Object);
            SetPrivateField(
                resolver,
                "_convInfoFields",
                new Pair<List<MailItemHelper>>(items, items)
            );
            SetPrivateField(dataModel, "_conversationResolver", resolver);
            return dataModel;
        }

        /// <summary>
        /// Allocates an instance without running its constructor, so no WinForms handle, message
        /// pump, or synchronization context is required. The finalizer is suppressed because the
        /// instance's fields are deliberately left uninitialized.
        /// </summary>
        private static T CreateHeadless<T>()
        {
            var instance = (T)FormatterServices.GetUninitializedObject(typeof(T));
            GC.SuppressFinalize(instance);
            return instance;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must exist on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private sealed class WriteRecorder
        {
            internal List<MetricWrite> Writes { get; } = new List<MetricWrite>();

            internal void Write(string filename, string[] lines, string folderRoot)
            {
                Writes.Add(new MetricWrite(filename, lines, folderRoot));
            }
        }

        private sealed class MetricWrite
        {
            internal MetricWrite(string filename, string[] lines, string folderRoot)
            {
                Filename = filename;
                Lines = lines;
                FolderRoot = folderRoot;
            }

            internal string Filename { get; }
            internal string[] Lines { get; }
            internal string FolderRoot { get; }
        }

        private sealed class FakeApplicationGlobals : IApplicationGlobals
        {
            internal FakeApplicationGlobals(IFileSystemFolderPaths fileSystem)
            {
                FS = fileSystem;
            }

            public Task LoadAsync(bool parallel)
            {
                return Task.CompletedTask;
            }

            public IFileSystemFolderPaths FS { get; }
            public IOlObjects Ol => null;
            public IToDoObjects TD => null;
            public IAppAutoFileObjects AF => null;
            public IAppEvents Events => null;
            public IAppQuickFilerSettings QfSettings => null;
            public IAppItemEngines Engines => null;
            public IntelligenceConfig IntelRes => null;
            public IStoreDisableService StoreDisable => null;
        }

        private sealed class FakeFileSystemFolderPaths : IFileSystemFolderPaths
        {
            internal FakeFileSystemFolderPaths(ConcurrentDictionary<string, string> specialFolders)
            {
                SpecialFolders = specialFolders;
            }

            public ConcurrentDictionary<string, string> SpecialFolders { get; }
            public IAppStagingFilenames Filenames => null;

            public void Reload() { }

            public string MatchBestSpecialFolder(string path)
            {
                return null;
            }
        }
    }
}
