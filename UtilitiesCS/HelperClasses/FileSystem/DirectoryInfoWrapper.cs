#nullable enable
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
        private readonly IDirectoryInfo _directoryInfo;

        public DirectoryInfoWrapper(DirectoryInfo directoryInfo)
            : this(
                directoryInfo is null
                    ? throw new ArgumentNullException(nameof(directoryInfo))
                    : new PhysicalDirectoryInfoAdapter(directoryInfo)
            ) { }

        internal DirectoryInfoWrapper(IDirectoryInfo directoryInfo)
        {
            _directoryInfo =
                directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
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

        public IDirectoryInfo Parent => _directoryInfo.Parent;

        public IDirectoryInfo Root => _directoryInfo.Root;

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
            return _directoryInfo.CreateSubdirectory(path);
        }

        public IDirectoryInfo CreateSubdirectory(string path, DirectorySecurity directorySecurity)
        {
            return _directoryInfo.CreateSubdirectory(path, directorySecurity);
        }

        public void Delete(bool recursive)
        {
            _directoryInfo.Delete(recursive);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories()
        {
            return _directoryInfo.EnumerateDirectories();
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            return _directoryInfo.EnumerateDirectories(searchPattern);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(
            string searchPattern,
            SearchOption searchOption
        )
        {
            return _directoryInfo.EnumerateDirectories(searchPattern, searchOption);
        }

        public IEnumerable<IFileInfo> EnumerateFiles()
        {
            return _directoryInfo.EnumerateFiles();
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
        {
            return _directoryInfo.EnumerateFiles(searchPattern);
        }

        public IEnumerable<IFileInfo> EnumerateFiles(
            string searchPattern,
            SearchOption searchOption
        )
        {
            return _directoryInfo.EnumerateFiles(searchPattern, searchOption);
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
        {
            return _directoryInfo.EnumerateFileSystemInfos();
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            return _directoryInfo.EnumerateFileSystemInfos(searchPattern);
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
            string searchPattern,
            SearchOption searchOption
        )
        {
            return _directoryInfo.EnumerateFileSystemInfos(searchPattern, searchOption);
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
            return _directoryInfo.GetDirectories();
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern)
        {
            return _directoryInfo.GetDirectories(searchPattern);
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.GetDirectories(searchPattern, searchOption);
        }

        public IFileInfo[] GetFiles()
        {
            return _directoryInfo.GetFiles();
        }

        public IFileInfo[] GetFiles(string searchPattern)
        {
            return _directoryInfo.GetFiles(searchPattern);
        }

        public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.GetFiles(searchPattern, searchOption);
        }

        public IFileSystemInfo[] GetFileSystemInfos()
        {
            return _directoryInfo.GetFileSystemInfos();
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            return _directoryInfo.GetFileSystemInfos(searchPattern);
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return _directoryInfo.GetFileSystemInfos(searchPattern, searchOption);
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
    }
}
