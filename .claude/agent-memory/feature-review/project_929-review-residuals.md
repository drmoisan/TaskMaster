---
name: 929-review-residuals
description: #929 (package-manifest residuals; altcover imports, SVGControl redirects, client-id) reduced audit 2026-09-30T10-30 PASS 7/7 AC, 0 blocking, 12 non-blocking; no-Bash review again; executor labels led the embedded UTC clock by 38-72 min; AC satisfied by tests of a PRE-EXISTING detector rule (plan D1) accepted; follow-ups list
metadata:
  type: project
---

Reduced audit (minor-audit, parallel cohort bugs-2026-09-28, nested worktree `agent-a74dcedbc13b789fd`): PASS, 7/7 AC,
0 blocking, 12 non-blocking. Same no-Bash mechanics as [[928-review-residuals]] (Read/Grep/Glob only; HEAD from
`<session>/.git/worktrees/<wt>/HEAD` -> loose ref; gitignored CI JaCoCo read at `<sourcefile>` level; 3-`..` advertised
path). Session-cwd `artifacts/pr_context.summary.txt` was #936's and listed only .md/.yml/.csproj, so the hook's
language checks were disarmed again; clean PASS/FAIL rows written anyway.

**Accepted judgment calls worth reusing:**
- AC4 asked for a detector behaviour "and" its tests; the detector (`Find-PackageAbsentFromManifest`) pre-existed and the
  executor discharged the AC with two in-memory tests + a tree census test red-then-green + a what-if run (ABSENT 2->0),
  adding no production rule (plan D1). Evaluated PASS: the criterion names the behaviour, not a new rule.
- AC1 pathspec scope: two tracked `*.csproj.bak` copies still carry the removed token; MSBuild never reads `.bak`, so
  PASS with a follow-up (delete the eight tracked `.bak` files listed at P0-T1 BAK-TRACKED).
- AC7 "no new failures relative to the Phase 0 baseline" evaluated on the local baseline-vs-final pair; one CI mstest
  failure (`Transaction_SecondCallerCannotInstallUntilTheFirstRestores`) and one local iter-1 failure
  (`RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`, 5-s `Task.Wait`) on a zero-`.cs` diff =
  not attributable; the PR-time CI run on the head stays the orchestrator's gate.
- Modified `workflow_run` workflow needing an unprovisioned App secret: green-run gate recorded PARTIAL non-blocking
  because issue.md itself declares the credential item "not a merge gate" (see
  [[modified-workflow-green-run-manual-check]]).

**Evidence hygiene datapoint:** executor `Timestamp:` labels led the embedded RUN-START UTC stamps by 38-72 min and
drifted monotonically (P1-T2 `10-02` vs `13:24:02Z`; P2-T3 iter2 `10-58` vs `13:46:37Z`; corroborated by the bundled
PoshQC JaCoCo report name `09:47:03` local). Non-blocking per
[[evidence-timestamps-are-synthetic-cross-check-commit-dates]]; the JUNIT-WRITTEN-after-RUN-START pairs prove the runs.

**C# coverage noise:** projection delta +4 lines / +1 branch, all in UtilitiesCS, an assembly the change never touched
(see [[csharp-coverage-constants-nondeterministic]]). SVGControl (config-only change) byte-identical between projections.

**Follow-ups handed to the orchestrator (not filed):** wall-clock waits in the two QuickFiler.Test timing tests;
delete 8 tracked `.csproj.bak`; runbook line 301 "App ID location" stale; `dependabot-repair.yml` line 14 wrap;
test-2 per-assembly split; 11 other app.config Fizzler 1.3.0.0 redirects (already a potential entry).
