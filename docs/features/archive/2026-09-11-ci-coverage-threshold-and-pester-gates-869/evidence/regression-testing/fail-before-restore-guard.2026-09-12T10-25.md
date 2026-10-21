# Fail-before — Invoke-Restore.ps1 entry-point guard (P3-T2)

Timestamp: 2026-09-14T19-02

Expected Result: RED

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-Restore.Tests.ps1"; $c.Run.PassThru = $true; $c.Filter.FullName = "Invoke-Restore.ps1 entry-point guard*"; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

## Printed result lines, verbatim

```
PESTER Passed=0 Failed=1 Total=7
PESTER NotRun=6 Skipped=0 Inconclusive=0
PESTER SELECTED=1
FAILED: defines functions only and performs no work on dot-source || Expected 3, but got 12.
```

## Count reconciliation, and what the required "total count of 1" measures

Failed count: **1**, as required.

The task states a required total count of 1. As recorded in the sibling P3-T1 artifact, `$r.TotalCount` in Pester 5.6.1 counts every **discovered** test, including those the filter excluded, so it reads 7 here: the one selected case plus the six `NotRun` cases in the same file. The count of tests selected and run is **1**, recorded as `PESTER SELECTED=1` and reconciled by the identity `Total 7 = SELECTED 1 + NotRun 6`. The substantive condition — exactly one test case ran, and that case failed — holds exactly.

## The failure

```
Expected 3, but got 12.
```

On the pre-fix tree, `scripts/vscode/Invoke-Restore.ps1` carries **12** top-level statements that are neither the parameter block nor a function definition. The file defines no function at all: its entire body, from line 12 to line 39, sits unguarded at file scope. After P4-T2 extracts that body into `Invoke-RestoreMain`, introduces the two wrapper seams and adds the invocation guard, exactly three top-level statements remain: the strict-mode statement, the error-preference assignment, and the guard conditional.

The case never dot-sources or executes the production file. It parses it with `[System.Management.Automation.Language.Parser]::ParseFile` and asserts over the resulting abstract syntax tree, by the same technique the P3-T1 guard case uses.

## The name filter

The filter confines the run to the parser-only case, so neither of the other two `Describe` blocks in this file dot-sources the pre-fix production file. Dot-sourcing it would launch `vswhere.exe` and then `msbuild.exe` with the `/t:Restore` target against the real repository root. The file carries no file-level setup block; each `Describe` owns its own setup, which is what makes the filter effective.

## Structure of the new test file

`tests/scripts/vscode/Invoke-Restore.Tests.ps1` holds three `Describe` blocks and 7 cases in total:

1. `Invoke-Restore.ps1 entry-point guard` — exactly one case, the parser-only guard assertion above, asserting the guard body contains exactly one command whose name is `Invoke-RestoreMain`.
2. `Invoke-RestoreMain` — four cases covering the four scenarios P4-T2 lists: the missing-solution throw, the missing-vswhere throw, the unresolved-MSBuild throw, and the non-zero-exit throw.
3. `Invoke-Restore.ps1 wrapper seams` — exactly two cases that drive the seam bodies directly rather than mocking them, named `resolves the MSBuild path through the vswhere seam` and `forwards every argument array element as a separate positional argument`. The first uses a helper function defined inside that `Describe`'s setup block whose parameters bind the seam's own argument list; the second uses the `Join-Path` builtin. No external process is launched by either.

The two seam names `Get-RestoreMSBuildPath` and `Invoke-RestoreMSBuildExe` deliberately differ from the build script's `Get-MSBuildPath` and `Invoke-MSBuildExe`. Pester runs every test container in one runspace, so two same-named functions dot-sourced from two production files shadow each other and the mock registered for one file would silently govern the other.

Output Summary: RED as expected. One case selected, one case failed, on the assertion `Expected 3, but got 12`. This is the fail-before evidence for the missing invocation guard in the restore script; P4-T2 adds the guard and P4-T5 records the pass-after result.
