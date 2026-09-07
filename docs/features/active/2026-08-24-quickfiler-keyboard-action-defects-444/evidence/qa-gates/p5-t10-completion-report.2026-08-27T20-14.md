# [P5-T10] Completion report

Timestamp: 2026-08-27T20-14
Command: none — this artifact is a report assembled from the Phase 4 gate evidence and `spec.md`
EXIT_CODE: 0
Output Summary: all four toolchain steps passed in a single final pass with no step auto-fixing a
file. Repository-wide line coverage 85.13 percent, branch 79.21 percent, both non-negative against
the Phase 0 baseline. The coverage-policy conflict is recorded as pre-existing and unresolved.

## The four toolchain commands actually run in the final pass

| # | Step | Command as executed | Exit code | Artifact |
| --- | --- | --- | --- | --- |
| 1 | Format | `dotnet tool run csharpier format QuickFiler\Controllers\KbdActions.cs QuickFiler\Controllers\QfcCollectionController.cs QuickFiler\Controllers\QfcItemController.Navigation.cs QuickFiler.Test\Controllers\KbdActionsTests.cs QuickFiler.Test\Controllers\KbdActionsRemainingBranchesTests.cs QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs QuickFiler.Test\Controllers\QfcItemController.NavigationTests.cs` | 0 | `p4-t1-format.2026-08-27T09-45.md` |
| 2 | Lint / static analysis | `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | `p4-t4-analyzers.2026-08-27T19-50.md` |
| 3 | Type check | `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | `p4-t5-typecheck.2026-08-27T19-51.md` |
| 4 | Test | `& $vstest @assemblies /Settings:scripts\vscode\TaskMaster.cli.runsettings /EnableCodeCoverage /InIsolation /Logger:"trx;LogFileName=p4-t6-final.trx" /ResultsDirectory:docs\features\active\quickfiler-keyboard-action-defects-444\evidence\qa-gates\p4-t6 /TestCaseFilter:"TestCategory!=LiveOutlook"` | 0 | `p4-t6-final-tests.2026-08-27T19-53.md` |

The read-only formatting verification that gates step 1 ran repository-wide as
`dotnet tool run csharpier check .`, exit 0, recorded at `p4-t2-format-check.2026-08-27T19-48.md`.

Both msbuild steps used `/t:Rebuild`, never `/t:Build`, and each log carries **zero** occurrences of
`Skipping target "CoreCompile"` — 81 `CoreCompile` references and 18 assemblies produced for the
analyzer step, 85 and 18 for the type-check step. A warm `/t:Build` returns exit 0 having skipped
`CoreCompile` on every project, because MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so it could not have failed. `/p:Nullable=enable` was **not** added to
either command.

## All four steps passed in a single pass with no step auto-fixing files

**Explicit statement: all four toolchain steps passed in one single final pass, and no step of that
pass auto-fixed or rewrote any file.**

The rewritten-file count for the final pass is `0`. It is derived from the `[P4-T1]`
before-and-after SHA-256 comparison over all seven owned paths: every before digest equals its after
digest. CSharpier's own `Formatted 7 files` line is a **processed** count, not a rewritten count, so
the digest comparison is the assertion of record. The derivation is independently corroborated by
the repository-wide `csharpier check .` over 1541 files reporting zero unformatted files, and by
`git status --porcelain -- '*.cs'` returning no modified path at any point during Phase 4.

Because no step failed and no step changed a file, the restart rule did not fire and the loop did not
return to step 1.

## Results

| Measure | Baseline (`[P0-T20]`) | Final | Delta |
| --- | --- | --- | --- |
| Tests total | 6686 | 6713 | +27 |
| Tests passed | 6686 | 6713 | +27 |
| Tests failed | 0 | **0** | 0 |
| Repository-wide line coverage | 85.04 percent | **85.13 percent** | **+0.09** |
| Repository-wide branch coverage | 79.12 percent | **79.21 percent** | **+0.09** |
| `KbdActions.cs` line rate | 0.9397590361445783 | 0.9897959183673469 | higher |
| `QfcItemController.Navigation.cs` line rate | 0.90678 | 0.92126 | higher |
| `SyncExpandedRegistrations` line rate | n/a (new member) | **1** | — |
| `lines-valid` (denominator) | 63921 | 63905 | -16 |

Both repository-wide deltas are non-negative, so the denominator-drift reconciliation branch of
`[P4-T11]` was not taken. The coverage figures come from the **unfiltered whole-run denominator**
produced by the repository's own `Invoke-MSTestWithCoverage.ps1` wrapper, which is the same command
and the same denominator as the Phase 0 baseline; that identity is what makes the two comparable.

No coverage figure is attributed to `QuickFiler/Controllers/QfcCollectionController.cs`. That class
carries `[ExcludeFromCodeCoverage]` at its declaration, so it is outside every coverage denominator
in both the baseline and the final document (decision D-P4).

## Coverage-policy conflict — pre-existing and unresolved

This repository carries **two mutually inconsistent coverage policies**. They are recorded here as
**pre-existing and unresolved**; this feature did not resolve them and did not silently select one.

| Source | Line floor | Branch floor | New-code floor |
| --- | --- | --- | --- |
| `CLAUDE.md` §UT2 | `>= 80%` | not stated | `>= 90%` for any new module, class, or method |
| `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` | `>= 85%` | `>= 75%` | not stated separately |

The two documents state different repository-wide line floors (80 versus 85 percent), only one states
a branch floor, and only one states a new-code floor. Nothing in either document says which is
authoritative, and neither cross-references the other. Resolving that requires a decision by the
repository maintainer about document precedence; it is outside a defect fix's scope.

This feature avoided needing an interpretation by clearing **both** readings: final line coverage
85.13 percent clears the 80 percent floor and the 85 percent floor; final branch coverage 79.21
percent clears the 75 percent floor; and the one new member, `SyncExpandedRegistrations`, reaches 100
percent line coverage, clearing the 90 percent new-code floor. Had a figure landed between the two
line floors — at 82 percent, say — this feature would have had to pick a policy, and it would then
have reported the conflict as blocking rather than as recorded.

### Acceptance Criteria Status

- Source: `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md`
- Total AC items: 57
- Checked off (delivered): 39 as of this report; `[P5-T11]` through `[P5-T24]` and `[P5-T29]` check
  off the remaining satisfiable items after this artifact is written
- Remaining (unchecked): 18 as of this report
- Items remaining: the 14 scope-discipline and toolchain criteria checked off by `[P5-T11]` through
  `[P5-T24]` and `[P5-T29]` immediately after this report, plus the four criteria that this feature
  cannot satisfy from inside its own branch:
  - AC-472-10 — the count-mismatch defect promoted to a potential entry **and** a GitHub issue, with
    the issue number recorded in this feature's PR body. The potential entry and GitHub issue #644
    both exist (commit `12256da4`); the PR-body clause can only be satisfied by the integration pull
    request. Deferral recorded by `[P5-T25]`.
  - AC-482-08 — checked off by `[P4-T20]`; listed here only because it was deferred out of Phase 3.
  - AC-482-11 — the deliberate behaviour widening stated in the PR body. Text supplied by `[P5-T9]`.
    Deferral recorded by `[P5-T26]`.
  - AC-482-12 — the corrected #482 trigger and severity stated in this spec **and repeated in the PR
    body**. The spec clause is satisfied; the PR-body clause is not. Text supplied by `[P5-T9]`.
    Deferral recorded by `[P5-T27]`.

`[P5-T28]` writes the full 57-row reconciliation with a final count. The definitive status summary is
that artifact, not this one, because five further check-off tasks run after this report.

## Acceptance

- The report names all four commands — met, in the table above, with exit codes and artifact paths.
- It carries the coverage-policy-conflict paragraph — met, under
  `## Coverage-policy conflict — pre-existing and unresolved`, recorded as pre-existing and
  unresolved rather than silently resolved.
- It carries an `### Acceptance Criteria Status` block whose source line names
  `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md` — met.
