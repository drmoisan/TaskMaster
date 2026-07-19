# Coverage Baseline (P0-T5)

- Timestamp: 2026-07-19T08-48
- Command: `dotnet-coverage collect --output docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/baseline/coverage-baseline.2026-07-19T08-48.cobertura.xml --output-format cobertura --settings coverage.config -- "<VS18>/vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- EXIT_CODE: 0
- Output Summary: `Test Run Successful. Total tests: 4511, Passed: 4511, Total time: 21.51s`. Cobertura XML written to the baseline evidence folder.

## Numeric coverage (from Cobertura root `<coverage>` element)

- Overall line-rate: 0.7206858 (72.07%) — lines-covered 98272 / lines-valid 136359
- Overall branch-rate: 0.4844674 (48.45%) — branches-covered 12289 / branches-valid 25366

Note: the overall rate reflects the documented denominator behavior — `dotnet-coverage` instruments all loaded assemblies (including vendored Swordfish/SVGControl and other first-party assemblies whose own test projects are not in this single-assembly run), which depresses the aggregate. The scope-relevant figure for this feature is the targeted NewtonsoftHelpers production coverage below.

## Targeted UtilitiesCS/NewtonsoftHelpers production coverage (19 in-scope files)

- Method: dedup by `(filename, line-number)` across all `<class>` entries whose `filename` contains `NewtonsoftHelpers` and does NOT contain `Test` (production only). All 19 in-scope production files are present in the coverage set.
- NewtonsoftHelpers production line-rate: 0.9371 (93.71%) — dedup lines covered 1876 / valid 2002

## Coverage tool

Used `dotnet-coverage` directly (documented fallback) because `Invoke-MSTestWithCoverage.ps1` throws `The property 'Count' cannot be found on this object` under StrictMode when its test-assembly discovery returns a single scalar (only `UtilitiesCS.Test.dll` in this scope). The direct invocation reproduces the script's inner command (same `coverage.config` instrumentation excludes and `TaskMaster.cli.runsettings`) and emits identical-format Cobertura. Scope: `UtilitiesCS.Test` — the plan P0-T5 title scopes the baseline to "the UtilitiesCS test assemblies," and all in-scope changes are confined to `UtilitiesCS/NewtonsoftHelpers/`.
