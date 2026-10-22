# Final QC Step 6 — Repository-wide test and coverage pass (issue #781)

Timestamp: 2026-09-05T17-07

Task: [P2-T6]

Command: `pwsh -NoProfile -File "$env:TEMP\taskmaster-781-coverage.ps1" -CoverageOutput "coverage\final-781.cobertura.xml" -ResultsDirectory "TestResults\final-781"`

The same throwaway session script used by [P0-T8], invoked from the repository root with console
output redirected to a log file under the user temporary directory.

EXIT_CODE: 0

No `ExpectedExitCode:` line is written, because the failed count is 0 and the mechanical rule
stated in [P2-T5] prescribes exactly that outcome for this case.

## Output Summary

- Collector result: `COLLECT_EXIT_CODE: 0`
- `ASSEMBLY_COUNT: 9`
- Result: `Test Run Successful.`
- Total tests: **6997**
- Passed: **6997**
- Failed: **0**
- Skipped: **0**
- Total time: 26.6271 seconds

Fully-qualified names of failed tests: **none**.

All three acceptance conditions hold:

1. Every `ASSEMBLY:` line printed is free of the substring `\.claude\`; a filter over the nine
   printed lines returned a count of 0.
2. `ASSEMBLY_COUNT:` is **9**, equal to the value recorded in
   `FEATURE/evidence/baseline/mstest-coverage.2026-09-05T10-49.md`, so both runs cover the same
   assembly set and their coverage denominators are comparable.
3. Every failed test name is a member of `BASELINE_FAILURE_SET`. The set of failed names is
   empty, so the condition holds in the only way it can without failures.

The total test count rose from 6992 at baseline to 6997, a net increase of five. That is exactly
the arithmetic of this plan's test edits: seven tests added by
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` less the two obsolete D4
tests deleted from `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`.

The known-flaky test `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`
(issue #780), which is a member of `BASELINE_FAILURE_SET` whether or not it fails on a given run,
was observed to **pass** in this run (`Passed TryAddValuesAsync_UpdatesExistingValue [4 ms]`), as
it did at baseline.

## Coverage Output Path — same deviation as [P0-T8], mechanically corrected the same way

The collector again wrote its Cobertura document to `coverage\coverage.cobertura.xml` rather than
to the `coverage\final-781.cobertura.xml` path passed on the command line, reporting
`Code coverage results: coverage\coverage.cobertura.xml.`

The cause is the one verified during [P0-T8] and recorded in that artifact:
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` declares
`[string]$CoverageOutput = "coverage\coverage.cobertura.xml"` in its `param` block at line 9, and
dot-sourcing a script that has a `param` block re-creates its parameter variables in the calling
scope with their default values, so `$CoverageOutput` is overwritten before the
`dotnet-coverage collect` line reads it. `$ResultsDirectory` is not a parameter of the
dot-sourced script, which is why the TRX landed correctly under `TestResults\final-781\`.

Correction applied: the produced document was copied verbatim to
`coverage\final-781.cobertura.xml`, the input path [P2-T7] and [P2-T8] read. The copy is
byte-identical (18,101,707 bytes) and carries the same write time as the collector's output
(2026-09-05 17:04:43), which is inside this run's window. Both paths are under `coverage\`, which
`.gitignore` line 144 ignores, so neither is staged. This is a path correction only: no test was
re-run to work around it, no coverage figure was altered, and no plan task was added.

TRX output remains under `TestResults\final-781\` and was not copied into this evidence folder.

## Post-Change Coverage Headline (appended by [P2-T7])

Timestamp: 2026-09-05T17-10

Command: `pwsh -NoProfile -Command` over the [P0-T9] block with `.\coverage\final-781.cobertura.xml`
substituted for the baseline path, run from the repository root.

EXIT_CODE: 0

Root attribute values of the post-processed final Cobertura document, quoted from the single
printed line `line-rate=0.848316 branch-rate=0.791421 lines-covered=54920 lines-valid=64740 branches-covered=13174 branches-valid=16646`:

- `line-rate` = 0.848316
- `branch-rate` = 0.791421
- `lines-covered` = 54920
- `lines-valid` = 64740
- `branches-covered` = 13174
- `branches-valid` = 16646

ItemViewerBreadcrumbClassCount = 0

`Assert-CoberturaLineCoverageThreshold` returned without throwing, so the recomputed root
`line-rate` of 0.848316 is at or above the 0.80 floor. The full comparison against the baseline
is recorded in `FEATURE/evidence/qa-gates/coverage-delta.2026-09-05T10-49.md` and the changed-code
determination in `FEATURE/evidence/qa-gates/changed-code-coverage.2026-09-05T10-49.md`.

## Final Coverage Artifacts (appended by [P2-T8])

Timestamp: 2026-09-05T17-11

Command: `pwsh -NoProfile -Command` over the [P0-T10] projection block with
`.\coverage\final-781.cobertura.xml` as input and
`FEATURE/evidence/qa-gates/coverage-final.jacoco.2026-09-05T10-49.xml` as output, followed by a
`Copy-Item` of the post-processed `.\coverage\final-781.cobertura.xml` to
`artifacts/csharp/coverage.xml`, creating `artifacts/csharp/` first.

EXIT_CODE: 0

Output Summary: All three acceptance conditions hold.

1. The JaCoCo projection exists, its first line is
   `<report name="TaskMaster C# (converted from Cobertura)">`, and it carries nine `<package>`
   entries. Its printed derived totals are
   `derived lines-covered=54920 lines-valid=64740 branches-covered=13174 branches-valid=16646`,
   which equal the final Cobertura root `lines-covered`, `lines-valid`, `branches-covered` and
   `branches-valid` values exactly. The per-package counters come from the repository's own
   `Get-CoberturaPackageLineSummary`, the helper `Get-CoberturaCoverageSummary` sums into the root
   attributes, so the identity holds by construction.
2. `artifacts/csharp/coverage.xml` exists and its root element is `<coverage`.
3. Its root `line-rate` attribute is **0.848316**, equal to the value recorded by [P2-T7].

AC8 names the canonical artifact as Cobertura, so the second step is a verbatim copy and not a
format conversion. That copy is git-ignored (`.gitignore` line 57 ignores `artifacts/`) and
exists for the feature reviewer, who reads the root `line-rate` and `branch-rate` attributes
directly. The compact JaCoCo projection, not the roughly 18 MB raw Cobertura document, is what
[P2-T13] stages into the feature folder.
