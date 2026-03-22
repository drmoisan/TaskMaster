using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    /// <summary>
    /// Concrete test implementation of TristateEngine for testing the abstract base class.
    /// </summary>
    internal class TestTristateEngine : TristateEngine
    {
        public bool TrainCalled { get; private set; }
        public bool TrainAsyncCalled { get; private set; }

        public override void Train(string[] tokens, bool state)
        {
            TrainCalled = true;
        }

        public override Task TrainAsync(string[] tokens, bool state)
        {
            TrainAsyncCalled = true;
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class TristateEngine_Tests
    {
        #region GetTristate

        [TestMethod]
        public void GetTristate_AboveMinTrue_ReturnsTrue()
        {
            var engine = CreateEngine();
            engine.Threshhold = new TristateThreshhold(0.8, 0.2);

            var result = engine.GetTristate(0.9);
            result.Should().BeTrue();
        }

        [TestMethod]
        public void GetTristate_BelowMaxFalse_ReturnsFalse()
        {
            var engine = CreateEngine();
            engine.Threshhold = new TristateThreshhold(0.8, 0.2);

            var result = engine.GetTristate(0.1);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void GetTristate_InBetween_ReturnsNull()
        {
            var engine = CreateEngine();
            engine.Threshhold = new TristateThreshhold(0.8, 0.2);

            var result = engine.GetTristate(0.5);
            result.Should().BeNull();
        }

        #endregion

        #region Train (object overload)

        [TestMethod]
        public void Train_WithTokenizer_CallsTrainOnTokens()
        {
            var engine = CreateEngine();
            engine.Tokenize = obj => new[] { "token1", "token2" };

            engine.Train("some item", true);
            engine.TrainCalled.Should().BeTrue();
        }

        [TestMethod]
        public void Train_WithCallback_InvokesCallback()
        {
            var engine = CreateEngine();
            engine.Tokenize = obj => new[] { "t" };
            bool callbackCalled = false;
            engine.Callback = obj => callbackCalled = true;

            engine.Train("item", true);
            callbackCalled.Should().BeTrue();
        }

        [TestMethod]
        public void Train_NullTokenizer_Throws()
        {
            var engine = CreateEngine();
            engine.Tokenize = null;

            System.Action act = () => engine.Train("item", true);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Train_NullItem_WithTokenizer_Completes()
        {
            var engine = CreateEngine();
            engine.Tokenize = obj => new[] { "t" };

            // Train delegates to Tokenize which handles null gracefully
            engine.Train(null, true);
            engine.TrainCalled.Should().BeTrue();
        }

        #endregion

        #region TrainAsync (object overload)

        [TestMethod]
        public async Task TrainAsync_WithTokenizer_CallsTrainAsync()
        {
            var engine = CreateEngine();
            engine.TokenizeAsync = obj => Task.FromResult(new[] { "t1" });

            await engine.TrainAsync("item", true);
            engine.TrainAsyncCalled.Should().BeTrue();
        }

        [TestMethod]
        public async Task TrainAsync_WithCallback_InvokesCallbackAsync()
        {
            var engine = CreateEngine();
            engine.TokenizeAsync = obj => Task.FromResult(new[] { "t1" });
            bool callbackCalled = false;
            engine.CallbackAsync = (obj, state) =>
            {
                callbackCalled = true;
                return Task.CompletedTask;
            };

            await engine.TrainAsync("item", false);
            callbackCalled.Should().BeTrue();
        }

        #endregion

        #region Properties

        [TestMethod]
        public void Properties_SetAndGet_RoundTrip()
        {
            var engine = CreateEngine();

            Func<object, string[]> tokenize = _ => Array.Empty<string>();
            engine.Tokenize = tokenize;
            engine.Tokenize.Should().BeSameAs(tokenize);

            Func<string[], double> calcProb = _ => 0.5;
            engine.CalculateProbability = calcProb;
            engine.CalculateProbability.Should().BeSameAs(calcProb);
        }

        #endregion

        #region SpamBayes

        [TestMethod]
        public void SpamBayes_Constructor_WithGlobals_CreatesInstance()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);
            sb.Should().NotBeNull();
            sb.Globals.Should().BeSameAs(mockGlobals.Object);
        }

        [TestMethod]
        public void SpamBayes_IsActivated_FalseByDefault()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);
            sb.IsActivated.Should().BeFalse();
        }

        [TestMethod]
        public void SpamBayes_ClassNames_ContainsSpamAndHam()
        {
            SpamBayes.ClassNames.Should().Contain("Spam");
            SpamBayes.ClassNames.Should().Contain("Ham");
        }

        [TestMethod]
        public void SpamBayes_GroupName_IsSpam()
        {
            SpamBayes.GroupName.Should().Be("Spam");
        }

        [TestMethod]
        public void SpamBayes_CreateNewClassifier_ReturnsGroupWithClassifiers()
        {
            var group = SpamBayes.CreateNewClassifier();
            group.Should().NotBeNull();
            group.Name.Should().Be("Spam");
            group.Classifiers.Should().ContainKey("Spam");
            group.Classifiers.Should().ContainKey("Ham");
        }

        [TestMethod]
        public void SpamBayes_ValidatePathsSet_NullGlobals_ReturnsFalse()
        {
            var sb = new SpamBayes(null);
            sb.ValidatePathsSet().Should().BeFalse();
        }

        [TestMethod]
        public void SpamBayes_ValidatePathsSet_ValidPaths_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var mockOl = new Mock<IOlObjects>();
            var mockFolder = new Mock<Microsoft.Office.Interop.Outlook.Folder>();
            var mockInbox = new Mock<Microsoft.Office.Interop.Outlook.Folder>();
            mockOl.Setup(o => o.JunkCertain).Returns(mockFolder.Object);
            mockOl.Setup(o => o.JunkPotential).Returns(mockFolder.Object);
            mockOl.Setup(o => o.Inbox).Returns(mockInbox.Object);
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);

            var sb = new SpamBayes(mockGlobals.Object);
            sb.ValidatePathsSet().Should().BeTrue();
        }

        [TestMethod]
        public async Task SpamBayes_ValidateSpamClassifierAsync_ValidValidator_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);

            var result = await sb.ValidateSpamClassifierAsync(
                _ => Task.FromResult<(bool, string)>((true, "")),
                (_, __, ___) => Task.FromResult(false),
                Enums.NotFoundEnum.Skip,
                default
            );
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task SpamBayes_ValidateSpamClassifierAsync_InvalidValidator_CallsAction()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);
            bool actionCalled = false;

            var result = await sb.ValidateSpamClassifierAsync(
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

        [TestMethod]
        public async Task SpamBayes_MissingHandler_SkipTreatment_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);

            var result = await sb.SpamBayesMissingHandlerAsync(
                Enums.NotFoundEnum.Skip,
                "test",
                default
            );
            result.Should().BeFalse();
        }

        [TestMethod]
        public void SpamBayes_MissingHandler_ThrowTreatment_ThrowsArgumentNullException()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);

            Func<Task> act = async () =>
                await sb.SpamBayesMissingHandlerAsync(Enums.NotFoundEnum.Throw, "err", default);
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void SpamBayes_MissingHandler_InvalidTreatment_ThrowsOutOfRange()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);

            Func<Task> act = async () =>
                await sb.SpamBayesMissingHandlerAsync((Enums.NotFoundEnum)999, "test", default);
            act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public async Task SpamBayes_CreateSpamClassifiersAsync_ReturnsGroup()
        {
            var group = await SpamBayes.CreateSpamClassifiersAsync();
            group.Should().NotBeNull();
            group.Classifiers.Should().ContainKey("Spam");
            group.Classifiers.Should().ContainKey("Ham");
        }

        [TestMethod]
        public void SpamBayes_ClassifierGroup_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            sb.ClassifierGroup = group;

            sb.ClassifierGroup.Should().BeSameAs(group);
            sb.IsActivated.Should().BeTrue();
        }

        [TestMethod]
        public void SpamBayes_GetDestinationFolder_NullMailItem_ReturnsNull()
        {
            var mockGlobals = CreateMockGlobals();
            var sb = new SpamBayes(mockGlobals.Object);

            var result = sb.GetDestinationFolder(null, true);
            result.Should().BeNull();
        }

        [TestMethod]
        public void SpamBayes_GetDestinationFolder_IsSpamFalse_ReturnsNull()
        {
            var mockGlobals = CreateMockGlobals();
            var mockOl = new Mock<IOlObjects>();
            var mockInbox = new Mock<Microsoft.Office.Interop.Outlook.Folder>();
            mockOl.Setup(o => o.Inbox).Returns(mockInbox.Object);
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);

            var sb = new SpamBayes(mockGlobals.Object);

            var mockMailItem = new Mock<Microsoft.Office.Interop.Outlook.MailItem>();
            mockMailItem.Setup(m => m.Parent).Returns((object)null);

            var result = sb.GetDestinationFolder(mockMailItem.Object, false);
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task SpamBayes_HasValidSpamClassifierAsync_NullAF_ReturnsFalse()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(g => g.AF).Returns((IAppAutoFileObjects)null);
            var sb = new SpamBayes(mockGlobals.Object);

            var (isValid, message) = await sb.HasValidSpamClassifierAsync(default);
            isValid.Should().BeFalse();
            message.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task SpamBayes_HasValidSpamClassifierAsync_NoSpamGroup_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var sb = new SpamBayes(mockGlobals.Object);

            var (isValid, message) = await sb.HasValidSpamClassifierAsync(default);
            isValid.Should().BeFalse();
            message.Should().Contain("Spam");
        }

        [TestMethod]
        public async Task SpamBayes_HasValidSpamClassifierAsync_ValidGroup_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var group = SpamBayes.CreateNewClassifier();
            manager["Spam"] = group.ToAsyncLazy();
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var sb = new SpamBayes(mockGlobals.Object);

            var (isValid, message) = await sb.HasValidSpamClassifierAsync(default);
            isValid.Should().BeTrue();
            message.Should().BeEmpty();
        }

        #endregion

        #region Helpers

        private static TestTristateEngine CreateEngine()
        {
            return new TestTristateEngine();
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            return mockGlobals;
        }

        #endregion
    }
}
