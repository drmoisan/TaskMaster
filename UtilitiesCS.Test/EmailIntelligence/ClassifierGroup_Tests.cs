using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.HelperClasses;

#pragma warning disable CS0618

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class ClassifierGroup_Remediation_Tests
    {
        [TestMethod]
        public async Task AfterDeserialize_WithConfiguredGlobals_LoadsClassifiersAndReportsCompletion()
        {
            var progressPane = new Mock<Microsoft.Office.Tools.CustomTaskPane>();
            progressPane.SetupProperty(x => x.Visible, false);
            var group = CreateConfiguredGroup(
                CreateGlobals(progressPane.Object, CancellationToken.None),
                classifierCount: 1,
                clearProbabilities: false
            );

            await group.AfterDeserialize(CancellationToken.None);

            progressPane.Object.Visible.Should().BeTrue();
            foreach (var classifier in group.Classifiers.Values)
            {
                classifier.Loaded.Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task HeavyParallelizationAsync_WithNullProbabilities_RebuildsLegacyClassifierState()
        {
            var processors = Math.Max(Environment.ProcessorCount - 2, 1);
            var progressPane = new Mock<Microsoft.Office.Tools.CustomTaskPane>();
            progressPane.SetupProperty(x => x.Visible, false);
            var group = CreateConfiguredGroup(
                CreateGlobals(progressPane.Object, CancellationToken.None),
                classifierCount: processors,
                clearProbabilities: true
            );
            var stopwatch = new SegmentStopWatch().Start();

            await group.AfterDeserialized_HeavyParallelizationAsync(
                CancellationToken.None,
                stopwatch
            );

            foreach (var classifier in group.Classifiers.Values)
            {
                classifier.Match.Should().NotBeNull();
                classifier.Prob.Should().NotBeNull();
                classifier.Prob.Should().NotBeEmpty();
            }
        }

        private static IApplicationGlobals CreateGlobals(
            Microsoft.Office.Tools.CustomTaskPane progressPane,
            CancellationToken cancelToken
        )
        {
            var autoFiles = new Mock<IAppAutoFileObjects>(MockBehavior.Loose);
            autoFiles
                .SetupGet(x => x.ProgressTracker)
                .Returns(BayesianPerformanceMeasurement_Tests.CreateFakeProgressTrackerPane());
            autoFiles.SetupGet(x => x.ProgressPane).Returns(progressPane);
            autoFiles.SetupGet(x => x.CancelToken).Returns(cancelToken);

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            globals.SetupGet(x => x.AF).Returns(autoFiles.Object);
            return globals.Object;
        }

        private static ClassifierGroup CreateConfiguredGroup(
            IApplicationGlobals globals,
            int classifierCount,
            bool clearProbabilities
        )
        {
            var group = new ClassifierGroup { AppGlobals = globals };

            for (var i = 0; i < classifierCount; i++)
            {
                var positiveTokens = Enumerable
                    .Repeat($"positive-{i}", 6)
                    .Concat(Enumerable.Repeat("shared", 6))
                    .ToArray();
                var negativeTokens = Enumerable.Repeat($"negative-{i}", 6).ToArray();
                group.SharedTokenBase.AddOrIncrementTokens(positiveTokens.Concat(negativeTokens));
                group.ForceClassifierUpdate($"tag-{i}", positiveTokens, negativeTokens);
                if (clearProbabilities)
                {
                    typeof(BayesianClassifier)
                        .GetField("_prob", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(group.Classifiers[$"tag-{i}"], null);
                }
            }

            return group;
        }
    }
}
