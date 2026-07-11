using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;

namespace UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection
{
    /// <summary>
    /// Coverage for the <see cref="ConcurrentObservableCollection{T}"/> serialization/deserialize
    /// branches — the file constructors, the primary/backup/create-empty/prompt error paths, and the
    /// public <c>Deserialize</c> overloads — driven entirely through the injectable filesystem and
    /// prompt seams. No temp files are used. Fake paths point at a non-existent directory so the
    /// deferred (timer-scheduled) serialize is a harmless caught no-op.
    /// </summary>
    [TestClass]
    public class ConcurrentObservableCollectionSerialization_Tests
    {
        private const string Folder = @"C:\nonexistent-cocol-serialization-tests";
        private const string FileName = "collection.json";

        [TestMethod]
        public void ByteArrayConstructor_RunsDeserializeJson_WithoutThrowing()
        {
            var bytes = Encoding.UTF8.GetBytes("[1,2,3]");

            Action act = () => _ = new ConcurrentObservableCollection<int>(bytes);

            // The byte[] constructor exercises DeserializeJson(byte[]); it does not repopulate the
            // instance (behavior ported verbatim from the legacy ScoCollection byte[] ctor).
            act.Should().NotThrow();
        }

        [TestMethod]
        public void FileConstructor_WhenPrimaryMissing_AndPromptDisabled_CreatesEmptyCollection()
        {
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new FileNotFoundException());
            fs.Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(new MemoryStream()));

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    askUserOnError: false
                );

                sut.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void FileConstructor_WhenReadThrowsGenericError_AndPromptDisabled_CreatesEmpty()
        {
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>()))
                .Throws(new InvalidOperationException("boom"));
            fs.Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(new MemoryStream()));

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    askUserOnError: false
                );

                sut.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void FileConstructor_WhenDeserializesToNull_AndPromptDisabled_CreatesEmpty()
        {
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("null");
            fs.Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(new MemoryStream()));

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    askUserOnError: false
                );

                sut.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void FileConstructor_WhenPromptDeclines_ThrowsArgumentNullException()
        {
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new FileNotFoundException());
            var prompt = new Mock<IConcurrentObservableCollectionPrompt>();
            prompt.Setup(p => p.ShowError(It.IsAny<string>())).Returns(DialogResult.No);

            using (Seam<int>.Install(fs.Object, prompt.Object))
            {
                Action act = () =>
                    _ = new ConcurrentObservableCollection<int>(
                        FileName,
                        Folder,
                        askUserOnError: true
                    );

                act.Should().Throw<ArgumentNullException>();
                prompt.Verify(p => p.ShowError(It.IsAny<string>()), Times.AtLeastOnce);
            }
        }

        [TestMethod]
        public void FileConstructor_WhenPrimarySucceeds_LoadsItems()
        {
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("[10,20]");

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    askUserOnError: false
                );

                sut.Should().Equal(new[] { 10, 20 });
            }
        }

        [TestMethod]
        public void PublicDeserialize_Overload_ReplacesContentsFromDisk()
        {
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("[5,6,7]");

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int> { 99 };

                sut.Deserialize(FileName, Folder, askUserOnError: false);

                sut.Should().Equal(new[] { 5, 6, 7 });
            }
        }

        [TestMethod]
        public void BackupConstructor_WhenPrimaryMissingAndBackupExists_LoadsFromBackup()
        {
            const string backupPath = @"C:\nonexistent-cocol-serialization-tests\backup.csv";
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new FileNotFoundException());
            fs.Setup(f => f.Exists(backupPath)).Returns(true);
            fs.Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(new MemoryStream()));

            ConcurrentObservableCollection<int>.AltListLoader loader = _ => new List<int>
            {
                7,
                8,
                9,
            };

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    loader,
                    backupPath,
                    askUserOnError: false
                );

                sut.Should().Equal(new[] { 7, 8, 9 });
            }
        }

        [TestMethod]
        public void BackupConstructor_WhenBackupMissing_AndPromptDisabled_CreatesEmpty()
        {
            const string backupPath =
                @"C:\nonexistent-cocol-serialization-tests\missing-backup.csv";
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new FileNotFoundException());
            fs.Setup(f => f.Exists(backupPath)).Returns(false);
            fs.Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(new MemoryStream()));

            ConcurrentObservableCollection<int>.AltListLoader loader = _ => new List<int>();

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    loader,
                    backupPath,
                    askUserOnError: false
                );

                sut.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void BackupConstructor_WhenPrimarySucceeds_LoadsPrimary_WithoutBackup()
        {
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("[1,2]");

            ConcurrentObservableCollection<int>.AltListLoader loader = _ => new List<int> { 99 };

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    loader,
                    @"C:\nonexistent-cocol-serialization-tests\unused.csv",
                    askUserOnError: false
                );

                sut.Should().Equal(new[] { 1, 2 });
            }
        }

        [TestMethod]
        public void BackupConstructor_WhenPrimaryGenericErrorAndBackupExists_LoadsFromBackup()
        {
            const string backupPath = @"C:\nonexistent-cocol-serialization-tests\gen-backup.csv";
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>()))
                .Throws(new InvalidOperationException("corrupt"));
            fs.Setup(f => f.Exists(backupPath)).Returns(true);
            fs.Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(new MemoryStream()));

            ConcurrentObservableCollection<int>.AltListLoader loader = _ => new List<int> { 4, 5 };

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    loader,
                    backupPath,
                    askUserOnError: false
                );

                sut.Should().Equal(new[] { 4, 5 });
            }
        }

        [TestMethod]
        public void BackupConstructor_WhenBackupLoaderThrows_AndPromptDisabled_CreatesEmpty()
        {
            const string backupPath = @"C:\nonexistent-cocol-serialization-tests\throwing.csv";
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new FileNotFoundException());
            fs.Setup(f => f.Exists(backupPath)).Returns(true);
            fs.Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(new MemoryStream()));

            ConcurrentObservableCollection<int>.AltListLoader loader = _ =>
                throw new InvalidOperationException("backup load failed");

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(
                    FileName,
                    Folder,
                    loader,
                    backupPath,
                    askUserOnError: false
                );

                sut.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void PublicBackupDeserialize_Overload_LoadsFromBackup()
        {
            const string backupPath = @"C:\nonexistent-cocol-serialization-tests\pub-backup.csv";
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new FileNotFoundException());
            fs.Setup(f => f.Exists(backupPath)).Returns(true);
            fs.Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(new MemoryStream()));

            ConcurrentObservableCollection<int>.AltListLoader loader = _ => new List<int> { 1, 3 };

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>();

                sut.Deserialize(FileName, Folder, loader, backupPath, askUserOnError: false);

                sut.Should().Equal(new[] { 1, 3 });
            }
        }

        [TestMethod]
        public void DiskPathAccessors_And_SerializeAsyncWithConfiguredPath()
        {
            var fs = new Mock<IConcurrentObservableCollectionFileSystem>();
            fs.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("[1]");

            using (Seam<int>.Install(fs.Object, null))
            {
                var sut = new ConcurrentObservableCollection<int>(FileName, Folder);

                sut.FileName.Should().Be(FileName);
                sut.FolderPath.Should().Be(Folder);
                sut.FilePath.Should().Contain(FileName);
            }
        }

        [TestMethod]
        public async Task SerializeAsync_WithConfiguredFilePath_SchedulesWithoutThrowing()
        {
            var sut = new ConcurrentObservableCollection<int> { 1 };
            sut.FilePath = @"C:\nonexistent-cocol-serialization-tests\out.json";

            await sut.Awaiting(s => s.SerializeAsync()).Should().NotThrowAsync();
        }

        /// <summary>Installs and restores the static FS/Prompt seams for independent tests.</summary>
        private sealed class Seam<T> : IDisposable
        {
            private readonly IConcurrentObservableCollectionFileSystem _fs;
            private readonly IConcurrentObservableCollectionPrompt _prompt;

            private Seam()
            {
                _fs = ConcurrentObservableCollection<T>.FileSystem;
                _prompt = ConcurrentObservableCollection<T>.Prompt;
            }

            public static Seam<T> Install(
                IConcurrentObservableCollectionFileSystem fs,
                IConcurrentObservableCollectionPrompt prompt
            )
            {
                var scope = new Seam<T>();
                ConcurrentObservableCollection<T>.FileSystem = fs;
                if (prompt is not null)
                {
                    ConcurrentObservableCollection<T>.Prompt = prompt;
                }
                return scope;
            }

            public void Dispose()
            {
                ConcurrentObservableCollection<T>.FileSystem = _fs;
                ConcurrentObservableCollection<T>.Prompt = _prompt;
            }
        }
    }
}
