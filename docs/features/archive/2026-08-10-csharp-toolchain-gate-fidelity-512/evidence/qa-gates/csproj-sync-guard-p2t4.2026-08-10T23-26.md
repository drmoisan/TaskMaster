# `.csproj` sync guard for [P2-T4]

Timestamp: 2026-08-10T23-26
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -NoExecute`
EXIT_CODE: 0

## Why this guard exists

`scripts/vscode/Invoke-VSBuild.ps1` unconditionally runs `Sync-PackageReferences.ps1` (call site at
line 144 in the merge-base file, before the `-NoExecute` early return at line 150), and that script
rewrites `.csproj` files via `[System.IO.File]::WriteAllText` at `Sync-PackageReferences.ps1:148`.
Any such rewrite is unrelated HintPath churn, is **not** this feature's change, and must be reverted.

## Captures

| Capture | `git status --porcelain -- '*.csproj'` |
|---|---|
| Immediately **before** [P2-T4] | (empty) |
| Immediately **after** [P2-T4] | (empty) |

## Sync console output

```
Using MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
Sync-PackageReferences: All HintPaths are up to date
```

The line emitted is **`Sync-PackageReferences: All HintPaths are up to date`**, which the script emits
only when it changed nothing (the `$fixCount -eq 0` early return at `Sync-PackageReferences.ps1:112`
guards the `WriteAllText` at :148). Neither the per-project line
`[<project>] Fixed N broken HintPath(s)` nor the summary line
`Sync-PackageReferences: Fixed N HintPath(s) total` was emitted.

## Revert performed

**None required.** No `.csproj` was rewritten, so no `git checkout -- <path>` was executed. No
`.csproj` is left modified, so [P6-T9] is not invalidated by this task.

## [P2-T4] acceptance

- `-Target $Target` was added to the `Get-MSBuildBuildArguments` call site, so the script-level
  `-Target` value reaches the argument builder.
- `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -NoExecute` returns without error
  (`EXIT_CODE: 0`), exercising the full parameter-binding and argument-construction path with the new
  parameter present.

## Output Summary

`Invoke-VSBuild.ps1 -NoExecute` ran cleanly with `EXIT_CODE: 0` after the `-Target $Target` call-site
change. `Sync-PackageReferences.ps1` reported `All HintPaths are up to date` and rewrote nothing;
`git status --porcelain -- '*.csproj'` is empty both before and after the task, so no revert was
needed.
