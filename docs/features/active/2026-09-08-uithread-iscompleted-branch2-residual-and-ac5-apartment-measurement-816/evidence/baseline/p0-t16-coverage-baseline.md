# P0-T16 — Baseline Cobertura figures for the threading source (baseline)

Timestamp: 2026-09-13T23-11

Command:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
dotnet-coverage collect --output coverage\p0-t16-baseline.cobertura.xml --output-format cobertura --settings coverage.config -- $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 0

Output Summary:

Inner test run: `Test Run Successful.`, Total tests 6332, Passed 6332, 45.7091 seconds. The
collector then reported `Code coverage results: coverage\p0-t16-baseline.cobertura.xml.`

### Figures for `UtilitiesCS/Threading/UiThread.cs`

| Figure | Value |
|---|---|
| Covered lines | **121** |
| Total lines | **126** |
| Line rate | **96.03%** |

### The derivation, stated mechanically

1. Take every `class` element in the Cobertura document whose `filename` attribute ends with the two
   path segments `Threading` and `UiThread.cs`. Three matched:
   - `UtilitiesCS.UiThread`
   - `UtilitiesCS.UiThread.SynchronizationContextAwaiter`
   - `UtilitiesCS.UiThread.SynchronizationContextAwaiter.<>c`

   (The `filename` attributes in the document are absolute host paths; they are not reproduced here,
   because this plan commits no absolute host path. Each ends with
   `<repo-root>\UtilitiesCS\Threading\UiThread.cs`.)
2. Union their `line` elements by the `number` attribute, so that a member split across several
   class elements is counted once. The union has 126 distinct numbers.
3. Count distinct numbers whose `hits` attribute is greater than zero as covered: 121. The
   remainder, 5, are uncovered.
4. Line rate = 121 / 126 = 96.03% to two decimal places.

The five uncovered line numbers are **38, 39, 40, 177, 178**.

### The positive control for the Phase 4 coverage gate

| Pre-change line | Source text | Cobertura observation |
|---|---|---|
| 176 | `if (ReferenceEquals(_context, _uiSyncContext))` | line element present, hits **1** |
| 177 | `{` — the opening brace of that exit | line element present, hits **0** |
| 178 | `return true;` | line element present, hits **0** |

Line 178 is **uncovered at baseline**, as the plan expects. That is the positive control for the
Phase 4 coverage gate in P4-T12: the transition from uncovered to covered is real and falsifiable,
not a restatement. Line 177 is recorded here as having a line element with hits 0 rather than as
non-executable, because the document does contain a `line` element for that number.

The Cobertura document is written under the gitignored coverage directory. It is neither committed
nor named as an evidence artifact.
