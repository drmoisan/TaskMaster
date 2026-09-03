# Case 10 — Get-MSTestAssemblyPathList array-shape assertions are discriminating

Task: H1 (orchestrator-directed test hardening, outside the numbered plan; no plan checkbox added, no plan task renumbered).

## Defect addressed

The three It cases added by P4-T2 in `tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1`
read the returned value as `@($result).Count`. The `@(...)` at the assertion site restores array
shape locally, so those cases return the same result whether or not `Get-MSTestAssemblyPathList`
preserves the array. They failed pre-fix only with `CommandNotFoundException` (the function did not
exist), never on array shape, so they cannot fail on the behavior finding 7 is about.

## Fix

Two It cases were added to the same `Describe 'Get-MSTestAssemblyPathList'` block. They read the
returned value's own shape with no re-wrapping:

- `returns a value that is itself an array when discovery matches exactly one assembly` —
  asserts `($result -is [array])`, then reads `$result.Count` directly (unwrapped) and `$result[0]`.
- `returns a value that is itself an array when discovery matches nothing` —
  asserts `($result -is [array])`, then reads `$result.Count` directly (unwrapped).

The original three It cases were kept unchanged, as directed.

## Production code under test (unchanged by this task)

`scripts/vscode/Invoke-MSTest.ps1`, `Get-MSTestAssemblyPathList`, line 100:
the return statement carries a unary comma before the `@(...)` wrapper. That comma is
load-bearing: a function return enumerates its output, so `return @(...)` unwraps the array again
at the call site (zero matches yield `$null`, one match yields a bare string). The comma returns
the array as a single object and preserves the shape.

## Run 1 — unary comma TEMPORARILY REMOVED (expected: the two new cases FAIL)

Timestamp: 2026-09-02T22-57
Command: `pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = "REPO_ROOT/tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; Write-Host ("RESULT Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Skipped=" + $r.SkippedCount + " Total=" + $r.TotalCount); if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`
(REPO_ROOT stands for this item worktree's repository root; the literal path is not recorded here
per the no-absolute-host-path artifact rule.)
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- Pester v5.6.1. Discovery found 5 tests. Passed=3 Failed=2 Skipped=0 Total=5.
- FAILED `returns a value that is itself an array when discovery matches exactly one assembly` at
  line 67: "Expected $true, because the single-match return must not unwrap to a bare string, but
  got $false."
- FAILED `returns a value that is itself an array when discovery matches nothing` at line 77:
  "Expected $true, because the zero-match return must not unwrap to $null, but got $false."
- PASSED, in the same run, all three original P4-T2 cases:
  `returns an empty array when discovery matches nothing`,
  `returns a single-element array when discovery matches exactly one assembly`,
  `returns every match when discovery matches multiple assemblies`.
- This is the direct measurement of the defect: with the production array shape broken, the three
  `@($result).Count` cases still pass and only the two new unwrapped-read cases fail. The new
  assertions are therefore discriminating and the original three are not.

## Run 2 — unary comma RESTORED (expected: all five cases PASS)

Timestamp: 2026-09-02T22-57
Command: identical to Run 1.
EXIT_CODE: 0

Output Summary:
- Pester v5.6.1. Discovery found 5 tests. Passed=5 Failed=0 Skipped=0 Total=5.
- All five It cases in `Describe 'Get-MSTestAssemblyPathList'` passed, including both new
  unwrapped-read cases.

## Post-run state verification

The unary comma was restored before any further work. Verified by search:
`scripts/vscode/Invoke-MSTest.ps1` line 100 reads
`return , @(Get-ChildItem -Path $SearchRoot -Recurse -Filter '*.Test.dll' |`.
No net production-code change was made by task H1.
