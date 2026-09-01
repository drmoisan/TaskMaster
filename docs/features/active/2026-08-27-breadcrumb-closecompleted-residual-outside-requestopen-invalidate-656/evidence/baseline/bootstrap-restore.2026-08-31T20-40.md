# Baseline — NuGet Restore Bootstrap (Issue #656)

Timestamp: 2026-09-01T14-36
Task: [P0-T4]

Command:
```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1
pwsh -NoProfile -Command "(Get-ChildItem -Directory packages).Count"
```

EXIT_CODE: 0

Results:

- Pre-state: no `packages/` directory existed in this worktree.
- The restore resolved MSBuild through vswhere and reported `Installed: 172 package(s) to
  packages.config projects`, then `Build succeeded. 0 Warning(s) 0 Error(s)`.
- Recorded directory count: `(Get-ChildItem -Directory packages).Count` = **172**, which is greater
  than 0 as the acceptance requires.

Rationale for this step: every first-party project declares `EnsureNuGetPackageBuildImports` whose
`<Error>` fires at `BeforeTargets="PrepareForBuild"`, and `.claude/rules/csharp.md` wires each of the
five analyzers through an explicit `..\packages\...` path. msbuild therefore hard-fails without a
populated `packages/`. `packages/` is git-ignored and does not enter the change set.

Output Summary: Bootstrap succeeded. 172 packages restored, 0 errors, 0 warnings. This is a
bootstrap step, not a toolchain gate.
