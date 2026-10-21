# Fail-before — Invoke-VSBuild.ps1 entry-point guard (P3-T1)

Timestamp: 2026-09-14T18-58

Expected Result: RED

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-VSBuild.Tests.ps1"; $c.Run.PassThru = $true; $c.Filter.FullName = "Invoke-VSBuild.ps1 entry-point guard*"; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

## Printed result lines, verbatim

```
PESTER Passed=0 Failed=1 Total=16
PESTER NotRun=15 Skipped=0 Inconclusive=0
PESTER SELECTED=1
FAILED: defines functions only and performs no work on dot-source || Expected 3, but got 17.
```

Pester's own console lines for the same run:

```
Discovery found 16 tests in 113ms.
Filter 'FullName' set to ('Invoke-VSBuild.ps1 entry-point guard*').
Filters selected 1 tests to run.
Tests Passed: 0, Failed: 1, Skipped: 0, Inconclusive: 0, NotRun: 15
```

## Count reconciliation, and what the required "total count of 1" measures

Failed count: **1**, as required.

The task states a required total count of 1. The `$r.TotalCount` property of a Pester 5.6.1 result object counts every **discovered** test, including those the filter excluded, so it reads 16 here: the one selected case plus the fifteen `NotRun` cases in the same file. The count of tests actually selected and run is **1**, which is the quantity the requirement is about, and it is recorded three independent ways above:

- Pester's own line `Filters selected 1 tests to run.`
- the derived `PESTER SELECTED=1`, computed as passed plus failed plus skipped plus inconclusive;
- the identity `Total 16 = SELECTED 1 + NotRun 15`.

This is a measurement-semantics clarification, not a relaxation. The substantive condition — exactly one test case ran, and that case failed — holds exactly. The fifteen excluded cases could not be made to disappear from `TotalCount` without moving them to a different file, which the task forbids: it requires the three pure-helper `Describe` blocks and their six cases to be retained in this same file, and it requires the two new `Describe` blocks to be added to it.

## The failure

```
Expected 3, but got 17.
```

The assertion that failed is `$topLevelStatements.Count | Should -Be 3`. On the pre-fix tree, `scripts/vscode/Invoke-VSBuild.ps1` carries **17** top-level statements that are neither the parameter block nor a function definition, because its whole entry-point body sits unguarded at file scope from line 127 to line 167. After P4-T1 extracts that body into `Invoke-VSBuildMain` and adds the invocation guard, exactly three remain: the strict-mode statement, the error-preference assignment, and the guard conditional.

The case never dot-sources or executes the production file. It parses it with `[System.Management.Automation.Language.Parser]::ParseFile` and asserts over the resulting abstract syntax tree.

## The name filter, and why it is load-bearing

The filter confines the run to the parser-only case. Without it, the `BeforeAll` blocks of the four other `Describe` blocks in this file would each dot-source the pre-fix production file, and each dot-source would launch `vswhere.exe` and execute `Sync-PackageReferences.ps1` against the real repository root. The rewritten file carries **no file-level setup block**; each `Describe` owns its own setup, which is what makes the filter effective, because Pester 5 does not run the setup of a block that has no selected tests.

Confirmation that no production side effect occurred:

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- '*.csproj'`
EXIT_CODE: 0
Output verbatim: empty.

The filtered fail-before run wrote no project file.

## Structure of the rewritten test file

`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` now holds five `Describe` blocks and 16 cases in total:

1. `Invoke-VSBuild.ps1 entry-point guard` — exactly one case, the parser-only guard assertion above.
2. `ConvertTo-MSBuildPropertyArgument` — the two pre-existing cases, unchanged, with its own dot-sourcing setup block.
3. `Get-MSBuildBuildArguments` — the two pre-existing cases, unchanged, with its own setup block.
4. `Get-RequestedMSBuildProperties` — the two pre-existing cases, unchanged, with its own setup block.
5. `Invoke-VSBuildMain` — seven new cases covering the seven scenarios P4-T1 lists: the missing-solution throw, the missing-vswhere throw, the unresolved-MSBuild throw, the sync-script-absent branch, the sync-script-present branch asserting the seam receives the resolved repository root, the no-execute path asserting the MSBuild seam is invoked zero times, and the non-zero-exit throw.
6. `Invoke-VSBuild.ps1 wrapper seams` — exactly two cases that drive the seam bodies directly rather than mocking them, named `resolves the MSBuild path through the vswhere seam` and `forwards every argument array element as a separate positional argument`. Each passes an in-process command name in place of the executable path. The first uses a helper function defined inside that `Describe`'s setup block whose parameters bind the seam's own argument list; the second uses the `Join-Path` builtin, in the style of the splatting-seam case in `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1`. No external process is launched by either.

All six pre-existing helper cases are retained unchanged; only their setup placement moved, because the file-level setup block was removed.

Output Summary: RED as expected. One case selected, one case failed, on the assertion `Expected 3, but got 17`. The filtered run dot-sourced nothing and wrote no project file. This is the fail-before evidence for the missing invocation guard; P4-T1 adds the guard and P4-T5 records the pass-after result.
