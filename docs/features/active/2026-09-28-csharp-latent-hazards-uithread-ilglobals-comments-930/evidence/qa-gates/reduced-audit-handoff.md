# Reduced-audit handoff ([P2-T20])

Timestamp: 2026-09-29T09-26
Work Mode: minor-audit
AC Source: docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/issue.md, section `## Acceptance Criteria`

## Commits made by this run (`git log --format=%H%n%s ac819907f479ee18026993054e714dc2e056142f..HEAD`)

- df86ec9e76e3e30a1db800ce0959532c804a7edb docs(930): phase 0 baseline evidence
- 699ad109cd503cd1609cd7d874f0735bf9c2db8d fix(930): UiThread dispatcher-exit null guard, ILGlobals dead statics, stale doc-comment line counts
- 3b82d604c9f458c4c7ef14aadce43aa60d5571ac docs(930): final QC evidence
- The terminal commit made by [P2-T21] and the final plan-only check-off commit follow this artifact and are reported in the executor's return.

## Evidence artifacts by kind (under EVIDENCE)

- baseline (16): phase0-instructions-read.md, tree-anchor.md, preimplementation-gate.md, bootstrap-sdk.md, bootstrap-tool-restore.md, bootstrap-restore.md, bootstrap-dotnet-coverage.md, outlook-state.md, baseline-01-csharpier-check.md, baseline-02-analyzers.md, baseline-03-nullable.md, baseline-04-mstest-coverage.md, baseline-coverage.jacoco.xml, baseline-test-summary.txt, baseline-scoped-tests.md, baseline-census.md
- regression-testing (13): 889-build-red.md, 889-fail-before.md, 889-fix-applied.md, 889-build-green.md, 889-pass-after.md, 863-build-red.md, 863-fail-before.md, 863-fix-applied.md, 863-build-green.md, 863-pass-after.md, 863-reference-search.md, 862-comment-edit.md, file-size-advisory.md
- qa-gates (15): final-01-csharpier-format.md, final-02-csharpier-check.md, final-03-file-size.md, final-04-analyzers.md, final-05-nullable.md, final-06-mstest-coverage.md, final-coverage.jacoco.xml, final-test-summary.txt, coverage-comparison.md, toolchain-final-pass.md, concurrency-regime.md, footprint.md, evidence-hygiene.md, ac-status-summary.md, reduced-audit-handoff.md

## Acceptance criteria

TOTAL: 7 of 7
UNMET: NONE

## Deviations recorded

- Decision D2: the baseline formatter ran read-only (`csharpier check`); BASELINE-DRIFT: NONE.
- Decision D3: both coverage runs used the repository runner with `Get-DotnetCoverageArgumentList` overridden to add four `FullyQualifiedName!~` exclusions and a `/Blame` hang collector. Excluded classes: UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests, UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests, UtilitiesCS.Test.HelperClasses.SysImageListHelperTests, UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests. Reason: these classes call the Windows shell icon API, which hangs process-wide on this workstation; the repository mstest-coverage workflow runs them unfiltered on every pull request.
- Decision D4 (breaking public API change): the public static fields `ILGlobals.Cache` and `ILGlobals.modules` in UtilitiesCS were removed. The in-repo consumer check is [P1-T11] (solution-wide Rebuild with 0 Error(s) after the deletion; EVIDENCE/regression-testing/863-build-green.md) and [P1-T13] (no name, string or reflection reference remains in source scope; EVIDENCE/regression-testing/863-reference-search.md).
- Environment provisioning (pre-existing, not caused by this branch; recorded in EVIDENCE/baseline/baseline-02-analyzers.md): the `<Analyzer Include>` items on origin/main name Meziantou.Analyzer 3.0.235 (UtilitiesCS.csproj, VBFunctions.csproj) and MSTest.Analyzers 4.4.0 (SVGControl.Test.csproj) while packages.config restores 3.0.290 and 4.4.1, so the first baseline analyzer Rebuild failed with 4 CS0006 errors on this fresh worktree. The two named versions were installed into the gitignored packages directory; no tracked file changed, and the unmodified command then passed.
- Environment contention: the first SDK install attempt ([P0-T4]) failed because a sibling worktree held the shared temporary download file; it was re-run after the file was released.
- Hook refusal: the verbatim [P2-T9] span was refused by the EPIC_WORKTREE_REMOVAL_BLOCKED PreToolUse hook (a string match on the worktree path plus the removed-lines variable name); it was re-run with the variable renamed and identical output labels (EVIDENCE/qa-gates/concurrency-regime.md).
- Timestamp correction: several Phase 0 artifact `Timestamp:` values were first written as estimates and were corrected in the [P1-T16] commit to the observed file write times.

## Candidate follow-up (report to the caller; no obligation is created by this plan)

- The coverage runner scripts/vscode/Invoke-MSTestWithCoverage.ps1 hard-codes its vstest test-case filter and exposes no filter extension point and no hang timeout, which forced the Decision D3 argument-list override.
- The analyzer HintPath skew described above recurs on fresh worktrees after each analyzer package bump; the `<Analyzer Include>` versions should be updated together with packages.config.

## Reduced-audit input set

The complete input set for the reduced audit is: this artifact; the seven check-off lines `- [x] AC1` to `- [x] AC7` in issue.md; and the four committed tool-derived files under EVIDENCE: baseline/baseline-coverage.jacoco.xml, qa-gates/final-coverage.jacoco.xml, baseline/baseline-test-summary.txt and qa-gates/final-test-summary.txt.
