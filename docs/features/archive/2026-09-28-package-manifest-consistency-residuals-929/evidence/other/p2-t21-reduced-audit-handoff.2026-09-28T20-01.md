# P2-T21 — Reduced-audit handoff index (issue 929)

Timestamp: 2026-09-30T11-22
Command: Enumerate the evidence tree with each artifact's first EXIT_CODE line; CMD-HYGIENE over the feature folder; pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & ".\scripts\hygiene\Test-RepositoryHygiene.ps1"; "GUARD_EXIT=$LASTEXITCODE"'
EXIT_CODE: 0
Output Summary: index of the 61 evidence .md artifacts produced before this one, the committed coverage copies, the commits, the CI runs, the observations the plan requires to be carried forward, and the pre-commit hygiene and CI hygiene-guard results (appended below).

## Anchors (P0-T1)

- BASE-SHA: 231e1c0b55105aeb626bf5a6e8d0266a567cacad
- P0-START: 481b33c594d8412cb64e604ff53295db215ac2f1
- MERGED-MAIN-ANCESTOR: 0

## Commits

| Task | SHA | Message | Pushed |
|---|---|---|---|
| P0-T21 | 488492f135c17c1ca4c8e6224bf7663a58c5e7b1 | docs(929): phase 0 baseline evidence | yes |
| P1-T14 | b96926588d562f994430e7ba7301de5de86f206c | fix(929): remove altcover imports, correct SVGControl redirects, pass client-id to the token action | yes |
| P2-T20 | 9e41ffbcf069b1b4f0aa7fe8e5eab2483c561530 | docs(929): final QC evidence, coverage projection and acceptance check-off | yes |

No P2-T1 formatter commit and no P2-T2 repair commit was made in either iteration (the formatter rewrote nothing and the analyzer reported ok true).

P2-T20 commit transcription (the P2-T20 artifact is inside that commit):
- Head: 9e41ffbcf069b1b4f0aa7fe8e5eab2483c561530
- git show --name-only --format= HEAD: 32 paths, all under the feature folder, including evidence/qa-gates/p2-t20-ac-status-summary.2026-09-28T20-01.md, evidence/qa-gates/p2-t7-coverage-projection.2026-09-28T20-01.jacoco.xml, evidence/qa-gates/p2-t7-test-results.2026-09-28T20-01.summary.txt, issue.md and the plan
- Porcelain after the commit: only agent-memory entries (the two modified MEMORY.md files and five untracked agent-memory notes)

## Fail-before / pass-after pair

- Fail-before: evidence/regression-testing/p1-t2-tree-test-fail-before.2026-09-28T20-01.md (RepositoryTreeConsistency.Tests.ps1 tests=4 failures=4; EXIT_CODE 1, ExpectedExitCode 1)
- Pass-after: evidence/regression-testing/p1-t11-tree-test-pass-after.2026-09-28T20-01.md (tests=4 failures=0; root 137 failures 0)

## Committed coverage evidence copies

- Baseline: evidence/baseline/p0-t12-coverage-projection.2026-09-28T20-01.jacoco.xml and evidence/baseline/p0-t12-test-results.2026-09-28T20-01.summary.txt
- Final: evidence/qa-gates/p2-t7-coverage-projection.2026-09-28T20-01.jacoco.xml and evidence/qa-gates/p2-t7-test-results.2026-09-28T20-01.summary.txt
- All four are package-level projections or trx-derived summaries; no raw document is committed.

## CI runs

| Purpose | Run id | Head SHA | Pester job | Pester conclusion | Run conclusion |
|---|---|---|---|---|---|
| Baseline (P0-T16) | 36666302259 | 231e1c0b55105aeb626bf5a6e8d0266a567cacad (main) | 109731601928 | success | success |
| Branch (P2-T3) | 36722780748 | b96926588d562f994430e7ba7301de5de86f206c | 109911885533 | success | failure (mstest-coverage job only: 1 of 7346 failed, Transaction_SecondCallerCannotInstallUntilTheFirstRestores) |

## Carried-forward observations

- ANALYZER-ITEM-STATE: aligned (P0-T6)
- MEETS-85 observations: P0-T12 true (C# line 85.91); P0-T16 true (PowerShell 94.51); P2-T3 true (PowerShell 94.51, both iterations); P2-T7 true (C# line 85.92, iteration 2). Convention 10 conflict (CLAUDE.md 80 percent versus the rules' 85 percent) is tracked as open issue 668.
- OUT-OF-SCOPE-RESIDUAL (P0-T18): eleven app.config files other than SVGControl and UtilitiesCS redirect Fizzler to 1.3.0.0 (QuickFiler, QuickFiler.Test, SVGControl.Test, Tags, TaskMaster, TaskTree, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS.Test); recorded in docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md.
- CSHARPIER-COUNTS-IGNORED-XML: false (P2-T4, both iterations)
- P1-T13: no potential entry authored; coordinator ruling 2026-09-30
- Maintainer follow-up (P2-T19): AC18 to AC20 of issue 911 remain deferred; the secret store is unconfirmed; DEPENDABOT_REPAIR_APP_ID must now hold the App's Client ID. Not an acceptance criterion and not a merge gate.

## GATE-SUBSTITUTION lines carried by the artifacts

- P0-T8: Pester provisioning replaced by the PoshQC MCP test route
- P0-T14, P2-T2 (iter1, iter2): PoshQC analyze ok flag stands in for a diagnostic count
- P0-T15, P1-T2, P1-T3, P1-T7, P1-T11: JUnit per-file counts stand in for a direct Pester run
- P0-T16: CI Pester job 109731601928 stands in for a local coverage run
- P2-T3 (iter1, iter2): CI Pester job on the pushed head stands in for a local coverage run

## Final QC loop iterations

- Iteration 1: P2-T1 to P2-T6 passed; P2-T7 failed (1 of 7346, RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs, a five-second wait that did not complete).
- Iteration 2: P2-T1 to P2-T7 passed on the unchanged tree; P2-T8 attests iteration 2.

## AC status summary

- Source: issue.md `## Acceptance Criteria`; Total 7; Checked off 7; Remaining 0.

## Evidence artifacts (path relative to the feature folder | task | EXIT_CODE)

| Path | Task | EXIT_CODE |
|---|---|---|
| evidence/baseline/p0-t1-worktree-anchor.2026-09-28T20-01.md | P0-T1 | 0 |
| evidence/baseline/phase0-instructions-read.2026-09-28T20-01.md | P0-T2 | 0 |
| evidence/baseline/p0-t3-sdk-bootstrap.2026-09-28T20-01.md | P0-T3 | 0 |
| evidence/baseline/p0-t4-tool-restore.2026-09-28T20-01.md | P0-T4 | 0 |
| evidence/baseline/p0-t5-package-restore.2026-09-28T20-01.md | P0-T5 | 0 |
| evidence/baseline/p0-t6-analyzer-item-census.2026-09-28T20-01.md | P0-T6 | 0 |
| evidence/baseline/p0-t7-dotnet-coverage.2026-09-28T20-01.md | P0-T7 | 0 |
| evidence/baseline/p0-t8-pester-provision.2026-09-28T20-01.md | P0-T8 | 0 |
| evidence/baseline/p0-t9-csharpier-check.2026-09-28T20-01.md | P0-T9 | 0 |
| evidence/baseline/p0-t10-msbuild-analyzers.2026-09-28T20-01.md | P0-T10 | 0 |
| evidence/baseline/p0-t11-msbuild-nullable.2026-09-28T20-01.md | P0-T11 | 0 |
| evidence/baseline/p0-t12-mstest-coverage.2026-09-28T20-01.md | P0-T12 | 0 |
| evidence/baseline/p0-t13-poshqc-format.2026-09-28T20-01.md | P0-T13 | 0 |
| evidence/baseline/p0-t14-poshqc-analyze.2026-09-28T20-01.md | P0-T14 | 0 |
| evidence/baseline/p0-t15-poshqc-test-mcp.2026-09-28T20-01.md | P0-T15 | 0 |
| evidence/baseline/p0-t16-pester.2026-09-28T20-01.md | P0-T16 | 0 |
| evidence/baseline/p0-t17-altcover-and-verifier-prefix.2026-09-28T20-01.md | P0-T17 | 0 |
| evidence/baseline/p0-t18-redirect-prefix.2026-09-28T20-01.md | P0-T18 | 0 |
| evidence/baseline/p0-t19-workflow-prefix-and-actionlint.2026-09-28T20-01.md | P0-T19 | 0 |
| evidence/baseline/p0-t20-hygiene.2026-09-28T20-01.md | P0-T20 | 0 |
| evidence/baseline/p0-t21-commit.2026-09-28T20-01.md | P0-T21 | 0 |
| evidence/regression-testing/p1-t1-tree-test-authored.2026-09-28T20-01.md | P1-T1 | 0 |
| evidence/regression-testing/p1-t2-tree-test-fail-before.2026-09-28T20-01.md | P1-T2 | 1 (expected 1) |
| evidence/regression-testing/p1-t3-verifier-import-tests.2026-09-28T20-01.md | P1-T3 | 1 (expected 1) |
| evidence/qa-gates/p1-t4-altcover-imports-removed.2026-09-28T20-01.md | P1-T4 | 0 |
| evidence/qa-gates/p1-t5-redirects-fixed.2026-09-28T20-01.md | P1-T5 | 0 |
| evidence/qa-gates/p1-t6-workflow-client-id.2026-09-28T20-01.md | P1-T6 | 0 |
| evidence/qa-gates/p1-t7-actionlint.2026-09-28T20-01.md | P1-T7 | 1 (expected 1) |
| evidence/qa-gates/p1-t8-runbook-client-id.2026-09-28T20-01.md | P1-T8 | 0 |
| evidence/qa-gates/p1-t9-readme-client-id.2026-09-28T20-01.md | P1-T9 | 0 |
| evidence/qa-gates/p1-t10-verifier-comment.2026-09-28T20-01.md | P1-T10 | 0 |
| evidence/regression-testing/p1-t11-tree-test-pass-after.2026-09-28T20-01.md | P1-T11 | 0 |
| evidence/qa-gates/p1-t12-verifier-postfix.2026-09-28T20-01.md | P1-T12 | 0 |
| evidence/qa-gates/p1-t14-commit.2026-09-28T20-01.md | P1-T14 | 0 |
| evidence/qa-gates/p2-t1-poshqc-format.iter1.2026-09-28T20-01.md | P2-T1 iter1 | 0 |
| evidence/qa-gates/p2-t2-poshqc-analyze.iter1.2026-09-28T20-01.md | P2-T2 iter1 | 0 |
| evidence/qa-gates/p2-t3-pester.iter1.2026-09-28T20-01.md | P2-T3 iter1 | 0 |
| evidence/qa-gates/p2-t4-csharpier-check.iter1.2026-09-28T20-01.md | P2-T4 iter1 | 0 |
| evidence/qa-gates/p2-t5-msbuild-analyzers.iter1.2026-09-28T20-01.md | P2-T5 iter1 | 0 |
| evidence/qa-gates/p2-t6-msbuild-nullable.iter1.2026-09-28T20-01.md | P2-T6 iter1 | 0 |
| evidence/qa-gates/p2-t7-mstest-coverage.iter1.2026-09-28T20-01.md | P2-T7 iter1 | 1 (failed; loop restarted) |
| evidence/qa-gates/p2-t1-poshqc-format.iter2.2026-09-28T20-01.md | P2-T1 iter2 | 0 |
| evidence/qa-gates/p2-t2-poshqc-analyze.iter2.2026-09-28T20-01.md | P2-T2 iter2 | 0 |
| evidence/qa-gates/p2-t3-pester.iter2.2026-09-28T20-01.md | P2-T3 iter2 | 0 |
| evidence/qa-gates/p2-t4-csharpier-check.iter2.2026-09-28T20-01.md | P2-T4 iter2 | 0 |
| evidence/qa-gates/p2-t5-msbuild-analyzers.iter2.2026-09-28T20-01.md | P2-T5 iter2 | 0 |
| evidence/qa-gates/p2-t6-msbuild-nullable.iter2.2026-09-28T20-01.md | P2-T6 iter2 | 0 |
| evidence/qa-gates/p2-t7-mstest-coverage.iter2.2026-09-28T20-01.md | P2-T7 iter2 | 0 |
| evidence/qa-gates/p2-t8-attestation-and-coverage-delta.2026-09-28T20-01.md | P2-T8 | 0 |
| evidence/qa-gates/p2-t9-file-size-audit.2026-09-28T20-01.md | P2-T9 | 0 |
| evidence/qa-gates/p2-t10-change-footprint.2026-09-28T20-01.md | P2-T10 | 0 |
| evidence/qa-gates/p2-t11-hygiene.2026-09-28T20-01.md | P2-T11 | 0 |
| evidence/qa-gates/p2-t12-ac1-checkoff.2026-09-28T20-01.md | P2-T12 | 0 |
| evidence/qa-gates/p2-t13-ac2-checkoff.2026-09-28T20-01.md | P2-T13 | 0 |
| evidence/qa-gates/p2-t14-ac3-checkoff.2026-09-28T20-01.md | P2-T14 | 0 |
| evidence/qa-gates/p2-t15-ac4-checkoff.2026-09-28T20-01.md | P2-T15 | 0 |
| evidence/qa-gates/p2-t16-ac5-checkoff.2026-09-28T20-01.md | P2-T16 | 0 |
| evidence/qa-gates/p2-t17-ac6-checkoff.2026-09-28T20-01.md | P2-T17 | 0 |
| evidence/qa-gates/p2-t18-ac7-checkoff.2026-09-28T20-01.md | P2-T18 | 0 |
| evidence/other/p2-t19-maintainer-followup.2026-09-28T20-01.md | P2-T19 | 0 |
| evidence/qa-gates/p2-t20-ac-status-summary.2026-09-28T20-01.md | P2-T20 | 0 |

Artifact count listed: 61 evidence .md files (at least 54), each present on disk at the time of this index.

## Pre-commit checks

CMD-HYGIENE over the feature folder:
- PATTERNS=3
- SELFTEST=1
- SELFTEST_NEG=0
- SCANNED=68
- HITS=0
- PRE-FIX-HITS: 0

CI hygiene guard run locally (scripts/hygiene/Test-RepositoryHygiene.ps1 over every tracked file):
- HYGIENE Findings=0
- GUARD_EXIT=0
