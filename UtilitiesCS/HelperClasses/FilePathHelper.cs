using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class FilePathHelper: INotifyPropertyChanged
    {
        public FilePathHelper() { PropertyChanged += FilePathHelper_PropertyChanged; }

        public FilePathHelper(string fileName, string folderPath)
        {
            FileName = fileName;
            FolderPath = folderPath;
            PropertyChanged += FilePathHelper_PropertyChanged;
        }

        

        private string _filePath = "";
        public string FilePath { get => _filePath; set { _filePath = value; NotifyPropertyChanged(); } }
        
        private string _folderPath = "";
        public string FolderPath { get=> _folderPath; set { _folderPath = value; NotifyPropertyChanged(); } }

        private string _fileName = "";
        public string FileName { get => _fileName; set { _fileName = value; NotifyPropertyChanged(); }}

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
                    _folderPath = Path.GetDirectoryName(_filePath);
                    _fileName = Path.GetFileName(_filePath);
                    if (_fileName == "") 
                        throw new ArgumentException($"FilePath {_filePath} must include a FileName");
                    break;
                default:
                    break;
            }
        }

    }
}
