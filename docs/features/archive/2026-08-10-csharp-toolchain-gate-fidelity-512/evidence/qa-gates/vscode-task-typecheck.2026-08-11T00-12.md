# Corrected `type-check:` VS Code task surface ([P5-T9])

Timestamp: 2026-08-11T00-12
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -Target Rebuild -TreatWarningsAsErrors 2>&1 | Tee-Object -FilePath coverage/task-typecheck.log`
EXIT_CODE: 0

Issued from a PowerShell parent via
`pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-task-typecheck.ps1`. The transcript
capture is the explicit `2>&1 | Tee-Object` redirection, per [P5-T8].

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| `Skipping target "CoreCompile"` count in the transcript | **0** | required 0 — PASS |
| Elapsed | **18.0 s** | required >= 10 s — PASS |
| MSBuild summary | `0 Error(s)` | — |

## Transcript-channel capability (cited, not re-asserted)

The `2>&1 | Tee-Object` transcript channel's capability to surface
`Skipping target "CoreCompile"` was established by the control run in
**`FEATURE/evidence/qa-gates/vscode-task-lint.2026-08-11T00-06.md`**, which recorded **18**
occurrences in 3.8 s. That artifact records the control run found a **non-zero** skip count, so
[P5-T8] did **not** fall back to a direct `MSBUILD` `/fl` invocation, and this task therefore applies
**no substitution**: the zero-skip condition above is asserted directly against the transcript.

## Contrast with the warm-vacuous baseline

| Run | Command shape | EXIT | Elapsed | Skip count |
|---|---|---|---|---|
| [P0-T11] | DOC-TYPECHECK warm (`/t:Build` + `/p:Nullable=enable`) | 0 | **1.8 s** | **18** |
| [P5-T9] (this run) | `type-check:` task after [P2-T6] (`-Target Rebuild`, no `-EnableNullable`) | 0 | **18.0 s** | **0** |

## Argument-list parity with `.vscode/tasks.json`

The `type-check: TaskMaster.sln (nullable warnings as errors)` task args after [P2-T6]:

```
"-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
"scripts/vscode/Invoke-VSBuild.ps1",
"-SolutionPath", "TaskMaster.sln",
"-Configuration", "Debug",
"-Platform", "Any CPU",
"-Target", "Rebuild",
"-TreatWarningsAsErrors"
```

This run's argument list is **identical**. The task label is unchanged, so external references by
label continue to resolve.

## `.csproj` sync guard

| Capture | `git status --porcelain -- '*.csproj'` |
|---|---|
| Immediately before the run | (empty) |
| Immediately after the run | (empty) |

Sync console line emitted: `Sync-PackageReferences: All HintPaths are up to date` — it changed
nothing. **No `.csproj` was rewritten and no revert was required**; [P6-T9] is not invalidated.

## Output Summary

The corrected `type-check:` task surface returns `EXIT_CODE: 0` with **zero**
`Skipping target "CoreCompile"` occurrences in **18.0 s**, against the merge-base behaviour of exit 0
in 1.8 s with 18 skips. The repo-defined preferred execution path now performs a genuine compile and
no longer passes `Nullable=enable`. No `.csproj` was rewritten.
