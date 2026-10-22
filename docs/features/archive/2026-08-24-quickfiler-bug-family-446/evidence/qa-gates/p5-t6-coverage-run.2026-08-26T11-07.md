# [P5-T6] Post-Change Cobertura Capture

Timestamp: 2026-08-26T11-07

Task: [P5-T6]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "docs\features\active\quickfiler-bug-family-446\evidence\qa-gates\coverage-final.cobertura.xml"`
EXIT_CODE: 0

This is the same script, with the same switches, that `[P0-T12]` used to produce the baseline, so
the two artifacts are directly comparable.

## Artifact

`docs/features/active/quickfiler-bug-family-446/evidence/qa-gates/coverage-final.cobertura.xml`
exists at the path this task names (10,604,471 bytes). The script post-processed the XML for
Koverage compatibility, as it reports at the end of the run. The baseline counterpart is
`docs/features/active/quickfiler-bug-family-446/evidence/baseline/coverage-baseline.cobertura.xml`
(10,602,263 bytes).

The evidence path used is the canonical `<FEATURE>/evidence/qa-gates/` location. No
`artifacts/csharp/coverage.xml` was produced by this task.

## Repository-wide coverage (Cobertura root `<coverage>` element)

Both required numeric rates are recorded:

- `line-rate`: `0.848402` (84.8402%)
- `branch-rate`: `0.787469` (78.7469%)

Supporting counters from the same element:

- `lines-covered`: `53843`
- `lines-valid`: `63464`
- `branches-covered`: `12694`
- `branches-valid`: `16120`
- `complexity`: `25042`

Baseline comparison (`[P0-T12]` root element): `line-rate` `0.847782`, `branch-rate` `0.786876`.
Both post-change rates are above their baseline values.

**Denominator statement.** These two figures are the **unfiltered repository-wide** rates read
directly from the Cobertura root element. They cover every package the run instrumented, including
vendored code, and they are not the filtered first-party denominator. They are recorded and
reported only; AC28 makes the repository-wide figure explicitly non-blocking. The blocking
threshold in this plan is applied by `[P5-T7]` to the changed-file scope, whose denominators are
derived by a different aggregation and are stated there.

## Test counts

- Total: `6500`
- Passed: `6500`
- Failed: `0`
- Skipped: `0`

Reproduced from the run: `Test Run Successful.` / `Total tests: 6500` / `Passed: 6500` /
`Total time: 58.3510 Seconds`. The runner prints no `Failed:` or `Skipped:` row when those counts
are zero.

The script printed `Discovered 9 test assemblies.`, matching the nine assemblies discovered by
`[P5-T5]` and by the `[P0-T12]` baseline.

## Failed-test set

(empty)

The failed count is `0`, so this task completes on its **primary** branch. The
pre-existing-baseline reconciliation branch is not taken and is not needed: `[P0-T12]` recorded an
empty failed set, so no reconciliation set exists and no pre-existing failure needs naming. No
recorded failure belongs to `QuickFiler.Test` because there is no recorded failure at all.

## Reconciliation of the total with `[P5-T5]`

`[P5-T5]` recorded 6501 tests and this task records 6500. The one-test difference is fully
accounted for and is not a skipped or lost test:
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` appends `/TestCaseFilter:TestCategory!=LiveOutlook`
to its inner `vstest.console.exe` invocation (visible at line 76 of that script), whereas the
`[P5-T5]` command specified by the plan passes no `/TestCaseFilter` and therefore runs the whole
suite. Exactly one test method in the repository carries that category:
`TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs:72`. `6501 - 1 = 6500`.

The `[P0-T12]` baseline was produced by this same script and therefore applied the same filter, so
the like-for-like comparison is `6482 -> 6500`, a net increase of 18 tests, with a failed count of
`0` in both runs.

## Output Summary

Post-change coverage run is green: `EXIT_CODE: 0`, 6500 tests, 6500 passed, 0 failed, 0 skipped
across 9 discovered test assemblies. `coverage-final.cobertura.xml` written to the canonical
qa-gates evidence path. Unfiltered repository-wide `line-rate` `0.848402` and `branch-rate`
`0.787469`, both above the `[P0-T12]` baseline values of `0.847782` and `0.786876`.
