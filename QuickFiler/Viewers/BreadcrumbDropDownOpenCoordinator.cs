#nullable enable
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Coordinates one selector-open request across the captured owner boundary and the native
    /// popup host without depending on a concrete viewer surface.
    /// </summary>
    internal sealed class BreadcrumbDropDownOpenCoordinator
    {
        private static readonly Task<bool> ClosedTask = Task.FromResult(false);

        private readonly object _sync = new object();
        private readonly BreadcrumbPopupUiOperations _operations;
        private readonly IBreadcrumbDropDownHost _host;
        private readonly Func<int> _rowCount;
        private readonly Func<bool> _isSelectorOpen;
        private readonly Func<bool> _openSelector;
        private readonly Action _cancelSelector;
        private readonly Action _detachPopupMessenger;
        private Func<Rectangle> _anchorBounds;
        private Func<Rectangle> _workingArea;
        private Task<bool>? _currentOpenTask;
        private int _generation;
        private bool _released;

        internal BreadcrumbDropDownOpenCoordinator(
            BreadcrumbPopupUiOperations operations,
            IBreadcrumbDropDownHost host,
            Func<Rectangle> anchorBounds,
            Func<Rectangle> workingArea,
            Func<int> rowCount,
            Func<bool> isSelectorOpen,
            Func<bool> openSelector,
            Action cancelSelector,
            Action detachPopupMessenger
        )
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _anchorBounds =
                anchorBounds ?? throw new ArgumentNullException(nameof(anchorBounds));
            _workingArea = workingArea ?? throw new ArgumentNullException(nameof(workingArea));
            _rowCount = rowCount ?? throw new ArgumentNullException(nameof(rowCount));
            _isSelectorOpen =
                isSelectorOpen ?? throw new ArgumentNullException(nameof(isSelectorOpen));
            _openSelector = openSelector ?? throw new ArgumentNullException(nameof(openSelector));
            _cancelSelector =
                cancelSelector ?? throw new ArgumentNullException(nameof(cancelSelector));
            _detachPopupMessenger =
                detachPopupMessenger
                ?? throw new ArgumentNullException(nameof(detachPopupMessenger));
        }

        internal IBreadcrumbDropDownHost Host => _host;

        internal Task<bool> CurrentOpenTask
        {
            get
            {
                lock (_sync)
                    return _currentOpenTask ?? ClosedTask;
            }
        }

        internal void UpdateRequestProviders(
            Func<Rectangle> anchorBounds,
            Func<Rectangle> workingArea
        )
        {
            _ = anchorBounds ?? throw new ArgumentNullException(nameof(anchorBounds));
            _ = workingArea ?? throw new ArgumentNullException(nameof(workingArea));
            lock (_sync)
            {
                ThrowIfReleased();
                _anchorBounds = anchorBounds;
                _workingArea = workingArea;
            }
        }

        internal Task<bool> RequestOpen()
        {
            lock (_sync)
            {
                if (_released)
                    return ClosedTask;
                if (_currentOpenTask != null && !_currentOpenTask.IsCompleted)
                    return _currentOpenTask;
                _currentOpenTask = OpenCoreAsync(_generation);
                return _currentOpenTask;
            }
        }

        internal void SetDroppedDown(bool droppedDown)
        {
            if (IsReleased())
                return;
            _ = _operations.PostAsync(() =>
            {
                if (IsReleased())
                    return;
                if (droppedDown)
                {
                    bool changed = _openSelector();
                    if (!changed && _isSelectorOpen())
                        _ = RequestOpen();
                    return;
                }
                CloseCore(BreadcrumbDropDownCloseReason.Uncommitted);
            });
        }

        internal void HandleSelectorOpenStateChanged()
        {
            if (IsReleased())
                return;
            _ = _operations.PostAsync(() =>
            {
                if (IsReleased())
                    return;
                if (_isSelectorOpen())
                    _ = RequestOpen();
                else if (_host.IsOpen)
                    _host.Close(BreadcrumbDropDownCloseReason.ExplicitCommit);
            });
        }

        internal void Reset()
        {
            if (!Invalidate(release: false))
                return;
            _ = _operations.PostAsync(() =>
            {
                CloseCore(BreadcrumbDropDownCloseReason.Uncommitted);
                _detachPopupMessenger();
                _host.Reset();
            });
        }

        internal void Release()
        {
            if (!Invalidate(release: true))
                return;
            _ = _operations.PostAsync(() =>
            {
                _detachPopupMessenger();
                _host.Dispose();
            });
        }

        private async Task<bool> OpenCoreAsync(int generation)
        {
            try
            {
                Task<bool> opening = await _operations
                    .RunAsync(() => BeginOpenCore(generation))
                    .ConfigureAwait(false);
                await _operations.ObserveReadinessAsync(opening).ConfigureAwait(false);
                bool opened = opening.GetAwaiter().GetResult();
                return await _operations
                    .RunAsync(() => FinishOpenCore(generation, opened))
                    .ConfigureAwait(false);
            }
            catch
            {
                return await RollbackAsync(generation).ConfigureAwait(false);
            }
        }

        private Task<bool> BeginOpenCore(int generation)
        {
            Func<Rectangle> anchorBounds;
            Func<Rectangle> workingArea;
            lock (_sync)
            {
                if (!IsCurrentCore(generation))
                    return ClosedTask;
                anchorBounds = _anchorBounds;
                workingArea = _workingArea;
            }

            Rectangle anchor = anchorBounds();
            int rows = _rowCount();
            var size = new Size(anchor.Width, Math.Min(320, Math.Max(120, rows * 26)));
            return _host.OpenAsync(anchor, workingArea(), size)
                ?? throw new InvalidOperationException(
                    "The breadcrumb popup host returned no open task."
                );
        }

        private bool FinishOpenCore(int generation, bool opened)
        {
            bool current = IsCurrent(generation);
            if (!opened)
            {
                if (current && _isSelectorOpen())
                    _cancelSelector();
                return false;
            }
            if (!current || !_isSelectorOpen())
            {
                if (_host.IsOpen)
                    _host.Close(BreadcrumbDropDownCloseReason.ExplicitCommit);
                return false;
            }
            return true;
        }

        private async Task<bool> RollbackAsync(int generation)
        {
            try
            {
                return await _operations
                    .RunAsync(() =>
                    {
                        if (IsCurrent(generation) && _isSelectorOpen())
                            _cancelSelector();
                        return false;
                    })
                    .ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
        }

        private bool CloseCore(BreadcrumbDropDownCloseReason reason)
        {
            if (_host.IsOpen && _host.Close(reason))
                return true;
            if (
                reason == BreadcrumbDropDownCloseReason.Uncommitted
                && _isSelectorOpen()
            )
                _cancelSelector();
            return false;
        }

        private bool Invalidate(bool release)
        {
            lock (_sync)
            {
                if (_released)
                    return false;
                _generation++;
                _currentOpenTask = null;
                _released = release;
                return true;
            }
        }

        private bool IsCurrent(int generation)
        {
            lock (_sync)
                return IsCurrentCore(generation);
        }

        private bool IsCurrentCore(int generation) =>
            !_released && generation == _generation;

        private bool IsReleased()
        {
            lock (_sync)
                return _released;
        }

        private void ThrowIfReleased()
        {
            if (_released)
                throw new ObjectDisposedException(nameof(BreadcrumbDropDownOpenCoordinator));
        }
    }
}
