using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
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

    internal class TestBuildingEngine : MulticlassEngine<TestBuildingEngine>
    {
        public TestBuildingEngine()
            : base() { }

        public TestBuildingEngine(IApplicationGlobals globals)
            : base(globals) { }

        public override Task<bool> BuildClassifiersAsync(
            BayesianClassifierGroup classifierGroup,
            MinedMailInfo[] collection,
            ProgressPackage ppkg,
            string groupName,
            int minimumCountPerToken = 0
        )
        {
            foreach (var mail in collection)
            {
                var key = mail.GroupingKey ?? mail.EntryId ?? Guid.NewGuid().ToString();
                classifierGroup.Classifiers[key] = new BayesianClassifierShared(key);
            }

            return Task.FromResult(true);
        }

        public override Task TestAsync(MailItemHelper helper) => Task.CompletedTask;
    }

    [TestClass]
    public class MulticlassEngine_Tests
    {
        [TestMethod]
        public void Constructor_WithGlobals_SetsGlobals()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            engine.Globals.Should().BeSameAs(mockGlobals.Object);
            engine.CgUtilities.Should().NotBeNull();
        }

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

        [TestMethod]
        public void Serialize_CallsClassifierGroupSerialize()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            engine.ClassifierGroup = new BayesianClassifierGroup();
            ((IConditionalEngine<MailItemHelper>)engine).Serialize();
        }

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

        [TestMethod]
        public void Config_ReturnsClassifierGroupConfig()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            var group = new BayesianClassifierGroup();
            engine.ClassifierGroup = group;

            engine.Config.Should().BeSameAs(group.Config);
        }

        [TestMethod]
        public void TypedItem_SetAndGet()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            engine.TypedItem = null;

            engine.TypedItem.Should().BeNull();
        }

        [TestMethod]
        public void EngineInitializer_IsNotNull()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);

            engine.EngineInitializer.Should().NotBeNull();
        }

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

        [TestMethod]
        public async Task InitAsync_GroupInManager_WiresClassifierGroupAndEngineName()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup();
            manager["MyGroup"] = classifierGroup.ToAsyncLazy();
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var engine = new TestMulticlassEngine(mockGlobals.Object);

            var result = await engine.InitAsync("MyGroup");

            result.Should().NotBeNull();
            result!.ClassifierGroup.Should().BeSameAs(classifierGroup);
            result.EngineName.Should().Be("MyGroup");
            result.Globals.Should().BeSameAs(mockGlobals.Object);
        }

        [TestMethod]
        public async Task BuildClassifiersAsync_ThreeMails_CreatesThreeClassifiers()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestBuildingEngine(mockGlobals.Object);
            var cg = new BayesianClassifierGroup();
            var mails = new[]
            {
                new MinedMailInfo { GroupingKey = "Category:A" },
                new MinedMailInfo { GroupingKey = "Category:B" },
                new MinedMailInfo { GroupingKey = "Category:C" },
            };

            await engine.BuildClassifiersAsync(cg, mails, null, "TestGroup");

            cg.Classifiers.Should().HaveCount(3);
            cg.Classifiers.Should().ContainKey("Category:A");
            cg.Classifiers.Should().ContainKey("Category:B");
            cg.Classifiers.Should().ContainKey("Category:C");
        }

        [TestMethod]
        public async Task InitAsync_GroupAbsentFromManager_ReturnsNullWithoutCreatingClassifier()
        {
            var mockGlobals = CreateMockGlobals();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new ManagerAsyncLazy(mockGlobals.Object);
            mockAf.Setup(a => a.Manager).Returns(manager);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);

            var engine = new TestMulticlassEngine(mockGlobals.Object);

            var result = await engine.InitAsync("MissingGroup");

            result.Should().BeNull();
            engine.ClassifierGroup.Should().BeNull();
        }

        [TestMethod]
        public async Task BuildClassifiersAsync_NoAppData_CompletesAndHidesProgressPane()
        {
            var mockGlobals = CreateMockGlobals();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            var manager = new StubManagerAsyncLazy(mockGlobals.Object);
            var progressPane = new Mock<CustomTaskPane>();
            progressPane.SetupProperty(x => x.Visible, false);
            mockFs
                .SetupGet(x => x.SpecialFolders)
                .Returns(new ConcurrentDictionary<string, string>());
            mockGlobals.SetupGet(x => x.FS).Returns(mockFs.Object);
            mockAf.SetupGet(x => x.Manager).Returns(manager);
            mockAf.SetupGet(x => x.ProgressTracker).Returns(CreateHeadlessProgressTrackerPane());
            mockAf.SetupGet(x => x.ProgressPane).Returns(progressPane.Object);
            mockGlobals.SetupGet(x => x.AF).Returns(mockAf.Object);

            var engine = new TestMulticlassEngine(mockGlobals.Object) { EngineName = "MyGroup" };
            engine.CgUtilities = new StubMulticlassEngineUtilities(
                mockGlobals.Object,
                new BayesianClassifierGroup()
            );

            await engine.BuildClassifiersAsync();

            progressPane.Object.Visible.Should().BeFalse();
        }

        [TestMethod]
        public async Task BuildClassifierAsync_GroupingKey_RebuildsClassifierFromGroupedTokens()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 2,
                SharedTokenBase = new Corpus(new Dictionary<string, int> { { "keep", 3 } }),
            };
            var items = new[]
            {
                new MinedMailInfo { GroupingKey = "Team", Tokens = new[] { "keep", "keep" } },
                new MinedMailInfo { GroupingKey = "Team", Tokens = new[] { "keep" } },
            };

            await engine.BuildClassifierAsync(
                items.GroupBy(x => x.GroupingKey).First(),
                classifierGroup,
                CancellationToken.None
            );

            classifierGroup.Classifiers.Should().ContainKey("Team");
            classifierGroup.Classifiers["Team"].MatchEmailCount.Should().Be(2);
        }

        [TestMethod]
        public void Condition_NonMailItem_ReturnsFalse()
        {
            var mockGlobals = CreateMockGlobals();
            var engine = new TestMulticlassEngine(mockGlobals.Object);
            var mockAppointment = new Mock<Microsoft.Office.Interop.Outlook.AppointmentItem>();
            mockAppointment
                .Setup(m => m.Class)
                .Returns(Microsoft.Office.Interop.Outlook.OlObjectClass.olAppointment);
            mockAppointment.Setup(m => m.CreationTime).Returns(new DateTime(2026, 4, 3, 9, 15, 0));
            mockAppointment.Setup(m => m.Subject).Returns("Planning");

            engine.Condition(mockAppointment.Object).Should().BeFalse();
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

        private static ProgressTrackerPane CreateHeadlessProgressTrackerPane(double progress = 0)
        {
            var pane = (ProgressTrackerPane)
                FormatterServices.GetUninitializedObject(typeof(ProgressTrackerPane));
            var parentProgressType = typeof(ProgressTrackerPane)
                .Assembly.GetType("UtilitiesCS.ParentProgress`1")!
                .MakeGenericType(typeof(ValueTuple<int, string>));
            var parentProgress = Activator.CreateInstance(
                parentProgressType,
                new Progress<(int Value, string JobName)>(_ => { }),
                100,
                0
            );
            typeof(ProgressTrackerPane)
                .GetField(
                    "_parent",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )!
                .SetValue(pane, parentProgress);
            typeof(ProgressTrackerPane)
                .GetField(
                    "_progress",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )!
                .SetValue(pane, progress);
            typeof(ProgressTrackerPane)
                .GetField(
                    "_isRoot",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )!
                .SetValue(pane, false);
            typeof(ProgressTrackerPane)
                .GetField(
                    "_jobName",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )!
                .SetValue(pane, "Test");
            return pane;
        }

        private sealed class StubManagerAsyncLazy : ManagerAsyncLazy
        {
            public StubManagerAsyncLazy(IApplicationGlobals globals)
                : base(globals)
            {
                Configuration = new AsyncLazy<
                    ConcurrentDictionary<string, SmartSerializableLoader>
                >(() =>
                    Task.FromResult(new ConcurrentDictionary<string, SmartSerializableLoader>())
                );
            }
        }

        private sealed class StubMulticlassEngineUtilities(
            IApplicationGlobals globals,
            BayesianClassifierGroup classifierGroup
        ) : ClassifierGroupUtilities(globals)
        {
            public override Task<BayesianClassifierGroup> GetOrCreateClassifierGroupAsync(
                MinedMailInfo[] collection,
                string name,
                int minimumCountPerToken = 0
            ) => Task.FromResult(classifierGroup);
        }
    }
}
