using System;
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

        /// <summary>
        /// Issue #872 AC4. A package that constructed its own cancellation token source owns it, so
        /// disposing the package releases it. Release is probed through the token getter, which
        /// throws once the source is disposed; no timer and no finalizer is involved. The tracker
        /// overload is selected by naming the progressTracker parameter, and a non-null tracker plus
        /// an explicit stop watch keep the run headless and deterministic.
        /// </summary>
        [TestMethod]
        public async Task Dispose_WhenPackageConstructedTheSource_ReleasesIt()
        {
            // Arrange
            using var trackerSource = new CancellationTokenSource();
            var progressTracker = new ProgressTracker(trackerSource);
            var stopWatch = new SegmentStopWatch();
            var package = new ProgressPackage();
            await package.InitializeAsync(
                null,
                default,
                progressTracker: progressTracker,
                stopWatch: stopWatch
            );
            var constructed = package.CancelSource;
            constructed
                .Should()
                .NotBeSameAs(trackerSource, "the package constructed its own source");

            // Act
            package.Dispose();

            // Assert
            Action readToken = () => _ = constructed.Token;
            readToken
                .Should()
                .Throw<ObjectDisposedException>(
                    "a source the package owns is released on disposal"
                );
        }

        /// <summary>
        /// Issue #872 AC4. A caller-supplied cancellation token source belongs to that caller and
        /// must never be released by the package, so it stays usable after the package is disposed.
        /// </summary>
        [TestMethod]
        public async Task Dispose_WhenCallerSuppliedTheSource_LeavesItUsable()
        {
            // Arrange
            using var cancelSource = new CancellationTokenSource();
            var progressTracker = new ProgressTracker(cancelSource);
            var stopWatch = new SegmentStopWatch();
            var package = new ProgressPackage();
            await package.InitializeAsync(
                cancelSource,
                default,
                progressTracker: progressTracker,
                stopWatch: stopWatch
            );

            // Act
            package.Dispose();

            // Assert
            Action readToken = () => _ = cancelSource.Token;
            readToken.Should().NotThrow("a borrowed source is never released by the borrower");
            cancelSource.Cancel();
            cancelSource
                .IsCancellationRequested.Should()
                .BeTrue("the caller's source is still fully usable after the package is disposed");
        }

        /// <summary>
        /// Issue #872 AC4. Ownership is never transferred by assignment, so a child that received the
        /// parent's source through the property setter owns nothing and its disposal is a no-op on
        /// that source.
        /// </summary>
        [TestMethod]
        public async Task Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource()
        {
            // Arrange
            using var trackerSource = new CancellationTokenSource();
            var progressTracker = new ProgressTracker(trackerSource);
            var stopWatch = new SegmentStopWatch();
            var parent = new ProgressPackage();
            await parent.InitializeAsync(
                null,
                default,
                progressTracker: progressTracker,
                stopWatch: stopWatch
            );
            var parentSource = parent.CancelSource;
            ProgressPackage child = parent.SpawnChild(25);

            // Act
            child.Dispose();

            // Assert
            Action readToken = () => _ = parentSource.Token;
            readToken.Should().NotThrow("a spawned child owns nothing it did not construct");
        }
    }
}
