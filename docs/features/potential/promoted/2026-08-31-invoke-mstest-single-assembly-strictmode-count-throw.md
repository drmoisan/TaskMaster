# invoke-mstest-single-assembly-strictmode-count-throw (Issue #713)

- Date captured: 2026-08-31
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/invoke-mstest-single-assembly-strictmode-count-throw/ (Issue #713)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #713
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/713
- Last Updated: 2026-09-01
## Summary

`scripts/vscode/Invoke-MSTest.ps1` throws before running a single test whenever its `-SearchRoot` matches exactly one `*.Test.dll`. Under `Set-StrictMode -Version Latest` the discovery pipeline yields a bare `System.String` for a single match, and the guard that follows reads `.Count` on it. The scoped, single-project form of the wrapper is therefore unusable, and only the repository-wide `-SearchRoot .` form works.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable; the affected script is PowerShell, run under `pwsh -NoProfile`
- Command/flags used: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test -Configuration Debug`
- Data source or fixture: any project directory whose `bin/<Configuration>` tree contains exactly one `*.Test.dll`

## Steps to Reproduce

1. Build the solution so that `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` exists.
2. Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test -Configuration Debug`.
3. Observe that the script terminates before invoking `vstest.console.exe`.

The scalar half of the defect reproduces with no build at all:

```
pwsh -NoProfile -Command 'Set-StrictMode -Version Latest; $x = "one-string"; $x.Count'
```

## Expected Behavior

A `-SearchRoot` naming a single test project discovers that project's one test assembly and runs it, exactly as `-SearchRoot .` discovers nine assemblies and runs them. Scoping the run to one project is the natural way to get fast feedback on a single project's tests.

## Actual Behavior

The script throws `PropertyNotFoundException: The property 'Count' cannot be found on this object. Verify that the property exists.`

`Set-StrictMode -Version Latest` is set at `scripts/vscode/Invoke-MSTest.ps1:77`. The discovery pipeline at `:107-113` ends in `Select-Object -ExpandProperty FullName`, which unrolls to a bare `System.String` rather than a one-element array when exactly one object flows out of the pipeline. The guard at `:115` is:

```powershell
if (-not $testAssemblies -or $testAssemblies.Count -eq 0) {
```

For a non-empty string the left operand is `$false`, so `-or` proceeds to evaluate the right operand and reads `.Count` on a scalar, which StrictMode rejects. `:120` would fail identically at `$($testAssemblies.Count)`.

With `-SearchRoot .` the pipeline emits nine assemblies, the variable binds an array, `.Count` resolves, and the script behaves correctly. That is why the defect has gone unnoticed: the repository-wide form is the one in habitual use.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:

```
type=String
PropertyNotFoundException: The property 'Count' cannot be found on this object. Verify that the property exists.
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

No production code is affected and no CI job is affected, because CI and the habitual local invocation both use the repository-wide search root. The cost is developer and agent feedback latency: a per-project test run is the fast path, and it is unavailable, so every scoped verification has to run all nine assemblies. It also silently pushes plans and acceptance criteria toward the repository-wide run, which carries known load-driven flakiness that a single-assembly run does not.

## Suspected Cause / Notes

The guard predates the `Set-StrictMode -Version Latest` line, or was written against a search root that always matched more than one assembly. The same shape appears in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; both files should be checked, and any sibling script that assigns a pipeline result and then reads `.Count` under StrictMode.

Discovered during preparation for issue #663, where the plan and the spec both had to be steered away from the single-assembly form. It is unrelated to that fix and is recorded separately so the finding survives that feature's merge.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: force the array shape at the assignment with the array subexpression operator, `$testAssemblies = @(Get-ChildItem ... | Select-Object -ExpandProperty FullName)`, which makes `.Count` well-defined for zero, one, and many matches. Audit the sibling coverage wrapper for the identical shape.
- [x] Integration scenario to retest: run the wrapper with a `-SearchRoot` matching exactly one assembly, one matching several, and one matching none, and confirm the first two run and the third throws the intended "No test assemblies found" message rather than a property error.
- [x] Manual verification notes: the zero-match path must keep reporting the existing actionable message, which is the branch the current guard was written for.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
