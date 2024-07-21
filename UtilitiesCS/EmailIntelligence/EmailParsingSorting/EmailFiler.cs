using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookExtensions;
using System.IO;
using UtilitiesCS.EmailIntelligence.Bayesian;
using System.Text.RegularExpressions;

namespace UtilitiesCS.EmailIntelligence.EmailParsingSorting
{

    /// <summary>
    /// This class is responsible for sorting emails to specific folders. 
    /// It is a rewrite of the original SortEmail static class that was ported from VBA. 
    /// This version is written for C# and written as a non-static class to enable method testing
    /// </summary>
    public class EmailFiler
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Initializers
        
        public EmailFiler() { }

        public EmailFiler(EmailFilerConfig options)
        {
            Config = options;
        }

        #endregion Constructors and Initializers

        #region Private Fields

        //private YesNoToAllResponse _responseSaveFile = YesNoToAllResponse.Empty;
        //private YesNoToAllResponse _attachmentsOverwrite = YesNoToAllResponse.Empty;
        //private YesNoToAllResponse _attachmentsAltName = YesNoToAllResponse.Empty;
        //private YesNoToAllResponse _picturesOverwrite = YesNoToAllResponse.Empty;
        //private YesNoToAllResponse _removeReadOnly = YesNoToAllResponse.Empty;

        //private const int MAX_PATH = 256;

        #endregion Private Fields

        #region Public Properties

        private EmailFilerConfig _config;
        public EmailFilerConfig Config { get => _config; set => _config = value; }

        private IApplicationGlobals _globals;
        internal IApplicationGlobals Globals { get => _globals; set => _globals = value; }

        private IList<MailItemHelper> _mailHelpers;
        public IList<MailItemHelper> MailHelpers { get => _mailHelpers; set => _mailHelpers = value; }

        #endregion Public Properties

        #region Public Methods

        async public Task SortAsync(IList<MailItemHelper> mailHelpers)
        {
            TraceUtility.LogMethodCall(mailHelpers);
            mailHelpers.ThrowIfNullOrEmpty(nameof(mailHelpers));
            MailHelpers = mailHelpers;
            Config.ResolvePaths((Folder)MailHelpers.FirstOrDefault().FolderInfo.OlFolder);
            await SortAsync();
        }

        async public Task SortAsync()
        {
            TraceUtility.LogMethodCall();
            if (!TryValidateParameters()) { return; }

            // Process each email
            foreach (var mailHelper in MailHelpers)
            {
                await ProcessMailHelperAsync(mailHelper).ConfigureAwait(false);
            }

        }

        public async Task ProcessMailHelperAsync(MailItemHelper mailHelper)
        {
            // Save the message
            if (Config.SaveMsg) { await SaveMessageAsMsgAsync(mailHelper.Item, Config.SaveFsPath); }

            // Save the attachments and pictures
            await SaveAttachmentsPicturesAsync(mailHelper);

            await Task.Run(() => Globals.AF.Manager["Folder"].UnTrain(Config.OriginOlStem, mailHelper.Tokens, 1));
            // Move the email to the destination folder
            var mailItemTemp = await TryMoveMailItemHelperAsync(mailHelper);

            // If successful, mark it as sorted, push to undo stack, and capture training metrics and move details
            if (mailItemTemp is not null)
            {
                var trainingTasks = StartTrainingMetrics(mailHelper);
                await LabelAutoSortedAsync(mailItemTemp);
                PushToUndoStack(mailHelper.Item, mailItemTemp);
                await Task.WhenAll(trainingTasks).ConfigureAwait(false);
                await Task.Run(() => CaptureMoveDetails(mailHelper.Item, mailItemTemp)).ConfigureAwait(false);
            }
            
        }

        private void PushToUndoStack(MailItem beforeMove,MailItem afterMove)
        {
            var info = new MovedMailInfo(beforeMove, afterMove, Globals.Ol.Root.FolderPath);
            Globals.AF.MovedMails.Push(info);
        }

        private void CaptureMoveDetails(MailItem mailItem, MailItem oMailTmp)
        {
            TraceUtility.LogMethodCall(mailItem, oMailTmp, _globals);

            string[] strAry = oMailTmp.Details(Globals.Ol.ArchiveRootPath).Skip(1).ToArray();
            var output = SanitizeArrayLineTSV(ref strAry);

            Globals.Ol.EmailMoveWriter.Enqueue(output);
        }

        private string SanitizeArrayLineTSV(ref string[] strOutput)
        {
            var line = string.Join("\t", strOutput
                         //.Where(s => !string.IsNullOrEmpty(s))
                         .Select(s => s ?? "")
                         .Select(s => StripTabsCrLf(s))
                         .ToArray());
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

        public List<Task> StartTrainingMetrics(MailItemHelper mailHelper)
        {
            var tasks = new List<Task>()
            {
                Task.Run(() => Globals.AF.Manager["Folder"].AddOrUpdateClassifier(Config.DestinationOlStem, mailHelper.Tokens, 1)),
                Task.Run(() => Globals.AF.SubjectMap.Add(mailHelper.Subject, Config.DestinationOlStem))
            };
            //if(Config.DeleteAndUnTrain)
            //{
            //    tasks.Add(Task.Run(() => Globals.AF.Manager["Folder"].UnTrain(Config.OriginOlStem, mailHelper.Tokens, 1)));
            //}
            return tasks;
        }

        async public Task LabelAutoSortedAsync(MailItem mailItem)
        {
            await Task.Run(() =>
            {
                mailItem.SetUdf("AutoSorted", "Yes");
                mailItem.UnRead = false;
                mailItem.Save();
            });
        }

        async public Task SaveAttachmentsPicturesAsync(MailItemHelper mailHelper)
        {
            if (Config.SaveAttachments || Config.SavePictures)
            {

                var attachments = mailHelper.AttachmentsHelper.ToAsyncEnumerable();
                await attachments.ForEachAsync(async x =>
                {
                    await x.SaveAttachmentAsync(Config.SaveFsPath);
                });

                var toDelete = attachments.Where(x => !x.FilePathDelete.IsNullOrEmpty());
                await foreach (var attachment in toDelete) { await Task.Run(() => File.Delete(attachment.FilePathDelete)); }
            }
        }
        
        async public Task SaveMessageAsMsgAsync(
            MailItem mailItem,
            string fsLocation)
        {
            TraceUtility.LogMethodCall(mailItem, fsLocation);

            var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

            var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
            await Task.Run(() => mailItem.SaveAs(strPath, OlSaveAsType.olMSG));
        }

        public async Task<MailItem> TryMoveMailItemHelperAsync(MailItemHelper mailHelper)
        {
            try
            {
                var mailItemTemp = await Task.Run(() => (MailItem)mailHelper.Item.Move(Config.DestinationOlFolder));
                return mailItemTemp;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error moving email {mailHelper.Subject} to {Config.DestinationOlFolder.FolderPath}\n{e.Message}", e);
                return null;
            }
        }

        public bool TryValidateParameters()
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

        public void ValidateParameters()
        {
            Config.ThrowIfNull(nameof(Config));
            MailHelpers.ThrowIfNullOrEmpty(nameof(MailHelpers));
            Globals ??= Config.Globals;
            Globals.ThrowIfNull(nameof(Globals));
        }

        #endregion Public Methods
    }
}
