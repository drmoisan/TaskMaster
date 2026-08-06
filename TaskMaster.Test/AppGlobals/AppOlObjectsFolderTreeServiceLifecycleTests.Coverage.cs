using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.Threading;
using DispatchMode = TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceLifecycleTests.DispatchMode;
using Lifecycle = TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceLifecycleTests;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster.Test.AppGlobals
{
    public sealed partial class AppOlObjectsFolderTreeServiceLifecycleTests
    {
        [TestMethod]
        public void BaseDispatcherHooks_AreCallableWithoutOutlookAccess()
        {
            using var probe = new BaseHooksProbe();
            var dispatcher = new ControlledUiDispatcher(DispatchMode.Immediate);

            probe.IsDispatcherThread(dispatcher).Should().BeFalse();
            probe.CreateDispatcher().Should().BeOfType<WpfUiDispatcher>();
            probe.InvokeCompositionStarting();
            probe.InvokeBeforeCompletion(null);
            probe.InvokeTerminal(Task.FromResult<IOutlookFolderTreeService>(null));
        }

        [TestMethod]
        public void BaseLoadFolderTreeService_ComposesWithMockedEmptyStores()
        {
            var application = new Mock<Outlook.Application>(MockBehavior.Strict);
            var namespaceMapi = new Mock<Outlook.NameSpace>(MockBehavior.Strict);
            application.SetupGet(value => value.Application).Returns(application.Object);
            application.Setup(value => value.GetNamespace("MAPI")).Returns(namespaceMapi.Object);
            namespaceMapi.SetupGet(value => value.Stores).Returns((Outlook.Stores)null);
            using var sut = new BaseLoadProbe(application.Object, Mock.Of<IApplicationGlobals>());
            sut.StoresWrapper = new StoresWrapper();

            var service = sut.FolderTreeService;

            service.Should().NotBeNull();
            sut.CompositionStartingCount.Should().Be(1);
            sut.BeforeCompletionCount.Should().Be(1);
            sut.TerminalCount.Should().Be(1);
        }

        [TestMethod]
        public async Task SetupAndLoadFailures_ResetOwnershipForAOneServiceRetry()
        {
            await VerifySetupFailureAndRetryAsync(SetupMode.NullDispatcher);
            await VerifySetupFailureAndRetryAsync(SetupMode.DispatcherFactory);
            await VerifySetupFailureAndRetryAsync(SetupMode.DispatcherThreadCheck);

            var dispatcher = new ControlledUiDispatcher(DispatchMode.Immediate);
            using var sut = CreateSut(dispatcher);
            var failure = new InvalidOperationException("controlled load failure");
            sut.CandidateFactory = _ => throw failure;

            GetException(() => _ = sut.FolderTreeService).Should().BeSameAs(failure);

            sut.CandidateFactory = null;
            sut.FolderTreeService.Should().BeSameAs(sut.Service);
            sut.LoadCount.Should().Be(2);
        }

        [TestMethod]
        public async Task DispatchAndCandidateDisposalFailures_PreserveTerminalBehavior()
        {
            await VerifyCompositionFailureRetryAsync();
            await VerifyCandidateOwnershipAsync(CandidateScenario.IndependentDiscard);
            VerifyDisposedGetter(publishFirst: false);
        }

        [TestMethod]
        public void SynchronousDispatcherThrow_PreservesExceptionIdentityAndPermitsRetry()
        {
            var failure = new InvalidOperationException("synchronous dispatcher failure");
            var dispatcher = new ThrowOnceDispatcher(failure);
            var service = new Mock<IOutlookFolderTreeService>(MockBehavior.Strict);
            service.Setup(value => value.Dispose());
            using var sut = new ThrowingDispatcherProbe(dispatcher, service.Object);

            GetException(() => _ = sut.FolderTreeService).Should().BeSameAs(failure);

            dispatcher.ThrowOnInvoke = false;
            sut.FolderTreeService.Should().BeSameAs(service.Object);
            dispatcher.InvokeCount.Should().Be(2);
        }

        [TestMethod]
        public async Task DiscardCandidate_SinkDisposeFailureIsContained()
        {
            var namespaceMapi = new Mock<Outlook.NameSpace>(MockBehavior.Strict);
            namespaceMapi.SetupGet(value => value.Stores).Returns((Outlook.Stores)null);
            var sink = new OutlookFolderNotificationSink(namespaceMapi.Object);
            var disposeEvents = 0;
            sink.Disposed += (_, _) =>
            {
                Interlocked.Increment(ref disposeEvents);
                throw new InvalidOperationException("sink dispose failure");
            };
            var service = new Mock<IOutlookFolderTreeService>(MockBehavior.Strict);
            service.Setup(value => value.Dispose());
            var dispatcher = new ControlledUiDispatcher(DispatchMode.Pending);
            var sut = CreateSut(dispatcher, service: service.Object);
            sut.CandidateFactory = _ => (service.Object, sink);
            sut.BeforeCompletion = _ => sut.Dispose();
            var run = await StartWorkerAsync(sut, dispatcher);
            try
            {
                await run.Operation.ReleaseAsync();
                var terminalException = await GetExceptionAsync(await run.Terminal);
                AssertSameObjectDisposed(await GetExceptionAsync(run.Worker), terminalException);
                service.Verify(value => value.Dispose(), Times.Once);
                Volatile.Read(ref disposeEvents).Should().Be(1);
            }
            finally
            {
                await CleanupAsync(sut, run.Operation, run.Worker);
            }
        }

        private sealed class BaseHooksProbe : AppOlObjects
        {
            internal BaseHooksProbe()
                : base(null, null) { }

            internal IUiDispatcher CreateDispatcher() => base.CreateFolderTreeServiceDispatcher();

            internal bool IsDispatcherThread(IUiDispatcher dispatcher) =>
                base.IsFolderTreeServiceDispatcherThread(dispatcher);

            internal void InvokeCompositionStarting() =>
                base.OnFolderTreeServiceCompositionStarting();

            internal void InvokeBeforeCompletion(IOutlookFolderTreeService service) =>
                base.OnFolderTreeServiceBeforeInitializationCompletion(service);

            internal void InvokeTerminal(Task<IOutlookFolderTreeService> initialization) =>
                base.OnFolderTreeServiceInitializationTerminal(initialization);
        }

        private sealed class BaseLoadProbe : AppOlObjects
        {
            internal BaseLoadProbe(Outlook.Application application, IApplicationGlobals globals)
                : base(application, globals) { }

            internal int CompositionStartingCount { get; private set; }

            internal int BeforeCompletionCount { get; private set; }

            internal int TerminalCount { get; private set; }

            protected internal override IUiDispatcher CreateFolderTreeServiceDispatcher() =>
                new ControlledUiDispatcher(DispatchMode.Immediate);

            protected internal override bool IsFolderTreeServiceDispatcherThread(
                IUiDispatcher dispatcher
            ) => true;

            protected internal override void OnFolderTreeServiceCompositionStarting() =>
                CompositionStartingCount++;

            protected internal override void OnFolderTreeServiceBeforeInitializationCompletion(
                IOutlookFolderTreeService service
            ) => BeforeCompletionCount++;

            protected internal override void OnFolderTreeServiceInitializationTerminal(
                Task<IOutlookFolderTreeService> initialization
            ) => TerminalCount++;
        }

        private sealed class ThrowingDispatcherProbe : AppOlObjects
        {
            private readonly IUiDispatcher _dispatcher;
            private readonly IOutlookFolderTreeService _service;

            internal ThrowingDispatcherProbe(
                IUiDispatcher dispatcher,
                IOutlookFolderTreeService service
            )
                : base(null, Mock.Of<IApplicationGlobals>())
            {
                _dispatcher = dispatcher;
                _service = service;
            }

            protected internal override IUiDispatcher CreateFolderTreeServiceDispatcher() =>
                _dispatcher;

            protected internal override bool IsFolderTreeServiceDispatcherThread(
                IUiDispatcher dispatcher
            ) => false;

            protected internal override IOutlookFolderTreeService LoadFolderTreeService(
                IUiDispatcher dispatcher,
                out OutlookFolderNotificationSink notificationSink
            )
            {
                notificationSink = null;
                return _service;
            }
        }

        private sealed class ThrowOnceDispatcher : IUiDispatcher
        {
            private readonly Exception _failure;

            internal ThrowOnceDispatcher(Exception failure)
            {
                _failure = failure;
                ThrowOnInvoke = true;
            }

            internal int InvokeCount { get; private set; }
            internal bool ThrowOnInvoke { get; set; }

            public void Invoke(Action action) => action();

            public Task InvokeAsync(Action action)
            {
                InvokeCount++;
                if (ThrowOnInvoke)
                    throw _failure;

                action();
                return Task.CompletedTask;
            }

            public Task InvokeAsync(
                Action action,
                DispatcherPriority priority,
                CancellationToken token
            )
            {
                token.ThrowIfCancellationRequested();
                return InvokeAsync(action);
            }

            public IAsyncResult BeginInvoke(Action action)
            {
                action();
                return Task.CompletedTask;
            }

            public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
                Task.FromResult(func());

            public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func) => func();
        }
    }
}
