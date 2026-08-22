# Baseline — Worktree-Local .NET SDK Bootstrap

Timestamp: 2026-08-22T09-16

Command:

```
# 1. Pre-state confirmation (run from the worktree root)
ls -d .dotnet-sdk
cat global.json
dotnet --version

# 2. Provisioning (mirror method, authorized by the task text)
pwsh -NoProfile -Command "robocopy 'C:\Users\DanMoisan\repos\TaskMaster\.dotnet-sdk' 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad37a256a0fb60243\.dotnet-sdk' /E /MT:16 /NFL /NDL /NJH /NP"

# 3. Post-state confirmation (run from the worktree root)
dotnet --version
dotnet --list-sdks
git status --porcelain
```

EXIT_CODE: 0

The `robocopy` invocation itself reported exit code `1`, which is a **success** code in robocopy's
exit-code scheme ("one or more files were copied successfully"). Robocopy reserves exit codes `>= 8`
for failures. Copy statistics: 880 of 880 directories copied, 5,266 of 5,266 files copied, 733.22 MB
transferred, 0 mismatched, 0 FAILED, 0 extras. The provisioning step therefore succeeded and the
recorded `EXIT_CODE: 0` reflects the task outcome.

Output Summary:

## Pre-state (confirmed first, as the task requires)

- `ls -d .dotnet-sdk` → `ls: cannot access '.dotnet-sdk': No such file or directory`. The directory
  did **not** exist in this worktree.
- `global.json` pins `sdk.version` to `8.0.205` with `"rollForward": "latestFeature"`,
  `"allowPrerelease": false`, and a `paths` list of `[".dotnet-sdk", "$host$"]` — `.dotnet-sdk` ahead
  of the host fallback.
- `dotnet --version` run from the worktree root printed the `global.json` `errorMessage` instead of a
  version:

  ```
  The command could not be loaded, possibly because:
    * You intended to execute a .NET application:
        The application '--version' does not exist or is not a managed .dll or .exe.
    * You intended to execute a .NET SDK command:
        The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
  ```

  Every `dotnet` and CSharpier task in this plan was therefore unrunnable before this task.

## Provisioning method used

**Mirror**, not the install script. The task authorizes either
`pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1` or mirroring the already-populated
`.dotnet-sdk` tree from the main checkout at `C:\Users\DanMoisan\repos\TaskMaster`. The mirror was
chosen because the source tree was already present and complete (747 MB, containing
`.dotnet-sdk\sdk\8.0.205\`), so the mirror avoids a network download and produces a byte-identical
tree. `robocopy /E /MT:16` was used to perform the recursive copy.

## Post-state

- `dotnet --version` run from the worktree root → **`8.0.205`**. Acceptance condition met.
- `dotnet --list-sdks` run from the worktree root:

  ```
  8.0.205 [C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad37a256a0fb60243\.dotnet-sdk\sdk]
  10.0.302 [C:\Program Files\dotnet\sdk]
  ```

  An entry whose path ends `.dotnet-sdk\sdk` is present, and it is the worktree-local one. Acceptance
  condition met. The host fallback offers only `10.0.302`, which cannot satisfy the `8.0.205` pin
  under `latestFeature`, confirming the pre-state diagnosis.
- `git status --porcelain` reports **no entry** for `.dotnet-sdk`. `.gitignore:350` carries the
  pattern `.dotnet*/`, which ignores the tree. The full porcelain output is the two lines produced by
  this Phase 0 execution itself:

  ```
   M docs/features/active/winformspumphost-suite-determinism-511/plan.2026-08-21T18-10.md
  ?? docs/features/active/winformspumphost-suite-determinism-511/evidence/
  ```

  Acceptance condition met: provisioning the SDK did not dirty the tree.

## CI scope note

CI is unaffected by this condition. The `windows-latest` image preinstalls an 8.0.x SDK that
satisfies the `$host$` fallback under `rollForward: latestFeature`. This was a worktree-provisioning
gap only, not a repository defect.
