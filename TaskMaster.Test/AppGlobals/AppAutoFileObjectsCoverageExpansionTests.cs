using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Coverage expansion for AppAutoFileObjects initialization and property decision paths that can
    /// run without Outlook automation or filesystem temporary files.
    /// </summary>
    [TestClass]
    public class AppAutoFileObjectsCoverageExpansionTests
    {
        private int _originalLngConvCtPwr;
        private int _originalConversationWeight;
        private int _originalMatchScore;
        private int _originalMismatchScore;
        private int _originalGapPenalty;
        private int _originalMaxRecents;

        [TestInitialize]
        public void TestInitialize()
        {
            var defaults = TaskMaster.Properties.Settings.Default;
            _originalLngConvCtPwr = defaults.ConversationExponent;
            _originalConversationWeight = defaults.ConversationWeight;
            _originalMatchScore = defaults.SmithWatterman_MatchScore;
            _originalMismatchScore = defaults.SmithWatterman_MismatchScore;
            _originalGapPenalty = defaults.SmithWatterman_GapPenalty;
            _originalMaxRecents = defaults.MaxRecents;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var defaults = TaskMaster.Properties.Settings.Default;
            defaults.ConversationExponent = _originalLngConvCtPwr;
            defaults.ConversationWeight = _originalConversationWeight;
            defaults.SmithWatterman_MatchScore = _originalMatchScore;
            defaults.SmithWatterman_MismatchScore = _originalMismatchScore;
            defaults.SmithWatterman_GapPenalty = _originalGapPenalty;
            defaults.MaxRecents = _originalMaxRecents;
            defaults.Save();
        }

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

        [TestMethod]
        public void ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());

            // Act
            sut.LngConvCtPwr = _originalLngConvCtPwr + 1;
            sut.Conversation_Weight = _originalConversationWeight + 1;
            sut.SmithWatterman_MatchScore = _originalMatchScore + 1;
            sut.SmithWatterman_MismatchScore = _originalMismatchScore + 1;
            sut.SmithWatterman_GapPenalty = _originalGapPenalty + 1;
            sut.MaxRecents = _originalMaxRecents + 1;

            // Assert
            sut.LngConvCtPwr.Should().Be(_originalLngConvCtPwr + 1);
            sut.Conversation_Weight.Should().Be(_originalConversationWeight + 1);
            sut.SmithWatterman_MatchScore.Should().Be(_originalMatchScore + 1);
            sut.SmithWatterman_MismatchScore.Should().Be(_originalMismatchScore + 1);
            sut.SmithWatterman_GapPenalty.Should().Be(_originalGapPenalty + 1);
            sut.MaxRecents.Should().Be(_originalMaxRecents + 1);
        }

        [TestMethod]
        public void CommonWordsSetter_WhenFlowFolderMissing_DoesNotSerializeOrAssignFolder()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());
            var commonWords = new SerializableList<string>();

            // Act
            sut.CommonWords = commonWords;

            // Assert
            sut.CommonWords.Should().BeSameAs(commonWords);
            commonWords.Folderpath.Should().BeEmpty();
            commonWords.Filename.Should().BeEmpty();
        }

        [TestMethod]
        public async Task PrivateAsyncLoaders_WhenConfigurationMissing_FailDeterministically()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(
                new ConcurrentDictionary<string, string>(),
                new ConcurrentDictionary<string, SmartSerializableLoader>()
            );

            // Act
            await InvokePrivateTaskAsync(sut, "LoadRecentsListAsync");
            await InvokePrivateTaskAsync(sut, "LoadMovedMailsAsync");
            await InvokePrivateTaskAsync(sut, "LoadCtfMapAsync");
            await InvokePrivateTaskAsync(sut, "LoadCommonWordsAsync");
            Func<Task> loadFilters = async () =>
                await InvokePrivateTaskAsync(sut, "LoadFiltersAsync");
            Func<Task> loadSubjectMapAndEncoder = async () =>
                await InvokePrivateTaskAsync(sut, "LoadSubjectMapAndEncoderAsync");

            // Assert
            sut.RecentsList.Should().BeNull();
            sut.MovedMails.Should().BeNull();
            sut.CtfMap.Should().BeNull();
            sut.CommonWords.Should().BeNull();
            await loadFilters.Should().ThrowAsync<NullReferenceException>();
            await loadSubjectMapAndEncoder.Should().ThrowAsync<NullReferenceException>();
        }

        [TestMethod]
        public void BackupLoaders_WithMissingFiles_ReturnEmptyCollections()
        {
            // Arrange
            var sut = CreateSutWithSpecialFolders(new ConcurrentDictionary<string, string>());

            // Act
            var commonWords = InvokePrivate<IList<string>>(
                sut,
                "CommonWordsBackupLoader",
                @"C:\TaskMaster\missing-common-words.csv"
            );
            var subjectMapEntries = InvokePrivate<IList<SubjectMapEntry>>(
                sut,
                "SubjectMapBackupLoader",
                @"C:\TaskMaster\missing-subject-map.csv"
            );

            // Assert
            commonWords.Should().BeEmpty();
            subjectMapEntries.Should().BeEmpty();
        }

        private static AppAutoFileObjects CreateSutWithSpecialFolders(
            ConcurrentDictionary<string, string> specialFolders,
            ConcurrentDictionary<string, SmartSerializableLoader> config = null
        )
        {
            var mockFs = new Mock<IFileSystemFolderPaths>(MockBehavior.Strict);
            mockFs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);
            if (config is not null)
            {
                mockGlobals
                    .SetupGet(x => x.IntelRes)
                    .Returns(new StubIntelligenceConfig(mockGlobals.Object, config));
            }
            return new AppAutoFileObjects(mockGlobals.Object);
        }

        private static async Task InvokePrivateTaskAsync(AppAutoFileObjects sut, string methodName)
        {
            var task = (Task)InvokePrivate<object>(sut, methodName);
            await task;
        }

        private static T InvokePrivate<T>(
            AppAutoFileObjects sut,
            string methodName,
            params object[] arguments
        )
        {
            var method = typeof(AppAutoFileObjects).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            method.Should().NotBeNull();
            return (T)method.Invoke(sut, arguments);
        }

        private sealed class StubIntelligenceConfig : IntelligenceConfig
        {
            internal StubIntelligenceConfig(
                IApplicationGlobals globals,
                ConcurrentDictionary<string, SmartSerializableLoader> config
            )
                : base(globals)
            {
                Config = config;
            }
        }
    }
}
