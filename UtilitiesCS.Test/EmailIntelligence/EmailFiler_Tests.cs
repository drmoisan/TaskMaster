using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public partial class EmailFiler_Tests
    {
        [TestMethod]
        public void Globals_SetAndGet_RoundTrips()
        {
            var filer = new EmailFiler();
            var globals = new Mock<IApplicationGlobals>();

            filer.Globals = globals.Object;

            filer.Globals.Should().BeSameAs(globals.Object);
        }

        [TestMethod]
        public void OpenFileSystemFolder_WhenPathDoesNotExist_CompletesWithoutThrowing()
        {
            var filer = new EmailFiler();

            filer
                .Invoking(x => x.OpenFileSystemFolder(@"C:\__TaskMaster_Impossible_Path__"))
                .Should()
                .NotThrow();
        }

        [TestMethod]
        public void StripTabsCrLf_WhenInputContainsTabsAndCrLf_ReturnsCleanTrimmedString()
        {
            var filer = new EmailFiler();

            filer.StripTabsCrLf("Hello\t\r\nWorld\t\t!").Should().Be("Hello World !");
        }

        [TestMethod]
        public void ValidateParameters_WhenConfigSuppliesGlobals_AssignsInstanceGlobals()
        {
            var globals = new Mock<IApplicationGlobals>();
            var filer = new EmailFiler(
                new EmailFilerConfig { Globals = globals.Object, CanSort = true }
            )
            {
                MailHelpers = new[] { new MailItemHelper() },
            };

            filer.ValidateParameters();

            filer.Globals.Should().BeSameAs(globals.Object);
        }

        [TestMethod]
        public void TryValidateParameters_WhenInputsAreValid_ReturnsConfigCanSort()
        {
            var globals = new Mock<IApplicationGlobals>();
            var filer = new EmailFiler(
                new EmailFilerConfig { Globals = globals.Object, CanSort = true }
            )
            {
                MailHelpers = new[] { new MailItemHelper() },
            };

            filer.TryValidateParameters().Should().BeTrue();
        }

        [TestMethod]
        public void TryValidateParameters_WhenValidationThrows_ReturnsFalse()
        {
            new EmailFiler().TryValidateParameters().Should().BeFalse();
        }

        [TestMethod]
        public async Task SortAsync_WithMailHelpers_ResolvesPathsAndDelegatesToParameterlessSort()
        {
            var folder = new Mock<Folder>();
            var folderInfo = new Mock<IFolderWrapper>();
            folderInfo.SetupGet(x => x.OlFolder).Returns(folder.Object);
            var helper = new MailItemHelper { FolderInfo = folderInfo.Object };
            var filer = new TrackingEmailFiler { ParameterlessSortResult = true };

            var result = await filer.SortAsync(new[] { helper });

            result.Should().BeTrue();
            filer.MailHelpers.Should().ContainSingle().Which.Should().BeSameAs(helper);
            filer.ResolvedFolder.Should().BeSameAs(folder.Object);
            filer.ParameterlessSortCalls.Should().Be(1);
        }

        [TestMethod]
        public async Task SortAsync_WhenTryValidateParametersFails_ReturnsFalseWithoutProcessing()
        {
            var filer = new TrackingEmailFiler
            {
                TryValidateParametersResult = false,
                MailHelpers = new[] { new MailItemHelper() },
            };

            var result = await ((EmailFiler)filer).SortAsync();

            result.Should().BeFalse();
            filer.ProcessedHelpers.Should().BeEmpty();
            filer.SerializeFolderManagerCalls.Should().Be(0);
        }

        [TestMethod]
        public async Task SortAsync_WhenTryValidateParametersPasses_ProcessesAllMailAndSerializes()
        {
            var first = new MailItemHelper { Subject = "one" };
            var second = new MailItemHelper { Subject = "two" };
            var filer = new TrackingEmailFiler
            {
                TryValidateParametersResult = true,
                UseBaseSortAsync = true,
                MailHelpers = new[] { first, second },
            };

            var result = await ((EmailFiler)filer).SortAsync();

            result.Should().BeTrue();
            filer.ProcessedHelpers.Should().ContainInOrder(first, second);
            filer.SerializeFolderManagerCalls.Should().Be(1);
        }

        [TestMethod]
        public async Task ProcessMailHelperAsync_WhenMoveSucceeds_RunsAllPostMoveActions()
        {
            var helper = new MailItemHelper { Item = new Mock<MailItem>().Object };
            var original = new Mock<MailItem>().Object;
            var moved = new Mock<MailItem>().Object;
            var filer = new TrackingEmailFiler(
                new EmailFilerConfig { SaveMsg = true, SaveFsPath = @"C:\archive" }
            )
            {
                UseBaseProcessMailHelperAsync = true,
                MoveResult = (original, moved),
            };

            await ((EmailFiler)filer).ProcessMailHelperAsync(helper);

            filer.SaveMessageCalls.Should().Be(1);
            filer.SaveAttachmentsCalls.Should().Be(1);
            filer.UnTrainCalls.Should().Be(1);
            filer.StartTrainingMetricsCalls.Should().Be(1);
            filer.LabelCalls.Should().Be(1);
            filer.PushUndoCalls.Should().Be(1);
            filer.CaptureMoveDetailsCalls.Should().Be(1);
        }

        [TestMethod]
        public async Task ProcessMailHelperAsync_WhenMoveFails_SkipsPostMoveActions()
        {
            var helper = new MailItemHelper { Item = new Mock<MailItem>().Object };
            var original = new Mock<MailItem>().Object;
            var filer = new TrackingEmailFiler(new EmailFilerConfig { SaveMsg = false })
            {
                UseBaseProcessMailHelperAsync = true,
                MoveResult = (original, null),
            };

            await ((EmailFiler)filer).ProcessMailHelperAsync(helper);

            filer.SaveMessageCalls.Should().Be(0);
            filer.SaveAttachmentsCalls.Should().Be(1);
            filer.UnTrainCalls.Should().Be(1);
            filer.StartTrainingMetricsCalls.Should().Be(0);
            filer.LabelCalls.Should().Be(0);
            filer.PushUndoCalls.Should().Be(0);
            filer.CaptureMoveDetailsCalls.Should().Be(0);
        }

        [TestMethod]
        public async Task StartTrainingMetrics_WhenCalled_InvokesAllTrainingHooks()
        {
            var filer = new TrackingEmailFiler(
                new EmailFilerConfig { DestinationOlStem = "Archive\\Projects" }
            );
            var helper = new MailItemHelper { Subject = "Quarterly Review", Actionable = "Acted" };

            var tasks = ((EmailFiler)filer).StartTrainingMetrics(helper);
            await Task.WhenAll(tasks);

            filer.TrainFolderCalls.Should().Be(1);
            filer.TrainActionableCalls.Should().Be(1);
            filer.RecordSubjectMapCalls.Should().Be(1);
            filer.RecordRecentDestinationCalls.Should().Be(1);
        }

        [TestMethod]
        public async Task SaveAttachmentsPicturesAsync_WhenSavingAttachmentsOnly_SkipsImages()
        {
            var filer = new TrackingEmailFiler(
                new EmailFilerConfig { SaveAttachments = true, SavePictures = false }
            );
            var helper = new MailItemHelper();
            var document = CreateAttachmentHelper(
                "report.pdf",
                isImage: false,
                @"C:\delete-report"
            );
            var image = CreateAttachmentHelper("chart.png", isImage: true, @"C:\delete-chart");
            filer.AttachmentsToEnumerate.AddRange(new[] { document, image });

            await ((EmailFiler)filer).SaveAttachmentsPicturesAsync(helper);

            filer.SavedAttachments.Should().ContainSingle();
            filer.SavedAttachments[0].AttachmentInfo.FileName.Should().Be("report.pdf");
            filer.SavedAttachments[0].AttachmentInfo.IsImage.Should().BeFalse();
            filer.DeletedFiles.Should().ContainSingle().Which.Should().Be(@"C:\delete-report");
        }

        [TestMethod]
        public async Task SaveAttachmentsPicturesAsync_WhenSavingPicturesOnly_SkipsDocuments()
        {
            var filer = new TrackingEmailFiler(
                new EmailFilerConfig { SaveAttachments = false, SavePictures = true }
            );
            var helper = new MailItemHelper();
            var document = CreateAttachmentHelper(
                "report.pdf",
                isImage: false,
                @"C:\delete-report"
            );
            var image = CreateAttachmentHelper("chart.png", isImage: true, @"C:\delete-chart");
            filer.AttachmentsToEnumerate.AddRange(new[] { document, image });

            await ((EmailFiler)filer).SaveAttachmentsPicturesAsync(helper);

            filer.SavedAttachments.Should().ContainSingle();
            filer.SavedAttachments[0].AttachmentInfo.FileName.Should().Be("chart.png");
            filer.SavedAttachments[0].AttachmentInfo.IsImage.Should().BeTrue();
            filer.DeletedFiles.Should().ContainSingle();
        }

        [TestMethod]
        public async Task SaveMessageAsMsgAsync_WhenCalled_SanitizesSubjectAndUsesMsgFormat()
        {
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.Subject).Returns("Quarterly: Update");
            string savedPath = null;
            OlSaveAsType savedType = default;
            mailItem
                .Setup(x => x.SaveAs(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>(
                    (path, type) =>
                    {
                        savedPath = path;
                        savedType = (OlSaveAsType)type;
                    }
                );

            await new ExposedEmailFiler().SaveMessageAsMsgAsync(mailItem.Object, @"C:\mail");

            savedPath.Should().Contain("Quarterly_ Update");
            savedType.Should().Be(OlSaveAsType.olMSG);
        }

        [TestMethod]
        public async Task TryMoveMailItemForProcessingAsync_WhenMoveSucceeds_ReturnsOriginalAndMoved()
        {
            var destination = new Mock<Folder>();
            var moved = new Mock<MailItem>();
            var original = new Mock<MailItem>();
            original.Setup(x => x.Move(destination.Object)).Returns(moved.Object);
            var helper = new MailItemHelper { Item = original.Object, Subject = "Move me" };
            var filer = new ExposedEmailFiler(
                new EmailFilerConfig { DestinationOlFolder = destination.Object }
            );

            var result = await filer.CallTryMoveMailItemForProcessingAsync(helper);

            result.Original.Should().BeSameAs(original.Object);
            result.Moved.Should().BeSameAs(moved.Object);
        }

        [TestMethod]
        public async Task TryMoveMailItemForProcessingAsync_WhenMoveThrows_ReturnsOriginalAndNullMoved()
        {
            var destination = new Mock<Folder>();
            destination.SetupGet(x => x.FolderPath).Returns(@"\\Mailbox - Root\\Archive");
            var original = new Mock<MailItem>();
            original
                .Setup(x => x.Move(destination.Object))
                .Throws(new InvalidOperationException("move failed"));
            var helper = new MailItemHelper { Item = original.Object, Subject = "Move me" };
            var filer = new ExposedEmailFiler(
                new EmailFilerConfig { DestinationOlFolder = destination.Object }
            );

            var result = await filer.CallTryMoveMailItemForProcessingAsync(helper);

            result.Original.Should().BeSameAs(original.Object);
            result.Moved.Should().BeNull();
        }

        [TestMethod]
        public async Task LabelAutoSortedAsync_WhenCalled_SetsFieldMarksMessageReadAndSaves()
        {
            var property = new Mock<UserProperty>();
            property.SetupProperty(x => x.Value);
            var userProperties = new Mock<UserProperties>();
            userProperties
                .Setup(x => x.Find("AutoSorted", It.IsAny<object>()))
                .Returns((UserProperty)null);
            userProperties
                .Setup(x =>
                    x.Add(
                        "AutoSorted",
                        OlUserPropertyType.olText,
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(property.Object);
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            mailItem.SetupProperty(x => x.UnRead, true);

            await new ExposedEmailFiler().LabelAutoSortedAsync(mailItem.Object);

            property.Object.Value.Should().Be("Yes");
            mailItem.Object.UnRead.Should().BeFalse();
            mailItem.Verify(x => x.Save(), Times.Exactly(2));
        }

        [TestMethod]
        public async Task ManagerHooks_WhenInvoked_UpdateClassifierSubjectMapAndRecents()
        {
            var folderGroup = new BayesianClassifierGroup();
            folderGroup.Train("Inbox", new[] { "alpha", "beta" }, 1);
            var actionableGroup = new BayesianClassifierGroup();
            var manager = CreateManager(folderGroup, actionableGroup);
            var subjectMap = new SubjectMapSco(new SerializableList<string>());
            var recents = new SloLinkedList<string>(new[] { "Existing" });
            var filer = new ExposedEmailFiler(
                new EmailFilerConfig
                {
                    DestinationOlStem = "Archive\\Projects",
                    OriginOlStem = "Inbox",
                }
            )
            {
                Globals = CreateGlobals(
                    manager,
                    subjectMap,
                    recents,
                    new ScoStack<IMovedMailInfo>(),
                    null
                ),
            };
            var helper = new TestMailItemHelper();
            helper.SetTokens("alpha", "beta");
            helper.Actionable = "Acted";
            helper.Subject = "Quarterly Review";

            await filer.CallSerializeFolderManagerAsync();
            await filer.CallUnTrainFolderAsync(helper);
            await filer.CallTrainFolderAsync(helper);
            await filer.CallTrainActionableAsync(helper);
            filer.CallRecordSubjectMap(helper);
            filer.CallRecordRecentDestination();

            folderGroup.Classifiers.Should().ContainKey("Archive\\Projects");
            actionableGroup.Classifiers.Should().ContainKey("Acted");
            subjectMap.Find("Archive\\Projects", Enums.FindBy.Folder).Should().ContainSingle();
            recents.First.Value.Should().Be("Archive\\Projects");
        }

        [TestMethod]
        public async Task TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier()
        {
            var actionableGroup = new BayesianClassifierGroup();
            var manager = CreateManager(new BayesianClassifierGroup(), actionableGroup);
            var filer = new ExposedEmailFiler
            {
                Globals = CreateGlobals(manager, null, null, null, null),
            };
            var helper = new TestMailItemHelper();
            helper.SetTokens("alpha", "beta");
            helper.Actionable = "None";

            await filer.CallTrainActionableAsync(helper);

            actionableGroup.Classifiers.Should().BeEmpty();
        }

        [TestMethod]
        public async Task EnumerateAttachments_WhenHelperContainsAttachments_ReturnsAllConfiguredAttachments()
        {
            var first = CreateAttachmentHelper("report.pdf", isImage: false);
            var second = CreateAttachmentHelper("chart.png", isImage: true);
            var helper = new TestMailItemHelper();
            helper.SetAttachments(first, second);
            var filer = new ExposedEmailFiler();

            var attachments = await CollectAsync(filer.CallEnumerateAttachments(helper));

            attachments.Should().ContainInOrder(first, second);
        }

        [TestMethod]
        public void DeleteFile_WhenPathDoesNotExist_CompletesWithoutThrowing()
        {
            new ExposedEmailFiler()
                .Invoking(x => x.CallDeleteFile(@"C:\__TaskMaster_Impossible_Delete__"))
                .Should()
                .NotThrow();
        }

        [TestMethod]
        public void PushToUndoStack_WhenGlobalsContainMovedMailStack_PushesMoveRecord()
        {
            var root = CreateFolder(@"\\Mailbox - Root");
            var beforeFolder = CreateFolder(@"\\Mailbox - Root\Inbox");
            var afterFolder = CreateFolder(@"\\Mailbox - Root\Archive", "store-id");
            var beforeMove = CreateMailItem("before-id", beforeFolder.Object);
            var afterMove = CreateMailItem("after-id", afterFolder.Object);
            var movedMails = new ScoStack<IMovedMailInfo>();
            var filer = new ExposedEmailFiler
            {
                Globals = CreateGlobals(null, null, null, movedMails, root.Object),
            };

            filer.CallPushToUndoStack(beforeMove.Object, afterMove.Object);

            movedMails.Count.Should().Be(1);
            var captured = movedMails.Peek();
            captured.FolderPathOld.Should().Be("Inbox");
            captured.FolderPathNew.Should().Be("Archive");
            captured.EntryId.Should().Be("after-id");
        }

        [TestMethod]
        public void CaptureMoveDetails_WhenHelperContainsTabsAndCrLf_EnqueuesSanitizedTsv()
        {
            var writer = new TimedDiskWriter<string>(TimeSpan.FromMinutes(5), _ => { });
            var helper = CreateDetailedMailHelper("Subject\tLine\r\n", "Body\r\nText");
            var filer = new ExposedEmailFiler
            {
                Globals = CreateGlobals(null, null, null, null, null, writer),
            };

            filer.CallCaptureMoveDetails(helper);

            writer.Queue.TryTake(out var output).Should().BeTrue();
            output.Should().Contain("Subject Line");
            output.Should().Contain("Body Text");
            writer.StopTimer();
        }

        [TestMethod]
        public void ScoStack_WhenMovedMailInfoPushed_RecordsExpectedPathsOnPeek()
        {
            var info = new MovedMailInfo
            {
                FolderPathOld = "Inbox",
                FolderPathNew = "Archive",
                EntryId = "entry-abc-123",
                StoreId = "store-xyz-456",
            };
            var stack = new ScoStack<IMovedMailInfo>();

            stack.Push(info);

            stack.Count.Should().Be(1);
            var captured = stack.Peek();
            captured.FolderPathOld.Should().Be("Inbox");
            captured.FolderPathNew.Should().Be("Archive");
        }
    }
}
