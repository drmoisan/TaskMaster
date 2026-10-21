# Issue #751 — Local Update Mirror (P5-T11)

Timestamp: 2026-09-03T14-51

PostedAs: unknown

This artifact mirrors a **local** update to
`docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/issue.md`. The three checkboxes
under `## Proposed Fix / Validation Ideas` were reconciled in the local feature-folder `issue.md`. No update
was posted to the GitHub issue by this plan: the plan authorizes no `gh` invocation, and posting is outside
its task set. The GitHub issue is https://github.com/drmoisan/TaskMaster/issues/751.

## Exact text written into `issue.md`

```markdown
- [x] Trace `ControlledUiDispatcher` and `AppOlObjectsFolderTreeService`'s terminal-hook dispatch path
  to determine whether `ReleaseAsync()` can legitimately return before the terminal hook has run.
  Answered by research §2.2-§2.3: the worker is released by `TrySetException` at
  `AppOlObjects.FolderTreeService.cs:261` while the terminal notification is dispatched afterwards at
  `:269-272`, and `ReleaseAsync()` is a complete no-op on this path (the identity guard at `:177-183`
  returns immediately because the initialization field was already nulled at `:262`). So yes,
  `ReleaseAsync()` legitimately returns before the terminal hook has run, and it is not a barrier.
- [x] Add a deterministic synchronization barrier (e.g., await a completion signal/TaskCompletionSource
  the terminal hook sets) so the assertion cannot race the hook's execution.
  Answered by the delivered change and P3-T3: the test now awaits the terminal signal the fixture already
  captures, via the inserted assertion
  `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);`, and the counter is hardened
  with `Interlocked.Increment` and `Volatile.Read`. No new primitive was introduced. The five-run
  repeat-run series recorded by P3-T3 shows the repaired test passing on every run.
- [x] Confirm no production code path shares the same unguarded race; broaden scope if it does.
  Answered by research §3: the defect is test-only. `OnFolderTreeServiceInitializationTerminal` is
  overridden only in test code, `AppOlObjects` has no production subclass at all, and the
  notify-after-publish ordering is deliberate. Scope was therefore not broadened, and
  `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` is byte-identical to branch point `f8414ee9`
  (P4-T8).
```

## Citation mapping

| Checklist item | Citation required by P5-T11 | Citation written |
|---|---|---|
| Trace item | research §2.2-§2.3 | research §2.2-§2.3 |
| Synchronization-barrier item | the delivered change and P3-T3 | the delivered change and P3-T3 |
| Production-reachability item | research §3 | research §3 |

## Acceptance

| Required | Observed | Result |
|---|---|---|
| All three checkboxes under `## Proposed Fix / Validation Ideas` in `issue.md` read `- [x]` | all three | PASS |
| Each carries its citation | each carries the citation in the mapping table above | PASS |
| The mirror artifact exists carrying `Timestamp:` and `PostedAs:` | this file carries both | PASS |
