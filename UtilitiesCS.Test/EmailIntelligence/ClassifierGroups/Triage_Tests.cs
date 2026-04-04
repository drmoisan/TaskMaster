using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions.Lazy;
using TriageClass = UtilitiesCS.EmailIntelligence.Triage;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public partial class Triage_Tests
    {
        [TestCleanup]
        public void ResetMyBoxDialogInvoker()
        {
            MyBox.DialogInvoker = viewer => viewer.ShowDialog();
        }

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
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.AF).Returns((IAppAutoFileObjects)null);
            var triage = new TriageClass(mockGlobals.Object);

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
            var manager = ConfigureManager(mockGlobals);
            var group = TriageClass.CreateClassifier();
            manager["Triage"] = group.ToAsyncLazy();

            var triage = new TriageClass(mockGlobals.Object);

            var (isValid, message) = await triage.HasValidTriageManagerAsync(default);
            isValid.Should().BeTrue();
            message.Should().BeEmpty();
        }

        [TestMethod]
        public async Task HasValidTriageManagerAsync_GroupMissingClassifier_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = ConfigureManager(mockGlobals);
            var group = TriageClass.CreateClassifier();
            group.Classifiers.TryRemove("B", out _);
            manager[TriageClass.GroupName] = group.ToAsyncLazy();

            var triage = new TriageClass(mockGlobals.Object);

            var (isValid, message) = await triage.HasValidTriageManagerAsync(default);

            isValid.Should().BeFalse();
            message.Should().Contain("classifier named B");
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
            var manager = ConfigureManager(mockGlobals);
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
        public async Task CreateAsync_CreateTreatment_InitializesClassifierAndDelegates()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = ConfigureManager(mockGlobals);

            var triage = await TriageClass.CreateAsync(
                mockGlobals.Object,
                treatment: Enums.NotFoundEnum.Create
            );

            triage.Should().NotBeNull();
            triage.ClassifierGroup.Should().NotBeNull();
            triage.TokenizeAsync.Should().NotBeNull();
            triage.CallbackAsync.Should().NotBeNull();
            manager.Should().ContainKey(TriageClass.GroupName);
            (await manager[TriageClass.GroupName]).Should().BeSameAs(triage.ClassifierGroup);
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
        public async Task Engine_ReturnsSelf()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = ConfigureManager(mockGlobals);
            manager[TriageClass.GroupName] = TriageClass.CreateClassifier().ToAsyncLazy();

            var triage = (TriageClass)await TriageClass.CreateEngineAsync(mockGlobals.Object);
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
        public async Task AsyncCondition_ReturnsDelegate()
        {
            var mockGlobals = CreateMockGlobals();
            var triage = new TriageClass(mockGlobals.Object);
            var existingTriage = CreateUserProperty("A");
            var withoutTriage = CreateMailItem(
                userProperties: CreateWritableUserProperties(CreateUserProperty().Object).Object
            );
            var withTriage = CreateMailItem(
                userProperties: CreateWritableUserProperties(
                    CreateUserProperty().Object,
                    existingTriage.Object
                ).Object
            );
            var condition = triage.AsyncCondition;

            condition.Should().NotBeNull();
            (await condition(withoutTriage.Object)).Should().BeTrue();
            (await condition(withTriage.Object)).Should().BeFalse();
            (await condition(new object())).Should().BeFalse();
        }

        #endregion

        #region Helpers

        [TestMethod]
        public async Task WorkflowOverloads_ProcessSelectionAndMailItem()
        {
            var mockGlobals = CreateMockGlobals();
            var manager = ConfigureManager(mockGlobals);
            var triage = new TriageClass(mockGlobals.Object);
            var callbackValues = new List<string>();
            var tokens = new[] { "workflow", "coverage" };
            var mailItem = CreateMailItem(
                userProperties: CreateWritableUserProperties(CreateUserProperty().Object).Object
            );
            var selection = CreateSelection(mailItem.Object);

            await triage.CreateNewTriageClassifierGroupAsync(default);
            triage.ClassifierGroup.Globals = mockGlobals.Object;
            triage.ClassifierGroup.MinimumProbability = 0;
            triage.ClassifierGroup.TokenizeAsync = (_, _, _) => Task.FromResult(tokens);
            triage.TokenizeAsync = (_, _, _) => Task.FromResult(tokens);
            triage.CallbackAsync = (_, value) =>
            {
                callbackValues.Add(value);
                return Task.CompletedTask;
            };

            await triage.TrainAsync(selection.Object, "A");
            await triage.ClassifyAsync(selection.Object);
            await triage.TestAsync((Selection)null);
            await triage.TestAsync(selection.Object);
            await triage.TestAsync(mailItem.Object);

            manager.Should().ContainKey(TriageClass.GroupName);
            triage.ClassifierGroup.TotalEmailCount.Should().BeGreaterThan(0);
            callbackValues.Should().Contain("A");
        }

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
            mockOl.Setup(g => g.EmailPrefixToStrip).Returns(string.Empty);
            mockFs.Setup(f => f.SpecialFolders).Returns(specialFolders);
            return mockGlobals;
        }

        #endregion
    }
}
