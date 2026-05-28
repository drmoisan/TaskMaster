using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

#pragma warning disable CS0618

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [DoNotParallelize]
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

        [TestMethod]
        public async Task HeavyParallelizationAsync_WithFewerClassifiersThanProcessors_DoesNotThrow()
        {
            // Regression: InferNegative computed chunkSize via Math.Round(count / processors),
            // which rounds to 0 when the classifier count is smaller than half the processor
            // count. Chunk(0) then throws ArgumentOutOfRangeException. A single classifier
            // forces the chunkSize == 0 path on any multi-core host. The sibling RecalcProbsAsync
            // was already clamped; this guards the matching InferNegative path.
            var progressPane = new Mock<Microsoft.Office.Tools.CustomTaskPane>();
            progressPane.SetupProperty(x => x.Visible, false);
            var group = CreateConfiguredGroup(
                CreateGlobals(progressPane.Object, CancellationToken.None),
                classifierCount: 1,
                clearProbabilities: true
            );
            var stopwatch = new SegmentStopWatch().Start();

            Func<Task> act = () =>
                group.AfterDeserialized_HeavyParallelizationAsync(
                    CancellationToken.None,
                    stopwatch
                );

            // The fix's contract: the chunk pipeline runs end-to-end without the
            // ArgumentOutOfRangeException that Chunk(0) previously raised. Probability
            // rebuild content is covered by the sibling test with a realistic classifier
            // count; asserting Prob is non-null here confirms the rebuild path executed
            // while keeping this test deterministic under parallel load.
            await act.Should().NotThrowAsync();
            group.Classifiers.Values.Should().OnlyContain(classifier => classifier.Prob != null);
        }

        private static IApplicationGlobals CreateGlobals(
            Microsoft.Office.Tools.CustomTaskPane progressPane,
            CancellationToken cancelToken
        )
        {
            var autoFiles = new Mock<IAppAutoFileObjects>(MockBehavior.Loose);
            autoFiles
                .SetupGet(x => x.ProgressTracker)
                .Returns(CreateFakeProgressTrackerPane());
            autoFiles.SetupGet(x => x.ProgressPane).Returns(progressPane);
            autoFiles.SetupGet(x => x.CancelToken).Returns(cancelToken);

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            globals.SetupGet(x => x.AF).Returns(autoFiles.Object);
            return globals.Object;
        }

        private static ProgressTrackerPane CreateFakeProgressTrackerPane()
        {
            var pane = (ProgressTrackerPane)
                FormatterServices.GetUninitializedObject(typeof(ProgressTrackerPane));
            var parentField = typeof(ProgressTrackerPane).GetField(
                "_parent",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var parent = Activator.CreateInstance(
                parentField.FieldType,
                new SynchronousProgress<(int Value, string JobName)>(_ => { }),
                100,
                0
            );

            parentField.SetValue(pane, parent);
            typeof(ProgressTrackerPane)
                .GetField("_progressViewer", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pane, null);
            typeof(ProgressTrackerPane)
                .GetField("_isRoot", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pane, false);
            typeof(ProgressTrackerPane)
                .GetField("_root100", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pane, false);
            typeof(ProgressTrackerPane)
                .GetField("_progress", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pane, 0d);
            typeof(ProgressTrackerPane)
                .GetField("_jobName", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(pane, string.Empty);

            return pane;
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

        private sealed class SynchronousProgress<T> : IProgress<T>
        {
            private readonly Action<T> _callback;

            public SynchronousProgress(Action<T> callback)
            {
                _callback = callback;
            }

            public void Report(T value)
            {
                _callback(value);
            }
        }
    }
}
