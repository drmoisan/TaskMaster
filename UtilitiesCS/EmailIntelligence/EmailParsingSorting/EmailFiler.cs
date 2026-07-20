#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.EmailIntelligence.EmailParsingSorting
{
    /// <summary>
    /// This class is responsible for sorting emails to specific folders.
    /// It is a rewrite of the original SortEmail static class that was ported from VBA.
    /// This version is written for C# and written as a non-static class to enable method testing
    /// </summary>
    public class EmailFiler
    {
        protected internal sealed class MoveMailResult
        {
            public MoveMailResult(MailItem original, MailItem? moved)
            {
                Original = original;
                Moved = moved;
            }

            public MailItem Original { get; }

            public MailItem? Moved { get; }
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Constructors and Initializers

        public EmailFiler() { }

        public EmailFiler(EmailFilerConfig options)
        {
            Config = options;
        }

        #endregion Constructors and Initializers

        #region Private Fields


        #endregion Private Fields

        #region Public Properties

        // Config/Globals/MailHelpers are required dependencies validated via ThrowIfNull /
        // ThrowIfNullOrEmpty in ValidateParameters() before real use; the backing field is
        // seeded with a justified `default!` (rather than widening the public property to `?`)
        // so the many existing unguarded dereferences throughout this class remain unchanged.
        private EmailFilerConfig _config = default!;
        public EmailFilerConfig Config
        {
            get => _config;
            set => _config = value;
        }

        private IApplicationGlobals _globals = default!;
        internal IApplicationGlobals Globals
        {
            get => _globals;
            set => _globals = value;
        }

        private IList<MailItemHelper> _mailHelpers = default!;
        public IList<MailItemHelper> MailHelpers
        {
            get => _mailHelpers;
            set => _mailHelpers = value;
        }

        #endregion Public Properties

        #region Public Methods

        async public Task OpenOlFolderAsync()
        {
            //TraceUtility.LogMethodCall();
            await Task.Run(TryOpenOlFolder);
        }

        internal void TryOpenOlFolder()
        {
            try
            {
                Config.ResolvePaths();
                Config.Globals!.Ol.App.ActiveExplorer().CurrentFolder = Config.DestinationOlFolder;
            }
            catch (System.Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show($"Error opening folder \n{ex.Message}");
            }
        }

        public async Task OpenFileSystemFolderAsync()
        {
            //TraceUtility.LogMethodCall();
            Config.ResolvePaths();
            await Task.Run(() => OpenFileSystemFolder(Config.SaveFsPath!));
        }

        internal void OpenFileSystemFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", folderPath);
            }
            else
            {
                logger.Error($"The folder path '{folderPath}' does not exist.");
            }
        }

        public async Task<bool> SortAsync(IList<MailItemHelper> mailHelpers)
        {
            //TraceUtility.LogMethodCall(mailHelpers);
            mailHelpers.ThrowIfNullOrEmpty(nameof(mailHelpers));
            MailHelpers = mailHelpers;
            ResolvePaths((Folder)MailHelpers.FirstOrDefault()!.FolderInfo!.OlFolder!);
            return await SortAsync();
        }

        public virtual async Task<bool> SortAsync()
        {
            //TraceUtility.LogMethodCall();
            if (!TryValidateParameters())
            {
                return false;
            }

            // Process each email
            foreach (var mailHelper in MailHelpers)
            {
                await ProcessMailHelperAsync(mailHelper).ConfigureAwait(false);
            }

            await SerializeFolderManagerAsync().ConfigureAwait(false);
            return true;
        }

        public virtual async Task ProcessMailHelperAsync(MailItemHelper mailHelper)
        {
            // Save the message
            if (Config.SaveMsg)
            {
                await SaveMessageAsMsgAsync(mailHelper.Item, Config.SaveFsPath!);
            }

            // Save the attachments and pictures
            await SaveAttachmentsPicturesAsync(mailHelper);

            await UnTrainFolderAsync(mailHelper).ConfigureAwait(false);
            // Move the email to the destination folder
            //var mailItemOriginal = mailHelper.Item;
            var moveResult = await TryMoveMailItemForProcessingAsync(mailHelper)
                .ConfigureAwait(false);
            var mailItemOriginal = moveResult.Original;
            var mailItemTemp = moveResult.Moved;

            // If successful, mark it as sorted, push to undo stack, and capture training metrics and move details
            if (mailItemTemp is not null)
            {
                var trainingTasks = StartTrainingMetrics(mailHelper);
                await LabelAutoSortedAsync(mailItemTemp);
                PushToUndoStack(mailItemOriginal, mailItemTemp);
                await Task.WhenAll(trainingTasks).ConfigureAwait(false);
                await Task.Run(() => CaptureMoveDetails(mailHelper)).ConfigureAwait(false);
            }
        }

        protected internal virtual void PushToUndoStack(MailItem beforeMove, MailItem afterMove)
        {
            var info = new MovedMailInfo(beforeMove, afterMove, Globals.Ol.Root.FolderPath);
            Globals.AF.MovedMails.Push(info);
        }

        protected internal virtual void CaptureMoveDetails(MailItemHelper helper)
        {
            //TraceUtility.LogMethodCall(mailItem, oMailTmp, _globals);

            string[] strAry = GetMoveDetails(helper);
            var output = SanitizeArrayLineTSV(ref strAry);

            EnqueueMoveOutput(output);
        }

        //private void CaptureMoveDetails(MailItem mailItem, MailItem oMailTmp)
        //{
        //    //TraceUtility.LogMethodCall(mailItem, oMailTmp, _globals);

        //    string[] strAry = oMailTmp.Details(Globals.Ol.ArchiveRootPath).Skip(1).ToArray();
        //    var output = SanitizeArrayLineTSV(ref strAry);

        //    Globals.Ol.EmailMoveWriter.Enqueue(output);
        //}

        private string SanitizeArrayLineTSV(ref string[] strOutput)
        {
            var line = string.Join(
                "\t",
                strOutput
                    //.Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s ?? "")
                    .Select(s => StripTabsCrLf(s))
                    .ToArray()
            );
            return line;
        }

        internal string StripTabsCrLf(string str)
        {
            var _regex = new Regex(@"[\t\n\r]+");
            string result = _regex.Replace(str, " ");

            // ensure max of one space per word
            _regex = new Regex(@"  +");
            result = _regex.Replace(result, " ");
            result = result.Trim();
            return result;
        }

        public virtual List<Task> StartTrainingMetrics(MailItemHelper mailHelper)
        {
            var tasks = new List<Task>()
            {
                TrainFolderAsync(mailHelper),
                TrainActionableAsync(mailHelper),
                Task.Run(() => RecordSubjectMap(mailHelper)),
                Task.Run(RecordRecentDestination),
            };

            return tasks;
        }

        public virtual async Task LabelAutoSortedAsync(MailItem mailItem)
        {
            await Task.Run(() =>
            {
                mailItem.SetUdf("AutoSorted", "Yes");
                mailItem.UnRead = false;
                mailItem.Save();
            });
        }

        public virtual async Task SaveAttachmentsPicturesAsync(MailItemHelper mailHelper)
        {
            if (Config.SaveAttachments || Config.SavePictures)
            {
                var attachments = EnumerateAttachments(mailHelper);
                if (!Config.SavePictures)
                {
                    attachments = attachments.Where(x => !x.AttachmentInfo.IsImage);
                }
                if (!Config.SaveAttachments)
                {
                    attachments = attachments.Where(x => x.AttachmentInfo.IsImage);
                }

                // ForEachAsync is obsolete (CS0618) per the framework's migration guidance
                // ("Use the language support for async foreach instead"), but replacing it
                // with `await foreach` here is a control-flow change to a production async
                // method, not an annotation-only edit. Suppressing narrowly preserves the
                // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
                await attachments.ForEachAsync(async x =>
                {
                    await SaveAttachmentAsync(x).ConfigureAwait(false);
                });
#pragma warning restore CS0618

                var toDelete = attachments.Where(x => !x.FilePathDelete.IsNullOrEmpty());
                await foreach (var attachment in toDelete)
                {
                    await Task.Run(() => DeleteFile(attachment.FilePathDelete!))
                        .ConfigureAwait(false);
                }
            }
        }

        public virtual async Task SaveMessageAsMsgAsync(MailItem mailItem, string fsLocation)
        {
            //TraceUtility.LogMethodCall(mailItem, fsLocation);

            var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

            var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
            await Task.Run(() => mailItem.SaveAs(strPath, OlSaveAsType.olMSG));
        }

        //public async Task<MailItem> TryMoveMailItemHelperAsync(MailItemHelper mailHelper)
        //{
        //    return await Task.Run(() =>
        //    {
        //        try
        //        {
        //            return (MailItem)mailHelper.Item.Move(Config.DestinationOlFolder);
        //        }
        //        catch (System.Exception e)
        //        {
        //            logger.Error($"Error moving email {mailHelper.Subject} to {Config.DestinationOlFolder.FolderPath}\n{e.Message}", e);
        //            return null;
        //        }
        //    });
        //}

        public virtual async Task<(MailItem Original, MailItem? Moved)> TryMoveMailItemHelperAsync(
            MailItemHelper mailHelper
        )
        {
            return await Task.Run(() =>
            {
                lock (mailHelper.Item)
                {
                    var original = mailHelper.Item;
                    try
                    {
                        var moved = (MailItem)mailHelper.Item.Move(Config.DestinationOlFolder);
                        return (original, moved);
                    }
                    catch (System.Exception e)
                    {
                        logger.Error(
                            $"Error moving email {mailHelper.Subject} to {Config.DestinationOlFolder!.FolderPath}\n{e.Message}",
                            e
                        );
                        return (original, (MailItem?)null);
                    }
                }
            });
        }

        protected internal virtual async Task<MoveMailResult> TryMoveMailItemForProcessingAsync(
            MailItemHelper mailHelper
        )
        {
            var (original, moved) = await TryMoveMailItemHelperAsync(mailHelper)
                .ConfigureAwait(false);
            return new MoveMailResult(original, moved);
        }

        public virtual bool TryValidateParameters()
        {
            try
            {
                ValidateParameters();
                return Config.CanSort;
            }
            catch (System.Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }

        public virtual void ValidateParameters()
        {
            Config.ThrowIfNull(nameof(Config));
            MailHelpers.ThrowIfNullOrEmpty(nameof(MailHelpers));
            Globals ??= Config.Globals!;
            Globals.ThrowIfNull(nameof(Globals));
        }

        protected internal virtual void ResolvePaths(Folder currentFolder) =>
            Config.ResolvePaths(currentFolder);

        // Resolves the active Folder predictor through the Folder-only IFolderPredictor seam on
        // OlFolderClassifierGroup. With UseLcppnPredictor off (default) this returns the unchanged
        // flat Manager["Folder"] BayesianClassifierGroup, preserving prior behavior.
        protected internal virtual Task<IFolderPredictor> GetFolderPredictorAsync() =>
            new OlFolderClassifierGroup(Globals).GetFolderPredictorAsync();

        protected internal virtual async Task SerializeFolderManagerAsync()
        {
            (await GetFolderPredictorAsync()).Serialize();
            (await Globals.AF.Manager["Actionable"]).Serialize();
        }

        protected internal virtual async Task UnTrainFolderAsync(MailItemHelper mailHelper)
        {
            (await GetFolderPredictorAsync()).UnTrain(Config.OriginOlStem, mailHelper.Tokens, 1);
        }

        protected internal virtual Task TrainFolderAsync(MailItemHelper mailHelper)
        {
            return Task.Run(async () =>
                (await GetFolderPredictorAsync()).Train(
                    Config.DestinationOlStem,
                    mailHelper.Tokens,
                    1
                )
            );
        }

        protected internal virtual Task TrainActionableAsync(MailItemHelper mailHelper)
        {
            // Only train on confirmed actionable signals; skip "None" to avoid diluting the classifier
            // with the majority class and producing a model that always predicts "None".
            if (mailHelper.Actionable == "None")
            {
                return Task.CompletedTask;
            }

            return Task.Run(async () =>
                (await Globals.AF.Manager["Actionable"]).Train(
                    mailHelper.Actionable,
                    mailHelper.Tokens,
                    1
                )
            );
        }

        protected internal virtual void RecordSubjectMap(MailItemHelper mailHelper)
        {
            Globals.AF.SubjectMap.Add(mailHelper.Subject, Config.DestinationOlStem);
        }

        protected internal virtual void RecordRecentDestination()
        {
            Globals.AF.RecentsList.AddOrMoveFirst(Config.DestinationOlStem, 5);
        }

        protected internal virtual IAsyncEnumerable<AttachmentHelper> EnumerateAttachments(
            MailItemHelper mailHelper
        )
        {
            return mailHelper.AttachmentsHelper.ToAsyncEnumerable();
        }

        protected internal virtual Task SaveAttachmentAsync(AttachmentHelper attachment)
        {
            return attachment.SaveAttachmentAsync(Config.SaveFsPath!);
        }

        protected internal virtual void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        protected internal virtual string[] GetMoveDetails(MailItemHelper helper)
        {
            return helper.Details().Skip(1).ToArray();
        }

        protected internal virtual void EnqueueMoveOutput(string output)
        {
            Globals.Ol.EmailMoveWriter.Enqueue(output);
        }

        #endregion Public Methods
    }
}
