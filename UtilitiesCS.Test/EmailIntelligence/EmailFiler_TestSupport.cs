using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace UtilitiesCS.Test.EmailIntelligence
{
    public partial class EmailFiler_Tests
    {
        private static AttachmentHelper CreateAttachmentHelper(
            string fileName,
            bool isImage,
            string deletePath = null
        )
        {
            return new AttachmentHelper
            {
                AttachmentInfo =
                    new UtilitiesCS.EmailIntelligence.EmailParsing.AttachmentSerializable
                    {
                        FileName = fileName,
                        IsImage = isImage,
                    },
                FilePathDelete = deletePath,
            };
        }

        private static Mock<Folder> CreateFolder(string folderPath, string storeId = "store-id")
        {
            var folder = new Mock<Folder>();
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.StoreID).Returns(storeId);
            return folder;
        }

        private static Mock<MailItem> CreateMailItem(string entryId, Folder parent)
        {
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.Parent).Returns(parent);
            mailItem.SetupGet(x => x.EntryID).Returns(entryId);
            return mailItem;
        }

        private static MailItemHelper CreateDetailedMailHelper(string subject, string body)
        {
            var helper = new TestMailItemHelper
            {
                Triage = "A",
                SentOn = "2026-04-03T9:30:00+00:00",
                Sender = new RecipientInfo("Ada Lovelace", "ada@example.com", "<a>Ada</a>"),
                Subject = subject,
                Body = body,
                ConversationID = "conversation-id",
                EntryId = "entry-id",
                Item = CreateTaskMailItem().Object,
            };
            var folderInfo = new Mock<IFolderWrapper>();
            folderInfo.SetupGet(x => x.RelativePath).Returns(@"Inbox\Projects");
            helper.FolderInfo = folderInfo.Object;
            helper.SetRecipients(
                new[] { new RecipientInfo("Grace Hopper", "grace@example.com", null) },
                new[] { new RecipientInfo("Alan Turing", "alan@example.com", null) }
            );
            helper.SetAttachmentsInfo(
                new[] { Mock.Of<IAttachment>(x => x.FileName == "report.pdf") }
            );
            return helper;
        }

        private static Mock<MailItem> CreateTaskMailItem()
        {
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.IsMarkedAsTask).Returns(true);
            return mailItem;
        }

        private static ManagerAsyncLazy CreateManager(
            BayesianClassifierGroup folderGroup,
            BayesianClassifierGroup actionableGroup
        )
        {
            var globals = new Mock<IApplicationGlobals>();
            var manager = new ManagerAsyncLazy(globals.Object);
            manager["Folder"] = new AsyncLazy<BayesianClassifierGroup>(() =>
                Task.FromResult(folderGroup)
            );
            manager["Actionable"] = new AsyncLazy<BayesianClassifierGroup>(() =>
                Task.FromResult(actionableGroup)
            );
            return manager;
        }

        private static IApplicationGlobals CreateGlobals(
            ManagerAsyncLazy manager,
            SubjectMapSco subjectMap,
            SloLinkedList<string> recents,
            ScoStack<IMovedMailInfo> movedMails,
            Folder rootFolder,
            TimedDiskWriter<string> writer = null
        )
        {
            var autoFile = new Mock<IAppAutoFileObjects>();
            autoFile.SetupGet(x => x.Manager).Returns(manager);
            autoFile.SetupGet(x => x.SubjectMap).Returns(subjectMap);
            autoFile.SetupGet(x => x.RecentsList).Returns(recents);
            autoFile.SetupGet(x => x.MovedMails).Returns(movedMails);

            var ol = new Mock<IOlObjects>();
            ol.SetupGet(x => x.Root).Returns(rootFolder);
            ol.SetupGet(x => x.EmailMoveWriter).Returns(writer);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.AF).Returns(autoFile.Object);
            globals.SetupGet(x => x.Ol).Returns(ol.Object);
            return globals.Object;
        }

        private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> items)
        {
            var results = new List<T>();
            await foreach (var item in items)
            {
                results.Add(item);
            }

            return results;
        }

        private sealed class TrackingEmailFiler : ExposedEmailFiler
        {
            public TrackingEmailFiler(EmailFilerConfig config = null)
                : base(config) { }

            public readonly List<AttachmentHelper> AttachmentsToEnumerate = new();
            public readonly List<AttachmentHelper> SavedAttachments = new();
            public readonly List<string> DeletedFiles = new();
            public readonly List<MailItemHelper> ProcessedHelpers = new();
            public bool ParameterlessSortResult { get; set; }
            public int ParameterlessSortCalls { get; private set; }
            public bool TryValidateParametersResult { get; set; } = true;
            public bool UseBaseSortAsync { get; set; }
            public bool UseBaseProcessMailHelperAsync { get; set; }
            public Folder ResolvedFolder { get; private set; }
            public int SaveMessageCalls { get; private set; }
            public int SaveAttachmentsCalls { get; private set; }
            public int UnTrainCalls { get; private set; }
            public int StartTrainingMetricsCalls { get; private set; }
            public int LabelCalls { get; private set; }
            public int PushUndoCalls { get; private set; }
            public int CaptureMoveDetailsCalls { get; private set; }
            public int SerializeFolderManagerCalls { get; private set; }
            public int TrainFolderCalls { get; private set; }
            public int TrainActionableCalls { get; private set; }
            public int RecordSubjectMapCalls { get; private set; }
            public int RecordRecentDestinationCalls { get; private set; }
            public (MailItem Original, MailItem Moved) MoveResult { get; set; }

            public override async Task<bool> SortAsync()
            {
                if (UseBaseSortAsync)
                {
                    return await base.SortAsync().ConfigureAwait(false);
                }

                ParameterlessSortCalls++;
                return await Task.FromResult(ParameterlessSortResult);
            }

            public override bool TryValidateParameters() => TryValidateParametersResult;

            protected internal override void ResolvePaths(Folder currentFolder) =>
                ResolvedFolder = currentFolder;

            public override Task ProcessMailHelperAsync(MailItemHelper mailHelper)
            {
                if (UseBaseProcessMailHelperAsync)
                {
                    return base.ProcessMailHelperAsync(mailHelper);
                }

                ProcessedHelpers.Add(mailHelper);
                return Task.CompletedTask;
            }

            protected internal override async Task SerializeFolderManagerAsync()
            {
                SerializeFolderManagerCalls++;
                await Task.CompletedTask;
            }

            public override async Task SaveMessageAsMsgAsync(MailItem mailItem, string fsLocation)
            {
                SaveMessageCalls++;
                await Task.CompletedTask;
            }

            public override async Task SaveAttachmentsPicturesAsync(MailItemHelper mailHelper)
            {
                SaveAttachmentsCalls++;
                await base.SaveAttachmentsPicturesAsync(mailHelper);
            }

            protected internal override async Task UnTrainFolderAsync(MailItemHelper mailHelper)
            {
                UnTrainCalls++;
                await Task.CompletedTask;
            }

            protected internal override async Task<MoveMailResult> TryMoveMailItemForProcessingAsync(
                MailItemHelper mailHelper
            )
            {
                await Task.CompletedTask;
                return new MoveMailResult(MoveResult.Original, MoveResult.Moved);
            }

            public override List<Task> StartTrainingMetrics(MailItemHelper mailHelper)
            {
                StartTrainingMetricsCalls++;
                return base.StartTrainingMetrics(mailHelper);
            }

            public override async Task LabelAutoSortedAsync(MailItem mailItem)
            {
                LabelCalls++;
                await Task.CompletedTask;
            }

            protected internal override void PushToUndoStack(
                MailItem beforeMove,
                MailItem afterMove
            ) => PushUndoCalls++;

            protected internal override void CaptureMoveDetails(MailItemHelper helper) =>
                CaptureMoveDetailsCalls++;

            protected internal override Task TrainFolderAsync(MailItemHelper mailHelper)
            {
                TrainFolderCalls++;
                return Task.CompletedTask;
            }

            protected internal override Task TrainActionableAsync(MailItemHelper mailHelper)
            {
                TrainActionableCalls++;
                return Task.CompletedTask;
            }

            protected internal override void RecordSubjectMap(MailItemHelper mailHelper) =>
                RecordSubjectMapCalls++;

            protected internal override void RecordRecentDestination() =>
                RecordRecentDestinationCalls++;

            protected internal override IAsyncEnumerable<AttachmentHelper> EnumerateAttachments(
                MailItemHelper mailHelper
            ) => AttachmentsToEnumerate.ToAsyncEnumerable();

            protected internal override Task SaveAttachmentAsync(AttachmentHelper attachment)
            {
                SavedAttachments.Add(attachment);
                return Task.CompletedTask;
            }

            protected internal override void DeleteFile(string filePath) =>
                DeletedFiles.Add(filePath);
        }

        private class ExposedEmailFiler : EmailFiler
        {
            public ExposedEmailFiler(EmailFilerConfig config = null)
                : base(config ?? new EmailFilerConfig()) { }

            public Task CallSerializeFolderManagerAsync() => base.SerializeFolderManagerAsync();

            public Task CallUnTrainFolderAsync(MailItemHelper helper) =>
                base.UnTrainFolderAsync(helper);

            public Task CallTrainFolderAsync(MailItemHelper helper) =>
                base.TrainFolderAsync(helper);

            public Task CallTrainActionableAsync(MailItemHelper helper) =>
                base.TrainActionableAsync(helper);

            public Task<MoveMailResult> CallTryMoveMailItemForProcessingAsync(
                MailItemHelper helper
            ) => base.TryMoveMailItemForProcessingAsync(helper);

            public void CallRecordSubjectMap(MailItemHelper helper) =>
                base.RecordSubjectMap(helper);

            public void CallRecordRecentDestination() => base.RecordRecentDestination();

            public IAsyncEnumerable<AttachmentHelper> CallEnumerateAttachments(
                MailItemHelper helper
            ) => base.EnumerateAttachments(helper);

            public void CallDeleteFile(string filePath) => base.DeleteFile(filePath);

            public void CallPushToUndoStack(MailItem beforeMove, MailItem afterMove) =>
                base.PushToUndoStack(beforeMove, afterMove);

            public void CallCaptureMoveDetails(MailItemHelper helper) =>
                base.CaptureMoveDetails(helper);
        }

        private sealed class TestMailItemHelper : MailItemHelper
        {
            public void SetRecipients(IRecipientInfo[] toRecipients, IRecipientInfo[] ccRecipients)
            {
                ToRecipients = toRecipients;
                CcRecipients = ccRecipients;
            }

            public void SetAttachments(params AttachmentHelper[] attachments)
            {
                AttachmentsHelper = attachments;
            }

            public void SetAttachmentsInfo(IAttachment[] attachments)
            {
                AttachmentsInfo = attachments;
            }

            public void SetTokens(params string[] tokens)
            {
                Tokens = tokens;
            }
        }
    }
}
