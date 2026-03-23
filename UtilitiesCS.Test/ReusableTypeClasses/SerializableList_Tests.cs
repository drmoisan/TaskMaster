using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SerializableList_Tests
    {
        private static readonly string RepoRoot = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..")
        );
        private const string ValidFixtureJson = "[5, 4, 6]";
        private const string InvalidJson = "not valid json {{ broken";

        [TestMethod]
        public void DefaultConstructorAndCoreListOperations_WorkLikeAList()
        {
            // Arrange
            var list = new SerializableList<string>();

            // Act
            list.Add("alpha");
            list.Insert(1, "gamma");
            list.Insert(1, "beta");
            list[2] = "delta";
            var removed = list.Remove("beta");
            var containsDelta = list.Contains("delta");
            var indexOfDelta = list.IndexOf("delta");

            // Assert
            list.Count.Should().Be(2);
            list.Should().Equal("alpha", "delta");
            removed.Should().BeTrue();
            containsDelta.Should().BeTrue();
            indexOfDelta.Should().Be(1);
            list.IsReadOnly.Should().BeFalse();
        }

        [TestMethod]
        public void IEnumerableConstructor_LoadsLazySequenceOnFirstUse()
        {
            // Arrange
            var list = new SerializableList<int>(Enumerable.Range(1, 4));

            // Act
            var count = list.Count;
            var values = list.ToList();

            // Assert
            count.Should().Be(4);
            values.Should().Equal(1, 2, 3, 4);
        }

        [TestMethod]
        public void CopyToRemoveAtClearAndFromList_UpdateCollectionState()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 1, 2, 3, 4 });
            var copied = new int[6];

            // Act
            list.CopyTo(copied, 1);
            list.RemoveAt(1);
            list.FromList(new List<int> { 9, 8 });
            var valuesAfterFromList = list.ToList();
            list.Clear();

            // Assert
            copied.Should().Equal(0, 1, 2, 3, 4, 0);
            valuesAfterFromList.Should().Equal(9, 8);
            list.Count.Should().Be(0);
        }

        [TestMethod]
        public void FindIndexOverloadsAndEnumeration_ReturnExpectedMatches()
        {
            // Arrange
            var list = new SerializableList<string>(
                new List<string> { "ant", "bear", "cat", "dog", "emu" }
            );

            // Act
            var firstThreeLetterIndex = list.FindIndex(value => value.Length == 3);
            var laterThreeLetterIndex = list.FindIndex(2, value => value.Length == 3);
            var rangedIndex = list.FindIndex(
                1,
                3,
                value => value.StartsWith("d", StringComparison.Ordinal)
            );
            var enumerated = list.ToArray();

            // Assert
            firstThreeLetterIndex.Should().Be(0);
            laterThreeLetterIndex.Should().Be(2);
            rangedIndex.Should().Be(3);
            enumerated.Should().Equal("ant", "bear", "cat", "dog", "emu");
        }

        [TestMethod]
        public void Add_RaisesPropertyChangedForAdd()
        {
            // Arrange
            var list = new SerializableList<string>();
            var raisedNames = new List<string>();
            list.PropertyChanged += (_, args) => raisedNames.Add(args.PropertyName);

            // Act
            list.Add("value");

            // Assert
            raisedNames
                .Should()
                .ContainSingle()
                .Which.Should()
                .Be(nameof(SerializableList<string>.Add));
        }

        [TestMethod]
        public void FilenameAndFolderpath_ComposeFilepath()
        {
            // Arrange
            var list = new SerializableList<string>();
            var folder = @"C:\Example";

            // Act
            list.Filename = "items.json";
            list.Folderpath = folder;

            // Assert
            list.Filepath.Should().Be(Path.Combine(folder, "items.json"));
        }

        [TestMethod]
        public void Folderpath_SetBeforeFilename_ComposesFilepathViaFilenameSetter()
        {
            // Arrange
            var list = new SerializableList<string>();
            var folder = @"C:\Example";

            // Act
            list.Folderpath = folder;
            list.Filename = "items.json";

            // Assert
            list.Filepath.Should().Be(Path.Combine(folder, "items.json"));
        }

        [TestMethod]
        public void Filepath_SetToExistingFolderWithoutExtension_ThrowsArgumentException()
        {
            // Arrange
            var list = new SerializableList<string>();
            var existingFolder = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(
                Path.DirectorySeparatorChar
            );

            // Act
            Action act = () => list.Filepath = existingFolder;

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Folder Path*");
        }

        [TestMethod]
        public void JsonRoundTrip_PreservesItems()
        {
            // Arrange
            var source = new SerializableList<int>(new List<int> { 2, 1, 3 })
            {
                Filename = "values.json",
                Folderpath = @"C:\Lists",
            };
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };

            // Act
            var json = JsonConvert.SerializeObject(source, settings);
            var roundTrip = JsonConvert.DeserializeObject<SerializableList<int>>(json, settings);

            // Assert
            roundTrip.Should().NotBeNull();
            roundTrip!.ToList().Should().Equal(2, 1, 3);
            roundTrip.Filename.Should().BeEmpty();
            roundTrip.Folderpath.Should().BeEmpty();
            roundTrip.Filepath.Should().BeEmpty();
        }

        [TestMethod]
        public void IndexerGetter_ReturnsExpectedItem()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 2, 1, 3 });

            // Act
            var value = list[1];

            // Assert
            value.Should().Be(1);
        }

        [TestMethod]
        public void Sort_OrdersValuesUsingComparableImplementation()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 4, 1, 3, 2 });

            // Act
            list.Sort();

            // Assert
            list.ToList().Should().Equal(1, 2, 3, 4);
        }

        [TestMethod]
        public void Filename_SetWithoutFolderpath_LeavesFilepathEmpty()
        {
            // Arrange
            var list = new SerializableList<string>();

            // Act
            list.Filename = "items.json";

            // Assert
            list.Filepath.Should().BeEmpty();
        }

        [TestMethod]
        public void Serialize_WithNoConfiguredPath_DoesNothing()
        {
            // Arrange
            var list = new SerializableList<string>(new List<string> { "alpha" });

            // Act
            list.Serialize();

            // Assert
            list.ToList().Should().Equal("alpha");
        }

        [TestMethod]
        public void Serialize_WithExplicitInvalidPath_UpdatesFilepath()
        {
            // Arrange
            var list = new SerializableList<string>(new List<string> { "alpha" });
            var invalidPath = CreateInvalidFilePath();
            var fileSystemMock = CreateFileSystemMock();

            // Act
            using var scope = new SerializableListDependencyScope<string>(fileSystemMock.Object);
            list.Serialize(invalidPath);

            // Assert
            list.Filepath.Should().Be(invalidPath);
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInvalidPath_IsSwallowedByProductionErrorHandling()
        {
            // Arrange
            var list = new SerializableList<string>(new List<string> { "alpha" });
            var invalidPath = CreateInvalidFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.CreateText(invalidPath))
                .Throws(new IOException("simulated create failure"));

            // Act
            using var scope = new SerializableListDependencyScope<string>(fileSystemMock.Object);
            Action act = () => list.SerializeThreadSafe(invalidPath);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInjectedWriter_SerializesJsonToInjectedStream()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 1, 2, 3 });
            using var stream = new MemoryStream();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.CreateText("ignored.json"))
                .Returns(() => new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true));

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.SerializeThreadSafe("ignored.json");
            stream.Position = 0;
            using var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true
            );
            var json = reader.ReadToEnd();

            // Assert
            json.Should().Contain("1");
            json.Should().Contain("2");
            json.Should().Contain("3");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task SerializeAsync_WithNoConfiguredPath_Completes()
        {
            // Arrange
            var list = new SerializableList<string>(new List<string> { "alpha" });

            // Act
            await list.SerializeAsync();

            // Assert
            list.ToList().Should().Equal("alpha");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task SerializeAsync_WithExplicitInvalidPath_CompletesAndUpdatesFilepath()
        {
            // Arrange
            var list = new SerializableList<string>(new List<string> { "alpha" });
            var invalidPath = CreateInvalidFilePath();
            var fileSystemMock = CreateFileSystemMock();

            // Act
            using var scope = new SerializableListDependencyScope<string>(fileSystemMock.Object);
            await list.SerializeAsync(invalidPath);

            // Assert
            list.Filepath.Should().Be(invalidPath);
        }

        [TestMethod]
        public void Deserialize_WithoutConfiguredPath_DoesNothing()
        {
            // Arrange
            var list = new SerializableList<string>(new List<string> { "alpha" });

            // Act
            list.Deserialize();
            list.Deserialize(askUserOnError: false);

            // Assert
            list.ToList().Should().Equal("alpha");
        }

        [TestMethod]
        public void Deserialize_WithConfiguredValidFile_LoadsItems()
        {
            // Arrange
            var list = new SerializableList<int>();
            list.Filepath = GetValidFixturePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(GetValidFixturePath()))
                .Returns(ValidFixtureJson);
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            list.Deserialize(askUserOnError: false);

            // Assert
            list.ToList().Should().Equal(5, 4, 6);
        }

        [TestMethod]
        public void Deserialize_WithInvalidPathAndPromptDisabled_CreatesEmptyList()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 1, 2, 3 });
            var invalidPath = CreateInvalidFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(invalidPath))
                .Throws(new FileNotFoundException("missing", invalidPath));

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.Deserialize(invalidPath, askUserOnError: false);

            // Assert
            list.Should().BeEmpty();
            list.Filepath.Should().Be(invalidPath);
        }

        [TestMethod]
        public void Deserialize_WithMissingFileAndPromptDisabled_CreatesEmptyList()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 1, 2, 3 });
            var missingPath = CreateMissingFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(missingPath))
                .Throws(new FileNotFoundException("missing", missingPath));

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.Deserialize(missingPath, askUserOnError: false);

            // Assert
            list.Should().BeEmpty();
            list.Filepath.Should().Be(missingPath);
        }

        [TestMethod]
        public void Deserialize_WithMissingFileAndPromptResponderNo_PreservesExistingList()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 1, 2, 3 });
            var missingPath = CreateMissingFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(missingPath))
                .Throws(new FileNotFoundException("missing", missingPath));
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);
            promptMock
                .Setup(prompt =>
                    prompt.Show(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<System.Windows.Forms.MessageBoxButtons>(),
                        It.IsAny<System.Windows.Forms.MessageBoxIcon>()
                    )
                )
                .Returns(System.Windows.Forms.DialogResult.No);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            Action act = () => list.Deserialize(missingPath, askUserOnError: true);

            // Assert
            act.Should().NotThrow();
            list.ToList().Should().Equal(1, 2, 3);
            list.Filepath.Should().Be(missingPath);
        }

        [TestMethod]
        public void Deserialize_WithMissingFileAndPromptResponderYes_CreatesEmptyList()
        {
            // Arrange
            var list = new SerializableList<int>(Enumerable.Empty<int>());
            var missingPath = CreateMissingFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(missingPath))
                .Throws(new FileNotFoundException("missing", missingPath));
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);
            promptMock
                .Setup(prompt =>
                    prompt.Show(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<System.Windows.Forms.MessageBoxButtons>(),
                        It.IsAny<System.Windows.Forms.MessageBoxIcon>()
                    )
                )
                .Returns(System.Windows.Forms.DialogResult.Yes);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            Action act = () => list.Deserialize(missingPath, askUserOnError: true);

            // Assert
            act.Should().NotThrow();
            list.Should().BeEmpty();
        }

        [TestMethod]
        public void Deserialize_WithMissingFileAndPromptResponderNoWithoutExistingList_Throws()
        {
            // Arrange
            var list = new SerializableList<int>(Enumerable.Empty<int>());
            var missingPath = CreateMissingFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(missingPath))
                .Throws(new FileNotFoundException("missing", missingPath));
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);
            promptMock
                .Setup(prompt =>
                    prompt.Show(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<System.Windows.Forms.MessageBoxButtons>(),
                        It.IsAny<System.Windows.Forms.MessageBoxIcon>()
                    )
                )
                .Returns(System.Windows.Forms.DialogResult.No);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            Action act = () => list.Deserialize(missingPath, askUserOnError: true);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Deserialize_WithMalformedFileAndPromptDisabled_CreatesEmptyList()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 1, 2, 3 });
            var invalidPath = GetInvalidFixturePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(invalidPath))
                .Returns(InvalidJson);

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.Deserialize(invalidPath, askUserOnError: false);

            // Assert
            list.Should().BeEmpty();
            list.Filepath.Should().Be(invalidPath);
        }

        [TestMethod]
        public void Deserialize_WithGenericErrorAndPromptResponderYes_CreatesEmptyList()
        {
            // Arrange
            var list = new SerializableList<int>(Enumerable.Empty<int>());
            var invalidPath = CreateInvalidFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(invalidPath))
                .Throws(new InvalidDataException("broken json"));
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);
            promptMock
                .Setup(prompt =>
                    prompt.Show(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<System.Windows.Forms.MessageBoxButtons>(),
                        It.IsAny<System.Windows.Forms.MessageBoxIcon>()
                    )
                )
                .Returns(System.Windows.Forms.DialogResult.Yes);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            Action act = () => list.Deserialize(invalidPath, askUserOnError: true);

            // Assert
            act.Should().NotThrow();
            list.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithExistingJsonFile_DeserializesItems()
        {
            // Arrange
            var fixturePath = GetValidFixturePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(fixturePath))
                .Returns(ValidFixtureJson);
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            var list = new SerializableList<int>(
                Path.GetFileName(fixturePath),
                Path.GetDirectoryName(fixturePath)
            );

            // Assert
            list.ToList().Should().Equal(5, 4, 6);
            list.Filepath.Should().Be(fixturePath);
        }

        [TestMethod]
        public void Constructor_WithBackupLoaderAndMissingPrimary_UsesBackupLoaderContents()
        {
            // Arrange
            var primaryPath = Path.Combine(WorkspaceRoot, "*missing-serializable-list.json");
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(primaryPath))
                .Throws(new FileNotFoundException("missing primary", primaryPath));

            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            var list = new SerializableList<int>(
                "*missing-serializable-list.json",
                WorkspaceRoot,
                _ => new List<int> { 8, 9 },
                Path.Combine(WorkspaceRoot, "backup.csv"),
                askUserOnError: false
            );

            // Assert
            list.ToList().Should().Equal(8, 9);
            list.Filepath.Should()
                .Be(Path.Combine(WorkspaceRoot, "*missing-serializable-list.json"));
        }

        [TestMethod]
        public void Deserialize_WithBackupLoaderAndExplicitBackupPath_UsesBackupLoaderContents()
        {
            // Arrange
            var list = new SerializableList<int>();
            var observedPath = string.Empty;
            var invalidPath = CreateInvalidFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(invalidPath))
                .Throws(new FileNotFoundException("missing primary", invalidPath));

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.Deserialize(
                invalidPath,
                path =>
                {
                    observedPath = path;
                    return new List<int> { 7, 8 };
                },
                askUserOnError: false
            );

            // Assert
            list.ToList().Should().Equal(7, 8);
            observedPath.Should().Be(Path.Combine(WorkspaceRoot, "*invalid-serializable-list.csv"));
        }

        [TestMethod]
        public void Deserialize_WithBackupLoaderAndStoredBackupPath_UsesStoredBackupFilepath()
        {
            // Arrange
            var backupFilepath = Path.Combine(WorkspaceRoot, "stored-backup.csv");
            var observedPath = string.Empty;
            var seedPath = Path.Combine(WorkspaceRoot, "*seed.json");
            var invalidPath = CreateInvalidFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(seedPath))
                .Throws(new FileNotFoundException("missing seed", seedPath));
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(invalidPath))
                .Throws(new FileNotFoundException("missing primary", invalidPath));

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            var list = new SerializableList<int>(
                "*seed.json",
                WorkspaceRoot,
                _ => new List<int> { 1 },
                backupFilepath,
                askUserOnError: false
            );
            list.Deserialize(
                invalidPath,
                path =>
                {
                    observedPath = path;
                    return new List<int> { 10, 11 };
                },
                askUserOnError: false
            );

            // Assert
            list.ToList().Should().Equal(10, 11);
            observedPath.Should().Be(backupFilepath);
        }

        [TestMethod]
        public void Deserialize_WithMissingFileAndBackupLoader_UsesBackupLoaderContents()
        {
            // Arrange
            var list = new SerializableList<int>();
            var observedPath = string.Empty;
            var missingPath = CreateMissingFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(missingPath))
                .Throws(new FileNotFoundException("missing", missingPath));

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.Deserialize(
                missingPath,
                path =>
                {
                    observedPath = path;
                    return new List<int> { 14, 15 };
                },
                askUserOnError: false
            );

            // Assert
            list.ToList().Should().Equal(14, 15);
            observedPath.Should().Be(Path.Combine(WorkspaceRoot, "missing-serializable-list.csv"));
        }

        [TestMethod]
        public void Deserialize_WithMalformedFileAndBackupLoader_UsesBackupLoaderContents()
        {
            // Arrange
            var list = new SerializableList<int>();
            var observedPath = string.Empty;
            var invalidPath = GetInvalidFixturePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(invalidPath))
                .Returns(InvalidJson);

            // Act
            using var scope = new SerializableListDependencyScope<int>(fileSystemMock.Object);
            list.Deserialize(
                invalidPath,
                path =>
                {
                    observedPath = path;
                    return new List<int> { 12, 13 };
                },
                askUserOnError: false
            );

            // Assert
            list.ToList().Should().Equal(12, 13);
            observedPath
                .Should()
                .Be(
                    Path.Combine(
                        WorkspaceRoot,
                        "UtilitiesCS.Test",
                        "TestData",
                        "serializable-list-invalid.csv"
                    )
                );
        }

        [TestMethod]
        public void Deserialize_WithBackupLoaderAndPromptResponderYes_LoadsBackupContents()
        {
            // Arrange
            var list = new SerializableList<int>(Enumerable.Empty<int>());
            var observedPath = string.Empty;
            var invalidPath = CreateInvalidFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(invalidPath))
                .Throws(new InvalidDataException("broken json"));
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);
            promptMock
                .Setup(prompt =>
                    prompt.Show(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<System.Windows.Forms.MessageBoxButtons>(),
                        It.IsAny<System.Windows.Forms.MessageBoxIcon>()
                    )
                )
                .Returns(System.Windows.Forms.DialogResult.Yes);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            Action act = () =>
                list.Deserialize(
                    invalidPath,
                    path =>
                    {
                        observedPath = path;
                        return new List<int> { 21, 34 };
                    },
                    askUserOnError: true
                );

            // Assert
            act.Should().NotThrow();
            list.ToList().Should().Equal(21, 34);
            observedPath.Should().Be(Path.Combine(WorkspaceRoot, "*invalid-serializable-list.csv"));
        }

        [TestMethod]
        public void Deserialize_WithBackupLoaderAndPromptResponderNoThenYes_CreatesEmptyList()
        {
            // Arrange
            var list = new SerializableList<int>(Enumerable.Empty<int>());
            var loaderWasCalled = false;
            var missingPath = CreateMissingFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(missingPath))
                .Throws(new FileNotFoundException("missing", missingPath));
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);
            promptMock
                .SetupSequence(prompt =>
                    prompt.Show(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<System.Windows.Forms.MessageBoxButtons>(),
                        It.IsAny<System.Windows.Forms.MessageBoxIcon>()
                    )
                )
                .Returns(System.Windows.Forms.DialogResult.No)
                .Returns(System.Windows.Forms.DialogResult.Yes);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            Action act = () =>
                list.Deserialize(
                    missingPath,
                    _ =>
                    {
                        loaderWasCalled = true;
                        return new List<int> { 99 };
                    },
                    askUserOnError: true
                );

            // Assert
            act.Should().NotThrow();
            loaderWasCalled.Should().BeFalse();
            list.Should().BeEmpty();
        }

        [TestMethod]
        public void Deserialize_WithBackupLoaderAndPromptResponderNoThenNo_Throws()
        {
            // Arrange
            var list = new SerializableList<int>(Enumerable.Empty<int>());
            var missingPath = CreateMissingFilePath();
            var fileSystemMock = CreateFileSystemMock();
            fileSystemMock
                .Setup(fileSystem => fileSystem.ReadAllText(missingPath))
                .Throws(new FileNotFoundException("missing", missingPath));
            var promptMock = new Mock<ISerializableListPrompt>(MockBehavior.Strict);
            promptMock
                .SetupSequence(prompt =>
                    prompt.Show(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<System.Windows.Forms.MessageBoxButtons>(),
                        It.IsAny<System.Windows.Forms.MessageBoxIcon>()
                    )
                )
                .Returns(System.Windows.Forms.DialogResult.No)
                .Returns(System.Windows.Forms.DialogResult.No);

            // Act
            using var scope = new SerializableListDependencyScope<int>(
                fileSystemMock.Object,
                promptMock.Object
            );
            Action act = () =>
                list.Deserialize(missingPath, _ => new List<int> { 99 }, askUserOnError: true);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        private static string WorkspaceRoot => Path.Combine(RepoRoot, "TaskMaster");

        private static string GetValidFixturePath()
        {
            return Path.Combine(
                WorkspaceRoot,
                "UtilitiesCS.Test",
                "TestData",
                "serializable-list-valid.json"
            );
        }

        private static string GetInvalidFixturePath()
        {
            return Path.Combine(
                WorkspaceRoot,
                "UtilitiesCS.Test",
                "TestData",
                "serializable-list-invalid.json"
            );
        }

        private static string CreateInvalidFilePath()
        {
            return Path.Combine(WorkspaceRoot, "*invalid-serializable-list.json");
        }

        private static string CreateMissingFilePath()
        {
            return Path.Combine(WorkspaceRoot, "missing-serializable-list.json");
        }

        private static Mock<ISerializableListFileSystem> CreateFileSystemMock()
        {
            var fileSystemMock = new Mock<ISerializableListFileSystem>(MockBehavior.Strict);
            fileSystemMock
                .Setup(fileSystem => fileSystem.CreateText(It.IsAny<string>()))
                .Returns(() => new StreamWriter(Stream.Null));
            return fileSystemMock;
        }

        private sealed class SerializableListDependencyScope<T> : IDisposable
            where T : IComparable<T>
        {
            private readonly ISerializableListFileSystem _originalFileSystem;
            private readonly ISerializableListPrompt _originalPrompt;

            public SerializableListDependencyScope(
                ISerializableListFileSystem fileSystem,
                ISerializableListPrompt prompt = null
            )
            {
                _originalFileSystem = SerializableList<T>.FileSystem;
                _originalPrompt = SerializableList<T>.Prompt;
                SerializableList<T>.FileSystem = fileSystem;
                if (prompt is not null)
                {
                    SerializableList<T>.Prompt = prompt;
                }
            }

            public void Dispose()
            {
                SerializableList<T>.FileSystem = _originalFileSystem;
                SerializableList<T>.Prompt = _originalPrompt;
            }
        }
    }
}
