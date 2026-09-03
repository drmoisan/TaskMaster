# Phase 3 pass-after run (P3-T4)

Timestamp: 2026-09-02T22-42

Task: [P3-T4]

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
summary: Ran bundled PoshQC test against '<item worktree repository root>' with 2 selected scan
         folder(s).
```

`ok: true` restores the P0-T7 baseline signal, which had flipped to `ok: false` during the Phase 1
and Phase 2 expect-fail windows.

## Command 2 — Direct Pester run over tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 (absolute path within the
item worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, then the explicit
trailing branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

Counts: Passed 12, Failed 0, Skipped 0, Total 12. Pester version 5.6.1. Run duration 666ms.

## P3-T3 verdict

```
Describing Remove-CoberturaExemptClosureCoverage
  [+] retains a closure whose bare member name collides with a non-exempt overload 4ms (4ms|0ms)
```

## No-regression check against the P0-T7 baseline

The P0-T7 baseline recorded tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
at Passed 11, Failed 0, Skipped 0. This run records Passed 12, Failed 0, Skipped 0. The delta is
exactly +1 passed, which is the single It added by P3-T3. Every one of the eleven baseline tests
is individually present and passing on this run:

```
  [+] drops closure lines whose declaring member is absent from the instrumented method set
  [+] keeps closure lines whose declaring member is present in the instrumented method set
  [+] keeps closure lines whose declaring member exists only as an async state-machine class
  [+] drops only the exempt method from a mixed closure class and retains an underivable method
  [+] removes a closure class outright when every method resolves to an absent member
  [+] leaves an async state-machine class untouched even when its member has no plain method
  [+] removes covered closure lines from both the numerator and the denominator
  [+] creates a missing rollup and merges a line number shared by two retained methods
  [+] emits a zero rate when every retained method contributes no line
  [+] derives declaring member, declaring type and closure classification purely from names
  [+] is idempotent and silent when applied twice to the same document
```

That list includes "removes a closure class outright when every method resolves to an absent
member", whose Part B pins the local-function non-admission that P3-T1's docstring addendum
describes; it passes unchanged, confirming the addendum made no behavioral change.

## Output Summary

The P3-T3 pinning test passes, and no test in the file regressed relative to the P0-T7 baseline:
11 baseline tests passing before, 12 passing now, zero failed and zero skipped on both runs.
Phase 3's two production edits (P3-T1 and P3-T2) are comment-based-help additions inside
Get-CoberturaInstrumentedMemberName's .DESCRIPTION block only; no assertion, parameter, or
return-value change was made, and the unchanged test outcomes across the whole file confirm it.
Absolute host paths naming the item worktree were replaced with their repository-relative
equivalents in the captured Pester output.
