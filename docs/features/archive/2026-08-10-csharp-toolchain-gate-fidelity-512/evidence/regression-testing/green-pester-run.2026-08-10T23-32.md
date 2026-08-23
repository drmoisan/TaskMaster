# Green regression run — red-to-green closure ([P2-T7])

Timestamp: 2026-08-10T23-32

Pairs with `FEATURE/evidence/regression-testing/red-pester-run.2026-08-10T23-20.md`.

## Channel 1 — the MCP function (the recorded channel)

Command: `mcp__drm-copilot__run_poshqc_test` with `workspace_root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb"` and `scan_folders = ["tests/scripts/vscode"]`
EXIT_CODE: 0

```json
{
  "ok": true,
  "tool": "run_poshqc_test",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb",
  "summary": "Ran bundled PoshQC test against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb' with 1 selected scan folder(s)."
}
```

`MCP_DETAIL_UNAVAILABLE: run_poshqc_test emits no per-It enumeration`

The per-`It` pass/fail names and the numeric total-test-count assertion are therefore asserted
against the **direct channel** below.

## Channel 2 — direct Pester (the enumerating channel)

Command: the [P0-T16] direct command with `$c.CodeCoverage.Enabled = $false`, delivered via
`pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-pester-nocoverage.ps1`.

EXIT_CODE: 0 (`$LASTEXITCODE` immediately after `Invoke-Pester` = 0)

| Metric | Red ([P1-T3]) | Green (this run) |
|---|---|---|
| `TotalCount` | 41 | **41** |
| `PassedCount` | 39 | **41** |
| `FailedCount` | **2** | **0** |
| `SkippedCount` | 0 | 0 |
| `$LASTEXITCODE` | 2 | **0** |

**Total test count assertion: 41 = baseline 40 ([P0-T16] direct channel) + 1** — satisfied.

## The two previously failing `It` names now pass

| `It` recorded as failing in the red run | Result now |
|---|---|
| `Get-MSBuildBuildArguments.emits /t:Rebuild in the target position when -Target Rebuild is supplied` | **Passed** |
| `Get-RequestedMSBuildProperties.emits no MSBuild property for the deprecated -EnableNullable switch` | **Passed** |

Full per-`It` results for `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`:

```
[Passed] ConvertTo-MSBuildPropertyArgument.adds the /p: prefix for bare property assignments
[Passed] ConvertTo-MSBuildPropertyArgument.preserves an existing /p: prefix
[Passed] Get-MSBuildBuildArguments.returns each additional MSBuild property as a separate argument
[Passed] Get-MSBuildBuildArguments.emits /t:Rebuild in the target position when -Target Rebuild is supplied
[Passed] Get-RequestedMSBuildProperties.maps analyzer switches to the expected MSBuild properties
[Passed] Get-RequestedMSBuildProperties.emits no MSBuild property for the deprecated -EnableNullable switch
```

The deprecation warning fires exactly where expected and does not fail the test:

```
WARNING: The -EnableNullable switch is deprecated and has no effect. This repository enforces
nullability per file via #nullable enable; /p:Nullable=enable is deliberately absent from CI and
makes the gate unpassable. See CLAUDE.md C#1 item 3.
```

## `.csproj` sync guard

| Capture | `git status --porcelain -- '*.csproj'` |
|---|---|
| Immediately before this task | (empty) |
| Immediately after this task | (empty) |

Sync console line emitted: `Sync-PackageReferences: All HintPaths are up to date`. No `.csproj` was
rewritten; no revert was required.

## Output Summary

Green state confirmed. MCP `run_poshqc_test` returned `EXIT_CODE: 0`; the direct Pester 5.6.1 channel
returned `EXIT_CODE: 0` with **41 tests, 41 passed, 0 failed** — the [P0-T16] baseline of 40 plus the
one `It` added by [P1-T1]. Both `It` blocks recorded as failing in the red run now pass, completing
the red-to-green regression proof required by the `full-bug` Bugfix Workflow. No `.csproj` was
rewritten.
