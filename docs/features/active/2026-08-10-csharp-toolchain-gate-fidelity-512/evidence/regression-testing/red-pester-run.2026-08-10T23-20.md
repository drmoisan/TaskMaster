# Red regression run — [expect-fail] ([P1-T3])

Timestamp: 2026-08-10T23-20

This artifact is the **failing-test-before-the-fix** evidence the `full-bug` Bugfix Workflow
requires. A non-zero result here is the **expected** outcome for this task only.

## Channel 1 — the MCP function (the recorded channel)

Command: `mcp__drm-copilot__run_poshqc_test` with `workspace_root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb"` and `scan_folders = ["tests/scripts/vscode"]`
EXIT_CODE: 2

Return payload, verbatim:

```json
{
  "ok": false,
  "tool": "run_poshqc_test",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb",
  "summary": "Command exited with code 2."
}
```

`EXIT_CODE: 2` is recorded as returned and is **non-zero**, as this `[expect-fail]` task expects.

`MCP_DETAIL_UNAVAILABLE: run_poshqc_test emits no per-It enumeration`

The payload carries no per-`It` names and no failure messages. The "exactly two failing `It` names"
and the verbatim failure messages below are therefore asserted against the **direct channel**, which
is the only channel emitting per-`It` detail.

## Channel 2 — direct Pester (the enumerating channel)

Command: the [P0-T16] direct command with `$c.CodeCoverage.Enabled = $false`, delivered via
`pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-pester-nocoverage.ps1` (the same recorded
quoting deviation as [P0-T16]: a script file is parsed only by PowerShell, so `$c` cannot be
interpolated by any parent shell).

EXIT_CODE: 2

### Recorded exit-code semantics for the direct channel

| Setting | Observed value |
|---|---|
| `$c.Run.Exit` | `False` (Pester 5.6.1 default; **not** overridden) |
| `$c.Run.Throw` | `False` (Pester 5.6.1 default; **not** overridden) |
| `$LASTEXITCODE` immediately after `Invoke-Pester` | **2** |

The plan notes that a `0` from the direct channel would be expected and would **not** contradict this
task's `[expect-fail]` status, because `Run.Exit` and `Run.Throw` both default to `False`. The
observed value here is **2**, not `0`; `$c.Run.Exit` was **not** set to `$true`. Either value is
consistent with the plan, which states that the discriminating red-state proof is the enumerated
failing `It` names and their verbatim failure messages, not any process exit code. Both are recorded
below.

### Counts

| Metric | Value |
|---|---|
| `TotalCount` | 41 (baseline 40 + 1 new `It`) |
| `PassedCount` | 39 |
| **`FailedCount`** | **2** |
| `SkippedCount` | 0 |

### The two failing `It` names, with verbatim failure messages

**Exactly two** `It` blocks fail, and they are exactly the tests added by [P1-T1] and modified by
[P1-T2]:

1. `Get-MSBuildBuildArguments.emits /t:Rebuild in the target position when -Target Rebuild is supplied`
   (added by [P1-T1])

   ```
   ParameterBindingException: A parameter cannot be found that matches parameter name 'Target'.
      at <ScriptBlock>, ...\tests\scripts\vscode\Invoke-VSBuild.Tests.ps1:46
   ```

   This is the expected **parameter-binding failure for `-Target`**: the parameter does not yet
   exist on `Get-MSBuildBuildArguments`. [P2-T2] adds it.

2. `Get-RequestedMSBuildProperties.emits no MSBuild property for the deprecated -EnableNullable switch`
   (modified by [P1-T2])

   ```
   Expected 'TreatWarningsAsErrors=true', but got @('Nullable=enable', 'TreatWarningsAsErrors=true').
      at $properties | Should -Be @(, ...\tests\scripts\vscode\Invoke-VSBuild.Tests.ps1:81
   ```

   This is the expected **`Nullable=enable` array mismatch**: the current implementation still emits
   the property. [P2-T3] removes it.

### Per-`It` results for `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`

| Result | `It` |
|---|---|
| Passed | `ConvertTo-MSBuildPropertyArgument.adds the /p: prefix for bare property assignments` |
| Passed | `ConvertTo-MSBuildPropertyArgument.preserves an existing /p: prefix` |
| **Passed** | `Get-MSBuildBuildArguments.returns each additional MSBuild property as a separate argument` (the **default-target** `It`, left byte-identical by [P1-T1]) |
| **Failed** | `Get-MSBuildBuildArguments.emits /t:Rebuild in the target position when -Target Rebuild is supplied` |
| Passed | `Get-RequestedMSBuildProperties.maps analyzer switches to the expected MSBuild properties` |
| **Failed** | `Get-RequestedMSBuildProperties.emits no MSBuild property for the deprecated -EnableNullable switch` |

The default-target `It` passes and both `ConvertTo-MSBuildPropertyArgument` tests pass, exactly as
the acceptance condition requires.

## `.csproj` sync guard

`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` dot-sources `scripts/vscode/Invoke-VSBuild.ps1` with
`-NoExecute` (line 6), which reaches the unconditional `Sync-PackageReferences.ps1` call at line 144
before the `-NoExecute` early return at line 150.

| Capture | `git status --porcelain -- '*.csproj'` |
|---|---|
| Immediately before this task | (empty) |
| Immediately after this task | (empty) |

Sync console line emitted: `Sync-PackageReferences: All HintPaths are up to date` — it changed
nothing. **No `.csproj` was rewritten and no revert was required.**

## Output Summary

Red state confirmed. MCP `run_poshqc_test` returned `EXIT_CODE: 2` with no per-`It` detail; the
direct Pester 5.6.1 channel returned `EXIT_CODE: 2` with **41 tests, 39 passed, exactly 2 failed**.
The two failures are precisely the `It` added by [P1-T1] (parameter-binding failure for `-Target`)
and the `It` modified by [P1-T2] (`Nullable=enable` array mismatch), with both messages quoted
verbatim. The default-target `It` and both `ConvertTo-MSBuildPropertyArgument` tests pass. No
`.csproj` was rewritten.
