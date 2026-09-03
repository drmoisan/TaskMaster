# Case 07 — .claude worktree assemblies survive discovery (P2-T1, expect-fail)

Timestamp: 2026-09-02T22-37

Task: [P2-T1] [expect-fail]

## Change Made

Added one new It, "excludes assemblies discovered under a .claude worktree segment", to the
existing Describe 'Invoke-MSTestWithCoverageMain' block in
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1.

The test overrides the BeforeEach `Mock Get-ChildItem` with a two-item fixture:

- an ordinary path, `C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
- a worktree path under a `.claude` segment,
  `C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`

Both satisfy the existing `\bin\Debug\` discovery filter, so only a `.claude` clause can
separate them. The test also overrides `Mock Invoke-DotnetCoverageCollection` with a param-block
mock that captures the `-TestAssembly` value into `$script:capturedTestAssembly`, mirroring the
existing `Mock Invoke-VsWhereExe` capture pattern already used in the same Describe block. It
then calls `Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir` and asserts the captured
array equals the single-element array containing only the ordinary path. A comment citing issue
#733 finding 3 records why the assertion exists.

## Command

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 (absolute path within the item
worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, `Filter.FullName` =
"*excludes assemblies discovered under a*", then the explicit trailing branch
`if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

## Observed Failure (as predicted)

```
Describing Invoke-MSTestWithCoverageMain
  [-] excludes assemblies discovered under a .claude worktree segment 338ms (319ms|19ms)
   at Should -Be @('C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll'),
      tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:440
   Expected 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll', but got
   @('C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll',
     'C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').
```

Counts: Passed 0, Failed 1, Skipped 0 (26 NotRun, excluded by the name filter), Total 27.
Pester version 5.6.1.

## Output Summary

The predicted pre-fix failure was observed exactly: both fixture paths are present in the
captured `-TestAssembly` array because no `.claude` exclusion clause exists in the
`Invoke-MSTestWithCoverageMain` discovery predicate yet. P2-T3 adds that clause. Absolute host
paths naming the item worktree were replaced with their repository-relative equivalents in the
captured Pester output above; the `C:\repo\...` strings are the test's own synthetic fixture
values, not host paths.
