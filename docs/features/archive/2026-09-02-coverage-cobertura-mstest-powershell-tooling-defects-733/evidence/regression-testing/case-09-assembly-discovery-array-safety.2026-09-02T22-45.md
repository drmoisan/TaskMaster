# Case 09 — Assembly-discovery array safety at zero, one, and many (P4-T2, expect-fail)

Timestamp: 2026-09-02T22-45

Task: [P4-T2] [expect-fail]

## Target file

Per the [P4-T1] decision recorded in
evidence/other/phase4-test-file-placement.2026-09-02T22-43.md, the new Describe block was placed
in the new file tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1, because the
projected total for tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 (487 measured + 33
projected = 520) exceeds the 500-line ceiling.

## Change Made

Created tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 with
`Set-StrictMode -Version Latest`, a BeforeAll that resolves the repository root from
`$PSScriptRoot` and dot-sources scripts/vscode/Invoke-MSTest.ps1 through the same
`. $script:mstestScript -NoExecute` try/catch pattern used in
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1, and one
Describe 'Get-MSTestAssemblyPathList' block containing three It cases:

- (a) zero matches — `Get-ChildItem` mocked to return an empty array; the call is asserted not to
  throw and the returned array's Count asserted to equal 0.
- (b) exactly one match — `Get-ChildItem` mocked to return a single item; the call is asserted not
  to throw and the returned array's Count asserted to equal 1. This is the StrictMode regression
  case for finding 7.
- (c) multiple matches — `Get-ChildItem` mocked to return three items; the returned array's Count
  is asserted to equal 3.

A block comment cites issue #733 finding 7 and states why the zero, one, and many boundaries are
the ones pinned.

## Command

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 (absolute path within the item
worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, then the explicit trailing
branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

## Observed Failures (as predicted)

All three It cases failed with CommandNotFoundException on Get-MSTestAssemblyPathList.

```
Describing Get-MSTestAssemblyPathList
  [-] returns an empty array when discovery matches nothing 166ms (146ms|21ms)
   Expected no exception to be thrown, but an exception "The term 'Get-MSTestAssemblyPathList' is
   not recognized as a name of a cmdlet, function, script file, or executable program." was thrown
   from tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1:23
  [-] returns a single-element array when discovery matches exactly one assembly 25ms (23ms|2ms)
   Expected no exception to be thrown, but an exception "The term 'Get-MSTestAssemblyPathList' is
   not recognized as a name of a cmdlet, function, script file, or executable program." was thrown
   from tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1:34
  [-] returns every match when discovery matches multiple assemblies 20ms (19ms|1ms)
   CommandNotFoundException: The term 'Get-MSTestAssemblyPathList' is not recognized as a name of
   a cmdlet, function, script file, or executable program.
   at <ScriptBlock>, tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1:49
```

Counts: Passed 0, Failed 3, Skipped 0, Total 3. Pester version 5.6.1. Run duration 821ms.

## Output Summary

All three predicted pre-fix failures were observed exactly: CommandNotFoundException on
Get-MSTestAssemblyPathList in every case, because the function does not exist yet. Cases (a) and
(b) surface it through the `Should -Not -Throw` wrapper and case (c) as a direct
CommandNotFoundException, which is the same underlying cause reported in the two available
shapes. P4-T4 adds the function. Absolute host paths naming the item worktree were replaced with
their repository-relative equivalents in the captured Pester output; the `C:\repo\...` strings are
the tests' own synthetic fixture values.
