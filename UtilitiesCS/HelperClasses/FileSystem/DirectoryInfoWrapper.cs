using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.AccessControl;

namespace UtilitiesCS.HelperClasses.FileSystem
{
    public class DirectoryInfoWrapper : IDirectoryInfo
    {
        private readonly DirectoryInfo _directoryInfo;

        public DirectoryInfoWrapper(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
        }

        public FileAttributes Attributes
        {
            get => _directoryInfo.Attributes;
            set => _directoryInfo.Attributes = value;
        }

        public DateTime CreationTime
        {
            get => _directoryInfo.CreationTime;
            set => _directoryInfo.CreationTime = value;
        }

        public DateTime CreationTimeUtc
        {
            get => _directoryInfo.CreationTimeUtc;
            set => _directoryInfo.CreationTimeUtc = value;
        }

        public bool Exists => _directoryInfo.Exists;

        public string Extension => _directoryInfo.Extension;

        public string FullName => _directoryInfo.FullName;

        public DateTime LastAccessTime
        {
            get => _directoryInfo.LastAccessTime;
            set => _directoryInfo.LastAccessTime = value;
        }

        public DateTime LastAccessTimeUtc
        {
            get => _directoryInfo.LastAccessTimeUtc;
            set => _directoryInfo.LastAccessTimeUtc = value;
        }

        public DateTime LastWriteTime
        {
            get => _directoryInfo.LastWriteTime;
            set => _directoryInfo.LastWriteTime = value;
        }

        public DateTime LastWriteTimeUtc
        {
            get => _directoryInfo.LastWriteTimeUtc;
            set => _directoryInfo.LastWriteTimeUtc = value;
        }

        public string Name => _directoryInfo.Name;

        public IDirectoryInfo Parent => new DirectoryInfoWrapper(_directoryInfo.Parent);

        public IDirectoryInfo Root => new DirectoryInfoWrapper(_directoryInfo.Root);

        public void Create()
        {
            _directoryInfo.Create();
        }

        public void Create(DirectorySecurity directorySecurity)
        {
            _directoryInfo.Create(directorySecurity);
        }

        public IDirectoryInfo CreateSubdirectory(string path)
        {
            return new DirectoryInfoWrapper(_directoryInfo.CreateSubdirectory(path));
        }

        public IDirectoryInfo CreateSubdirectory(string path, DirectorySecurity directorySecurity)
        {
            return new DirectoryInfoWrapper(_directoryInfo.CreateSubdirectory(path, directorySecurity));
        }

        public void Delete(bool recursive)
        {
            _directoryInfo.Delete(recursive);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories()
        {
            return _directoryInfo.EnumerateDirectories().Select(d => new DirectoryInfoWrapper(d));
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            return _directoryInfo.EnumerateDirectories(searchPattern).Select(d => new DirectoryInfoWrapper(d));
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.EnumerateDirectories(searchPattern, searchOption).Select(d => new DirectoryInfoWrapper(d));
        }

        public IEnumerable<IFileInfo> EnumerateFiles()
        {
            return _directoryInfo.EnumerateFiles().Select(f => new FileInfoWrapper(f));
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
        {
            return _directoryInfo.EnumerateFiles(searchPattern).Select(f => new FileInfoWrapper(f));
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.EnumerateFiles(searchPattern, searchOption).Select(f => new FileInfoWrapper(f));
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
        {
            return _directoryInfo.EnumerateFileSystemInfos().Select(fsi => WrapFileSystemInfo(fsi));
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            return _directoryInfo.EnumerateFileSystemInfos(searchPattern).Select(fsi => WrapFileSystemInfo(fsi));
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.EnumerateFileSystemInfos(searchPattern, searchOption).Select(fsi => WrapFileSystemInfo(fsi));
        }

        public DirectorySecurity GetAccessControl()
        {
            return _directoryInfo.GetAccessControl();
        }

        public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
        {
            return _directoryInfo.GetAccessControl(includeSections);
        }

        public IDirectoryInfo[] GetDirectories()
        {
            return _directoryInfo.GetDirectories().Select(d => new DirectoryInfoWrapper(d)).ToArray();
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern)
        {
            return _directoryInfo.GetDirectories(searchPattern).Select(d => new DirectoryInfoWrapper(d)).ToArray();
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.GetDirectories(searchPattern, searchOption).Select(d => new DirectoryInfoWrapper(d)).ToArray();
        }

        public IFileInfo[] GetFiles()
        {
            return _directoryInfo.GetFiles().Select(f => new FileInfoWrapper(f)).ToArray();
        }

        public IFileInfo[] GetFiles(string searchPattern)
        {
            return _directoryInfo.GetFiles(searchPattern).Select(f => new FileInfoWrapper(f)).ToArray();
        }

        public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.GetFiles(searchPattern, searchOption).Select(f => new FileInfoWrapper(f)).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos()
        {
            return _directoryInfo.GetFileSystemInfos().Select(fsi => WrapFileSystemInfo(fsi)).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            return _directoryInfo.GetFileSystemInfos(searchPattern).Select(fsi => WrapFileSystemInfo(fsi)).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.GetFileSystemInfos(searchPattern, searchOption).Select(fsi => WrapFileSystemInfo(fsi)).ToArray();
        }

        public void MoveTo(string destDirName)
        {
            _directoryInfo.MoveTo(destDirName);
        }

        public void SetAccessControl(DirectorySecurity directorySecurity)
        {
            _directoryInfo.SetAccessControl(directorySecurity);
        }

        public override string ToString()
        {
            return _directoryInfo.ToString();
        }

        public void Delete()
        {
            _directoryInfo.Delete();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _directoryInfo.GetObjectData(info, context);
        }

        public void Refresh()
        {
            _directoryInfo.Refresh();
        }

        private IFileSystemInfo WrapFileSystemInfo(FileSystemInfo fsi)
        {
            if (fsi is FileInfo fileInfo)
                return new FileInfoWrapper(fileInfo);
            if (fsi is DirectoryInfo directoryInfo)
                return new DirectoryInfoWrapper(directoryInfo);
            throw new ArgumentException("Unsupported FileSystemInfo type", nameof(fsi));
        }
    }
}
