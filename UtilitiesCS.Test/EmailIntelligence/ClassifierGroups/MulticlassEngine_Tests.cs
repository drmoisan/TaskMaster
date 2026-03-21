using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    /// <summary>
    /// Concrete test implementation of MulticlassEngine for testing the abstract base class.
    /// </summary>
    internal class TestMulticlassEngine : MulticlassEngine<TestMulticlassEngine>
    {
        public TestMulticlassEngine()
            : base() { }

        public TestMulticlassEngine(IApplicationGlobals globals)
            : base(globals) { }

        public override Task<bool> BuildClassifiersAsync(
            BayesianClassifierGroup classifierGroup,
            MinedMailInfo[] collection,
            ProgressPackage ppkg,
            string groupName,
            int minimumCountPerToken = 0
        )
        {
            return Task.FromResult(true);
        }

        public override Task TestAsync(MailItemHelper helper)
        {
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class MulticlassEngine_Tests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_WithGlobals_SetsGlobals()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            engine.Globals.Should().BeSameAs(mockGlobals.Object);
            engine.CgUtilities.Should().NotBeNull();
        }

        #endregion

        #region Properties

        [TestMethod]
        public void IsActivated_NoClassifierGroup_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            engine.IsActivated.Should().BeFalse();
        }

        [TestMethod]
        public void IsActivated_WithClassifierGroup_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            engine.ClassifierGroup = new BayesianClassifierGroup();

            engine.IsActivated.Should().BeTrue();
        }

        [TestMethod]
        public void ProbabilityThreshold_DefaultIs0_8()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            engine.ProbabilityThreshold.Should().Be(0.8);
        }

        [TestMethod]
        public void EngineName_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            engine.EngineName = "TestEngine";

            engine.EngineName.Should().Be("TestEngine");
        }

        [TestMethod]
        public void Engine_ReturnsSelf()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            engine.Engine.Should().BeSameAs(engine);
        }

        [TestMethod]
        public void Message_HasDefaultValue()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            engine.Message.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void AsyncAction_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            Func<MailItemHelper, Task> action = _ => Task.CompletedTask;
            engine.AsyncAction = action;

            engine.AsyncAction.Should().BeSameAs(action);
        }

        [TestMethod]
        public void AsyncCondition_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            Func<object, Task<bool>> condition = _ => Task.FromResult(true);
            engine.AsyncCondition = condition;

            engine.AsyncCondition.Should().BeSameAs(condition);
        }

        #endregion

        #region IConditionalEngine

        [TestMethod]
        public void Serialize_CallsClassifierGroupSerialize()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            engine.ClassifierGroup = new BayesianClassifierGroup();

            // Serialize should not throw
            ((IConditionalEngine<MailItemHelper>)engine).Serialize();
        }

        #endregion

        #region Condition

        [TestMethod]
        public void Condition_MailItem_WithIPMNote_ReturnsTrue()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            var mockMailItem = new Mock<Microsoft.Office.Interop.Outlook.MailItem>();
            mockMailItem
                .Setup(m => m.Class)
                .Returns(Microsoft.Office.Interop.Outlook.OlObjectClass.olMail);
            mockMailItem.Setup(m => m.MessageClass).Returns("IPM.Note");

            var result = engine.Condition(mockMailItem.Object);
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Condition_MailItem_NonIPMNote_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            var mockMailItem = new Mock<Microsoft.Office.Interop.Outlook.MailItem>();
            mockMailItem
                .Setup(m => m.Class)
                .Returns(Microsoft.Office.Interop.Outlook.OlObjectClass.olMail);
            mockMailItem.Setup(m => m.MessageClass).Returns("IPM.Schedule.Meeting.Request");

            var result = engine.Condition(mockMailItem.Object);
            result.Should().BeFalse();
        }

        #endregion

        #region CreateEngineAsync

        [TestMethod]
        public async Task CreateEngineAsync_GroupNotInManager_ReturnsDefault()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var result = await TestMulticlassEngine.CreateEngineAsync(
                mockGlobals.Object,
                "NonExistent"
            );
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task CreateEngineAsync_GroupInManager_ReturnsEngine()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup();
            manager["TestGroup"] = classifierGroup.ToAsyncLazy();
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var result = await TestMulticlassEngine.CreateEngineAsync(
                mockGlobals.Object,
                "TestGroup"
            );
            result.Should().NotBeNull();
            result.ClassifierGroup.Should().BeSameAs(classifierGroup);
            result.EngineName.Should().Be("TestGroup");
        }

        #endregion

        #region Config

        [TestMethod]
        public void Config_ReturnsClassifierGroupConfig()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            engine.ClassifierGroup = group;

            engine.Config.Should().BeSameAs(group.Config);
        }

        #endregion

        #region TypedItem

        [TestMethod]
        public void TypedItem_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            engine.TypedItem = null;

            engine.TypedItem.Should().BeNull();
        }

        #endregion

        #region EngineInitializer

        [TestMethod]
        public void EngineInitializer_IsNotNull()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            engine.EngineInitializer.Should().NotBeNull();
        }

        #endregion

        #region InitAsync

        [TestMethod]
        public async Task InitAsync_ClassifierGroupNotInManager_ReturnsDefault()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var engine = new TestMulticlassEngine(mockGlobals.Object);

            var result = await engine.InitAsync("NonExistentGroup");
            result.Should().BeNull();
        }

        #endregion

        #region Helpers

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
