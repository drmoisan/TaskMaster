using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public class ProgressTrackerAsync_Tests
    {
        [TestMethod]
        public void Constructor_WithTokenSource_ShouldSetDefaultProperties()
        {
            var cts = new CancellationTokenSource();

            var tracker = new ProgressTrackerAsync(cts);

            tracker.Allocation.Should().Be(100);
            tracker.StartingAt.Should().Be(0);
            tracker.ProgressViewer.Should().BeNull();
            tracker.UiDispatcher.Should().BeNull();
        }

        [TestMethod]
        public void Allocation_ShouldBeSettable()
        {
            var tracker = new ProgressTrackerAsync(new CancellationTokenSource());

            tracker.Allocation = 50;

            tracker.Allocation.Should().Be(50);
        }

        [TestMethod]
        public void StartingAt_ShouldBeSettable()
        {
            var tracker = new ProgressTrackerAsync(new CancellationTokenSource());

            tracker.StartingAt = 25;

            tracker.StartingAt.Should().Be(25);
        }

        [TestMethod]
        public void JobName_ShouldBeSettable()
        {
            var tracker = new ProgressTrackerAsync(new CancellationTokenSource());

            tracker.JobName = "Processing";

            tracker.JobName.Should().Be("Processing");
        }
    }
}
