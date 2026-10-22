# Phase 4 pass-after run (P4-T6)

Timestamp: 2026-09-02T22-52

Task: [P4-T6]

## Production change under test

[P4-T4] added `Get-MSTestAssemblyPathList` to scripts/vscode/Invoke-MSTest.ps1, placed after
`Invoke-VsTestExe` and before the file's `Set-StrictMode -Version Latest`, with
`[CmdletBinding()]`, `[OutputType([System.Array])]`, and mandatory `[string]$SearchRoot` and
`[string]$Configuration` parameters. [P4-T5] replaced the former top-level inline pipeline with a
single call to it. The file parses with zero parse errors.

### Recorded implementation detail: the unary comma on the return

The task contract specifies returning the discovery pipeline wrapped in `@(...)`, matching the
pattern already used by Invoke-MSTestWithCoverage.ps1's discovery block. That block's `@(...)`
sits at an assignment site, where it genuinely yields an array. A function `return` enumerates its
output, so `return @(pipeline)` alone unwraps the array again and hands the caller the very shapes
finding 7 is about. This was measured directly against the function before the correction:

```
ONE isNull=False isArray=False wrapCount=1
ZERO isNull=True  isArray=False wrapCount=0
```

That is, a single match returned a bare string and a zero-match run returned nothing at all. The
significance was also measured directly: under `Set-StrictMode -Version Latest`,

```
scalarCountThrew=PropertyNotFoundException: The property 'Count' cannot be found on this object.
nullCountThrew=The property 'Count' cannot be found on this object.
```

so `$testAssemblies.Count` at scripts/vscode/Invoke-MSTest.ps1 line 146 would still throw on a
single-match run. The unary comma (`return , @(...)`) is therefore the mechanism that delivers to
the caller the same array shape the cited Invoke-MSTestWithCoverage.ps1 pattern produces, and is
what makes the finding-7 fix effective rather than nominal. A comment in the function's
.DESCRIPTION records the reason.

## Command 1 — MCP test run

Command: mcp__drm-copilot__run_poshqc_test
  workspace_root = the item worktree repository root for this run
  scan_folders = ["scripts/vscode", "tests/scripts/vscode"]

EXIT_CODE: not applicable — this MCP tool returns no exit code and no counts. Payload:

```
ok: true
tool: run_poshqc_test
workspace_root: <item worktree repository root>
summary: Ran bundled PoshQC test against '<item worktree repository root>' with 2 selected scan
         folder(s).
```

## Command 2 — Direct Pester run over tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 (absolute path within the item
worktree; the file chosen by P4-T1), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, then
the explicit trailing branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

Counts: Passed 3, Failed 0, Skipped 0, Total 3. Pester version 5.6.1. Run duration 1.01s.

```
Describing Get-MSTestAssemblyPathList
  [+] returns an empty array when discovery matches nothing 253ms (226ms|27ms)
  [+] returns a single-element array when discovery matches exactly one assembly 14ms (12ms|2ms)
  [+] returns every match when discovery matches multiple assemblies 9ms (8ms|1ms)
```

All three cases pass, including the exactly-one-match case, which does not throw under the test
file's `Set-StrictMode -Version Latest`.

## Command 3 — Direct measurement of the returned array at each cardinality

To record the returned Count explicitly rather than only through the assertion verdict, the
production function was dot-sourced into a pwsh session in which `Get-ChildItem` was shadowed by a
local function returning zero, one, and three synthetic items in turn, and the returned value's
own `.Count` was read with no `@(...)` wrapper at the call site.

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper defining the shadow
`Get-ChildItem`, dot-sourcing scripts/vscode/Invoke-MSTest.ps1 with `-NoExecute` inside a
try/catch, then calling `Get-MSTestAssemblyPathList -SearchRoot "C:\repo" -Configuration "Debug"`.

EXIT_CODE: 0

Output:

```
ONE  isArray=True directCount=1
ZERO isArray=True directCount=0
MANY isArray=True directCount=3
```

The exactly-one-match case's returned array Count is explicitly recorded as **1**, and the value
is a real array (`isArray=True`), so the direct `.Count` access succeeds under
`Set-StrictMode -Version Latest` with no wrapper.

## Command 4 — Whole-folder no-regression run

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper, `Run.Path` =
tests/scripts/vscode (the whole folder), `Run.PassThru = $true`, `Output.Verbosity = "Normal"`,
and the explicit trailing exit branch.

EXIT_CODE: 0

Counts: Passed 79, Failed 0, Skipped 0, Total 79, across 9 test files. Run duration 19.89s.

Every file reported green, including
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1, which dot-sources the same
scripts/vscode/Invoke-MSTest.ps1 this phase modified.

The count reconciles exactly against the earlier phase totals: 74 after Phase 1, plus 1 (P2-T1),
plus 1 (P3-T3), plus 3 (P4-T2) equals 79.

## Output Summary

All three P4-T2 cases pass after the P4-T4 and P4-T5 production changes, with zero failed and zero
skipped. The exactly-one-match case's returned array Count is 1 and the returned value is a real
array at every cardinality, so the StrictMode member access that finding 7 reports is now safe.
The whole tests/scripts/vscode folder is green at 79 passed / 0 failed / 0 skipped, so nothing
regressed. Absolute host paths naming the item worktree were replaced with their
repository-relative equivalents; the `C:\repo\...` strings are synthetic fixture values.
