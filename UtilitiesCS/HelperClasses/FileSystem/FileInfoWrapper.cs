using System;
using System.IO;
using System.Security.AccessControl;
using System.Runtime.Serialization;
using System.Linq;

namespace UtilitiesCS.HelperClasses.FileSystem
{
    public class FileInfoWrapper : IFileInfo
    {
        private readonly FileInfo _fileInfo;

        public FileInfoWrapper(FileInfo fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        }

        public FileAttributes Attributes
        {
            get => _fileInfo.Attributes;
            set => _fileInfo.Attributes = value;
        }

        public DateTime CreationTime
        {
            get => _fileInfo.CreationTime;
            set => _fileInfo.CreationTime = value;
        }

        public DateTime CreationTimeUtc
        {
            get => _fileInfo.CreationTimeUtc;
            set => _fileInfo.CreationTimeUtc = value;
        }

        public bool Exists => _fileInfo.Exists;

        public string Extension => _fileInfo.Extension;

        public string FullName => _fileInfo.FullName;

        public DateTime LastAccessTime
        {
            get => _fileInfo.LastAccessTime;
            set => _fileInfo.LastAccessTime = value;
        }

        public DateTime LastAccessTimeUtc
        {
            get => _fileInfo.LastAccessTimeUtc;
            set => _fileInfo.LastAccessTimeUtc = value;
        }

        public DateTime LastWriteTime
        {
            get => _fileInfo.LastWriteTime;
            set => _fileInfo.LastWriteTime = value;
        }

        public DateTime LastWriteTimeUtc
        {
            get => _fileInfo.LastWriteTimeUtc;
            set => _fileInfo.LastWriteTimeUtc = value;
        }

        public string Name => _fileInfo.Name;

        public IDirectoryInfo Directory => new DirectoryInfoWrapper(_fileInfo.Directory);

        public string DirectoryName => _fileInfo.DirectoryName;

        public bool IsReadOnly
        {
            get => _fileInfo.IsReadOnly;
            set => _fileInfo.IsReadOnly = value;
        }

        public long Length => _fileInfo.Length;

        public StreamWriter AppendText()
        {
            return _fileInfo.AppendText();
        }

        public IFileInfo CopyTo(string destFileName)
        {
            return new FileInfoWrapper(_fileInfo.CopyTo(destFileName));
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite)
        {
            return new FileInfoWrapper(_fileInfo.CopyTo(destFileName, overwrite));
        }

        public FileStream Create()
        {
            return _fileInfo.Create();
        }

        public StreamWriter CreateText()
        {
            return _fileInfo.CreateText();
        }

        public void Decrypt()
        {
            _fileInfo.Decrypt();
        }

        public void Encrypt()
        {
            _fileInfo.Encrypt();
        }

        public FileSecurity GetAccessControl()
        {
            return _fileInfo.GetAccessControl();
        }

        public FileSecurity GetAccessControl(AccessControlSections includeSections)
        {
            return _fileInfo.GetAccessControl(includeSections);
        }

        public void MoveTo(string destFileName)
        {
            _fileInfo.MoveTo(destFileName);
        }

        public FileStream Open(FileMode mode)
        {
            return _fileInfo.Open(mode);
        }

        public FileStream Open(FileMode mode, FileAccess access)
        {
            return _fileInfo.Open(mode, access);
        }

        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return _fileInfo.Open(mode, access, share);
        }

        public FileStream OpenRead()
        {
            return _fileInfo.OpenRead();
        }

        public StreamReader OpenText()
        {
            return _fileInfo.OpenText();
        }

        public FileStream OpenWrite()
        {
            return _fileInfo.OpenWrite();
        }

        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName)
        {
            return new FileInfoWrapper(_fileInfo.Replace(destinationFileName, destinationBackupFileName));
        }

        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            return new FileInfoWrapper(_fileInfo.Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors));
        }

        public void SetAccessControl(FileSecurity fileSecurity)
        {
            _fileInfo.SetAccessControl(fileSecurity);
        }

        public override string ToString()
        {
            return _fileInfo.ToString();
        }

        public void Delete()
        {
            _fileInfo.Delete();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _fileInfo.GetObjectData(info, context);
        }

        public void Refresh()
        {
            _fileInfo.Refresh();
        }

        public static explicit operator DirectoryInfoWrapper(FileInfoWrapper fileInfoWrapper)
        {
            return (DirectoryInfoWrapper)fileInfoWrapper.Directory;
        }
    }
}
