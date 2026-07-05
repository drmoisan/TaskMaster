using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster;
using UtilitiesCS;
using UtilitiesCS.Interfaces;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Coverage expansion for AppToDoObjects initialization and state decisions that do not require
    /// Outlook automation or filesystem temporary files.
    /// </summary>
    [TestClass]
    public class AppToDoObjectsCoverageExpansionTests
    {
        [TestMethod]
        public void Constructor_WhenCreated_InitializesParentFilenamesAndDefaultState()
        {
            // Arrange
            var globals = CreateGlobals(new ConcurrentDictionary<string, string>());

            // Act
            var sut = new AppToDoObjects(globals);

            // Assert
            sut.Parent.Should().BeSameAs(globals);
            sut.ProjInfo_Filename.Should().NotBeNullOrWhiteSpace();
            sut.FnameIDList.Should().NotBeNullOrWhiteSpace();
            sut.FnameDictRemap.Should().NotBeNullOrWhiteSpace();
            sut.FindMatchingTag.Should().NotBeNull();
            sut.SelectFromList.Should().BeNull();
            sut.FlagChangeTrainingQueue.Should().BeNull();
        }

        [TestMethod]
        public void DelegatesAndQueue_WhenAssigned_ReturnAssignedValues()
        {
            // Arrange
            var sut = CreateSut(new ConcurrentDictionary<string, string>());
            var trainingQueue = new Mock<IFlagChangeTrainingQueue>().Object;

            // Act
            sut.FindMatchingTag = (_, _, _, _) => "matched-tag";
            sut.SelectFromList = options => new List<string>(options);
            sut.FlagChangeTrainingQueue = trainingQueue;

            // Assert
            sut.FindMatchingTag(["unused"], null, "subject", "folder").Should().Be("matched-tag");
            sut.SelectFromList(["alpha", "beta"]).Should().Equal("alpha", "beta");
            sut.FlagChangeTrainingQueue.Should().BeSameAs(trainingQueue);
        }

        [TestMethod]
        public void PythonStagingBackedProperties_WhenFolderMissing_ReturnNullWithoutFileAccess()
        {
            // Arrange
            var sut = CreateSut(new ConcurrentDictionary<string, string>());

            // Act
            var dictRemap = sut.DictRemap;
            var categoryFilters = sut.CategoryFilters;
            var prefixList = sut.PrefixList;
            var filteredFolderScraping = sut.FilteredFolderScraping;
            var folderRemap = sut.FolderRemap;
            var loadedPrefixList = sut.LoadPrefixList();
            var loadedFilteredFolderScraping = sut.LoadFilteredFolderScraping();
            var loadedFolderRemap = sut.LoadFolderRemap();

            // Assert
            dictRemap.Should().BeNull();
            categoryFilters.Should().BeNull();
            prefixList.Should().BeNull();
            filteredFolderScraping.Should().BeNull();
            folderRemap.Should().BeNull();
            loadedPrefixList.Should().BeNull();
            loadedFilteredFolderScraping.Should().BeNull();
            loadedFolderRemap.Should().BeNull();
        }

        [TestMethod]
        public void AppDataBackedProperties_WhenFolderMissing_ReturnNullWithoutOutlookAccess()
        {
            // Arrange
            var sut = CreateSut(new ConcurrentDictionary<string, string>());

            // Act
            var projInfo = sut.ProjInfo;
            var programInfo = sut.ProgramInfo;
            var idList = sut.IDList;

            // Assert
            projInfo.Should().BeNull();
            programInfo.Should().BeNull();
            idList.Should().BeNull();
        }

        private static AppToDoObjects CreateSut(
            ConcurrentDictionary<string, string> specialFolders
        ) => new(CreateGlobals(specialFolders));

        private static IApplicationGlobals CreateGlobals(
            ConcurrentDictionary<string, string> specialFolders
        )
        {
            var fileSystem = new StubFileSystemFolderPaths();
            foreach (var item in specialFolders)
            {
                fileSystem.SpecialFolders[item.Key] = item.Value;
            }

            return new StubApplicationGlobals(fileSystem, OlObjectsProxy.Create(() => null!));
        }
    }
}
