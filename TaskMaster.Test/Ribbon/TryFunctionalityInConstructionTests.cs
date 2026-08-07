using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Ribbon;
using UtilitiesCS;

namespace TaskMaster.Test.Ribbon
{
    [TestClass]
    public class TryFunctionalityInConstructionTests
    {
        [TestMethod]
        public async Task TryLoadFolderFilterAsync_AwaitsControlledInitialization()
        {
            var method = typeof(TryFunctionalityInConstruction).GetMethod(
                "TryLoadFolderFilterAsync",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null
            );

            method
                .Should()
                .NotBeNull("the ribbon path must expose an awaitable filter-loading seam");
            if (method is null)
            {
                return;
            }

            var controller = new TryFunctionalityInConstruction(null);
            var initializationSource = new TaskCompletionSource<bool>();
            var initializerField = typeof(TryFunctionalityInConstruction).GetField(
                "_loadFolderFilterAsync",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            initializerField
                .Should()
                .NotBeNull("the seam must permit deterministic initialization control");
            if (initializerField is null)
            {
                return;
            }

            initializerField.SetValue(
                controller,
                new Func<IApplicationGlobals, Task>(_ => initializationSource.Task)
            );
            var initialization = (Task)method.Invoke(controller, null);

            initialization.IsCompleted.Should().BeFalse();

            initializationSource.SetResult(true);
            await initialization;

            initialization.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        [TestMethod]
        public async Task TryLoadFolderFilter_PropagatesTheOriginalControlledInitializationFault()
        {
            var controller = new TryFunctionalityInConstruction(null);
            var originalException = new InvalidOperationException("controlled failure");
            var initializerField = typeof(TryFunctionalityInConstruction).GetField(
                "_loadFolderFilterAsync",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var legacyMethod = typeof(TryFunctionalityInConstruction).GetMethod(
                "TryLoadFolderFilter",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            initializerField.Should().NotBeNull();
            legacyMethod.Should().NotBeNull();
            if (initializerField is null || legacyMethod is null)
            {
                return;
            }

            initializerField.SetValue(
                controller,
                new Func<IApplicationGlobals, Task>(_ => Task.FromException(originalException))
            );

            var initialization = (Task)legacyMethod.Invoke(controller, null);
            Func<Task> awaitInitialization = async () => await initialization;

            var failure = await awaitInitialization
                .Should()
                .ThrowAsync<InvalidOperationException>();

            failure.Which.Should().BeSameAs(originalException);
        }

        [TestMethod]
        public async Task RibbonFolderFilterCallback_ReportsOriginalFaultExactlyOnceAfterInitializationCompletes()
        {
            var originalException = new InvalidOperationException(
                "controlled ribbon folder-filter failure"
            );
            var initializationSource = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var reportedFailures = new List<Exception>();
            var observedFailure = new TaskCompletionSource<Exception>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var viewer = new RibbonViewer(
                () => initializationSource.Task,
                exception =>
                {
                    reportedFailures.Add(exception);
                    observedFailure.TrySetResult(exception);
                }
            );

            viewer.RunFolderFilterCallback();

            reportedFailures.Should().BeEmpty("the incomplete initialization has not faulted");

            initializationSource.SetException(originalException);
            var observed = await observedFailure.Task;
            observed.Should().BeSameAs(originalException);
            reportedFailures.Should().ContainSingle().Which.Should().BeSameAs(originalException);
        }

        [TestMethod]
        public async Task RibbonFolderFilterCallback_DelayedSuccessfulInitializationDoesNotReportFailure()
        {
            var initializationSource = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var initializationCompleted = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var failureCount = 0;
            var viewer = new RibbonViewer(
                async () =>
                {
                    await initializationSource.Task;
                    initializationCompleted.TrySetResult(true);
                },
                _ => failureCount++
            );

            viewer.RunFolderFilterCallback();

            failureCount.Should().Be(0);

            initializationSource.SetResult(true);
            await initializationCompleted.Task;

            failureCount.Should().Be(0);
        }

        [TestMethod]
        public async Task RibbonFolderFilterCallback_ContainsThrowingFailureReporter()
        {
            var originalException = new InvalidOperationException(
                "controlled ribbon folder-filter failure"
            );
            var initializationSource = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var reporterInvocation = new TaskCompletionSource<Exception>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var viewer = new RibbonViewer(
                () => initializationSource.Task,
                exception =>
                {
                    reporterInvocation.TrySetResult(exception);
                    throw new InvalidOperationException("reporter failure");
                }
            );

            viewer.RunFolderFilterCallback();
            initializationSource.SetException(originalException);

            var reportedException = await reporterInvocation.Task;
            reportedException.Should().BeSameAs(originalException);
        }
    }
}
