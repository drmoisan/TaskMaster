using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.AccessControl;

namespace UtilitiesCS.HelperClasses.FileSystem
{
    internal sealed class PhysicalDirectoryInfoAdapter(DirectoryInfo directoryInfo) : IDirectoryInfo
    {
        private readonly DirectoryInfo _directoryInfo =
            directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));

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

        public void Create() => _directoryInfo.Create();

        public void Create(DirectorySecurity directorySecurity) =>
            _directoryInfo.Create(directorySecurity);

        public IDirectoryInfo CreateSubdirectory(string path) =>
            new DirectoryInfoWrapper(_directoryInfo.CreateSubdirectory(path));

        public IDirectoryInfo CreateSubdirectory(
            string path,
            DirectorySecurity directorySecurity
        ) => new DirectoryInfoWrapper(_directoryInfo.CreateSubdirectory(path, directorySecurity));

        public void Delete() => _directoryInfo.Delete();

        public void Delete(bool recursive) => _directoryInfo.Delete(recursive);

        public IEnumerable<IDirectoryInfo> EnumerateDirectories() =>
            _directoryInfo
                .EnumerateDirectories()
                .Select(directory => new DirectoryInfoWrapper(directory));

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern) =>
            _directoryInfo
                .EnumerateDirectories(searchPattern)
                .Select(directory => new DirectoryInfoWrapper(directory));

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(
            string searchPattern,
            SearchOption searchOption
        ) =>
            _directoryInfo
                .EnumerateDirectories(searchPattern, searchOption)
                .Select(directory => new DirectoryInfoWrapper(directory));

        public IEnumerable<IFileInfo> EnumerateFiles() =>
            _directoryInfo.EnumerateFiles().Select(file => new FileInfoWrapper(file));

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern) =>
            _directoryInfo.EnumerateFiles(searchPattern).Select(file => new FileInfoWrapper(file));

        public IEnumerable<IFileInfo> EnumerateFiles(
            string searchPattern,
            SearchOption searchOption
        ) =>
            _directoryInfo
                .EnumerateFiles(searchPattern, searchOption)
                .Select(file => new FileInfoWrapper(file));

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos() =>
            _directoryInfo.EnumerateFileSystemInfos().Select(WrapFileSystemInfo);

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern) =>
            _directoryInfo.EnumerateFileSystemInfos(searchPattern).Select(WrapFileSystemInfo);

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
            string searchPattern,
            SearchOption searchOption
        ) =>
            _directoryInfo
                .EnumerateFileSystemInfos(searchPattern, searchOption)
                .Select(WrapFileSystemInfo);

        public DirectorySecurity GetAccessControl() => _directoryInfo.GetAccessControl();

        public DirectorySecurity GetAccessControl(AccessControlSections includeSections) =>
            _directoryInfo.GetAccessControl(includeSections);

        public IDirectoryInfo[] GetDirectories() =>
            _directoryInfo
                .GetDirectories()
                .Select(directory => new DirectoryInfoWrapper(directory))
                .ToArray();

        public IDirectoryInfo[] GetDirectories(string searchPattern) =>
            _directoryInfo
                .GetDirectories(searchPattern)
                .Select(directory => new DirectoryInfoWrapper(directory))
                .ToArray();

        public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption) =>
            _directoryInfo
                .GetDirectories(searchPattern, searchOption)
                .Select(directory => new DirectoryInfoWrapper(directory))
                .ToArray();

        public IFileInfo[] GetFiles() =>
            _directoryInfo.GetFiles().Select(file => new FileInfoWrapper(file)).ToArray();

        public IFileInfo[] GetFiles(string searchPattern) =>
            _directoryInfo
                .GetFiles(searchPattern)
                .Select(file => new FileInfoWrapper(file))
                .ToArray();

        public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption) =>
            _directoryInfo
                .GetFiles(searchPattern, searchOption)
                .Select(file => new FileInfoWrapper(file))
                .ToArray();

        public IFileSystemInfo[] GetFileSystemInfos() =>
            _directoryInfo.GetFileSystemInfos().Select(WrapFileSystemInfo).ToArray();

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern) =>
            _directoryInfo.GetFileSystemInfos(searchPattern).Select(WrapFileSystemInfo).ToArray();

        public IFileSystemInfo[] GetFileSystemInfos(
            string searchPattern,
            SearchOption searchOption
        ) =>
            _directoryInfo
                .GetFileSystemInfos(searchPattern, searchOption)
                .Select(WrapFileSystemInfo)
                .ToArray();

        public void GetObjectData(SerializationInfo info, StreamingContext context) =>
            _directoryInfo.GetObjectData(info, context);

        public void MoveTo(string destDirName) => _directoryInfo.MoveTo(destDirName);

        public void Refresh() => _directoryInfo.Refresh();

        public void SetAccessControl(DirectorySecurity directorySecurity) =>
            _directoryInfo.SetAccessControl(directorySecurity);

        public override string ToString() => _directoryInfo.ToString();

        private static IFileSystemInfo WrapFileSystemInfo(FileSystemInfo info)
        {
            if (info is FileInfo fileInfo)
            {
                return new FileInfoWrapper(fileInfo);
            }

            if (info is DirectoryInfo directoryInfo)
            {
                return new DirectoryInfoWrapper(directoryInfo);
            }

            throw new ArgumentException("Unsupported FileSystemInfo type", nameof(info));
        }
    }
}
