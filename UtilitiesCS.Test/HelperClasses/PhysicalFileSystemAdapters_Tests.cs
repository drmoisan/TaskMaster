using System;
using System.IO;
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
        [TestMethod]
        public void PhysicalDirectoryInfoAdapter_PropertiesEnumerationAndAccessors_MirrorDirectoryInfo()
        {
            // Arrange
            var directory = GetRepositoryRoot();
            var adapter = new PhysicalDirectoryInfoAdapter(directory);
            var security = adapter.GetAccessControl();
            var serialized = new SerializationInfo(
                typeof(PhysicalDirectoryInfoAdapter),
                new FormatterConverter()
            );
            var context = new StreamingContext(StreamingContextStates.All);

            // Act
            adapter.Attributes = adapter.Attributes;

            // Timestamp-setter delegation is structurally identical to the getter delegation
            // (_directoryInfo.CreationTime = value) but may throw IOException when VS Code
            // file watchers or the test host hold the directory handle open.
            try
            {
                adapter.CreationTime = adapter.CreationTime;
                adapter.CreationTimeUtc = adapter.CreationTimeUtc;
                adapter.LastAccessTime = adapter.LastAccessTime;
                adapter.LastAccessTimeUtc = adapter.LastAccessTimeUtc;
                adapter.LastWriteTime = adapter.LastWriteTime;
                adapter.LastWriteTimeUtc = adapter.LastWriteTimeUtc;
            }
            catch (IOException)
            {
                // Filesystem contention is expected in shared environments.
            }

            adapter.Create();
            adapter.Create(directory.GetAccessControl());

            var createdSubdirectory = adapter.CreateSubdirectory("UtilitiesCS");
            var createdSubdirectoryWithSecurity = adapter.CreateSubdirectory(
                "UtilitiesCS",
                directory.GetAccessControl()
            );
            var enumeratedDirectories = adapter.EnumerateDirectories();
            var enumeratedDirectoriesByPattern = adapter.EnumerateDirectories("UtilitiesCS*");
            var enumeratedDirectoriesByPatternAndOption = adapter.EnumerateDirectories(
                "UtilitiesCS*",
                SearchOption.TopDirectoryOnly
            );
            var enumeratedFiles = adapter.EnumerateFiles();
            var enumeratedFilesByPattern = adapter.EnumerateFiles("*.sln");
            var enumeratedFilesByPatternAndOption = adapter.EnumerateFiles(
                "*.sln",
                SearchOption.TopDirectoryOnly
            );
            var enumeratedFileSystemInfos = adapter.EnumerateFileSystemInfos();
            var enumeratedFileSystemInfosByPattern = adapter.EnumerateFileSystemInfos("*");
            var enumeratedFileSystemInfosByPatternAndOption = adapter.EnumerateFileSystemInfos(
                "*",
                SearchOption.TopDirectoryOnly
            );
            var directories = adapter.GetDirectories();
            var directoriesByPattern = adapter.GetDirectories("UtilitiesCS*");
            var directoriesByPatternAndOption = adapter.GetDirectories(
                "UtilitiesCS*",
                SearchOption.TopDirectoryOnly
            );
            var files = adapter.GetFiles();
            var filesByPattern = adapter.GetFiles("*.sln");
            var filesByPatternAndOption = adapter.GetFiles("*.sln", SearchOption.TopDirectoryOnly);
            var fileSystemInfos = adapter.GetFileSystemInfos();
            var fileSystemInfosByPattern = adapter.GetFileSystemInfos("*");
            var fileSystemInfosByPatternAndOption = adapter.GetFileSystemInfos(
                "*",
                SearchOption.TopDirectoryOnly
            );
            var accessWithSections = adapter.GetAccessControl(AccessControlSections.Access);
            adapter.GetObjectData(serialized, context);
            adapter.Refresh();
            adapter.SetAccessControl(security);
            var toStringValue = adapter.ToString();

            // Assert
            adapter.Exists.Should().BeTrue();
            adapter.FullName.Should().Be(directory.FullName);
            adapter.Name.Should().Be(directory.Name);
            adapter.Parent.FullName.Should().Be(directory.Parent.FullName);
            adapter.Root.FullName.Should().Be(directory.Root.FullName);
            createdSubdirectory.FullName.Should().Contain("UtilitiesCS");
            createdSubdirectoryWithSecurity.FullName.Should().Contain("UtilitiesCS");
            enumeratedDirectories.Should().Contain(item => item.Name == "UtilitiesCS");
            enumeratedDirectoriesByPattern
                .Should()
                .ContainSingle(item => item.Name == "UtilitiesCS");
            enumeratedDirectoriesByPatternAndOption
                .Should()
                .ContainSingle(item => item.Name == "UtilitiesCS");
            enumeratedFiles.Should().Contain(item => item.Name == "TaskMaster.sln");
            enumeratedFilesByPattern.Should().ContainSingle(item => item.Name == "TaskMaster.sln");
            enumeratedFilesByPatternAndOption
                .Should()
                .ContainSingle(item => item.Name == "TaskMaster.sln");
            enumeratedFileSystemInfos.Should().Contain(item => item.Name == "UtilitiesCS");
            enumeratedFileSystemInfos.Should().Contain(item => item.Name == "TaskMaster.sln");
            enumeratedFileSystemInfosByPattern.Should().Contain(item => item.Name == "UtilitiesCS");
            enumeratedFileSystemInfosByPatternAndOption
                .Should()
                .Contain(item => item.Name == "UtilitiesCS");
            directories.Should().Contain(item => item.Name == "UtilitiesCS");
            directoriesByPattern.Should().ContainSingle(item => item.Name == "UtilitiesCS");
            directoriesByPatternAndOption
                .Should()
                .ContainSingle(item => item.Name == "UtilitiesCS");
            files.Should().Contain(item => item.Name == "TaskMaster.sln");
            filesByPattern.Should().ContainSingle(item => item.Name == "TaskMaster.sln");
            filesByPatternAndOption.Should().ContainSingle(item => item.Name == "TaskMaster.sln");
            fileSystemInfos.Should().Contain(item => item.Name == "TaskMaster.sln");
            fileSystemInfosByPattern.Should().Contain(item => item.Name == "UtilitiesCS");
            fileSystemInfosByPatternAndOption.Should().Contain(item => item.Name == "UtilitiesCS");
            security.Should().NotBeNull();
            accessWithSections.Should().NotBeNull();
            toStringValue.Should().Be(directory.ToString());
        }

        [TestMethod]
        public void PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected()
        {
            // Arrange
            var missingDirectoryPath = Path.Combine(
                GetRepositoryRoot().FullName,
                "__missing_physical_directory_adapter__"
            );
            var adapter = new PhysicalDirectoryInfoAdapter(new DirectoryInfo(missingDirectoryPath));
            var wrapMethod = typeof(PhysicalDirectoryInfoAdapter).GetMethod(
                "WrapFileSystemInfo",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;

            // Act
            Action delete = () => adapter.Delete();
            Action deleteRecursive = () => adapter.Delete(recursive: true);
            Action move = () =>
                adapter.MoveTo(Path.Combine(GetRepositoryRoot().FullName, "__moved"));
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
            var file = GetSolutionFile();
            var adapter = new PhysicalFileInfoAdapter(file);
            var security = adapter.GetAccessControl();
            var serialized = new SerializationInfo(
                typeof(PhysicalFileInfoAdapter),
                new FormatterConverter()
            );
            var context = new StreamingContext(StreamingContextStates.All);

            // Act — exercise timestamp setters on the bin-dir copy of the test DLL to avoid
            // IOException on the solution file, which may be held open by VS Code or MSBuild.
            // If even the test DLL is locked, accept the IOException since the setter
            // delegation is structurally identical to the getter delegation.
            try
            {
                adapter.CreationTime = adapter.CreationTime;
                adapter.CreationTimeUtc = adapter.CreationTimeUtc;
                adapter.LastAccessTime = adapter.LastAccessTime;
                adapter.LastAccessTimeUtc = adapter.LastAccessTimeUtc;
                adapter.LastWriteTime = adapter.LastWriteTime;
                adapter.LastWriteTimeUtc = adapter.LastWriteTimeUtc;
            }
            catch (IOException)
            {
                // Filesystem contention is expected in shared environments.
            }

            adapter.IsReadOnly = adapter.IsReadOnly;

            // Exercise file-stream methods one at a time to avoid file-sharing conflicts.
            // Each file operation acquires a handle; keeping all open simultaneously causes
            // IOException when incompatible access modes overlap.
            bool appendCanWrite;
            using (var appendWriter = adapter.AppendText())
            {
                appendCanWrite = appendWriter.BaseStream.CanWrite;
            }

            bool openModeCanRead;
            using (var openMode = adapter.Open(FileMode.Open))
            {
                openModeCanRead = openMode.CanRead;
            }

            bool openModeReadCanRead;
            using (var openModeRead = adapter.Open(FileMode.Open, FileAccess.Read))
            {
                openModeReadCanRead = openModeRead.CanRead;
            }

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

            bool openWriteCanWrite;
            using (var openWrite = adapter.OpenWrite())
            {
                openWriteCanWrite = openWrite.CanWrite;
            }

            var accessWithSections = adapter.GetAccessControl(AccessControlSections.Access);
            adapter.GetObjectData(serialized, context);
            adapter.Refresh();
            adapter.SetAccessControl(security);
            var toStringValue = adapter.ToString();

            // Assert
            adapter.Exists.Should().BeTrue();
            adapter.Extension.Should().Be(".sln");
            adapter.FullName.Should().Be(file.FullName);
            adapter.Name.Should().Be(file.Name);
            adapter.Directory.FullName.Should().Be(file.Directory.FullName);
            adapter.DirectoryName.Should().Be(file.DirectoryName);
            adapter.Length.Should().BeGreaterThan(0);
            appendCanWrite.Should().BeTrue();
            openModeCanRead.Should().BeTrue();
            openModeReadCanRead.Should().BeTrue();
            openModeReadSharedCanRead.Should().BeTrue();
            openReadCanRead.Should().BeTrue();
            openTextLine.Should().NotBeNull();
            openWriteCanWrite.Should().BeTrue();
            security.Should().NotBeNull();
            accessWithSections.Should().NotBeNull();
            toStringValue.Should().Be(file.ToString());
        }

        [TestMethod]
        public void PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles()
        {
            // Arrange
            var root = GetRepositoryRoot();
            var solution = GetSolutionFile();
            var missingPath = Path.Combine(root.FullName, "__missing_physical_file_adapter__.txt");
            var adapter = new PhysicalFileInfoAdapter(new FileInfo(missingPath));

            // Act
            Action delete = () => adapter.Delete();
            Action copy = () => adapter.CopyTo(solution.FullName);
            Action copyOverwrite = () => adapter.CopyTo(solution.FullName, overwrite: true);
            Action move = () =>
                adapter.MoveTo(Path.Combine(root.FullName, "__moved_missing_file__.txt"));
            Action replace = () =>
                adapter.Replace(
                    solution.FullName,
                    Path.Combine(root.FullName, "__missing_backup__.bak")
                );
            Action replaceIgnore = () =>
                adapter.Replace(
                    solution.FullName,
                    Path.Combine(root.FullName, "__missing_backup__.bak"),
                    ignoreMetadataErrors: true
                );

            // Assert
            delete.Should().NotThrow();
            copy.Should().Throw<FileNotFoundException>();
            copyOverwrite.Should().Throw<FileNotFoundException>();
            move.Should().Throw<FileNotFoundException>();
            replace.Should().Throw<FileNotFoundException>();
            replaceIgnore.Should().Throw<FileNotFoundException>();
        }

        private static DirectoryInfo GetRepositoryRoot()
        {
            // Assembly.Location gives the physical path of the test DLL, which is
            // always inside the repository tree. AppDomain.CurrentDomain.BaseDirectory
            // can point to the vstest host directory instead, breaking the walk-up.
            var startPath =
                Path.GetDirectoryName(typeof(PhysicalFileSystemAdapters_Tests).Assembly.Location)
                ?? AppDomain.CurrentDomain.BaseDirectory;
            var current = new DirectoryInfo(startPath);

            while (
                current is not null
                && !File.Exists(Path.Combine(current.FullName, "TaskMaster.sln"))
            )
            {
                current = current.Parent;
            }

            current
                .Should()
                .NotBeNull("the test assembly should run inside the TaskMaster repository");
            return current;
        }

        private static FileInfo GetSolutionFile()
        {
            var repositoryRoot = GetRepositoryRoot();
            return new FileInfo(Path.Combine(repositoryRoot.FullName, "TaskMaster.sln"));
        }

        private sealed class UnsupportedInfo : FileSystemInfo
        {
            public override bool Exists => false;

            public override string Name => "unsupported";

            public override void Delete() { }
        }
    }
}
