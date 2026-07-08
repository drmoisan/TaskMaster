using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcHomeControllerMetricsTests
    {
        [TestMethod]
        public void BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines()
        {
            var now = new DateTime(2026, 7, 4, 13, 5, 0);

            EfcHomeController
                .BuildQuickFileMetricLines(now, 120, "Archive/Target", null)
                .Should()
                .BeEmpty();
            EfcHomeController
                .BuildQuickFileMetricLines(now, 120, "Archive/Target", new List<MailItemHelper>())
                .Should()
                .BeEmpty();
        }

        [TestMethod]
        public void BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine()
        {
            var now = new DateTime(2026, 7, 4, 13, 5, 0);
            var moved = new List<MailItemHelper>
            {
                new MailItemHelper
                {
                    Subject = "Quarterly Update",
                    ToRecipientsName = "Recipient",
                    SenderName = "Sender",
                    SentDate = new DateTime(2026, 6, 30, 9, 45, 10),
                },
            };

            var result = EfcHomeController.BuildQuickFileMetricLines(
                now,
                120,
                "Archive/Target",
                moved
            );

            result
                .Should()
                .Equal(
                    "07/04/2026,01:05,Quarterly Update,SingleSorted,120,2.00,RecipientSender,Email,Archive/Target,06/30/2026,09:45:10"
                );
        }

        [TestMethod]
        public void QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter()
        {
            var writes = new List<MetricWrite>();
            var controller = CreateController(
                new Dictionary<string, string> { ["MyDocuments"] = "C:/Users/Test/Documents" },
                (filename, lines, folderRoot) =>
                    writes.Add(new MetricWrite(filename, lines, folderRoot))
            );
            var moved = new List<MailItemHelper>
            {
                new MailItemHelper
                {
                    Subject = "Subject",
                    ToRecipientsName = "To",
                    SenderName = "From",
                    SentDate = new DateTime(2026, 7, 1, 8, 0, 0),
                },
            };

            controller.QuickFileMetrics_WRITE("metrics.csv", "Archive", moved, 60);

            writes.Should().ContainSingle();
            writes[0].Filename.Should().Be("metrics.csv");
            writes[0].FolderRoot.Should().Be("C:/Users/Test/Documents");
            writes[0].Lines.Should().ContainSingle().Which.Should().Contain("Subject");
        }

        [TestMethod]
        public void QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter()
        {
            var writes = new List<MetricWrite>();
            var controller = CreateController(
                new Dictionary<string, string>(),
                (filename, lines, folderRoot) =>
                    writes.Add(new MetricWrite(filename, lines, folderRoot))
            );
            var moved = new List<MailItemHelper>
            {
                new MailItemHelper
                {
                    Subject = "Subject",
                    ToRecipientsName = "To",
                    SenderName = "From",
                    SentDate = new DateTime(2026, 7, 1, 8, 0, 0),
                },
            };

            controller.QuickFileMetrics_WRITE("metrics.csv", "Archive", moved, 60);

            writes.Should().BeEmpty();
        }

        [TestMethod]
        public void QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter()
        {
            var writes = new List<MetricWrite>();
            var controller = CreateController(
                new Dictionary<string, string> { ["MyDocuments"] = "C:/Users/Test/Documents" },
                (filename, lines, folderRoot) =>
                    writes.Add(new MetricWrite(filename, lines, folderRoot))
            );

            controller.QuickFileMetrics_WRITE("metrics.csv", "Archive", null, 60);
            controller.QuickFileMetrics_WRITE(
                "metrics.csv",
                "Archive",
                new List<MailItemHelper>(),
                60
            );

            writes.Should().BeEmpty();
        }

        [TestMethod]
        public void QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract()
        {
            var controller = CreateController(
                new Dictionary<string, string>(),
                (filename, lines, folderRoot) => { }
            );

            System.Action act = () => controller.QuickFileMetrics_WRITE("metrics.csv");

            act.Should().Throw<NotImplementedException>();
        }

        private static EfcHomeController CreateController(
            IDictionary<string, string> specialFolders,
            System.Action<string, string[], string> writer
        )
        {
            var globals = new FakeApplicationGlobals(
                new FakeFileSystemFolderPaths(
                    new ConcurrentDictionary<string, string>(specialFolders)
                )
            );
            var dependencies = new EfcHomeControllerDependencies(
                dataModelFactory: (factoryGlobals, mail, tokenSource, token) =>
                    CreateDataModelWithoutMail(),
                metricsNowFactory: () => new DateTime(2026, 7, 4, 13, 5, 0),
                metricsLineWriter: writer
            );

            return new EfcHomeController(globals, () => { }, dependencies);
        }

        private static EfcDataModel CreateDataModelWithoutMail()
        {
            var dataModel = (EfcDataModel)
                FormatterServices.GetUninitializedObject(typeof(EfcDataModel));
            dataModel.Mail = null;
            return dataModel;
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
