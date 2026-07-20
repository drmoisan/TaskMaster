#nullable enable
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.AccessControl;

namespace UtilitiesCS.HelperClasses.FileSystem
{
    internal sealed class PhysicalFileInfoAdapter : IFileInfo
    {
        private readonly FileInfo _fileInfo;

        // The write-mode members delegate through these fields rather than calling the wrapped
        // FileInfo directly. Why: this narrow injectable-delegate seam lets unit tests cover the
        // AppendText/Open(mode)/OpenWrite delegation deterministically without acquiring a real
        // write/append handle on any shared file. Open(FileMode, FileAccess) is also included in
        // this seam because its default FileShare.None behavior can contend with a shared/real
        // file (not because it is a write-mode member). The public constructor binds the defaults
        // to the real FileInfo, so production behavior is unchanged.
        private readonly Func<StreamWriter> _appendText;
        private readonly Func<FileMode, FileStream> _openByMode;
        private readonly Func<FileMode, FileAccess, FileStream> _openByModeAndAccess;
        private readonly Func<FileStream> _openWrite;

        public PhysicalFileInfoAdapter(FileInfo fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _appendText = _fileInfo.AppendText;
            _openByMode = _fileInfo.Open;
            _openByModeAndAccess = _fileInfo.Open;
            _openWrite = _fileInfo.OpenWrite;
        }

        internal PhysicalFileInfoAdapter(
            FileInfo fileInfo,
            Func<StreamWriter> appendText,
            Func<FileMode, FileStream> openByMode,
            Func<FileMode, FileAccess, FileStream> openByModeAndAccess,
            Func<FileStream> openWrite
        )
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _appendText = appendText ?? throw new ArgumentNullException(nameof(appendText));
            _openByMode = openByMode ?? throw new ArgumentNullException(nameof(openByMode));
            _openByModeAndAccess =
                openByModeAndAccess ?? throw new ArgumentNullException(nameof(openByModeAndAccess));
            _openWrite = openWrite ?? throw new ArgumentNullException(nameof(openWrite));
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

        // Behavior-preserving `!`: at a filesystem root FileInfo.Directory is null and the
        // DirectoryInfoWrapper ctor throws ArgumentNullException, exactly as before annotation.
        // The wrapped IDirectoryInfo interface is out of scope (oblivious), so `!` preserves the
        // latent root-throws behavior rather than changing the contract. FLAGGED (evidence/other).
        public IDirectoryInfo Directory => new DirectoryInfoWrapper(_fileInfo.Directory!);

        // Behavior-preserving `!`: FileInfo.DirectoryName is null only at a root; returning it as
        // the non-null interface contract preserves the current behavior. FLAGGED.
        public string DirectoryName => _fileInfo.DirectoryName!;

        public bool IsReadOnly
        {
            get => _fileInfo.IsReadOnly;
            set => _fileInfo.IsReadOnly = value;
        }

        public long Length => _fileInfo.Length;

        public StreamWriter AppendText() => _appendText();

        public IFileInfo CopyTo(string destFileName) =>
            new FileInfoWrapper(_fileInfo.CopyTo(destFileName));

        public IFileInfo CopyTo(string destFileName, bool overwrite) =>
            new FileInfoWrapper(_fileInfo.CopyTo(destFileName, overwrite));

        public FileStream Create() => _fileInfo.Create();

        public StreamWriter CreateText() => _fileInfo.CreateText();

        public void Decrypt() => _fileInfo.Decrypt();

        public void Delete() => _fileInfo.Delete();

        public void Encrypt() => _fileInfo.Encrypt();

        public FileSecurity GetAccessControl() => _fileInfo.GetAccessControl();

        public FileSecurity GetAccessControl(AccessControlSections includeSections) =>
            _fileInfo.GetAccessControl(includeSections);

        public void GetObjectData(SerializationInfo info, StreamingContext context) =>
            _fileInfo.GetObjectData(info, context);

        public void MoveTo(string destFileName) => _fileInfo.MoveTo(destFileName);

        public FileStream Open(FileMode mode) => _openByMode(mode);

        public FileStream Open(FileMode mode, FileAccess access) =>
            _openByModeAndAccess(mode, access);

        public FileStream Open(FileMode mode, FileAccess access, FileShare share) =>
            _fileInfo.Open(mode, access, share);

        public FileStream OpenRead() => _fileInfo.OpenRead();

        public StreamReader OpenText() => _fileInfo.OpenText();

        public FileStream OpenWrite() => _openWrite();

        public void Refresh() => _fileInfo.Refresh();

        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName) =>
            new FileInfoWrapper(_fileInfo.Replace(destinationFileName, destinationBackupFileName));

        public IFileInfo Replace(
            string destinationFileName,
            string destinationBackupFileName,
            bool ignoreMetadataErrors
        ) =>
            new FileInfoWrapper(
                _fileInfo.Replace(
                    destinationFileName,
                    destinationBackupFileName,
                    ignoreMetadataErrors
                )
            );

        public void SetAccessControl(FileSecurity fileSecurity) =>
            _fileInfo.SetAccessControl(fileSecurity);

        public override string ToString() => _fileInfo.ToString();
    }
}
