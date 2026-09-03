# Phase 1 pass-after run (P1-T13)

Timestamp: 2026-09-02T22-37

Task: [P1-T13]

## Command 1 — MCP test run

Command: mcp__drm-copilot__run_poshqc_test
  workspace_root = the item worktree repository root for this run
  scan_folders = ["scripts/vscode", "tests/scripts/vscode"]

EXIT_CODE: not applicable — this MCP tool returns no exit code, no pass/fail/skip counts, no
per-test names, and no coverage figure. The returned payload is recorded verbatim below in place
of one, and all numeric and per-test evidence comes from Command 2.

MCP payload:

```
ok: true
tool: run_poshqc_test
workspace_root: <item worktree repository root>
summary: Ran bundled PoshQC test against '<item worktree repository root>' with 2 selected scan folder(s).
```

The payload returned to `ok: true`, reversing the `ok: false` / "Command exited with code 5"
recorded by P1-T7 while the expect-fail tests were unsatisfied.

## Command 2 — Direct Pester run over the Phase 1 regression scope

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path`
set to the same two-element array used by P1-T7 —
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 and
tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 (absolute paths within the
item worktree) — `Run.PassThru = $true`, `Output.Verbosity = "Normal"`, then the explicit
trailing branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

Counts over the whole Run.Path scope: Passed 29, Failed 0, Skipped 0, Total 29. Pester version
5.6.1. Run duration 16.19s.

## The six It cases added or updated across P1-T1 through P1-T6

Six It cases were added or updated in Phase 1: four new (P1-T1, P1-T2, P1-T5, P1-T6) and two
updated in place (P1-T3, P1-T4). All six are recorded individually below with the verdict this
run produced.

| Task | Test file | Describe / It | Result |
|---|---|---|---|
| P1-T1 | tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | Get-CoberturaPackageLineSummary.accumulates line and branch totals across every class in the package | Passed |
| P1-T2 | tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | Get-CoberturaPackageLineSummary.falls back to a zero rate when no class in the package carries any lines | Passed |
| P1-T3 | tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | ConvertTo-KoverageCoberturaXml.computes the merged per-file line-rate from the merged rollup alone | Passed |
| P1-T4 | tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | ConvertTo-KoverageCoberturaXml.preserves the primary class methods subtree and every hits value when merging | Passed |
| P1-T5 | tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | Merge-CoberturaClassesByFilename.unions the methods of every group member into the merged class | Passed |
| P1-T6 | tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | Merge-CoberturaClassesByFilename.takes the higher hits value when the second class seen for a filename is strictly higher | Passed |

Passed among the six: 6. Failed among the six: 0. Skipped among the six: 0.

## Reconciliation with the P0-T7 baseline

The P0-T7 baseline recorded 25 passing tests in
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 and no
tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 at all. This run reports 29
over the same two-file scope: 25 baseline plus the two new Helpers It cases (P1-T5, P1-T6) plus
the two new PackageRate It cases (P1-T1, P1-T2). P1-T3 and P1-T4 changed existing cases in
place and therefore add no count. 25 + 2 + 2 = 29, reconciling exactly. No previously passing
test regressed.

## Output Summary

All six It cases added or updated across P1-T1 through P1-T6 pass. The five expect-fail cases
recorded by P1-T7 (P1-T1 through P1-T5) are now green; P1-T6, which was already green, remains
green. Whole-scope counts are Passed 29, Failed 0, Skipped 0, with direct-run EXIT_CODE 0, and
the MCP payload returned to ok: true. Absolute host paths in the captured Pester output were
replaced with their repository-relative equivalents.
