# Feature Audit — terminal-notification-hook-test-lacks-sync-barrier (Issue #751)

- Artifact timestamp: 2026-09-03T17-30
- Work mode: `full-bug` (`issue.md:12`) → the authoritative acceptance-criteria source is **`spec.md` only**
- Baseline: `f8414ee9` (re-derived independently as the merge base against `main` and `origin/main`)
- Head: `d2fbc327`
- AC section located at `spec.md:324` (`## Acceptance Criteria`); ten checkbox items at lines 326, 331, 333, 339, 341, 346, 352, 356, 358, 362
- Result: **10 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.** No checkbox was unchecked; every `[x]` in `spec.md` is supported by evidence this reviewer verified independently.

## Method

Each criterion was evaluated against the tree and against primary artifacts (git objects, TRX files, coverage collector output, file contents), not against the executor's summaries. Where a criterion was verified only from a committed evidence artifact, that limitation is stated in the row.

## Acceptance criteria evaluation

| # | Criterion (abbreviated) | Verdict | Evidence verified by this reviewer |
|---|---|---|---|
| AC1 | Barrier assertion present in `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`, after the `run.Worker` assertion and before the `LoadCount` assertion, using the captured `run.Terminal` | **PASS** | `AppOlObjectsFolderTreeServiceTests.cs:113` reads `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);`, between the worker assertion at `:111` and the `LoadCount` assertion at `:114`. `run.Terminal` is the tuple member captured by `StartWorkerAsync` at `:258` before the worker starts, not a fresh `sut.NextTerminal` read. |
| AC2 | The terminal-hook count assertion still expects 1, is not relaxed, widened, or deleted, and is reached only after the barrier | **PASS** | `:115` reads `Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);` — the expected value is unchanged at 1, and it sits two lines after the barrier. |
| AC3 | Every read and write of `InvokedTerminalHookCount` across the two files is synchronized (`Interlocked.Increment` write, `Volatile.Read` read); no new `using` required | **PASS** | Repository-wide search returns exactly three source occurrences: declaration (`Lifecycle:158`), `Interlocked.Increment` (`Lifecycle:200`), `Volatile.Read` (`Tests:115`). Zero occurrences of `InvokedTerminalHookCount++` and zero bare `sut.InvokedTerminalHookCount.Should()` reads remain. `System.Threading` was already imported in both files (`Tests:2`, `Lifecycle:2`); the diff adds no `using`. |
| AC4 | `AppOlObjects.FolderTreeService.cs` byte-identical to `f8414ee9`; branch diff contains no production-assembly file | **PASS** | `git rev-parse f8414ee9:<path>` and `HEAD:<path>` both return blob `fe980d3fc6726cbc49e391798489e0af82a683a1`. `git diff --name-only f8414ee9 HEAD` restricted to all nine production project directories returns 0 rows. |
| AC5 | The repaired test passes on every run of a repeat-run series under the CI-shaped invocation with `/EnableCodeCoverage /InIsolation`, no failure and no re-run; series committed under `evidence/qa-gates/` | **PASS** | Read directly from `coverage/trx/P3-T2-run1..5/*.trx`: 408 total, 408 passed, 0 failed, 0 notExecuted on each of the five runs, with `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault = Passed` in all five. Runs are timestamped 14:36:46–14:37:35, after the fix commit at 14:35:08. `repeat-run-1..5` and `repeat-run-comparison` are committed under `evidence/qa-gates/`. |
| AC6 | Exactly one fail-before route executed and its artifact committed under `evidence/regression-testing/`; if route 1, no instrumentation remains in the diff | **PASS** | Route 2 selected and executed: `fail-before-route-selection.2026-09-03T11-48.md` and `no-fail-before-rationale.2026-09-03T11-48.md` are both committed under `evidence/regression-testing/`, paired with the repeat-run stress record required by that route. Route 1 was not executed, and the three-line diff contains no instrumentation, so the instrumentation clause is satisfied vacuously. See the non-blocking observation below on the invoked condition. |
| AC7 | Neither file exceeds the 500-line cap; no new fixture type, gate field, `TaskCompletionSource`, or helper method introduced | **PASS** | Line counts measured directly: `AppOlObjectsFolderTreeServiceTests.cs` = 493, `AppOlObjectsFolderTreeServiceLifecycleTests.cs` = 490. The full diff is 3 added and 2 removed lines; none declares a type, field, `TaskCompletionSource`, or method. |
| AC8 | No banned determinism API (`Thread.Sleep`, `Task.Delay`, wall-clock wait, polling loop, `[DoNotParallelize]`) added anywhere in the branch diff | **PASS** | The diff adds only an `await` on an existing completion signal, `Interlocked.Increment`, and `Volatile.Read`. A search of `TaskMaster.Test/AppGlobals` finds `[DoNotParallelize]` only in five unrelated pre-existing files and `[Timeout]` only in `NonBlockingDelayTests.cs`; neither changed file contains any of them. |
| AC9 | One clean toolchain pass in order: `csharpier check` clean, analyzer rebuild no error, nullable rebuild no error, MSTest no failed test; commands stated in the completion report | **PASS** | `csharpier check` on both files re-executed by this reviewer at exit 0. MSTest re-verified by this reviewer from `P4-T5.trx`: 6984 total, 6984 passed, 0 failed. The two msbuild gates are taken from the committed records (exit 0, 0 warnings, 0 errors, both using `/t:Rebuild` and the exact `CLAUDE.md` command strings) and were not re-executed here; they are corroborated by build-output timestamps showing a single compile sweep ending at 14:40:53, immediately before the 14:41:34 test run. |
| AC10 | `issue.md` "Proposed Fix / Validation Ideas" items reconciled: trace item and production-reachability item answered by research, barrier item answered by the delivered change | **PASS** | All three items at `issue.md:79-98` are checked and carry substantive answers. The trace answer matches the production code as read: the worker is released by `TrySetException` inside `_folderTreeServiceGate` and the terminal notification is dispatched after the lock is released, and `ReleaseAsync()` is a no-op on this path because the identity guard at `AppOlObjects.FolderTreeService.cs:177-179` returns immediately once the initialization field has been nulled. The production-reachability answer is consistent with `OnFolderTreeServiceInitializationTerminal` being an empty virtual overridden only in test code. The barrier answer matches the delivered `:113`/`:115` lines. |

## Non-blocking observations attached to criteria

- **AC6.** The route-2 selection invokes the `spec.md` condition "exceeds the change budget or the remaining line headroom". That condition is the weaker of the two recorded justifications, because route-1 instrumentation is reverted before landing and so never consumes landed line headroom. The sound justification is the second recorded reason: a red produced by deferring the fixture's own increment restates the race rather than reproducing it and is not reproducible from the landed tree. AC6 is satisfied as written, and the evidence honestly states that the post-change series does not demonstrate a red-to-green transition and does not claim one. The consequence to record plainly: the delivery carries no demonstration that the *specific* delivered barrier converts a red into a green, and the only recorded natural red for this defect remains the PR #746 CI failure cited as history.
- **AC9.** The two full-solution msbuild rebuilds were corroborated rather than reproduced by this review.
- **Outside the AC set.** The barrier changes the failure mode of a future "hook never invoked" regression from a named assertion failure into an unbounded wait, and no test timeout bounds it in CI. Detailed as NB-1 in `code-review.2026-09-03T17-30.md`.
- **Outside the AC set.** The plan's numeric coverage capture produced no figures on either side. This reviewer recovered the pair independently and found no regression and clearance of the 85% line floor on first-party production code. Fully adjudicated in § 5 of `policy-audit.2026-09-03T17-30.md`; non-blocking.

## Scope conformance

The delivered footprint matches the spec's declared scope exactly: three source lines, two files, both in `TaskMaster.Test`, zero production files, zero configuration files. Nothing outside the spec's stated scope was changed, and nothing the spec required was omitted.

## Checkbox actions taken

None. All ten criteria were already checked `[x]` in `spec.md`, and all ten are supported by verified evidence. No criterion was unchecked and no criterion was added.

### Acceptance Criteria Status
- Source: `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md`
- Total AC items: 10
- Checked off (delivered): 10
- Remaining (unchecked): 0
- Items remaining: none
