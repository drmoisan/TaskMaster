# No fail-before run claimed for the terminal-hook synchronization barrier (Issue #751, P1-T2)

Timestamp: 2026-09-03T14-35

This artifact records, as an auditable negative claim, that no fail-before (red-before) run is claimed for
the repair of `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`. Route 2 of the `spec.md` Test
Strategy was selected by P1-T1; route 1 (temporary, reverted instrumentation) was not executed and no
instrumentation is added to any file by this plan.

SearchScope: `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/regression-testing/`

SearchPatterns: `fail-before-exception.*.md`

SearchResult: none. At the time of this search that directory existed and contained exactly one entry,
`fail-before-route-selection.2026-09-03T11-48.md`, written by P1-T1 earlier in this phase. No file matching
`fail-before-exception.*.md` was found. Command run:
`Get-ChildItem -Path 'docs\features\active\terminal-notification-hook-test-lacks-sync-barrier-751\evidence\regression-testing' -Filter 'fail-before-exception.*.md'`,
which returned 0 items.

## WhyFailingRunImpossible:

**Observed P0-T15 branch: all-green.** The pre-change three-run series recorded by P0-T15 ran the target test
three times under the identical CI-shaped invocation and recorded it as `Passed` on all three runs. The
`NATURAL_RED_OBSERVED` branch was not taken.

The mechanism, not a restatement of the symptom. The race window between the worker's release and the
counter increment is sub-microsecond. Research §2.2 traces it precisely: the worker task the test awaits is
released by `TrySetException` at `AppOlObjects.FolderTreeService.cs:261`, inside the composition lock, and
the terminal notification that eventually reaches the fixture override and performs the increment is
dispatched deliberately **after** that lock is released (`:269-272`). Those are ten source lines and one
lock-release apart, and from `:261` onward the notifying thread and the test thread proceed concurrently.

Research §2.4 establishes that only one of the two possible interleavings races at all. In interleaving (b)
the fault lands before the worker evaluates the completion check at `AppOlObjects.FolderTreeService.cs:159`,
so `ObserveFolderTreeServiceDispatchTerminal` is invoked **inline on the worker thread**, the increment
happens on the worker thread before it reaches `:166`, and the counter is already 1 when `run.Worker`
completes. **Interleaving (b) passes unconditionally.** Only interleaving (a), in which the continuation is
queued to `TaskScheduler.Default`, races the test thread, and even then the losing window is sub-microsecond.

Consequently **no run of the unmodified tree is reliably red.** A red is possible on any given run; it is not
producible on demand. The three consecutive green pre-change runs recorded by P0-T15 are the recorded
evidence for that claim in this worktree. They are evidence that a red is not *reliably* producible; they are
not evidence that the defect is absent, and this dossier makes no such claim.

The one recorded natural red this defect actually produced is the PR #746 `mstest-coverage` CI failure, whose
FluentAssertions message was:

```
Expected sut.InvokedTerminalHookCount to be 1, but found 0.
```

(`spec.md:53`). That CI run is the historical red-before for this defect. It is cited as history, and it is
what motivated the issue; it is not a run this plan reproduced.

## Mechanical absence-of-barrier proof

Exactly two claims are made, and no stronger claim is made.

### Claim 1 — the counter is touched on exactly three lines within the two source globs

Command re-derived in this task (Phase 1, before the Phase 2 edits):

```powershell
Select-String -Path 'TaskMaster.Test\AppGlobals\*.cs','TaskMaster\AppGlobals\*.cs' -SimpleMatch 'InvokedTerminalHookCount'
```

Output:

```
AppOlObjectsFolderTreeServiceLifecycleTests.cs:158: internal int InvokedTerminalHookCount,
AppOlObjectsFolderTreeServiceLifecycleTests.cs:200: InvokedTerminalHookCount++;
AppOlObjectsFolderTreeServiceTests.cs:114: sut.InvokedTerminalHookCount.Should().Be(1);
```

Match count: **3**, as required.

The three lines are the field declaration and the `++` write in
`AppOlObjectsFolderTreeServiceLifecycleTests.cs`, and the read in `AppOlObjectsFolderTreeServiceTests.cs`.

**Scope statement.** The claim is scoped to those two globs by construction. The same identifier also appears
in this feature folder's Markdown — `spec.md`, `issue.md`, the research record, and the plan — which the
globs exclude. The claim is not that the identifier appears nowhere else in the repository.

### Claim 2 — the reading test is the only test that observes a terminal-hook side effect without awaiting `run.Terminal`

Command re-derived in this task (Phase 1, before the Phase 2 edits):

```powershell
Select-String -Path 'TaskMaster.Test\AppGlobals\*.cs' -Pattern '\.Terminal\b'
```

Output:

```
AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs:117: var terminalException = await GetExceptionAsync(await run.Terminal);
AppOlObjectsFolderTreeServiceLifecycleTests.cs:38: var terminalException = await GetExceptionAsync(await run.Terminal);
AppOlObjectsFolderTreeServiceLifecycleTests.cs:118: var terminalException = await GetExceptionAsync(await run.Terminal);
AppOlObjectsFolderTreeServiceTests.cs:73: var terminalException = await GetExceptionAsync(await firstRun.Terminal);
AppOlObjectsFolderTreeServiceTests.cs:143: var terminal = await run.Terminal;
AppOlObjectsFolderTreeServiceTests.cs:308: var terminalException = await GetExceptionAsync(await run.Terminal);
AppOlObjectsFolderTreeServiceTests.cs:341: var terminalFault = await GetExceptionAsync(await staleRun.Terminal);
```

Match count: **7**, as required. All seven are awaits of the captured terminal signal.

**The claim made is:** the reading test is the only test that observes a terminal-hook side effect without
awaiting `run.Terminal`, while seven sibling call sites await it.

**The claim deliberately NOT made** is that the reading test is "the only test in its class that never awaits
`run.Terminal`". That stronger claim is false. `AppOlObjectsFolderTreeServiceLifecycleTests` is a partial
class spanning three files, and the following also call `StartWorkerAsync` without awaiting the returned
`Terminal`:

- `VerifyCompositionFailureRetryAsync` (`AppOlObjectsFolderTreeServiceTests.cs`, declared at `:384`)
- `InitializationLinearization_CoalescedCallersReceiveOnePublishedService`
  (`AppOlObjectsFolderTreeServiceLifecycleTests.cs`, declared at `:52`)
- the `secondRun` follow-on call inside `WorkerFirst_NullDispatchTask_ResetsOwnershipAndPermitsSingleServiceRetry`
  (`AppOlObjectsFolderTreeServiceTests.cs`, declared at `:66`; the follow-on call is at `:85`)
- the `retryRun` follow-on call inside `VerifyCandidateOwnershipAsync`
  (`AppOlObjectsFolderTreeServiceTests.cs`; the follow-on call is at `:345`)

**None of them observes a terminal-hook side effect**, which is why their omission of the await is not a
defect. `spec.md` already carries the correct qualifier: "Every other test in the class that observes
terminal-hook side effects already awaits it."

**Recorded discrepancy with the research record.** Research §1 enumerates **six** awaiting call sites
(`AppOlObjectsFolderTreeServiceLifecycleTests.cs:38`, `AppOlObjectsFolderTreeServiceTests.cs:73`, `:143`,
`:308`, `:341`, and `AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs:117`) and **omits**
`AppOlObjectsFolderTreeServiceLifecycleTests.cs:118`. The figure derived mechanically here is therefore
**seven**, and the research figure of six is an undercount. The research record is **not** edited by this
plan; the discrepancy is recorded here instead.

**Expected drift of the second derivation.** Both derivations above were run in Phase 1, before the Phase 2
edits. After P2-T1 lands its inserted statement `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);`
into the reading test, the second derivation returns **eight** matches rather than seven. A later re-run is
therefore not expected to reproduce seven, and that is the intended effect of the fix rather than a
regression in this evidence.
