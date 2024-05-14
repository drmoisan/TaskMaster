using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsing;
//using static Microsoft.TeamFoundation.Common.Internal.NativeMethods;

namespace UtilitiesCS.EmailIntelligence
{
    public class AttachmentHelper 
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #region Constructors and Initializers

        public AttachmentHelper() { }

        public AttachmentHelper(Attachment attachment)
        {
            _attachment = attachment;
            _attachmentInfo = new AttachmentSerializable(attachment);
        }

        public AttachmentHelper(Attachment attachment, DateTime sentOn, string saveFolderPath)
        {
            Init(attachment, sentOn, saveFolderPath, null);
        }

        public AttachmentHelper(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        {
            Init(attachment, sentOn, saveFolderPath, deleteFolderPath);
        }

        async public static Task<AttachmentHelper> CreateAsync(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        {
            var att = new AttachmentHelper();
            await att.InitAsync(attachment, sentOn, saveFolderPath, deleteFolderPath);
            return att;
        }
                
        public void Init(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        {
            _attachment = attachment;
            _attachmentInfo = new AttachmentSerializable(attachment);

            if (CheckParameters(attachment, sentOn, saveFolderPath, deleteFolderPath))
            {
                (AttachmentInfo.FilenameSeed, AttachmentInfo.FileExtension) = GetAttachmentFilename(Attachment);
                AttachmentInfo.FilenameSeed = FolderConverter.SanitizeFilename(AttachmentInfo.FilenameSeed);
                AttachmentInfo.FilenameSeed = PrependDatePrefix(AttachmentInfo.FilenameSeed, sentOn);
                FilePathSave = AdjustForMaxPath(saveFolderPath, AttachmentInfo.FilenameSeed, AttachmentInfo.FileExtension);
                FilePathSaveAlt = AdjustForMaxPath(saveFolderPath, AttachmentInfo.FilenameSeed, AttachmentInfo.FileExtension, GetNameSuffix());

                if (deleteFolderPath is not null)
                {
                    _folderPathDelete = deleteFolderPath;
                    _filePathDelete = AdjustForMaxPath(deleteFolderPath, AttachmentInfo.FilenameSeed, AttachmentInfo.FileExtension);
                }
                AttachmentInfo.Size = attachment.Size;
                AttachmentInfo.Type = attachment.Type;
            }
        }

        async protected internal Task InitAsync(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        {
            _attachment = attachment;
            _attachmentInfo = new AttachmentSerializable(attachment);

            if (CheckParameters(attachment, sentOn, saveFolderPath, deleteFolderPath))
            {
                (AttachmentInfo.FilenameSeed, AttachmentInfo.FileExtension) = GetAttachmentFilename(Attachment);
                AttachmentInfo.FilenameSeed = FolderConverter.SanitizeFilename(AttachmentInfo.FilenameSeed);
                AttachmentInfo.FilenameSeed = PrependDatePrefix(AttachmentInfo.FilenameSeed, sentOn);
                FilePathSave = AdjustForMaxPath(saveFolderPath, AttachmentInfo.FilenameSeed, AttachmentInfo.FileExtension);
                FilePathSaveAlt = AdjustForMaxPath(saveFolderPath, AttachmentInfo.FilenameSeed, AttachmentInfo.FileExtension, GetNameSuffix());

                if (deleteFolderPath is not null)
                {
                    _folderPathDelete = deleteFolderPath;
                    _filePathDelete = AdjustForMaxPath(deleteFolderPath, AttachmentInfo.FilenameSeed, AttachmentInfo.FileExtension);
                }
            }
            await Task.CompletedTask;

        }

        #endregion

        #region Public Properties

        public const int MAX_PATH = 256;
        public const string PR_ATTACH_DATA_BIN = "http://schemas.microsoft.com/mapi/proptag/0x37010102";

        private AttachmentSerializable _attachmentInfo;
        public IAttachment AttachmentInfo { get => _attachmentInfo; set => _attachmentInfo = value as AttachmentSerializable; }

        private Attachment _attachment;
        public Attachment Attachment { get => _attachment; set => _attachment = value; }

        private bool _datePrefix;
        public bool DatePrefix { get => _datePrefix; set => _datePrefix = value; }

        private List<string> _errorMessages;
        public List<string> ErrorMessages { get => _errorMessages; }

        private FilePathHelper _filePathHelperSave = new FilePathHelper();
        internal FilePathHelper FilePathHelperSave => _filePathHelperSave;

        private FilePathHelper _filePathHelperSaveAlt = new FilePathHelper();
        internal FilePathHelper FilePathHelperSaveAlt => _filePathHelperSaveAlt;

        //private string _filePathSave;
        public string FilePathSave { get => FilePathHelperSave.FilePath; set => FilePathHelperSave.FilePath = value; }

        //private string _filePathSaveAlt;
        public string FilePathSaveAlt { get => FilePathHelperSaveAlt.FilePath; set => FilePathHelperSaveAlt.FilePath = value; }

        private string _filePathDelete;
        public string FilePathDelete { get => _filePathDelete; set => _filePathDelete = value; }

        //private string _folderPathSave;
        public string FolderPathSave { get => FilePathHelperSave.FolderPath; set => FilePathHelperSave.FolderPath = value; }

        private string _folderPathDelete;
        public string FolderPathDelete { get => _folderPathDelete; set => _folderPathDelete = value; }

        #endregion

        #region Helper Methods

        public static string AdjustForMaxPath(string folderPath, string filenameSeed, string fileExtension, string filenameSuffix = "")
        {
            var filename = $"{filenameSeed}{filenameSuffix}{fileExtension}";
            var filepath = Path.Combine(folderPath, filename);
            if (filepath.Length >= MAX_PATH)
            {
                var maxSeedLength = filenameSeed.Length + MAX_PATH - filepath.Length;
                filenameSeed = filenameSeed.Substring(0, maxSeedLength);
                filename = $"{filenameSeed.Substring(0, maxSeedLength)}{filenameSuffix}{fileExtension}";
                filepath = Path.Combine(folderPath, filename);
            }
            return filepath;
        }

        internal bool CheckParameters(Attachment attachment, DateTime sentOn, string saveFolderPath)
        {
            _errorMessages = new List<string>();

            if (attachment is null) { _errorMessages.Add("The attachment is null"); }

            if (saveFolderPath.Length >= MAX_PATH - 10)
            {
                _errorMessages.Add($"The path {saveFolderPath} is too long to save. It must be smaller than {MAX_PATH - 10} characters");
            }

            if (_errorMessages.Count > 0) { return false; }
            else { return true; }
        }

        internal bool CheckParameters(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        {
            _errorMessages = new List<string>();

            if (attachment is null) { _errorMessages.Add("The attachment is null"); }

            if (saveFolderPath.Length >= MAX_PATH - 10)
            {
                _errorMessages.Add($"The path {saveFolderPath} is too long to save. It must be smaller than {MAX_PATH - 10} characters");
            }

            if (deleteFolderPath is not null && deleteFolderPath.Length >= MAX_PATH)
            {
                _errorMessages.Add($"The path {deleteFolderPath} is too long to save. It must be smaller than {MAX_PATH} characters");
            }

            if (_errorMessages.Count > 0) { return false; }
            else { return true; }
        }

        internal virtual (string Filename, string Extension) GetAttachmentFilename(Attachment attachment)
        {
            string filename = "";
            string extension = "";
            try
            {
                filename = Path.GetFileNameWithoutExtension(attachment.FileName);
                extension = Path.GetExtension(attachment.FileName);
                if (filename.Length == 0)
                {
                    filename = extension;
                    extension = "";
                }
            }
            catch (System.Exception e)
            {
                logger.Error($"Error getting filename for item. Attachment type {attachment.Type}\n{e.Message}",e);
            }
            
            
            return (filename, extension);
        }

        public string GetNameSuffix()
        {
            return $"_{DateTime.Now:yyyyMMddHHmmss}";
        }

        public string PrependDatePrefix(string seed, DateTime date)
        {
            var prefix = date.ToString("yyyyMMdd");
            return $"{prefix}_{seed}";
        }

        #endregion
    }
}
