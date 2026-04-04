using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.TaskPane;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class ProgressTrackerPane_Tests
    {
        [TestMethod]
        [STAThread]
        public void Constructor_AndRootFlows_InitializeViewerAndSupportChildSpawns()
        {
            var priorContext = SynchronizationContext.Current;
            var dispatcherField = typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            )!;
            var priorDispatcher = (Dispatcher)dispatcherField.GetValue(null);
            SynchronizationContext.SetSynchronizationContext(new ImmediateSynchronizationContext());

            try
            {
                dispatcherField.SetValue(null, Dispatcher.CurrentDispatcher);
                using var cts = new CancellationTokenSource();
                var pane = new ProgressTrackerPane(cts);

                try
                {
                    pane.Progress.Should().Be(0);
                    pane.ProgressViewer.Should().NotBeNull();
                    pane.ProgressViewer.JobName.Text.Should().Be("Initializing");
                    pane.ProgressViewer.Bar.Value.Should().Be(0);

                    pane.Increment(25, "Loading");
                    pane.Progress.Should().Be(25);
                    pane.ProgressViewer.JobName.Text.Should().Be("Loading");
                    pane.ProgressViewer.Bar.Value.Should().Be(25);

                    pane.Report((50, "Tuple"));
                    pane.Progress.Should().Be(50);
                    pane.ProgressViewer.JobName.Text.Should().Be("Tuple");
                    pane.ProgressViewer.Bar.Value.Should().Be(50);

                    pane.Report(100, "Complete");
                    pane.ProgressViewer.Bar.BackColor.Should().Be(Color.Green);
                    pane.Report(100, "Complete again");
                    pane.ProgressViewer.Bar.BackColor.Should().Be(Color.Blue);
                    pane.Report(150, "Overflow");
                    pane.ProgressViewer.Bar.BackColor.Should().Be(Color.Green);
                    pane.Report(101);

                    var intChild = pane.SpawnChild(15);
                    var doubleChild = pane.SpawnChild(12.7);
                    var remainingChild = pane.SpawnChild();

                    intChild.ProgressViewer.Should().BeSameAs(pane.ProgressViewer);
                    doubleChild.ProgressViewer.Should().BeSameAs(pane.ProgressViewer);
                    remainingChild.ProgressViewer.Should().BeSameAs(pane.ProgressViewer);
                    pane.Progress.Should().Be(100);
                    pane.ProgressViewer.Bar.BackColor.Should().Be(Color.Blue);
                }
                finally
                {
                    pane.ProgressViewer.Dispose();
                }
            }
            finally
            {
                dispatcherField.SetValue(null, priorDispatcher);
                SynchronizationContext.SetSynchronizationContext(priorContext);
            }
        }

        [TestMethod]
        [STAThread]
        public void ReportAndSafeAction_RejectNegativeValuesAndIgnoreDisposedViewer()
        {
            var priorContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new ImmediateSynchronizationContext());

            try
            {
                using var viewer = new ProgressPane();
                var pane = CreateHeadlessPane(viewer);
                using var replacementViewer = new ProgressPane();
                Action namedReport = () => pane.Report(-1, "bad");
                Action valueReport = () => pane.Report(-1);

                typeof(ProgressTrackerPane)
                    .GetProperty(
                        nameof(ProgressTrackerPane.ProgressViewer),
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    )!
                    .SetValue(pane, replacementViewer);

                pane.ProgressViewer.Should().BeSameAs(replacementViewer);
                namedReport.Should().Throw<ArgumentOutOfRangeException>();
                valueReport.Should().Throw<ArgumentOutOfRangeException>();

                replacementViewer.Dispose();
                viewer.Dispose();

                Action safeAction = () =>
                    typeof(ProgressTrackerPane)
                        .GetMethod("SafeAction", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(
                            pane,
                            new object[] { new Action(() => throw new InvalidOperationException()) }
                        );

                safeAction.Should().NotThrow();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(priorContext);
            }
        }

        private static ProgressTrackerPane CreateHeadlessPane(ProgressPane viewer)
        {
            var pane = (ProgressTrackerPane)
                FormatterServices.GetUninitializedObject(typeof(ProgressTrackerPane));
            var parentField = typeof(ProgressTrackerPane).GetField(
                "_parent",
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;
            var parent = Activator.CreateInstance(
                parentField.FieldType,
                new Progress<(int Value, string JobName)>(_ => { }),
                100,
                0
            );

            parentField.SetValue(pane, parent);
            typeof(ProgressTrackerPane)
                .GetField("_progressViewer", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, viewer);
            typeof(ProgressTrackerPane)
                .GetField("_jobName", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, string.Empty);
            typeof(ProgressTrackerPane)
                .GetField("_progress", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, 0d);
            return pane;
        }

        private sealed class ImmediateSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object state) => d(state);

            public override void Send(SendOrPostCallback d, object state) => d(state);
        }
    }
}
