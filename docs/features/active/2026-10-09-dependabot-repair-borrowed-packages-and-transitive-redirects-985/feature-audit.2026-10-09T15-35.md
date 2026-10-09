# Feature Audit: Dependabot repair, borrowed packages and transitive redirects (Issue #985)

- Review timestamp: 2026-10-09T15-35 (re-audit after remediation cycle R1)
- Work mode: `full-bug` (marker `- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` only

## Scope and Baseline

- Base branch: `origin/main` (caller-supplied; matches the PR-context resolved base)
- Merge base: `9911fe138952e2b93476850582847c2831e1cbbd` (2026-10-07T08:22:27-04:00)
- Head: `07d3a8a982d605eeb3bca77810955b4de5773ea8` (4 commits: `c83b6a965`, `79385ad07`, `de397b992`, `07d3a8a98`)
- Diff: `git diff 9911fe138...HEAD`, 107 files, +5386/-691
- PR context: `artifacts/pr_context.summary.txt` generated 2026-10-09 19:32:00 UTC for head `07d3a8a98` (current); `artifacts/pr_context.appendix.txt` alongside.
- Baseline behaviour: on main, `QuickFiler.Test`, `UtilitiesCS.Test` and `TaskTree.Test` hint-path packages their manifests do not declare (7 orphaned HintPaths, `evidence/regression-testing/fail-before-orphaned-hintpath.2026-10-09T14-15.md`), and the repair script has no redirect pass reachable without `-CandidateUpgrade` (`evidence/regression-testing/fail-before-redirect-sync.2026-10-09T14-15.md`).
- Prior review: `2026-10-09T14-55` (REMEDIATION_REQUIRED, B-1). R1 closed B-1 and the in-scope code observations CR-1 to CR-3.

## Acceptance Criteria Inventory

| ID | Criterion (abridged) | Evidence class | Source state at review start |
|---|---|---|---|
| AC1 | Borrowed packages declared at sibling versions; duplicate WebView2.Core reference group removed, one reference remains | pre-merge | checked |
| AC2 | `BindingRedirectSync.psm1` exports three functions; behaviours pinned by Pester; repair script invokes unconditionally, separate `RedirectSync` field, no sync records in `Report.Repair`, within file-size limit | pre-merge | checked |
| AC3 | Orphaned-HintPath Pester gate with positive examined count; fail-first and pass-after evidence | pre-merge | checked |
| AC4 | No file under `.github/workflows/**` modified | pre-merge | checked |
| AC5 | In-memory fixtures only; new-module coverage meets target; PowerShell and C# toolchains pass in a single pass | pre-merge | checked |
| AC6 | Local integration rehearsal on PR #984 branch: repair, restore, nullable and analyzer rebuilds succeed; BindingRedirectVerification passes; projections stored | pre-merge (local) | checked |
| AC7 | Post-merge: all required checks on PR #984 pass after `@dependabot recreate`, and the repair re-run writes nothing | pending CI | unchecked |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence and reviewer verification |
|---|---|---|
| AC1 | PASS | Manifests and csproj unchanged by R1 (`git show --stat 07d3a8a98` lists no C#-family file). Prior reviewer verification stands: declarations at WebView2 1.0.4191.47 and ObjectListView.Official 2.9.1 in the three test manifests, matching production siblings and HintPath folders; one `Microsoft.Web.WebView2.Core` reference remains in `QuickFiler.Test.csproj`. |
| AC2 | PASS | `Export-ModuleMember` (lines 324-328) lists the three public functions; the new `Get-RedirectDirection` is private. Stale-only, own-reference preference, highest by `[System.Version]`, unverifiable, unresolvable and idempotent behaviours remain pinned; reviewer run of `BindingRedirectSync.Tests.ps1` (24) and `RedirectSync.Tests.ps1` (9) passes. Wiring unchanged (unconditional call, `RedirectSync` field, `Report.Repair` exclusion asserted). Repair script 495 lines, within the limit. |
| AC3 | PASS | Unchanged by R1; reviewer run includes `RepositoryTreeConsistency.Tests.ps1`, passing. |
| AC4 | PASS | Reviewer: `git diff --name-only 9911fe138...HEAD -- .github scripts/benchmarks .github/actions` is empty; executor R1 re-check `evidence/qa-gates/workflows-untouched-r1.2026-10-09T15-30.md`. |
| AC5 | PASS | Temporary-file audit 0 in both R1-changed test files (`evidence/qa-gates/ps-temp-file-audit-r1.2026-10-09T15-29.md`); new module 100.00% lines (120/120), confirmed by reviewer in-session coverage. PowerShell toolchain R1 iteration 1 clean (format with no rewrite, analyze, MCP test 187/187); C# toolchain cycle-0 iteration clean and not invalidated because R1 changed no C#-family file (`evidence/qa-gates/csharp-not-applicable-r1.2026-10-09T15-30.md`). |
| AC6 | PASS | `evidence/other/integration-rehearsal.2026-10-09T14-35.md` unchanged. R1 alters only the report label and the `WrittenPath` de-duplication, neither of which changes which files are rewritten or their content; the R1 live-tree `-WhatIf` smoke shows 0 writes and 0 sync repairs on the current tree (`evidence/other/whatif-live-tree-r1.2026-10-09T15-26.md`). |
| AC7 | PENDING CI | Requires merge, `@dependabot recreate` on PR #984 and observed CI; owned by the item's orchestrator run per `acceptance-criteria-tracking` (CI-dependent criteria). Not a blocking finding. |

## Summary

- Total AC items: 7
- PASS: 6 (AC1 to AC6)
- PARTIAL: 0
- FAIL: 0
- PENDING CI: 1 (AC7)
- Review verdict for the branch: PASS. No blocking finding remains; B-1 from review 2026-10-09T14-55 is closed.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/spec.md`
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC7 (pending CI after merge and `@dependabot recreate` on PR #984)

## Acceptance Criteria Check-off

- AC1 to AC6 are already checked off in `spec.md` (lines 193-198); the reviewer's evaluation confirms each as PASS, so no change to the source file was required.
- AC7 remains unchecked (pending CI), line 199.
- No criterion text was modified and no new criterion was added.
