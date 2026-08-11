# Corrected `lint:` VS Code task surface, with transcript-channel control ([P5-T8])

Timestamp: 2026-08-11T00-06
Command: two invocations of `scripts/vscode/Invoke-VSBuild.ps1`, quoted verbatim below, each captured with an explicit `2>&1 | Tee-Object` redirection
EXIT_CODE: 0 (both runs)

Both runs were issued from a PowerShell parent via
`pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-task-lint.ps1`, per the plan's command
conventions. `Start-Transcript` is not usable with `-File` in the same invocation, so the explicit
`2>&1 | Tee-Object` redirection is the **defined input** for every transcript assertion below.
MSBuild is invoked by `Invoke-VSBuild.ps1` without a `/v:` switch, so console verbosity is `normal`
and the `Skipping target "CoreCompile"` message is emissible.

## Run 1 — control (establishes the transcript channel's discriminating power)

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild 2>&1 | Tee-Object -FilePath coverage/task-lint-control.log
```

**No `-Target`**, run first against the warm tree left by [P5-T7].

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | 0 | — |
| Elapsed | **3.8 s** | required < 5 s — PASS |
| `Skipping target "CoreCompile"` count | **18** | required **> 0** — PASS |

**The transcript channel is discriminating.** It does surface the skip message, so the corrected
run's zero count below is meaningful rather than an artifact of a channel that cannot emit the
message. The fallback branch (substituting a direct `MSBUILD` `/fl` invocation) is therefore **not**
taken, and [P5-T9] cites this artifact rather than re-establishing the capability.

This run also independently reproduces Defect C through the **repo-defined task surface**: the
`lint:` task as it stood at the merge base returned exit 0 in 3.8 s having run no analyzers on any
project.

## Run 2 — corrected (the `lint:` task surface after [P2-T5])

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild 2>&1 | Tee-Object -FilePath coverage/task-lint.log
```

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| `Skipping target "CoreCompile"` count | **0** | required 0 — PASS |
| Elapsed | **18.4 s** | required >= 10 s — PASS (contrast: [P0-T10] warm-vacuous 2.8 s) |

## Argument-list parity with `.vscode/tasks.json`

The `lint: TaskMaster.sln (.NET analyzers)` task args after [P2-T5]:

```
"-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
"scripts/vscode/Invoke-VSBuild.ps1",
"-SolutionPath", "TaskMaster.sln",
"-Configuration", "Debug",
"-Platform", "Any CPU",
"-Target", "Rebuild",
"-EnableNETAnalyzers", "-EnforceCodeStyleInBuild"
```

The corrected run's argument list is **identical**.

## `.csproj` sync guard

`Invoke-VSBuild.ps1` unconditionally runs `Sync-PackageReferences.ps1` (call site at line 144 of the
merge-base file, before the `-NoExecute` early return at line 150), which rewrites `.csproj` files
via `[System.IO.File]::WriteAllText` at `Sync-PackageReferences.ps1:148`.

| Capture | `git status --porcelain -- '*.csproj'` |
|---|---|
| Before the control run | (empty) |
| After the control run | (empty) |
| After the corrected run | (empty) |

Sync console line emitted by **both** runs:
`Sync-PackageReferences: All HintPaths are up to date` — i.e. it changed nothing. Neither
`[<project>] Fixed N broken HintPath(s)` nor `Sync-PackageReferences: Fixed N HintPath(s) total` was
emitted. **No `.csproj` was rewritten and no revert was required**, so [P6-T9] is not invalidated by
this task.

## Output Summary

The control run recorded **18** `Skipping target "CoreCompile"` occurrences in 3.8 s, proving the
`2>&1 | Tee-Object` transcript channel can surface the message. The corrected `lint:` task surface
then returned `EXIT_CODE: 0` with **zero** skips in **18.4 s**, using an argument list identical to
the post-[P2-T5] task definition. The repo-defined preferred execution path now performs a genuine
analyzer-bearing compile. No `.csproj` was rewritten by either run.
