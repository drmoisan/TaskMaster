#nullable enable
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler.Viewers
{
    /// <summary>Identifies one cancellable popup-open generation.</summary>
    internal readonly struct BreadcrumbDropDownOpenLease
    {
        internal BreadcrumbDropDownOpenLease(long generation, Task cancellation)
        {
            Generation = generation;
            Cancellation = cancellation;
        }

        internal long Generation { get; }
        internal Task Cancellation { get; }
    }

    /// <summary>Owns shared popup-open generation, cancellation, and owner scheduling.</summary>
    internal sealed class BreadcrumbDropDownOpenLifetime : IDisposable
    {
        private readonly object _sync = new object();
        private readonly BreadcrumbDropDownHost _host;
        private readonly BreadcrumbPopupUiOperations _uiOperations;
        private TaskCompletionSource<bool> _cancellation = NewCompletionSource();
        private TaskCompletionSource<bool>? _openCompletion;
        private volatile TaskCompletionSource<bool>? _pendingCloseCompletion;
        private Task<bool>? _openTask;
        private long _generation;
        private bool _disposed;

        internal BreadcrumbDropDownOpenLifetime(
            BreadcrumbDropDownHost host,
            BreadcrumbPopupUiOperations uiOperations
        )
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _uiOperations = uiOperations ?? throw new ArgumentNullException(nameof(uiOperations));
        }

        internal Task<bool> OpenAsync(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize
        )
        {
            TaskCompletionSource<bool> completion,
                canceled;
            BreadcrumbDropDownOpenLease lease;
            lock (_sync)
            {
                if (_openTask != null)
                    return _openTask;

                canceled = InvalidateCore();
                lease = new BreadcrumbDropDownOpenLease(_generation, _cancellation.Task);
                completion = NewCompletionSource();
                _openCompletion = completion;
                _openTask = completion.Task;
            }
            canceled.TrySetResult(true);

            Task<Task<bool>> kickoff = RunOnOwnerAsync(() =>
                OpenCoreAsync(anchorScreenBounds, workingArea, desiredSize, lease)
            );
            _ = CompleteOpenAsync(kickoff, lease, completion);
            return completion.Task;
        }

        internal bool TryCancelPendingOpen(Action closeOperation)
        {
            _ = closeOperation ?? throw new ArgumentNullException(nameof(closeOperation));
            TaskCompletionSource<bool> completion,
                canceled;
            lock (_sync)
            {
                if (_disposed || _openCompletion == null || _pendingCloseCompletion != null)
                    return false;
                completion = _openCompletion;
                _pendingCloseCompletion = completion;
                canceled = InvalidateCore();
            }
            canceled.TrySetResult(true);
            Task<Task> kickoff = RunOnOwnerAsync(() =>
            {
                closeOperation();
                return Task.CompletedTask;
            });
            _ = CompletePendingCloseAsync(kickoff, completion);
            return true;
        }

        internal bool IsCurrent(BreadcrumbDropDownOpenLease lease)
        {
            lock (_sync)
                return IsCurrentCore(lease, allowDisposed: false);
        }

        internal bool IsPendingClose => _pendingCloseCompletion != null;

        internal void Schedule(Action operation)
        {
            Schedule(() =>
            {
                operation();
                return Task.CompletedTask;
            });
        }

        internal void Schedule(Func<Task> operation)
        {
            BreadcrumbDropDownOpenLease lease;
            lock (_sync)
            {
                if (_disposed)
                    return;
                lease = new BreadcrumbDropDownOpenLease(_generation, _cancellation.Task);
            }
            ScheduleObserved(() =>
                IsLifecycleCurrent(lease, allowDisposed: false) ? operation() : Task.CompletedTask
            );
        }

        internal void InvalidateAndSchedule(Action operation) =>
            InvalidateAndSchedule(() =>
            {
                operation();
                return Task.CompletedTask;
            });

        internal void InvalidateAndSchedule(Func<Task> operation) =>
            ScheduleInvalidating(operation, disposing: false);

        internal void DisposeAndSchedule(Func<Task> operation) =>
            ScheduleInvalidating(operation, disposing: true);

        public void Dispose()
        {
            TaskCompletionSource<bool> canceled;
            lock (_sync)
            {
                if (_disposed)
                    return;
                _disposed = true;
                canceled = InvalidateCore();
            }
            canceled.TrySetResult(true);
        }

        private async Task CompleteOpenAsync(
            Task<Task<bool>> kickoff,
            BreadcrumbDropDownOpenLease lease,
            TaskCompletionSource<bool> completion
        )
        {
            bool result = false;
            try
            {
                Task<bool> running = await kickoff.ConfigureAwait(false);
                bool opened = await running.ConfigureAwait(false);
                result = opened && IsCurrent(lease);
            }
            catch (Exception exception)
            {
                await HandleOpenFailureAsync(exception, lease).ConfigureAwait(false);
            }
            finally
            {
                lock (_sync)
                {
                    if (!ReferenceEquals(_pendingCloseCompletion, completion))
                    {
                        if (ReferenceEquals(_openCompletion, completion))
                        {
                            _openCompletion = null;
                            _openTask = null;
                        }
                        completion.TrySetResult(result && IsCurrentCore(lease, false));
                    }
                }
            }
        }

        private async Task CompletePendingCloseAsync(
            Task<Task> kickoff,
            TaskCompletionSource<bool> completion
        )
        {
            try
            {
                Task running = await kickoff.ConfigureAwait(false);
                await running.ConfigureAwait(false);
            }
            catch { }
            finally
            {
                lock (_sync)
                {
                    if (ReferenceEquals(_openCompletion, completion))
                    {
                        _openCompletion = null;
                        _openTask = null;
                    }
                    if (ReferenceEquals(_pendingCloseCompletion, completion))
                        _pendingCloseCompletion = null;
                }
                completion.TrySetResult(false);
            }
        }

        private async Task<bool> OpenCoreAsync(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize,
            BreadcrumbDropDownOpenLease lease
        )
        {
            try
            {
                if (!await EnsureSurfaceAsync(lease).ConfigureAwait(false))
                    return false;

                BreadcrumbPopupPlacementResult? placement = await _uiOperations
                    .PlaceSurfaceAsync(
                        _host.DropDown,
                        _host.InstalledControlHost!,
                        _host._popupControl!,
                        anchorScreenBounds,
                        workingArea,
                        desiredSize,
                        () => IsCurrent(lease)
                    )
                    .ConfigureAwait(false);
                if (!placement.HasValue)
                    return false;

                bool shown = await _uiOperations
                    .RunAsync(() => ShowCurrentSurface(placement.Value, lease))
                    .ConfigureAwait(false);
                if (!shown)
                    return false;
                return await _uiOperations
                    .RunAsync(() => FocusCurrentSurface(lease))
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await HandleOpenFailureAsync(exception, lease).ConfigureAwait(false);
                return false;
            }
        }

        private bool ShowCurrentSurface(
            BreadcrumbPopupPlacementResult placement,
            BreadcrumbDropDownOpenLease lease
        ) =>
            RunIfCurrent(lease, () => ValidatePlacement(placement))
            && RunIfCurrent(
                lease,
                () =>
                {
                    _host.OpenState = true;
                    return true;
                }
            )
            && RunIfCurrent(
                lease,
                () =>
                {
                    _host.ShowPopup(placement.Bounds.Location);
                    return IsCurrent(lease) && _host.OpenState;
                }
            );

        private bool ValidatePlacement(BreadcrumbPopupPlacementResult placement)
        {
            if (placement.Bounds.Width == 0 || placement.Bounds.Height == 0)
                throw new InvalidOperationException(
                    "The active working area has no space for the folder selector popup."
                );
            return true;
        }

        private bool FocusCurrentSurface(BreadcrumbDropDownOpenLease lease) =>
            RunIfCurrent(
                lease,
                () =>
                {
                    if (!_host.OpenState)
                        return false;
                    _host.FocusPending();
                    return IsCurrent(lease) && _host.OpenState;
                }
            )
            && RunIfCurrent(
                lease,
                () =>
                {
                    _host.LastInitializationException = null;
                    return true;
                }
            );

        private async Task<bool> EnsureSurfaceAsync(BreadcrumbDropDownOpenLease lease)
        {
            if (_host.HasInstalledSurface)
                return await _uiOperations.RunAsync(() => IsCurrent(lease)).ConfigureAwait(false);

            Tuple<ToolStripControlHost, Control, IWebViewMessenger>? installed = null;
            try
            {
                installed = await _uiOperations
                    .CreateAndInstallSurfaceAsync(
                        _host.SurfaceFactory,
                        _host.Environment,
                        _host.DropDown,
                        () => IsCurrent(lease),
                        lease.Cancellation
                    )
                    .ConfigureAwait(false);
                if (installed == null)
                    return false;

                bool? retained = await _uiOperations
                    .RunAsync(() => RetainCurrentSurface(installed, lease))
                    .ConfigureAwait(false);
                if (retained == true)
                    return true;
                if (!retained.HasValue)
                {
                    await _uiOperations
                        .DisposeHostedSurfaceAsync(
                            _host.DropDown,
                            installed.Item1,
                            installed.Item2,
                            installed.Item3
                        )
                        .ConfigureAwait(false);
                }
                return false;
            }
            catch
            {
                try
                {
                    if (installed != null || IsCurrent(lease))
                        await _host
                            .DisposeSurfaceAfterFailureAsync(installed)
                            .ConfigureAwait(false);
                }
                catch (Exception cleanupFailure)
                {
                    _uiOperations.Report(cleanupFailure);
                }
                throw;
            }
        }

        private bool? RetainCurrentSurface(
            Tuple<ToolStripControlHost, Control, IWebViewMessenger> installed,
            BreadcrumbDropDownOpenLease lease
        )
        {
            if (!IsCurrent(lease))
                return null;
            _host.InstalledControlHost = installed.Item1;
            _host._popupControl = installed.Item2;
            _host._popupMessenger = installed.Item3;
            _host.PublishPopupMessengerReady();
            return IsCurrent(lease);
        }

        private bool RunIfCurrent(BreadcrumbDropDownOpenLease lease, Func<bool> operation) =>
            IsCurrent(lease) && operation();

        private async Task HandleOpenFailureAsync(
            Exception exception,
            BreadcrumbDropDownOpenLease lease
        )
        {
            try
            {
                await _uiOperations
                    .RunAsync(
                        () =>
                        {
                            if (!IsCurrent(lease))
                                return;
                            _host.LastInitializationException = exception;
                            _host.RestoreAfterOpenFailure();
                        },
                        reportFailure: false
                    )
                    .ConfigureAwait(false);
            }
            catch (Exception rollbackFailure)
            {
                _uiOperations.Report(rollbackFailure);
            }
        }

        private bool IsLifecycleCurrent(BreadcrumbDropDownOpenLease lease, bool allowDisposed)
        {
            lock (_sync)
                return IsCurrentCore(lease, allowDisposed);
        }

        private bool IsCurrentCore(BreadcrumbDropDownOpenLease lease, bool allowDisposed) =>
            (allowDisposed || !_disposed)
            && lease.Generation == _generation
            && !lease.Cancellation.IsCompleted;

        private void ScheduleInvalidating(Func<Task> operation, bool disposing)
        {
            TaskCompletionSource<bool> canceled;
            BreadcrumbDropDownOpenLease lease;
            lock (_sync)
            {
                if (_disposed)
                    return;
                _disposed = disposing;
                canceled = InvalidateCore();
                if (_pendingCloseCompletion == null)
                {
                    _openCompletion = null;
                    _openTask = null;
                }
                lease = new BreadcrumbDropDownOpenLease(_generation, _cancellation.Task);
            }
            canceled.TrySetResult(true);
            ScheduleObserved(() =>
                IsLifecycleCurrent(lease, disposing) ? operation() : Task.CompletedTask
            );
        }

        private void ScheduleObserved(Func<Task> operation) =>
            _ = ObserveScheduledAsync(RunOnOwnerAsync(operation));

        private async Task<T> RunOnOwnerAsync<T>(Func<T> operation)
        {
            Task<T>? running = null;
            await _uiOperations
                .PostAsync(() => running = _uiOperations.RunAsync(operation))
                .ConfigureAwait(false);
            if (running == null)
                throw new InvalidOperationException("The popup operation could not be scheduled.");
            return await running.ConfigureAwait(false);
        }

        private static async Task ObserveScheduledAsync(Task<Task> kickoff)
        {
            try
            {
                Task running = await kickoff.ConfigureAwait(false);
                await running.ConfigureAwait(false);
            }
            catch
            {
                // Dispatch and operation failures are reported before their tasks fault.
            }
        }

        private TaskCompletionSource<bool> InvalidateCore()
        {
            _generation++;
            TaskCompletionSource<bool> canceled = _cancellation;
            _cancellation = NewCompletionSource();
            return canceled;
        }

        private static TaskCompletionSource<bool> NewCompletionSource() =>
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
