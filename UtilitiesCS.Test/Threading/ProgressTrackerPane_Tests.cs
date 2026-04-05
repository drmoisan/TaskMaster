using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class ProgressTrackerPane_Tests
    {
        [TestMethod]
        public void HeadlessRootFlows_ShouldReportProgressAndSupportChildSpawnsWithoutUi()
        {
            var harness = CreateHeadlessPane(isRoot: true);
            var pane = harness.Pane;

            pane.Progress.Should().Be(0);
            pane.ProgressViewer.Should().BeNull();

            pane.Report(0, "Initializing");
            pane.Increment(25, "Loading");
            pane.Progress.Should().Be(25);

            pane.Report((50, "Tuple"));
            pane.Progress.Should().Be(50);

            pane.Report(100, "Complete");
            pane.Report(100, "Complete again");
            pane.Report(150, "Overflow");
            pane.Report(101);

            var intChild = pane.SpawnChild(15);
            var doubleChild = pane.SpawnChild(12.7);
            var remainingChild = pane.SpawnChild();

            intChild.ProgressViewer.Should().BeNull();
            doubleChild.ProgressViewer.Should().BeNull();
            remainingChild.ProgressViewer.Should().BeNull();
            pane.Progress.Should().Be(100);
            harness
                .Reports.Should()
                .ContainInOrder(
                    (0, "Initializing"),
                    (25, "Loading"),
                    (50, "Tuple"),
                    (100, "Complete")
                );
        }

        [TestMethod]
        public void HeadlessPane_ShouldRejectNegativeValues_AndIgnoreMissingViewer()
        {
            var harness = CreateHeadlessPane();
            var pane = harness.Pane;
            Action namedReport = () => pane.Report(-1, "bad");
            Action valueReport = () => pane.Report(-1);

            pane.ProgressViewer.Should().BeNull();
            namedReport.Should().Throw<ArgumentOutOfRangeException>();
            valueReport.Should().Throw<ArgumentOutOfRangeException>();

            Action safeAction = () =>
                typeof(ProgressTrackerPane)
                    .GetMethod("SafeAction", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(
                        pane,
                        new object[] { new Action(() => throw new InvalidOperationException()) }
                    );

            safeAction.Should().NotThrow();
        }

        private static HeadlessPaneHarness CreateHeadlessPane(bool isRoot = false)
        {
            var pane = (ProgressTrackerPane)
                FormatterServices.GetUninitializedObject(typeof(ProgressTrackerPane));
            var reports = new List<(int Value, string JobName)>();
            var parentField = typeof(ProgressTrackerPane).GetField(
                "_parent",
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;
            var parent = Activator.CreateInstance(
                parentField.FieldType,
                new SynchronousProgress<(int Value, string JobName)>(reports.Add),
                100,
                0
            );

            parentField.SetValue(pane, parent);
            typeof(ProgressTrackerPane)
                .GetField("_progressViewer", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, null);
            typeof(ProgressTrackerPane)
                .GetField("_jobName", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, string.Empty);
            typeof(ProgressTrackerPane)
                .GetField("_progress", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, 0d);
            typeof(ProgressTrackerPane)
                .GetField("_isRoot", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, isRoot);
            typeof(ProgressTrackerPane)
                .GetField("_root100", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, false);
            return new HeadlessPaneHarness(pane, reports);
        }

        private sealed class HeadlessPaneHarness
        {
            public HeadlessPaneHarness(
                ProgressTrackerPane pane,
                List<(int Value, string JobName)> reports
            )
            {
                Pane = pane;
                Reports = reports;
            }

            public ProgressTrackerPane Pane { get; }

            public List<(int Value, string JobName)> Reports { get; }
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
