# Baseline — NuGet Restore

Timestamp: 2026-08-22T09-18

Command:

```
# 1. Pre-state confirmation (run from the worktree root)
ls -d packages

# 2. Restore
pwsh -NoProfile -Command 'nuget restore TaskMaster.sln'

# 3. Post-state confirmation (run from the worktree root)
ls -d packages
ls packages | wc -l
git status --porcelain
```

EXIT_CODE: 0

Output Summary:

## Pre-state (confirmed first, as the task requires)

`ls -d packages` → `ls: cannot access 'packages': No such file or directory`. The directory did
**not** exist at the worktree root before the restore. This is the condition the task predicts for a
fresh agent worktree.

## Installed-package count

**172 packages installed to `packages.config` projects.** NuGet's own closing line:

```
Installed:
    172 package(s) to packages.config projects
```

The resulting `packages` directory holds **172** top-level package folders, matching the reported
install count.

Feeds used, as reported by NuGet:

```
    C:\Users\DanMoisan\.nuget\packages\
    https://api.nuget.org/v3/index.json
    C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\
```

## Post-state — acceptance conditions

1. **`EXIT_CODE: 0`** — the restore completed with no error and reported 172 installs.
2. **The `packages` directory exists at the worktree root** — `ls -d packages` → `packages/`.
3. **`git status --porcelain` reports zero entries whose path begins with the restored packages
   directory name.** The full porcelain output is the two lines produced by this Phase 0 execution
   itself:

   ```
    M docs/features/active/winformspumphost-suite-determinism-511/plan.2026-08-21T18-10.md
   ?? docs/features/active/winformspumphost-suite-determinism-511/evidence/
   ```

   `.gitignore:191` carries the pattern `**/[Pp]ackages/*`, which ignores the tree's contents. The
   surrounding block reads:

   ```
   # NuGet Packages
   *.nupkg
   # The packages folder can be ignored because of Package Restore
   **/[Pp]ackages/*
   # except build/, which is used as an MSBuild target.
   !**/[Pp]ackages/build/
   ```

   Restoring therefore does not dirty the tree and does not endanger the clean-tree acceptance in
   P6-T18.

## Why this task is load-bearing

Every project declares an `EnsureNuGetPackageBuildImports` target whose `Error` fires at
`BeforeTargets="PrepareForBuild"` when the `packages` tree is missing
(`QuickFiler.Test/QuickFiler.Test.csproj:452-460` is the representative instance). Without the
restore, every msbuild task in this plan hard-fails before compilation and every `Reference` hint path
under the `packages` tree is unresolvable. This step mirrors the CI step at
`.github/workflows/_build-analyzers.yml:45`.
