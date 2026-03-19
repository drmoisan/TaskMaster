using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class ProgressPackage_Tests
    {
        [TestMethod]
        public async Task InitializeAsync_ShouldUseProvidedTrackerTokenAndStopwatch()
        {
            // Arrange
            using var cancelSource = new CancellationTokenSource();
            var progressTracker = new ProgressTracker(cancelSource);
            var stopWatch = new SegmentStopWatch();
            var package = new ProgressPackage();

            // Act
            ProgressPackage result = await package.InitializeAsync(
                cancelSource,
                default,
                progressTracker,
                stopWatch,
                screen: null
            );

            // Assert
            result.Should().BeSameAs(package);
            package.CancelSource.Should().BeSameAs(cancelSource);
            package.Cancel.Should().Be(cancelSource.Token);
            package.ProgressTracker.Should().BeSameAs(progressTracker);
            package.StopWatch.Should().BeSameAs(stopWatch);
            package.ProgressTrackerPane.Should().BeNull();
        }

        [TestMethod]
        public async Task CreateAsTupleAsync_ShouldReturnProvidedDependencies()
        {
            // Arrange
            using var cancelSource = new CancellationTokenSource();
            var progressTracker = new ProgressTracker(cancelSource);
            var stopWatch = new SegmentStopWatch();

            // Act
            var tuple = await ProgressPackage.CreateAsTupleAsync(
                cancelSource,
                default,
                progressTracker,
                stopWatch,
                screen: null
            );

            // Assert
            tuple.CancelSource.Should().BeSameAs(cancelSource);
            tuple.Cancel.Should().Be(cancelSource.Token);
            tuple.ProgressTracker.Should().BeSameAs(progressTracker);
            tuple.StopWatch.Should().BeSameAs(stopWatch);
        }

        [TestMethod]
        public void SpawnChild_ShouldReuseSharedState_AndCreateChildProgressTracker()
        {
            // Arrange
            using var cancelSource = new CancellationTokenSource();
            var stopWatch = new SegmentStopWatch();
            var package = new ProgressPackage
            {
                CancelSource = cancelSource,
                Cancel = cancelSource.Token,
                StopWatch = stopWatch,
                ProgressTracker = new ProgressTracker(cancelSource),
            };

            // Act
            ProgressPackage child = package.SpawnChild(25);

            // Assert
            child.CancelSource.Should().BeSameAs(package.CancelSource);
            child.Cancel.Should().Be(package.Cancel);
            child.StopWatch.Should().BeSameAs(package.StopWatch);
            child.ProgressTracker.Should().NotBeNull();
            child.ProgressTrackerPane.Should().BeNull();
        }

        [TestMethod]
        public void ToTupleAndToTuplePane_ShouldExposeCurrentPropertyValues()
        {
            // Arrange
            using var cancelSource = new CancellationTokenSource();
            var progressTracker = new ProgressTracker(cancelSource);
            var stopWatch = new SegmentStopWatch();
            var package = new ProgressPackage
            {
                CancelSource = cancelSource,
                Cancel = cancelSource.Token,
                ProgressTracker = progressTracker,
                ProgressTrackerPane = null,
                StopWatch = stopWatch,
            };

            // Act
            var tuple = package.ToTuple();
            var paneTuple = package.ToTuplePane();

            // Assert
            tuple.CancelSource.Should().BeSameAs(cancelSource);
            tuple.Cancel.Should().Be(cancelSource.Token);
            tuple.ProgressTracker.Should().BeSameAs(progressTracker);
            tuple.StopWatch.Should().BeSameAs(stopWatch);
            paneTuple.CancelSource.Should().BeSameAs(cancelSource);
            paneTuple.Cancel.Should().Be(cancelSource.Token);
            paneTuple.ProgressTrackerPane.Should().BeNull();
            paneTuple.StopWatch.Should().BeSameAs(stopWatch);
        }
    }
}
