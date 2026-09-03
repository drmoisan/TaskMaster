# Phase 1 expect-fail run (P1-T7)

Timestamp: 2026-09-02T22-27

Task: [P1-T7]

## Command 1 — MCP test run

Command: mcp__drm-copilot__run_poshqc_test
  workspace_root = the item worktree repository root for this run
  scan_folders = ["scripts/vscode", "tests/scripts/vscode"]

EXIT_CODE: not applicable — this MCP tool returns no exit code, no pass/fail/skip counts, no
per-test names, and no coverage figure. The returned payload is recorded verbatim below in place
of one, and all numeric and per-test evidence comes from Command 2.

MCP payload:

```
ok: false
tool: run_poshqc_test
workspace_root: <item worktree repository root>
summary: Command exited with code 5.
```

The payload flipped from the P0-T7 baseline's `ok: true` to `ok: false` with a non-zero
underlying command code, which is the expected signal while the Phase 1 expect-fail tests are in
place and the production fixes have not yet landed. The payload carries no counts, so the
individual verdicts below are read from Command 2.

## Command 2 — Direct Pester run over the Phase 1 regression scope

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path`
set to the two-element array
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 and
tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 (absolute paths within the
item worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, then the explicit
trailing branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

Counts: Passed 24, Failed 5, Skipped 0, Total 29. Pester version 5.6.1. Run duration 15.35s.

## Per-task verdicts

### [P1-T1] — FAILED as predicted

Test: `Get-CoberturaPackageLineSummary` / "accumulates line and branch totals across every class
in the package", in tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1.

Predicted failure: CommandNotFoundException, because Get-CoberturaPackageLineSummary does not
exist yet.

Observed:

```
CommandNotFoundException: The term 'Get-CoberturaPackageLineSummary' is not recognized as a name
of a cmdlet, function, script file, or executable program.
   at <ScriptBlock>, tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1:38
```

Match: exact.

### [P1-T2] — FAILED as predicted

Test: `Get-CoberturaPackageLineSummary` / "falls back to a zero rate when no class in the package
carries any lines", in tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1.

Predicted failure: CommandNotFoundException.

Observed:

```
CommandNotFoundException: The term 'Get-CoberturaPackageLineSummary' is not recognized as a name
of a cmdlet, function, script file, or executable program.
   at <ScriptBlock>, tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1:63
```

Match: exact.

### [P1-T3] — FAILED as predicted

Test: `ConvertTo-KoverageCoberturaXml` / "computes the merged per-file line-rate from the merged
rollup alone", in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1.

Predicted failure: the package node's line-rate and branch-rate attributes remain at the
fixture's stale input value ('0'), because no code path currently writes them after a merge.

Observed:

```
at $resultXml.SelectSingleNode('//package').'line-rate' | Should -Be '0.6',
   tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:273
Expected strings to be the same, but they were different.
Expected: '0.6'
But was:  '0'
```

Match: exact. The assertion that fails is the line-rate one; the branch-rate assertion is
non-discriminating for this branch-free fixture because '0' is both the stale input and the
correct post-fix value.

### [P1-T4] — FAILED as predicted

Test: `ConvertTo-KoverageCoberturaXml` / "preserves the primary class methods subtree and every
hits value when merging", in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1.

Predicted failure: `$methodNodes.Count` remains 1, containing only 'M', against the updated
assertion of 2.

Observed:

```
at $methodNodes.Count | Should -Be 2,
   tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:350
Expected 2, but got 1.
```

Match: exact.

### [P1-T5] — FAILED as predicted

Test: `Merge-CoberturaClassesByFilename` / "unions the methods of every group member into the
merged class", in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1.

Predicted failure: only method 'M' is present (today's clone-primary-only behavior).

Observed:

```
at $methodNames.Count | Should -Be 3,
   tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:535
Expected 3, but got 1.
```

Match: exact — one surviving method, which is the primary class's own 'M'.

### [P1-T6] — PASSED, as required

Test: `Merge-CoberturaClassesByFilename` / "takes the higher hits value when the second class
seen for a filename is strictly higher", in
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1.

Observed:

```
[+] takes the higher hits value when the second class seen for a filename is strictly higher
    4ms (4ms|0ms)
```

This task is deliberately not tagged expect-fail; the production max(hits) branch already
behaves correctly and this closes a test-coverage gap only.

## Output Summary

All five expect-fail tasks (P1-T1 through P1-T5) failed with exactly the failure each task
predicted: two CommandNotFoundException cases on the not-yet-created
Get-CoberturaPackageLineSummary, one stale package line-rate mismatch ('0' against '0.6'), and
two methods-union count mismatches (1 against 2, and 1 against 3). P1-T6 passed on the same run
against unmodified production code. Failed 5, Passed 24, Skipped 0 over 29 tests; direct-run
EXIT_CODE 1, which is the expected value at this point in the phase. Absolute host paths in the
captured Pester output were replaced with their repository-relative equivalents.
