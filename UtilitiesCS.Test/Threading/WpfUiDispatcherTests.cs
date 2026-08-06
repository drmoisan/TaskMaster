using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public sealed class WpfUiDispatcherTests
    {
        [TestMethod]
        public async Task InjectedDispatcher_ExecutesInvokeBeginInvokeAndBothAsyncOverloads()
        {
            var host = new StaDispatcherHost();
            try
            {
                var dispatcher = new WpfUiDispatcher(host.Dispatcher);
                var invokeThreadId = -1;
                var beginInvokeThreadId = -1;

                dispatcher.Invoke(() => invokeThreadId = Thread.CurrentThread.ManagedThreadId);
                var beginInvokeResult = dispatcher.BeginInvoke(() =>
                    beginInvokeThreadId = Thread.CurrentThread.ManagedThreadId
                );
                await ((Task)beginInvokeResult);
                var syncResult = await dispatcher.InvokeAsync(() =>
                    Thread.CurrentThread.ManagedThreadId
                );
                var asyncResult = await dispatcher.InvokeAsync(async () =>
                {
                    var executedThreadId = Thread.CurrentThread.ManagedThreadId;
                    await Task.FromResult(true);
                    return executedThreadId;
                });

                invokeThreadId.Should().Be(host.ThreadId);
                beginInvokeThreadId.Should().Be(host.ThreadId);
                syncResult.Should().Be(host.ThreadId);
                asyncResult.Should().Be(host.ThreadId);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task InjectedDispatcher_PropagatesOriginalFaultsFromBothAsyncOverloads()
        {
            var host = new StaDispatcherHost();
            try
            {
                var dispatcher = new WpfUiDispatcher(host.Dispatcher);
                var synchronousFault = new InvalidOperationException(
                    "controlled synchronous fault"
                );
                var asynchronousFault = new InvalidOperationException(
                    "controlled asynchronous fault"
                );
                Func<Task> invokeSynchronous = async () =>
                    await dispatcher.InvokeAsync<int>(new Func<int>(() => throw synchronousFault));
                Func<Task> invokeAsynchronous = async () =>
                    await dispatcher.InvokeAsync<int>(
                        new Func<Task<int>>(() => Task.FromException<int>(asynchronousFault))
                    );

                (await invokeSynchronous.Should().ThrowAsync<InvalidOperationException>())
                    .Which.Should()
                    .BeSameAs(synchronousFault);
                (await invokeAsynchronous.Should().ThrowAsync<InvalidOperationException>())
                    .Which.Should()
                    .BeSameAs(asynchronousFault);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task InjectedDispatcher_CanceledBeforeDispatch_DoesNotExecuteAction()
        {
            var host = new StaDispatcherHost();
            try
            {
                using var source = new CancellationTokenSource();
                var dispatcher = new WpfUiDispatcher(host.Dispatcher);
                var executed = false;
                source.Cancel();
                Func<Task> invokeAction = async () =>
                    await dispatcher.InvokeAsync(
                        () => executed = true,
                        DispatcherPriority.Normal,
                        source.Token
                    );
                Func<Task> invokeSynchronousFunction = async () =>
                    await dispatcher.InvokeAsync<int>(
                        new Func<int>(() => throw new OperationCanceledException(source.Token))
                    );
                Func<Task> invokeAsyncFunction = async () =>
                    await dispatcher.InvokeAsync<int>(
                        new Func<Task<int>>(() => Task.FromCanceled<int>(source.Token))
                    );

                source.Token.IsCancellationRequested.Should().BeTrue();
                await invokeAction.Should().ThrowAsync<OperationCanceledException>();
                await invokeSynchronousFunction.Should().ThrowAsync<OperationCanceledException>();
                (await invokeAsyncFunction.Should().ThrowAsync<OperationCanceledException>())
                    .Which.CancellationToken.Should()
                    .Be(source.Token);
                executed.Should().BeFalse();
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        private sealed class StaDispatcherHost
        {
            private readonly AutoResetEvent _ready = new AutoResetEvent(false);
            private readonly TaskCompletionSource<bool> _stopped = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private readonly Thread _thread;

            internal StaDispatcherHost()
            {
                _thread = new Thread(() =>
                {
                    Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                    ThreadId = Thread.CurrentThread.ManagedThreadId;
                    _ready.Set();
                    try
                    {
                        Dispatcher.Run();
                    }
                    finally
                    {
                        _stopped.TrySetResult(true);
                    }
                });
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
                _ready.WaitOne();
            }

            internal Dispatcher Dispatcher { get; private set; }

            internal int ThreadId { get; private set; }

            internal async Task StopAsync()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                await _stopped.Task.ConfigureAwait(false);
                _thread.Join();
                if (_thread.IsAlive)
                {
                    throw new InvalidOperationException(
                        "The STA dispatcher thread did not terminate."
                    );
                }

                _ready.Dispose();
            }
        }
    }
}
