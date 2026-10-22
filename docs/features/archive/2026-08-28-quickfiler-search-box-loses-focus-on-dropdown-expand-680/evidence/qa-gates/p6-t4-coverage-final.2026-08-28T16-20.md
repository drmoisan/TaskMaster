# P6-T4 — Coverage-Mode Full Test Run (final pass)

Timestamp: 2026-08-28T16-30

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\coverage-final-680.cobertura.xml`
(launched detached with redirected stdout/stderr per DR-6's long-run mechanic, then polled to
completion. The script always applies `/TestCaseFilter:TestCategory!=LiveOutlook` and `/InIsolation`
internally.)

EXIT_CODE: 0

Output Summary:

### Cobertura root attributes

Read from the root `<coverage>` element of `coverage\coverage-final-680.cobertura.xml`:

- `line-rate` = **0.85279**
- `branch-rate` = **0.792235**
- `lines-covered` = 54715
- `lines-valid` = 64160

`Assert-CoberturaLineCoverageThreshold` did not throw, so the post-processed line coverage is at or
above the 80% floor. No pre-existing-shortfall clause is invoked.

### Test counts

- Total tests: **6839**
- Passed: **6839**
- Failed: **0**
- `Test Run Successful.` Total time 55.8502 seconds. Standard-error stream empty (0 bytes).
- Baseline was 6821 tests; the delta of **+18** is exactly the eighteen tests this plan adds
  (6 host-seam, 6 controller dismissal, 2 wiring, 4 additive-contract).

### DR-7 subset comparison

- Final failing-test FQN set: **{} (empty)**
- `BASELINE_FAILURE_SET` from P0-T10: **{} (empty)**
- Subset comparison: final failing set is a subset of `BASELINE_FAILURE_SET` — **PASS**. No test that
  passed at baseline fails now.

### DR-6 independent assembly enumeration

Executed from the worktree root with the same filter the script applies internally
(`\\bin\\Debug\\` supplied as `[regex]::Escape('\bin\Debug\')` to survive shell quoting; the
executed pattern was echoed back as `\\bin\\Debug\\`):

```
<repo-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
<repo-root>\SVGControl.Test\bin\Debug\SVGControl.Test.dll
<repo-root>\Tags.Test\bin\Debug\Tags.Test.dll
<repo-root>\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
<repo-root>\TaskTree.Test\bin\Debug\TaskTree.Test.dll
<repo-root>\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
<repo-root>\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
<repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
<repo-root>\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

Assertions (results only; no raw prefix is recorded):

- Every entry's `FullName` begins with the resolved worktree root: **True**
- No entry contains `\.claude\`: **True**
- Enumerated count = **9**; the script printed `Discovered 9 test assemblies.` — count parity: **True**

Three absolute paths printed by the script, recorded with `<repo-root>` substitution:

- `Using vstest.console: <program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- `Coverage output: <repo-root>\coverage\coverage-final-680.cobertura.xml`
- `Done. Coverage artifact: <repo-root>\coverage\coverage-final-680.cobertura.xml`

Acceptance: satisfied — the Cobertura file exists, the failing set is a subset of
`BASELINE_FAILURE_SET`, all numeric values are present, and the enumeration parity check passes.
