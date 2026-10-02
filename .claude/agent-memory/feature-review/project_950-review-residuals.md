---
name: 950-review-residuals
description: '#950 (QuickFiler tests depend on wall-clock timing) full-bug review 2026-10-02T14-30 PASS 16/17 AC (AC17 pending CI under coordinator ruling), 0 blocking, 6 non-blocking; WorkerStarter delegate seam + R4 baseline pin; theme-test null-dispatcher exposure analysis; no-Bash review with attribute-excluded production file'
metadata:
  type: project
---

Full-bug review (parallel cohort bugs-2026-09-28, nested worktree `agent-a7805823735145ca4`, head `523c93ee9`, base
`34c2ed88c` = post-merge origin/main anchor): PASS, 16/17 AC, 0 blocking, 6 non-blocking (CR-1..CR-6), 8 observations,
9 follow-ups. Caller forbade Bash entirely and supplied the production diff; verified with Read/Grep/Glob plus the committed
footprint/anchor evidence and the worktree reflog `<session>/.git/worktrees/<wt>/logs/HEAD` as head + clock. Advertised the
3-`..` hook traversal path from the session cwd as at [[948-review-residuals]]; no mirror; spec.md untouched (AC1-16 were
already [x]; AC17 left [ ] by binding ruling).

**Reusable verification points:**
- AC17 "pending CI" pattern: when the coordinator rules that an AC is checked off ONLY from the PR's own CI run, record the
  ruling verbatim in the feature audit, evaluate the AC as PENDING (not FAIL), leave the box unchecked, and list the closure
  steps (run ID, head SHA, pass/fail counts, coverage figures). Spec carried a dated note under the AC; confirm it matches.
- Changed production file with a pre-existing type-level `[ExcludeFromCodeCoverage]` (QfcDatamodel.cs:25, #197 commit): no
  class node in either Cobertura document (`QFCDATAMODEL-CLASS-NODES: 0`). Write the per-file verdict as PASS on the
  no-regression limb and prove EXECUTION separately (every test reaches the seam; negative controls that remove the seam
  assignment fail with NullReferenceException at the start site). Package counters for QuickFiler identical at both stages
  corroborate "only excluded lines changed". Table New Code Coverage cell `N/A` + omit the `New/changed-code coverage:`
  field in the comparison bullet; keep every C#/csharp/dotnet+coverage line free of N/A/UNVERIFIED.
- Hook-safe zero-file-language wording: checklist lines `none consulted (zero PowerShell files changed on this branch)` and
  a `PowerShell coverage gate: PASS by vacuity (...)` line, so a stale session `pr_context.summary.txt` listing .ps1 files
  cannot trip the narrowing regex on `N/A - out of scope`. Not yet confirmed against the validator; it accepted at #950
  only if the orchestrator's validator run passes (unknown at write time).
- THEME TEST NULL-DISPATCHER EXPOSURE analysis (FocusAndThemeTests call `EnsureUiThreadDispatcher()` and discard the scope,
  then read the static): count the null-restoring writes R4 performs before vs after. Pre-fix null baseline: one null write
  at `transactionA.Dispose()`. Post-fix: restore-to-parked at Dispose, then one null write at the `using (baseline)` exit via
  `EnsureScope.Dispose()` (`CompareExchange(parked, null)`). Count unchanged, write moved later; hazard belongs to the
  discarded-scope pattern (spec out of scope). Report "not observed" + structural count, not a finding of the change.
- `SynchronousBackgroundWorker : BackgroundWorker { RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null)); }` runs the
  privately subscribed async-void `DoWork` handler inline to its first incomplete await. With a test-owned
  `SynchronizationContext` installed and a TCS created with `RunContinuationsAsynchronously`, `SetResult` posts (never runs
  inline) and `Drain()` observes it; negative controls "Drain removed, release set -> fails" prove the ordering empirically.
- Reflog clock: executor Phase labels matched reflog epochs to the minute at -0400; the orchestrator-authored
  preflight-clearance-r2 label (12-10) led its commit epoch (04:44Z) by ~7.4 h and the gitignored checkpoint's times
  (12:03Z-14:05Z) are unreconcilable with the reflog. Chose my label 14-30 monotone after both; documented the derivation.
- `Action<BackgroundWorker>` in a file importing both `System` and `Microsoft.Office.Interop.Outlook` is unambiguous
  (`Outlook.Action` is non-generic); tests must write `System.Action` for the bare delegate (CS0104).

**Follow-ups owed to the orchestrator:** F-1 consolidate the three duplicated test helpers into QuickFiler.Test/TestSupport
(needs csproj Compile Include); F-2 QueueProcessing.cs:17 doc still says `RunWorkerAsync()`; F-3 QfcDatamodel.cs 495/500 with
dead legacy loader variants + `Worker_RunWorkerCompleted`; F-4 R4 `transactionA` lacks try/finally (R1/R5/R6 have it);
F-5 theme tests should hold the ensure scope; F-6 canonical `artifacts/csharp/coverage.xml` absent (recurring); F-7
clock-derived labels; F-8 `quality-tiers.yml` absent (promoted at #956); F-9 AC17 closure from CI.
