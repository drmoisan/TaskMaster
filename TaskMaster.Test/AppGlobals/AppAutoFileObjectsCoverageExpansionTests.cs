using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Coverage expansion for AppAutoFileObjects initialization and property decision paths that can
    /// run without Outlook automation or filesystem temporary files.
    /// </summary>
    [TestClass]
    public class AppAutoFileObjectsCoverageExpansionTests
    {
        [TestMethod]
        public void Constructor_WhenCreated_InitializesDefaultStateAndCancelToken()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());

            // Act
            var firstToken = sut.CancelToken;
            var secondToken = sut.CancelToken;

            // Assert
            sut.SuggestionFilesLoaded.Should().BeFalse();
            sut.MaximizeQuickFileWindow.Should().BeNull();
            sut.FolderPredictor.Should().BeNull();
            firstToken.Should().Be(sut.CancelSource.Token);
            secondToken.Should().Be(firstToken);
        }

        [TestMethod]
        public void PropertyDecisions_WhenAssigned_RetainProvidedValues()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());
            var invoked = false;
            var predictor = new Mock<IFolderPredictor>().Object;

            // Act
            sut.SuggestionFilesLoaded = true;
            sut.MaximizeQuickFileWindow = () => invoked = true;
            sut.FolderPredictor = predictor;
            sut.MaximizeQuickFileWindow();

            // Assert
            sut.SuggestionFilesLoaded.Should().BeTrue();
            invoked.Should().BeTrue();
            sut.FolderPredictor.Should().BeSameAs(predictor);
        }

        [TestMethod]
        public void FileBackedProperties_WhenPythonStagingMissing_ReturnNullWithoutFilesystemAccess()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());

            // Act
            var movedMails = sut.MovedMails;
            var ctfMapPropertyValue = sut.CtfMap;
            var loadedCtfMap = sut.LoadCtfMap();
            var commonWords = sut.CommonWords;

            // Assert
            movedMails.Should().BeNull();
            ctfMapPropertyValue.Should().BeNull();
            loadedCtfMap.Should().BeNull();
            commonWords.Should().BeNull();
        }

        [TestMethod]
        public void FileBackedProperties_WhenOnlyFlowFolderExists_StillSkipPythonStagingLoads()
        {
            // Arrange
            var specialFolders = new ConcurrentDictionary<string, string>();
            specialFolders.TryAdd("Flow", @"C:\TaskMaster\Flow");
            var sut = CreateSutWithSpecialFolders(specialFolders);

            // Act
            var movedMails = sut.MovedMails;
            var ctfMapPropertyValue = sut.CtfMap;
            var loadedCtfMap = sut.LoadCtfMap();
            var commonWords = sut.CommonWords;

            // Assert
            movedMails.Should().BeNull();
            ctfMapPropertyValue.Should().BeNull();
            loadedCtfMap.Should().BeNull();
            commonWords.Should().BeNull();
        }

        private static AppAutoFileObjects CreateSutWithSpecialFolders(
            ConcurrentDictionary<string, string> specialFolders
        )
        {
            var mockFs = new Mock<IFileSystemFolderPaths>(MockBehavior.Strict);
            mockFs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);
            return new AppAutoFileObjects(mockGlobals.Object);
        }
    }
}
