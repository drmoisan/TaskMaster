using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class ActionableClassifierGroup_Remediation_Tests
    {
        [TestMethod]
        public async Task BuildClassifiersAsync_WithActionableGroups_ReturnsTrueAndBuildsExpectedKeys()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new RecordingActionableClassifierGroup(mockGlobals.Object);
            var classifierGroup = new BayesianClassifierGroup
            {
                TotalEmailCount = 3,
                SharedTokenBase = new Corpus(
                    new Dictionary<string, int> { { "alpha", 2 }, { "beta", 2 } }
                ),
            };
            var collection = new[]
            {
                new MinedMailInfo { Actionable = "Action", Tokens = new[] { "alpha", "beta" } },
                new MinedMailInfo { Actionable = "Action", Tokens = new[] { "alpha" } },
                new MinedMailInfo { Actionable = "Reference", Tokens = new[] { "beta" } },
            };
            var package = CreateHeadlessProgressPackage();

            var result = await group.BuildClassifiersAsync(
                classifierGroup,
                collection,
                package,
                "Actionable"
            );

            result.Should().BeTrue();
            group.BuiltGroupingKeys.Should().BeEquivalentTo("Action", "Reference");
            classifierGroup.Classifiers.Keys.Should().Contain(new[] { "Action", "Reference" });
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

        private static ProgressPackage CreateHeadlessProgressPackage()
        {
            var cts = new CancellationTokenSource();
            return new ProgressPackage
            {
                CancelSource = cts,
                Cancel = cts.Token,
                ProgressTrackerPane = CreateHeadlessProgressTrackerPane(),
                StopWatch = new SegmentStopWatch().Start(),
            };
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

            SetPrivateField(pane, "_parent", parentProgress);
            SetPrivateField(pane, "_progress", progress);
            SetPrivateField(pane, "_isRoot", false);
            SetPrivateField(pane, "_jobName", "Test");
            return pane;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance
                .GetType()
                .GetField(
                    fieldName,
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                );
            field.Should().NotBeNull();
            field!.SetValue(instance, value);
        }

        private sealed class RecordingActionableClassifierGroup(IApplicationGlobals globals)
            : ActionableClassifierGroup(globals)
        {
            public ConcurrentBag<string> BuiltGroupingKeys { get; } = new();

            public override async Task BuildClassifierAsync(
                IGrouping<string, MinedMailInfo> group,
                BayesianClassifierGroup classifierGroup,
                CancellationToken cancel,
                int minimumCountPerToken = 0
            )
            {
                BuiltGroupingKeys.Add(group.Key);
                await base.BuildClassifierAsync(
                    group,
                    classifierGroup,
                    cancel,
                    minimumCountPerToken
                );
            }
        }
    }
}
