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

        /// <summary>
        /// Issue #462 (I-462.1): true only while <c>_host.Close(reason)</c> is executing, i.e. a close
        /// is in flight on this coordinator. It is cleared in a <c>finally</c> around that call, so it
        /// reads <c>false</c> on every exit from <see cref="CloseCore"/> — success, not-closed, throw,
        /// and released. Its sole purpose is to suppress a concurrent second close of the same
        /// in-flight operation; it must never outlive the call.
        /// </summary>
        private bool _closeInFlight;

        /// <summary>
        /// Issue #462 (I-462.1): true after a close that returned <c>true</c>, i.e. the host has been
        /// closed and no reopen has been requested since. It suppresses a repeated close of an
        /// already-closed host (I-462.3), and is cleared by <see cref="RequestOpen"/> and by
        /// <c>Invalidate</c>. Separating it from <see cref="_closeInFlight"/> is what lets a
        /// legitimate reopen through while still suppressing the repeated close: the single close flag
        /// these two replace was doing both jobs at once and could not distinguish them.
        /// </summary>
        private bool _closeCompleted;

        private bool _released;
        private bool _nextOpenTakesNoFocus;

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
            _anchorBounds = anchorBounds ?? throw new ArgumentNullException(nameof(anchorBounds));
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
                if (_closeInFlight && _host.IsOpen)
                    return ClosedTask;
                _closeCompleted = false;
                _currentOpenTask = OpenCoreAsync(_generation);
                return _currentOpenTask;
            }
        }

        /// <summary>
        /// Issue #438: latches "the next native open takes no focus" for a search-originated open.
        /// </summary>
        /// <remarks>
        /// The latch exists because the actual open request does not arrive on the call that starts
        /// it: <c>SetDroppedDown</c> opens the selector session, and the resulting
        /// <c>SelectorOpenStateChanged</c> event is what reaches <see cref="RequestOpen"/>. Both the
        /// <c>SetDroppedDown</c>-posted work and the <c>HandleSelectorOpenStateChanged</c>-posted
        /// work run FIFO on the same <see cref="BreadcrumbPopupUiOperations"/> queue, so a latch set
        /// before the session is opened is deterministically observed by the open it belongs to, and
        /// by no later one. <see cref="BeginOpenCore"/> consumes the latch exactly once.
        /// </remarks>
        internal void LatchNextOpenTakesNoFocus()
        {
            lock (_sync)
            {
                if (_released)
                    return;
                _nextOpenTakesNoFocus = true;
            }
        }

        /// <summary>Test-visible latch state; true while a non-focusing open is pending.</summary>
        internal bool NextOpenTakesNoFocus
        {
            get
            {
                lock (_sync)
                    return _nextOpenTakesNoFocus;
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
                else
                    CloseCore(BreadcrumbDropDownCloseReason.ExplicitCommit);
            });
        }

        internal void Reset()
        {
            if (!Invalidate(release: false))
                return;
            _ = _operations.PostAsync(() =>
            {
                if (
                    (!_host.IsOpen || !_host.Close(BreadcrumbDropDownCloseReason.Uncommitted))
                    && _isSelectorOpen()
                )
                    _cancelSelector();
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
            bool takeFocus;
            lock (_sync)
            {
                if (!IsCurrentCore(generation))
                    return ClosedTask;
                anchorBounds = _anchorBounds;
                workingArea = _workingArea;
                // Consume the latch exactly once, so a search-driven open does not leak its
                // non-focusing intent onto a later gesture open.
                takeFocus = !_nextOpenTakesNoFocus;
                _nextOpenTakesNoFocus = false;
            }

            Rectangle anchor = anchorBounds();
            int rows = _rowCount();
            var size = new Size(anchor.Width, Math.Min(320, Math.Max(120, rows * 26)));
            // A default (gesture) open deliberately keeps calling the original 3-parameter overload
            // rather than the 4-parameter one with takeFocus: true. The two are semantically
            // identical, but existing loose Mock<IBreadcrumbDropDownHost> setups across the suite are
            // configured only for the 3-parameter shape; a 4-parameter call would return a null Task
            // and trip the guard below. Only the non-focusing search open needs the new overload.
            Task<bool>? opening = takeFocus
                ? _host.OpenAsync(anchor, workingArea(), size)
                : _host.OpenAsync(anchor, workingArea(), size, takeFocus: false);
            return opening
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
                CloseCore(BreadcrumbDropDownCloseReason.ExplicitCommit);
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

        /// <summary>
        /// Issue #462: the guard order is released, then in-flight, then completed. The two flags have
        /// distinct meanings (see <see cref="_closeInFlight"/> and <see cref="_closeCompleted"/>), and
        /// <see cref="_closeInFlight"/> is cleared in a <c>finally</c> so it reads <c>false</c> on the
        /// success, not-closed, throw and released exits alike (I-462.1).
        /// </summary>
        private bool CloseCore(BreadcrumbDropDownCloseReason reason)
        {
            lock (_sync)
            {
                if (_released)
                    return false;
                if (_closeInFlight)
                    return true;
                if (_closeCompleted)
                    return true;
                _closeInFlight = true;
            }
            bool closed;
            try
            {
                closed = _host.Close(reason);
            }
            finally
            {
                lock (_sync)
                    _closeInFlight = false;
            }
            if (closed)
            {
                lock (_sync)
                {
                    _generation++;
                    _closeCompleted = true;
                }
                return true;
            }
            if (reason == BreadcrumbDropDownCloseReason.Uncommitted && _isSelectorOpen())
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
                _closeCompleted = false;
                _released = release;
                return true;
            }
        }

        private bool IsCurrent(int generation)
        {
            lock (_sync)
                return IsCurrentCore(generation);
        }

        private bool IsCurrentCore(int generation) => !_released && generation == _generation;

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
