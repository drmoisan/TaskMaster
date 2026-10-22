# Feature Audit — Issue #900: breadcrumb thread-affinity tests assume Task.Run yields a distinct thread

- Timestamp: 2026-09-17T02-51
- Work mode: `full-bug` (`issue.md` line 4: `- Work Mode: full-bug`). AC source: `spec.md` `## Acceptance Criteria` only. `user-story.md` does not exist, which is correct for this mode.
- Companion artifacts: `policy-audit.2026-09-17T02-51.md`, `code-review.2026-09-17T02-51.md`

## Scope and Baseline

- Branch: `bug/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900`, head `3538c3617edb1c0e26186bec2a2e3aa26171b7d1`.
- Base: `origin/main` @ `66b65a4626095ade5a01643aee4a43c90cc58cbf`, fetched explicitly in the execution worktree before diffing. `git merge-base origin/main HEAD` returns `66b65a46` and `git merge-base --is-ancestor origin/main HEAD` exits 0: `origin/main` is an ancestor of the head because the branch merged it before execution (`b617c1fe3`). The two-dot and three-dot diff forms are therefore the same set (52 paths each); their agreement is stated as a topology fact, not as independent confirmation.
- Commits on the branch: `88dd92169` (preparation), `b617c1fe3` (merge of origin/main), `852d33a5d` (Phase 0 evidence), `63142ec73` (the fix), `3538c3617` (evidence, check-off, plan state).
- Diff composition: 1 source file (`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, +101/-30), 46 feature-folder documents, 5 inherited `.claude/agent-memory/` files. No production file, `.csproj`, `packages.config`, workflow, or runsettings change.
- Baseline state of the subject file at `origin/main`: 419 lines, `Task.Run(` x3, `.GetAwaiter()` x3, no `RunOnDedicatedWorkerThread`, no `UiDispatcher.CheckAccess()` (P1-T1 census; base blob CRLF-normalized SHA-256 `CE87F6C2...A1CA` re-derived by the reviewer and equal to the executor's `PRE-EDIT-HASH`).
- Baseline behaviour: the original two tests passed on this run (P0-T9 `ORIGINAL-FLAKE-OBSERVED: NO`; P0-T10 both `Passed` in the 7288-test run). The dossier (AC4) correctly treats a green observation as a single sample of the stolen branch, not as evidence against the defect.
- Reviewer's independent check: `vstest.console.exe` over the seven-test class in the execution worktree under `/Settings:scripts\vscode\TaskMaster.cli.runsettings` (SHA-256 equal to the executor's anchor), assembly built 02:34 by P5-T4: 7/7 passed, exit 0, both rewritten tests 0.045-0.047 s.

## Acceptance Criteria Inventory

Source: `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/spec.md`, section `## Acceptance Criteria`, lines 273-299. Eight checkbox items, all `- [x]` on disk at head (reviewer grep `^- \[[ x]\] AC[1-8]\. ` = 8 lines, all `[x]`; P0-T2 recorded all eight as `[ ]` before execution began).

| AC | Criterion (abridged; text unchanged in `spec.md`) | Box at head |
| --- | --- | --- |
| AC1 | The two `_WorkerThread_ThrowsBoundaryDiagnostic` tests no longer obtain their worker from `Task.Run(...).GetAwaiter().GetResult()`; they use a dedicated `System.Threading.Thread` the test creates and joins. | `[x]` |
| AC2 | Each rewritten test explicitly establishes the distinct-thread precondition (`scope.Viewer.UiDispatcher.CheckAccess()` is false on the dedicated thread) before asserting the boundary diagnostic, so the boundary assertion cannot pass vacuously. | `[x]` |
| AC3 | Each rewritten test asserts the captured exception is exactly `InvalidOperationException` (excluding `ObjectDisposedException`) with a message containing the guarded operation's name, preserving the #781 AC3 contract. | `[x]` |
| AC4 | A `fail-before-exception.<timestamp>.md` dossier under `<FEATURE>/evidence/regression-testing/` documents why a deterministic failing run of the original tests is not achievable, with the wait-inlining mechanism chain as the alternative proof. | `[x]` |
| AC5 | A deterministic guard-disabled failing run of the replacement tests (temporary, fully reverted `ClearViewerDispatcher(scope.Viewer)` insertion, no production edit) is captured as evidence, and the insertion is confirmed removed (clean `git diff`) before the final pass-after run. | `[x]` |
| AC6 | Both rewritten tests pass under `scripts/vscode/TaskMaster.cli.runsettings` (`Workers=0`, `Scope=ClassLevel`), with no change to that file. | `[x]` |
| AC7 | No sibling test in the file regresses (all 7 `[TestMethod]`s pass), and no production file is modified in the final committed diff. | `[x]` |
| AC8 | Full C# toolchain pass in order (format -> analyzers -> nullable -> vstest with CLI runsettings), restarting from step 1 on any failure or file change, with numeric coverage recorded and confirmed not regressed. | `[x]` |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence the reviewer read or re-derived |
| --- | --- | --- |
| AC1 | PASS | Diff read directly: both tests' Act phase is `Exception captured = RunOnDedicatedWorkerThread(() => { ... });` (lines 228-238, 277-291); the helper (lines 385-403) does `new Thread(...)`, `IsBackground = true`, `Start()`, `Join()`. The only remaining `Task.Run(` / `.GetAwaiter()` pair is at lines 332-336 in the out-of-scope third test. Census transitions 3 -> 1 for both tokens (P1-T1 -> P2-T1). |
| AC2 | PASS | Lines 230-236 and 279-285: `bool isOwnerThread = scope.Viewer.UiDispatcher.CheckAccess(); isOwnerThread.Should().BeFalse(...)` precedes the guarded call inside the delegate. `UiDispatcher` is the same `Dispatcher` the guard reads (`ItemViewer.cs:65-68`; `ItemViewer.Breadcrumb.cs:434`). Observed failing: P3-T3 (delegate run inline) fails both tests with `Expected isOwnerThread to be False ... vacuously, but found True.` |
| AC3 | PASS | Lines 247-249 and 300-302: `BeOfType<InvalidOperationException>()` (exact type in FluentAssertions 8.10.0), `Message.Should().Contain("InitializeBreadcrumbPipeline")` / `Contain("ConfigureBreadcrumbDropDown")`, `NotBeOfType<ObjectDisposedException>()`. Observed failing: P3-T1 fails both tests on the `Contain` assertion with the `CaptureCurrent()` message, proving the operation-name check is load-bearing and that `BeOfType` passed first. The production message template (`{operation} must be called on the thread that owns this ItemViewer...`) is unchanged and contains the `nameof` operation. |
| AC4 | PASS | `evidence/regression-testing/fail-before-exception.2026-09-17T02-19.md` exists with `Timestamp:`, `WhyFailingRunImpossible:`, `## Alternative Proof`, `## Output Summary`; the four-step mechanism chain (`TaskAwaiter.GetResult` -> `InternalWait` inline attempt -> `ThreadPoolTaskScheduler.QueueTask` local queue -> `ThreadPoolWorkQueue.Enqueue` thread-local) with primary sources; the `CheckAccess()` object-identity point; the MSTest 4.4.0 `DefaultFactoryAsync` point; and the P0-T9 observation with its limits stated. Precedent dossier at `docs/features/archive/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/fail-before-exception.2026-07-07T14-05.md` exists (reviewer `ls`). |
| AC5 | PASS | P3-T1: `EXIT_CODE: 1`, `ExpectedExitCode: 1`, `COUNTERS total=2 executed=2 passed=0 failed=2`, census `ClearViewerDispatcher(scope.Viewer); = 3`, both `MESSAGE` lines verbatim; no production file edited (P5-T9 `CHANGED-PATHS:` is exactly the test file; `git diff --name-status origin/main..HEAD` shows nothing under `QuickFiler/`). Revert P3-T2: census 1, anchored diff exit 0, empty porcelain, SHA-256 = `FIX-HASH`. Reviewer: head file SHA-256 = `8EBC19F8...BB164` = `FIX-HASH`; `git diff 63142ec73..HEAD -- <file>` empty; `git log -S` for the mutation token over the branch range returns nothing. Pass-after run P4-T2 (2/2) executed on the P4-T1 rebuild (`DLL_ADVANCED: True`). |
| AC6 | PASS | P4-T2: exit 0, 2/2 `Passed`, `/Settings:scripts\vscode\TaskMaster.cli.runsettings`, `RUNSETTINGS-HASH-NOW` = anchor `98EF03A8...CEF57`. P5-T5: both tests `Passed` inside the 7288-test, nine-assembly, `Workers=0`/`ClassLevel` run. Reviewer: `git diff origin/main..HEAD -- scripts/vscode/TaskMaster.cli.runsettings` = 0 bytes; file reads `Workers 0`, `Scope ClassLevel`; reviewer re-run 7/7 under the same settings with the same hash. |
| AC7 | PASS | P4-T3: `COUNTERS total=7 ... passed=7 failed=0`, the seven `RESULT` names equal the seven `[TestMethod]` names at base (P0-T9); P5-T5 seven in-scope results `Passed`; reviewer re-run 7/7. Production-file clause: `git diff --name-status origin/main..HEAD` lists one path under `QuickFiler.Test/`, none under `QuickFiler/`; P5-T9 and P5-T15 both list exactly that path under the `QuickFiler QuickFiler.Test` pathspec on the working tree and on the committed tip. |
| AC8 | PASS | Order: P5-T1 `csharpier format .` (`FORMAT_CHANGED_TREE: False`) -> P5-T2 `csharpier check .` (exit 0, 1641 files) -> P5-T3 analyzer `/t:Rebuild` (exit 0, 0 errors, 0 warnings, `CSC_OUT_LINES: 2`) -> P5-T4 nullable `/t:Rebuild` (exit 0, 0 errors) -> P5-T5 repository-wide vstest under the CLI runsettings (7288/7288) -> P5-T6 coverage delta -> P5-T7 size/census. Restart rule honoured: iteration 1 failed at P5-T5 on an environmental failure and the loop restarted from P5-T1 (`ITERATIONS: 2`, `LOOP: CLEAN PASS`). Numeric coverage: baseline `line-rate` 0.852658 / `branch-rate` 0.796851, final 0.852566 / 0.796675 on an identical `lines-valid` of 65616 (`COMPARABILITY: A`, delta -0.000092 within the 0.005 gate); reviewer read `line-rate="0.852566"` from the root element of `artifacts/csharp/coverage.xml` directly. Not regressed within the documented run-to-run band; no production line in the denominator changed. |

No criterion is checked without support. No criterion is PARTIAL, FAIL, or UNVERIFIED.

## Acceptance Criteria Check-off

All eight items were already `- [x]` in `spec.md` at head, checked by the executor at P4-T4 through P4-T8, P5-T10 and P5-T11 with the evidence cited above. The reviewer evaluated every item as PASS, so no box is changed: none newly checked, none unchecked. `spec.md` is not modified by this review.

### Acceptance Criteria Status
- Source: `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Summary

- Per-AC verdicts: AC1 PASS, AC2 PASS, AC3 PASS, AC4 PASS, AC5 PASS, AC6 PASS, AC7 PASS, AC8 PASS.
- The defect named in the issue is fixed by construction (dedicated `Thread`, in-delegate precondition using the guard's own predicate), the non-vacuity of the replacement assertions is proven by two pre-declared mutations whose observed failures match their predictions verbatim, and both reverts are hash-proven.
- No serialisation, pinning, retry, sleep, timeout, or tolerance was introduced; the runsettings file is unchanged.
- Blocking findings: 0. Non-blocking: 3 Low, 4 Info (see `code-review.2026-09-17T02-51.md`); none affects an acceptance criterion.
- Ready to merge: **yes**.
