# P7-T8 — Inherited #439 Regression Coverage Still Passes and Was Not Edited

Timestamp: 2026-08-26T11-10

Command: `pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/trx/p7-t8"; "EXIT_CODE: $LASTEXITCODE"'`

Second command: `git status --porcelain --untracked-files=all -- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`

EXIT_CODE: 0

## Output Summary

**PASS at the primary acceptance condition. No degradation was used or available.**

### Test result

`Test Run Successful.` Counts read from the TRX `<ResultSummary><Counters>` element at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/trx/p7-t8/results.trx`:

| Metric | Value | Required |
|---|---:|---|
| total | 10 | equal to the `P0-T8` catalogued `[TestMethod]` count of **10** — MET |
| executed | 10 | — |
| passed | **10** | equal to total minus baseline-failing identifiers in this class (10 − 0 = 10) — MET |
| **failed** | **0** | 0 — MET |

Total time 1.6839 seconds.

### Catalogued-method reconciliation

The `P0-T8` catalogue in `evidence/baseline/phase0-instructions-read.md` recorded 10 `[TestMethod]`
members by name. All ten appear in the TRX and all ten passed; no eleventh identifier appeared and none
was missing:

| # | Method (from the `P0-T8` catalogue) | Present in TRX | Outcome |
|---:|---|---|---|
| 1 | `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability` | yes | Passed |
| 2 | `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` | yes | Passed |
| 3 | `Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome` | yes | Passed |
| 4 | `Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget` | yes | Passed |
| 5 | `Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget` | yes | Passed |
| 6 | `Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget` | yes | Passed |
| 7 | `Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild` | yes | Passed |
| 8 | `Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows` | yes | Passed |
| 9 | `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` | yes | Passed |
| 10 | `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` | yes | Passed |

### Read-only confirmation

`git status --porcelain --untracked-files=all -- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
produced **no output**. `P7-T2` independently established byte-identity with the `P0-T16` baseline
(`git hash-object` = `57af52e2ff05729e537274e8b14a00b0b00b6189`, 694 lines, both matching baseline), and
`P7-T3` confirmed the path is absent from the cumulative change set. The MUST-NOT-WRITE file was neither
edited nor extended.

### Degradation status

The `P0-T15` `BASELINE_FAILURE_SET` is EMPTY, so the conditional degradation is **unavailable** and the
gate stands at its primary condition `failed 0`, which was met absolutely.

### Retired criteria

This artifact is the evidence that the RETIRED criteria **AC-10, AC-12 and AC-13** — delivered by pull
request #605 and already checked in `spec.md` — remain satisfied by the inherited work after this
feature's changes. They are not re-implemented or re-verified as this feature's work; the inherited
regression suite that pins them is green and its file is untouched.
