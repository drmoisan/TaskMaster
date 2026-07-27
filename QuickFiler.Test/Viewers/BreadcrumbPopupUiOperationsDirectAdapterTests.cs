using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    [TestClass]
    public sealed class BreadcrumbPopupUiOperationsDirectAdapterTests
    {
        [TestMethod]
        public void CoreProbe_AbsentAndPresentPaths()
        {
            using (var fixture = new PopupFixture())
            {
                Task<object> present = fixture.Operations.ReadRequiredAsync(
                    () => fixture.PresentCore,
                    "missing"
                );
                fixture.Queue.DrainOnCreatorThread();
                present.Result.Should().BeSameAs(fixture.PresentCore);

                Task<object> absent = fixture.Operations.ReadRequiredAsync<object>(
                    () => null,
                    "missing"
                );
                fixture.Queue.DrainOnCreatorThread();
                ((Action)(() => absent.GetAwaiter().GetResult()))
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("missing");
            }
        }

        [TestMethod]
        public void Initializer_ThrowAndNullTaskPaths()
        {
            using (var fixture = new PopupFixture())
            {
                Task<Task> throwing = fixture.Operations.BeginInitializationAsync(() =>
                    throw new InvalidOperationException("initialize")
                );
                fixture.Queue.DrainOnCreatorThread();
                ((Action)(() => throwing.GetAwaiter().GetResult()))
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("initialize");

                Task<Task> nullTask = fixture.Operations.BeginInitializationAsync(() => null);
                fixture.Queue.DrainOnCreatorThread();
                ((Action)(() => nullTask.GetAwaiter().GetResult()))
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*no completion task*");
            }
        }

        [TestMethod]
        public void MessengerConstructionFailure_DisposesReadiness()
        {
            int detaches = 0;
            var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++);

            Action create = () =>
                BreadcrumbPopupLifecycleOperations.CreateNavigationSurface(
                    readiness,
                    () => throw new InvalidOperationException("messenger")
                );

            create.Should().Throw<InvalidOperationException>().WithMessage("messenger");
            readiness.Completion.IsCanceled.Should().BeTrue();
            detaches.Should().Be(1);
        }

        [TestMethod]
        public void NavigationBinder_TranslatesDetachesAndCleansOnThrow()
        {
            using (var fixture = new PopupFixture())
            {
                var binding = new RecordingNavigationBinding();
                BreadcrumbNavigationReadiness readiness =
                    BreadcrumbPopupLifecycleOperations.NavigateWithSubscription(
                        fixture.Dispatcher,
                        "Popup",
                        () => { },
                        binding.Create
                    );
                binding.Start(7);
                fixture.Queue.DrainOnCreatorThread();
                binding.Complete(7, true, "none");
                fixture.Queue.DrainOnCreatorThread();
                readiness.Completion.Status.Should().Be(TaskStatus.RanToCompletion);
                binding.DetachCount.Should().Be(1);

                var failingBinding = new RecordingNavigationBinding();
                Action fail = () =>
                    BreadcrumbPopupLifecycleOperations.NavigateWithSubscription(
                        fixture.Dispatcher,
                        "Popup",
                        () => throw new InvalidOperationException("navigate"),
                        failingBinding.Create
                    );
                fail.Should().Throw<InvalidOperationException>().WithMessage("navigate");
                failingBinding.DetachCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup()
        {
            var calls = new List<string>();

            Action cleanup = () =>
                BreadcrumbPopupLifecycleOperations.DisposeTwoResources(
                    () =>
                    {
                        calls.Add("messenger");
                        throw new InvalidOperationException("first");
                    },
                    () =>
                    {
                        calls.Add("control");
                        throw new InvalidOperationException("second");
                    }
                );

            cleanup.Should().Throw<InvalidOperationException>().WithMessage("first");
            calls.Should().Equal("messenger", "control");
        }

        private sealed class PopupFixture : IDisposable
        {
            internal PopupFixture()
            {
                Queue = new QueuedCreatorThreadSynchronizationContext();
                Dispatcher = new BreadcrumbUiDispatcher(Queue, _ => { });
                Operations = new BreadcrumbPopupUiOperations(Dispatcher);
                PresentCore = new object();
            }

            internal QueuedCreatorThreadSynchronizationContext Queue { get; }
            internal BreadcrumbUiDispatcher Dispatcher { get; }
            internal BreadcrumbPopupUiOperations Operations { get; }
            internal object PresentCore { get; }

            public void Dispose() { }
        }

        private sealed class RecordingNavigationBinding
        {
            private Action<ulong> _started;
            private Action<ulong, bool, string> _completed;
            private Action _ownerDisposed;

            internal int DetachCount { get; private set; }

            internal BreadcrumbNavigationSubscription Create(
                Action<ulong> started,
                Action<ulong, bool, string> completed,
                Action ownerDisposed
            )
            {
                _started = started;
                _completed = completed;
                _ownerDisposed = ownerDisposed;
                return new BreadcrumbNavigationSubscription(() => DetachCount++);
            }

            internal void Start(ulong navigationId) => _started(navigationId);

            internal void Complete(ulong navigationId, bool success, string status) =>
                _completed(navigationId, success, status);

            internal void OwnerDisposed() => _ownerDisposed();
        }

        private sealed class QueuedCreatorThreadSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> _callbacks =
                new Queue<Tuple<SendOrPostCallback, object>>();

            internal QueuedCreatorThreadSynchronizationContext()
            {
                CreatorThreadId = Environment.CurrentManagedThreadId;
            }

            internal int CreatorThreadId { get; }
            internal List<int> CallbackThreads { get; } = new List<int>();

            public override void Post(SendOrPostCallback callback, object state) =>
                _callbacks.Enqueue(Tuple.Create(callback, state));

            internal void DrainOnCreatorThread()
            {
                Environment.CurrentManagedThreadId.Should().Be(CreatorThreadId);
                while (_callbacks.Count > 0)
                {
                    Tuple<SendOrPostCallback, object> callback = _callbacks.Dequeue();
                    CallbackThreads.Add(Environment.CurrentManagedThreadId);
                    callback.Item1(callback.Item2);
                }
            }
        }
    }
}
