# P2-T18 — AC7 check-off

Timestamp: 2026-09-30T11-17
Command: Read the cited artifacts; Edit issue.md changing `- [ ] AC7:` to `- [x] AC7:`
EXIT_CODE: 0
Output Summary:
- Evidence read:
  - docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t5-package-restore.2026-09-28T20-01.md — cold restore: PACKAGE_DIRS_BEFORE=0, PACKAGE_DIRS_AFTER=172, "Build succeeded." and "0 Error(s)"
  - docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t6-analyzer-item-census.2026-09-28T20-01.md
  - docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t8-attestation-and-coverage-delta.2026-09-28T20-01.md and the seven iter2 artifacts it cites (P2-T1 to P2-T7, all EXIT_CODE 0)
  - docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t10-change-footprint.2026-09-28T20-01.md
- Quoted verbatim from the P0-T6 artifact:
  - `ANALYZER-ITEM-STATE: aligned`
  - `ANALYZER_ITEMS=162 FILES=17 UNRESOLVED=0`
- The cold restore of P0-T5 (PACKAGE_DIRS_BEFORE=0) was followed by the P0-T10, P0-T11, P2-T5 and P2-T6 rebuilds exiting 0 with no back-fill performed (decision D8; the amended AC7's back-fill clause is permissive and was not exercised, so AC7 is satisfied with its text unchanged).
- The P2-T10 footprint records 0 other *.csproj, packages.config or app.config paths, which shows that no analyzer item or manifest was changed on this branch.
- No-new-failure comparison (P2-T8):
  - C#: CSharpier check exit 0 with no findings (baseline exit 0, no findings); analyzer and nullable rebuilds exit 0 with 0 errors and OUT_LINES 36 (baseline identical); MSTest 7346 of 7346 passed with line 85.92 percent and branch 80.08 percent (baseline 7346 of 7346, 85.91 and 80.08; deltas +0.01 and 0.00).
  - PowerShell: PoshQC format rewrote nothing, PoshQC analyze ok true, local PoshQC test 137 of 137 passed (baseline 131 of 131); CI Pester job 379 passed, 0 failed, LinePercent 94.51 on run 36722780748 (head b96926588) against 373 passed, 0 failed, LinePercent 94.51 on run 36666302259 (main, head 231e1c0b5).
- Disclosure: the final QC loop needed two iterations. Iteration 1 failed at P2-T7 on one timing-dependent QuickFiler.Test test (RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces, a five-second wait that did not complete); iteration 2 passed on the unchanged tree. Separately, the CI mstest-coverage job on run 36722780748 failed one different QuickFiler.Test test (Transaction_SecondCallerCannotInstallUntilTheFirstRestores). This change edits no C# source, both tests passed in the local baseline and in the final local iteration, and both are reported to the caller as pre-existing timing-dependent tests outside this issue's scope.
- Change to issue.md: only the AC7 checkbox.
