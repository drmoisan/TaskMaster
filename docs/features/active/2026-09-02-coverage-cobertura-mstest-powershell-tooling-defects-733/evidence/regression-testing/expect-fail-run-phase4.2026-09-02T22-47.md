# Phase 4 expect-fail run (P4-T3)

Timestamp: 2026-09-02T22-47

Task: [P4-T3]

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
summary: Command exited with code 3.
```

`ok: false` with an underlying code of 3 is the expected signal while the three P4-T2 expect-fail
tests are in place and the P4-T4 production function has not yet landed. The payload carries no
counts, so the individual verdicts below are read from Command 2.

## Command 2 — Direct Pester run over tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 (absolute path within the item
worktree; the file chosen by P4-T1), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, then
the explicit trailing branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

Counts: Passed 0, Failed 3, Skipped 0, Total 3. Pester version 5.6.1. Run duration 755ms.

## Per-case verdicts — all three FAILED with CommandNotFoundException, as predicted

### Case (a) zero matches — FAILED as predicted

```
  [-] returns an empty array when discovery matches nothing 163ms (144ms|19ms)
   Expected no exception to be thrown, but an exception "The term 'Get-MSTestAssemblyPathList' is
   not recognized as a name of a cmdlet, function, script file, or executable program." was thrown
   from tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1:23
```

### Case (b) exactly one match — FAILED as predicted

```
  [-] returns a single-element array when discovery matches exactly one assembly 24ms (22ms|1ms)
   Expected no exception to be thrown, but an exception "The term 'Get-MSTestAssemblyPathList' is
   not recognized as a name of a cmdlet, function, script file, or executable program." was thrown
   from tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1:34
```

### Case (c) multiple matches — FAILED as predicted

```
  [-] returns every match when discovery matches multiple assemblies 19ms (19ms|1ms)
   CommandNotFoundException: The term 'Get-MSTestAssemblyPathList' is not recognized as a name of
   a cmdlet, function, script file, or executable program.
   at <ScriptBlock>, tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1:49
```

## Output Summary

All three P4-T2 cases fail with CommandNotFoundException on Get-MSTestAssemblyPathList, which is
exactly the predicted pre-fix failure: the function does not exist yet. Cases (a) and (b) report
the exception through their `Should -Not -Throw` wrapper and case (c) reports it directly, which
are the two available shapes of the same underlying CommandNotFoundException. Failed 3, Passed 0,
Skipped 0 over 3 tests; direct-run EXIT_CODE 1, which is the expected value at this point in the
phase. Absolute host paths naming the item worktree were replaced with their repository-relative
equivalents in the captured Pester output.
