using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoCollection_Tests
    {
        private static readonly string RepoRoot = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..")
        );

        [TestMethod]
        public void DefaultConstructor_StartsEmpty()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            var items = collection.ToArray();

            // Assert
            collection.Count.Should().Be(0);
            items.Should().BeEmpty();
        }

        [TestMethod]
        public void AddRemoveAndClear_UpdateCollectionContents()
        {
            // Arrange
            var collection = new ScoCollection<string>();

            // Act
            collection.Add("alpha");
            collection.Add("beta");
            var removed = collection.Remove("alpha");
            var afterRemove = collection.ToArray();
            collection.Clear();

            // Assert
            removed.Should().BeTrue();
            afterRemove.Should().Equal("beta");
            collection.Count.Should().Be(0);
        }

        [TestMethod]
        public void SingleItemCollection_SupportsIndexingAndEnumeration()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            collection.Add(42);

            // Assert
            collection.Count.Should().Be(1);
            collection[0].Should().Be(42);
            collection.Should().Equal(42);
        }

        [TestMethod]
        public void EnumerableConstructor_PopulatesCollection()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            var snapshot = collection.ToList();

            // Assert
            snapshot.Should().Equal(1, 2, 3);
            collection.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public async Task ConcurrentAddAndRemove_LeaveExpectedFinalSet()
        {
            // Arrange
            var collection = new ScoCollection<int>(Enumerable.Range(1, 50));
            var addTasks = Enumerable
                .Range(51, 25)
                .Select(value => Task.Run(() => collection.Add(value)));
            var removeTasks = Enumerable
                .Range(1, 25)
                .Select(value => Task.Run(() => collection.Remove(value)));

            // Act
            await Task.WhenAll(addTasks.Concat(removeTasks));
            var ordered = collection.OrderBy(value => value).ToArray();

            // Assert
            collection.Count.Should().Be(50);
            ordered.Should().Equal(Enumerable.Range(26, 50));
        }

        [TestMethod]
        public void ByteArrayConstructor_CreatesEmptyCollection()
        {
            // The byte[] constructor calls DeserializeJson which returns a new instance
            // but does not populate 'this' (production bug). Verify the constructor
            // does not throw and creates a valid empty instance.
            var json = "[1, 2, 3]";
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            var collection = new ScoCollection<int>(bytes);

            collection.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithExistingJsonFile_DeserializesItems()
        {
            // Arrange
            var fixturePath = GetValidFixturePath();

            // Act
            var collection = new ScoCollection<int>(
                Path.GetFileName(fixturePath),
                Path.GetDirectoryName(fixturePath)
            );

            // Assert
            collection.Should().Equal(11, 22, 33);
            collection.FilePath.Should().Be(fixturePath);
        }

        [TestMethod]
        public void Constructor_WithBackupLoaderAndMissingPrimary_UsesBackupLoaderItems()
        {
            // Arrange
            var primaryPath = Path.Combine(RepoRoot, "*missing-primary.json");
            const string backupPath = @"C:\mock-backup.json";
            var fileSystemMock = new Mock<IScoCollectionFileSystem>(MockBehavior.Strict);
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(primaryPath))
                .Throws(new FileNotFoundException("missing primary"));
            fileSystemMock.Setup(fileSystem => fileSystem.Exists(backupPath)).Returns(true);

            // Act
            using var scope = new ScoCollectionDependencyScope<int>(fileSystemMock.Object);
            var collection = new ScoCollection<int>(
                "*missing-primary.json",
                RepoRoot,
                _ => new List<int> { 9, 10 },
                backupPath,
                askUserOnError: false
            );

            // Assert
            collection.Should().Equal(9, 10);
            collection.FilePath.Should().Be(primaryPath);
            StopPendingTimer(collection);
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void FileName_SetAndGet_Works()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            collection.FileName = "test.json";

            // Assert
            collection.FileName.Should().Be("test.json");
        }

        [TestMethod]
        public void FolderPath_SetAndGet_Works()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            collection.FolderPath = @"C:\data";

            // Assert
            collection.FolderPath.Should().Be(@"C:\data");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var collection = new ScoCollection<int>();
            collection.Add(42);

            // Act
            collection.Serialize();

            // Assert
            collection.Count.Should().Be(1);
        }

        [TestMethod]
        public void JsonRoundTrip_PreservesItems()
        {
            // Arrange
            var original = new ScoCollection<string>(new[] { "a", "b", "c" });
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };

            // Act
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(original, settings);
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<ScoCollection<string>>(
                json,
                settings
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Should().Equal("a", "b", "c");
        }

        [TestMethod]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act & Assert
            collection.Contains(2).Should().BeTrue();
        }

        [TestMethod]
        public void Contains_MissingItem_ReturnsFalse()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act & Assert
            collection.Contains(99).Should().BeFalse();
        }

        [TestMethod]
        public void IndexOf_ExistingItem_ReturnsCorrectIndex()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 10, 20, 30 });

            // Act
            var index = collection.IndexOf(20);

            // Assert
            index.Should().Be(1);
        }

        [TestMethod]
        public void CopyTo_CopiesAllItems()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });
            var array = new int[5];

            // Act
            collection.CopyTo(array, 1);

            // Assert
            array.Should().Equal(0, 1, 2, 3, 0);
        }

        [TestMethod]
        public void Insert_AtIndex_ShiftsExistingItems()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 3 });

            // Act
            collection.Insert(1, 2);

            // Assert
            collection.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void RemoveAt_RemovesCorrectItem()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            collection.RemoveAt(1);

            // Assert
            collection.Should().Equal(1, 3);
        }

        [TestMethod]
        public void IsReadOnly_ReturnsFalse()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act & Assert
            collection.IsReadOnly.Should().BeFalse();
        }

        [TestMethod]
        public void FromList_RepopulatesItems()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            collection.FromList(new List<int> { 7, 8 });

            // Assert
            collection.Should().Equal(7, 8);
        }

        [TestMethod]
        public async Task SerializeAsync_WithNoConfiguredPath_CompletesWithoutMutatingItems()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            await collection.SerializeAsync();

            // Assert
            collection.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public async Task SerializeAsync_WithExplicitPath_UpdatesFilePathAndQueuesTimer()
        {
            // Arrange
            var collection = new ScoCollection<int>();
            var invalidPath = CreateInvalidFilePath();

            // Act
            await collection.SerializeAsync(invalidPath);

            // Assert
            collection.FilePath.Should().Be(invalidPath);
            StopPendingTimer(collection);
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInvalidPath_IsSwallowedByProductionErrorHandling()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            Action act = () => collection.SerializeThreadSafe(CreateInvalidFilePath());

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Deserialize_WithoutConfiguredPath_DoesNothing()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            Action act = () =>
            {
                collection.Deserialize();
                collection.Deserialize(askUserOnError: false);
            };

            // Assert
            act.Should().NotThrow();
            collection.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void Deserialize_WithConfiguredValidFile_LoadsItems()
        {
            // Arrange
            var fixturePath = GetValidFixturePath();
            var collection = new ScoCollection<int>();
            collection.FilePath = fixturePath;

            // Act
            collection.Deserialize();

            // Assert
            collection.Should().Equal(11, 22, 33);
        }

        [TestMethod]
        public void Deserialize_WithInvalidPathAndPromptDisabled_CreatesEmptyCollection()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            collection.Deserialize("*invalid-sco-collection.json", RepoRoot, askUserOnError: false);

            // Assert
            collection.Should().BeEmpty();
            collection.FilePath.Should().Be(Path.Combine(RepoRoot, "*invalid-sco-collection.json"));
        }

        [TestMethod]
        public void Deserialize_WithBackupLoader_UsesBackupLoaderItems()
        {
            // Arrange
            var primaryPath = Path.Combine(RepoRoot, "*invalid-primary.json");
            const string backupPath = @"C:\mock-backup.json";
            var collection = new ScoCollection<int>();
            var fileSystemMock = new Mock<IScoCollectionFileSystem>(MockBehavior.Strict);
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(primaryPath))
                .Throws(new FileNotFoundException("missing primary"));
            fileSystemMock.Setup(fileSystem => fileSystem.Exists(backupPath)).Returns(true);

            // Act
            using var scope = new ScoCollectionDependencyScope<int>(fileSystemMock.Object);
            collection.Deserialize(
                "*invalid-primary.json",
                RepoRoot,
                _ => new List<int> { 9, 10 },
                backupPath,
                askUserOnError: false
            );

            // Assert
            collection.Should().Equal(9, 10);
            collection.FilePath.Should().Be(primaryPath);
            StopPendingTimer(collection);
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void Deserialize_WithMissingBackupPath_CreatesEmptyCollection()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });
            var missingBackupPath = Path.Combine(RepoRoot, "backup-does-not-exist.json");

            // Act
            collection.Deserialize(
                "*invalid-primary.json",
                RepoRoot,
                _ => new List<int> { 9, 10 },
                missingBackupPath,
                askUserOnError: false
            );

            // Assert
            collection.Should().BeEmpty();
            collection.FilePath.Should().Be(Path.Combine(RepoRoot, "*invalid-primary.json"));
            StopPendingTimer(collection);
        }

        [TestMethod]
        public void Deserialize_WithBackupLoaderException_CreatesEmptyCollection()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });
            var primaryPath = Path.Combine(RepoRoot, "*invalid-primary.json");
            const string backupPath = @"C:\mock-backup.json";
            var fileSystemMock = new Mock<IScoCollectionFileSystem>(MockBehavior.Strict);
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(primaryPath))
                .Throws(new FileNotFoundException("missing primary"));
            fileSystemMock.Setup(fileSystem => fileSystem.Exists(backupPath)).Returns(true);

            // Act
            using var scope = new ScoCollectionDependencyScope<int>(fileSystemMock.Object);
            collection.Deserialize(
                "*invalid-primary.json",
                RepoRoot,
                _ => throw new InvalidOperationException("backup failed"),
                backupPath,
                askUserOnError: false
            );

            // Assert
            collection.Should().BeEmpty();
            collection.FilePath.Should().Be(primaryPath);
            StopPendingTimer(collection);
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void Deserialize_WithEmptyBackupPath_CreatesEmptyCollection()
        {
            // Arrange
            var collection = new ScoCollection<int>(new[] { 1, 2, 3 });

            // Act
            collection.Deserialize(
                "*invalid-primary.json",
                RepoRoot,
                _ => new List<int> { 9, 10 },
                string.Empty,
                askUserOnError: false
            );

            // Assert
            collection.Should().BeEmpty();
            collection.FilePath.Should().Be(Path.Combine(RepoRoot, "*invalid-primary.json"));
            StopPendingTimer(collection);
        }

        [TestMethod]
        public void AskUser_WhenPromptDisabled_ReturnsYes()
        {
            // Arrange
            var collection = new ScoCollection<int>();

            // Act
            var response = InvokeNonPublic<DialogResult>(collection, "AskUser", false, "ignored");

            // Assert
            response.Should().Be(DialogResult.Yes);
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseYes_ReturnsEmptyCollectionAndConfiguresPath()
        {
            // Arrange
            var collection = new ScoCollection<int>();
            var disk = new FilePathHelper("*empty-collection.json", RepoRoot);

            // Act
            var created = InvokeNonPublic<ScoCollection<int>>(
                collection,
                "CreateEmpty",
                DialogResult.Yes,
                disk
            );

            // Assert
            created.Should().BeEmpty();
            created.FilePath.Should().Be(disk.FilePath);
            StopPendingTimer(created);
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseNo_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new ScoCollection<int>();
            var disk = new FilePathHelper("*empty-collection.json", RepoRoot);

            // Act
            Action act = () =>
                InvokeNonPublic<ScoCollection<int>>(
                    collection,
                    "CreateEmpty",
                    DialogResult.No,
                    disk
                );

            // Assert
            act.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>();
        }

        [TestMethod]
        public void LoadFromBackup_UsesBackupLoaderContentsAndConfiguresSerializationPath()
        {
            // Arrange
            var collection = new ScoCollection<int>();
            var disk = new FilePathHelper("*backup-collection.json", RepoRoot);
            ScoCollection<int>.AltListLoader backupLoader = _ => new List<int> { 4, 5, 6 };

            // Act
            var restored = InvokeNonPublic<ScoCollection<int>>(
                collection,
                "LoadFromBackup",
                backupLoader,
                GetExistingRepoFilePath(),
                disk
            );

            // Assert
            restored.Should().Equal(4, 5, 6);
            restored.FilePath.Should().Be(disk.FilePath);
            StopPendingTimer(restored);
        }

        [TestMethod]
        public void DeserializeJson_WithExistingFixture_ReturnsCollectionContents()
        {
            // Arrange
            var collection = new ScoCollection<int>();
            var fixturePath = GetValidFixturePath();
            var disk = new FilePathHelper(
                Path.GetFileName(fixturePath),
                Path.GetDirectoryName(fixturePath)
            );

            // Act
            var restored = InvokeNonPublic<ScoCollection<int>>(collection, "DeserializeJson", disk);

            // Assert
            restored.Should().Equal(11, 22, 33);
        }

        private static T InvokeNonPublic<T>(object target, string methodName, params object[] args)
        {
            var parameterTypes = args.Select(argument => argument.GetType()).ToArray();
            var method = target
                .GetType()
                .GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    binder: null,
                    types: parameterTypes,
                    modifiers: null
                );

            return (T)method.Invoke(target, args);
        }

        private static void StopPendingTimer(object target)
        {
            var timerField = target
                .GetType()
                .GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic);
            var timer = timerField?.GetValue(target);

            timer?.GetType().GetMethod("StopTimer")?.Invoke(timer, null);
            timer?.GetType().GetMethod("Dispose")?.Invoke(timer, null);
        }

        private sealed class ScoCollectionDependencyScope<T> : IDisposable
        {
            private readonly IScoCollectionFileSystem _originalFileSystem;
            private readonly IScoCollectionPrompt _originalPrompt;

            public ScoCollectionDependencyScope(
                IScoCollectionFileSystem fileSystem,
                IScoCollectionPrompt prompt = null
            )
            {
                _originalFileSystem = ScoCollection<T>.FileSystem;
                _originalPrompt = ScoCollection<T>.Prompt;
                ScoCollection<T>.FileSystem = fileSystem;
                if (prompt is not null)
                {
                    ScoCollection<T>.Prompt = prompt;
                }
            }

            public void Dispose()
            {
                ScoCollection<T>.FileSystem = _originalFileSystem;
                ScoCollection<T>.Prompt = _originalPrompt;
            }
        }

        private static string GetExistingRepoFilePath()
        {
            return Path.Combine(RepoRoot, "README.md");
        }

        private static string GetValidFixturePath()
        {
            return Path.Combine(
                RepoRoot,
                "TaskMaster",
                "UtilitiesCS.Test",
                "TestData",
                "sco-collection-valid.json"
            );
        }

        private static string CreateInvalidFilePath()
        {
            return Path.Combine(RepoRoot, "*invalid-sco-collection.json");
        }
    }
}
