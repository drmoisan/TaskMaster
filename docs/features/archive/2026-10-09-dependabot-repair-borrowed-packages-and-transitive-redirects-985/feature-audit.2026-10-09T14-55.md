# Feature Audit: Dependabot repair, borrowed packages and transitive redirects (Issue #985)

- Review timestamp: 2026-10-09T14-55
- Work mode: `full-bug` (marker `- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` only

## Scope and Baseline

- Base branch: `origin/main` (caller-supplied; re-derived with `git merge-base HEAD origin/main`)
- Merge base: `9911fe138952e2b93476850582847c2831e1cbbd` (2026-10-07T08:22:27-04:00)
- Head: `de397b992868882530a447c662d21b3fbff123eb` (3 commits: `c83b6a965`, `79385ad07`, `de397b992`)
- Diff: `git diff 9911fe138...HEAD`, 70 files, +3945/-609
- PR context: `artifacts/pr_context.summary.txt` generated 2026-10-09 18:51:01 UTC for head `de397b992` (current); `artifacts/pr_context.appendix.txt` alongside.
- Baseline behaviour: on main, `QuickFiler.Test`, `UtilitiesCS.Test` and `TaskTree.Test` hint-path packages their manifests do not declare (7 orphaned HintPaths, `evidence/regression-testing/fail-before-orphaned-hintpath.2026-10-09T14-15.md`), and the repair script has no redirect pass reachable without `-CandidateUpgrade` (`evidence/regression-testing/fail-before-redirect-sync.2026-10-09T14-15.md`).

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
| AC1 | PASS | Reviewer Grep: `QuickFiler.Test/packages.config` lines 42 (WebView2 1.0.4191.47) and 47 (ObjectListView.Official 2.9.1); `UtilitiesCS.Test/packages.config` line 64 (WebView2 1.0.4191.47); `TaskTree.Test/packages.config` line 41 (ObjectListView.Official 2.9.1). Production siblings: `QuickFiler/packages.config` 19 and 22, `UtilitiesCS/packages.config` 57, `TaskTree/packages.config` 8, same versions. HintPaths in the three test csproj files name `Microsoft.Web.WebView2.1.0.4191.47` and `ObjectListView.Official.2.9.1`. `Include="Microsoft.Web.WebView2.Core` count in `QuickFiler.Test.csproj`: 1. |
| AC2 | PASS | Module exports the three functions (`Export-ModuleMember`, lines 302-306). Pinned behaviours: stale-only and idempotent (`It` "leaves a redirect whose newVersion is already deployed unchanged..."), own-reference preference, highest by `[System.Version]` (3.10.0.0 versus 3.9.0.0), ambiguous own reference fallback, unverifiable, unresolvable, `-WhatIf`, second run writes nothing (`RedirectSync.Tests.ps1`). Wiring: unconditional call at lines 449-450 with no `-CandidateUpgrade` guard; `RedirectSync` field lines 487-491; `Report.Repair` exclusion asserted in the RedirectSync suite. Repair script 493 lines. Reviewer re-run: 178/178 pass. |
| AC3 | PASS | New `It` in `RepositoryTreeConsistency.Tests.ps1` asserts `$examined -gt 0` and zero findings. Fail-before: exit 1 with 7 findings naming the three projects; pass-after: 5/5 (`evidence/regression-testing/`). Reviewer re-run includes the suite, passing. |
| AC4 | PASS | Reviewer: `git diff --name-only 9911fe138...HEAD -- .github` is empty; also `scripts/benchmarks` and `.github/actions` empty. |
| AC5 | PASS | Temporary-file audit 0 in all three test files; reviewer read confirms in-memory stores. New module 100.00% lines (114/114), above the 90% new-code target, confirmed by reviewer in-session coverage. PowerShell toolchain iteration 2 clean (format, analyze, test); C# toolchain iteration 1 clean (CSharpier, analyzer rebuild, nullable rebuild, MSTest 7427/7427). |
| AC6 | PASS | `evidence/other/integration-rehearsal.2026-10-09T14-35.md`: repair exit 0, 7 synchronised redirects, idempotent re-run writes 0; restore exit 0; analyzer and nullable `/t:Rebuild` 0 errors over 18 assemblies; `BindingRedirectVerification.Tests.ps1` 16/16. Evidence is projected summaries, not raw TRX or coverage documents. The rehearsal simulated the Dependabot bump of the two new test-project WebView2 entries (fidelity note D1), which AC7 confirms in the real flow. |
| AC7 | PENDING CI | Requires merge, `@dependabot recreate` on PR #984 and observed CI; cannot be verified before merge. Owned by the item's orchestrator run per `acceptance-criteria-tracking` (CI-dependent criteria). Not counted as a blocking finding. |

## Summary

- Total AC items: 7
- PASS: 6 (AC1 to AC6)
- PARTIAL: 0
- FAIL: 0
- PENDING CI: 1 (AC7)
- Review verdict for the branch: REMEDIATION_REQUIRED because of policy finding B-1 (account name in the committed plan, autonomous), which is independent of the acceptance criteria. No acceptance criterion is blocked.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/spec.md`
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC7 (pending CI after merge and `@dependabot recreate` on PR #984)

## Acceptance Criteria Check-off

- AC1 to AC6 were already checked off in `spec.md` by the executor; the reviewer's evaluation confirms each as PASS, so no change to the source file was required.
- AC7 remains unchecked (pending CI).
- No criterion text was modified and no new criterion was added.
