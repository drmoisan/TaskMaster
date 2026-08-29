# P0-T10 — Baseline Full-Suite Coverage Run (Issue #680)

Timestamp: 2026-08-28T15-15

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\coverage-baseline-680.cobertura.xml`
(launched detached via `Start-Process ... -RedirectStandardOutput coverage\p0t10.log -RedirectStandardError coverage\p0t10.err.log`
per DR-6's long-run mechanic, then polled to completion. The script always applies
`/TestCaseFilter:TestCategory!=LiveOutlook` and `/InIsolation` internally.)

EXIT_CODE: 0

Output Summary:

### (a) Cobertura root attributes

Read from the root `<coverage>` element of `coverage\coverage-baseline-680.cobertura.xml`:

- `line-rate` = **0.85269**
- `branch-rate` = **0.792133**
- `lines-covered` = 54683
- `lines-valid` = 64130

The script's `Assert-CoberturaLineCoverageThreshold` did not throw, so the post-processed line
coverage is at or above the 80% floor. No pre-existing shortfall to record under spec AC-7.

### (b) Test counts

- Total tests: **6821**
- Passed: **6821**
- Failed: **0**
- `Test Run Successful.` Total time 44.2833 seconds.
- Standard-error stream was empty (0 bytes).

### (c) BASELINE_FAILURE_SET (DR-7)

`none` — the baseline run produced zero failing tests. The Phase 6 full-suite gate requires its
failing set to be a subset of this empty set.

### (d) DR-6 independent assembly enumeration

Executed from the worktree root with the same filter the script applies internally:

`Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match "\\bin\\Debug\\" -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' } | Select-Object -ExpandProperty FullName`

(The `\\bin\\Debug\\` regex was supplied as `[regex]::Escape('\bin\Debug\')` to survive shell
quoting; the executed pattern was echoed back as `\\bin\\Debug\\`, byte-identical to the form above.)

Enumerated list, worktree-root prefix replaced by the literal `<repo-root>`:

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
- `Coverage output: <repo-root>\coverage\coverage-baseline-680.cobertura.xml`
- `Done. Coverage artifact: <repo-root>\coverage\coverage-baseline-680.cobertura.xml`

Acceptance: satisfied — the Cobertura file exists, all numeric values are present (no `UNVERIFIED`
placeholders), `BASELINE_FAILURE_SET` is `none`, and the independent enumeration parity check passes.
