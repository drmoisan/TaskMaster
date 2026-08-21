# breadcrumb-dropdown-coordinator-stale-closepending-drops-reopen (Issue #462)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-dropdown-coordinator-stale-closepending-drops-reopen/ (Issue #462)
- Work Mode: full-bug
- Discovered during: preparation research for issue #455 (epic #136, child F13)

- Issue: #462
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/462
- Last Updated: 2026-08-08
## Summary

`BreadcrumbDropDownOpenCoordinator.CloseCore` never clears its `_closePending` flag on the
**successful** close path. The flag latches `true` after the first close that actually closes the
host and is never reset. `RequestOpen` consults that stale flag and can silently return a
already-closed sentinel task instead of opening the drop-down, dropping a legitimate reopen
request with no error and no log.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- Affected path: QuickFiler item folder-selector breadcrumb drop-down open/close lifetime

## Suspected Cause

`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:237-267`:

```csharp
private bool CloseCore(BreadcrumbDropDownCloseReason reason)
{
    lock (_sync)
    {
        if (_released)
            return false;
        if (_closePending)
            return true;
        _closePending = true;          // :245  latched here
    }
    bool closed;
    try
    {
        closed = _host.Close(reason);
    }
    catch
    {
        ClearClosePending();           // :254  cleared on throw
        throw;
    }
    if (closed)
    {
        lock (_sync)
            _generation++;
        return true;                   // :261  returns WITHOUT ClearClosePending()
    }
    ClearClosePending();               // :263  cleared on the not-closed path
    ...
}
```

Every exit path clears `_closePending` **except** the successful one at `:257-261`. That is the
inverted case: the successful close is exactly the path after which the coordinator should be
ready to accept a new open.

The stale flag is then read by `RequestOpen` at `:92-93`:

```csharp
if (_closePending && _host.IsOpen)
    return ClosedTask;
```

## Steps to Reproduce

1. Open the breadcrumb drop-down so `_host.IsOpen` is true.
2. Close it through a path that reaches `CloseCore` and where `_host.Close(reason)` returns `true`.
   `_closePending` is now permanently `true`.
3. Cause the host to become open again through a path that does not route through
   `CloseCore`/`RequestOpen` — for example `SetDroppedDown(true)` at `:108-112`, where
   `_openSelector()` reports no change and `_isSelectorOpen()` is true.
4. Call `RequestOpen`.
5. Observe that the guard at `:92` is satisfied (`_closePending` stale-true and `_host.IsOpen`
   true) and `ClosedTask` is returned. The open request is discarded silently.

## Expected Behavior

`_closePending` describes an in-flight close. Once `_host.Close` has completed successfully the
close is no longer pending, so a subsequent `RequestOpen` should proceed and open the drop-down.

## Actual Behavior

`_closePending` remains `true` for the remaining lifetime of the coordinator, so `RequestOpen` can
short-circuit to `ClosedTask` whenever the host is open.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The user-visible symptom is a folder-selector drop-down that intermittently refuses to reopen until
the viewer is recycled. The failure is silent — no exception, no log line — which makes it
expensive to diagnose from a bug report. Severity is Medium because reaching the state requires the
specific reopen path in step 3 rather than the common open/close cycle.

## Additional Note — this is also the file's coverage gap

The same condition is one of the four uncovered branch outcomes measured in this file
(`BreadcrumbDropDownOpenCoordinator.cs`, 98.25% line / 92.05% branch in the committed Cobertura at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`).
The branch is uncovered precisely because no test asserts the post-close reopen contract. The
coverage gap and the defect are the same finding, which is why a coverage-only change cannot close
it: writing the test that covers the branch would assert the current, incorrect behavior.

## Suggested Remediation

Call `ClearClosePending()` on the successful-close path before returning `true` at `:261`, or
restructure so the flag is cleared in a `finally`. Then add a regression test asserting that
`RequestOpen` opens after a successful `CloseCore`.

Related nearby observations, worth reconciling in the same change:

- `_host.IsOpen` is evaluated while holding `_sync` at `:92`, inconsistent with `CloseCore`'s
  deliberate decision to call `_host.Close` **outside** the lock at `:250`. That asymmetry is a
  lock-ordering hazard (`Coordinator._sync` -> host lock).
- `Close` is a *claim* rather than a completion: `CloseCore` returns `true` at `:243-244` when a
  close is already pending, so callers cannot distinguish "closed" from "someone else is closing".

## Why this is not fixed under epic #136

Epic #136 child F13 (issue #455) carries a hard no-behavior-change NFR. Clearing the flag changes
observable drop-down open/close behavior, so it belongs in its own issue.

## Related

- Issue #455 — F13, breadcrumb drop-down and WebView2 host coverage (where this was found).
- Issue #136 — parent epic.
- Issue #440 — open breadcrumb arrow-key navigation bug in adjacent territory; reconcile scheduling.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Reconcile against F13's plan before scheduling, since F13 adds tests over this file
