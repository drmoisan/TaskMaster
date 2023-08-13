using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using static Microsoft.TeamFoundation.Common.Internal.NativeMethods;

namespace ToDoModel
{
    public class AttachmentInfo
    {
        public AttachmentInfo() { }

        public AttachmentInfo(Attachment attachment, DateTime sentOn, string saveFolderPath)
        {
            Task.Run(()=>Init(attachment, sentOn, saveFolderPath, null)).ConfigureAwait(false);
        }

        public AttachmentInfo(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        {
            Task.Run(() => Init(attachment, sentOn, saveFolderPath, deleteFolderPath)).ConfigureAwait(false);
        }

        async public static Task<AttachmentInfo> LoadAsync(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        {
            var _att = new AttachmentInfo();
            await _att.Init(attachment,sentOn, saveFolderPath, deleteFolderPath);
            return _att;
        }
        
        async public Task Init(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
        {
            _attachment = attachment;

            if (CheckParameters(attachment, sentOn, saveFolderPath, deleteFolderPath))
            {                
                (var filenameSeed, _fileExtension) = GetAttachmentFilename(Attachment);
                filenameSeed = FolderConverter.SanitizeFilename(filenameSeed);
                (_filenameSeed, _fileExtension) = PrependDatePrefix(filenameSeed, sentOn);
                _filePathSave = AdjustForMaxPath(saveFolderPath, filenameSeed, _fileExtension);
                _filePathSaveAlt = AdjustForMaxPath(saveFolderPath, filenameSeed, _fileExtension, GetNameSuffix());
                
                if (deleteFolderPath is not null)
                {
                    _folderPathDelete = deleteFolderPath;
                    _filePathDelete = AdjustForMaxPath(deleteFolderPath, filenameSeed, _fileExtension);
                }                
            }
            await Task.CompletedTask;

        }

        #region Public Properties

        private Attachment _attachment;
        public Attachment Attachment { get => _attachment; set => _attachment = value; }

        private bool _datePrefix;
        public bool DatePrefix { get => _datePrefix; set => _datePrefix = value; }

        private List<string> _errorMessages;
        public List<string> ErrorMessages { get => _errorMessages; }

        private string _filenameSeed;
        public string FilenameSeed { get => _filenameSeed; set => _filenameSeed = value; }

        private string _fileExtension;
        public string FileExtension { get => _fileExtension; set => _fileExtension = value; }

        private string _filePathSave;
        public string FilePathSave { get => _filePathSave; set => _filePathSave = value; }

        private string _filePathSaveAlt;
        public string FilePathSaveAlt { get => _filePathSaveAlt; set => _filePathSaveAlt = value; }

        private string _filePathDelete;
        public string FilePathDelete { get => _filePathDelete; set => _filePathDelete = value; }

        private string _folderPathSave;
        public string FolderPathSave { get => _folderPathSave; set => _folderPathSave = value; }

        private string _folderPathDelete;
        public string FolderPathDelete { get => _folderPathDelete; set => _folderPathDelete = value; }

        private List<string> _imageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        public bool IsImage { get => _imageExtensions.Contains(FileExtension ?? ""); }

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
        
        public bool CheckParameters(Attachment attachment, DateTime sentOn, string saveFolderPath)
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

        public bool CheckParameters(Attachment attachment, DateTime sentOn, string saveFolderPath, string deleteFolderPath)
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
        
        public (string Filename, string Extension) GetAttachmentFilename(Attachment attachment)
        {
            var filename = Path.GetFileNameWithoutExtension(attachment.FileName);

            var extension = Path.GetExtension(attachment.FileName);
            return (filename, extension);
        }

        public string GetNameSuffix() 
        { 
            return $"_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        }
        
        public (string, string) PrependDatePrefix(string seed, DateTime date)
        {
            var prefix = date.ToString("yyyyMMdd");
            return (prefix, $"{prefix}_{seed}");
        }


        #endregion
    }
}
