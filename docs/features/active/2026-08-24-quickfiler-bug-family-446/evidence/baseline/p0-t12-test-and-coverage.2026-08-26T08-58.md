# [P0-T12] Baseline Full-Suite Tests with Cobertura Coverage

Timestamp: 2026-08-26T08-58

Task: [P0-T12]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "docs\features\active\quickfiler-bug-family-446\evidence\baseline\coverage-baseline.cobertura.xml"`
EXIT_CODE: 0

Execution note: the invocation was launched as a detached `pwsh` process writing to a log, because
a full-suite run exceeds the foreground command timeout available to this executor. The command
string, its arguments and its exit code are unchanged by that launch mechanism.

## Test Counts

- Total: `6482`
- Passed: `6482`
- Failed: `0`
- Skipped: `0`

Summary line reproduced from the run: `Test Run Successful.` / `Total tests: 6482` /
`Passed: 6482` / `Total time: 42.1878 Seconds`. The runner prints no `Failed:` or `Skipped:`
row when those counts are zero, so both are recorded as `0`.

## Failed-Test Set (reconciliation reference for `[P5-T5]`)

(empty — the failed count is zero, so no fully qualified test name is recorded)

Explicit statement required by this task's acceptance condition: **no recorded failure belongs to
`QuickFiler.Test.dll`**, because there is no recorded failure at all.

## Discovered Assemblies

The script printed only a count: `Discovered 9 test assemblies.`

The executor reproduced the list with the Command-conventions discovery prelude
(`Get-ChildItem -Path . -Recurse -Filter *.Test.dll -File`, filtered to `\bin\Debug\`, projected
through `Resolve-Path -Relative`, then filtered with `-notmatch "\.claude"`). The prelude
reported `COUNT=9`, matching the script, and produced these nine workspace-root-relative paths:

1. `.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
2. `.\SVGControl.Test\bin\Debug\SVGControl.Test.dll`
3. `.\Tags.Test\bin\Debug\Tags.Test.dll`
4. `.\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`
5. `.\TaskTree.Test\bin\Debug\TaskTree.Test.dll`
6. `.\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll`
7. `.\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll`
8. `.\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
9. `.\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll`

None of the nine recorded paths contains a `.claude` segment when expressed relative to the
workspace root. (The workspace root itself lies beneath a `.claude` directory, which is why the
exclusion is applied to the relative path and not the absolute one; an absolute-path test would
have excluded every assembly and produced a vacuously green run.)

## Repository-Wide Coverage (Cobertura root `<coverage>` element)

- `line-rate`: `0.847782` (84.7782%)
- `branch-rate`: `0.786876` (78.6876%)
- `lines-covered`: `53768`
- `lines-valid`: `63422`
- `branches-covered`: `12675`
- `branches-valid`: `16108`

## Artifact

`docs/features/active/quickfiler-bug-family-446/evidence/baseline/coverage-baseline.cobertura.xml`
exists (10,602,263 bytes). The script post-processed the XML for Koverage compatibility, as it
reports at the end of the run.

## Output Summary

Baseline full-suite run is green: 6482 tests, 6482 passed, 0 failed, 0 skipped, exit code 0 across
9 discovered test assemblies. Repository-wide baseline line-rate `0.847782` and branch-rate
`0.786876`. No pre-existing failure exists, so `[P5-T5]` and `[P5-T6]` have no reconciliation set
and must themselves report a failed count of `0`.
