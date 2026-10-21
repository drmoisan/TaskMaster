# P6-T39 C# coverage baseline re-anchored to the merge-base CI run (AC13)

Timestamp: 2026-09-29T23-40
Command: P6-T39 steps (0) to (6) as the plan states them: (0) GATE6 after the P6-T38 commit; (1) git fetch origin, git merge-base origin/main HEAD, git rev-parse origin/main, and gh run list --repo drmoisan/TaskMaster --branch main --commit MERGE-BASE-NOW --workflow CI --event push (completed runs, greatest database id) with gh run view --json jobs; (2) gh run view 36651909330 --repo drmoisan/TaskMaster --log filtered case-sensitively to mstest-coverage / lines matching First-party coverage, Total tests, Passed or Failed, teed to mstest-baseline-ci.txt under (Join-Path $env:TEMP "hygiene-927"); (3) gh pr view bug/evidence-and-identity-hygiene-sweep-927 with gh run list --commit PR-HEAD --workflow CI and gh run view --json jobs; (4) the step (2) filter over run 36664415704, teed to mstest-pr-ci.txt under the same scratch expression; (5) the comparison payload (one parser Get-Figures and one comparator Compare-Figures over the two scratch files, csharp-coverage-projection.md and p0-t14-mstest-coverage-baseline.md; ends with exit 0); (6) this artifact and the check-offs
EXIT_CODE: 0
Output Summary:
- EXIT_CODE above is the step (5) payload's exit code (explicit exit 0). Labelled lines (C3): FETCH-EXIT=0; BASELINE-LOG-EXIT=0; PR-LOG-EXIT=0.
- CONFIRMING-RUN: HYGIENE Findings=0 (GATE6 after the P6-T38 commit 776f82325; GUARD-EXIT=0; GUARD-SECONDS=40)

Step (1), merge base and main's CI push run:

- FETCH-EXIT=0
- MERGE-BASE-NOW=ddbab26a0149bf2ca5d0256e60686ad79e74d90c
- MAIN-TIP-NOW=ddbab26a0149bf2ca5d0256e60686ad79e74d90c
- MAIN-MOVED=0
- BASELINE-MSTEST-RUN-COUNT=1
- BASELINE-MSTEST-RUN-ID=36651909330
- BASELINE-MSTEST-RUN-HEAD=ddbab26a0149bf2ca5d0256e60686ad79e74d90c (equal to MERGE-BASE-NOW)
- BASELINE-MSTEST-RUN-CONCLUSION=success
- Baseline jobs:

```text
BASELINE-JOB| build-nullable / Build with nullable warnings treated as errors	success
BASELINE-JOB| format-check / Verify formatting	success
BASELINE-JOB| mstest-coverage / Run MSTest suite with coverage	success
BASELINE-JOB| pester / Run Pester suite with coverage	success
BASELINE-JOB| actionlint / actionlint	success
BASELINE-JOB| build-analyzers / Build with analyzers and code style enforcement	success
```

Step (2), BASELINE-CI-MSTEST: run 36651909330, head ddbab26a0149bf2ca5d0256e60686ad79e74d90c (transcribed verbatim; three lines on an all-green run, no runner path present):

```text
BASELINE-CI-MSTEST| mstest-coverage / Run MSTest suite with coverage	UNKNOWN STEP	2026-09-30T00:50:27.8820402Z Total tests: 7346
BASELINE-CI-MSTEST| mstest-coverage / Run MSTest suite with coverage	UNKNOWN STEP	2026-09-30T00:50:27.8820851Z      Passed: 7346
BASELINE-CI-MSTEST| mstest-coverage / Run MSTest suite with coverage	UNKNOWN STEP	2026-09-30T00:50:59.4945399Z First-party coverage: lines 56482/65736 (85.92%), branches 13657/17054 (80.08%)
```

Step (3), pull-request head and run:

- PR-NUMBER=943
- PR-HEAD=67a69cb23916878c436fa570d587847b7a0745fa
- PR-ANCESTRY-EXIT=0
- PR-RUN-COUNT=1
- PR-RUN-ID=36664415704
- PR-RUN-HEAD=67a69cb23916878c436fa570d587847b7a0745fa (equal to PR-HEAD)
- PR-RUN-STATUS=completed
- PR-RUN-CONCLUSION=success
- Pull-request jobs:

```text
PR-JOB| mstest-coverage / Run MSTest suite with coverage	success
PR-JOB| hygiene / Repository hygiene guard	success
PR-JOB| format-check / Verify formatting	success
PR-JOB| build-analyzers / Build with analyzers and code style enforcement	success
PR-JOB| pester / Run Pester suite with coverage	success
PR-JOB| actionlint / actionlint	success
PR-JOB| build-nullable / Build with nullable warnings treated as errors	success
```

Step (4), PR-CI-MSTEST: run 36664415704, head 67a69cb23916878c436fa570d587847b7a0745fa (transcribed verbatim):

```text
PR-CI-MSTEST| mstest-coverage / Run MSTest suite with coverage	UNKNOWN STEP	2026-09-30T03:31:09.1562253Z Total tests: 7346
PR-CI-MSTEST| mstest-coverage / Run MSTest suite with coverage	UNKNOWN STEP	2026-09-30T03:31:09.1563916Z      Passed: 7346
PR-CI-MSTEST| mstest-coverage / Run MSTest suite with coverage	UNKNOWN STEP	2026-09-30T03:31:51.7435036Z First-party coverage: lines 56478/65736 (85.92%), branches 13656/17054 (80.08%)
```

Step (5), comparison output (verbatim):

```text
BASELINE-CI-FIRST-PARTY-VALUES=1
BASELINE-CI LinePercent=85.92 BranchPercent=80.08 LinesCovered=56482 LinesValid=65736 BranchesCovered=13657 BranchesValid=17054
BASELINE-CI TotalLines=1 PassedLines=1 FailedLines=0 Total=7346 Passed=7346 Failed=0
PR-CI-FIRST-PARTY-VALUES=1
PR-CI LinePercent=85.92 BranchPercent=80.08 LinesCovered=56478 LinesValid=65736 BranchesCovered=13656 BranchesValid=17054
PR-CI TotalLines=1 PassedLines=1 FailedLines=0 Total=7346 Passed=7346 Failed=0
LOCAL-P6-T9-FIRST-PARTY-VALUES=1
LOCAL-P6-T9 LinePercent=85.92 BranchPercent=80.08 LinesCovered=56479 LinesValid=65736 BranchesCovered=13656 BranchesValid=17054
LOCAL-P6-T9 TotalLines=0 PassedLines=0 FailedLines=0 Total=-1 Passed=-1 Failed=0
STALE-P0-T14-FIRST-PARTY-VALUES=1
STALE-P0-T14 LinePercent=85.93 BranchPercent=80.09 LinesCovered=56486 LinesValid=65737 BranchesCovered=13657 BranchesValid=17052
STALE-P0-T14 TotalLines=0 PassedLines=0 FailedLines=0 Total=-1 Passed=-1 Failed=0
MSTEST-COMPARE=NOT-BELOW
LOCAL-VS-CI-BASELINE=NOT-BELOW
CONTROL-MSTEST-COMPARE=BELOW
PR-CI-FAILED-ZERO=1
PR-CI-PASSED-NOT-BELOW=1
```

- The comparison is on the two-decimal percentages as printed, per the plan. Recorded, not gated: at the same denominators (65736 lines, 17054 branches) the pull-request run covers 4 fewer lines (56478 against 56482) and 1 fewer branch (13656 against 13657) than main's run at the merge base; both differences are below the 0.01 display resolution. The branch changes no production C# file (P6-T14 PROD-CS=0).
- Local P6-T9 figures (reference): line 85.92 (56479 of 65736), branch 80.08 (13656 of 17054). LOCAL-VS-CI-BASELINE=NOT-BELOW.
- P0-T14 figures (stale): line 85.93 (56486 of 65737), branch 80.09 (13657 of 17052). P0-T14 was measured on the pre-merge tree at the P0-T2 merge base 177b6d78e.
- Negative control: CONTROL-MSTEST-COMPARE=BELOW (local P6-T9 85.92 and 80.08 against stale P0-T14 85.93 and 80.09, read by the same parser and comparator), which shows the comparator can report BELOW.
- CI-to-CI evidence of main's movement: run 36484682458 at 177b6d78e, lines 56485 of 65737 at 85.93 percent, branches 13657 of 17052 at 80.09 percent, 7343 tests (transcribed from this plan, not re-read by this task; read from that run's log by preflight round 15 on 2026-09-29: head 177b6d78e1b2408e5aedbd794cef3aad6b7fb372, branch main, push, conclusion success, `First-party coverage: lines 56485/65737 (85.93%), branches 13657/17052 (80.09%)`, `Total tests: 7343`, `Passed: 7343`); run 36651909330 at ddbab26a0, the observed baseline lines above (85.92 and 80.08, 7346 tests).
- PROJECTION-CORRECTION: csharp-coverage-projection.md line 36 says the ddbab26a0 merge added only QuickFiler.Test test files and documents; measured from the Phase 0 base 177b6d78e, main also changed UtilitiesCS production and test files, ILGlobals.cs and UiThread.cs among them, and the QuickFiler production files BreadcrumbBridgeCoordinator.Search.cs and BreadcrumbItemViewerLifecycleCoordinator.Search.cs. The projection artifact is not edited.
- Scratch handling: the filtered log lines were kept only under the scratch expression (Join-Path $env:TEMP "hygiene-927"); the unfiltered log was never written. This artifact carries placeholders and expressions only and no absolute host path.

Outcome: MAIN-MOVED=0; the mstest-coverage / PR-JOB line reads success; MSTEST-COMPARE=NOT-BELOW; PR-CI-FAILED-ZERO=1; PR-CI-PASSED-NOT-BELOW=1; CONTROL-MSTEST-COMPARE=BELOW. AC13: MET (P6-T39, re-anchored). P6-T9 ticked; the RE-ANCHORED line appended to csharp-toolchain-pass.md.
