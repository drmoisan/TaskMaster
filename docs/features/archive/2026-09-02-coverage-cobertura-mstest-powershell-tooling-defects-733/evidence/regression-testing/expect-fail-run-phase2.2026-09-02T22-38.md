# Phase 2 expect-fail run (P2-T2)

Timestamp: 2026-09-02T22-38

Task: [P2-T2]

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
summary: Command exited with code 1.
```

`ok: false` is the expected signal while the P2-T1 expect-fail test is in place and the P2-T3
production clause has not yet landed. The payload carries no counts, so the verdict below is read
from Command 2.

## Command 2 — Direct Pester run over tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 (absolute path within the item
worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, then the explicit trailing
branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

Counts: Passed 26, Failed 1, Skipped 0, Total 27. Pester version 5.6.1. Run duration 1.7s.

## Per-task verdict

### [P2-T1] — FAILED as predicted

Test: `Invoke-MSTestWithCoverageMain` / "excludes assemblies discovered under a .claude worktree
segment", in tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1.

Predicted failure: both paths are present in the captured `-TestAssembly` array, because no
`.claude` exclusion clause exists yet.

Observed:

```
  [-] excludes assemblies discovered under a .claude worktree segment 78ms (77ms|1ms)
   at Should -Be @('C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll'),
      tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:440
   Expected 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll', but got
   @('C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll',
     'C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').
```

Match: exact — the captured array holds both the ordinary path and the `.claude` worktree path.

## Output Summary

The single P2-T1 expect-fail test failed with exactly the predicted failure: the captured
`-TestAssembly` array contains both the ordinary path and the `.claude` worktree path. All 26
pre-existing tests in the file continue to pass, confirming the new fixture does not disturb the
shared BeforeEach mocks for any sibling test. Failed 1, Passed 26, Skipped 0 over 27 tests;
direct-run EXIT_CODE 1, which is the expected value at this point in the phase. Absolute host
paths naming the item worktree were replaced with their repository-relative equivalents in the
captured Pester output; the `C:\repo\...` strings are the test's own synthetic fixture values.
