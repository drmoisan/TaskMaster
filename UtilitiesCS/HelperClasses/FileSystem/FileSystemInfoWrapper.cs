using System;
using System.IO;
using System.Runtime.Serialization;

namespace UtilitiesCS.HelperClasses.FileSystem
{
    public class FileSystemInfoWrapper : IFileSystemInfo
    {
        private readonly FileSystemInfo _fileSystemInfo;
        
        public FileSystemInfoWrapper(FileSystemInfo fileSystemInfo)
        {
            _fileSystemInfo = fileSystemInfo ?? throw new ArgumentNullException(nameof(fileSystemInfo));
        }

        public FileAttributes Attributes
        {
            get => _fileSystemInfo.Attributes;
            set => _fileSystemInfo.Attributes = value;
        }

        public DateTime CreationTime
        {
            get => _fileSystemInfo.CreationTime;
            set => _fileSystemInfo.CreationTime = value;
        }

        public DateTime CreationTimeUtc
        {
            get => _fileSystemInfo.CreationTimeUtc;
            set => _fileSystemInfo.CreationTimeUtc = value;
        }

        public bool Exists => _fileSystemInfo.Exists;

        public string Extension => _fileSystemInfo.Extension;

        public string FullName => _fileSystemInfo.FullName;

        public DateTime LastAccessTime
        {
            get => _fileSystemInfo.LastAccessTime;
            set => _fileSystemInfo.LastAccessTime = value;
        }

        public DateTime LastAccessTimeUtc
        {
            get => _fileSystemInfo.LastAccessTimeUtc;
            set => _fileSystemInfo.LastAccessTimeUtc = value;
        }

        public DateTime LastWriteTime
        {
            get => _fileSystemInfo.LastWriteTime;
            set => _fileSystemInfo.LastWriteTime = value;
        }

        public DateTime LastWriteTimeUtc
        {
            get => _fileSystemInfo.LastWriteTimeUtc;
            set => _fileSystemInfo.LastWriteTimeUtc = value;
        }

        public string Name => _fileSystemInfo.Name;

        public void Delete()
        {
            _fileSystemInfo.Delete();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _fileSystemInfo.GetObjectData(info, context);
        }

        public void Refresh()
        {
            _fileSystemInfo.Refresh();
        }
    }
}
