using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses.FileSystem;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class PhysicalFileSystemAdapters_Tests
    {
        // Issue #940 fixtures. The running host's own loaded assembly image exists for the whole
        // run and its physical path is the project output directory, so that directory and its
        // parent are owned by this test process; no repository file and no temporary file is
        // involved. Every mutating member is invoked either on a path under the owned directory
        // that is asserted absent beforehand, so the wrapped BCL member reports the missing path
        // before touching the disk, or as a no-op on an owned entry that already exists.
        private static FileInfo OwnedAssemblyFile =>
            new FileInfo(typeof(PhysicalFileSystemAdapters_Tests).Assembly.Location);

        private static DirectoryInfo OwnedAssemblyDirectory => OwnedAssemblyFile.Directory;

        private static string MissingOwnedPath(string suffix) =>
            Path.Combine(OwnedAssemblyDirectory.FullName, "__940_missing_" + suffix);

        [TestMethod]
        public void PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory()
        {
            // Arrange
            var directory = OwnedAssemblyDirectory;
            var parent = directory.Parent;
            var adapter = new PhysicalDirectoryInfoAdapter(directory);
            var parentAdapter = new PhysicalDirectoryInfoAdapter(parent);
            var security = adapter.GetAccessControl();
            var serialized = new SerializationInfo(
                typeof(PhysicalDirectoryInfoAdapter),
                new FormatterConverter()
            );
            var context = new StreamingContext(StreamingContextStates.All);

            // Act: the creation members are invoked on entries that already exist, so nothing
            // is created; the returned wrappers name the owned directory, which proves both the
            // delegation and the wrapping.
            adapter.Create();
            adapter.Create(directory.GetAccessControl());
            var createdSubdirectory = parentAdapter.CreateSubdirectory(directory.Name);
            var createdSubdirectoryWithSecurity = parentAdapter.CreateSubdirectory(
                directory.Name,
                parent.GetAccessControl()
            );
            var accessWithSections = adapter.GetAccessControl(AccessControlSections.Access);
            adapter.GetObjectData(serialized, context);
            adapter.Refresh();
            adapter.SetAccessControl(security);
            var toStringValue = adapter.ToString();

            // Assert
            adapter.Exists.Should().BeTrue();
            adapter.Attributes.Should().Be(directory.Attributes);
            adapter.CreationTime.Should().Be(directory.CreationTime);
            adapter.CreationTimeUtc.Should().Be(directory.CreationTimeUtc);
            adapter.LastAccessTime.Should().Be(directory.LastAccessTime);
            adapter.LastAccessTimeUtc.Should().Be(directory.LastAccessTimeUtc);
            adapter.LastWriteTime.Should().Be(directory.LastWriteTime);
            adapter.LastWriteTimeUtc.Should().Be(directory.LastWriteTimeUtc);
            adapter.Extension.Should().Be(directory.Extension);
            adapter.FullName.Should().Be(directory.FullName);
            adapter.Name.Should().Be(directory.Name);
            adapter.Parent.FullName.Should().Be(parent.FullName);
            adapter.Root.FullName.Should().Be(directory.Root.FullName);
            createdSubdirectory.FullName.Should().Be(directory.FullName);
            createdSubdirectoryWithSecurity.FullName.Should().Be(directory.FullName);
            security.Should().NotBeNull();
            accessWithSections.Should().NotBeNull();
            toStringValue.Should().Be(directory.ToString());
            serialized.MemberCount.Should().BeGreaterThan(0, "GetObjectData adds values");
        }

        [TestMethod]
        public void PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries()
        {
            // Arrange
            var directory = OwnedAssemblyDirectory;
            var fileName = OwnedAssemblyFile.Name;
            var directoryName = directory.Name;
            var adapter = new PhysicalDirectoryInfoAdapter(directory);
            var parentAdapter = new PhysicalDirectoryInfoAdapter(directory.Parent);

            // Act: files are enumerated from the owned directory and directories from its
            // parent, because the parent containing the owned directory is guaranteed by
            // construction while subdirectories inside the owned directory are not.
            var enumeratedDirectories = parentAdapter.EnumerateDirectories();
            var enumeratedDirectoriesByPattern = parentAdapter.EnumerateDirectories(directoryName);
            var enumeratedDirectoriesByPatternAndOption = parentAdapter.EnumerateDirectories(
                directoryName,
                SearchOption.TopDirectoryOnly
            );
            var enumeratedFiles = adapter.EnumerateFiles();
            var enumeratedFilesByPattern = adapter.EnumerateFiles(fileName);
            var enumeratedFilesByPatternAndOption = adapter.EnumerateFiles(
                fileName,
                SearchOption.TopDirectoryOnly
            );
            var enumeratedInfos = adapter.EnumerateFileSystemInfos();
            var enumeratedInfosByPattern = parentAdapter.EnumerateFileSystemInfos(directoryName);
            var enumeratedInfosByPatternAndOption = adapter.EnumerateFileSystemInfos(
                fileName,
                SearchOption.TopDirectoryOnly
            );
            var directories = parentAdapter.GetDirectories();
            var directoriesByPattern = parentAdapter.GetDirectories(directoryName);
            var directoriesByPatternAndOption = parentAdapter.GetDirectories(
                directoryName,
                SearchOption.TopDirectoryOnly
            );
            var files = adapter.GetFiles();
            var filesByPattern = adapter.GetFiles(fileName);
            var filesByPatternAndOption = adapter.GetFiles(fileName, SearchOption.TopDirectoryOnly);
            var infos = parentAdapter.GetFileSystemInfos();
            var infosByPattern = adapter.GetFileSystemInfos(fileName);
            var infosByPatternAndOption = parentAdapter.GetFileSystemInfos(
                directoryName,
                SearchOption.TopDirectoryOnly
            );

            // Assert
            files.Select(item => item.Name).Should().Contain(fileName);
            files.Should().OnlyContain(item => item is FileInfoWrapper);
            filesByPattern.Should().ContainSingle().Which.Name.Should().Be(fileName);
            filesByPatternAndOption.Should().ContainSingle().Which.Name.Should().Be(fileName);
            enumeratedFiles.Select(item => item.Name).Should().Contain(fileName);
            enumeratedFilesByPattern.Should().ContainSingle().Which.Name.Should().Be(fileName);
            enumeratedFilesByPatternAndOption
                .Should()
                .ContainSingle()
                .Which.Name.Should()
                .Be(fileName);
            directories.Select(item => item.Name).Should().Contain(directoryName);
            directories.Should().OnlyContain(item => item is DirectoryInfoWrapper);
            directoriesByPattern.Should().ContainSingle().Which.Name.Should().Be(directoryName);
            directoriesByPatternAndOption
                .Should()
                .ContainSingle()
                .Which.Name.Should()
                .Be(directoryName);
            enumeratedDirectories.Select(item => item.Name).Should().Contain(directoryName);
            enumeratedDirectoriesByPattern
                .Should()
                .ContainSingle()
                .Which.Name.Should()
                .Be(directoryName);
            enumeratedDirectoriesByPatternAndOption
                .Should()
                .ContainSingle()
                .Which.Name.Should()
                .Be(directoryName);
            enumeratedInfos
                .OfType<FileInfoWrapper>()
                .Select(item => item.Name)
                .Should()
                .Contain(fileName);
            enumeratedInfosByPattern
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<DirectoryInfoWrapper>();
            enumeratedInfosByPatternAndOption
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<FileInfoWrapper>();
            infos
                .OfType<DirectoryInfoWrapper>()
                .Select(item => item.Name)
                .Should()
                .Contain(directoryName);
            infosByPattern.Should().ContainSingle().Which.Should().BeOfType<FileInfoWrapper>();
            infosByPatternAndOption
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<DirectoryInfoWrapper>();
        }

        [TestMethod]
        public void PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries()
        {
            // Arrange
            var adapter = new PhysicalDirectoryInfoAdapter(
                new DirectoryInfo(MissingOwnedPath("directory-setters"))
            );
            var stamp = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            adapter.Exists.Should().BeFalse();

            // Act
            Action setCreationTime = () => adapter.CreationTime = stamp;
            Action setCreationTimeUtc = () => adapter.CreationTimeUtc = stamp;
            Action setLastAccessTime = () => adapter.LastAccessTime = stamp;
            Action setLastAccessTimeUtc = () => adapter.LastAccessTimeUtc = stamp;
            Action setLastWriteTime = () => adapter.LastWriteTime = stamp;
            Action setLastWriteTimeUtc = () => adapter.LastWriteTimeUtc = stamp;
            Action setAttributes = () => adapter.Attributes = FileAttributes.Directory;

            // Assert: each setter reaches the wrapped BCL member, which reports the missing path.
            setCreationTime.Should().Throw<FileNotFoundException>();
            setCreationTimeUtc.Should().Throw<FileNotFoundException>();
            setLastAccessTime.Should().Throw<FileNotFoundException>();
            setLastAccessTimeUtc.Should().Throw<FileNotFoundException>();
            setLastWriteTime.Should().Throw<FileNotFoundException>();
            setLastWriteTimeUtc.Should().Throw<FileNotFoundException>();
            setAttributes.Should().Throw<FileNotFoundException>();
            adapter.Refresh();
            adapter.Exists.Should().BeFalse();
        }

        [TestMethod]
        public void PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected()
        {
            // Arrange
            var adapter = new PhysicalDirectoryInfoAdapter(
                new DirectoryInfo(MissingOwnedPath("directory-delete"))
            );
            var wrapMethod = typeof(PhysicalDirectoryInfoAdapter).GetMethod(
                "WrapFileSystemInfo",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;
            var directoryMoveTarget = MissingOwnedPath("directory-moved");
            adapter.Exists.Should().BeFalse();
            Directory.Exists(directoryMoveTarget).Should().BeFalse();

            // Act
            Action delete = () => adapter.Delete();
            Action deleteRecursive = () => adapter.Delete(recursive: true);
            Action move = () => adapter.MoveTo(directoryMoveTarget);
            Action wrapUnsupported = () =>
                wrapMethod.Invoke(null, new object[] { new UnsupportedInfo() });

            // Assert
            delete.Should().Throw<DirectoryNotFoundException>();
            deleteRecursive.Should().Throw<DirectoryNotFoundException>();
            move.Should().Throw<DirectoryNotFoundException>();
            wrapUnsupported
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentException>();
        }

        [TestMethod]
        public void PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo()
        {
            // Arrange
            var file = OwnedAssemblyFile;
            var adapter = new PhysicalFileInfoAdapter(file);
            var security = adapter.GetAccessControl();
            var serialized = new SerializationInfo(
                typeof(PhysicalFileInfoAdapter),
                new FormatterConverter()
            );
            var context = new StreamingContext(StreamingContextStates.All);

            // Act: the read-only opens target the running host's own loaded assembly image. The
            // three-argument Open requests read-write sharing; OpenRead and OpenText request read
            // sharing, which admits the loader's own read handles on the image.
            bool openModeReadSharedCanRead;
            using (
                var openModeReadShared = adapter.Open(
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                )
            )
            {
                openModeReadSharedCanRead = openModeReadShared.CanRead;
            }

            bool openReadCanRead;
            using (var openRead = adapter.OpenRead())
            {
                openReadCanRead = openRead.CanRead;
            }

            string openTextLine;
            using (var openText = adapter.OpenText())
            {
                openTextLine = openText.ReadLine();
            }

            // The write-mode members and the two-argument Open are exercised through the
            // adapter's internal injectable-delegate seam with test-owned sentinel streams, so no
            // write handle and no exclusive handle is ever requested on the image. The append
            // sentinel wraps an in-memory stream because StreamWriter requires a writable backing
            // stream; the other sentinels are read-only opens of the image with read-write sharing.
            using var sentinelAppendStream = new MemoryStream();
            using var sentinelAppendWriter = new StreamWriter(
                sentinelAppendStream,
                System.Text.Encoding.UTF8,
                1024,
                leaveOpen: true
            );
            using var sentinelOpenModeStream = new FileStream(
                typeof(PhysicalFileSystemAdapters_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var sentinelOpenModeAndAccessStream = new FileStream(
                typeof(PhysicalFileSystemAdapters_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var sentinelOpenWriteStream = new FileStream(
                typeof(PhysicalFileSystemAdapters_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            var seamAdapter = new PhysicalFileInfoAdapter(
                file,
                () => sentinelAppendWriter,
                _ => sentinelOpenModeStream,
                (mode, access) => sentinelOpenModeAndAccessStream,
                () => sentinelOpenWriteStream
            );

            var accessWithSections = adapter.GetAccessControl(AccessControlSections.Access);
            adapter.GetObjectData(serialized, context);
            adapter.Refresh();
            adapter.SetAccessControl(security);
            var toStringValue = adapter.ToString();

            // Assert
            adapter.Exists.Should().BeTrue();
            adapter.Extension.Should().Be(".dll");
            adapter.FullName.Should().Be(file.FullName);
            adapter.Name.Should().Be(file.Name);
            adapter.Directory.FullName.Should().Be(file.Directory.FullName);
            adapter.DirectoryName.Should().Be(file.DirectoryName);
            adapter.Length.Should().BeGreaterThan(0);
            adapter.IsReadOnly.Should().Be(file.IsReadOnly);
            adapter.Attributes.Should().Be(file.Attributes);
            adapter.CreationTime.Should().Be(file.CreationTime);
            adapter.CreationTimeUtc.Should().Be(file.CreationTimeUtc);
            adapter.LastAccessTime.Should().Be(file.LastAccessTime);
            adapter.LastAccessTimeUtc.Should().Be(file.LastAccessTimeUtc);
            adapter.LastWriteTime.Should().Be(file.LastWriteTime);
            adapter.LastWriteTimeUtc.Should().Be(file.LastWriteTimeUtc);
            openModeReadSharedCanRead.Should().BeTrue();
            openReadCanRead.Should().BeTrue();
            openTextLine.Should().NotBeNull();
            seamAdapter.AppendText().Should().BeSameAs(sentinelAppendWriter);
            seamAdapter.Open(FileMode.Open).Should().BeSameAs(sentinelOpenModeStream);
            seamAdapter
                .Open(FileMode.Open, FileAccess.Read)
                .Should()
                .BeSameAs(sentinelOpenModeAndAccessStream);
            seamAdapter.OpenWrite().Should().BeSameAs(sentinelOpenWriteStream);
            security.Should().NotBeNull();
            accessWithSections.Should().NotBeNull();
            toStringValue.Should().Be(file.ToString());
            serialized.MemberCount.Should().BeGreaterThan(0, "GetObjectData adds values");
        }

        [TestMethod]
        public void PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles()
        {
            // Arrange
            var adapter = new PhysicalFileInfoAdapter(
                new FileInfo(MissingOwnedPath("file-setters.txt"))
            );
            var stamp = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            adapter.Exists.Should().BeFalse();

            // Act
            Action setIsReadOnly = () => adapter.IsReadOnly = true;
            Action setAttributes = () => adapter.Attributes = FileAttributes.Normal;
            Action setCreationTime = () => adapter.CreationTime = stamp;
            Action setCreationTimeUtc = () => adapter.CreationTimeUtc = stamp;
            Action setLastAccessTime = () => adapter.LastAccessTime = stamp;
            Action setLastAccessTimeUtc = () => adapter.LastAccessTimeUtc = stamp;
            Action setLastWriteTime = () => adapter.LastWriteTime = stamp;
            Action setLastWriteTimeUtc = () => adapter.LastWriteTimeUtc = stamp;

            // Assert: each setter reaches the wrapped BCL member, which reports the missing path.
            setIsReadOnly.Should().Throw<FileNotFoundException>();
            setAttributes.Should().Throw<FileNotFoundException>();
            setCreationTime.Should().Throw<FileNotFoundException>();
            setCreationTimeUtc.Should().Throw<FileNotFoundException>();
            setLastAccessTime.Should().Throw<FileNotFoundException>();
            setLastAccessTimeUtc.Should().Throw<FileNotFoundException>();
            setLastWriteTime.Should().Throw<FileNotFoundException>();
            setLastWriteTimeUtc.Should().Throw<FileNotFoundException>();
            adapter.Refresh();
            adapter.Exists.Should().BeFalse();
        }

        [TestMethod]
        public void PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles()
        {
            // Arrange: every destination and backup is another missing owned path, so no existing
            // file is a target under any outcome.
            var adapter = new PhysicalFileInfoAdapter(
                new FileInfo(MissingOwnedPath("file-source.txt"))
            );
            var copyTarget = MissingOwnedPath("file-copy.txt");
            var moveTarget = MissingOwnedPath("file-moved.txt");
            var replaceTarget = MissingOwnedPath("file-replace.txt");
            var backupTarget = MissingOwnedPath("file-backup.bak");
            adapter.Exists.Should().BeFalse();
            File.Exists(copyTarget).Should().BeFalse();
            File.Exists(moveTarget).Should().BeFalse();
            File.Exists(replaceTarget).Should().BeFalse();
            File.Exists(backupTarget).Should().BeFalse();

            // Act
            Action delete = () => adapter.Delete();
            Action copy = () => adapter.CopyTo(copyTarget);
            Action copyOverwrite = () => adapter.CopyTo(copyTarget, overwrite: true);
            Action move = () => adapter.MoveTo(moveTarget);
            Action replace = () => adapter.Replace(replaceTarget, backupTarget);
            Action replaceIgnore = () =>
                adapter.Replace(replaceTarget, backupTarget, ignoreMetadataErrors: true);

            // Assert
            delete.Should().NotThrow();
            copy.Should().Throw<FileNotFoundException>();
            copyOverwrite.Should().Throw<FileNotFoundException>();
            move.Should().Throw<FileNotFoundException>();
            replace.Should().Throw<FileNotFoundException>();
            replaceIgnore.Should().Throw<FileNotFoundException>();
        }

        private sealed class UnsupportedInfo : FileSystemInfo
        {
            public override bool Exists => false;

            public override string Name => "unsupported";

            public override void Delete() { }
        }
    }
}
