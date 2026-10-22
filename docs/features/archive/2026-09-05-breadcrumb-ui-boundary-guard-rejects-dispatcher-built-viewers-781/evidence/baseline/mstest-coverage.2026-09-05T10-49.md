# Baseline Repository-Wide Test and Coverage Run (issue #781)

Timestamp: 2026-09-05T16-26

Task: [P0-T8]

Command: `pwsh -NoProfile -File "$env:TEMP\taskmaster-781-coverage.ps1" -CoverageOutput "coverage\baseline-781.cobertura.xml" -ResultsDirectory "TestResults\baseline-781"`

The script body is the block quoted verbatim in [P0-T8] of the plan, saved to the named
throwaway session path outside the repository. The invocation was issued from the repository
root; console output was redirected to a log file under the user temporary directory.

EXIT_CODE: 0

ExpectedExitCode: 0

Setting the expectation equal to the observation is correct for this task and only this task: it
records the pre-existing state that every later gate is measured against rather than gating on
it.

## Run Result

- Collector banner: `dotnet-coverage v18.10.0.0 [win-x64 - .NET 10.0.11]`, VSTest version
  18.9.0 (x64).
- `COLLECT_EXIT_CODE: 0`
- `ASSEMBLY_COUNT: 9`
- Reported by vstest: `A total of 9 test files matched the specified pattern.`
- Result: `Test Run Successful.`
- Total tests: **6992**
- Passed: **6992**
- Failed: **0**
- Skipped: **0**
- Total time: 26.3684 seconds.

No `ASSEMBLY:` line printed by the script contains the substring `\.claude\`; a filter over the
nine printed lines for that substring returned a count of 0. The printed paths are absolute host
paths and are therefore inspected in the run output only and not copied into this artifact, as
the task directs. The nine assemblies are the `*.Test.dll` outputs under `bin\Debug\` of the
repository's nine test projects.

The four shell-icon classes that stall a local vstest run on this workstation
(`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`,
`HelperClasses.SysImageListHelperTests`, `EmailIntelligence.OSBrowser_Tests`) were excluded by
the script's `FullyQualifiedName!~` filter terms, together with `TestCategory!=LiveOutlook`.

## BASELINE_FAILURE_SET:

(observed failed-name list is empty: zero tests failed in this run)
DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue KNOWN-FLAKY #780

The known-flaky name is recorded under this heading whether or not it was observed to fail, per
the `BASELINE_FAILURE_SET` convention in the plan. In this run it was observed to **pass**
(`Passed TryAddValuesAsync_UpdatesExistingValue [319 ms]`). It remains a member of the set
because it fails only intermittently under a parallel coverage run and may therefore fail in
[P2-T6] while having passed here.

## Coverage Output Path — deviation observed and mechanically corrected

The collector wrote its Cobertura document to `coverage\coverage.cobertura.xml`, not to the
`coverage\baseline-781.cobertura.xml` path passed on the command line. The run reported
`Code coverage results: coverage\coverage.cobertura.xml.`

Cause, verified against the tree rather than inferred: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
declares `[string]$CoverageOutput = "coverage\coverage.cobertura.xml"` in its `param` block at
line 9. The plan's script dot-sources that file, and dot-sourcing a script that has a `param`
block re-creates its parameter variables in the calling scope with their default values, so
`$CoverageOutput` was overwritten with the script's default before the `dotnet-coverage collect`
line read it. `$ResultsDirectory` is not a parameter of the dot-sourced script, which is why the
results directory bound correctly and the TRX landed under `TestResults\baseline-781\`.

Correction applied: the produced document was copied verbatim to
`coverage\baseline-781.cobertura.xml`, which is the input path [P0-T9] reads. The copy is
byte-identical (18,101,564 bytes) and carries the same write time as the collector's output
(2026-09-05 16:25:40), which is inside this run's window; the run began at 16:25:17 per the TRX
file name. Both paths are under `coverage\`, which `.gitignore` line 144 ignores, so neither is
staged. This is a path correction only: no test was re-run, no coverage figure was altered, and
no plan task was added. The same clobbering will recur in [P2-T6], where the identical
correction applies to `coverage\final-781.cobertura.xml`.

TRX output remains under `TestResults\baseline-781\` and was not copied into this evidence
folder, per the plan's convention on host tokens in TRX files.

Output Summary: Baseline repository-wide run passed with 6992 of 6992 tests passed, 0 failed, 0
skipped, across 9 test assemblies, `COLLECT_EXIT_CODE: 0`. `BASELINE_FAILURE_SET` is the
known-flaky name alone, the observed failure list being empty. The coverage headline values from
the post-processed document are recorded below by [P0-T9].

## Baseline Coverage Headline (appended by [P0-T9])

Timestamp: 2026-09-05T16-40

Command: `pwsh -NoProfile -Command` over the [P0-T9] block, run from the repository root. The
block dot-sources `.\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1`, rewrites
`.\coverage\baseline-781.cobertura.xml` in place with `ConvertTo-KoverageCoberturaXml`, prints
the six root attribute values and the `ItemViewerBreadcrumbClassCount`, and calls
`Assert-CoberturaLineCoverageThreshold` inside `try`/`catch` as its last statement so a throw is
recorded rather than fatal.

EXIT_CODE: 0

Root attribute values of the post-processed Cobertura document, quoted from the single printed
line `line-rate=0.848347 branch-rate=0.791542 lines-covered=54922 lines-valid=64740 branches-covered=13176 branches-valid=16646`:

- `line-rate` = 0.848347
- `branch-rate` = 0.791542
- `lines-covered` = 54922
- `lines-valid` = 64740
- `branches-covered` = 13176
- `branches-valid` = 16646

ItemViewerBreadcrumbClassCount = 0

The observed class count is `0`, the value plan fact 5 predicts, so no non-zero class count is
reported to the orchestrator. The `[ExcludeFromCodeCoverage]` attribute on the `ItemViewer`
partial class at `QuickFiler/Viewers/ItemViewer.cs` line 20 applies to the whole type, including
the members declared in `ItemViewer.Breadcrumb.cs`, so the collector emits no class element for
that file and the changed production lines lie outside the coverage denominator. [P2-T7] answers
the changed-code coverage question on that basis.

BASELINE_FLOOR: MET 0.848347

`Assert-CoberturaLineCoverageThreshold` returned without throwing, so the recomputed root
`line-rate` of 0.848347 is at or above the 0.80 floor the helper enforces. No
`BASELINE COVERAGE FLOOR NOT MET` condition is reported to the orchestrator, and the
continue-on-breach branch of the [P0-T9] acceptance was not exercised.

These six values are the baseline side of the no-regression comparison [P2-T9] performs, and
`lines-valid` = 64740 is the denominator figure that makes that comparison meaningful.


## Baseline JaCoCo Projection (appended by [P0-T10])

Timestamp: 2026-09-05T16-41

Command: `pwsh -NoProfile -Command` over the [P0-T10] projection block, run from the repository
root. The block dot-sources `.\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1`, iterates
`/coverage/packages/package` of the post-processed baseline Cobertura document, derives each
package counter with the repository helper `Get-CoberturaPackageLineSummary`, and writes the
result to `FEATURE/evidence/baseline/coverage-baseline.jacoco.2026-09-05T10-49.xml`.

EXIT_CODE: 0

Output Summary: The projection file exists, its root element is `<report`, and it carries nine
`<package>` entries, one per package in the post-processed document. The printed derived totals
are `derived lines-covered=54922 lines-valid=64740 branches-covered=13176 branches-valid=16646`,
which equal the [P0-T9] root `lines-covered`, `lines-valid`, `branches-covered` and
`branches-valid` values exactly. The identity holds by construction because
`Get-CoberturaPackageLineSummary` is the same helper `Get-CoberturaCoverageSummary` sums into
the root attributes; no hand-written class-direct node count was used. This projection, not the
roughly 18 MB raw Cobertura document, is what [P2-T13] stages.
