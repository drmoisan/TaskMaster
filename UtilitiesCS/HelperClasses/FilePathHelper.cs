using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class FilePathHelper: INotifyPropertyChanged
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FilePathHelper() { PropertyChanged += FilePathHelper_PropertyChanged; }

        public FilePathHelper(string fileName, string folderPath)
        {
            FileName = fileName;
            FolderPath = folderPath;
            FilePath = Path.Combine(_folderPath, _fileName);
            PropertyChanged += FilePathHelper_PropertyChanged;
        }

        private FilePathHelper(string fileNameSeed, string fileExtension, string fileNameSuffix, string folderPath)
        {
            FileStemSeed = fileNameSeed;
            FileStemSuffix = fileNameSuffix;
            FileExtension = fileExtension;
            FolderPath = folderPath;
            FilePath = Path.Combine(_folderPath, _fileName);
            PropertyChanged += FilePathHelper_PropertyChanged;
        }

        public static FilePathHelper FromSeed(string fileNameSeed, string fileExtension, string fileNameSuffix, string folderPath)
        {
            var fph = new FilePathHelper(fileNameSeed, fileExtension, fileNameSuffix, folderPath);
            
            return fph;
        }

        private string _filePath = "";
        public string FilePath { get => _filePath; set { _filePath = value; NotifyPropertyChanged(); } }
        
        private string _folderPath = "";
        public string FolderPath { get=> _folderPath; set { _folderPath = value; NotifyPropertyChanged(); } }

        private string _fileName = "";
        public string FileName { get => _fileName; set { _fileName = value; NotifyPropertyChanged(); }}

        private string _fileStemSeed = null;
        public string FileStemSeed { get => _fileStemSeed; set { _fileStemSeed = value; NotifyPropertyChanged(); } }

        private string _fileStemSuffix = null;
        public string FileStemSuffix { get => _fileStemSuffix; set { _fileStemSuffix = value; NotifyPropertyChanged(); } }

        private string _fileStem = null;
        public string FileStem { get => _fileStem; protected set => _fileStem = value; }

        private string _fileExtension = null;
        public string FileExtension { get => _fileExtension; set { _fileExtension = value; NotifyPropertyChanged(); } }

        public const int MAX_PATH = 256;

        internal bool StemInitialized()
        {
            if (FileStemSeed is null || FileStemSuffix is null || FileExtension is null)
            {
                if (FolderPath.IsNullOrEmpty() || !TryParseFileName(FileName))
                    return false;
            }
            return !FolderPath.IsNullOrEmpty();
        }

        public int CalcMaxSeedLength()
        {
            if (!StemInitialized())
                return MAX_PATH;
            
            return MAX_PATH - FolderPath.Length - FileExtension.Length - FileStemSuffix.Length;
        }

        public (string Stem, string Extension) ExtractStemAndExtension(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileStem = Path.GetFileNameWithoutExtension(fileName);
            
            if (fileStem.IsNullOrEmpty())
            {
                fileStem = fileExtension ?? "";
                fileExtension = "";
            }

            return (fileStem, fileExtension ?? "");
        }
        
        public bool TryParseFileStem(string fileStem, out string fileStemSeed, out string fileStemSuffix)
        {
            fileStemSeed = FileStemSeed ?? "";
            fileStemSuffix = FileStemSuffix ?? "";
            string remainingChars = fileStem;

            // case 1: empty fileStem
            if (fileStem.IsNullOrEmpty())
                return false;

            // case 2: fileStemSeed is empty AND fileStemSuffix is empty
            if (fileStemSeed.IsNullOrEmpty() && fileStemSuffix.IsNullOrEmpty())
            {
                fileStemSeed = fileStem;
                remainingChars = null;
            }

            // case 3: Some part of seed or suffix remains
            // step 1 strip existing seed if it exists
            if (!fileStemSeed.IsNullOrEmpty() && (remainingChars?.StartsWith(fileStemSeed) ?? false))
                remainingChars = remainingChars.Replace(fileStemSeed, "");

            // step 2 strip existing suffix if it exists and append any remaining chars to seed
            if (!fileStemSuffix.IsNullOrEmpty() && (remainingChars?.EndsWith(fileStemSuffix) ?? false))
            {
                remainingChars = remainingChars.Replace(fileStemSuffix, "");
                fileStemSeed += remainingChars;
            }

            // step 3 append any remaining
            if (fileStemSeed.IsNullOrEmpty())
            {
                fileStemSeed = remainingChars;
                fileStemSuffix = "";
            }
            else if (fileStemSuffix.IsNullOrEmpty())
            {
                fileStemSuffix = remainingChars;
            }
            else 
            {
                fileStemSeed += remainingChars;
            }

            return true;
        }

        public bool TryParseFileName(string fileName)
        {
            if (fileName.IsNullOrEmpty())
                return false;
            
            var (fileStem, fileExtension) = ExtractStemAndExtension(fileName);
                        
            if (TryParseFileStem(fileStem, out string fileStemSeed, out string fileStemSuffix))
            {
                _fileStemSeed = fileStemSeed;
                _fileStemSuffix = fileStemSuffix;
                _fileStem = fileStem;
                _fileExtension = fileExtension;
                return true;
            }
            else
            {
                return false;
            }
        }
                
        public bool AdjustForMaxPath()
        {
            if (!StemInitialized())
                return false;

            var maxSeedLength = MAX_PATH - FolderPath.Length - FileExtension.Length - FileStemSuffix.Length;
            
            var fileName = $"{FileStemSeed}{FileStemSuffix}{FileExtension}";
            var filePath = Path.Combine(FolderPath, fileName);
            if (filePath.Length > MAX_PATH)
            {
                maxSeedLength = FileStemSeed.Length + MAX_PATH - filePath.Length;
                _fileStemSeed = FileStemSeed.Substring(0, maxSeedLength);
            }
            return true;
        }

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

        #region INotifyPropertyChanged Implementation

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void FilePathHelper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FileName":
                    if (_folderPath != "")
                        _filePath = Path.Combine(_folderPath, _fileName);
                    break;
                case "FolderPath":
                    if (!_fileName.IsNullOrEmpty())
                        _filePath = Path.Combine(_folderPath, _fileName);
                    break;
                case "FilePath":
                    try
                    {
                        _folderPath = Path.GetDirectoryName(_filePath);    
                    }
                    catch (System.Exception)
                    {
                        var st = string.Join("\n",TraceUtility.GetMyStackSummary(new StackTrace()));
                        string msg = $"FilePath: {_filePath} is invalid.\n{st}";
                        logger.Error(msg);
                        throw;
                    }
                    
                    _fileName = Path.GetFileName(_filePath);
                    if (_fileName == "") 
                        throw new ArgumentException($"FilePath {_filePath} must include a FileName");
                    break;
                case "FileStemSeed":
                    if (AdjustForMaxPath())
                    {
                        _fileStem = $"{_fileStemSeed}{_fileStemSuffix}";
                        FileName = $"{_fileStem}{_fileExtension}";
                    }
                    break;
                case "FileStemSuffix":
                    if (AdjustForMaxPath())
                    {
                        _fileStem = $"{_fileStemSeed}{_fileStemSuffix}";
                        FileName = $"{_fileStem}{_fileExtension}";
                    }
                    break;
                case "FileExtension":
                    if (AdjustForMaxPath())
                    {
                        _fileStem = $"{_fileStemSeed}{_fileStemSuffix}";
                        FileName = $"{_fileStem}{_fileExtension}";
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion INotifyPropertyChanged Implementation
    }
}
