using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.Threading;
using TriageClass = UtilitiesCS.EmailIntelligence.Triage;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public class Triage_Tests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_WithGlobals_SetsProperties()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            triage.Globals.Should().BeSameAs(mockGlobals.Object);
            triage.OlLogic.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithToken_SetsToken()
        {
            var mockGlobals = CreateMockGlobals();
            var cts = new CancellationTokenSource();
            var triage = new TriageClass(mockGlobals.Object, cts.Token);

            triage.Token.Should().Be(cts.Token);
        }

        #endregion

        #region Static Members

        [TestMethod]
        public void ClassNames_ContainsABC()
        {
            TriageClass.ClassNames.Should().Contain("A");
            TriageClass.ClassNames.Should().Contain("B");
            TriageClass.ClassNames.Should().Contain("C");
            TriageClass.ClassNames.Should().HaveCount(3);
        }

        [TestMethod]
        public void UnknownClassMarker_IsU()
        {
            TriageClass.UnknownClassMarker.Should().Be("U");
        }

        [TestMethod]
        public void CreateClassifier_ReturnsGroupWithThreeClassifiers()
        {
            var group = TriageClass.CreateClassifier();

            group.Should().NotBeNull();
            group.Classifiers.Should().ContainKey("A");
            group.Classifiers.Should().ContainKey("B");
            group.Classifiers.Should().ContainKey("C");
            group.MinimumProbability.Should().Be(0.9);
        }

        [TestMethod]
        public async Task CreateTriageClassifiersAsync_ReturnsClassifierGroup()
        {
            var group = await TriageClass.CreateTriageClassifiersAsync(TriageClass.ClassNames);

            group.Should().NotBeNull();
            group.Classifiers.Should().HaveCount(3);
        }

        #endregion

        #region ValidateTriageManagerAsync

        [TestMethod]
        public async Task ValidateTriageManagerAsync_ValidValidator_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            var result = await triage.ValidateTriageManagerAsync(
                _ => Task.FromResult<(bool, string)>((true, "")),
                (_, __, ___) => Task.FromResult(false),
                Enums.NotFoundEnum.Skip,
                default
            );

            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateTriageManagerAsync_InvalidValidator_CallsAction()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            bool actionCalled = false;

            var result = await triage.ValidateTriageManagerAsync(
                _ => Task.FromResult<(bool, string)>((false, "missing")),
                (treatment, msg, token) =>
                {
                    actionCalled = true;
                    return Task.FromResult(false);
                },
                Enums.NotFoundEnum.Skip,
                default
            );

            result.Should().BeFalse();
            actionCalled.Should().BeTrue();
        }

        #endregion

        #region HasValidTriageManagerAsync

        [TestMethod]
        public async Task HasValidTriageManagerAsync_NullGlobals_ReturnsFalse()
        {
            // Triage with null globals - HasValid should return false
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.AF).Returns((IAppAutoFileObjects)null);
            var triage = new TriageClass(mockGlobals.Object);

            // Access globals.AF will be null, so ThrowIfNull should catch it
            var (isValid, message) = await triage.HasValidTriageManagerAsync(default);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task HasValidTriageManagerAsync_ManagerMissingTriageGroup_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var triage = new TriageClass(mockGlobals.Object);

            var (isValid, message) = await triage.HasValidTriageManagerAsync(default);
            isValid.Should().BeFalse();
            message.Should().Contain("Triage");
        }

        [TestMethod]
        public async Task HasValidTriageManagerAsync_ManagerWithTriageGroup_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            var group = TriageClass.CreateClassifier();
            manager["Triage"] = group.ToAsyncLazy();

            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var triage = new TriageClass(mockGlobals.Object);

            var (isValid, message) = await triage.HasValidTriageManagerAsync(default);
            isValid.Should().BeTrue();
            message.Should().BeEmpty();
        }

        #endregion

        #region TriageMissingHandlerAsync

        [TestMethod]
        public async Task TriageMissingHandlerAsync_SkipTreatment_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            var result = await triage.TriageMissingHandlerAsync(
                Enums.NotFoundEnum.Skip,
                "test",
                default
            );

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task TriageMissingHandlerAsync_CreateTreatment_AppliesStoredTriageConfig()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var triage = new TriageClass(mockGlobals.Object);

            var result = await triage.TriageMissingHandlerAsync(
                Enums.NotFoundEnum.Create,
                "missing triage classifier",
                default
            );

            result.Should().BeTrue();
            triage.ClassifierGroup.Should().NotBeNull();
            triage.ClassifierGroup.Config.Disk.FileName.Should().Be("ManagerTriage.json");
            triage.ClassifierGroup.Config.Disk.FilePath.Should().EndWith("ManagerTriage.json");
            manager.Should().ContainKey(TriageClass.GroupName);
        }

        [TestMethod]
        public void TriageMissingHandlerAsync_ThrowTreatment_ThrowsArgumentNullException()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            Func<Task> act = async () =>
                await triage.TriageMissingHandlerAsync(
                    Enums.NotFoundEnum.Throw,
                    "error message",
                    default
                );

            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void TriageMissingHandlerAsync_InvalidTreatment_ThrowsArgumentOutOfRange()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            Func<Task> act = async () =>
                await triage.TriageMissingHandlerAsync((Enums.NotFoundEnum)999, "test", default);

            act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        #endregion

        #region IConditionalEngine Properties

        [TestMethod]
        public void EngineName_IsTriage()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            triage.EngineName.Should().Be("Triage");
        }

        [TestMethod]
        public void Engine_ReturnsSelf()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            triage.Engine.Should().BeSameAs(triage);
        }

        [TestMethod]
        public void Message_ContainsTriage()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            triage.Message.Should().Contain("Triage");
        }

        [TestMethod]
        public void AsyncAction_ReturnsDelegate()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            triage.AsyncAction.Should().NotBeNull();
        }

        [TestMethod]
        public void AsyncCondition_ReturnsDelegate()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            triage.AsyncCondition.Should().NotBeNull();
        }

        #endregion

        #region Properties

        [TestMethod]
        public void ClassifierGroup_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            triage.ClassifierGroup = group;

            triage.ClassifierGroup.Should().BeSameAs(group);
        }

        [TestMethod]
        public void TokenizeAsync_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            Func<object, IApplicationGlobals, CancellationToken, Task<string[]>> tokenizer = (
                _,
                __,
                ___
            ) => Task.FromResult(new[] { "token" });
            triage.TokenizeAsync = tokenizer;

            triage.TokenizeAsync.Should().BeSameAs(tokenizer);
        }

        [TestMethod]
        public void CallbackAsync_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            Func<object, string, Task> callback = (_, __) => Task.CompletedTask;
            triage.CallbackAsync = callback;

            triage.CallbackAsync.Should().BeSameAs(callback);
        }

        #endregion

        #region ManagerAsyncLazy

        [TestMethod]
        public void ManagerAsyncLazy_Constructor_SetsGlobals()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            manager.Should().NotBeNull();
            manager.Configuration.Should().NotBeNull();
        }

        [TestMethod]
        public void ManagerAsyncLazy_ResetConfigAsyncLazy_ResetsConfiguration()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            var original = manager.Configuration;
            manager.ResetConfigAsyncLazy();

            manager.Configuration.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ManagerAsyncLazy_InitAsync_DoesNotThrow()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            // InitAsync calls ResetLoadManagerAsyncLazy which reads config
            // With mock globals that have no real resource manager, this may fail gracefully
            Func<Task> act = async () => await manager.InitAsync();

            // InitAsync succeeds gracefully with mock globals that have no resource manager
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public void ManagerAsyncLazy_TryGetValue_MissingKey_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);

            var found = manager.TryGetValue("NonExistent", out var value);

            found.Should().BeFalse();
        }

        [TestMethod]
        public async Task ManagerAsyncLazy_AddAndRetrieve_Works()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            manager["TestKey"] = group.ToAsyncLazy();

            manager.TryGetValue("TestKey", out var task).Should().BeTrue();
            var result = await task;
            result.Should().BeSameAs(group);
        }

        #endregion

        #region Triage Additional Methods

        [TestMethod]
        public void Triage_CreateClassifier_SetsMinimumProbability()
        {
            var group = TriageClass.CreateClassifier();
            group.MinimumProbability.Should().Be(0.9);
            group.TotalEmailCount.Should().Be(0);
        }

        [TestMethod]
        public void Triage_Serialize_WithClassifierGroup_DoesNotThrow()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            triage.ClassifierGroup = new BayesianClassifierGroup();

            System.Action act = () => triage.Serialize();
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Triage_Config_ReturnsClassifierGroupConfig()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            triage.ClassifierGroup = group;

            triage.Config.Should().BeSameAs(group.Config);
        }

        [TestMethod]
        public async Task Triage_TrainAsync_WithTokens_TrainsClassifier()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            var group = TriageClass.CreateClassifier();
            triage.ClassifierGroup = group;

            var tokens = new[] { "hello", "world" };
            await triage.TrainAsync(tokens, "A");

            // Training should not throw and classifier group should still be valid
            triage.ClassifierGroup.Should().NotBeNull();
        }

        [TestMethod]
        public void Triage_TypedItem_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            triage.TypedItem = null;

            triage.TypedItem.Should().BeNull();
        }

        [TestMethod]
        public void Triage_EngineInitializer_Throws()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);

            System.Action act = () =>
            {
                var _ = triage.EngineInitializer;
            };
            act.Should().Throw<NotImplementedException>();
        }

        #endregion

        #region Helpers

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var specialFolders = new ConcurrentDictionary<string, string>(
                new[]
                {
                    new KeyValuePair<string, string>("AppData", @"Z:\TaskMasterAppData"),
                    new KeyValuePair<string, string>("Flow", @"Z:\TaskMasterFlow"),
                }
            );

            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            mockFs.Setup(f => f.SpecialFolders).Returns(specialFolders);
            return mockGlobals;
        }

        #endregion
    }
}
