# invoke-mstest-scalar-count-strictmode (Potential Bug)

- Date captured: 2026-08-04
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`scripts/vscode/Invoke-MSTest.ps1` fails before reaching `vstest.console.exe` whenever assembly discovery finds exactly one test assembly. Lines 115 and 120 evaluate `$testAssemblies.Count` while `Set-StrictMode -Version Latest` is in force (line 77), and a single-item pipeline result is a scalar `String` rather than an array, so the property access throws.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- .NET/framework: .NET Framework 4.8.1 (`net481`) solution; script runs under PowerShell 7
- Command/flags used: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot SVGControl.Test -Configuration Debug`
- Data source or fixture: any search root containing exactly one built `*.Test.dll`

## Steps to Reproduce

1. Build a single test project so exactly one `*.Test.dll` exists under `<project>/bin/Debug/`.
2. Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot SVGControl.Test -Configuration Debug` from the repository root.
3. Observe the failure before any test executes.

The inverse case passes: `-SearchRoot .` discovers nine assemblies, `$testAssemblies` is an array, and the run completes.

## Expected Behavior

A search root containing one test assembly runs that assembly's tests, exactly as a search root containing several runs all of them. Assembly count should not change whether the script works.

## Actual Behavior

The script throws `PropertyNotFoundException` at line 115 and never invokes `vstest.console.exe`.

Directly verified 2026-08-04 under `Set-StrictMode -Version Latest`:

- `@('a.Test.dll') | Select-Object -First 1` yields type `String`; accessing `.Count` throws `PropertyNotFoundException: The property 'Count' cannot be found on this object. Verify that the property exists.`
- `@('a.Test.dll','b.Test.dll').Count` returns `2` without error.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: `PropertyNotFoundException: The property 'Count' cannot be found on this object. Verify that the property exists.`

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Any plan or workflow that scopes a test run to one project cannot use the repo-standard wrapper and must fall back to invoking `vstest.console.exe` directly. Encountered during issue #418, where three plan-task command citations named a single-assembly search root and had to be retargeted to the repo-wide form. The defect is in shared tooling, so it affects any caller, not only #418.

## Suspected Cause / Notes

`scripts/vscode/Invoke-MSTest.ps1`:

- Line 77 sets `Set-StrictMode -Version Latest`, which makes access to a non-existent property a terminating error instead of returning `$null`.
- Lines 107-113 build `$testAssemblies` via `Get-ChildItem ... | Where-Object { ... } | Select-Object -ExpandProperty FullName`. PowerShell unwraps a single-element pipeline result to a scalar, so the variable holds a `String` rather than a one-element array.
- Line 115 (`if (-not $testAssemblies -or $testAssemblies.Count -eq 0)`) and line 120 (`Write-Host "Discovered $($testAssemblies.Count) test assemblies."`) both then fail.

Worth checking whether the sibling `Invoke-MSTestWithCoverage.ps1` and any other `scripts/vscode/` script share the pattern; the same idiom may appear elsewhere.

## Proposed Fix / Validation Ideas

The one-line remedy is the array subexpression operator at both sites: `@($testAssemblies).Count`. Alternatively force the array at assignment by wrapping the pipeline in `@(...)`, which fixes both call sites at once and is the more robust shape.

- [ ] Unit coverage areas: a Pester test asserting the script resolves a single-assembly search root without throwing, and one asserting the multi-assembly path is unchanged. Mock the discovery seam rather than the `Get-ChildItem` call directly, per the wrapper-seam rule in `.claude/rules/powershell.md`.
- [ ] Integration scenario to retest: `-SearchRoot SVGControl.Test` (one assembly) and `-SearchRoot .` (nine assemblies), both reaching `vstest.console.exe`.
- [ ] Manual verification notes: `-NoExecute` is sufficient to confirm the argument list is built without invoking the test host.

Note for whoever takes this: PowerShell changes carry the PoshQC format -> PSScriptAnalyzer -> Pester toolchain and the `>= 85%` line / `>= 75%` branch coverage floors. That obligation is why the fix was deliberately kept out of the C#-only #418 change rather than folded into it.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
